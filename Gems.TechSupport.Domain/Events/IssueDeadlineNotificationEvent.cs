using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Primitives;

namespace Gems.TechSupport.Domain.Events;

public record IssueDeadlineNotificationEvent(
    long IssueId,
    long AssigneeId,
    string ContactFullName,
    IssueType IssueType,
    IssuePriority IssuePriority)
    : IDomainEvent;
