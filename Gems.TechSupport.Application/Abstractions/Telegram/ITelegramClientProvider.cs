using Telegram.Bot;

namespace Gems.TechSupport.Application.Abstractions.Telegram
{
    public interface ITelegramClientProvider
    {
        ITelegramBotClient Client { get; }
    }
}
