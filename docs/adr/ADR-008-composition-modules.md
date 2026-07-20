# ADR-008 — Composition Modules

**Status:** Accepted · **Date:** 2026-07-19 · **Supersedes:** nothing ·
**Related:** ADR-006 (read model composition), ADR-007 (module ownership)

> Extracted verbatim from `docs/architecture/09-technology-decisions.md` during
> the ADR reconciliation of 2026-07-20. Content unchanged; number unchanged.
>
> **Numbering note.** `ADR-009-command-validation-model.md` was written against
> a parallel series in which "ADR-008" meant the tenant context decision. That
> decision is now recorded as [ADR-013](ADR-013-ambient-tenant-context.md).
> Any pre-2026-07-20 reference to "ADR-008 (tenant context)" means ADR-013, not
> this document.

## Decision

Experience modules aggregate information but never own business state.

Examples include:

- Application Workspace
- Dashboard
- Reporting
- Analytics

## Rationale

Business rules remain within producer modules while composition modules focus
exclusively on presenting information.

## Consequences

- Clear separation of concerns
- Simpler maintenance
- Flexible user experiences

## Revisit When

- A composition module needs to persist state that no producer module wants to
  own.
