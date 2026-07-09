# Engineering Philosophy

---
**Title:** Engineering Philosophy

**Owner:** Architecture Review Board

**Status:** Approved

**Version:** 1.0

**Effective Date:** 2026-07-08

**Last Reviewed:** 2026-07-08

**Next Review:** 2027-07-08

**Related Documents:**
- README.md
- MANIFESTO.md
- ENGINEERING.md

**Related ADRs:**
- None

---

# Purpose

The RegOS Engineering Manual defines the engineering standards used to design, implement, and evolve the RegOS platform.

Its purpose is to ensure that every engineer—human or AI—makes consistent architectural decisions while preserving the business understanding of the regulatory domain.

Technology will evolve.

Programming languages will change.

Frameworks will be replaced.

The engineering philosophy described in this document is intended to remain stable regardless of implementation technology.

Engineering at RegOS begins with understanding the business before writing software.

---

# Engineering Articles

## Article 1 — Model Reality Before Modeling Software

Software should faithfully represent the regulatory world.

Engineering begins by understanding products, regulations, evidence, decisions, and submissions before designing technical solutions.

---

## Article 2 — Business Language Is the Primary Language of RegOS

Business terminology is the foundation of the platform.

Classes, capabilities, APIs, events, and documentation should reflect the shared business vocabulary rather than technical implementation details.

---

## Article 3 — Knowledge Is the Primary Asset

RegOS is a knowledge platform.

Code exists to capture, preserve, and apply business knowledge.

Knowledge always has greater long-term value than implementation.

---

## Article 4 — Capabilities Own Behavior. Domain Objects Support Capabilities.

Business capabilities define what the platform does.

Aggregates, entities, value objects, and domain services exist to support those capabilities.

Engineering should always begin from the capability, not from the data model.

---

## Article 5 — Architecture Preserves Business Understanding

Architecture exists to protect the integrity of the regulatory domain.

Patterns, frameworks, and infrastructure decisions must never obscure business meaning.

---

## Article 6 — Every Business Decision Must Be Explainable and Traceable

Every important regulatory decision must be supported by facts, rules, evidence, and historical context.

The platform should make it possible to understand why a decision was reached.

---

## Article 7 — Optimize for Clarity Over Cleverness

Readable systems outlive clever systems.

When multiple solutions are technically correct, prefer the one that is easier for another engineer to understand.

---

## Article 8 — Code Implements Knowledge. Code Does Not Define Knowledge.

Business knowledge originates from the regulatory domain, not from software.

Implementation should always reflect established business understanding rather than invent it.

---

## Article 9 — Artificial Intelligence Augments Engineering. Humans Remain Accountable.

Artificial Intelligence is an engineering collaborator.

It accelerates implementation, analysis, and documentation.

Responsibility for architecture, business understanding, and engineering decisions remains with human engineers.

---

## Article 10 — Leave RegOS Easier to Understand Than You Found It

Every contribution should improve the platform's clarity.

Engineers should continuously simplify, clarify, and strengthen the shared understanding of the system.

---

# Decision Hierarchy

Engineering decisions shall follow the hierarchy below.

```text
Vision
    ↓
Manifesto
    ↓
Constitution
    ↓
Engineering Manual
    ↓
Architecture Decision Records (ADRs)
    ↓
Capability Specifications
    ↓
Implementation
```

Artifacts lower in the hierarchy must never contradict those above them.

When conflicts occur, the higher-level artifact takes precedence.

---

# Engineering Promise

As an engineer contributing to RegOS, I make the following commitments.

- I will understand the regulatory domain before changing the software.
- I will preserve the shared language of RegOS.
- I will optimize for clarity over cleverness.
- I will document architectural decisions that change business understanding.
- I will treat knowledge as the primary asset of the platform.
- I will leave RegOS easier to understand than I found it.

---

# Change History

| Version | Date | Summary |
|----------|------------|----------------------------------------------|
| 1.0 | 2026-07-08 | Initial approved version. |