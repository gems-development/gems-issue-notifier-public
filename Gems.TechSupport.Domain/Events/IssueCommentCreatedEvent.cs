using Gems.TechSupport.Domain.Primitives;

namespace Gems.TechSupport.Domain.Events;

public record IssueCommentCreatedEvent(long IssueId, long ContactId, string ContactFullName, string CommentContent) 
    : IDomainEvent;