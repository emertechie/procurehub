using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ProcureHub;

internal sealed class Instrumentation : IDisposable
{
    private const string ActivitySourceName = "ProcureHub";
    private const string ActivitySourceVersion = "0.0.1";

    private readonly Meter _meter;

    public Instrumentation(IMeterFactory meterFactory)
    {
        ActivitySource = new ActivitySource(ActivitySourceName, ActivitySourceVersion);

        _meter = meterFactory.Create("ProcureHub");
        DepartmentChangedCounter = _meter.CreateCounter<int>("department_changed");
    }

    public ActivitySource ActivitySource { get; }

    public Counter<int> DepartmentChangedCounter { get; }

    public void Dispose()
    {
        ActivitySource.Dispose();
        _meter.Dispose();
    }
}
