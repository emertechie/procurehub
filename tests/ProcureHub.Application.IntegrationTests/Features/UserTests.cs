using ProcureHub.Application.IntegrationTests.Infrastructure.BaseTestTypes;
using ProcureHub.Application.IntegrationTests.Infrastructure.Identity;
using ProcureHub.Application.Common.Authorization;
using ProcureHub.Application.IntegrationTests.Infrastructure.Xunit;
using ProcureHub.Application.Common.Pagination;
using ProcureHub.Application.Features.Users;
using ProcureHub.Application.Constants;
using ProcureHub.Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ProcureHub.Application.IntegrationTests.Features;

[Collection("WebApplicationFactory")]
public class UserTests(
    WebApplicationFactoryFixture webApplicationFactoryFixture,
    ITestOutputHelper testOutputHelper)
    : DbResetBase(webApplicationFactoryFixture, testOutputHelper)
{
    [Fact]
    public async Task Admin_user_can_create_user()
    {
        using var scope = CreateScope();
        var identityData = scope.ServiceProvider.GetRequiredService<IdentityTestDataBuilder>();
        var adminUser = await identityData.EnsureAdminAsync();

        using var _ = RunAsAdmin(Guid.Parse(adminUser.Id));

        var newUserEmailMixedCase = "User1@Example.COM";
        var newUserEmailMixedCase2 = "USER1@example.com";
        var newUserEmailLower = newUserEmailMixedCase.ToLowerInvariant();

        // Search for new user by email -> No result
        var userList1 = await ExecuteQueryAsync<QueryUsers.Request, PagedResult<QueryUsers.Response>>(
            new QueryUsers.Request(newUserEmailMixedCase, null, null));
        Assert.Empty(userList1.Data);

        // Admin creates user
        var newUserCmd = new CreateUser.Command(newUserEmailMixedCase, IdentityTestDataBuilder.ValidPassword, "Some", "User");
        var createResult = await ExecuteCommandAsync<CreateUser.Command, Result<string>>(newUserCmd);
        Assert.True(createResult.IsSuccess, createResult.IsFailure ? createResult.Error.Message : null);
        var newUserId = createResult.Value;

        // Can find new user by email
        var userList2 = await ExecuteQueryAsync<QueryUsers.Request, PagedResult<QueryUsers.Response>>(
            new QueryUsers.Request(newUserEmailMixedCase2, null, null));
        var newUser = Assert.Single(userList2.Data);
        Assert.Equal(newUserId, newUser.Id);
        Assert.Equal(newUserEmailLower, newUser.Email);

        // Can get new user by ID
        var userById = await ExecuteQueryAsync<GetUserById.Request, GetUserById.Response?>(new GetUserById.Request(newUserId));
        Assert.NotNull(userById);
        Assert.Equal(newUserId, userById!.Id);
        Assert.Equal(newUserEmailLower, userById.Email);
    }

    [Fact]
    public async Task Non_admin_user_cannot_create_user()
    {
        using var scope = CreateScope();
        var identityData = scope.ServiceProvider.GetRequiredService<IdentityTestDataBuilder>();
        var requesterUser = await identityData.EnsureUserAsync(
            email: "test-requester@example.com",
            password: IdentityTestDataBuilder.ValidPassword,
            firstName: "Test",
            lastName: "Requester",
            roles: [RoleNames.Requester]);

        using var _ = RunAs(Guid.Parse(requesterUser.Id), RoleNames.Requester);

        var command = new CreateUser.Command("new-user@example.com", IdentityTestDataBuilder.ValidPassword, "New", "User");

        await Assert.ThrowsAsync<RequestForbiddenException>(
            () => ExecuteCommandAsync<CreateUser.Command, Result<string>>(command));
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_user()
    {
        var command = new CreateUser.Command("new-user@example.com", IdentityTestDataBuilder.ValidPassword, "New", "User");

        await Assert.ThrowsAsync<RequestUnauthenticatedException>(
            () => ExecuteCommandAsync<CreateUser.Command, Result<string>>(command));
    }
}
