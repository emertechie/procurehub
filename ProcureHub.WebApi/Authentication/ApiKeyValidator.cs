namespace ProcureHub.WebApi.Authentication;

/// <summary>
/// Not implemented yet.
/// </summary>
public class ApiKeyValidator : IApiKeyValidator
{
    private readonly ILogger<ApiKeyValidator> _logger;
    // TODO: Inject your DbContext to query API keys from database
    // private readonly ApplicationDbContext _context;

    public ApiKeyValidator(ILogger<ApiKeyValidator> logger)
    {
        _logger = logger;
    }

    public async Task<ApiKeyValidationResult> ValidateAsync(string apiKey)
    {
        // TODO: Query your database for the API key
        // Example implementation:
        // var storedKey = await _context.ApiKeys
        //     .Include(k => k.Client)
        //     .FirstOrDefaultAsync(k => k.Key == apiKey && k.IsActive);
        //
        // if (storedKey == null)
        // {
        //     _logger.LogWarning("Invalid API key attempt");
        //     return new ApiKeyValidationResult { IsValid = false };
        // }
        //
        // return new ApiKeyValidationResult
        // {
        //     IsValid = true,
        //     ClientName = storedKey.Client.Name,
        //     ClientId = storedKey.ClientId.ToString(),
        //     Claims = new List<Claim>
        //     {
        //         new(ClaimTypes.Role, "ApiClient")
        //     }
        // };

        // For now, return invalid until API key storage is implemented
        _logger.LogWarning("API key validation not yet implemented");

        return new ApiKeyValidationResult
        {
            IsValid = false
        };
    }
}
