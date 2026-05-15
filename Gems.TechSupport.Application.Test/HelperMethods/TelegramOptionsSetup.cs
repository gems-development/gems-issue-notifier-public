using Gems.TechSupport.Infrastructure.Services.Telegram;
using Microsoft.Extensions.Options;
using Moq;

namespace Gems.TechSupport.Application.Test.HelperMethods
{
    public static class TelegramOptionsSetup
    {
        public static TelegramOptions DefaultOptions(int? maxCommentLength = null, string? issueCommentCreatedMessageTemplate = null)
        {
            return new TelegramOptions
            {
                BotToken = "1",
                ChatId = "1",
                ThreadId = 1,
                MaxCommentLength = maxCommentLength ?? 4096,
                IssueCommentCreatedMessageTemplate = issueCommentCreatedMessageTemplate ??
                    "Issue [id] from [contact]: [comment]",
                IssuePriorityUpdatedMessageTemplate = "",
                StaleIssueNotificationMessageTemplate = "",
                AssigneeUsername = new Dictionary<long, string>
                {
                    { 42, "@john" }
                }
            };
        }
        public static Mock<IOptionsMonitor<TelegramOptions>> CreateOptionsMonitor(TelegramOptions options)
        {
            var mock = new Mock<IOptionsMonitor<TelegramOptions>>();
            mock.Setup(x => x.CurrentValue).Returns(options);
            return mock;
        }
    }
}
