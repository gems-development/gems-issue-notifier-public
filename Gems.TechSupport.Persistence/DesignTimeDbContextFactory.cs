using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Gems.TechSupport.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    ApplicationDbContext IDesignTimeDbContextFactory<ApplicationDbContext>.CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

        IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();


        var connectionString = configuration.GetConnectionString(nameof(ApplicationDbContext))
            ?? throw new InvalidOperationException("Connection string 'ApplicationDbContext' not found.");

        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

        builder.UseNpgsql(connectionString);

        return new ApplicationDbContext(builder.Options);
    }
}