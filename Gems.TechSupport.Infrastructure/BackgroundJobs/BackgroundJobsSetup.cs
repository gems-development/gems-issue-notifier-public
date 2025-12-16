using Gems.TechSupport.Infrastructure.Services.Okdesk;
using Microsoft.Extensions.Options;
using Quartz;

namespace Gems.TechSupport.Infrastructure.BackgroundJobs;

internal sealed class BackgroundJobsSetup(
    IOptionsMonitor<OkdeskOptions> okdeskOptionsMonitor,
    IOptionsMonitor<ProcessOutboxMessagesOptions> outboxOptionsMonitor) 
    : IConfigureOptions<QuartzOptions>
{
    public void Configure(QuartzOptions options)
    {
        var okdeskOptions = okdeskOptionsMonitor.CurrentValue;

        var okdeskJobKey = new JobKey(nameof(OkdeskSyncJob));
        options
            .AddJob<OkdeskSyncJob>(jobKeyBuilder => jobKeyBuilder.WithIdentity(okdeskJobKey))
            .AddTrigger(t => t.ForJob(okdeskJobKey).WithSimpleSchedule(
                s => s.WithIntervalInMinutes(okdeskOptions.IssuesRequestIntervalInMinutes)
                .RepeatForever()));


        // Uncomment to start processing domain events
        var outboxOptions = outboxOptionsMonitor.CurrentValue;

        var outboxJobKey = new JobKey(nameof(ProcessOutboxMessagesJob));
        options
            .AddJob<ProcessOutboxMessagesJob>(jobKeyBuilder => jobKeyBuilder.WithIdentity(outboxJobKey))
            .AddTrigger(t => t.ForJob(outboxJobKey).WithSimpleSchedule(
                s => s.WithIntervalInSeconds(outboxOptions.ProcessIntervalInSeconds)
                .RepeatForever()));
    }
}
