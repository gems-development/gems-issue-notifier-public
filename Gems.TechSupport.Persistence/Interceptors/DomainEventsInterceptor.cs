using Gems.TechSupport.Domain.Events;
using Gems.TechSupport.Domain.Primitives;
using Gems.TechSupport.Persistence.Outbox;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.FeatureManagement;
using Newtonsoft.Json;

namespace Gems.TechSupport.Persistence.Interceptors;

internal sealed class DomainEventsInterceptor(IFeatureManager featureManager) : SaveChangesInterceptor
{
    static readonly JsonSerializerSettings serializeSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All,
    };
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;

        if (dbContext is null)
        {
            await base.SavingChangesAsync(eventData, result, cancellationToken);
            return result;
        }
        else
        {
            var domainEvents = dbContext.ChangeTracker
                .Entries<AggregateRoot>()
                .Select(x => x.Entity)
                .SelectMany(aggregateRoot =>
                {
                    var domainEvents = aggregateRoot.DomainEvents;

                    return domainEvents;
                });

            var enabledDomainEvents = await SelectEnabledDomainEvents(domainEvents);

            FilterDomainEvents(
                enabledDomainEvents,
                out var issueCommentEvents,
                out var regularEvents
            );

            var outboxMessagesForIssueCommentAggregator = CreateOutboxMessagesForIssueCommentAggregator(issueCommentEvents);
            var outboxMessagesForDomainEvents = CreateOutboxMessagesForDomainEvents(regularEvents);

            foreach (var aggregateRoot in dbContext.ChangeTracker.Entries<AggregateRoot>())
            {
                aggregateRoot.Entity.ClearDomainEvents();
            }

            dbContext.Set<IssueCommentAggregatorOutBoxMessage>().AddRange(outboxMessagesForIssueCommentAggregator);
            dbContext.Set<DomainEventOutboxMessage>().AddRange(outboxMessagesForDomainEvents);

            await base.SavingChangesAsync(eventData, result, cancellationToken);
            return result;
        }
    }
    private static void FilterDomainEvents(
            IEnumerable<IDomainEvent> enabledDomainEvents,
            out List<IDomainEvent> issueCommentEvents,
            out List<IDomainEvent> regularEvents)
    {
        issueCommentEvents = new List<IDomainEvent>();
        regularEvents = new List<IDomainEvent>();
        foreach (var domainEvent in enabledDomainEvents)
        {
            if (IsOkdeskCommentEvent(domainEvent))
            {
                issueCommentEvents.Add(domainEvent);
            }
            else
            {
                regularEvents.Add(domainEvent);
            }
        }

    }
    private static List<DomainEventOutboxMessage> CreateOutboxMessagesForDomainEvents(List<IDomainEvent> regularEvents)
    {
        var outboxMessagesDomainEvents = regularEvents
                .Select(domainEvent => new DomainEventOutboxMessage
                {
                    Id = Guid.NewGuid(),
                    OccuredOnUtc = DateTime.UtcNow,
                    Type = domainEvent.GetType().Name,
                    Content = JsonConvert.SerializeObject(
                        domainEvent,
                        serializeSettings
                        )
                })
                .ToList();
        return outboxMessagesDomainEvents;
    }
    private static List<IssueCommentAggregatorOutBoxMessage> CreateOutboxMessagesForIssueCommentAggregator(List<IDomainEvent> issueCommentEvents)
    {
        var outboxMessagesIssueCommentAggregator = issueCommentEvents
           .Select(domainEvent => new IssueCommentAggregatorOutBoxMessage
           {
               IssueId = GetIssueIdProperty(domainEvent),
               Id = Guid.NewGuid(),
               OccuredOnUtc = DateTime.UtcNow,
               Type = domainEvent.GetType().Name,
               Content = JsonConvert.SerializeObject(
                   domainEvent,
                   serializeSettings
                   )
           })
           .ToList();
        return outboxMessagesIssueCommentAggregator;
    }
    private static bool IsOkdeskCommentEvent(IDomainEvent e) => e switch
    {
        IssueDeadlineNotificationEvent => true,
        IssueStatusUpdatedEvent => true,
        IssueDeadlineUpdatedEvent => true,
        IssuePriorityUpdatedEvent priorityUpdatedEvent when priorityUpdatedEvent.UpdateAuthorType == "employee" => true,
        IssueCompletedEvent => true,
        IssueProblemPostCommentEvent => true,
        _ => false
    };
    private static long GetIssueIdProperty(IDomainEvent e) => e switch
    {
        IssueDeadlineNotificationEvent x => x.IssueId,
        IssueStatusUpdatedEvent x => x.IssueId,
        IssueDeadlineUpdatedEvent x => x.IssueId,
        IssuePriorityUpdatedEvent x => x.IssueId,
        IssueCompletedEvent x => x.IssueId,
        IssueProblemPostCommentEvent x => x.IssueId,
        IssueAutoCompletedEvent x => x.IssueId,
        _ => 0
    };
    private async Task<IEnumerable<IDomainEvent>> SelectEnabledDomainEvents(IEnumerable<IDomainEvent> domainEvents)
    {
        var enabledDomainEvents = new List<IDomainEvent>();

        foreach (var domainEvent in domainEvents)
        {
            if (await IsFeatureEnabledAsync(domainEvent))
            {
                enabledDomainEvents.Add(domainEvent);
            }
        }

        return enabledDomainEvents;
    }

    private Task<bool> IsFeatureEnabledAsync(IDomainEvent domainEvent)
    {
        return featureManager.IsEnabledAsync(domainEvent.GetType().Name + "Enabled");
    }
}
