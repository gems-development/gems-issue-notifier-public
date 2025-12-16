using Gems.TechSupport.Application.Extensions;
using Gems.TechSupport.Configuration;
using Gems.TechSupport.Infrastructure.Extensions;
using Gems.TechSupport.Persistence.Extensions;
using Microsoft.AspNetCore.Builder;
using Serilog;
using System.Globalization;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting application");

    WebApplicationBuilder builder = WebApplication.CreateBuilder();

    var configuration = builder.Configuration;
    var services = builder.Services;

    configuration.AddConsulConfiguration();
    configuration.AddEnvironmentVariables();

    services
        .AddApplicationServices()
        .AddPersistenceServices(configuration)
        .AddInfrastructureServices(configuration)
        .AddApiServices(configuration);
    var host = builder
        .Services
        .AddOpenTelemetry()
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter("*")
            .AddOtlpExporter(opt =>
            {
                opt.Endpoint = new Uri(configuration.GetValue<string>("Metrics:MetricsEndpoint"));
                opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            })
        );

    WebApplication app = builder.Build();

    app.Configure();

    app.Run();
}
catch(Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
