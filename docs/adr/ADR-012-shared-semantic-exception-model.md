# ADR-012 — Shared Semantic Exception Model

**Status:** Accepted · **Date:** 2026-07-20 (retro-documented) ·
**Implemented by:** commits `77e3681`, `cd79eed` (ARCH-001) ·
**Related:** ADR-009 (command validation model)

> **Retro-documented.** This decision was made and shipped during ARCH-001 but
> never written up. It was referred to as "ADR-007 (shared exception contract)"
> in `ADR-009-command-validation-model.md`, written against a parallel numbering
> series. ADR-007 in the canonical series is Module Ownership, so this decision
> takes the next free number. **Any reference to "ADR-007 (shared exception
> contract)" means this document.**

## Context

Before ARCH-001, each bounded context signalled failure its own way. The same
class of failure — an unknown id, a violated lifecycle rule — produced different
HTTP status codes depending on which module handled it, and the web client could
not respond to failures generically.

## Decision

Three exception types live in `RegOS.SharedKernel.Exceptions` and every context
uses them:

| Exception | Meaning | HTTP |
|---|---|---|
| `DomainException` | The request is malformed or self-inconsistent | 400 |
| `NotFoundException` | An entity addressed by the route does not exist | 404 |
| `BusinessRuleViolationException` | Current state forbids the operation | 409 |

The choice between them is not left to the handler author's preference; it
follows the decision tree in [ADR-009](ADR-009-command-validation-model.md).

## Consequences

- A single middleware maps all three to ProblemDetails.
- The web client can distinguish "you sent something wrong" from "the world is
  not in the right state" without per-endpoint knowledge.
- Adding a fourth exception type is a decision, not a convenience.

## Revisit When

- A failure class arrives that genuinely fits none of the three — authorization
  denial (403) is the most likely candidate once Epic 4 lands.
