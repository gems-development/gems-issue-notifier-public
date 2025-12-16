using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Primitives;

namespace Gems.TechSupport.Domain.Events;

public record IssueStatusUpdatedEvent(long IssueId, long AssigneeId, string ContactFullName, IssueStatus Status) 
    : IDomainEvent;