using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Models;

namespace Gems.TechSupport.Application.Abstractions.Telegram;

public interface ITelegramService
{
    Task SendIssueNewCommentNotificationAsync(long IssueId, string contactFullName, string commentContent, CancellationToken cancellationToken);
    Task SendIssuePriorityUpdatedNotificationAsync(long IssueId, long assigneeId, string contactFullName, IssuePriority priority, CancellationToken cancellationTokenn);
}
