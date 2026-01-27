namespace ProcureHub.Infrastructure;

/// <summary>Query handler - reads data, returns TResponse</summary>
public interface IQueryHandler<TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken token);
}
