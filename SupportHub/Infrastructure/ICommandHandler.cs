namespace SupportHub.Infrastructure;

// ReSharper disable once TypeParameterCanBeVariant
public interface ICommandHandler<TCommand>
{
    Task HandleAsync(TCommand command);
}

// ReSharper disable once TypeParameterCanBeVariant
public interface ICommandHandler<TCommand, TReturn>
{
    Task<TReturn> HandleAsync(TCommand command);
}