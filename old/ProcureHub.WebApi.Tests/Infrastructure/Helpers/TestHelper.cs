namespace ProcureHub.WebApi.Tests.Infrastructure.Helpers;

public static class TestHelper
{
    public static async Task RunTestsForAllAsync<TConfig>(Action<TConfig> configure) where TConfig : new()
    {
        var config = new TConfig();
        configure(config);

        var props = typeof(TConfig).GetProperties();

        foreach (var prop in props)
        {
            var propValue = prop.GetValue(config);
            if (propValue == null)
            {
                throw new InvalidOperationException($"You must implement the {typeof(TConfig).Name}.{prop.Name}' property");
            }

            var func = (Func<Task>)propValue;
            await func();
        }
    }
}
