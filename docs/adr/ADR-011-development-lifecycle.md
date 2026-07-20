# ADR-011 — Development Lifecycle

**Status:** Accepted · **Date:** 2026-07-19 · **Renumbered:** 2026-07-20 ·
**Related:** ADR-010 (documentation as code)

> **Renumbered from ADR-009.** This decision was published as "ADR-009 —
> Development Lifecycle" in `docs/architecture/09-technology-decisions.md`.
> A different decision — the command validation model — was independently
> published as `ADR-009` in `docs/adr/` and is cited by that number in four
> source files. The code citations decided the collision; this document moved.
> Content is otherwise unchanged.
>
> Any pre-2026-07-20 reference to "ADR-009 — Development Lifecycle" means this
> document. "ADR-009" without qualification now means the command validation
> model.

## Decision

Every capability follows the same engineering lifecycle.

```
Capability
    ↓
Design
    ↓
Sprint
    ↓
Milestone
    ↓
Implementation
    ↓
Review
    ↓
Freeze
```

## Rationale

A consistent process improves predictability, reduces rework, and ensures
architectural alignment.

## Consequences

- Higher implementation quality
- Repeatable delivery
- Better documentation

## Revisit When

- The ceremony cost exceeds its benefit for small, well-understood changes.
