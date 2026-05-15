using Gems.TechSupport.Application.Abstractions.Telegram;
using Gems.TechSupport.Domain.Enums;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
namespace Gems.TechSupport.Infrastructure.Services.Telegram;

public class TelegramService(ITelegramClientProvider telegramClientProvider, IOptionsMonitor<TelegramOptions> options) : ITelegramService
{
    private const int MAX_MESSAGE_LENGTH = 4096;
    private const string commentPlaceholder = "[comment]";
    private string _ellipsis = "...";
    public Task SendIssueNewCommentNotificationAsync(long IssueId, long? assigneeId, string contactFullName, string commentContent, CancellationToken cancellationToken)
    {

        var telegramOptions = options.CurrentValue;

        if (telegramOptions.MaxCommentLength <= 0)
        {
            throw new InvalidOperationException("Invalid MaxCommentLength");
        }

        string assigneeUsername = "";

        if (assigneeId.HasValue &&
            telegramOptions.AssigneeUsername.TryGetValue(assigneeId.Value, out var username))
        {
            assigneeUsername = username;
        }

        commentContent = TelegramHtmlConfiguration.Sanitize(commentContent);


        var messageContent = telegramOptions.IssueCommentCreatedMessageTemplate
            .Replace("[id]", IssueId.ToString())
            .Replace("[contact]", contactFullName)
            .Replace("[assignee_username]", assigneeUsername);

        messageContent = ApplyComment(messageContent, commentContent, telegramOptions);

        var telegramClient = telegramClientProvider.Client;

        return telegramClient.SendMessage(telegramOptions.ChatId, messageContent,
                    ParseMode.Html, messageThreadId: telegramOptions.ThreadId, cancellationToken: cancellationToken);
    }

    public Task SendIssuePriorityUpdatedNotificationAsync(long IssueId, long? assigneeId, string contactFullName, IssuePriority priority, CancellationToken cancellationToken)
    {
        var telegramOptions = options.CurrentValue;

        string assigneeUsername = "";

        if (assigneeId.HasValue &&
            telegramOptions.AssigneeUsername.TryGetValue(assigneeId.Value, out var username))
        {
            assigneeUsername = username;
        }

        var messageContent = telegramOptions.IssuePriorityUpdatedMessageTemplate
            .Replace("[id]", IssueId.ToString())
            .Replace("[contact]", contactFullName)
            .Replace("[priority]", TranslatePriority(priority))
            .Replace("[assignee_username]", assigneeUsername);

        var telegramClient = telegramClientProvider.Client;

        return telegramClient.SendMessage(telegramOptions.ChatId, messageContent,
                    ParseMode.Html, messageThreadId: telegramOptions.ThreadId, cancellationToken: cancellationToken);
    }

    public Task SendStaleIssueNotificationAsync(long issueId, long? assigneeId, string assigneeFullName, int hoursWithoutComments, CancellationToken cancellationToken)
    {
        var telegramOptions = options.CurrentValue;

        string assigneeUsername = "";

        if (assigneeId.HasValue &&
            telegramOptions.AssigneeUsername.TryGetValue(assigneeId.Value, out var username))
        {
            assigneeUsername = username;
        }

        var template = telegramOptions.StaleIssueNotificationMessageTemplate;
        if (string.IsNullOrEmpty(assigneeUsername))
        {
            template = template.Replace("@[assignee_username], ", string.Empty);
        }

        var messageContent = template
            .Replace("[id]", issueId.ToString())
            .Replace("[assignee]", assigneeFullName)
            .Replace("[assignee_username]", assigneeUsername)
            .Replace("[hours]", hoursWithoutComments.ToString());

        var telegramClient = telegramClientProvider.Client;

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
            _ => "Неизвестный"
        };
    }
    private string ApplyComment(string template, string comment, TelegramOptions telegramOptions)
    {
        int templateWithoutPlaceholderLength = template.Length - commentPlaceholder.Length;
        int maxCommentByTemplate = MAX_MESSAGE_LENGTH - templateWithoutPlaceholderLength;
        if (maxCommentByTemplate <= 0)
        {
            throw new InvalidOperationException("Template content exceeds maximum message length.");
        }
        int allowedCommentLength = Math.Min(telegramOptions.MaxCommentLength, maxCommentByTemplate);
        if (comment.Length > allowedCommentLength)
        {
            int cutLength = Math.Min(allowedCommentLength, maxCommentByTemplate - _ellipsis.Length);
            comment = comment.Substring(0, cutLength) + _ellipsis;
        }
        return template.Replace(commentPlaceholder, comment);
    }
}