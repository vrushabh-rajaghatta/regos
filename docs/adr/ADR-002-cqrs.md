# ADR-002 — CQRS

**Status:** Accepted · **Date:** 2026-07-19 · **Supersedes:** nothing ·
**Related:** ADR-006 (read model composition), ADR-003 (no MediatR)

> Extracted verbatim from `docs/architecture/09-technology-decisions.md` during
> the ADR reconciliation of 2026-07-20. Content unchanged; number unchanged.

## Context

Read and write paths in RegOS have different shapes: writes enforce aggregate
invariants, reads compose projections across contexts.

## Decision

Separate commands and queries.

Commands modify business state.

Queries build read models.

## Rationale

Read and write operations have different responsibilities. Keeping them separate
improves maintainability and simplifies testing.

## Alternatives Considered

Traditional CRUD services.

## Consequences

- Clear separation of responsibilities.
- Simpler query optimization.
- Easier evolution of read models.

## Revisit When

- The overhead of a separate read model exceeds its benefit for a context that
  is genuinely CRUD-shaped.
