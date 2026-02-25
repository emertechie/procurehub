using FluentValidation;

namespace ProcureHub.Application.Common.Validation;

/// <summary>Validation decorator for query handlers</summary>
public class ValidationQueryHandlerDecorator<TRequest, TResponse>(
    IQueryHandler<TRequest, TResponse> inner,
    IValidator<TRequest>? validator = null
) : IQueryHandler<TRequest, TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken token)
    {
        await ValidationHelper.ValidateAsync(validator, request, token);
        return await inner.HandleAsync(request, token);
    }
}

/// <summary>Validation decorator for command handlers with response</summary>
public class ValidationCommandHandlerDecorator<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> inner,
    IValidator<TCommand>? validator = null
) : ICommandHandler<TCommand, TResponse>
{
    public async Task<TResponse> HandleAsync(TCommand command, CancellationToken token)
    {
        await ValidationHelper.ValidateAsync(validator, command, token);
        return await inner.HandleAsync(command, token);
    }
}

file static class ValidationHelper
{
    public static async Task ValidateAsync<T>(IValidator<T>? validator, T request, CancellationToken token)
    {
        if (validator is not null)
        {
            var validationResult = await validator.ValidateAsync(request, token);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }
    }
}
