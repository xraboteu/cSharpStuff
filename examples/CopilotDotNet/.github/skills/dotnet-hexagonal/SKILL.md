---
name: dotnet-hexagonal
description: Design, review and implement hexagonal architecture (ports and adapters) in C#/.NET solutions.
---

# Hexagonal architecture for .NET

Use when creating or reviewing domain/application/infrastructure boundaries.

## Workflow
1. Inspect the solution, entry points, dependency direction and existing tests.
2. Identify domain invariants, application use cases, inbound ports and outbound ports.
3. Keep domain independent of EF Core, ASP.NET Core, message brokers and external SDKs.
4. Define interfaces at the boundary that owns the requirement; implement adapters in infrastructure.
5. Wire dependencies in the composition root; avoid service locator patterns.
6. Preserve contracts and add tests for use cases, domain rules and adapters.
7. Build and run targeted tests, reporting failures and remaining risks.

## Guardrails
- Do not create interfaces for every class without a real boundary.
- Avoid an anemic domain model when business rules belong in entities/value objects.
- Never move infrastructure dependencies into domain code.
- Prefer incremental changes over a full rewrite.

## Example layout
```text
src/
  Domain/
  Application/
  Infrastructure/
  Api/
```
This is illustrative; adapt to the existing repository.
