# ADR-023 — A Satellite Aggregate's Lifetime Is Enforced by the Database

**Status:** Superseded by
[ADR-026](ADR-026-lifecycle-owned-satellites.md) ·
**Date:** 2026-07-21 · **Superseded:** 2026-07-21 ·
**Related:** [ADR-016](ADR-016-persistence-access-model.md) (persistence access
model), [ADR-001](ADR-001-modular-architecture.md) (modular architecture),
[ADR-019](ADR-019-testing-strategy.md) (testing strategy)

> **The rule below is narrower than the one in force.** It required a
> satellite's primary key to *be* the principal's identity. That condition
> described this ADR's single example rather than the principle behind it — the
> key expresses cardinality, while the foreign key enforces lifetime.
> [ADR-026](ADR-026-lifecycle-owned-satellites.md) restates the rule around
> lifecycle ownership and covers one-to-many satellites such as `RefreshToken`.
>
> The decision recorded here — that `UserCredentials.UserId` has a foreign key
> with cascade delete — still holds and is unchanged. Retained as history; cite
> ADR-026 for guidance.

## Context

`UserCredential` is keyed by `UserId`, which makes *at most one credential per
user* a property of the type. It did not make *at least one user per credential*
anything at all: there was no foreign key from `UserCredentials.UserId` to
`Users.Id`, so deleting a user silently left the credential behind.

This was found the way such things usually are. `LoginHandlerTests` created
several users and its cleanup deleted only one credential, leaving four
orphaned rows — a direct violation of testing Principle 7, written two slices
earlier. The fixture was the bug, but the fixture could only have that bug
because the schema permitted the state.

The absence of the constraint was not a decision either. RegOS aggregates
reference each other by bare typed ID and no cross-aggregate foreign keys exist
anywhere, so `UserCredential` simply inherited the house style. That style is
correct for the references it was formed around:

```
Submission ──▶ Product          a product outlives the submissions citing it
Application ──▶ Organization    an organization outlives its applications
```

Both sides have independent lifecycles, and a foreign key there would couple
modules that ADR-001 keeps separable.

`User ↔ UserCredential` is not that shape. The credential has no identity, no
meaning and no reachable code path without its user. It is not a peer that
happens to point at a user; it is part of the user's existence, split into its
own aggregate so that a password hash is never loaded by a directory query.

Three options were considered:

1. **Discipline.** Require every caller that deletes a user to delete the
   credential. This is what already existed, and it had already failed once.
2. **A domain service or handler that deletes both.** Correct where it is used,
   but it constrains only the paths that go through it. Nothing prevents a
   migration, a repair script or a future handler from deleting a user directly.
3. **A foreign key with cascade delete.** The database refuses to hold the
   invalid state at all, regardless of which code path or which human causes it.

## Decision

**A satellite aggregate — one whose identity is entirely derived from another
aggregate, and which is meaningless without it — may enforce its lifetime with a
database foreign key and cascade delete.**

`UserCredentials.UserId` now has an FK to `Users.Id` with
`ON DELETE CASCADE`.

The constraint is declared **without navigation properties on either side**:

```csharp
builder.HasOne<UserAggregate>()
    .WithOne()
    .HasForeignKey<UserCredentialAggregate>(x => x.Id)
    .OnDelete(DeleteBehavior.Cascade);
```

The two remain separate aggregates, loaded and saved through separate
repositories, with no way to traverse from one to the other in code. What
changes is the schema, not the object model.

An aggregate qualifies as a satellite only when **all** of these hold:

- Its primary key *is* the other aggregate's identity — not a separate key that
  happens to carry a reference.
- It has no lifecycle of its own: it cannot be created before, or survive, its
  principal.
- It lives in the same module, so the constraint crosses no module boundary.

`UserCredential` is the only type in RegOS that qualifies today.

## Consequences

**Positive**

- An orphaned credential is now impossible, not merely discouraged. The class of
  bug that produced this ADR cannot recur through any code path.
- Deleting a user is a complete operation again. Callers do not need to know
  that a satellite exists.
- Test cleanup gets simpler: deleting users is sufficient. The explicit
  credential deletes in the two Platform fixtures are now redundant.

**Negative**

- This is the first cross-table foreign key in RegOS, so the codebase no longer
  answers *"do we use FKs?"* with a flat no. The qualifying conditions above
  exist to keep the answer narrow; the risk is that they get read as
  encouragement rather than as a boundary.
- A cascade delete is invisible at the call site. `DELETE FROM "Users"` now
  removes rows from a table it does not name, which is exactly the property that
  makes it useful and exactly the property that surprises people reading a
  repair script.
- Existing databases holding orphaned credentials cannot take the constraint
  until they are cleared. The migration deletes them, which is safe only because
  an orphaned credential is unreachable by definition — a duplicate email, by
  contrast, required a human decision about which user was real.

## Revisit When

- A second type is proposed as a satellite. Two is where a pattern should be
  named; one is a case.
- A satellite is needed across a module boundary, which the qualifying
  conditions currently forbid and which would put this ADR in tension with
  ADR-001.
- Soft deletion arrives for users. A cascade fires on physical delete only, so
  a soft-deleted user would keep a live credential and this decision would stop
  providing the guarantee it was adopted for.
