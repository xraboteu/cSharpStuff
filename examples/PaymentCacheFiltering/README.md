# Filtrage en mémoire : cache de paiements

Exemple autonome .NET 8, xUnit et Moq inspiré d'un contrôleur interrogeant un cache hydraté par un job.

- Builder pseudo-aléatoire avec seed pour des fixtures reproductibles.
- Filtres conditionnels via IEnumerable<T>.
- ShowAllPayments = true ou null ignore tous les filtres, mais conserve tri et pagination.
- Tri multicolonne et pagination; aucun Entity Framework.
- Tests sur combinaison des filtres, absence de résultat, tri, pagination et non-mutation du cache.

Ce dossier est un exemple indépendant : il ne prétend pas compiler contre les types privés du projet d'origine et ne modifie pas l'extension ApplySort de celui-ci.

Exécuter : `dotnet test examples/PaymentCacheFiltering/PaymentCacheFiltering.Tests/PaymentCacheFiltering.Tests.csproj`.
