using ProcureHub.Common.Pagination;

namespace ProcureHub.WebApi.ApiResponses;

public static class ApiPagedResponse
{
    /// <summary>
    /// Convenience method to convert from domain layer PagedResult<T> type. 
    /// </summary>
    public static ApiPagedResponse<T> From<T>(PagedResult<T> pagedResult) =>
        new(pagedResult.Data,
            new Pagination(pagedResult.Page, pagedResult.PageSize, pagedResult.TotalCount));
}

public record Pagination(int Page, int PageSize, int TotalCount);

public record ApiPagedResponse<T>(IReadOnlyList<T> Data, Pagination Pagination);
