namespace ProcureHub.Common.Pagination;

public record PagedResult<T>(
    IReadOnlyList<T> Data,
    int Page,
    int PageSize,
    int TotalCount
);
