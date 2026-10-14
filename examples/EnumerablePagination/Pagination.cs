using System.Reflection;
using System.Text.Json.Serialization;

namespace EnumerablePagination;

public sealed record PaginationRequest(int Page = 1, int PageSize = 20, List<SortCriteria>? Sort = null);
public sealed record SortCriteria(string Field, SortDirection Direction);

[JsonConverter(typeof(JsonStringEnumConverter<SortDirection>))]
public enum SortDirection { Asc, Desc }

public static class EnumerablePaginationExtensions
{
    public static IEnumerable<T> ApplySort<T>(
        this IEnumerable<T> source,
        IEnumerable<SortCriteria>? sorts,
        IReadOnlySet<string>? allowedFields = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (sorts is null) return source;

        IOrderedEnumerable<T>? ordered = null;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var sort in sorts)
        {
            if (sort is null || string.IsNullOrWhiteSpace(sort.Field))
                throw new ArgumentException("Sort field is required.", nameof(sorts));
            if (!Enum.IsDefined(sort.Direction))
                throw new ArgumentOutOfRangeException(nameof(sorts), "Invalid sort direction.");
            var property = typeof(T).GetProperty(sort.Field,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property is null || !property.CanRead || property.GetIndexParameters().Length != 0)
                throw new ArgumentException($"Unknown sort field: {sort.Field}", nameof(sorts));
            if (!seen.Add(property.Name))
                throw new ArgumentException($"Duplicate sort field: {sort.Field}", nameof(sorts));
            if (allowedFields is not null && !allowedFields.Contains(property.Name))
                throw new ArgumentException($"Sort field not allowed: {sort.Field}", nameof(sorts));

            // LINQ to Objects: property values are obtained in memory.
            Func<T, object?> keySelector = item => property.GetValue(item);
            ordered = (ordered, sort.Direction) switch
            {
                (null, SortDirection.Asc) => source.OrderBy(keySelector),
                (null, SortDirection.Desc) => source.OrderByDescending(keySelector),
                (not null, SortDirection.Asc) => ordered.ThenBy(keySelector),
                (not null, SortDirection.Desc) => ordered.ThenByDescending(keySelector),
                _ => throw new ArgumentOutOfRangeException(nameof(sorts))
            };
        }

        return ordered ?? source;
    }

    public static IEnumerable<T> ApplyPagination<T>(
        this IEnumerable<T> source, int page, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(pageSize));
        return source.Skip(checked((page - 1) * pageSize)).Take(pageSize);
    }
}
