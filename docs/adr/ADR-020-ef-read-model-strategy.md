# ADR-020 — EF Core Read Model Strategy

**Status:** Accepted · **Date:** 2026-07-20 (retro-documented) ·
**Related:** [ADR-006](ADR-006-read-model-composition.md) (read model composition),
[ADR-016](ADR-016-persistence-access-model.md) (repositories for writes, DbContext for reads)

> **Retro-documented.** ADR-006 and ADR-016 say reads project from `DbContext`
> rather than through aggregates. Neither records *why projecting from the write
> model fails*, which is an EF Core constraint discovered at runtime and
> rediscovered on every list screen since.

## Context

Aggregates persist value objects and strongly typed ids through EF value
converters: `Email`, `ProductCode`, `ProductName`, `UserId`, `ProductId`.

Converters work for materialisation. They do **not** work inside predicates.
Searching the user directory through the write model fails two ways:

```csharp
// InvalidOperationException — cannot be translated to SQL
.Where(x => x.Email.Value.Contains(term))

// InvalidCastException: Invalid cast from 'System.String' to 'Email'
// — the converter is applied to the parameter as well
.Where(x => EF.Property<string>(x, "Email").Contains(term))
```

Neither failure is visible at compile time. Both appear at runtime, on the
screen that needed them.

## Decision

**Any read that filters, searches, sorts or pages uses a dedicated keyless read
model of primitives, mapped over the existing table with `ToView`.**

```csharp
public sealed class ProductDirectoryRow          // primitives only
{
    public Guid Id { get; init; }
    public Guid OrganizationId { get; init; }
    public string Code { get; init; } = default!;
    // ...
}

builder.HasNoKey();
builder.ToView("Products");                       // owns no schema
builder.Property(x => x.Type).HasConversion<string>();
```

`ToView` is the load-bearing part: EF treats the entity as read-only and
**excludes it from migrations**, so the read model produces zero schema drift.
It reads the table its aggregate owns and owns nothing itself.

The row must convert enums the way the aggregate's configuration does —
`UserDirectoryRow` uses `int` because `Users` stores ints, `ProductDirectoryRow`
uses `string` because `Products` stores strings. A mismatch fails at runtime.

### Scope

| Read | Approach |
|---|---|
| Filter, search, sort or page | Keyless read model + `ToView` |
| Single record by primary key | Read model, for consistency — `GetUserById`, `GetProduct` |
| Load for a write | Aggregate via repository ([ADR-016](ADR-016-persistence-access-model.md)) |

The middle row is a deliberate simplification: fetching by id through the
aggregate would work, but having one read path per context is worth more than
saving a class.

## Consequences

- Every list screen needs a read model. That is a real cost per capability,
  and it is the price of value objects on the write side.
- Read models are pure primitives. Formatting and interpretation belong to the
  DTO or the UI, not the row.
- Zero schema drift: read models never appear in a migration, so adding one is
  not a database change.
- A read model can drift from its table. `ToView` is not validated at startup,
  so a renamed column fails at query time — covered by integration tests that
  run against real Postgres rather than an in-memory provider.
- This constraint is a property of EF Core, not of CQRS. ADR-006 would still
  hold on Dapper or Marten; this ADR would not.
