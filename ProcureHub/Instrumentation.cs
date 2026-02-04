using System.Diagnostics;

namespace ProcureHub;

internal sealed class Instrumentation : IDisposable
{
    private readonly string _activitySourceName = typeof(Instrumentation).Assembly.GetName().Name ?? "ProcureHub";
    private readonly string _activitySourceVersion = (typeof(Instrumentation).Assembly.GetName().Version ?? new Version("0.0.1")).ToString();

    public Instrumentation()
    {
        ActivitySource = new ActivitySource(_activitySourceName, _activitySourceVersion);
    }

    public ActivitySource ActivitySource { get; }

    public void Dispose()
    {
        ActivitySource.Dispose();
    }
}
