# ADR-043 — Entity Identity Derives From The Kernel, And Master Data Does Not

**Status:** Accepted · **Date:** 2026-08-01 ·
**Supersedes:** the final consequence of [ADR-017](ADR-017-shared-kernel-scope.md) (the eleven-id migration note) ·
**Related:** [ADR-017](ADR-017-shared-kernel-scope.md) (shared kernel scope),
[ADR-018](ADR-018-rule-of-three.md) (rule of three),
[ES-015](../ENGINEERING_STANDARDS.md) (master data has deterministic identities),
[ES-020](../ENGINEERING_STANDARDS.md) (the standard this records)

## Context

Two identity forms have coexisted since the beginning: `sealed class XId :
StronglyTypedId` and `readonly record struct XId(Guid Value)`. This was never
a decision, and the split is not a fringe: **24 conforming against 27 not**.
The older form is the marginal majority.

ADR-017 recorded the intent to converge, in a single consequence bullet:

> Eleven ids across ProductDocument, RegulatoryApplication and Submission are
> still `readonly record struct` … They migrate one bounded context per story,
> when that context is being worked on anyway.

Every factual claim in it is now wrong. It is 27 ids, not eleven, across seven
contexts, not three — and it never named ReferenceData, which holds the largest
share. More usefully, the policy is falsified rather than merely stale. Over its
whole life it produced **zero migrations and at least four regressions**: every
status-history entry added since took the record-struct form, each copied from
the last, including two written the week this was recorded.

That is the real finding. The rule was never enforced, so "migrate
opportunistically" decayed into "never migrate, and quietly grow the list."

Two things were also wrong with the rule as commonly stated. It was written as
"strongly typed, never a bare `Guid`" — which a record struct satisfies, so the
wording could not flag the thing it meant. And `CLAUDE.md` illustrated it with
`CountryId`, which is one of the 27 violations.

## Decision

### 1. The rule survives, because of the base class, not the id

On their own merits the two id forms are close to a wash. The record struct is
cheaper and allocation-free. Identity equality is rarely relied on, and
`AggregateRoot<TId>` is admitted in ADR-017 to be semantic rather than
functional. Only one benefit is concrete: `StronglyTypedId` rejects `Guid.Empty`
with `DomainException` (400) where a record struct yields a 500 — and even that
could be had by validating inside a struct.

Type safety is **not** a differentiator, though it is often assumed to be. Both
forms declare `public static implicit operator Guid` — 37 declarations across
the two — so a typed id decays to the primitive identically either way. Neither
form is currently buying the safety the name promises.

What decides it is `Entity<TId>` / `AggregateRoot<TId>`. They constrain
`TId : StronglyTypedId`, 24 types already inherit them, and they carry the
aggregate-boundary semantics the domain model rests on. Abandoning the class
form means deleting or weakening those base classes. That is a larger
architectural loss than the migration costs.

So: **every entity identity is a sealed class deriving from `StronglyTypedId`,
and the entity inherits `AggregateRoot<TId>` or `Entity<TId>`.**

### 2. Flat master data is outside the rule — permanently

Eight lookups keep their record struct ids as a decision, not a backlog:
`CountryId`, `DocumentTypeId`, `SubmissionTypeId`, `AuthorityId`,
`AuthorityDivisionId`, `CorrespondenceTypeId`, `ContactRoleId`,
`IdentifierSchemeId`.

These are the ES-015 records: platform-assigned deterministic ids, no child
entities, no lifecycle beyond `Create`, never loaded as an aggregate to be
mutated. None inherits `Entity<TId>` or wants to, so the argument in §1 does not
reach them. Migrating them would be conformity for its own sake — the
"because RIM says so" reasoning this project rejects.

**The carve-out is by shape, not by context.** ReferenceData also holds
Blueprint — `RegulatoryTemplate` is an aggregate root that owns its versions and
is tenant-scoped, and it is the metadata engine the product exists to be.
Excluding "the ReferenceData context" would have blessed the core aggregate as a
lookup table. Blueprint's five ids migrate.

If a lookup later grows children or a lifecycle, it has stopped being master
data, and it migrates.

### 3. Enforcement precedes migration

`IdentityConventionTests` asserts the rule with both lists shrink-only, in the
established pattern: a stale-entry test means an exemption cannot outlive the
thing it excused. The lists in the test are the authority; the table in ES-020
is a summary.

Enforcement comes first deliberately. The evidence above is that recording this
policy without a test is what produced the regressions, so the test is the
decision — the remaining 19 are a consequence of it, not a precondition.

### 4. The migration is per-context, and never partial

The 19 migrate a whole bounded context at a time, when that context is being
worked on anyway. A half-migrated context is worse than either end state: it
teaches both forms at once, which is how this started.

## Consequences

- New entities have one answer, and a test that gives it before review does.
- The exemption lists are visible and shrink-only, so the count can no longer
  grow quietly — the specific failure of the ADR-017 policy.
- Two identity forms remain in the codebase permanently. This is the cost of
  §2, accepted knowingly: the alternative is 8 migrations that buy nothing.
- `HaQuestion` and `CorrespondenceAttachment` have conforming ids but still
  declare `Id` by hand instead of inheriting `Entity<TId>`. The id form is
  enforced; that base-class rule stays review-time for now.
- **Both** id forms carry `public static implicit operator Guid` (37 files),
  which silently decays a typed id to the primitive and undercuts the safety the
  types exist to provide. Removing it was measured, not estimated: stripping all
  37 breaks **19 call sites**, 18 of them in `ReferenceData.Application` and one
  in `Organization.Application`, all of them query handlers projecting an id
  into a read model. That is a floor — the build short-circuits at the first
  failed project, so anything downstream is unmeasured. Small enough to be one
  story; not smuggled in here.

## Revisit When

- **A ninth type is proposed for the master-data exclusion.** One addition is a
  judgement call; a pattern of additions means §2's boundary is drawn wrong and
  the shape test ("no children, no lifecycle") should become explicit.
- **A master-data lookup grows a child collection or a status field.** It has
  left the carve-out and should move to the migration list in the same PR.
- **The pending list has not moved by the end of EPIC-006.** Two epics of no
  movement would say per-context-when-convenient has failed the same way
  opportunistic migration did, and the migration should be scheduled as work
  rather than attached to unrelated stories.
