using FluentValidation;

namespace ProcureHub.Application.Common.Validation;

/// <summary>Validation decorator for request handlers</summary>
public class ValidationRequestHandlerDecorator<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> inner,
    IValidator<TRequest>? validator = null
) : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken token)
    {
        await ValidationHelper.ValidateAsync(validator, request, token);
        return await inner.HandleAsync(request, token);
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
