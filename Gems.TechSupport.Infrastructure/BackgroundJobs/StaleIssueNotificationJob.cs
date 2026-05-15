using Gems.TechSupport.Application.Abstractions.Telegram;
using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Models;
using Gems.TechSupport.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Gems.TechSupport.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
internal sealed class StaleIssueNotificationJob(
    ApplicationDbContext dbContext,
    ITelegramService telegramService,
    IOptionsMonitor<StaleIssueNotificationOptions> optionsMonitor,
    ILogger<StaleIssueNotificationJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var options = optionsMonitor.CurrentValue;
        var now = DateTime.UtcNow;
        var silenceThreshold = now.AddHours(-options.SilenceThresholdInHours);

        var activeIssues = await GetActiveIssuesAsync(context.CancellationToken);

        foreach (var issue in activeIssues)
        {
            if (issue.IsSkitType) continue;

            var lastCommentAt = issue.Comments.Count > 0
                ? issue.Comments.Max(c => c.CreatedAt)
                : (DateTime?)null;

            var lastActivity = lastCommentAt ?? issue.CreatedAt;
            if (lastActivity is null || lastActivity >= silenceThreshold) continue;

            if (issue.StaleNotifiedAt is not null)
            {
                if (issue.StaleNotifiedAt > lastActivity
                    && (now - issue.StaleNotifiedAt.Value).TotalHours < options.RepeatIntervalInHours)
                    continue;
            }

            var hours = (int)(now - lastActivity.Value).TotalHours;

            try
            {
                await telegramService.SendStaleIssueNotificationAsync(
                    issue.Id, issue.Assignee!.Id, issue.Assignee!.FullName, hours, context.CancellationToken);

                issue.MarkAsStaleNotified();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Не удалось отправить уведомление о неактивности заявки {IssueId}", issue.Id);
            }
        }

        await dbContext.SaveChangesAsync(context.CancellationToken);

        var trigger = context.Trigger as ISimpleTrigger;

        if (trigger != null)
        {

            var currentIntervalMinutes = (int)trigger.RepeatInterval.TotalMinutes;

            if (currentIntervalMinutes != options.CheckIntervalInMinutes)
                await QuartzTriggerUpdater.UpdateIntervalMinutesAsync(context, options.CheckIntervalInMinutes);
        }
    }

    private Task<List<Issue>> GetActiveIssuesAsync(CancellationToken cancellationToken)
    {
        return dbContext.Issues
            .Include(i => i.Assignee)
            .Include(i => i.Comments)
            .Where(i => i.Status != IssueStatus.Closed
                        && i.Status != IssueStatus.Completed
                        && i.Status != null
                        && i.Assignee != null)
            .ToListAsync(cancellationToken);
    }
}
