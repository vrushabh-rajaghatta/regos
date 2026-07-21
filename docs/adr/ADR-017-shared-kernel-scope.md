# ADR-017 — Shared Kernel Scope

**Status:** Accepted · **Date:** 2026-07-20 (retro-documented) ·
**Related:** [ADR-012](ADR-012-shared-semantic-exception-model.md) (exceptions),
[ADR-013](ADR-013-ambient-tenant-context.md) (tenant context),
[ADR-018](ADR-018-rule-of-three.md) (rule of three)

> **Retro-documented.** `RegOS.SharedKernel` exists and every bounded context
> depends on it. What belongs in it — and what deliberately does not — was
> decided story by story and never written down.

## Context

Five bounded contexts need the same handful of building blocks: identity,
equality, and a vocabulary for failure. Without a shared home each context
reinvents them, and `BusinessRuleViolationException` existed in four namespaces
with no common base before this was settled.

The risk in the other direction is larger. A shared kernel that accumulates
"things that might be useful" becomes a framework every context is coupled to,
and coupling in the kernel is the most expensive kind: it cannot be refactored
by one team in one context.

## Decision

`RegOS.SharedKernel` contains exactly:

| Type | Purpose |
|---|---|
| `StronglyTypedId` | Identity: value equality, hashing, and type-safety between id types |
| `Entity<TId>` | Identity-based equality for entities |
| `AggregateRoot<TId>` | Marks an aggregate boundary. Intentionally empty |
| `ValueObject` | Structural equality from declared components |
| `DomainException` | The request itself is invalid (400) |
| `BusinessRuleViolationException` | Valid request, business state forbids it (409) |
| `NotFoundException` | Absent, or invisible to this caller (404) |
| `ITenantContext` | Who is asking |
| `TenantId` | The tenant's identity — the kernel's one concrete id (ADR-030) |

Two rules govern additions:

1. **It must be a concept, not a pattern.** Several contexts paging their data
   is a repeated *pattern*; that is duplication of code, not of meaning, and
   belongs in each application layer. `PagedResult<T>` is therefore duplicated
   in `Platform.Application` and `Product.Application` rather than shared — see
   [ADR-018](ADR-018-rule-of-three.md).

2. **It must not know about a bounded context.** `ITenantContext` originally
   exposed a `Guid` rather than an `OrganizationId` precisely because
   `OrganizationId` belongs to the Organization context, with each context
   converting at its own boundary. [ADR-030](ADR-030-tenant-is-its-own-aggregate.md)
   resolved this differently: the tenant is an infrastructure concept every
   context shares, so `TenantId` became the kernel's one concrete id and the
   conversion seam was deleted. The rule stands — the kernel still knows no
   bounded context; the tenant simply stopped being one context's concept.

### Kernel types obey the architecture they define

The kernel is not exempt from its own rules. `StronglyTypedId` rejected an empty
guid with `ArgumentException`, so an all-zero id in any route returned **500**
across the entire solution — the shared kernel violating the exception contract
it exists to define. It now raises `DomainException` (400).

Anything the kernel enforces, it must enforce on itself first.

## What is deliberately absent

Each of these was considered and rejected for lack of a demonstrated need:

- **Domain events** and any dispatcher
- **`IUnitOfWork`** — one `DbContext` per request already is the unit of work.
  Product had one; it was deleted when nothing was found to compose several
  repositories into a single commit
- **Generic repository**, **specifications**, **base CRUD services**
- **Mediator / pipeline behaviours** — handlers are invoked directly
- **`Result<T>`** and other monadic wrappers — failures are exceptions
  ([ADR-012](ADR-012-shared-semantic-exception-model.md))
- **AutoMapper** — projections are written by hand, in the query that needs them
- **`PagedResult<T>`**, **validators**, and other transport concerns

The list matters as much as the contents. Everything on it is a thing the kernel
could plausibly hold and does not.

## Consequences

- A new bounded context inherits identity, equality and failure semantics
  without inheriting a framework.
- Adding to the kernel requires an argument that the thing is a shared
  *concept*, which is a higher bar than "two contexts do this".
- A change to a kernel type ripples everywhere: migrating `ProductId` from
  `record struct` to `StronglyTypedId` touched 67 files. That cost is the reason
  the contents are small, and the reason additions are rare.
- Eleven ids across ProductDocument, RegulatoryApplication and Submission are
  still `readonly record struct` and do not use the kernel. They carry the same
  500-on-empty-guid defect that `StronglyTypedId` now prevents. They migrate one
  bounded context per story, when that context is being worked on anyway.
