using Gems.TechSupport.EndpointsSettings;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Gems.TechSupport.Configuration;

public static class AppExtensions
{
    public static IApplicationBuilder Configure(this WebApplication app)
    {
        RouteGroupBuilder apiGroup = app.MapGroup("/api");
        app.UseEndpoints(apiGroup);
        
        app.MapHealthChecks("/health", new HealthCheckOptions 
        { 
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    System.Text.Json.JsonSerializer.Serialize(new 
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new 
                        { 
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description
                        })
                    })
                );
            }
        });

        return app;
    }
}
