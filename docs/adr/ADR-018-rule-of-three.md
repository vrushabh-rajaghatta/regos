# ADR-018 — Duplicate Twice, Abstract on the Third

**Status:** Accepted · **Date:** 2026-07-20 (retro-documented) ·
**Related:** [ADR-017](ADR-017-shared-kernel-scope.md) (shared kernel scope)

> **Retro-documented.** This rule has governed every extraction and every
> deletion in RegOS. It is cultural rather than technical, which is exactly why
> it needs writing down: it is the one decision a new contributor cannot infer
> from the code, because its effects are invisible — the abstractions that were
> never built.

## Context

Two forces pull in opposite directions.

Premature abstraction is expensive and hard to reverse. An abstraction extracted
from two examples encodes the accident that both examples shared, and every
third case is then bent to fit it. The cost is paid by everyone who touches the
abstraction afterwards.

Duplication is also expensive, but its cost is visible and local, and it is
cheap to remove later. Critically, the third occurrence is the first point at
which you can *see* which parts are genuinely common.

## Decision

**Duplicate on the second occurrence. Extract on the third demonstrated need.**

At the third occurrence, ask a question rather than extracting mechanically:

> Are these actually the same thing, or three things that currently look alike?

If the answer is "three things that look alike", duplicate again and note it.

### What "demonstrated" excludes

A need is demonstrated by working code that exists, not by a plan. These are not
demonstrations:

- symmetry with another module ("Platform has one, so Product should")
- a capability we expect to build
- the observation that two things resemble each other

### Applied

| Case | Occurrences | Outcome |
|---|---|---|
| `GetRequiredAsync` (load-or-404, tenant-scoped) | 3 in Platform | **Extracted** to `UserRepositoryExtensions` |
| `PagedResult<T>` | 2 (Platform, Product) | **Duplicated.** Sharing it would either couple Product to Platform's application layer or put a transport concern in the kernel |
| `IProductReader` | **0** | **Deleted** with its whole project. Built for cross-context reads that never arrived |
| `ProductStatus.Active` | **0**, and unreachable | **Deleted.** No transition ever set it |
| `IUnitOfWork` | 1, unused | **Deleted.** Nothing composed several repositories into one commit |
| `SingleResult<T>` | 0 | **Never built.** Proposed for symmetry with `PagedResult<T>`; would have competed with the established `NotFoundException` convention |

### The rule cuts both ways

The same standard that forbids speculative *creation* forbids speculative
*deletion*. `IProductRepository.GetByIdAsync` was consumerless after reads moved
to projections and was **kept**, because the next two stories needed it to load
an aggregate for a write. Removing it to re-add it immediately would be
cargo-cult minimalism.

The test is not "is this used today". It is "is there evidence this describes
the system we have".

## Consequences

- The codebase contains deliberate duplication. Each instance should say so and
  say what would trigger extraction.
- Abstractions that do exist earned their place, so they can be trusted rather
  than worked around.
- Deleting speculative code requires the same evidence as building it, which
  makes both decisions reviewable instead of matters of taste.
- Someone reading the code will find things that "should obviously be shared".
  That is the rule working, not an oversight.
