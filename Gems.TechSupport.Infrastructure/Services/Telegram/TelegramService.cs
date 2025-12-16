using Gems.TechSupport.Application.Abstractions.Telegram;
using Gems.TechSupport.Domain.Enums;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
namespace Gems.TechSupport.Infrastructure.Services.Telegram;

public class TelegramService(ITelegramBotClient telegramClient, IOptionsMonitor<TelegramOptions> options) : ITelegramService
{
    public Task SendIssueNewCommentNotificationAsync(long IssueId, string contactFullName, string commentContent, CancellationToken cancellationToken)
    {
        var telegramOptions = options.CurrentValue;

        var messageContent = telegramOptions.IssueCommentCreatedMessageTemplate
            .Replace("[id]", IssueId.ToString())
            .Replace("[contact]", contactFullName)
            .Replace("[comment]", commentContent);

        return telegramClient.SendMessage(telegramOptions.ChatId,messageContent,
                    ParseMode.Html, messageThreadId: telegramOptions.ThreadId, cancellationToken: cancellationToken);
    }

    public Task SendIssuePriorityUpdatedNotificationAsync(long IssueId, long assigneeId, string contactFullName, IssuePriority priority, CancellationToken cancellationToken)
    {
        var telegramOptions = options.CurrentValue;

        var messageContent = telegramOptions.IssuePriorityUpdatedMessageTemplate
            .Replace("[id]", IssueId.ToString())
            .Replace("[contact]", contactFullName)
            .Replace("[priority]", TranslatePriority(priority));

        return telegramClient.SendMessage(telegramOptions.ChatId, messageContent,
                    ParseMode.Html, messageThreadId: telegramOptions.ThreadId, cancellationToken: cancellationToken);
    }

    private string TranslatePriority(IssuePriority priority)
    {
        return priority switch
        {
            IssuePriority.Low => "Низкий",
            IssuePriority.Normal => "Обычный",
            IssuePriority.High => "Высокий",
            IssuePriority.Highest => "Высший",
            _=> "Неизвестный"
        };
    }
}
