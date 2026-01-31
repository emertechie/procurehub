using Microsoft.Extensions.Configuration;

namespace ProcureHub.Infrastructure.Hosting;

public static class DatabaseConnectionString
{
    public static string GetConnectionString(
        IConfiguration configuration,
        string name = "DefaultConnection",
        string passwordKey = "DatabasePassword")
    {
        var connectionString = configuration.GetConnectionString(name)
            ?? throw new InvalidOperationException($"Connection string '{name}' not found.");

        var databasePassword = configuration[passwordKey];
        if (!string.IsNullOrWhiteSpace(databasePassword))
        {
            connectionString += $";Password={databasePassword}";
        }

        return connectionString;
    }
}
