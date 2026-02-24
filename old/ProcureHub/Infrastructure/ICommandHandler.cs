namespace ProcureHub.Infrastructure;

/// <summary>Command handler - mutates state, no return value</summary>
public interface ICommandHandler<in TCommand>
{
    Task HandleAsync(TCommand command, CancellationToken token);
}

/// <summary>Command handler - mutates state, returns TResponse (e.g., created ID, Result)</summary>
public interface ICommandHandler<in TCommand, TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken token);
}
