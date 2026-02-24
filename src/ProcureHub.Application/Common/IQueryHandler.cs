namespace ProcureHub.Application.Common;

/// <summary>Query handler - reads data, returns TResponse</summary>
public interface IQueryHandler<in TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}
