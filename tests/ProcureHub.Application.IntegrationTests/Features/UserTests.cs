using ProcureHub.Application.IntegrationTests.Infrastructure.BaseTestTypes;
using ProcureHub.Application.IntegrationTests.Infrastructure.xUnit;
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
        throw new NotImplementedException();

        /*// Log in as admin to be able to manage users
        await LoginAsAdminAsync();

        var newUserEmailMixedCase = "User1@Example.COM";
        var newUserEmailMixedCase2 = "USER1@example.com";
        var newUserEmailLower = newUserEmailMixedCase.ToLowerInvariant();

        // Search for new user by email -> No result
        var userList1 = await HttpClient.GetAsync($"/users?email={newUserEmailMixedCase}")
            .ReadJsonAsync<PagedResponse<QueryUsers.Response>>();
        Assert.Empty(userList1.Data);

        // Admin creates user
        var newUserCmd = ValidCreateCommand with { Email = newUserEmailMixedCase };
        var regResp = await HttpClient.PostAsync("/users", JsonContent.Create(newUserCmd));
        Assert.Equal(HttpStatusCode.Created, regResp.StatusCode);

        // Extract new user ID from response
        var createdUser = await regResp.ReadJsonAsync<EntityCreatedResponse<Guid>>();
        var newUserId = createdUser.Id.ToString();

        // Search for new user by email -> Found
        var userList2 = await HttpClient.GetAsync($"/users?email={newUserEmailMixedCase2}")
            .ReadJsonAsync<PagedResponse<QueryUsers.Response>>();
        var newUser = Assert.Single(userList2.Data);
        Assert.Equal(newUserId, newUser.Id);
        Assert.Equal(newUserEmailLower, newUser.Email);

        // Can get new user by ID
        var userById = await HttpClient.GetAsync($"/users/{newUserId}")
            .ReadJsonAsync<DataResponse<GetUserById.Response>>();
        Assert.Equal(newUserId, userById.Data.Id);
        Assert.Equal(newUserEmailLower, userById!.Data.Email);*/
    }
}
