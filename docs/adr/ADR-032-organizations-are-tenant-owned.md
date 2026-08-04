# ADR-032 — Organizations Are Tenant-Owned, Not Shared

**Status:** Accepted · **Date:** 2026-07-21 ·
**Amends:** [ADR-031](ADR-031-tenant-isolation-by-query-filters.md) (tier
classification) ·
**Amended by:** [ADR-060](ADR-060-a-tenant-provisions-without-an-organization.md)
— the *mirror entry* below is retired; a tenant now provisions with an empty
registry. Everything else in this ADR stands. ·
**Related:** [ADR-030](ADR-030-tenant-is-its-own-aggregate.md) (tenant split),
[ADR-013](ADR-013-ambient-tenant-context.md) (ambient tenant context)

## Context

ADR-031 classified `Organizations` as a global directory, alongside `Tenants`.
That classification inherited fused-model reasoning: when an organization *was*
a tenant, scoping the directory to the caller would have reduced it to one row.

The classification was wrong on two counts:

1. **Organizations are not world facts.** Countries and authorities are the
   same for everyone; a tenant's organizations are its business relationships —
   applicants, manufacturers, partners. The org CRUD UI is live, so these are
   customer-entered rows, and a shared directory shows every tenant who else
   uses RegOS and who they work with. Even the *names* are competitive
   intelligence.
2. **There is no tier-2 case either.** System document types are genuine
   platform content; there is no standard set of organizations the platform
   should ship. Regulators live in `Authorities`. A "platform-provided
   organization" has no production meaning.

## Decision

**Each tenant owns its organization registry.** `Organization.TenantId` is
required, stamped ambiently by the create handler (ADR-013), and enforced by
the same fail-closed query filter as every tenant-owned aggregate. Another
tenant's organization is indistinguishable from one that does not exist — for
reads and writes alike, which retires the interim write guard ADR-030 carried.

**The mirror entry.** Every tenant's own company is the first organization in
its registry, created with the *same guid as the tenant*:

- The `MakeOrganizationsTenantOwned` backfill sets `TenantId = Id` for every
  existing row — correct universally because `AddTenants` gave every
  organization an alter-ego tenant.
- When runtime tenant creation lands (platform-admin slice), the
  `CreateTenant` handler creates the mirror organization in the same unit of
  work — handler orchestration, per house style: no domain events, no
  triggers, no interceptors for business rules.
- The shared guid is a **stated convention**: the organization whose id equals
  the owning tenant's id is the tenant's own company. It gives the UI a free
  "your organization" default without a link column. A `Tenant.OrganizationId`
  link arrives only if a feature outgrows the convention.
- The mirror's `OrganizationType` is an **input to `CreateTenant`** — the
  platform admin creating the customer says what they are. It is not guessed.

**A consequence worth naming:** the applicant on a regulatory application must
now exist *in the caller's own registry* — the creation policy's existence
check runs through the filtered context. You cannot file on behalf of a
company you have not recorded. That is correct RIM behaviour, not a side
effect.

## Consequences

**Positive**

- No tenant can enumerate RegOS's customers or their business relationships.
- The registry model matches where the product is going: when `Site` arrives,
  a tenant's organizations and their sites are already private to it.
- Three write handlers got simpler: the filter's null answers ownership.

**Negative — accepted knowingly**

- **Duplicate rows across tenants.** Two tenants working with the same
  real-world company each record it separately, and nothing deduplicates.
  This is isolation's price. Shared master-data reconciliation (SPOR sync)
  is a future feature, not a reason to share rows today.
- Nobody can see another tenant's registry — including support scenarios.
  The platform-admin slice decides what cross-tenant visibility exists and
  audits it.
- The seeded demo trio is now visible only to its own demo tenant; the dev
  applicant dropdown shows one organization until more are added, which is
  the intended tenant experience.

## Revisit When

- The organization registry feature arrives (sites, multi-role, external
  identifiers) — it builds *on* this ownership model.
- A shared-reference reconciliation feature (SPOR/OMS sync) needs a curated
  cross-tenant organization source; that would be a new tier-2-like decision
  taken explicitly, not a relaxation of this one.
- The shared-guid mirror convention needs to survive an operation that
  changes ids (tenant merge/split) — introduce the explicit link then.
