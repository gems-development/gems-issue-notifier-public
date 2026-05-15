namespace Gems.TechSupport.Infrastructure.BackgroundJobs;

internal sealed class StaleIssueNotificationOptions
{
    public int SilenceThresholdInHours { get; init; } = 24;
    public int RepeatIntervalInHours { get; init; } = 24;
    public int CheckIntervalInMinutes { get; init; } = 60;
}
