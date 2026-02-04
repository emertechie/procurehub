using System.Diagnostics;

namespace ProcureHub;

internal sealed class Instrumentation : IDisposable
{
    private const string ActivitySourceName = "ProcureHub";
    private const string ActivitySourceVersion = "0.0.1";

    public Instrumentation()
    {
        ActivitySource = new ActivitySource(ActivitySourceName, ActivitySourceVersion);
    }

    public ActivitySource ActivitySource { get; }

    public void Dispose()
    {
        ActivitySource.Dispose();
    }
}
