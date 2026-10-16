namespace PaymentCacheFiltering;

public enum PaymentStatus { Pending, Approved, Rejected }
public enum StatusCategory { Initial, Final, Failed }
public enum SortDirection { Asc, Desc }
public sealed record SortCriteria(string Field, SortDirection Direction);
public sealed record Payment(
    int Id, string LegalInstrument, string PaymentInstrument, PaymentStatus ComplianceStatus,
    StatusCategory StatusCategory, string Desk, string Agent, string Country,
    DateTime ExecutionDate, decimal Amount);
public sealed class FilterStuckPaymentDto
{
    public int PageSize { get; init; } = 10;
    public int CurrentPage { get; init; } = 1;
    public List<SortCriteria> Sort { get; init; } = [];
    public string[]? Instruments { get; init; }
    public string[]? LocalInstruments { get; init; }
    public PaymentStatus[]? Statuses { get; init; }
    public string[]? Desks { get; init; }
    public StatusCategory[]? Categories { get; init; }
    public string[]? Agents { get; init; }
    public string[]? Countries { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public decimal? AmountFrom { get; init; }
    public decimal? AmountTo { get; init; }
    public bool? ShowAllPayments { get; init; }
}
public sealed record PagedPayments(IReadOnlyList<Payment> Payments, int TotalCount);
public interface IPaymentCacheManager { IReadOnlyList<Payment> GetPayments(); }
public sealed class PaymentQueryService(IPaymentCacheManager cache)
{
    public PagedPayments Search(FilterStuckPaymentDto filter)
    {
        IEnumerable<Payment> query = cache.GetPayments();
        if (filter.ShowAllPayments == false)
        {
            if (filter.Instruments is { Length: > 0 })
                query = query.Where(p => filter.Instruments.Contains(p.PaymentInstrument));
            if (filter.LocalInstruments is { Length: > 0 })
                query = query.Where(p => filter.LocalInstruments.Contains(p.LegalInstrument));
            if (filter.Statuses is { Length: > 0 })
                query = query.Where(p => filter.Statuses.Contains(p.ComplianceStatus));
            if (filter.Desks is { Length: > 0 })
                query = query.Where(p => filter.Desks.Contains(p.Desk));
            if (filter.Categories is { Length: > 0 })
                query = query.Where(p => filter.Categories.Contains(p.StatusCategory));
            if (filter.Agents is { Length: > 0 })
                query = query.Where(p => filter.Agents.Contains(p.Agent));
            if (filter.Countries is { Length: > 0 })
                query = query.Where(p => filter.Countries.Contains(p.Country));
            if (filter.DateFrom is { } from)
                query = query.Where(p => p.ExecutionDate >= from);
            if (filter.DateTo is { } to)
                query = query.Where(p => p.ExecutionDate <= to);
            if (filter.AmountFrom is { } min)
                query = query.Where(p => p.Amount >= min);
            if (filter.AmountTo is { } max)
                query = query.Where(p => p.Amount <= max);
        }

        var total = query.Count();
        // Illustrative typed sort. The existing application ApplySort extension is unchanged.
        IOrderedEnumerable<Payment>? ordered = null;
        foreach (var sort in filter.Sort)
        {
            ordered = (sort.Field.ToLowerInvariant(), sort.Direction, ordered) switch
            {
                ("amount", SortDirection.Asc, null) => query.OrderBy(p => p.Amount),
                ("amount", SortDirection.Desc, null) => query.OrderByDescending(p => p.Amount),
                ("amount", SortDirection.Asc, not null) => ordered.ThenBy(p => p.Amount),
                ("amount", SortDirection.Desc, not null) => ordered.ThenByDescending(p => p.Amount),
                ("desk", SortDirection.Asc, null) => query.OrderBy(p => p.Desk),
                ("desk", SortDirection.Desc, null) => query.OrderByDescending(p => p.Desk),
                ("desk", SortDirection.Asc, not null) => ordered.ThenBy(p => p.Desk),
                ("desk", SortDirection.Desc, not null) => ordered.ThenByDescending(p => p.Desk),
                ("executiondate", SortDirection.Asc, null) => query.OrderBy(p => p.ExecutionDate),
                ("executiondate", SortDirection.Desc, null) => query.OrderByDescending(p => p.ExecutionDate),
                ("executiondate", SortDirection.Asc, not null) => ordered.ThenBy(p => p.ExecutionDate),
                ("executiondate", SortDirection.Desc, not null) => ordered.ThenByDescending(p => p.ExecutionDate),
                _ => throw new ArgumentException($"Unsupported sort: {sort.Field}")
            };
        }
        var page = Math.Max(1, filter.CurrentPage);
        var size = Math.Clamp(filter.PageSize, 1, 200);
        var payments = (ordered ?? query.OrderBy(p => p.Id))
            .Skip((page - 1) * size).Take(size).ToArray();
        return new PagedPayments(payments, total);
    }
}
