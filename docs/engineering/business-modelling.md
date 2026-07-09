# Business Modeling

---
**Title:** Business Modeling

**Owner:** Architecture Review Board

**Status:** Approved

**Version:** 1.0

**Effective Date:** 2026-07-08

**Last Reviewed:** 2026-07-08

**Next Review:** 2027-07-08

**Related Documents:**
- philosophy.md
- repository.md
- ENGINEERING.md

**Related ADRs:**
- None

---

# Purpose

Business Modeling defines how the regulatory world is represented within RegOS.

Its purpose is to ensure that software faithfully reflects business reality rather than implementation convenience.

Business concepts are understood before they are modeled.

Models are designed before they are implemented.

Technology follows the model—not the other way around.

---

# Business Modeling Principles

## Principle 1 — Model Meaning Before Structure

A business concept must be understood before it is represented in software.

Database tables, APIs, and classes are implementation details.

Business meaning always comes first.

---

## Principle 2 — Model Reality, Not Documents

RegOS models the regulatory world.

Documents are outputs generated from business knowledge.

They are not the foundation of the platform.

---

## Principle 3 — Business Language Is Canonical

Every model must use the shared business vocabulary defined by the RegOS Glossary and Ontology.

New terminology requires architectural review.

---

## Principle 4 — Behavior Before Data

Business capabilities define behavior.

Data exists to support that behavior.

Models should never be created solely to store information.

---

## Principle 5 — Preserve Business History

Business history is valuable knowledge.

Historical changes should be represented explicitly rather than overwritten.

Whenever practical, prefer immutable history over destructive updates.

---

# Business Modeling Hierarchy

Business models are created using the following hierarchy.

```text
Business Capability
        │
        ▼
Aggregate
        │
        ▼
Entity
        │
        ▼
Value Object
```

Supporting concepts include:

- Facts
- Business Rules
- Specifications
- Domain Events

These concepts work together to describe business behavior.

---

# Business Concepts

Business Concepts represent real-world regulatory concepts.

Examples include:

- Product
- Product Version
- Regulation
- Requirement
- Evidence
- Submission
- Authority
- Manufacturer

Business Concepts are not automatically software classes.

They must first be understood within the regulatory domain before selecting an implementation model.

---

# Business Capabilities

Capabilities describe what the business can do.

Examples include:

- Register Product
- Release Product Version
- Assess Submission Evidence
- Evaluate Regulatory Impact

Capabilities own business behavior.

Data structures exist to support capabilities—not replace them.

---

# Aggregates

Aggregates protect business consistency.

Each Aggregate owns a clearly defined transactional boundary and is responsible for enforcing its business invariants.

Aggregates should remain focused on one business responsibility.

Cross-aggregate coordination should occur through business processes or domain events rather than direct coupling.

---

# Entities

Entities possess identity and continuity over time.

Their identity is more important than their current state.

Entities exist within an Aggregate and participate in business behavior.

Entities should not become containers for unrelated logic.

---

# Value Objects

Value Objects describe business characteristics.

They have no independent identity.

They are immutable whenever possible.

Value Objects should model business meaning rather than technical convenience.

---

# Business Invariants

Business Invariants define conditions that must always remain true.

Aggregates are responsible for protecting these invariants.

Examples include:

- A Product Version belongs to exactly one Product.
- A released Product Version cannot be modified.
- A Submission targets a specific Authority.

Business Invariants originate from the regulatory domain rather than implementation technology.

---

# Facts

Facts represent business truths at a specific point in time.

Facts are independent of storage technology and independent of user interfaces.

Facts provide trusted knowledge that can be consumed by decision engines, business rules, reporting, and submissions.

The complete Fact model is defined separately within the RegOS Facts documentation.

---

# Business Rules

Business Rules evaluate facts to determine business outcomes.

Rules should remain independent of application workflows whenever possible.

Business Rules express regulatory knowledge rather than technical implementation.

---

# Domain Events

Domain Events describe meaningful business events that have already occurred.

Events communicate changes between business capabilities while preserving loose coupling.

Events describe the past.

Commands describe intended future behavior.

---

# Specifications

Specifications express business conditions that can be evaluated consistently throughout the platform.

Specifications improve reuse and reduce duplicated business logic.

---

# Decision Models

Business decisions should be derived from facts, business rules, evidence, and regulatory knowledge.

Decision logic should remain explainable, traceable, and independently testable.

---

# Model Evolution

Business models evolve because the regulatory world evolves.

Changes to business models should preserve historical understanding whenever practical.

Architectural changes affecting business concepts require review through the Architecture Review Board.

---

# Business Modeling Checklist

Before introducing or modifying a business model, verify the following.

- [ ] The business concept is clearly understood.
- [ ] The terminology matches the RegOS Glossary.
- [ ] The model represents business meaning rather than technical implementation.
- [ ] Business invariants have been identified.
- [ ] Ownership boundaries are clear.
- [ ] Facts, Rules, Events, and Specifications have been considered where applicable.
- [ ] Historical behavior has been preserved.
- [ ] The model supports existing business capabilities.
- [ ] Architectural changes requiring an ADR have been identified.

---

# Change History

| Version | Date | Summary |
|----------|------------|-----------------------------------------|
| 1.0 | 2026-07-08 | Initial approved version. |