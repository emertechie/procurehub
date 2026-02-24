using ProcureHub.Common.Pagination;

namespace ProcureHub.WebApi.Responses;

public record PagedResponse<T>(IReadOnlyList<T> Data, Pagination Pagination);

public record Pagination(int Page, int PageSize, int TotalCount);

public static class PagedResponse
{
    /// <summary>
    /// Convenience method to convert from domain layer PagedResult[T] type.
    /// </summary>
    public static PagedResponse<T> From<T>(PagedResult<T> pagedResult)
    {
        return new PagedResponse<T>(pagedResult.Data,
            new Pagination(pagedResult.Page, pagedResult.PageSize, pagedResult.TotalCount));
    }
}
