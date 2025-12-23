namespace ProcureHub.WebApi.Responses;

/// <summary>
/// Ensures the response is wrapped in a Data property.
/// </summary>
public record DataResponse<T>(T Data);

public static class DataResponse
{
    public static DataResponse<T> From<T>(T data)
    {
        return new DataResponse<T>(data);
    }
}
