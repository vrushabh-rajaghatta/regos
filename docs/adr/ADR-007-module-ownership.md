# ADR-007 — Module Ownership

**Status:** Accepted · **Date:** 2026-07-19 · **Supersedes:** nothing ·
**Related:** ADR-001 (modular architecture), ADR-008 (composition modules)

> Extracted verbatim from `docs/architecture/09-technology-decisions.md` during
> the ADR reconciliation of 2026-07-20. Content unchanged; number unchanged.
>
> **Numbering note.** `ADR-009-command-validation-model.md` was written against
> a parallel series in which "ADR-007" meant the shared exception contract. That
> decision is now recorded as [ADR-012](ADR-012-shared-semantic-exception-model.md).
> Any pre-2026-07-20 reference to "ADR-007 (shared exception contract)" means
> ADR-012, not this document.

## Decision

Every business capability belongs to exactly one module.

## Rationale

Clear ownership prevents duplicated business logic and establishes well-defined
architectural boundaries.

## Consequences

- Single source of truth
- Reduced duplication
- Clear maintenance responsibility

## Revisit When

- A capability genuinely spans two modules and neither can own it without
  reaching into the other.
