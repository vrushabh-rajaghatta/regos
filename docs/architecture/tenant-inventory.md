# Tenant Scoping Inventory

Produced during ARCH-002, rewritten after ADR-030/ADR-031 (2026-07-21).
Records where tenancy exists in RegOS, what every tenant-shaped identifier
means, and what is deliberately outside the tenant boundary.

When this document and the codebase disagree, the codebase wins — and the
architecture tests in
`tests/Platform/RegOS.Platform.Application.Tests/Tenancy/` are the enforced
version of the tables below.

## The distinction this inventory turns on

| | Tenant | Organization |
|---|---|---|
| Nature | Infrastructure | Domain |
| Question it answers | Who is asking? | Who is this record about? |
| How it travels | Ambient — `ITenantContext`, from the caller's token claim | Explicit — a command property |
| Appears in commands | Never | Where the ubiquitous language says so |
| Type | `TenantId` (shared kernel) | `OrganizationId` (Organization context) |

Since ADR-030 they no longer coincide: `Tenant` is its own Platform aggregate
with its own table, and `Organization` is the (deliberately unfinished)
regulatory party. The two directories share seeded guids because the
`AddTenants` migration backfilled one from the other preserving ids.

## Classification of every tenant-shaped identifier

| Location | Meaning | Status |
|---|---|---|
| `SharedKernel` — `TenantId`, `ITenantContext` | The tenant concept itself | Kernel-owned since ADR-030; `TenantIdOrNull` exists only for the query filters |
| `Platform` — `User.TenantId` (nullable) | **Tenant**, persisted | Null ⇒ platform user; nullability enforced per factory, never optional on `CreateForTenant` |
| `Platform` — `Tenant` aggregate | The tenant directory | Global by definition; no filter |
| `Product.TenantId` | **Tenant**, persisted | Filtered |
| `ReferenceData` — `DocumentType.TenantId?` | Tier-2 discriminator | Null ⇒ system type visible to all tenants; value ⇒ tenant extension |
| `RegulatoryApplication.TenantId` | **Tenant** (owner) | Filtered; stamped from `ITenantContext` at creation |
| `RegulatoryApplication.ApplicantOrganizationId` | **Applicant** (business data) | Explicit command property, FK → `Organizations`. Names who the filing is for, not who owns the record. Since ADR-032 the applicant must exist in the caller's own registry |
| `Submission.TenantId`, `SubmissionSnapshot.TenantId`, `ProductDocument.TenantId` | **Tenant**, derived from parent | Filtered; a child structurally cannot disagree with its parent's tenant |
| `Organization.TenantId` | **Tenant** (registry owner) | Filtered (ADR-032). Stamped from `ITenantContext`; every entry is recorded by the tenant itself, provisioning creates none (ADR-060) |

## Enforcement (ADR-031)

Isolation is a property of `RegOSDbContext`, not of handler discipline: every
tenant-owned entity has a fail-closed `HasQueryFilter` (no identity ⇒ no
rows), including the `ToView` read models. The bypass surface is named and
small: `IUserRepository` (identity-scoped), `UserPolicy` email uniqueness
(ADR-021), and the startup initializers' guards.

## Deliberately outside the tenant boundary

| Tables | Why |
|---|---|
| `Tenants` | The global directory (the one table global by definition) |
| `Countries`, `Authorities`, `ApplicationTypes` | World facts (tier 1) — a tenant column here is a modelling error the architecture tests reject |
| `UserCredentials`, `RefreshTokens`, `Invitations`, `PasswordResets`, `Sessions` | Person-scoped (ADR-029); reachable only by user id or token hash |
| `SubmissionDocuments`, `DocumentVersions`, `SnapshotDocuments` | Children reachable only through a filtered root |

## Still open

- Postgres RLS as defence in depth, now unblocked: every table carries its
  tenant column and every path respects it.
- Tenant-scoped uniqueness for `DocumentTypes.Code`, with the extensions
  feature.
- Role management (promote/demote) — ADR-033 defers it; roles are assigned
  only at creation today.

Platform-admin cross-tenant access is settled (ADR-033): policy-guarded
`IgnoreQueryFilters` in `GetTenantUsersHandler`, one named tenant per request.
