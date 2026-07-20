# ADR-004 — Explicit Dependency Injection

**Status:** Accepted · **Date:** 2026-07-19 · **Supersedes:** nothing ·
**Related:** ADR-003 (no MediatR)

> Extracted verbatim from `docs/architecture/09-technology-decisions.md` during
> the ADR reconciliation of 2026-07-20. Content unchanged; number unchanged.

## Context

Handlers and services must be registered with the container. Assembly scanning
would do this implicitly.

## Decision

Register handlers and services explicitly.

## Rationale

Explicit registrations make dependencies visible and simplify troubleshooting.

Automatic assembly scanning was considered unnecessary for the current scale of
the project.

## Alternatives Considered

Assembly scanning

## Consequences

- Clear registrations
- Predictable startup
- Easier maintenance

## Revisit When

- The registration files become a routine merge-conflict site, or a module's
  `DependencyInjection.cs` is forgotten often enough to cause runtime failures.
