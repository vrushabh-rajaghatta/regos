# Engineering Standards

These standards apply across the entire RegOS platform.

> **File and folder layout is not here.** Where a file goes and what it is
> called is specified by
> [engineering/slice-conventions.md](engineering/slice-conventions.md) and
> enforced by `tests/Architecture/RegOS.Architecture.Tests`. This document
> covers cross-cutting platform standards; that one covers the shape of a
> vertical slice.

---

## ES-001

Feature-first frontend architecture.

---

## ES-002

One bounded context per capability.

---

## ES-003

Use business terminology.

Example

Product

RegulatoryApplication

Submission

---

## ES-004

Aggregate factories use

Create()

instead of Register().

---

## ES-005

Repositories are write-only abstractions.

---

## ES-006

Cross-capability reads use Readers.

Never repositories.

---

## ES-007

Capabilities expose Contracts.

Implementation stays internal.

---

## ES-008

Aggregates never cross capability boundaries.

Readers return immutable models.

---

## ES-009

Every Aggregate owns its invariants.

Application Layer orchestrates use cases.

---

## ES-010

Stay Within the Sprint.

Architecture is introduced only when required by the current sprint.

---

## ES-011

Every sprint must end with a demonstrable feature.

---

## ES-013

Pin the .NET SDK using `global.json`.

All developers and CI environments should build RegOS using the same SDK version to ensure reproducible builds.

---

## ES-014 — Prefer Identity References Across Aggregates

> Aggregates reference other aggregates by their strongly typed identifiers (e.g., `CountryId`, `ProductId`, `AuthorityId`) rather than navigation properties. Object relationships are materialized in the persistence layer, while the domain model remains persistence-agnostic and focused on business invariants.

---

## ES-015 — Master Data Records Use Deterministic Identities

> Master Data (Countries, Authorities, Languages, Dosage Forms, etc.) are assigned stable identifiers defined by the platform. Transactional aggregates (Products, Applications, Submissions) continue to generate identities within the domain. This gives us two distinct identity strategies, each appropriate for its purpose.

---

## ES-016 — Platform Data Is Initialized via IDataInitializer

> Platform-owned data is initialized by `IDataInitializer` implementations during application startup. Today it is Master Data; tomorrow it may be default roles, workflow templates, document types, or regulatory taxonomies. Rather than inventing a new bootstrapping mechanism each time, we extend the initialization pipeline. Implementations must be idempotent.

---

## ES-018 — Prefer Lifecycle State Transitions Over Deletion

> Long-lived business entities (Products, Organizations, Applications, Submissions) should generally move through lifecycle states (e.g. Active ↔ Inactive) rather than being physically deleted. This preserves history, supports auditability, and aligns with regulatory systems, where records are typically retained even when they are no longer active.

---

## ES-019 — Initializers Are Additive and Idempotent

> Every `IDataInitializer` has a single responsibility: ensure its capability has the minimum required platform-owned data. Initializers must be additive and idempotent — never deleting or overwriting existing customer data, and safe to run on every application startup. As RegOS grows, startup remains a simple loop over registered initializers, and each capability bootstraps itself independently without special orchestration logic.

---

## ES-020 — Entity Identity Derives From `StronglyTypedId`

> Every entity and aggregate identity is a `sealed class <X>Id : StronglyTypedId`
> ([SharedKernel/Primitives/StronglyTypedId.cs](../src/Shared/RegOS.SharedKernel/Primitives/StronglyTypedId.cs)).
> The older `readonly record struct <X>Id(Guid Value)` form is **closed to new
> code** and shrink-only in existing code. This applies to child entities and
> status-history entries exactly as it applies to aggregate roots.

### The two forms are not interchangeable

"Strongly typed" is not the standard — deriving from the kernel type is. A
`record struct` is strongly typed and still fails the standard, which is why
this needed writing down: the weaker phrasing let the second form spread.

[`Entity<TId>`](../src/Shared/RegOS.SharedKernel/Abstractions/Entity.cs)
constrains `where TId : StronglyTypedId`. A record struct cannot satisfy it, so
an entity keyed that way **cannot inherit `Entity<TId>` or `AggregateRoot<TId>`
at all**. It silently loses:

- **Identity equality** — two instances of the same entity with the same id are
  not equal, because the class has no base type supplying `Equals`.
- **The empty-guid guard** — `StronglyTypedId` rejects `Guid.Empty` with
  `DomainException` (400). A record struct accepts it and the request surfaces
  as a **500** further down. [ADR-017](adr/ADR-017-shared-kernel-scope.md) names
  this defect specifically.
- **The aggregate-boundary marker** — no `AggregateRoot<TId>`, so the
  consistency boundary is undeclared.

### Correct by construction

Every conforming id in the solution has exactly these four members. Copy them.

```csharp
// <Aggregate>Id.cs
using RegOS.SharedKernel.Primitives;

public sealed class CommitmentId : StronglyTypedId
{
    public CommitmentId(Guid value) : base(value)
    {
    }

    public static CommitmentId New() => new(Guid.NewGuid());

    public static CommitmentId From(Guid value) => new(value);

    public static implicit operator Guid(CommitmentId id) => id.Value;
}

// <Aggregate>.cs — inheriting the base class is the point, not just the id type
public sealed class Commitment : AggregateRoot<CommitmentId> { … }

// child entity or status-history entry
public sealed class CommitmentStatusEntry : Entity<CommitmentStatusEntryId> { … }
```

Reference: [Commitment.cs](../src/Interaction/RegOS.Interaction.Domain/Commitments/Commitment.cs)
and [CommitmentId.cs](../src/Interaction/RegOS.Interaction.Domain/Commitments/CommitmentId.cs).

### Where a new entity most often gets this wrong

The **status-history entry**. Six exist — `RegistrationStatusEntry`,
`MarketStatusEntry`, `CommitmentStatusEntry`, `InspectionStatusEntry`,
`HaMeetingStatusEntry`, `HaQuestionStatusEntry` — spread across four contexts,
and **every one** uses a record struct id, because each was copied from the
previous one. Two were added after [ADR-017](adr/ADR-017-shared-kernel-scope.md)
recorded the migration policy.

When adding a status entry to a new aggregate, take the id from an aggregate
root, not from the nearest status entry. This is the concrete case
[CLAUDE.md](../CLAUDE.md) means by "do not infer conventions by copying the
nearest file".

### Scope — master data is deliberately outside this rule

Eight flat master-data lookups keep their record struct ids, permanently and by
decision, not as a backlog: `CountryId`, `DocumentTypeId`, `SubmissionTypeId`,
`AuthorityId`, `AuthorityDivisionId`, `CorrespondenceTypeId`, `ContactRoleId`,
`IdentifierSchemeId`.

These are the ES-015 records — platform-assigned deterministic ids, no child
entities, no lifecycle beyond `Create`, never loaded as an aggregate to be
mutated. Nothing here inherits `Entity<TId>` or wants to, so the argument that
justifies the rule does not reach them. See
[ADR-043](adr/ADR-043-entity-identity-derives-from-the-kernel.md).

The carve-out is **not** "the ReferenceData context". `RegulatoryTemplate` is an
aggregate root that owns its versions and is tenant-scoped — the metadata engine
this product is built on. It and the rest of Blueprint migrate like anything
else.

### Migration ledger — shrink-only

19 identities predate this standard and are still to migrate, as of 2026-08-01.

| Context | Pending | Scope |
|---|---:|---|
| ReferenceData · Blueprint | 5 | `RegulatoryTemplate` + versions, sections, required docs, rules |
| Submission | 4 | entire context |
| Registration | 2 | entire context |
| ProductDocument | 2 | entire context |
| RegulatoryApplication | 1 | entire context |
| Interaction | 4 | status entries only — aggregates conform |
| Product | 1 | `MarketStatusEntry` only — aggregates conform |

These migrate **a whole bounded context at a time, when that context is being
worked on anyway** — never as a standalone refactor, and never half a context.
A conversion is mechanical but wide: the `ProductId` migration touched 67 files.

Two entities have a conforming id but still declare `Id` by hand instead of
inheriting `Entity<TId>` —
[HaQuestion.cs](../src/Interaction/RegOS.Interaction.Domain/Correspondence/HaQuestion.cs)
and [CorrespondenceAttachment.cs](../src/Interaction/RegOS.Interaction.Domain/Correspondence/CorrespondenceAttachment.cs).
The id form is enforced; this base-class rule is still review-time.

> **Enforced by** `IdentityConventionTests` in
> [tests/Architecture/RegOS.Architecture.Tests](../tests/Architecture/RegOS.Architecture.Tests/IdentityConventionTests.cs).
> Both lists above are asserted to hold no stale entries, so an exemption cannot
> outlive the thing it excused. The lists there are the authority; this table is
> a summary.

---

## ES-021 — A Persistence Refactor Proves Neutrality With EF's Model Differ

> A refactor that changes only EF configuration is behaviour-neutral **only if
> `dotnet ef migrations add` against the refactored model produces an empty `Up`
> and `Down`**, and regenerates `RegOSDbContextModelSnapshot.cs` unchanged.
> Green tests are supporting evidence, not the primary proof.

### Why tests are the weaker evidence

Tests can only demonstrate the behaviours they exercise. A configuration change
that alters a column type, drops an index, loosens a foreign key or renames a
table will pass every test that never happens to touch that column — and then
surface as a silent schema drift, or as a migration somebody else generates
later and cannot explain.

The model differ has no such gap. It compares **every** table, column, key,
index and conversion in the model EF would persist, against the snapshot of what
it persisted before. An empty diff is EF itself stating that nothing it knows
how to store has changed.

### The procedure

```bash
dotnet ef migrations add __VerifyNoModelChange \
  --project src/Persistence/RegOS.Persistence \
  --startup-project src/Host/RegOS.Api
# Up and Down must both be empty. Then delete both scaffolded files.
git status --short src/Persistence/RegOS.Persistence/Migrations/   # must be clean
```

The second check is not redundant. `migrations add` rewrites the model snapshot
as a side effect; if the snapshot comes back byte-identical, the model is
identical for reasons independent of how the differ chose to describe it.

### What it does not prove

**Query behaviour is outside the model diff.** `AutoInclude()`, query filters,
split-query settings and `PropertyAccessMode` do not appear in a migration, so
an empty diff says nothing about them. Those need a test that actually loads the
data — and if the only such coverage is a browser spec, say so rather than
letting the empty migration imply more than it shows.

> **First applied** by the EPIC-004 status-history configuration extraction
> ([ADR-046](adr/ADR-046-a-submissions-lifecycle-is-only-what-we-did.md)
> decision 6), which moved five owned-history mappings and had to demonstrate it
> moved nothing else.
