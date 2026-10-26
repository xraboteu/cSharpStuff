# GitHub Copilot — Skills et agents pour .NET

Exemples réutilisables pour les projets C#/.NET.

## Contenu

- `.github/skills/dotnet-hexagonal/SKILL.md` : architecture hexagonale.
- `.github/skills/dotnet-vertical-slice/SKILL.md` : architecture Vertical Slice.
- `.github/skills/dotnet-performance/SKILL.md` : optimisation et benchmarks.
- `.github/skills/dotnet-testing/SKILL.md` : tests unitaires et intégration.
- `.github/agents/architecture-migrator.agent.md` : migration incrémentale vers Vertical Slice.
- `.github/agents/performance-optimizer.agent.md` : optimisation mesurée.
- `.github/agents/test-engineer.agent.md` : génération de tests.
- `.github/copilot-instructions.md` : conventions transversales.

## Installation dans un projet

Copiez le contenu du répertoire `.github/` de cet exemple dans le `.github/` de votre dépôt cible (fusionnez les instructions existantes au lieu de les écraser). Les fichiers restent sous `examples/CopilotDotNet` dans ce dépôt afin de **ne pas activer ces agents pour cSharpStuff lui-même**.

## Exemples de demandes

- « Utilise le skill dotnet-hexagonal pour examiner les dépendances du projet. »
- « Implémente la fonctionnalité CreateOrder selon dotnet-vertical-slice. »
- « Avec architecture-migrator, propose puis migre la fonctionnalité Orders en slices, sans changer les endpoints. »
- « Avec performance-optimizer, mesure les allocations et améliore la requête EF Core. »
- « Avec test-engineer, écris les tests de ce use case, y compris les cas limites. »

Les noms de tools disponibles pour les agents peuvent varier selon la version de VS Code / GitHub Copilot ; ajustez la liste si nécessaire. Les agents doivent disposer des autorisations adéquates pour éditer et exécuter les tests. Vérifiez les capacités de votre environnement avant usage.
