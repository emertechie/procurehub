using System.Collections.Concurrent;
using ProcureHub.WebApi.Tests.Infrastructure.BaseTestTypes;

namespace ProcureHub.WebApi.Tests.Infrastructure.Xunit;

/// <summary>
/// Fixture that helps create a user one time per class fixture instance.
/// </summary>
public class UserSetupFixture
{
    private ConcurrentDictionary<string, bool> _createdUsers = new();

    public async Task EnsureUserCreated(
        IHttpClientAuthHelper httpClientAuthHelper,
        string adminEmail,
        string adminPassword,
        string userEmail,
        string userPassword,
        string roleName
    )
    {
        if (!_createdUsers.TryAdd($"{userEmail}-${roleName}", true))
        {
            return;
        }

        var httpClient = httpClientAuthHelper.HttpClient;

        try
        {
            await httpClientAuthHelper.LoginAsync(adminEmail, adminPassword);

            await UserHelper.CreateUserAsync(httpClient, userEmail, userPassword, [roleName]);
        }
        finally
        {
            await httpClientAuthHelper.LogoutAsync();
        }
    }
}
