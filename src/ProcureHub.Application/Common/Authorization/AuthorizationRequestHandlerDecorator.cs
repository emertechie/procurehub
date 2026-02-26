using ProcureHub.Application.Abstractions.Identity;

namespace ProcureHub.Application.Common.Authorization;

public class AuthorizationRequestHandlerDecorator<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> inner,
    ICurrentUserProvider currentUserProvider
) : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly string RequestName = typeof(TRequest).FullName ?? typeof(TRequest).Name;
    private static readonly AuthorizeRequestAttribute? AuthorizationAttribute = ResolveAuthorizationAttribute();

    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken token)
    {
        if (AuthorizationAttribute is null)
        {
            throw new MissingRequestAuthorizationAttributeException(RequestName);
        }

        var currentUser = await currentUserProvider.GetCurrentUserAsync();

        if (!currentUser.UserId.HasValue)
        {
            throw new RequestUnauthenticatedException(RequestName);
        }

        if (AuthorizationAttribute.Roles.Count != 0
            && !AuthorizationAttribute.Roles.Any(currentUser.IsInRole))
        {
            throw new RequestForbiddenException(RequestName, AuthorizationAttribute.Roles);
        }

        return await inner.HandleAsync(request, token);
    }

    private static AuthorizeRequestAttribute? ResolveAuthorizationAttribute()
    {
        return typeof(TRequest)
            .GetCustomAttributes(typeof(AuthorizeRequestAttribute), false)
            .Cast<AuthorizeRequestAttribute>()
            .FirstOrDefault();
    }
}
