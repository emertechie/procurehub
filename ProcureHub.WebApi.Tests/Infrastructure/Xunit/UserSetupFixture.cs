using System.Net;
using System.Net.Http.Json;
using ProcureHub.Features.Users;
using ProcureHub.WebApi.Tests.Infrastructure.BaseTestTypes;

namespace ProcureHub.WebApi.Tests.Infrastructure.Xunit;

/// <summary>
/// Fixture that helps create a user one time per class fixture instance.
/// </summary>
public class UserSetupFixture
{
    private bool _userCreated;

    public async Task EnsureUserCreated(
        IHttpClientAuthHelper httpClientAuthHelper,
        string adminEmail,
        string adminPassword,
        string userEmail,
        string userPassword
    )
    {
        if (!_userCreated)
        {
            await httpClientAuthHelper.LoginAsync(adminEmail, adminPassword);

            // Create a user (So we can log in as that user to test endpoints need admin auth)
            var createRequest = new CreateUser.Request(userEmail, userPassword, "Some", "User");
            var createResp = await httpClientAuthHelper.HttpClient.PostAsync("/users", JsonContent.Create(createRequest));
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

            await httpClientAuthHelper.LogoutAsync();

            _userCreated = true;
        }
    }
}
