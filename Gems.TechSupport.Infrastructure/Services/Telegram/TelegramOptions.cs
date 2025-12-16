namespace Gems.TechSupport.Infrastructure.Services.Telegram
{
    public class TelegramOptions
    {
        public const string ConfigurationSection = "Telegram";
        public required string BotToken { get; init; } = string.Empty;
        public required string ChatId { get; init; } = string.Empty;
        public required int? ThreadId { get; init; }
        public required string IssueCommentCreatedMessageTemplate { get; init; } = string.Empty;
        public required string IssuePriorityUpdatedMessageTemplate { get; init; } = string.Empty;
    }
}
