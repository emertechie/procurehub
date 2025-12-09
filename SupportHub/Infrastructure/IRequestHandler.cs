namespace SupportHub.Infrastructure;

// ReSharper disable once TypeParameterCanBeVariant
public interface IRequestHandler<TRequest>
{
    Task HandleAsync(TRequest request, CancellationToken token);
}

// ReSharper disable once TypeParameterCanBeVariant
public interface IRequestHandler<TRequest, TReturn>
{
    Task<TReturn> HandleAsync(TRequest request, CancellationToken token);
}