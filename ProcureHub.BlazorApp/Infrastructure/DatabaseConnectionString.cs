namespace ProcureHub.BlazorApp.Infrastructure;

public static class DatabaseConnectionString
{
    public static string GetConnectionString(
        IConfiguration configuration,
        string name = "DefaultConnection",
        string passwordKey = "DatabasePassword")
    {
        ArgumentNullException.ThrowIfNull(configuration);

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
