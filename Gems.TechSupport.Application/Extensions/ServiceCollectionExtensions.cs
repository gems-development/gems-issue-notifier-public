using Gems.TechSupport.Application.Abstractions.Masking;
using Gems.TechSupport.Application.Abstractions.Responses;
using Gems.TechSupport.Application.Commands.Okdesk.Logging;
using Gems.TechSupport.Application.Exceptions.Handler;
using Gems.TechSupport.Application.Masking;
using Gems.TechSupport.Application.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using System.Reflection;

namespace Gems.TechSupport.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.AddOpenBehavior(typeof(ExceptionHandlingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingPipelineBehavior<,>));
        });

        services.AddFeatureManagement();

        services.Configure<MaskingOptions>(configuration.GetSection(MaskingOptions.ConfigurationSection));

        services.AddSingleton<IMasker, Masker>();

        services.AddSingleton<IResponseToDomainMapper, ResponseToDomainMapper>();

        return services;
    }
}
