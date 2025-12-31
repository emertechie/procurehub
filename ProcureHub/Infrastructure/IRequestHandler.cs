namespace ProcureHub.Infrastructure;

// ReSharper disable once TypeParameterCanBeVariant
public interface IRequestHandler<TRequest>
{
    Task HandleAsync(TRequest request, CancellationToken token);
}

// ReSharper disable once TypeParameterCanBeVariant
public interface IRequestHandler<TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken token);
}
