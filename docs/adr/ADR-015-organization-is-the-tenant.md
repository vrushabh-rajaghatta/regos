# ADR-015 — Organization Is the Tenant

**Status:** Accepted · **Date:** 2026-07-20 (retro-documented) ·
**Related:** ADR-013 (ambient tenant context)

> **Retro-documented.** Implicit in the codebase since tenancy was introduced;
> recorded here so it can be cited and, eventually, challenged.

## Context

RegOS is a multi-tenant SaaS platform. Every regulatory artifact ultimately
belongs to a customer. A separate `Tenant` abstraction alongside `Organization`
would duplicate the concept without solving a current problem.

## Decision

The organization is the tenant. `User.OrganizationId` is how a user is bound to
its tenant, and `ITenantContext.TenantId` resolves to an organization id.

No separate `Tenant` aggregate exists.

The coupling is deliberately loose: `ITenantContext.TenantId` is a bare `Guid`,
not an `OrganizationId`, so each bounded context converts at its own boundary
(ADR-013). If a tenant ever stops being an organization, the abstraction does not
have to be rebuilt.

## Consequences

**Positive**

- One concept instead of two. Simpler model, less to explain.
- No join between tenant and organization on every scoped query.

**Negative**

- `Organization` carries both business and tenancy responsibilities.
- **The aggregate already contains regulatory-domain concepts.**
  `OrganizationType` is `Manufacturer | Sponsor | MarketingAuthorizationHolder |
  ContractResearchOrganization` — a regulatory taxonomy, not a generic tenancy
  attribute. Any claim that the tenancy model is domain-neutral is false today.
  This is also why the module lives at `src/Organization/` rather than inside
  `src/Platform/`.

## Revisit When

- One organization requires multiple isolated workspaces.
- Cross-organization hierarchies (parent company / subsidiary) become a
  requirement.
- Multi-region tenancy becomes necessary.
- The regulatory attributes on `Organization` grow enough that separating a
  generic tenant from a regulatory party becomes cheaper than keeping them
  fused.
