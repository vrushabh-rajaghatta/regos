# Testing

---
**Title:** Testing

**Owner:** Architecture Review Board

**Status:** Approved

**Version:** 1.1

**Effective Date:** 2026-07-08

**Last Reviewed:** 2026-08-01

**Next Review:** 2027-07-08

**Related Documents:**
- philosophy.md
- business-modeling.md
- implementation-standards.md
- engineering-workflow.md

**Related ADRs:**
- None

---

# Purpose

The purpose of testing in RegOS is to increase confidence in business behavior.

Tests exist to verify that the platform correctly models the regulatory domain, protects business invariants, produces explainable decisions, and remains trustworthy as it evolves.

Code coverage is a metric.

Business confidence is the objective.

---

# Testing Principles

## Principle 1 — Test Business Behavior

Tests should verify business outcomes rather than implementation details.

Refactoring should not require rewriting tests unless business behavior changes.

---

## Principle 2 — Protect Business Invariants

Every business invariant should be protected by automated tests.

If violating an invariant would create an invalid regulatory state, it must be tested.

---

## Principle 3 — Business Rules Must Be Independently Verifiable

Business Rules should be testable without APIs, databases, or user interfaces.

Rule evaluation should remain deterministic and repeatable.

---

## Principle 4 — Explainability Is Testable

If the platform produces a regulatory decision, the reasoning behind that decision must also be verifiable.

Testing should confirm both the decision and its explanation.

---

## Principle 5 — Prefer Deterministic Tests

Tests should produce identical results given identical inputs.

Time, randomness, and external dependencies should be controlled through abstractions.

---

## Principle 6 — Architecture Is Also Tested

Architecture is part of the product.

Dependency rules, project boundaries, and implementation standards should be verified through automated architecture tests.

---

## Principle 7 — A Test Owns Every Entity It Mutates

A browser spec must create the business entities it acts on, capture their
identifiers, and operate on those identifiers only.

```
seed via the API  →  capture the id  →  operate on that id  →  retire it
```

Never select a subject from ambient data:

```ts
// Wrong — acts on whatever the database happens to contain
const target = organizations.find((o) => o.status === "Active");

// Right — the spec owns what it touches
const id = await seedOrganization(unique("Browser Edit Org"));
```

This is stricter than "clean up after yourself", and it exists because a
violation is silent. A spec that reads ambient data passes for months and then
mutates seeded data that another spec depends on. RegOS lost a seeded
organization to exactly this — it was found by inspection, not by a failing
test.

Two safeguards back the rule, and they do different jobs:

- **`OrganizationInitializer` reconciles demo data on startup.** Developer
  convenience: experiment locally, restart, get a known state back. It updates
  only rows that already exist, so it never pushes demo data into a database
  holding real records.
- **`seed-integrity.spec.ts` is a canary.** It asserts the seeded organizations
  are present, unmodified and Active. If it fails, a spec mutated something it
  did not create. Fix the spec — never relax the assertion.

Related: [ADR-019](../adr/ADR-019-testing-strategy.md) rule 1, which this
principle strengthens.

---

## Principle 8 — Both Halves Of A Capability Are Reachable

For every new business capability, ask two questions:

1. **Can the user perform the action?**
2. **Can the user then observe the business fact that action created?**

A capability is not shipped until both are true. The question is symmetric on
purpose, because RegOS has now shipped each half without the other:

| | Missing half | Found |
|---|---|---|
| Milestone 1 | `Activate`/`Deactivate` existed on the aggregate; no endpoint reached them | EPIC-016, by inspection |
| EPIC-016 S003 | Organization identity was modelled and unreachable | during the same epic |
| EPIC-017 S003 | Market status history was **written on every change and readable nowhere** | EPIC-017 S004, when the row ran out of room |

The third is the instructive one. Nothing was broken: the write worked, the
data was correct, the tests passed. But the whole reason that history stores
`OccurredOn` *and* `RecordedOnUtc` is so a reader can tell a backdated entry
from a late one — and with no reader, the second timestamp was pure cost.

**An unobservable fact is indistinguishable from a fact that was never
recorded.** Write the observation surface in the same story, or say in the epic
which story owns it.

> Applies to reviews as much as to tests: *can perform → can observe* is short
> enough to run against every story in your head.

---

# Confidence Levels

RegOS organizes testing by confidence rather than by framework.

## Level 1 — Model Confidence

Verifies that Aggregates, Entities, Value Objects, Specifications, and Domain Services correctly represent the regulatory domain.

Typical examples include:

- Aggregate invariants
- Value Object validation
- Specification evaluation

---

## Level 2 — Capability Confidence

Verifies complete business capabilities.

Examples include:

- Register Product
- Release Product Version
- Assess Submission Evidence

A capability succeeds only when the intended business outcome is achieved.

---

## Level 3 — Decision Confidence

Verifies that identical facts always produce identical regulatory decisions.

Decision testing should validate:

- Facts
- Rules
- Evidence
- Decision outcome
- Decision explanation

---

## Level 4 — Integration Confidence

Verifies communication with external systems.

Examples include:

- Regulatory authority APIs
- Identity providers
- Messaging infrastructure
- File storage

Business behavior should remain isolated from integration failures whenever possible.

---

## Level 5 — System Confidence

Verifies complete end-to-end regulatory workflows.

Examples include:

- Product registration
- Submission preparation
- Regulatory assessment
- Decision publication

These tests provide confidence that the platform functions correctly as an integrated system.

---

# Testing Standards

## Standard 1

Every Capability must have business-focused automated tests.

---

## Standard 2

Every Aggregate must have invariant tests.

---

## Standard 3

Every Business Rule must have deterministic tests.

---

## Standard 4

Every Domain Event should be verified through business behavior rather than implementation details.

---

## Standard 5

External systems should be replaceable during testing.

---

## Standard 6

Tests should improve confidence, not inflate coverage metrics.

Coverage percentages should never become the primary engineering objective.

---

# Test Pyramid

The traditional testing pyramid focuses on implementation.

RegOS focuses on confidence.

```text
System Confidence
        ▲
Integration Confidence
        ▲
Decision Confidence
        ▲
Capability Confidence
        ▲
Model Confidence
```

Every layer builds confidence in the one above it.

---

# Testing Checklist

Before considering a capability complete, verify:

- [ ] Business invariants are tested.
- [ ] Capability outcomes are tested.
- [ ] Business Rules are independently verified.
- [ ] Decision reasoning is validated where applicable.
- [ ] Integration boundaries are tested.
- [ ] Failure scenarios are covered.
- [ ] Historical behavior remains preserved.
- [ ] Architectural boundaries remain valid.
- [ ] Every browser spec owns the entities it mutates (Principle 7).
- [ ] Both halves of every new capability are reachable — the user can perform
      the action *and* observe the fact it created (Principle 8).

---

# Change History

| Version | Date | Summary |
|----------|------------|------------------------------------------|
| 1.1 | 2026-08-01 | Principle 8 — both halves of a capability are reachable (EPIC-017 S005). |
| 1.0 | 2026-07-08 | Initial approved version. |