namespace ProcureHub.Application.Common;

/// <summary>Command handler - mutates state, no return value</summary>
public interface ICommandHandler<in TCommand>
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken);
}

/// <summary>Command handler - mutates state, returns TResponse (e.g., created ID, Result)</summary>
public interface ICommandHandler<in TCommand, TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
