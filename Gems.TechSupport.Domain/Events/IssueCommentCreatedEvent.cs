using Gems.TechSupport.Domain.Primitives;

namespace Gems.TechSupport.Domain.Events;

public record IssueCommentCreatedEvent(long IssueId, long? AssigneeId, long ContactId, string ContactFullName, bool isCommentPublic, string CommentContent) 
    : IDomainEvent;