using Gems.TechSupport.EndpointsSettings;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;

namespace Gems.TechSupport.Configuration;

public static class AppExtensions
{
    public static IApplicationBuilder Configure(this WebApplication app)
    {
        RouteGroupBuilder apiGroup = app.MapGroup("/api");

        app.UseEndpoints(apiGroup);
        app.MapHealthChecks("health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return app;
    }
}
