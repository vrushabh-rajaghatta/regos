# ADR-001 — Modular Architecture

**Status:** Accepted · **Date:** 2026-07-19 · **Supersedes:** nothing ·
**Related:** ADR-007 (module ownership), ADR-008 (composition modules)

> Extracted verbatim from `docs/architecture/09-technology-decisions.md` during
> the ADR reconciliation of 2026-07-20. Content unchanged; number unchanged.

## Context

RegOS needed a top-level organizing principle for the solution: whether to group
code by business capability or by technical layer.

## Decision

Organize the system around business modules rather than technical layers.

## Rationale

Business modules provide clear ownership, improve discoverability, and reduce
coupling between unrelated functionality.

## Alternatives Considered

- Layer-first architecture
- Namespace-by-technical-layer

## Consequences

- Higher cohesion
- Easier navigation
- Better long-term scalability

## Revisit When

- A module's internals are consistently changed by teams that do not own it.
- Cross-module duplication becomes cheaper to centralize than to repeat.
