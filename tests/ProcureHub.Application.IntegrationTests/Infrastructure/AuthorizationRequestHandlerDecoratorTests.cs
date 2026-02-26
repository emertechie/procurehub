using ProcureHub.Application.Abstractions.Identity;
using ProcureHub.Application.Common;
using ProcureHub.Application.Common.Authorization;
using Xunit;

namespace ProcureHub.Application.IntegrationTests.Infrastructure;

public class AuthorizationRequestHandlerDecoratorTests
{
    [Fact]
    public async Task Throws_when_request_has_no_authorize_request_attribute()
    {
        var decorator = new AuthorizationRequestHandlerDecorator<RequestWithoutAuthorizationAttribute, string>(
            new PassThroughHandler(),
            new UnusedCurrentUserProvider());

        await Assert.ThrowsAsync<MissingRequestAuthorizationAttributeException>(
            () => decorator.HandleAsync(new RequestWithoutAuthorizationAttribute(), CancellationToken.None));
    }

    private sealed record RequestWithoutAuthorizationAttribute() : IRequest<string>;

    private sealed class PassThroughHandler : IRequestHandler<RequestWithoutAuthorizationAttribute, string>
    {
        public Task<string> HandleAsync(RequestWithoutAuthorizationAttribute request, CancellationToken cancellationToken)
        {
            return Task.FromResult("ok");
        }
    }

    private sealed class UnusedCurrentUserProvider : ICurrentUserProvider
    {
        public Task<ICurrentUser> GetCurrentUserAsync()
        {
            throw new NotImplementedException();
        }
    }
}
