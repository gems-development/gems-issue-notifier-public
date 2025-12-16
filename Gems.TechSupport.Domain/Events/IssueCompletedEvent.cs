using Gems.TechSupport.Domain.Primitives;

namespace Gems.TechSupport.Domain.Events;

public record IssueCompletedEvent(long IssueId, long AssigneeId) : IDomainEvent;