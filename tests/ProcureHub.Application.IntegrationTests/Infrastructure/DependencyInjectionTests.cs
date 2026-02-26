using ProcureHub.Application.Common.Authorization;
using ProcureHub.Application.Features.Users;
using ProcureHub.Application.IntegrationTests.Infrastructure.BaseTestTypes;
using ProcureHub.Application.IntegrationTests.Infrastructure.Xunit;
using ProcureHub.Domain.Common;
using Xunit;

namespace ProcureHub.Application.IntegrationTests.Infrastructure;

[Collection("WebApplicationFactory")]
public class DependencyInjectionTests(
    WebApplicationFactoryFixture webApplicationFactoryFixture,
    ITestOutputHelper testOutputHelper)
    : DbResetBase(webApplicationFactoryFixture, testOutputHelper)
{
    [Fact]
    public async Task Authorization_decorator_runs_before_validation_decorator()
    {
        var invalidCommand = new CreateUser.Command("", "", "", "");

        await Assert.ThrowsAsync<RequestUnauthenticatedException>(
            () => ExecuteCommandAsync<CreateUser.Command, Result<string>>(invalidCommand));
    }
}
