# Pagination et tri dynamique sur IEnumerable<T>

Exemple **LINQ to Objects**, autonome en .NET 10, sans `AsQueryable()`, sans arbres d'expressions et sans EF Core.

## Exécuter

```bash
dotnet run --project examples/EnumerablePagination/EnumerablePagination.csproj
```

Sortie attendue :

```text
Id=3, Date=2026-10-13, Price=10
Id=2, Date=2026-10-13, Price=30
```

## Payload

```json
{
  "page": 1,
  "pageSize": 2,
  "sort": [
    { "field": "date", "direction": "desc" },
    { "field": "price", "direction": "asc" }
  ]
}
```

`ApplySort` applique `OrderBy` sur le premier champ, puis `ThenBy` pour les suivants. Un critère `Id ASC` est ajouté pour rendre l'ordre déterministe. `ApplyPagination` applique `Skip` et `Take` après le tri.

**Important :** `IEnumerable<T>` exécute le tri et la pagination côté application ; le tri doit examiner les éléments de la séquence avant de produire la page. Si les données viennent d'une base EF Core, privilégier `IQueryable<T>` et appliquer tri/pagination **avant** `ToListAsync()`.

La réflexion (`PropertyInfo.GetValue`) est volontairement simple pour un exemple pédagogique ; pour des volumes importants, privilégier des sélecteurs typés, compilés et mis en cache. Limitez les champs triables et la taille de page dans une API.
