using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;

namespace DynamicPagination;

public sealed record PaginationRequest(int Page = 1, int PageSize = 20, List<SortCriteria>? Sort = null);
public sealed record SortCriteria(string Field, SortDirection Direction);

[JsonConverter(typeof(JsonStringEnumConverter<SortDirection>))]
public enum SortDirection { Asc, Desc }

public static class PaginationExtensions
{
    // In a public API, restrict fields to an explicit allow-list.
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> source,
        IEnumerable<SortCriteria>? sorts,
        IReadOnlySet<string>? allowedFields = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (sorts is null) return source;

        var first = true;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var sort in sorts)
        {
            if (sort is null || string.IsNullOrWhiteSpace(sort.Field))
                throw new ArgumentException("Sort field is required.", nameof(sorts));
            if (!Enum.IsDefined(sort.Direction))
                throw new ArgumentOutOfRangeException(nameof(sorts), "Invalid sort direction.");
            if (!seen.Add(sort.Field))
                throw new ArgumentException($"Duplicate sort field: {sort.Field}", nameof(sorts));

            var property = typeof(T).GetProperty(sort.Field,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property is null || property.GetIndexParameters().Length != 0)
                throw new ArgumentException($"Unknown sort field: {sort.Field}", nameof(sorts));
            if (allowedFields is not null &&
                !allowedFields.Contains(property.Name))
                throw new ArgumentException($"Sort field not allowed: {sort.Field}", nameof(sorts));

            var parameter = Expression.Parameter(typeof(T), "x");
            var selector = Expression.Lambda(Expression.Property(parameter, property), parameter);
            var method = (first, sort.Direction) switch
            {
                (true, SortDirection.Asc) => nameof(Queryable.OrderBy),
                (true, SortDirection.Desc) => nameof(Queryable.OrderByDescending),
                (false, SortDirection.Asc) => nameof(Queryable.ThenBy),
                (false, SortDirection.Desc) => nameof(Queryable.ThenByDescending),
                _ => throw new ArgumentOutOfRangeException(nameof(sorts))
            };
            var call = Expression.Call(typeof(Queryable), method,
                new[] { typeof(T), property.PropertyType }, source.Expression,
                Expression.Quote(selector));
            source = source.Provider.CreateQuery<T>(call);
            first = false;
        }
        return source;
    }

    public static IQueryable<T> ApplyPagination<T>(
        this IQueryable<T> source, int page, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(pageSize));
        var skip = checked((page - 1) * pageSize);
        return source.Skip(skip).Take(pageSize);
    }
}
