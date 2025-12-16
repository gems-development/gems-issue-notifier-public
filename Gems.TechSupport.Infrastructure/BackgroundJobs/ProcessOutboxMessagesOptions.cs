namespace Gems.TechSupport.Infrastructure.BackgroundJobs;

internal sealed class ProcessOutboxMessagesOptions
{
    public const string ConfigurationSection = "OutboxMessages";

    public required int ProcessIntervalInSeconds { get; init; }
    public required int ProcessMessagesBatchSize { get; init; }
    public required int RetryCount { get; init; }
}
