using Gems.TechSupport.Application.Commands.Okdesk;
using Gems.TechSupport.Infrastructure.Services.Okdesk;
using MediatR;
using Microsoft.Extensions.Options;
using Quartz;

namespace Gems.TechSupport.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
internal sealed class OkdeskSyncJob(IOptionsMonitor<OkdeskOptions> options, ISender sender) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var okdeskOptions = options.CurrentValue;

        var dateTimeUntil = DateTime.Now;
        DateTime dateTimeSince;
        if (context.PreviousFireTimeUtc is not null)
        {
            dateTimeSince = context.PreviousFireTimeUtc.Value.DateTime;
        } 
        else
        {
            dateTimeSince = dateTimeUntil
                .Subtract(TimeSpan.FromMinutes(okdeskOptions.IssuesRequestIntervalInMinutes));
        }

        var syncCommand = new OkdeskSyncCommand(dateTimeSince, dateTimeUntil, 50);
        await sender.Send(syncCommand, context.CancellationToken);
    }
}
