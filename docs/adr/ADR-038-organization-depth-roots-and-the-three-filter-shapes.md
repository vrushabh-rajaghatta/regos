# ADR-038 — Sites, Contacts And Divisions Are Aggregate Roots, And Tenant Filtering Has Three Shapes

**Status:** Accepted · **Date:** 2026-07-31 ·
**Related:** [ADR-030](ADR-030-tenant-is-its-own-aggregate.md) (tenant separation),
[ADR-031](ADR-031-tenant-isolation-by-query-filters.md) (isolation enforcement),
[ADR-032](ADR-032-organizations-are-tenant-owned.md) (organizations are tenant-owned),
[ADR-018](ADR-018-rule-of-three.md) (rule of three),
[ADR-016](ADR-016-persistence-access-model.md) (persistence access model),
[ADR-037](ADR-037-registrations-are-regulatory-assets-with-derived-visibility.md) (derived visibility)

## Context

EPIC-016 gives an `Organization` the depth a regulatory party actually has. A
company is not a name and a type: it operates sites a regulator inspects, it
names people on filings, it is organised into business units, and it is known to
several registries by several identifiers at once.

Four questions had to be settled, and each of them is the question the *next*
context will face too:

1. **What becomes an aggregate root**, and what stays a child of `Organization`?
2. **How is each new entity scoped to a tenant?** RegOS had accumulated three
   different filter shapes without ever naming them, so every new entity was an
   independent re-derivation.
3. **Is an identifier scheme reference data or an enum?** The same question had
   already been answered twice, differently, for `RegistrationStatus` and
   `DocumentType`.
4. **When does a repeated shape get extracted?** Scheme-plus-value appeared
   twice in one epic.

## Decision

### 1. Three roots, for three different reasons

`OrganizationSite`, `Contact` and `OrganizationDivision` are aggregate roots.
They arrive at that conclusion by three distinct arguments, and recording the
arguments matters more than recording the conclusion — a future entity should
re-run the reasoning, not pattern-match on "the organization ones are all
roots".

| Aggregate | Why it is a root | Evidence |
|---|---|---|
| `OrganizationSite` | Users discover sites **independently of the company** — *"which manufacturing sites do we have in India?"* | `SiteDirectory`, shipped in the same commit |
| `Contact` | Users discover people independently — *"who is the QP for this application?"* | `ContactDirectory`, shipped in the same commit |
| `OrganizationDivision` | Other aggregates are **expected to hold stable references** to it | **None yet — see Revisit When** |

The first two are *verified*: the query ships beside the aggregate, so a wrong
justification shows up immediately as an empty directory. A root justified by a
query that does not exist yet is a demo of an empty table.

The third is a **prediction**, and is recorded as one. It is not weaker
engineering; it is a weaker *warrant*, and the difference is written down rather
than smoothed over.

### 2. Tenant filtering has three shapes, and choosing between them is a decision

Named here so that "which shape?" becomes an explicit question with three
answers rather than a guess. The authoritative list lives in `RegOSDbContext`'s
remarks, beside the filters themselves.

1. **Fail-closed tenant-owned** — `CurrentTenant != null && x.TenantId == CurrentTenant`.
   The tenant owns the data. Sites, contacts and divisions all take this shape.
2. **Shared plus extensible** — `CurrentTenant != null && (x.TenantId == null || x.TenantId == CurrentTenant)`.
   The platform ships a baseline the tenant may extend. `ContactRole` takes this
   shape: *Qualified Person* is defined by legislation, *APAC Regulatory Lead* is
   one company's own word for a job.
3. **Global world facts** — no filter. RegOS is describing an external reality
   that does not differ by tenant. `IdentifierScheme` takes this shape: a DUNS
   number is not tenant-specific.

Every filter opens with an explicit null guard. Without it, a null tenant
translates to `"TenantId" IS NULL`, which matches the platform rows rather than
nothing — "no identity" must mean *no rows*.

### 3. Vocabulary is data; things rules branch on are enums

`IdentifierScheme` and `ContactRole` are reference data. `OrganizationSiteType`
is an enum. The test is not "is it a short list of strings" but **does anything
branch on it**:

- Only a `Manufacturing` site can be named on a licence as an approved
  manufacturer. Code branches on the type, so the vocabulary lives in code.
- Nothing branches on *which registry* issued an identifier. Adding EU ORG-ID
  must not require a deployment.

This is the same test EPIC-005 applied to `RegistrationStatus`, stated once so
it stops being re-derived.

### 4. Scheme-plus-value is duplicated, deliberately

`SiteIdentifier` and `OrganizationIdentifier` are the same shape and share no
code. This is the **second** occurrence, and ADR-018 says wait: the third
consumer — likely the market-local product tier in EPIC-017 — may want a
different abstraction, and extracting now would guess at it. Both types carry a
breadcrumb naming the other.

The same restraint applies to the creation policies. `IOrganizationIdentifierPolicy`
is the fourth parallel policy in the codebase and still not the extraction
trigger, on the standard `IContactCreationPolicy` set: **the trigger fires when
two of them need the same non-trivial rule, not merely when another appears.**
What the four share is one line of "does this row exist".

### 5. Organizations are regulatory, and the UI says so

The organization UI lived under `/platform` since ADR-015, when an organization
*was* the tenant. ADR-030 superseded that model; the navigation had gone on
teaching it. Organizations, sites, contacts and divisions now live under
`/regulatory`, beside Products and Registrations. Tenants and Users stay in
Platform.

The tenant-wide directories are **siblings, not children**: `/regulatory/sites`
and `/regulatory/contacts`, on the same reasoning that put Registrations beside
Products. A directory that spans the registry is not scoped to one company.

## Consequences

**A new entity in this area now has a decision procedure**, not a precedent to
copy: does a user discover it independently (root), which filter shape (three
named options), does anything branch on its vocabulary (enum or data).

**Divisions carry an unverified justification** until EPIC-006. That is the cost
of shipping them now, and it is bounded by the Revisit When entry below.

**Sites, contacts and divisions are create-and-read only.** No deactivate or
update commands exist, so the workspace shows their status and does not offer to
change it. This is a deliberate MVP boundary: the UI reflects what the platform
can do rather than implying capability it would have to fake. Lifecycle over
deletion (ES-018) is modelled — `Status` and `StatusDate` are persisted — it is
simply not yet reachable.

**Two grandfathered exemptions were retired** while this context was open: the
four `/organizations` routes moved under `/api` (SC-001), and
`IOrganizationRepository` moved to the Domain project (SC-002). Neither list
grew.

**`GetByIdAsync` loads the identifiers.** `AddIdentifier` refuses a scheme the
company already holds and can only see what was loaded; a partial load would let
the duplicate reach the unique index, turning a stated business rule into a raw
persistence failure. An aggregate loaded for a write must be complete or it
cannot enforce its own invariants.

## Revisit When

- **EPIC-006 ships without any aggregate holding an `OrganizationDivisionId`.**
  The justification for that root did not materialise — collapse divisions into
  `Organization` or restate the justification. Do not leave it standing on the
  original expectation. *This entry is deliberately **absence-shaped**: every
  other `Revisit When` entry in this series fires when something arrives, and a
  prediction decays when something fails to arrive. Nothing else would catch it.*
- A third consumer needs scheme-plus-value — that triggers extracting the shared
  identifier type (ADR-018), and the retrofit of `SiteIdentifier` and
  `OrganizationIdentifier`.
- Two creation policies need the same non-trivial rule — not merely a fifth
  policy appearing.
- A customer needs a private identifier scheme. That moves `IdentifierScheme`
  from shape 3 to shape 2: a nullable `TenantId` and one filter, with the seeded
  rows keeping a null tenant.
- Sites or contacts need to be deactivated or corrected. That is a real story,
  not an oversight, and it arrives with commands rather than with UI.
