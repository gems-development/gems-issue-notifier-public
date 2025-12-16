using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Primitives;

namespace Gems.TechSupport.Domain.Events;

public record IssuePriorityUpdatedEvent(
    long IssueId,
    long AssigneeId,
    string ContactFullName,
    IssuePriority Priority,
    string UpdateAuthorType) 
    : IDomainEvent;
