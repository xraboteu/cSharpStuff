# Pagination et tri dynamique multi-colonnes

Exemple autonome en **.NET 10** : tri sur `Date DESC`, puis `Price ASC`, puis `Id ASC` pour stabiliser la pagination.

## Exécuter

```bash
dotnet run --project examples/DynamicPagination/DynamicPagination.csproj
```

Résultat attendu (page 1, taille 2) : produits **3**, puis **2**.

## Payload

```json
{
  "page": 1,
  "pageSize": 20,
  "sort": [
    { "field": "date", "direction": "desc" },
    { "field": "price", "direction": "asc" }
  ]
}
```

## Utilisation avec EF Core

```csharp
var query = dbContext.Products.AsNoTracking();
var sorts = (request.Sort ?? []).ToList();
if (!sorts.Any(s => s.Field.Equals("Id", StringComparison.OrdinalIgnoreCase)))
    sorts.Add(new SortCriteria("Id", SortDirection.Asc));
var result = await query
    .ApplySort(sorts, new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    { "Date", "Price", "Id" })
    .ApplyPagination(request.Page, request.PageSize)
    .ToListAsync();
```

Avec `List<T>.AsQueryable()`, les opérations sont réalisées en mémoire ; avec EF Core, elles sont traduites en SQL. Le projet autonome n'exige aucun package EF Core.

Pour une API publique, conservez une liste blanche de propriétés triables, limitez `PageSize` et utilisez un tri final unique. La validation des champs ne remplace pas la validation du payload HTTP.
