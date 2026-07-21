# ADR-031 — Tenant Isolation Is Enforced By Global Query Filters

**Status:** Accepted · **Date:** 2026-07-21 ·
**Related:** [ADR-021](ADR-021-email-is-globally-unique.md) (global email),
[ADR-024](ADR-024-tenancy-is-derived-from-identity.md) (tenancy from identity),
[ADR-029](ADR-029-sessions-record-minimal-device-context.md) (person-scoped sessions),
[ADR-030](ADR-030-tenant-is-its-own-aggregate.md) (tenant aggregate)

## Context

Until this decision, isolation was a per-handler convention: every query
handler wrote its own `.Where(x => x.TenantId == tenantId)`, every write
handler its own ownership check. The discipline was real — and incomplete. The
regulatory contexts had no tenant concept at all, `ListRegulatoryApplications`
returned every row in the database to any caller, and the failure mode of a
forgotten clause was a silent cross-tenant leak.

`tenant-inventory.md` was blunt about it: ARCH-002 made Platform's isolation
mandatory; it never made the regulatory domain multi-tenant.

## Decision

Four parts, layered:

**1. Every tenant-owned aggregate carries a `TenantId`.** `User` (nullable —
platform users), `Product`, `DocumentType` (nullable — system types),
`RegulatoryApplication`, `Submission`, `SubmissionSnapshot`, `ProductDocument`.
Where a parent exists, the tenant **derives from the parent** at creation —
submission from application, snapshot from submission, document from product —
so a child structurally cannot disagree with what it belongs to. Only the top
of a hierarchy reads `ITenantContext`. Child entities reachable solely through
a filtered root (`SubmissionDocuments`, `DocumentVersions`,
`SnapshotDocuments`) carry no tenant column.

**2. Isolation is enforced once, in `RegOSDbContext`, by `HasQueryFilter`.**
A handler that forgets its `.Where` now returns the caller's rows, not
everyone's. Every filter uses the explicit **fail-closed shape**:

```csharp
x => CurrentTenant != null && x.TenantId == CurrentTenant
```

never bare equality. The guard is load-bearing: with `Users.TenantId` nullable,
a null tenant under SQL null semantics would translate to
`"TenantId" IS NULL` — which matches exactly the platform users. Fail-closed
means no identity → **no rows**. The filters read
`ITenantContext.TenantIdOrNull`, the one lenient accessor, added for exactly
this consumer; handlers keep using `TenantId`, which throws. The `ToView` read
models get their own filters — aggregate filters do not propagate to a
different CLR type mapped over the same table.

**3. What is deliberately unfiltered, by tier.** Reference data has three
tiers, and each is a modelling statement:

| Tier | Tables | Tenant column | Filter |
|---|---|---|---|
| World facts | `Countries`, `Authorities`, `SubmissionTypes` | none | none |
| Extensible taxonomies | `DocumentTypes` | nullable | `null or mine` |
| Tenant-owned | everything in part 1 | required (Users: nullable) | fail-closed |

`Tenants` and `Organizations` are global directories. The person-scoped
satellites (`UserCredentials`, `RefreshTokens`, `Invitations`,
`PasswordResets`, `Sessions`) carry no tenant: they belong to a person
(ADR-029), are reachable only by user id or unguessable token hash, and
filtering them breaks authentication itself.

**4. The bypass surface is two places, both named.**
`IUserRepository` is *identity-scoped*: its implementation applies
`IgnoreQueryFilters()` because every caller passes an identity it already owns
— an id from a signed token or consumable grant, or an email at the two doors
where no tenant exists yet (sign-in, reset request; ADR-021 exists for them).
`UserPolicy`'s email-uniqueness checks bypass for the same ADR-021 reason, and
the startup initializers' `AnyAsync` guards run before any request exists.
Tenant-scoped user access goes through the query handlers and
`GetRequiredAsync`, which checks ownership explicitly. Anything else calling
`IgnoreQueryFilters` should fail review.

## Enforcement of the enforcement

Architecture tests walk the EF model: an entity with a `TenantId` and no
filter fails the build, as does a tier-1 table that grows a tenant column
(the "fixed shared reference data by copying it per tenant" mistake).
Isolation tests prove the filter alone isolates, with deliberately bare
queries; the fail-closed test pins the null-semantics behaviour.

## Consequences

**Positive**

- Forgetting a `.Where` is no longer a leak. The existing manual clauses stay
  as belt-and-braces, but nothing depends on them.
- The regulatory domain is tenant-isolated for the first time.
- Backfilled by the same parent-derivation joins the handlers use; a row whose
  join finds nothing keeps the all-zero guid, which matches no caller ever.

**Negative**

- Tests that construct `RegOSDbContext` directly must now decide their tenant;
  a bare context sees nothing. This is the feature, observed from a test.
- Every filtered query gains a `WHERE` conjunct. All tenant columns are
  indexed; no measured cost yet.
- `IgnoreQueryFilters` exists and can be misused. The architecture tests
  cannot catch a misuse, only review can — hence the named-bypass rule.

## Deferred, deliberately

- **Postgres row-level security.** The stronger guarantee (covers raw SQL and
  a compromised application layer). Its cost concentrates in login, refresh
  and seeding; adopting it after every table carries `TenantId` and every
  path respects it turns the policies into copy-paste. The fail-closed
  session-variable pattern is recorded in the planning discussion; nothing in
  this design blocks it.
- **Platform-admin cross-tenant access** — the authorization slice decides
  how `IgnoreQueryFilters` is granted, to whom, and how it is audited.
- **Tenant-scoped uniqueness for `DocumentTypes.Code`** — the second partial
  index lands with the tenant-extensions feature.

## Revisit When

- RLS is adopted (this ADR's filters remain as defence in depth).
- A read model needs child tables directly (they would then need tenant
  columns or a join through their root).
- Any feature needs a legitimate cross-tenant read beyond the platform-admin
  slice.
