# RegOS Foundation — Principles and Invariants

Two different kinds of rule live here, and conflating them is why architecture
documents lose authority.

- **Principles** are judgement calls. They guide a decision; they can be
  outweighed by a good reason.
- **Invariants** are not negotiable. Breaking one is a defect, not a trade-off.
  An invariant changes only by a new ADR.

Each invariant is labelled with its enforcement status, because an invariant
nobody enforces is a wish:

| Label | Meaning |
|---|---|
| **Enforced** | The codebase upholds it today; a violation would be visible. |
| **Target (v1)** | Accepted and binding on new code, but existing code violates it. |

---

## Principles

### P1 — The Foundation minimizes knowledge of regulatory concepts

The Foundation should be usable by any business module in RegOS.

It is stated as *minimize*, not *contain none*, because the code already
contains one: `OrganizationType` is a regulatory taxonomy
([ADR-015](../adr/ADR-015-organization-is-the-tenant.md)). Where regulatory
concerns exist inside foundational modules, they are implementation decisions
rather than architectural goals, and may be revisited as the domain evolves.

A stricter claim would be false on the day it was written.

### P2 — Behavior before data

An aggregate owns behavior. A type that only holds values is a value object, or
stays embedded in its parent until it earns an independent lifecycle.

Practical consequence: organization settings such as time zone and culture are a
value object on `Organization`, not a second aggregate. They have no lifecycle
and no invariant of their own.

### P3 — Convention over configuration

Prefer a sensible default over a setting. Configurability is added when a
business requirement demands it, not in anticipation of one.

### P4 — Build what the current product needs

Foundation abstractions are extracted when a second consumer appears, not
designed in advance of the first. `RegOS.SharedKernel` was built when `Product`
needed it — that is the pattern.

Speculative foundation work is the failure mode this principle exists to
prevent: an `ICurrentOrganization` designed before tenancy exists will be
designed wrong.

### P5 — The code is the source of truth until an ADR changes it

When a document and the codebase disagree, the codebase is the current
architecture and the document is stale. Documents describe; ADRs decide.

A preferred design that is not implemented is a **Proposed** ADR, never a
description of the system.

---

## Invariants

### I1 — The Foundation never depends on a Regulatory module — **Enforced**

`src/Organization` and `src/Platform` must not reference `Product`,
`ProductDocument`, `RegulatoryApplication` or `Submission`.

Dependency flows one way. This is what makes the Foundation reusable.

### I2 — Domain projects depend on no infrastructure — **Enforced**

A `*.Domain` project references no EF Core, no ASP.NET, no HTTP. Interfaces are
declared in Domain or Application; implementations live in Infrastructure
([ADR-016](../adr/ADR-016-persistence-access-model.md)).

### I3 — Tenant identity is ambient, never a parameter — **Enforced**

Tenant is resolved from `ITenantContext`. It never appears as a command or query
property ([ADR-013](../adr/ADR-013-ambient-tenant-context.md)).

An organization id that is genuinely business data — `ApplicantOrganizationId`,
which names the company applying rather than the caller — is a different concept
and remains explicit. If you cannot tell which you have, ask whether it answers
*who is asking* or *who is this record about*.

This invariant is what makes cross-tenant writes unexpressible rather than
merely forbidden: `InviteUserHandler` cannot invite into another organization
because there is no parameter with which to say so.

### I4 — Every tenant-scoped query is filtered by the current organization — **Target (v1)**

**Enforced in Platform. Not implemented anywhere else.** Product,
ProductDocument, RegulatoryApplication and Submission have no tenant concept;
their read paths return every row in the database.

Binding on all new code. Closing it for existing code is Epic 1 roadmap work and
requires a domain decision per context, not a mechanical change — see
[`tenant-inventory.md`](../architecture/tenant-inventory.md).

### I5 — Command rejection follows the ADR-009 decision tree — **Enforced**

Whether a rejection is 400, 404 or 409 is decided by
[ADR-009](../adr/ADR-009-command-validation-model.md), not by handler-author
preference. The three exception types in
[ADR-012](../adr/ADR-012-shared-semantic-exception-model.md) are the only
vocabulary.

### I6 — Authorization is enforced in the Application layer — **Target (v1)**

**Nothing enforces authorization today; Epic 4 has not started.**

Recorded now because it constrains how Epic 4 is built. Endpoint-level
attributes may *additionally* guard a route, but they are not the enforcement
point: a command must be safe when invoked from a background job, a test, or a
second transport that has no endpoint at all.

This resolves a tension in the original Epic 4 sketch, which said "authorization
policies protect endpoints" — endpoints are the outermost check, not the only
one.

### I7 — Cross-tenant access is explicit and auditable — **Target (v1)**

There is no legitimate implicit path to another tenant's data. If a platform-wide
operation is ever needed, it is named as such, authorized separately, and
recorded in the audit log.

No such operation exists today, and none should be added before Epic 5 can
record it.

### I8 — ADR numbers are immutable — **Enforced**

Once assigned, an ADR number never changes meaning. Reversing a decision creates
a new ADR that supersedes the old one. See
[`docs/adr/README.md`](../adr/README.md).

Source code cites ADR numbers; renumbering silently invalidates those comments.

---

## When an invariant is violated

1. If the code is right and the invariant is wrong, write an ADR superseding it.
2. If the invariant is right and the code is wrong, it is a defect — fix it or
   record it in `ARCHITECTURE_BACKLOG.md` with the gap named.
3. Do not quietly downgrade an invariant to a principle. That is how I4 would
   disappear.
