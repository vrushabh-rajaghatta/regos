# Testing

---
**Title:** Testing

**Owner:** Architecture Review Board

**Status:** Approved

**Version:** 1.0

**Effective Date:** 2026-07-08

**Last Reviewed:** 2026-07-08

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

---

# Change History

| Version | Date | Summary |
|----------|------------|------------------------------------------|
| 1.0 | 2026-07-08 | Initial approved version. |