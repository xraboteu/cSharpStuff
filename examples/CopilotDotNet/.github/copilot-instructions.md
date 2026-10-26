# .NET Copilot conventions

- Target the framework version already used by the solution; do not upgrade implicitly.
- Preserve observable behavior and public contracts unless the task explicitly requires a change.
- Prefer simple, maintainable C# and dependency injection at composition roots.
- Separate domain rules from infrastructure and transport concerns.
- Respect existing conventions unless a migration plan explicitly changes them.
- Add or update tests for every behavior change.
- Run relevant build, tests and analyzers; report commands and outcomes honestly.
- Never claim performance gains without measurements.
- Avoid committing secrets, connection strings or generated artifacts.
- Read the relevant skill before making architecture, performance or testing changes.
