using Gems.TechSupport.Application.Extensions;
using Gems.TechSupport.Configuration;
using Gems.TechSupport.Extensions;
using Gems.TechSupport.Infrastructure.Extensions;
using Gems.TechSupport.Persistence.Extensions;
using Microsoft.AspNetCore.Builder;
using OpenTelemetry.Metrics;
using Serilog;
using System.Globalization;


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

    builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration));

    var configuration = builder.Configuration;
    var services = builder.Services;

    configuration.AddConsulConfiguration();
    configuration.AddEnvironmentVariables();

#if DEBUG
    configuration.CheckOrThrow();
#endif

    services
        .AddApplicationServices(configuration)
        .AddPersistenceServices(configuration)
        .AddInfrastructureServices(configuration)
        .AddRecordMetrics()
        .AddApiServices(configuration);
    builder
        .Services
        .AddOpenTelemetry()
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter("*")
            .AddMeter("Gems.TechSupport.Infrastructure.Metrics")
            .AddOtlpExporter(opt =>
            {
                opt.Endpoint = new Uri(configuration["Otlp:Endpoint"] ?? "http://localhost:4318/v1/metrics");
                opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            })

        );
    builder.Services.AddHealthChecks();


    WebApplication app = builder.Build();


#if DEBUG
    await app.EnsureMigrations();
#endif

    app.Configure();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
