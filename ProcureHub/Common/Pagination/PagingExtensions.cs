using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace ProcureHub.Common.Pagination;

public static class PagingExtensions
{
    extension<T>(IOrderedQueryable<T> query)
    {
        public Task<PagedResult<T>> ToPagedResultAsync(
            int page,
            int pageSize,
            CancellationToken token = default)
        {
            return ToPagedResultAsyncInternal(query, page, pageSize, token);
        }

        public async Task<PagedResult<TResult>> ToPagedResultAsync<TResult>(int page,
            int pageSize,
            Expression<Func<T, TResult>> selector,
            CancellationToken token = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(page, 0);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);
        
            var projectedQueryable = query.Select(selector);

            return await ToPagedResultAsyncInternal(projectedQueryable, page, pageSize, token);
        }
    }

    private static async Task<PagedResult<T>> ToPagedResultAsyncInternal<T>(
        IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken token)
    {
        var itemsToSkip = (page - 1) * pageSize;

        var totalCountTask = query.CountAsync(token);

        var pageDataTask = query
            .Skip(itemsToSkip)
            .Take(pageSize)
            .ToArrayAsync(token);

        // Execute count and page query in parallel
        await Task.WhenAll(totalCountTask, pageDataTask);

        return new PagedResult<T>(pageDataTask.Result, page, pageSize, totalCountTask.Result);
    }
}