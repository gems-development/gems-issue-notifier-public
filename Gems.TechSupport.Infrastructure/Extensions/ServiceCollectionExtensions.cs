using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Abstractions.Telegram;
using Gems.TechSupport.Infrastructure.BackgroundJobs;
using Gems.TechSupport.Infrastructure.Metrics;
using Gems.TechSupport.Infrastructure.Services.Okdesk;
using Gems.TechSupport.Infrastructure.Services.Telegram;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using Quartz;
using Telegram.Bot;
using System.Threading.RateLimiting;


namespace Gems.TechSupport.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOkdeskHttpClient(configuration)
            .AddTelegramBotClient(configuration)
            .AddQuartzBackgroudJobs(configuration);

        services.AddScoped<ITelegramService, TelegramService>();

        services.AddSingleton<ProcessedDomainEventsMetrics>();

        return services;
    }

    private static IServiceCollection AddOkdeskHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OkdeskOptions>(configuration.GetSection(OkdeskOptions.ConfigurationSection));

        services.AddHttpClient<IOkdeskService, OkdeskService>((serviceProvider, options) =>
        {
            var okdeskSettings = serviceProvider.GetRequiredService<IOptionsMonitor<OkdeskOptions>>().CurrentValue;

            options.BaseAddress = new Uri(okdeskSettings.BaseAddress);
        }).AddResilienceHandler("okdesk-resilience-pipeline", (pipeline, context) =>
        {
            var okdeskSettings = context.ServiceProvider.GetRequiredService<IOptionsMonitor<OkdeskOptions>>().CurrentValue;

            pipeline.AddRateLimiter(new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                TokenLimit = okdeskSettings.RequestsPerSecondLimit,
                TokensPerPeriod = okdeskSettings.RequestsPerSecondLimit,
                ReplenishmentPeriod = TimeSpan.FromSeconds(okdeskSettings.RequestsPerSecondLimit),
                QueueLimit = int.MaxValue,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            }));

            pipeline.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromSeconds(1)
            });
        });

        services.AddSingleton<RateLimitedOkdeskService>();
        services.Decorate<IOkdeskService, RateLimitedOkdeskService>();

        services.AddScoped<IOkdeskNotificationTemplatesProvider, OkdeskNotificationTemplatesProvider>();

        return services;
    }

    private static IServiceCollection AddTelegramBotClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TelegramOptions>(configuration.GetSection(TelegramOptions.ConfigurationSection));

        services.AddSingleton<ITelegramBotClient>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptionsMonitor<TelegramOptions>>().CurrentValue;
            TelegramBotClient client;
            try
            {
                client = new TelegramBotClient(options.BotToken);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(e.Message);
            }

            return client;
        });


        return services;
    }

    private static IServiceCollection AddQuartzBackgroudJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ProcessOutboxMessagesOptions>(
            configuration.GetSection(ProcessOutboxMessagesOptions.ConfigurationSection));

        services.AddQuartz();
        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
            options.AwaitApplicationStarted = true;
        });

        services.ConfigureOptions<BackgroundJobsSetup>();

        return services;
    }
}
