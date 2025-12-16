using Gems.TechSupport.EndpointsSettings;
using Serilog;

namespace Gems.TechSupport.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSerilogLogging(configuration)
            .AddOpenApi()
            .AddEndpoints(typeof(Program).Assembly);

        return services;
    }

    private static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((services, lc) => lc
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
        );

        return services;
    }
}
