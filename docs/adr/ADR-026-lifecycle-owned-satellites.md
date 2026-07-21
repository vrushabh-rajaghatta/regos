# ADR-026 — A Satellite Is Defined by Lifecycle Ownership, Not by Its Key

**Status:** Accepted · **Date:** 2026-07-21 ·
**Supersedes:** [ADR-023](ADR-023-satellite-aggregate-lifetime.md) ·
**Related:** [ADR-001](ADR-001-modular-architecture.md) (modular architecture),
[ADR-018](ADR-018-rule-of-three.md) (rule of three),
[ADR-025](ADR-025-sessions-are-server-owned-cookies.md) (sessions)

## Context

ADR-023 allowed a foreign key with cascade delete for a "satellite" aggregate,
and required three conditions. The first was:

> Its primary key *is* the other aggregate's identity — not a separate key that
> happens to carry a reference.

That ADR closed by saying a second candidate is where the pattern gets named
properly. AUTH-006 produced one. `RefreshToken` has the same absolute lifetime
dependency on `User` — it cannot exist before its user, cannot outlive them, and
a row whose user is gone is unreachable by every code path — but its key is its
own `RefreshTokenId`, so the rule as written did not cover it.

Reviewing why the condition was there at all, it turns out to have been doing no
work. `UserCredential`'s key being `UserId` enforces *at most one credential per
user*: that is a statement about **cardinality**. What the foreign key enforces
is *a credential cannot outlive its user*: a statement about **lifetime**. ADR-023
took a property of its single example and wrote it into a rule about something
else.

The two members of the pattern now differ in exactly that irrelevant property:

| | Key | Cardinality | Lifetime dependency |
|---|---|---|---|
| `UserCredential` | `UserId` | 1:1 | absolute |
| `RefreshToken` | own id | 1:many | absolute |

The FK exists to enforce lifetime. Lifetime ownership is therefore the rule.

## Decision

**A satellite aggregate is one whose lifecycle is wholly owned by a principal
aggregate. A satellite may enforce that lifetime with a database foreign key
and cascade delete, whatever its cardinality and whatever its key.**

Three conditions, all required:

1. **Deleting the principal must make retaining the row *meaningless*, not
   merely inconvenient.** This is the load-bearing test and it is deliberately
   demanding. A credential with no user cannot authenticate anyone; a refresh
   token with no user cannot refresh anything. By contrast a `Submission` whose
   `Product` was deleted is *inconvenient* — it still records something that
   happened, and someone might reasonably want it. That is not a satellite.

2. **No independent lifecycle.** It cannot be created before its principal
   exists, and it has no meaning after the principal is gone.

3. **Same module.** A satellite never crosses a module boundary. This is a hard
   limit, not a guideline: a cascade across modules would couple their schemas
   in a way ADR-001 exists to prevent, and no cascade is worth that.

Cardinality is explicitly **not** a condition. Neither is the shape of the key.

Satellites are still declared **without navigation properties on either side**.
They remain separate aggregates with separate repositories and no way to
traverse between them in code; what the foreign key changes is the schema, not
the object model.

### Members today

`UserCredential` and `RefreshToken`, both principals of `User`, both in
Platform. Nothing else in RegOS qualifies, and in particular no cross-aggregate
reference in the regulatory contexts does.

## Consequences

**Positive**

- The rule now states the property it was always enforcing, so the second
  legitimate case stops looking like an exception that needs arguing.
- One-to-many satellites are covered. `RefreshToken` was already relying on the
  behaviour; it is no longer relying on it in contradiction of the written rule.
- The "meaningless, not inconvenient" test is answerable about a specific
  relationship without consulting anyone, which is what a rule has to be.

**Negative**

- **This is a widening, and the honest risk is that it reads as general
  permission for foreign keys.** Under ADR-023 a candidate had to be 1:1, which
  ruled almost everything out mechanically. Now any child table is arguable, and
  the argument has to be made on lifecycle rather than settled by inspection.
  Condition 1 is what holds the line, and it is a judgement rather than a
  syntactic check.
- Cascade deletes remain invisible at the call site. `DELETE FROM "Users"` now
  removes rows from two tables it does not name — which is exactly the property
  that makes it useful and exactly the property that surprises someone reading a
  repair script.
- ADR-023's numbering stands and its file remains. Anyone who read it before
  today learned a stricter rule than the one now in force.

## Revisit When

- A satellite is proposed across a module boundary. Condition 3 forbids it
  today; the request itself would be evidence worth weighing.
- Soft deletion arrives for a principal. A cascade fires on physical delete
  only, so a soft-deleted user would keep live satellites and this decision
  would stop providing the guarantee it was adopted for. That would be a real
  supersession, not an amendment.
- A third member joins and turns out to share a property these two do not, which
  would suggest the rule is still describing its examples.
