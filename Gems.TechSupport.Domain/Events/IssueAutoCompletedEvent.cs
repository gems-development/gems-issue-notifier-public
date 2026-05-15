using Gems.TechSupport.Domain.Primitives;

namespace Gems.TechSupport.Domain.Events;

public record IssueAutoCompletedEvent(
    long IssueId,
    long AssigneeId,
    string AutoValue) : IDomainEvent;
