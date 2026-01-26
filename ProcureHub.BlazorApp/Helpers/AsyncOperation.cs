namespace ProcureHub.BlazorApp.Helpers;

/// <summary>
/// Holds state for an async query operation that returns data.
/// </summary>
public sealed class QueryState<T>
{
    public bool IsLoading { get; set; }
    public T? Response { get; set; }
    public Exception? Error { get; set; }   
    
    public bool HasData => Response is not null;
    public bool HasError => Error is not null;
}

/// <summary>
/// Holds state for an async mutation operation (no return data).
/// </summary>
public sealed class MutationState
{
    public bool IsLoading { get; set; }
    public Exception? Error { get; set; }
    
    public bool HasError => Error is not null;
}

/// <summary>
/// Helper methods for executing async operations while managing state.
/// </summary>
public static class AsyncOperation
{
    /// <summary>
    /// Executes a query operation, managing loading state and error handling.
    ///
    /// Caller must ensure no concurrent calls modify the same state object.
    /// </summary>
    public static async Task Query<T>(
        QueryState<T> state,
        Func<Task<T>> fetcher,
        Action? onChanged = null)
    {
        state.IsLoading = true;
        state.Error = null;
        onChanged?.Invoke();
        try
        {
            state.Response = await fetcher();
        }
        catch (Exception ex)
        {
            state.Error = ex;
            // Note: not throwing ex
        }
        finally
        {
            state.IsLoading = false;
            onChanged?.Invoke();
        }
    }

    /// <summary>
    /// Executes a mutation operation, managing loading state and error handling.
    ///
    /// Caller must ensure no concurrent calls modify the same state object.
    /// </summary>
    public static async Task Mutate(
        MutationState state,
        Func<Task> mutator,
        Action? onChanged = null)
    {
        state.IsLoading = true;
        state.Error = null;
        onChanged?.Invoke();
        try
        {
            await mutator();
        }
        catch (Exception ex)
        {
            state.Error = ex;
            // Note: not throwing ex
        }
        finally
        {
            state.IsLoading = false;
            onChanged?.Invoke();
        }
    }
}
