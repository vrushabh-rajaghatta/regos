# ADR-006 — Read Model Composition

**Status:** Accepted · **Date:** 2026-07-19 · **Supersedes:** nothing ·
**Related:** ADR-002 (CQRS), ADR-005 (persistence access)

> Extracted verbatim from `docs/architecture/09-technology-decisions.md` during
> the ADR reconciliation of 2026-07-20. Content unchanged; number unchanged.
>
> **Cited in source:** `IProductRepository.cs:9`.

## Context

Query handlers frequently need data that another query handler already
assembles. Reusing handlers would couple read paths to each other.

## Decision

Query handlers compose their own read models.

Query handlers should not invoke other query handlers.

## Rationale

Read model composition remains explicit and avoids unnecessary coupling between
handlers.

## Consequences

- Predictable query flow
- Better performance
- Easier optimization

## Revisit When

- Duplication between read models becomes a correctness risk rather than a
  typing cost — i.e. the same projection rule is being fixed in several places.
