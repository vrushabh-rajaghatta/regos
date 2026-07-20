# ADR-005 — DbContext Usage

**Status:** Superseded by [ADR-016](ADR-016-persistence-access-model.md) ·
**Date:** 2026-07-19 · **Superseded:** 2026-07-20

> Extracted verbatim from `docs/architecture/09-technology-decisions.md` during
> the ADR reconciliation of 2026-07-20. Content unchanged; number unchanged.
>
> **This decision no longer describes the codebase.** It rejected the repository
> pattern outright; six repository interfaces now exist and are used on every
> write path. ADR-016 records what RegOS actually does. This file is retained as
> history — do not cite it as current guidance.

## Decision

Application handlers may use RegOSDbContext directly.

## Rationale

Introducing repositories would add abstraction without providing sufficient
value for the current architecture.

Entity Framework Core already provides a repository and unit-of-work pattern.

## Alternatives Considered

Repository pattern

## Consequences

- Less boilerplate
- Simpler implementation
- Easier query composition

## Why It Was Superseded

The decision treated persistence access as one question. It is two. Aggregate
writes need invariant enforcement and a single loading path; read models need
projection without tracking or value converters. RegOS converged on repositories
for the former and direct `DbContext` access for the latter — which this ADR
permits for reads and forbids for writes. See ADR-016.
