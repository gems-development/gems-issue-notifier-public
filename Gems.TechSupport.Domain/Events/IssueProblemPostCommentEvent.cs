using Gems.TechSupport.Domain.Primitives;

namespace Gems.TechSupport.Domain.Events;

public record IssueProblemPostCommentEvent(
    long IssueId,
    long AssigneeId,
    string ProblemName) : IDomainEvent;
