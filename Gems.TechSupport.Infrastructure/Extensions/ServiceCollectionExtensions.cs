using Gems.TechSupport.Application;
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
using System.Threading.RateLimiting;
using Gems.TechSupport.Application.Abstractions.Aggregation;
using Gems.TechSupport.Infrastructure.Services.Aggregation;
using Gems.TechSupport.Infrastructure.Services.Okdesk.Decorators;
using System.Diagnostics.Metrics;
using Gems.TechSupport.Application.Options;


namespace Gems.TechSupport.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOkdeskHttpClient(configuration)
            .AddQuartzBackgroudJobs(configuration);

        services.Configure<TelegramOptions>(configuration.GetSection(TelegramOptions.ConfigurationSection));

        services.AddSingleton<ITelegramClientProvider, TelegramClientProvider>();
        services.AddScoped<ITelegramService, TelegramService>();
        services.AddSingleton<IDisplayNameService, DisplayNameService>();

        services.Configure<ProblemTemplatesOptions>(
            configuration
                .GetSection(OkdeskOptions.ConfigurationSection)
                .GetSection(ProblemTemplatesOptions.ConfigurationSection));

        return services;
    }

    public static IServiceCollection AddRecordMetrics(this IServiceCollection services)
    {
        services.AddSingleton<ProcessedDomainEventsMetrics>(sp =>
        {
            var meterFactory = sp.GetRequiredService<IMeterFactory>();
            return new ProcessedDomainEventsMetrics(meterFactory);
        });
        services.AddSingleton<ProcessedPostCommentsMetrics>(sp =>
        {
            var meterFactory = sp.GetRequiredService<IMeterFactory>();
            return new ProcessedPostCommentsMetrics(meterFactory);
        });
        return services;
    }

    private static IServiceCollection AddOkdeskHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OkdeskOptions>(configuration.GetSection(Constants.ConfigurationSections.Okdesk));

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

        services.Decorate<IOkdeskService, OkdeskServiceWithMetrics>();
        services.AddSingleton<RateLimitedOkdeskService>();
        services.Decorate<IOkdeskService, RateLimitedOkdeskService>();

        services.AddScoped<IOkdeskNotificationTemplatesProvider, OkdeskNotificationTemplatesProvider>();

        services.AddScoped<IProcessedObserver, ProcessedObserver>();

        services.AddScoped<IIssueCommentAggregatePlanBuilder, IssueCommentAggregatePlanBuilder>();

        services.AddScoped<ICommentComposer, CommentComposer>();


        return services;
    }

    private static IServiceCollection AddQuartzBackgroudJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ProcessOutboxMessagesOptions>(
            configuration.GetSection(Constants.ConfigurationSections.OutboxMessages));

        services.Configure<StaleIssueNotificationOptions>(
            configuration.GetSection(Constants.ConfigurationSections.StaleIssueNotification));

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
