namespace Gems.TechSupport.Infrastructure.BackgroundJobs;

internal sealed class ProcessOutboxMessagesOptions
{
    public required int IntervalInHoursForGreetings { get; init; }
    public required int ProcessIntervalInSecondsForDomainEvent { get; init; }
    public required int ProcessIntervalInSecondsForCommentAggregator { get; init; }
    public required int ProcessMessagesBatchSize { get; init; }
    public required int RetryCount { get; init; }
}
