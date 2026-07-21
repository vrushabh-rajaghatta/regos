# ADR-013 — Tenant Context Is Ambient, Never a Command Property

**Status:** Accepted · **Date:** 2026-07-20 (retro-documented) ·
**Implemented by:** commit `0af343e` (ARCH-002) ·
**Related:** ADR-015 (organization is the tenant),
[ADR-024](ADR-024-tenancy-is-derived-from-identity.md) (tenancy from identity) ·
**Detail:** [`docs/architecture/tenant-inventory.md`](../architecture/tenant-inventory.md)

> **Retro-documented.** Shipped during ARCH-002 and documented in
> `tenant-inventory.md`, but never recorded as an ADR. It was referred to as
> "ADR-008 (tenant context)" in `ADR-009-command-validation-model.md`, written
> against a parallel numbering series. ADR-008 in the canonical series is
> Composition Modules, so this decision takes the next free number. **Any
> reference to "ADR-008 (tenant context)" means this document.**

> **The implementation described below is gone.** `HeaderTenantContext` and
> `X-Tenant-Id` were deleted by
> [ADR-024](ADR-024-tenancy-is-derived-from-identity.md); the tenant now comes
> from the authenticated caller's claim. The *decision* in this ADR — that
> tenant context is ambient and never a command property — still stands, and
> `ITenantContext` is unchanged. Read the header sections as history.

## Context

Platform commands and queries previously accepted an `organizationId` parameter.
Omitting it disabled tenant scoping entirely, which made isolation an
authorization check that could be forgotten.

The inventory in `tenant-inventory.md` separates two things that both appeared
as `OrganizationId`:

| | Tenant | Organization |
|---|---|---|
| Nature | Infrastructure | Domain |
| Question | Who is asking? | Who is this record about? |
| How it travels | Ambient — `ITenantContext` | Explicit — a command property |

## Decision

Tenant identity is ambient. It is resolved from `ITenantContext` and never
appears as a command or query property.

Where an organization id is genuinely *business data* — such as
`RegulatoryApplication.ApplicantOrganizationId`, which names the company
applying rather than the caller — it remains an explicit property. The two are
not the same concept and are not interchangeable.

`ITenantContext.TenantId` is a bare `Guid`; each bounded context converts it to
its own strongly-typed id at its boundary, so the abstraction does not assume a
tenant will always be an organization.

## Consequences

- Inviting a user into another tenant is not a check that could be forgotten —
  it is unexpressible. See `InviteUserHandler.cs:37`.
- A missing or malformed tenant throws; no header value produces an unscoped
  query.
- Resolution is lazy, so endpoints that are not tenant-scoped (reference data)
  never require a tenant.

## Scope Limit — Read This Before Citing It

This decision made Platform's isolation mandatory. **It did not make RegOS
tenant-isolated.** Product, ProductDocument, RegulatoryApplication and Submission
have no tenant concept at all — `grep` for `OrganizationId` or `TenantId` across
those source trees returns nothing.

`HeaderTenantContext` reads `X-Tenant-Id` and is a development mechanism: it
decides *which* tenant a request is scoped to and never establishes that the
caller is entitled to that tenant. It is replaced by a claims-based
implementation when Epic 3 lands; nothing above it changes.

## Revisit When

- A tenant stops being an organization — multi-workspace or multi-region tenancy.
- Authentication lands and `HeaderTenantContext` is replaced (mechanism change,
  not a decision change).
