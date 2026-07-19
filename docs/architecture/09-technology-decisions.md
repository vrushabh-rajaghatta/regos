# Technology Decisions

Version: 1.0 (Draft)

---

# Purpose

This document records the significant architectural and technology decisions made during the development of the Regulatory Operating System (RegOS).

Each decision captures:

- The problem being solved
- The decision that was made
- The rationale behind the decision
- Alternatives that were considered
- The implications of the decision

The objective is to preserve architectural intent and avoid repeatedly revisiting previously resolved discussions.

---

# Decision Status

| Status     | Meaning                      |
| ---------- | ---------------------------- |
| Proposed   | Under discussion             |
| Accepted   | Approved and adopted         |
| Superseded | Replaced by a newer decision |
| Deprecated | No longer recommended        |

---

# ADR-001 — Modular Architecture

## Status

Accepted

## Decision

Organize the system around business modules rather than technical layers.

## Rationale

Business modules provide clear ownership, improve discoverability, and reduce coupling between unrelated functionality.

## Alternatives Considered

- Layer-first architecture
- Namespace-by-technical-layer

## Consequences

- Higher cohesion
- Easier navigation
- Better long-term scalability

---

# ADR-002 — CQRS

## Status

Accepted

## Decision

Separate commands and queries.

Commands modify business state.

Queries build read models.

## Rationale

Read and write operations have different responsibilities.

Keeping them separate improves maintainability and simplifies testing.

## Alternatives Considered

Traditional CRUD services.

## Consequences

- Clear separation of responsibilities.
- Simpler query optimization.
- Easier evolution of read models.

---

# ADR-003 — No MediatR

## Status

Accepted

## Decision

Use explicit handlers with dependency injection instead of MediatR.

## Rationale

The project values explicit dependencies and straightforward execution flow over indirection.

Handlers are easy to discover, test, and debug without introducing an additional mediator abstraction.

## Alternatives Considered

- MediatR
- Custom mediator implementation

## Consequences

- Less abstraction
- Simpler debugging
- Explicit dependency registration

---

# ADR-004 — Explicit Dependency Injection

## Status

Accepted

## Decision

Register handlers and services explicitly.

## Rationale

Explicit registrations make dependencies visible and simplify troubleshooting.

Automatic assembly scanning was considered unnecessary for the current scale of the project.

## Alternatives Considered

Assembly scanning

## Consequences

- Clear registrations
- Predictable startup
- Easier maintenance

---

# ADR-005 — DbContext Usage

## Status

Accepted

## Decision

Application handlers may use RegOSDbContext directly.

## Rationale

Introducing repositories would add abstraction without providing sufficient value for the current architecture.

Entity Framework Core already provides a repository and unit-of-work pattern.

## Alternatives Considered

Repository pattern

## Consequences

- Less boilerplate
- Simpler implementation
- Easier query composition

---

# ADR-006 — Read Model Composition

## Status

Accepted

## Decision

Query handlers compose their own read models.

Query handlers should not invoke other query handlers.

## Rationale

Read model composition remains explicit and avoids unnecessary coupling between handlers.

## Consequences

- Predictable query flow
- Better performance
- Easier optimization

---

# ADR-007 — Module Ownership

## Status

Accepted

## Decision

Every business capability belongs to exactly one module.

## Rationale

Clear ownership prevents duplicated business logic and establishes well-defined architectural boundaries.

## Consequences

- Single source of truth
- Reduced duplication
- Clear maintenance responsibility

---

# ADR-008 — Composition Modules

## Status

Accepted

## Decision

Experience modules aggregate information but never own business state.

Examples include:

- Application Workspace
- Dashboard
- Reporting
- Analytics

## Rationale

Business rules remain within producer modules while composition modules focus exclusively on presenting information.

## Consequences

- Clear separation of concerns
- Simpler maintenance
- Flexible user experiences

---

# ADR-009 — Development Lifecycle

## Status

Accepted

## Decision

Every capability follows the same engineering lifecycle.

Capability

↓

Design

↓

Sprint

↓

Milestone

↓

Implementation

↓

Review

↓

Freeze

## Rationale

A consistent process improves predictability, reduces rework, and ensures architectural alignment.

## Consequences

- Higher implementation quality
- Repeatable delivery
- Better documentation

---

# ADR-010 — Documentation as Code

## Status

Accepted

## Decision

Architecture documentation evolves alongside the codebase.

Documentation updates are part of the Definition of Done.

## Rationale

Keeping documentation synchronized with implementation preserves architectural knowledge and supports onboarding.

## Consequences

- Living documentation
- Reduced knowledge loss
- Easier long-term maintenance

---

# Future Decisions

As RegOS evolves, additional decisions may be recorded, including:

- Event-driven architecture
- Background job processing
- Search infrastructure
- Caching strategy
- API versioning
- Multi-region deployment
- AI integration
- Plugin architecture
- External authority integrations

---

# Decision Process

New architectural decisions should follow this process.

1. Identify the problem.
2. Evaluate alternatives.
3. Record the proposed decision.
4. Review with stakeholders.
5. Approve or reject.
6. Update this document.

Architecture decisions should be made deliberately and documented before widespread implementation.

---

# Guiding Principle

Every significant technical decision should have a recorded rationale.

Future contributors should be able to understand not only _what_ the system does, but _why_ it was designed that way.
