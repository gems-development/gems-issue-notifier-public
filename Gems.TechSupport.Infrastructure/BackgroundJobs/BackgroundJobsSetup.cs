using Gems.TechSupport.Infrastructure.Services.Okdesk;
using Microsoft.Extensions.Options;
using Quartz;

namespace Gems.TechSupport.Infrastructure.BackgroundJobs;

internal sealed class BackgroundJobsSetup(
    IOptionsMonitor<OkdeskOptions> okdeskOptionsMonitor,
    IOptionsMonitor<ProcessOutboxMessagesOptions> outboxOptionsMonitor,
    IOptionsMonitor<StaleIssueNotificationOptions> staleOptionsMonitor)
    : IConfigureOptions<QuartzOptions>
{
    public void Configure(QuartzOptions options)
    {
        var okdeskOptions = okdeskOptionsMonitor.CurrentValue;

        var okdeskJobKey = new JobKey(nameof(OkdeskSyncJob));

        options
            .AddJob<OkdeskSyncJob>(jobKeyBuilder => jobKeyBuilder.WithIdentity(okdeskJobKey))
            .AddTrigger(t => t
                .ForJob(okdeskJobKey)
                .WithSimpleSchedule(
                    s => s.WithIntervalInMinutes(okdeskOptions.IssuesRequestIntervalInMinutes)
                    .RepeatForever()));

#if !DEBUG
        var domainEventOutboxOptions = outboxOptionsMonitor.CurrentValue;
        
        var domainEventOutboxJobKey = new JobKey(nameof(ProcessDomainEventOutboxMessagesJob));
        options
            .AddJob<ProcessDomainEventOutboxMessagesJob>(jobKeyBuilder => jobKeyBuilder.WithIdentity(domainEventOutboxJobKey))
            .AddTrigger(t => t.ForJob(domainEventOutboxJobKey).WithSimpleSchedule(
                s => s.WithIntervalInSeconds(domainEventOutboxOptions.ProcessIntervalInSecondsForDomainEvent)
                .RepeatForever()));
        
        
        var issueCommentAggregatorOutboxOptions = outboxOptionsMonitor.CurrentValue;
        
        var issueCommentAggregatorOutboxJobKey = new JobKey(nameof(ProcessIssueCommentAggregatorOutboxMessagesJob));
        options
            .AddJob<ProcessIssueCommentAggregatorOutboxMessagesJob>(jobKeyBuilder => jobKeyBuilder.WithIdentity(issueCommentAggregatorOutboxJobKey))
            .AddTrigger(t => t.ForJob(issueCommentAggregatorOutboxJobKey).WithSimpleSchedule(
                s => s.WithIntervalInSeconds(issueCommentAggregatorOutboxOptions.ProcessIntervalInSecondsForCommentAggregator)
                .RepeatForever()));

        var staleOptions = staleOptionsMonitor.CurrentValue;

        var staleJobKey = new JobKey(nameof(StaleIssueNotificationJob));
        options
            .AddJob<StaleIssueNotificationJob>(jobKeyBuilder => jobKeyBuilder.WithIdentity(staleJobKey))
            .AddTrigger(t => t.ForJob(staleJobKey).WithSimpleSchedule(
                s => s.WithIntervalInMinutes(staleOptions.CheckIntervalInMinutes)
                .RepeatForever()));
#endif
    }
}
