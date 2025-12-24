namespace ProcureHub.WebApi.Tests.Infrastructure.BaseTestTypes;

public interface IHttpClientAuthHelper
{
    HttpClient HttpClient { get; }

    Task LoginAsync(string email, string password);

    Task LogoutAsync();
}
