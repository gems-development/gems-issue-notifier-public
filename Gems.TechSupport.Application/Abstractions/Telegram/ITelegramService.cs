using Gems.TechSupport.Domain.Enums;

namespace Gems.TechSupport.Application.Abstractions.Telegram;

public interface ITelegramService
{
    Task SendIssueNewCommentNotificationAsync(long IssueId, long? assigneeId, string contactFullName, string commentContent, CancellationToken cancellationToken);
    Task SendIssuePriorityUpdatedNotificationAsync(long IssueId, long? assigneeId, string contactFullName, IssuePriority priority, CancellationToken cancellationTokenn);
    Task SendStaleIssueNotificationAsync(long issueId, long? assigneeId, string assigneeFullName, int hoursWithoutComments, CancellationToken cancellationToken);
}
