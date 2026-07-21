# Tenant Scoping Inventory

Produced during ARCH-002. Records where tenancy exists in RegOS today, what
every `OrganizationId` in the solution actually means, and what remains to be
migrated.

Verified against the codebase and against a running API on 2026-07-20.

## The distinction this inventory turns on

| | Tenant | Organization |
|---|---|---|
| Nature | Infrastructure | Domain |
| Question it answers | Who is asking? | Who is this record about? |
| How it travels | Ambient — `ITenantContext`, from the caller's token claim | Explicit — a command property |
| Appears in commands | Never | Where the ubiquitous language says so |

They coincide today: a tenant *is* an organization. The abstraction does not
assume that stays true, which is why `ITenantContext.TenantId` is a bare `Guid`
and each bounded context converts at its own boundary.

## Classification of every `OrganizationId`

| Location | Meaning | Status |
|---|---|---|
| `Platform` — user commands and queries | **Tenant** | ✅ Migrated. Removed from all commands/queries; resolved from `ITenantContext`. |
| `Platform` — `User.OrganizationId` | **Tenant**, persisted | ✅ Correct as-is. The column is how a user is bound to its tenant. |
| `RegulatoryApplication` — `ApplicantOrganizationId` | **Applicant** (business data) | ✅ Correct as-is. Stays an explicit command property; it names the company applying, not the caller. |
| `Organization` context | The aggregate itself | ✅ Not tenancy. |
| `ReferenceData`, `Persistence` config/migrations | Schema and seed data | ✅ Not tenancy. |

No occurrence was ambiguous. The only *tenant* usages in the solution were in
Platform, and they are now ambient.

## Tenant scoping across the API

28 routes. The picture is not "some endpoints have optional scoping" — it is
that only one module had any tenant concept at all.

| Module | Tenant scoping before ARCH-002 | After |
|---|---|---|
| Platform (6 routes) | Optional — omitting `?organizationId=` disabled it | **Mandatory** |
| Product (3 routes) | **None** | None |
| ProductDocument (5 routes) | **None** | None |
| RegulatoryApplication (2 routes) | **None** | None |
| Submission (7 routes) | **None** | None |
| Reference/master data (5 routes) | N/A — global by design | N/A |

`grep` for `OrganizationId`, `organizationId` or `TenantId` across
`src/Product`, `src/Submission` and `src/ProductDocument` returns **nothing**.
These modules have no tenant concept to make mandatory.

### The specific consequence

`ListRegulatoryApplicationsHandler` joins to `Organizations` only to display the
applicant's name. It applies no tenant filter, so it returns every regulatory
application in the database regardless of who is asking. The same is true of the
product, document and submission read paths.

**ARCH-002 makes Platform's isolation mandatory. It does not make the regulatory
domain multi-tenant, because that domain has never been tenant-aware at all.**
Anyone reading "no endpoint where tenant scoping is optional" as "the system is
tenant-isolated" would be wrong.

## Migration plan for the remaining contexts

Deferred deliberately — each needs a domain decision, not a mechanical change.

1. **Decide what owns a Product.** A product is a company's product, so it
   plausibly has an owning organization; that is a modelling decision, not a
   plumbing one, and it belongs to the Product bounded context work.
2. **RegulatoryApplication** already stores `ApplicantOrganizationId`. Whether
   the tenant filter is `ApplicantOrganizationId == tenant` or something
   involving the product's owner depends on (1).
3. **Submission and ProductDocument** inherit their scope from their parents
   (application, product respectively) and should be filtered through those
   rather than gaining their own organization column.

Recommended sequence: resolve (1) as part of the Product bounded context, then
(2) and (3) follow from it.

## Claims-based resolution

`ClaimsTenantContext` returns `ICurrentUser.OrganizationId` — the organization
claim from a signature-checked access token. The caller does not choose the
tenant; they prove it ([ADR-024](../adr/ADR-024-tenancy-is-derived-from-identity.md)).

This replaced `HeaderTenantContext`, which read `X-Tenant-Id` and therefore let
any caller name any tenant. That implementation is deleted, not disabled: there
is no configuration under which RegOS reads a tenant from a request header.
**Nothing above `ITenantContext` changed** — all fourteen consumers were
untouched, which is what the abstraction was introduced for.

Resolution is lazy: the claim is read when `TenantId` is first accessed, so
endpoints that are not tenant-scoped (reference data, master data) never force
it. An unauthenticated caller throws a 401 — there is no value that produces an
unscoped query.
