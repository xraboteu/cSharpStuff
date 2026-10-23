# DTO de réponse paginée

Les deux exemples (`EnumerablePagination` et `DynamicPagination`) exposent le même contrat `PagedResult<T>` : `Items`, `Page`, `PageSize`, `TotalCount`, `TotalPages`, `HasNextPage`, `HasPreviousPage`.

## LINQ to Objects

```csharp
var result = products
    .ApplySort(request.Sort)
    .ToPagedResult(request.Page, request.PageSize);
```

La séquence `IEnumerable<T>` est matérialisée au plus une fois, puis comptée et paginée en mémoire.

## IQueryable

```csharp
var result = query
    .ApplySort(request.Sort)
    .ToPagedResult(request.Page, request.PageSize);
```

Sur un fournisseur EF Core, cette version **synchrone** effectue `Count()` puis `Skip/Take/ToList()` ; privilégier une version async avec `CountAsync()` et `ToListAsync()` dans les APIs serveur.

Le nombre total d'éléments doit être calculé **après les filtres et avant la pagination**. Un ordre déterministe (ex. `Id` en dernier critère) est recommandé pour éviter les résultats instables d'une page à l'autre.

Exemple JSON :

```json
{
  "items": [
    { "id": 3, "date": "2026-10-13", "price": 10 },
    { "id": 2, "date": "2026-10-13", "price": 30 }
  ],
  "page": 1,
  "pageSize": 2,
  "totalCount": 4,
  "totalPages": 2,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```
