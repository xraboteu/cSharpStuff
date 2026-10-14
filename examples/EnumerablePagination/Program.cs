using System.Text.Json;
using EnumerablePagination;

var payload = """
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
    payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

IEnumerable<Product> products = new List<Product>
{
    new(1, new DateTime(2026, 10, 12), 50m),
    new(2, new DateTime(2026, 10, 13), 30m),
    new(3, new DateTime(2026, 10, 13), 10m),
    new(4, new DateTime(2026, 10, 11), 15m)
};

var allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{ nameof(Product.Date), nameof(Product.Price), nameof(Product.Id) };

var sorts = (request.Sort ?? []).ToList();
if (!sorts.Any(s => s.Field.Equals(nameof(Product.Id), StringComparison.OrdinalIgnoreCase)))
    sorts.Add(new SortCriteria(nameof(Product.Id), SortDirection.Asc));

var page = products
    .ApplySort(sorts, allowedFields)
    .ApplyPagination(request.Page, request.PageSize)
    .ToList();

foreach (var product in page)
    Console.WriteLine($"Id={product.Id}, Date={product.Date:yyyy-MM-dd}, Price={product.Price}");

public sealed record Product(int Id, DateTime Date, decimal Price);
