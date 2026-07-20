# ADR-003 — No MediatR

**Status:** Accepted · **Date:** 2026-07-19 · **Supersedes:** nothing ·
**Related:** ADR-004 (explicit dependency injection)

> Extracted verbatim from `docs/architecture/09-technology-decisions.md` during
> the ADR reconciliation of 2026-07-20. Content unchanged; number unchanged.

## Context

CQRS is commonly implemented with a mediator library. RegOS had to decide
whether the indirection earned its place.

## Decision

Use explicit handlers with dependency injection instead of MediatR.

## Rationale

The project values explicit dependencies and straightforward execution flow over
indirection.

Handlers are easy to discover, test, and debug without introducing an additional
mediator abstraction.

## Alternatives Considered

- MediatR
- Custom mediator implementation

## Consequences

- Less abstraction
- Simpler debugging
- Explicit dependency registration

## Revisit When

- Cross-cutting pipeline behaviors (validation, logging, transactions) are
  needed on every handler and hand-wiring them becomes the larger cost.
