using System.Text.Json;
using DynamicPagination;

var json = """
{
  "page": 1,
  "pageSize": 2,
  "sort": [
    { "field": "date", "direction": "desc" },
    { "field": "price", "direction": "asc" }
  ]
}
""";

var request = JsonSerializer.Deserialize<PaginationRequest>(
    json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

var products = new List<Product>
{
    new(1, new DateTime(2026, 10, 12), 50m),
    new(2, new DateTime(2026, 10, 13), 30m),
    new(3, new DateTime(2026, 10, 13), 10m),
    new(4, new DateTime(2026, 10, 11), 15m)
};

var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{ nameof(Product.Date), nameof(Product.Price), nameof(Product.Id) };

// Append a unique key to guarantee deterministic pagination.
var sorts = (request.Sort ?? []).ToList();
if (!sorts.Any(s => s.Field.Equals(nameof(Product.Id), StringComparison.OrdinalIgnoreCase)))
    sorts.Add(new SortCriteria(nameof(Product.Id), SortDirection.Asc));

var result = products.AsQueryable()
    .ApplySort(sorts, allowed)
    .ToPagedResult(request.Page, request.PageSize);

Console.WriteLine($"TotalCount={result.TotalCount}, TotalPages={result.TotalPages}");
foreach (var product in result.Items)
    Console.WriteLine($"Id={product.Id}, Date={product.Date:yyyy-MM-dd}, Price={product.Price}");

// Same extension works with EF Core IQueryable, where expressions translate to SQL.
public sealed record Product(int Id, DateTime Date, decimal Price);
