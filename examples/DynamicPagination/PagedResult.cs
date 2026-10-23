namespace DynamicPagination;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);

public static class PagedResultExtensions
{
    public static PagedResult<T> ToPagedResult<T>(
        this IQueryable<T> source, int page, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(pageSize));

        // IQueryable provider can translate Count, Skip and Take.
        // For EF Core async execution, prefer CountAsync and ToListAsync.
        var count = source.Count();
        var pages = (int)(((long)count + pageSize - 1) / pageSize);
        var offset = checked((page - 1) * pageSize);
        var items = source.Skip(offset).Take(pageSize).ToList();

        return new PagedResult<T>(
            items, page, pageSize, count, pages,
            page < pages, page > 1);
    }
}
