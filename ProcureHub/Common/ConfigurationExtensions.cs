using Microsoft.Extensions.Configuration;

namespace ProcureHub.Common;

public static class ConfigurationExtensions
{
    public static string GetRequiredString(this IConfiguration configuration, string key)
    {
        var value = configuration[key];
        return string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"Configuration value '{key}' is required.")
            : value;
    }
}
