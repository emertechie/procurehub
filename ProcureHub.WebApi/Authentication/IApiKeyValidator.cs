using System.Security.Claims;

namespace ProcureHub.WebApi.Authentication;

public interface IApiKeyValidator
{
    Task<ApiKeyValidationResult> ValidateAsync(string apiKey);
}

public class ApiKeyValidationResult
{
    public bool IsValid { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public List<Claim> Claims { get; set; } = [];
}
