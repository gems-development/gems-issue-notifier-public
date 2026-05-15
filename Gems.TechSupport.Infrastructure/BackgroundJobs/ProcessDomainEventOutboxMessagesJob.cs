using Gems.TechSupport.Domain.Primitives;
using Gems.TechSupport.Infrastructure.Metrics;
using Gems.TechSupport.Persistence;
using Gems.TechSupport.Persistence.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Quartz;

namespace Gems.TechSupport.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
internal sealed class ProcessDomainEventOutboxMessagesJob(
    ApplicationDbContext dbContext,
    IPublisher publisher,
    IOptionsMonitor<ProcessOutboxMessagesOptions> options,
    ILogger<ProcessDomainEventOutboxMessagesJob> logger,
    ProcessedDomainEventsMetrics metrics)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var outboxOptions = options.CurrentValue;

        List<DomainEventOutboxMessage> messages = await dbContext
            .Set<DomainEventOutboxMessage>()
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccuredOnUtc)
            .Take(outboxOptions.ProcessMessagesBatchSize)
            .ToListAsync(context.CancellationToken);

        foreach (var message in messages)
        {
            IDomainEvent? domainEvent = JsonConvert
                .DeserializeObject<IDomainEvent>(
                message.Content,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                });

            if (domainEvent is null)
            {
                continue;
            }

            var result = await ExecuteWithRetryPolicyAsync(domainEvent, message, outboxOptions, context.CancellationToken);

            var finalException = result.FinalException;

            message.Error = finalException?.ToString();
            message.ProcessedOnUtc = DateTime.UtcNow;

            if (finalException is null)
            {
                metrics.RecordDomainEventProcessedSuccessfully(message.Type);
            }
            else
            {
                logger.LogError(
                    finalException,
                    "An error occurred while processing the message {MessageType}:{MessageId}.",
                    message.Type, message.Id);
            }
        }
        await dbContext.SaveChangesAsync(context.CancellationToken);

        var trigger = context.Trigger as ISimpleTrigger;

        if (trigger != null)
        {

            var currentIntervalSeconds = (int)trigger.RepeatInterval.TotalSeconds;

            if (currentIntervalSeconds != outboxOptions.ProcessIntervalInSecondsForDomainEvent)
                await QuartzTriggerUpdater.UpdateIntervalSecondsAsync(context, outboxOptions.ProcessIntervalInSecondsForDomainEvent);
        }
    }

    private Task<PolicyResult> ExecuteWithRetryPolicyAsync(
        IDomainEvent domainEvent,
        DomainEventOutboxMessage message,
        ProcessOutboxMessagesOptions outboxOptions,
        CancellationToken cancellationToken)
    {
        AsyncRetryPolicy policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                outboxOptions.RetryCount,
                attempt => TimeSpan.FromSeconds(3 * attempt),
                (exception, delay, attempt, context) =>
                {
                    logger.LogWarning(
                        "Retry attempt={Attempt} after delay={Delay}s for message {MessageType}:{MessageId}",
                        attempt, delay, message.Type, message.Id);
                }
            );

        return policy.ExecuteAndCaptureAsync(() => publisher.Publish(domainEvent, cancellationToken));
    }
}
