using FluentValidation;

namespace ProcureHub.Infrastructure;

/// <summary>
/// Decorator that runs FluentValidation on requests before passing to the inner handler.
/// </summary>
public class ValidationRequestHandlerDecorator<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> inner,
    IValidator<TRequest>? validator = null
) : IRequestHandler<TRequest, TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken token)
    {
        if (validator is not null)
        {
            var validationResult = await validator.ValidateAsync(request, token);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        return await inner.HandleAsync(request, token);
    }
}

/// <summary>
/// Decorator that runs FluentValidation on requests before passing to the inner handler (void response variant).
/// </summary>
public class ValidationRequestHandlerDecorator<TRequest>(
    IRequestHandler<TRequest> inner,
    IValidator<TRequest>? validator = null
) : IRequestHandler<TRequest>
{
    public async Task HandleAsync(TRequest request, CancellationToken token)
    {
        if (validator is not null)
        {
            var validationResult = await validator.ValidateAsync(request, token);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        await inner.HandleAsync(request, token);
    }
}
