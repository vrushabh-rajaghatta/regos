# Implementation Standards

---
**Title:** Implementation Standards

**Owner:** Architecture Review Board

**Status:** Approved

**Version:** 1.1

**Effective Date:** 2026-07-08

**Last Reviewed:** 2026-08-04

**Next Review:** 2027-07-08

**Related Documents:**
- philosophy.md
- repository.md
- business-modeling.md
- ENGINEERING.md

**Related ADRs:**
- None

---

# Purpose

The Implementation Standards define how the RegOS architecture is translated into software.

They establish consistent implementation practices that preserve business understanding, architectural integrity, and long-term maintainability.

Implementation decisions should always reinforce the business model rather than reshape it.

---

# Implementation Principles

## Principle 1 — Implement Capabilities, Not CRUD Operations

Software should implement business capabilities.

Controllers, endpoints, repositories, and databases exist to support business behavior rather than define it.

---

## Principle 2 — Business Intent Precedes Technical Design

Every implementation begins with business intent.

Commands express intent.

Capabilities execute intent.

Technical implementation follows.

---

## Principle 3 — Architecture Is Expressed Through Code

Code should clearly communicate the architecture.

A reader should understand the business capability without studying framework configuration.

---

## Principle 4 — Consistency Is Preferred Over Creativity

When multiple valid implementation approaches exist, prefer the one that is already established within RegOS.

Consistency improves maintainability, onboarding, and AI-assisted development.

---

# Building a Capability

Every capability follows the same implementation lifecycle.

```text
Business Capability
        ↓
Command
        ↓
Aggregate
        ↓
Business Rules
        ↓
Domain Events
        ↓
Facts
        ↓
Persistence
        ↓
API
        ↓
Tests
```

Business behavior should emerge through this flow.

Implementation should never begin from persistence or user interfaces.

---

# Modeling the Domain

## Aggregates

Aggregates protect business invariants.

They own business consistency and define transactional boundaries.

Aggregates should expose behavior rather than mutable state.

---

## Entities

Entities possess identity and participate in business behavior.

Entities should never become containers for unrelated logic.

---

## Value Objects

Value Objects describe business concepts without identity.

They should be immutable whenever practical.

---

## Domain Services

Domain Services exist only when business behavior cannot naturally belong to an Aggregate or Value Object.

They should remain rare.

---

## Specifications

Specifications express reusable business conditions.

They improve consistency and reduce duplicated business logic.

---

## Domain Events

Domain Events represent completed business actions.

They communicate meaningful business changes.

Events describe what has already happened.

---

# Application Layer

The Application Layer coordinates business capabilities.

It is responsible for:

- Commands
- Queries
- Handlers
- Validation
- Transactions
- Orchestration

The Application Layer does not contain business decision logic.

Business decisions belong to the Domain.

---

# Infrastructure Layer

Infrastructure supports the Domain and Application layers.

Responsibilities include:

- Persistence
- Messaging
- External integrations
- File storage
- Background processing

Infrastructure implements contracts defined by the business architecture.

It never defines business behavior.

## A repository hydrates what the aggregate needs to enforce its invariants

> **A repository must load every collection the aggregate reads to enforce a
> rule.** Not every navigation — the criterion is invariant enforcement, and
> some collections genuinely are not needed on the write path.

An aggregate's rules are written against its own state. `_roles.Any(...)`,
`_history[^1].OccurredOn`, `_documents.FirstOrDefault(...)` all assume **my
collection represents my state**. A repository that omits one does not break a
rule loudly; it makes the rule *vacuous*:

| Rule reads a collection that was not loaded | What happens |
|---|---|
| a duplicate check | passes silently, and the unique index fails the insert instead |
| a "find mine and remove it" | finds nothing, and returns not-found for something that exists |
| an ordering rule over history | compares against nothing |

**The database becomes the first line of defence instead of the domain**, and a
business rule surfaces as a 500 or a 404. That is precisely the failure
aggregates exist to prevent.

Found in EPIC-004 S005: `SubmissionRepository` included `Documents` but not
`Roles`, so `RemoveRole` searched an empty list and `AssignRole`'s duplicate
check was vacuously true. See
[testing.md Principle 9](testing.md) for the test class that catches it —
no unit test can, because an in-memory aggregate always has its collection
populated.

---

## A cache is not an aggregate

The two are easy to confuse because both hold data that took work to produce.
They are opposites in the only way that matters:

| | A cache | An aggregate |
|---|---|---|
| Exists to | avoid recomputation | own business facts |
| May be discarded | ✅ yes | ✖ never — deletion loses information |
| May be regenerated | ✅ yes | ✖ no |
| Authoritative | ✖ no | ✅ yes |

**The test is what deletion costs.** If discarding it loses no business
information, it is a cache — however expensive it was to produce, however much a
user wanted it, and however permanent the file on disk looks.

This arrives as a performance proposal, not as a modelling one. *"Generated
packages are slow to build, let's store them"* is a caching decision; storing
them in a table with an id and a status is a modelling decision, and the first
does not license the second. See
[ADR-049](../adr/ADR-049-generation-derives-transmission-creates.md) §5, where
the concrete case is an eCTD package.

---

# Cross-Cutting Standards

## Identifiers

Business entities should use strongly typed identifiers.

Primitive identifiers should not leak into business models.

---

## Time

Business time should be obtained through an abstraction rather than directly from the system clock.

This improves testing, repeatability, and determinism.

---

## Errors

Business failures should be represented explicitly.

Exceptions are reserved for unexpected technical failures.

---

## Results

Application operations should communicate success and failure through explicit result types rather than relying solely on exceptions.

---

## Auditing

Important business actions should be traceable.

Audit information should support regulatory accountability without polluting business logic.

---

# Implementation Checklist

Before implementing a capability, verify the following.

- [ ] The business capability has been defined.
- [ ] Business intent is represented by a Command.
- [ ] Business invariants are protected by an Aggregate.
- [ ] Domain Events describe completed business actions.
- [ ] Facts have been identified where applicable.
- [ ] Business Rules remain inside the Domain.
- [ ] Infrastructure contains no business decision logic.
- [ ] Strongly typed identifiers are used.
- [ ] Business time is abstracted.
- [ ] The capability is independently testable.
- [ ] Architectural changes requiring an ADR have been identified.

## When the capability adds an aggregate

Three items, added because each has cost a build cycle in consecutive epics.
They are here **and** enforced — a checklist depends on somebody reading it, and
EPIC-010a's retrospective recorded two of these before EPIC-018 hit one anyway.

- [ ] **The constructor takes only scalars and identifiers.** EF binds
      constructor parameters by name from *mapped properties*, and an owned
      value object is not one — it cannot be bound to a parameter at all. Either
      take only scalars and set the owned value afterwards (`HaCorrespondence`),
      or use a private parameterless constructor with an object-initializer
      factory (`GlobalLabel`, `PharmaceuticalProductDetail`).
- [ ] **Every owned value object is resolved fresh per owner.** An owned value is
      tracked against exactly one owner; sharing one instance across two
      aggregates persists nulls on the second. A vocabulary lookup returns a
      copy, guarded by an `EachResolutionIsItsOwnInstance` test.
- [ ] **The generated migration has no nullable shadow foreign key.** A shadow FK
      declared only from the parent side is nullable by default. An orphan
      becomes representable, and — because Postgres treats NULLs as distinct —
      any unique index naming that column silently stops constraining the
      parentless rows.

> **Enforced by `AggregateChildArchitectureTests`** (in
> `RegOS.Platform.Application.Tests`, where the EF model is available). It
> carries a shrink-only grandfathered list for the five children that predate
> it, and a companion test that fails if an entry goes stale.

---

# Change History

| Version | Date | Summary |
|----------|------------|-------------------------------------------|
| 1.1 | 2026-08-04 | Aggregate checklist: constructor binding, fresh owned values, nullable shadow FKs — each now also enforced by a test. |
| 1.0 | 2026-07-08 | Initial approved version. |