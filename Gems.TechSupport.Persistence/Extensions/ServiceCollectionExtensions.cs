using Gems.TechSupport.Application.Abstractions.Data;
using Gems.TechSupport.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gems.TechSupport.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = (configuration.GetConnectionString(nameof(ApplicationDbContext)))!;

        services
            .AddScoped<AuditableEntitiesInterceptor>()
            .AddScoped<DomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(
                serviceProvider.GetRequiredService<AuditableEntitiesInterceptor>(),
                serviceProvider.GetRequiredService<DomainEventsInterceptor>());
        });

        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();


        services.AddHealthChecks()
            .AddNpgSql(connectionString);

        return services;
    }
}
