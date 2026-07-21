# ADR-030 — Tenant Is Its Own Aggregate, Separate From Organization

**Status:** Accepted · **Date:** 2026-07-21 ·
**Supersedes:** [ADR-015](ADR-015-organization-is-the-tenant.md) ·
**Related:** [ADR-013](ADR-013-ambient-tenant-context.md) (ambient tenant context),
[ADR-017](ADR-017-shared-kernel-scope.md) (shared kernel scope),
[ADR-024](ADR-024-tenancy-is-derived-from-identity.md) (tenancy from identity),
[ADR-031](ADR-031-tenant-isolation-by-query-filters.md) (isolation enforcement)

## Context

ADR-015 made the organization the tenant and was honest about the cost: the
aggregate carried a regulatory taxonomy (`OrganizationType`) that a generic
tenancy attribute has no business having, and its own Revisit-When list named
the trigger — *the regulatory attributes grow enough that separating a generic
tenant from a regulatory party becomes cheaper than keeping them fused*.

The trigger arrived from the product direction rather than the code: in a RIM
system an *organization* is a regulatory party — an applicant, a manufacturer,
a marketing authorization holder, eventually with sites, addresses and external
identifiers (DUNS, SPOR ORG-ID) — that a **tenant records and manages**. Every
tenant's own company is an organization; almost no organization is a tenant.
The fused model made that containment relationship an identity, and every new
regulatory attribute would have deepened the fusion.

Two implementation facts sharpened the choice:

- `RegulatoryApplication.ApplicantOrganizationId` is genuine business data
  (ADR-013): renaming `Organization` to `Tenant` would have renamed the
  applicant into a lie — "only our own customers can be applicants" — and
  burned the word `Organization` right before the registry feature needs it.
- The multi-tenancy work (platform users, isolation, the regulatory domain
  gaining tenant columns) needed a clean tenant *now*, and none of it needs
  the organization registry at all.

## Decision

**A new `Tenant` aggregate, beside `Organization`, not instead of it.**

1. `Tenant` lives in the Platform context: `Id`, `Name`, `Status` — and no
   `Type`. A regulatory taxonomy describes a party, not a customer account.
   (`src/Organization/` existed outside Platform precisely because it carried
   regulatory concepts; a clean tenant carries none, so it belongs in
   Platform.)
2. **`TenantId` lives in the shared kernel**, beside `ITenantContext`, and
   `ITenantContext.TenantId` is strongly typed. The bare `Guid` existed so the
   kernel would not depend on the Organization context's id; with a kernel-owned
   id the reason is gone, and the per-boundary
   `new OrganizationId(tenantContext.TenantId)` seam is deleted from every
   handler. This widens ADR-017's kernel by one type, deliberately: the tenant
   is an infrastructure concept every context shares, which is what the kernel
   is for.
3. Every *tenant-key* usage of `OrganizationId` repoints to `TenantId`:
   `User.TenantId`, `Product.TenantId`, `DocumentType.TenantId?`, the JWT claim
   (`regos:organization_id` → `regos:tenant_id`), `ICurrentUser`,
   `CurrentUserResponse`. The Platform, Product and ReferenceData domain
   projects drop their reference to the Organization context entirely.
4. `RegulatoryApplication.ApplicantOrganizationId` **stays pointing at
   `Organizations`**. Owner (`TenantId`, ambient) and applicant
   (`ApplicantOrganizationId`, explicit) are now two visibly different concepts
   on the same record — a tenant can file on behalf of a partner.
5. The `AddTenants` migration backfills `Tenants` from `Organizations`
   **preserving ids**, so every existing row and credential keeps working, and
   the seeded demo tenants share guids with the demo organizations of the same
   names.
6. A user may belong to the platform instead of a tenant: `User.TenantId` is
   nullable, enforced per factory (`CreateForTenant` rejects null;
   `CreatePlatformUser` is the only path to it), and a platform user's token
   carries no tenant claim — absent, not empty. Cross-tenant *power* is
   deliberately not granted anywhere yet; that is the authorization slice's
   decision to make.

`Organization` is left deliberately unfinished: `LegalName`, `Type`, `Status`,
nothing else. It becomes the RIM party registry (multi-role, sites, external
identifiers) when the first feature needs one — not before.

## Consequences

**Positive**

- The schema now states the separation instead of a doc asserting it: user,
  product and regulatory FKs point at `Tenants`; the applicant FK points at
  `Organizations`.
- The word `Organization` is preserved for what regulators, IDMP and every RIM
  user mean by it.
- The conversion seam and three cross-context project references are gone.

**Negative**

- Two similar-looking tables until the registry is real. The discriminator is
  crisp and recorded on `ITenantContext`: ambient and about the caller →
  `Tenant`; explicit and about the record → `Organization`.
- Renaming the claim invalidated every issued token — accepted while the only
  accounts were the development seeders'.
- The tenant's own company will eventually exist in both tables; a nullable
  `Tenant.OrganizationId` link is deferred until a feature needs it.

## Interim rule carried by this ADR

Until organizations belong to a tenant, the only organization a caller may
mutate is the one sharing their tenant's id (its pre-split alter ego). Before
this, any authenticated user could rename or deactivate any customer's
organization by guid. Reads stay global by design — the directory is shared.

## Revisit When

- The organization registry feature arrives (multi-role parties, sites,
  external identifiers) — that work reshapes `Organization` and adds the
  `Tenant.OrganizationId` link.
- A tenant needs multiple workspaces or regions.
- Cross-tenant filing arrangements need more than an applicant id (contracts,
  delegation).
