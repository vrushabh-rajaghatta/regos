# Engineering Workflow

---
**Title:** Engineering Workflow

**Owner:** Architecture Review Board

**Status:** Approved

**Version:** 1.0

**Effective Date:** 2026-07-08

**Last Reviewed:** 2026-07-08

**Next Review:** 2027-07-08

**Related Documents:**
- philosophy.md
- repository.md
- business-modeling.md
- implementation-standards.md
- testing.md

**Related ADRs:**
- None

---

# Purpose

The RegOS Engineering Workflow defines how ideas evolve into production software.

Its purpose is to ensure that every architectural decision, implementation, review, and deployment preserves the engineering standards of the platform.

The workflow applies equally to human engineers and AI engineering assistants.

---

# Engineering Principles

## Principle 1 — Understanding Precedes Implementation

Engineering begins by understanding the business problem.

Implementation must never begin before the business capability is understood.

---

## Principle 2 — Architecture Guides Implementation

Implementation follows architecture.

Code should realize architectural decisions rather than redefine them.

---

## Principle 3 — AI Augments Engineering

Artificial Intelligence accelerates implementation, documentation, and analysis.

Human engineers remain accountable for architectural and business decisions.

---

## Principle 4 — Review Protects Quality

Every meaningful change should be reviewed against the Engineering Manual and applicable ADRs.

Reviews protect the architecture as much as they protect the code.

---

# Engineering Lifecycle

Every significant change follows the same lifecycle.

```text
Understand
      ↓
Model
      ↓
Propose
      ↓
Architecture Review
      ↓
Approve
      ↓
Author
      ↓
Implement
      ↓
Test
      ↓
Review
      ↓
Merge
      ↓
Release
```

No phase should be skipped.

The purpose of each phase is to reduce uncertainty before moving to the next.

---

# Architecture Review

Architecture Review is required when a change affects:

- Business models
- Bounded contexts
- Cross-context dependencies
- Engineering standards
- Public APIs
- Foundational documents

Architecture Review should result in one of the following outcomes:

- Approved
- Approved with Changes
- Deferred
- Rejected

Significant architectural changes require an ADR before implementation begins.

---

# AI Engineering Workflow

AI is treated as an engineering collaborator.

Typical workflow:

```text
Architecture
      ↓
Approved Standard
      ↓
Implementation Prompt
      ↓
AI Generates Code
      ↓
Human Review
      ↓
Architecture Review
      ↓
Merge
```

AI-generated code is subject to the same engineering standards and review process as human-written code.

AI never becomes the authoritative source of architectural decisions.

---

# Pull Requests

Every Pull Request should clearly describe:

- Business capability being implemented
- Architectural impact
- Related ADRs
- Related capability specification
- Testing performed

Large Pull Requests should be decomposed whenever practical.

Smaller, focused changes improve review quality.

---

# Code Review

Code Reviews evaluate more than correctness.

Every review should consider:

- Business understanding
- Architectural consistency
- Business modeling
- Implementation standards
- Testing quality
- Simplicity
- Maintainability

The objective is to improve the platform rather than merely approve changes.

---

# Definition of Ready

Implementation may begin only when:

- [ ] Business capability is defined.
- [ ] Business terminology is understood.
- [ ] Architectural boundaries are clear.
- [ ] Required ADRs have been approved.
- [ ] Acceptance criteria are defined.

---

# Definition of Done

A capability is considered complete only when:

- [ ] Business behavior is implemented.
- [ ] Engineering standards are satisfied.
- [ ] Automated tests pass.
- [ ] **If the story changed an API contract, the browser suite has passed.**
- [ ] Architecture remains consistent.
- [ ] Documentation is updated where required.
- [ ] Code Review is approved.
- [ ] The capability is ready for production deployment.

> **Why the browser suite is called out separately.** It is not a stricter
> restatement of *"automated tests pass"* — the .NET suites can be entirely
> green while the browser suite is broken, because the browser suite is the only
> place a request body is composed the way a client composes it.
>
> This is not hypothetical. **EPIC-007a S001** moved a field from one endpoint's
> body to another's, shipped with 1,019 passing tests, and left eight browser
> specs posting the old shapes. S002 found them. The contract had changed; only
> the tests that speak the contract noticed.

---

# Continuous Improvement

Engineering standards evolve through experience.

Improvements should originate from implementation lessons rather than personal preference.

Changes to foundational standards require review through the Architecture Review Board.

---

# Change History

| Version | Date | Summary |
|----------|------------|-----------------------------------------------|
| 1.0 | 2026-07-08 | Initial approved version. |