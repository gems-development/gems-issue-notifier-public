using Gems.TechSupport.Domain.Primitives;

namespace Gems.TechSupport.Domain.Events;

public record IssueDeadlineUpdatedEvent(long IssueId, long AssigneeId, string ContactFullName, DateTime DeadlineAt) 
    : IDomainEvent;
