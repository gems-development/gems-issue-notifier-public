using Gems.TechSupport.Domain.Primitives;

namespace Gems.TechSupport.Application.Abstractions.Aggregation;

public interface IIssueCommentAggregatePlanBuilder
{
    CommentAggregatePlan Build(long issueId, IReadOnlyCollection<IDomainEvent> events);
}
