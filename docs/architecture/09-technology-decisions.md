# Technology Decisions — Retired

**This document was retired on 2026-07-20.** Its contents have been extracted
into individual Architecture Decision Records.

The canonical location for every RegOS architecture decision is
[`docs/adr/`](../adr/README.md).

## Why

This file held ADR-001 through ADR-010 as numbered sections. A second, parallel
ADR series was meanwhile growing in `docs/adr/`, and the two disagreed about
what ADR-007, ADR-008 and ADR-009 meant — while source code cited numbers from
both. A decision log that returns a different answer depending on which file you
open is worse than no log.

The reconciliation, and how each collision was resolved, is documented in
[`docs/adr/README.md`](../adr/README.md#the-2026-07-20-reconciliation).

## Where each decision went

| Was | Now |
|---|---|
| ADR-001 Modular Architecture | [ADR-001](../adr/ADR-001-modular-architecture.md) |
| ADR-002 CQRS | [ADR-002](../adr/ADR-002-cqrs.md) |
| ADR-003 No MediatR | [ADR-003](../adr/ADR-003-no-mediatr.md) |
| ADR-004 Explicit Dependency Injection | [ADR-004](../adr/ADR-004-explicit-dependency-injection.md) |
| ADR-005 DbContext Usage | [ADR-005](../adr/ADR-005-dbcontext-usage.md) — **superseded** by [ADR-016](../adr/ADR-016-persistence-access-model.md) |
| ADR-006 Read Model Composition | [ADR-006](../adr/ADR-006-read-model-composition.md) |
| ADR-007 Module Ownership | [ADR-007](../adr/ADR-007-module-ownership.md) |
| ADR-008 Composition Modules | [ADR-008](../adr/ADR-008-composition-modules.md) |
| ADR-009 Development Lifecycle | **[ADR-011](../adr/ADR-011-development-lifecycle.md)** — renumbered |
| ADR-010 Documentation as Code | [ADR-010](../adr/ADR-010-documentation-as-code.md) |

`ADR-009` now means the [Command Validation Model](../adr/ADR-009-command-validation-model.md),
which is cited by that number in four source files.

## Future decisions

The "Future Decisions" list previously kept here — event-driven architecture,
background job processing — belongs in the roadmap, not in a decision log. A
decision that has not been made does not get a number.
