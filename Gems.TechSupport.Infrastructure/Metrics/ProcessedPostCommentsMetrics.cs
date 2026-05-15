using System.Diagnostics.Metrics;

namespace Gems.TechSupport.Infrastructure.Metrics;

public class ProcessedPostCommentsMetrics
{
    private const string MeterName = "ProcessedCommentAggregation";
    private readonly Counter<long> _processedCommentPosted;

    public ProcessedPostCommentsMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _processedCommentPosted = meter.CreateCounter<long>(
            "gems.issue_notifier.okdesk_posted_comments",
            description: "Number of posted Okdesk comments"
        );
    }

    public void RecordPostCommentsProcessedSuccessfully(string eventType)
    {
        _processedCommentPosted.Add(
            1,
            new KeyValuePair<string, object?>("count_comment_post", eventType)
        );
    }
}
