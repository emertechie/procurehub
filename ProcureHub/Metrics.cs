using System.Diagnostics.Metrics;

namespace ProcureHub;

public class Metrics : IDisposable
{
    private readonly Counter<int> _departmentChangedCounter;
    private readonly Meter _meter;

    public Metrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create("ProcureHub");
        _departmentChangedCounter = _meter.CreateCounter<int>("department-changed");
    }

    public void DepartmentChanged(Guid? oldDepartmentId, Guid? newDepartmentId)
    {
        _departmentChangedCounter.Add(1, 
            new KeyValuePair<string, object?>("old", oldDepartmentId),
            new KeyValuePair<string, object?>("new", newDepartmentId));
    }

    public void Dispose()
    {
        _meter.Dispose();
    }
}
