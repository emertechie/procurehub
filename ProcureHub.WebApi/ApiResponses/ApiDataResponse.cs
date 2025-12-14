namespace ProcureHub.WebApi.ApiResponses;

public static class ApiDataResponse
{
    public static ApiDataResponse<T> From<T>(T data) => new(data);
}

/// <summary>
/// Ensures the response is wrapped in a Data property.
/// </summary>
public record ApiDataResponse<T>(T Data);