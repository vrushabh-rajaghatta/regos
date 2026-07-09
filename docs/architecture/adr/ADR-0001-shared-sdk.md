# ADR-0001: Introduce Shared SDK

## Status

Accepted

## Context

RegOS requires a shared project for engineering primitives that are common across bounded contexts.

The SDK intentionally excludes business concepts such as Entities, Aggregates, and Value Objects.

These remain owned by each bounded context.

## Decision

Introduce `RegOS.SDK` as the shared engineering project.

The SDK will grow incrementally as implementation demands shared primitives.

No abstraction should be introduced before it has a demonstrated need.

## Consequences

- Business modeling remains independent.
- Shared code remains minimal.
- Coupling between bounded contexts is reduced.