namespace Gems.TechSupport.Application.Abstractions.Aggregation;

public interface ICommentComposer
{
    Task Compose(CommentAggregatePlan plan, CancellationToken cancelattionToken);
}
