using Gems.TechSupport.Application.Exceptions.Handler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using System.Reflection;

namespace Gems.TechSupport.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            config.AddOpenBehavior(typeof(ExceptionHandlingPipelineBehavior<,>));
        });

        services.AddFeatureManagement();

        return services;
    }
}
