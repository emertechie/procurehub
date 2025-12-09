namespace SupportHub.Infrastructure;

// ReSharper disable once TypeParameterCanBeVariant
public interface IRequestHandler<TCommand>
{
    Task HandleAsync(TCommand command, CancellationToken token);
}

// ReSharper disable once TypeParameterCanBeVariant
public interface IRequestHandler<TCommand, TReturn>
{
    Task<TReturn> HandleAsync(TCommand command, CancellationToken token);
}