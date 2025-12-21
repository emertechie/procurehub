using Microsoft.AspNetCore.Authentication;

namespace ProcureHub.WebApi.Authentication;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// The scheme used to identify API key authentication.
    /// </summary>
    public const string DefaultScheme = "ApiKey";

    public string HeaderName { get; set; } = "X-API-Key";
}
