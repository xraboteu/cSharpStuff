namespace EnumerablePagination;

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
        this IEnumerable<T> source, int page, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(pageSize));

        // Materialize once: handles single-use IEnumerable sources safely.
        var items = source as IReadOnlyList<T> ?? source.ToList();
        var count = items.Count;
        var pages = (int)(((long)count + pageSize - 1) / pageSize);
        var offset = checked((page - 1) * pageSize);
        var result = items.Skip(offset).Take(pageSize).ToList();

        return new PagedResult<T>(
            result, page, pageSize, count, pages,
            page < pages, page > 1);
    }
}
