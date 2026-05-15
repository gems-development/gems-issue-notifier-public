using Gems.TechSupport.Application.Abstractions.Aggregation;
using Gems.TechSupport.Domain.Primitives;
using Gems.TechSupport.Infrastructure.Metrics;
using Gems.TechSupport.Persistence;
using Gems.TechSupport.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Quartz;

namespace Gems.TechSupport.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
internal sealed class ProcessIssueCommentAggregatorOutboxMessagesJob(
    ApplicationDbContext dbContext,
    IProcessedObserver observer,
    IIssueCommentAggregatePlanBuilder commentAggregate,
    ICommentComposer composerService,
    IOptionsMonitor<ProcessOutboxMessagesOptions> options,
    ILogger<ProcessIssueCommentAggregatorOutboxMessagesJob> logger,
    ProcessedDomainEventsMetrics metrics)
    : IJob
{
    static readonly JsonSerializerSettings serializeSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All,
    };
    public async Task Execute(IJobExecutionContext context)
    {
        var outboxOptions = options.CurrentValue;
        var issueIds = await GetBatchIssueFromMessage(dbContext, context);

        var trigger = context.Trigger as ISimpleTrigger;
        var currentIntervalSeconds = (int)trigger.RepeatInterval.TotalSeconds;

        if (!issueIds.Any())
        {
            logger.LogDebug("No messages in the IssueCommentAggregatorOutBox.");

            if (trigger != null)
            {

                if (currentIntervalSeconds != outboxOptions.ProcessIntervalInSecondsForCommentAggregator)
                    await QuartzTriggerUpdater.UpdateIntervalSecondsAsync(context, outboxOptions.ProcessIntervalInSecondsForCommentAggregator);
            }

            return;
        }

        var messages = await GetBatchMessagesForIssue(dbContext, issueIds, context.CancellationToken);
        var groupMessage = messages.GroupBy(x => x.IssueId);
        var now = DateTime.UtcNow;

        foreach (var message in groupMessage)
        {
            try
            {
                var messagesForIssue = message.ToList();

                if (!messagesForIssue.Any())
                {
                    continue;
                }

                var greetings = await observer.ObserveMessageAsync(message.Key, outboxOptions.IntervalInHoursForGreetings, context.CancellationToken);
                var events = GetEvents(messagesForIssue);

                try
                {
                    var aggregatePlan = commentAggregate.Build(message.Key, events) with
                    {
                        HasGreetings = greetings,
                    };

                    var result = await ExecuteWithRetryPolicyAsync(
                        aggregatePlan,
                        message,
                        outboxOptions,
                        context.CancellationToken);

                    var finalException = result.FinalException;
                    HandleProcessingResult(
                        finalException,
                        message,
                        logger,
                        aggregatePlan,
                        metrics,
                        now);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to aggregate/send {Count} events for IssueId={IssueId}", events.Count, message.Key);
                    foreach (var m in message)
                    {
                        m.Error = ex.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to Deserialize events for issueId={IssueId}", message.Key);
            }

        }

        await dbContext.SaveChangesAsync(context.CancellationToken);

        if (trigger == null)
            return;

        if (currentIntervalSeconds != outboxOptions.ProcessIntervalInSecondsForCommentAggregator)
            await QuartzTriggerUpdater.UpdateIntervalSecondsAsync(context, outboxOptions.ProcessIntervalInSecondsForCommentAggregator);
    }
    private async static Task<List<long>> GetBatchIssueFromMessage(
    ApplicationDbContext dbContext,
    IJobExecutionContext context)
    {
        return await dbContext
                            .Set<IssueCommentAggregatorOutBoxMessage>()
                            .Where(x => x.ProcessedOnUtc == null)
                            .OrderBy(x => x.Id)
                            .Select(x => x.IssueId)
                            .Distinct()
                            .ToListAsync(context.CancellationToken);
    }

    private static async Task<List<IssueCommentAggregatorOutBoxMessage>> GetBatchMessagesForIssue(
        ApplicationDbContext dbContext,
        List<long> issueId,
        CancellationToken cancellationToken)
    {
        return await dbContext
                            .Set<IssueCommentAggregatorOutBoxMessage>()
                            .Where(x => x.ProcessedOnUtc == null && issueId.Contains(x.IssueId))
                            .OrderBy(x => x.Id)
                            .ToListAsync(cancellationToken);
    }

    private static IReadOnlyCollection<IDomainEvent> GetEvents(
        List<IssueCommentAggregatorOutBoxMessage> issue)
    {
        IReadOnlyCollection<IDomainEvent> events = issue
            .OrderBy(x => x.OccuredOnUtc)
            .ThenBy(x => x.Id)
            .Select(x =>
            {
                IDomainEvent? domainEvent = JsonConvert
                    .DeserializeObject<IDomainEvent>(
                    x.Content,
                    serializeSettings
                    );
                return domainEvent;
            })
            .Where(x => x is not null)
            .Cast<IDomainEvent>()
            .ToList();
        return events;
    }
    private Task<PolicyResult> ExecuteWithRetryPolicyAsync(
        CommentAggregatePlan aggregatePlan,
        IGrouping<long, IssueCommentAggregatorOutBoxMessage> message,
        ProcessOutboxMessagesOptions outboxOptions,
        CancellationToken cancellationToken)
    {
        AsyncRetryPolicy policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                outboxOptions.RetryCount,
                retryAttempt => TimeSpan.FromSeconds(3 * retryAttempt),
                (exception, delay, attempt, context) =>
                {
                    logger.LogWarning(
                "Retry attempt={Attempt} after delay={Delay}s for message {MessageType}:{MessageId}",
                        attempt, delay.TotalSeconds, message.GetType(), message.GetHashCode()
                    );
                }
            );
        return policy.ExecuteAndCaptureAsync(async () =>
        {
            await composerService.Compose(aggregatePlan, cancellationToken);
        });
    }
    private static void RegisterMetrics(
        CommentAggregatePlan commentAggregatePlan,
        ProcessedDomainEventsMetrics metrics)
    {
        foreach (var ev in commentAggregatePlan.DomainEvents)
        {
            metrics.RecordDomainEventProcessedSuccessfully(ev.GetType().Name);
        }
    }
    private static void HandleProcessingResult(
        Exception finalException,
        IGrouping<long, IssueCommentAggregatorOutBoxMessage> group,
        ILogger logger,
        CommentAggregatePlan aggregatePlan,
        ProcessedDomainEventsMetrics metrics,
        DateTime now)
    {
        foreach (var m in group)
        {
            m.ProcessedOnUtc = now;
            m.Error = null;
        }
        if (finalException is null)
        {
            RegisterMetrics(aggregatePlan, metrics);
        }
        else
        {
            foreach (var e in group)
            {
                e.Error = finalException?.ToString();
            }
            logger.LogError(
                finalException,
                "An error occurred while processing the message {MessageType}:{MessageId}.",
                group.First().Type, group.First().Id);
        }
    }
}
