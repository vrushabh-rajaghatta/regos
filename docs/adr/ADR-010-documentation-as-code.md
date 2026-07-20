# ADR-010 — Documentation as Code

**Status:** Accepted · **Date:** 2026-07-19 · **Supersedes:** nothing ·
**Related:** ADR-011 (development lifecycle)

> Extracted verbatim from `docs/architecture/09-technology-decisions.md` during
> the ADR reconciliation of 2026-07-20. Content unchanged; number unchanged.

## Decision

Architecture documentation evolves alongside the codebase.

Documentation updates are part of the Definition of Done.

## Rationale

Keeping documentation synchronized with implementation preserves architectural
knowledge and supports onboarding.

## Consequences

- Living documentation
- Reduced knowledge loss
- Easier long-term maintenance

## Revisit When

- Documentation updates become a bottleneck that encourages smaller, undocumented
  changes to bypass the process.
