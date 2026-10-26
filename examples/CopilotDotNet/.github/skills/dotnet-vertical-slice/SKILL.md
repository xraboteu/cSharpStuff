---
name: dotnet-vertical-slice
description: Design and implement .NET features as vertical slices, grouping endpoint, validation, application logic and tests by feature.
---

# Vertical Slice Architecture

Use when adding features or evaluating whether to organize a .NET application by feature.

## Workflow
1. Identify one user-visible capability and its request/response contract.
2. Group feature-specific code in a dedicated folder, e.g. `Features/Products/Create/`.
3. Keep domain invariants in reusable domain types, not duplicated handlers.
4. Keep persistence and external dependencies behind appropriate boundaries when useful.
5. Prefer minimal APIs or the repository's existing HTTP style; do not introduce MediatR/Carter without a reason.
6. Add tests for success, validation, authorization and failure paths.
7. Verify endpoint contracts and behavior.

## Example
```text
Features/
  Products/
    Create/
      Endpoint.cs
      Request.cs
      Handler.cs
      Validator.cs
    GetById/
      Endpoint.cs
      Response.cs
```

## Guardrails
- A slice is a feature boundary, not permission to bypass domain invariants.
- Share stable cross-cutting capabilities, not generic abstractions prematurely.
- Avoid cross-slice direct coupling; use well-defined contracts.
