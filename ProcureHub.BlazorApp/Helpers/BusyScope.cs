namespace ProcureHub.BlazorApp.Helpers;

public class BusyScope : IDisposable
{
    private readonly Action<bool> _stateSetter;
    private readonly Action<bool>? _onStateChanged;

    public BusyScope(Action<bool> stateSetter, Action<bool>? onStateChanged = null)
    {
        _stateSetter = stateSetter;
        _onStateChanged = onStateChanged;

        ChangeState(true);
    }

    public void Dispose()
    {
        ChangeState(false);
    }

    private void ChangeState(bool newState)
    {
        _stateSetter(newState);
        _onStateChanged?.Invoke(newState);
    }
}
