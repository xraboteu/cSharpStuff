---
name: dotnet-testing
description: Write and maintain focused C# unit and integration tests for methods, classes, use cases and features.
---

# .NET testing

## Workflow
1. Inspect the target's behavior, contracts, dependencies and existing test conventions.
2. Build a test matrix: happy path, boundaries, null/invalid inputs, errors and cancellation where relevant.
3. Prefer xUnit when the repository has no established alternative; follow existing frameworks otherwise.
4. Use clear Arrange/Act/Assert and descriptive test names.
5. Test observable behavior, not private implementation details.
6. Prefer fakes or small stubs at external boundaries; avoid excessive mocking.
7. Use Testcontainers for real persistence/infrastructure integration tests when appropriate.
8. Run targeted tests and report failures, coverage gaps and assumptions.

## Guardrails
- Never alter production behavior just to make tests pass without justification.
- Avoid flaky timing-based assertions and shared mutable test state.
- Do not claim coverage percentages without a coverage report.
- Keep unit tests fast and deterministic.
