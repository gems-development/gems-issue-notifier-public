using Gems.TechSupport.Application.Abstractions.Aggregation;
using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Events;
using Gems.TechSupport.Domain.Primitives;

namespace Gems.TechSupport.Infrastructure.Services.Aggregation;

internal sealed class IssueCommentAggregatePlanBuilder(IDisplayNameService displayNameService) : IIssueCommentAggregatePlanBuilder
{
    private static Dictionary<string, int> _eventPriority = new Dictionary<string, int>
    {
        { nameof(IssueProblemPostCommentEvent), 1 },
        { nameof(IssueDeadlineNotificationEvent), 2 },
        { nameof(IssuePriorityUpdatedEvent), 3 },
        { nameof(IssueStatusUpdatedEvent), 4 },
        { nameof(IssueDeadlineUpdatedEvent), 5 },
        { nameof(IssueCompletedEvent), 6},
    };
    public CommentAggregatePlan Build(long issueId, IReadOnlyCollection<IDomainEvent> events)
    {
        FilterEventsByPriority(events, out var filteredEvents);

        return new CommentAggregatePlan(
            IssueId: issueId,
            AssigneId: GetAssigneIdByEvents(filteredEvents),
            ContactDisplayName: GetContactDisplayNameByEvent(filteredEvents),
            HasGreetings: false,
            DomainEvents: filteredEvents
        );
    }
    private string? GetContactDisplayNameByEvent(IEnumerable<IDomainEvent> events)
    {
        foreach (var ev in events)
        {
            switch (ev)
            {
                case IssueStatusUpdatedEvent x:
                    return displayNameService.GetDisplayName(x.ContactFullName);
                case IssuePriorityUpdatedEvent x:
                    return displayNameService.GetDisplayName(x.ContactFullName);
                case IssueDeadlineUpdatedEvent x:
                    return displayNameService.GetDisplayName(x.ContactFullName);
                case IssueDeadlineNotificationEvent x:
                    return displayNameService.GetDisplayName(x.ContactFullName);
            }
        }
        return null;
    }
    private static long? GetAssigneIdByEvents(IEnumerable<IDomainEvent> events)
    {
        foreach (var ev in events)
        {
            switch (ev)
            {
                case IssueStatusUpdatedEvent x:
                    return x.AssigneeId;
                case IssuePriorityUpdatedEvent x:
                    return x.AssigneeId;
                case IssueDeadlineUpdatedEvent x:
                    return x.AssigneeId;
                case IssueDeadlineNotificationEvent x:
                    return x.AssigneeId;
                case IssueCompletedEvent x:
                    return x.AssigneeId;
                case IssueProblemPostCommentEvent x:
                    return x.AssigneeId;
            }
        }
        return null;
    }
    private static void FilterEventsByPriority(
        IReadOnlyCollection<IDomainEvent> events,
        out IEnumerable<IDomainEvent> filteredEvents)
    {
        var prioritizationEvents = events
            .Where(IsComposingEvents)
            .OrderBy(x => _eventPriority.GetValueOrDefault(x.GetType().Name, int.MaxValue))
            .GroupBy(x => x.GetType().Name)
            .Select(x => x.Last())
            .ToList();


        var hasFinal = prioritizationEvents.Any(IsFinalEvent);
        var hasProblemEvent = prioritizationEvents.Any(x => x is IssueProblemPostCommentEvent);

        if (hasProblemEvent)
        {
            filteredEvents = prioritizationEvents
                .Where(x => x is IssueCompletedEvent or
                        IssueProblemPostCommentEvent)
                .ToList();
            return;
        }
        if (hasFinal)
        {
            filteredEvents = prioritizationEvents
               .Where(x => x is IssueCompletedEvent or IssueStatusUpdatedEvent)
               .ToList();
            return;
        }
        filteredEvents = prioritizationEvents;
    }

    private static bool IsComposingEvents(IDomainEvent ev)
    {
        return ev is
            IssueStatusUpdatedEvent or
            IssuePriorityUpdatedEvent or
            IssueDeadlineUpdatedEvent or
            IssueDeadlineNotificationEvent or
            IssueCompletedEvent or
            IssueProblemPostCommentEvent;
    }
    private static bool IsFinalEvent(IDomainEvent ev)
    {
        return ev switch
        {
            IssueCompletedEvent => true,
            IssueStatusUpdatedEvent s => s.NewStatus is IssueStatus.Completed or IssueStatus.Closed or IssueStatus.Wish,
            _ => false
        };
    }
}
