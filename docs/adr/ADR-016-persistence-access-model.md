# ADR-016 — Persistence Access: Repositories for Writes, DbContext for Reads

**Status:** Accepted · **Date:** 2026-07-20 (retro-documented) ·
**Supersedes:** [ADR-005](ADR-005-dbcontext-usage.md) ·
**Related:** ADR-002 (CQRS), ADR-006 (read model composition)

> **Retro-documented.** ADR-005 rejected the repository pattern. The codebase
> then grew six repository interfaces. This ADR records what RegOS actually
> does and retires the claim that repositories were rejected.

## Context

ADR-005 treated persistence access as a single question and answered it once:
handlers may use `RegOSDbContext` directly, repositories add abstraction without
value.

In practice the two sides of CQRS want different things. A write must load an
aggregate through one path so its invariants and value converters apply. A read
wants a flat projection with no tracking, no `Include`, and only the columns a
screen needs.

## Decision

**Writes go through a repository.** Each aggregate owns a repository interface
in its domain project, implemented in its infrastructure project:

- `IUserRepository`, `IProductRepository`, `IProductDocumentRepository`,
  `IRegulatoryApplicationRepository`, `ISubmissionRepository`,
  `ISubmissionSnapshotRepository`

**Reads use `RegOSDbContext` directly.** Query handlers inject the context and
project with `AsNoTracking()` — see `GetUsersHandler` and
`ListOrganizationsHandler`.

A query handler does not load aggregates. A command handler does not project.

## Consequences

- The aggregate is the only way business state changes, which is what makes
  invariant enforcement reliable.
- Read models are not constrained by the aggregate's shape or its value
  converters.
- Two persistence idioms coexist, so "which do I use?" must be answered by this
  ADR rather than by looking at the nearest file.
- `RegOSDbContext` is a single shared context (see `docs/ARCHITECTURE_BACKLOG.md`
  AB-001, now completed).

## Revisit When

- A read path needs aggregate behavior, which usually means it is a command in
  disguise.
- Repositories start growing query methods that serve screens rather than
  aggregate loading — the signal that the boundary has eroded.
