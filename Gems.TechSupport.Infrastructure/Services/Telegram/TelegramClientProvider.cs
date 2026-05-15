using Gems.TechSupport.Application.Abstractions.Telegram;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace Gems.TechSupport.Infrastructure.Services.Telegram
{
    internal sealed class TelegramClientProvider: ITelegramClientProvider
    {
        private ITelegramBotClient _client;

        public TelegramClientProvider(IOptionsMonitor<TelegramOptions> options)
        {
            _client = new TelegramBotClient(options.CurrentValue.BotToken);

            options.OnChange(config =>
            {
                _client = new TelegramBotClient(options.CurrentValue.BotToken);
            });
        }

        public ITelegramBotClient Client => _client;
    }
}
