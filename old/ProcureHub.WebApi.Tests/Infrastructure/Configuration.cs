using Microsoft.Extensions.Configuration;

namespace ProcureHub.WebApi.Tests.Infrastructure;

public static class Configuration
{
    private static readonly Lazy<IConfigurationRoot> ConfigurationBuilder = new(() => new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build());

    public static string GetConnectionString()
    {
        return ConfigurationBuilder.Value.GetConnectionString("DefaultConnection") ??
               throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }
}
