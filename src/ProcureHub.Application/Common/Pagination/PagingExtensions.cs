using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace ProcureHub.Application.Common.Pagination;

public static class PagingExtensions
{
    extension<T>(IOrderedQueryable<T> query)
    {
        public Task<PagedResult<T>> ToPagedResultAsync(
            int? page,
            int? pageSize,
            CancellationToken token = default)
        {
            return ToPagedResultAsyncInternal(
                query,
                page ?? 1,
                pageSize ?? Paging.DefaultPageSize,
                token);
        }

        public async Task<PagedResult<TResult>> ToPagedResultAsync<TResult>(
            Expression<Func<T, TResult>> selector,
            int? page,
            int? pageSize,
            CancellationToken token = default)
        {
            var projectedQueryable = query.Select(selector);

            return await ToPagedResultAsyncInternal(
                projectedQueryable,
                page ?? 1,
                pageSize ?? Paging.DefaultPageSize, token);
        }
    }

    private static async Task<PagedResult<T>> ToPagedResultAsyncInternal<T>(
        IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken token)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(page, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pageSize, Paging.MaxPageSize);

        var itemsToSkip = (page - 1) * pageSize;

        var totalCount = await query.CountAsync(token);

        var pageData = await query
            .Skip(itemsToSkip)
            .Take(pageSize)
            .ToArrayAsync(token);

        return new PagedResult<T>(pageData, page, pageSize, totalCount);
    }
}
