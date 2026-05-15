using Microsoft.Extensions.Configuration;
using Winton.Extensions.Configuration.Consul;

namespace Gems.TechSupport.Infrastructure.Extensions;
public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddConsulConfiguration(
        this IConfigurationBuilder builder)
    {
        var (consulHost, consulToken, consulKey) = GetConsulConnectionSettingsOrThrow();

        builder.AddConsul(consulKey, options =>
        {
            options.ConsulConfigurationOptions =
                cco =>
                {
                    cco.Address = new Uri(consulHost);
                    cco.Token = consulToken;
                };

            options.Optional = false;

            options.PollWaitTime = TimeSpan.FromSeconds(10);

            options.ReloadOnChange = true;

            options.OnLoadException = (consulLoadExceptionContext) =>
            {
                throw consulLoadExceptionContext.Exception;
            };
            options.OnWatchException = (consulWatchExceptionContext) =>
            {
                return TimeSpan.FromSeconds(2);
            };
        });
        return builder;
    }

    private static (string ConsulHost, string ConsulToken, string ConsulKey) GetConsulConnectionSettingsOrThrow()
    {
        var host = Environment.GetEnvironmentVariable("CONSUL_HOST") ?? throw new InvalidOperationException("CONSUL_HOST is not set.");
        var token = Environment.GetEnvironmentVariable("CONSUL_TOKEN") ?? throw new InvalidOperationException("CONSUL_TOKEN is not set.");
        var key = Environment.GetEnvironmentVariable("CONSUL_KEY") ?? throw new InvalidOperationException("CONSUL_KEY is not set.");

        return (ConsulHost: host, ConsulToken: token, ConsulKey: key);
    }
}
