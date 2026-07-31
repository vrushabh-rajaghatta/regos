# EPIC-016 — Organization depth: sites, contacts, divisions

**Status:** 🟡 In Progress · **Branch:** `epic/EPIC-016-organization-depth` (cut at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

RegOS knows *that* a partner exists. It does not know **where they operate** or **who to talk to**. This adds the two things almost every other regulatory object needs to point at.

> **Phases 1–2 are settled** (Phase 2 approved 2026-07-31 — see *Phase 2 rulings* below, which amend the original sketch in three places). Phase 3 is the approved four-story slice. See [RIM alignment](../BACKLOG.md#rim-alignment) for why this epic is first.

---

## Phase 1 — Epic plan

### Outcome
A regulatory user can record a partner organization's **sites** (manufacturing, testing, packaging, HA offices — with real addresses) and its **named contacts** with roles, and can find them the way the work actually asks: *"which manufacturing sites do we have in India?"*, *"who is the QP for this application?"* Licences can then name approved sites, and EPIC-006 has someone to correspond with.

### The concept it introduces

| | Means | Status |
|---|---|---|
| **Organization** | *who* — a legal entity we do business with | exists (shallow) |
| **Organization Site** | *where* — a physical location that entity operates | **new** |
| **Contact** | *who, specifically* — a named person with a role | **new** |
| **Organization Division** | *which part* — a business unit within the entity | **new** |

### Why this is the first RIM-alignment epic

Measured against the DIA RIM object model, `Organization Site` and `Contact` are the **most-referenced missing objects after Medicinal Product**:

| Missing object | Referenced by (RIM) |
|---|---|
| **Organization Site** | License (Approved Manufacturing sites; Contributors/Responsible Third parties), Application (Master File Location/PSMF), Mfg Business Operation, Ingredient (Manufacturing Source), Packaging (Manufacturer), Packaged Product (Manufacturer) |
| **Contact** | License (Responsible Contacts), Application (HA Reviewer Names, Sponsor Contributors, QP Contacts), Submission Role, Organization Site |
| **Organization Division** | Application (HA Division), License (HA Division), HA Meeting (HA Division) |

It is also **structurally unambiguous** — pure addition to an existing bounded context, no refactor, no `ProductId` re-pointing — and **independent of the product-hierarchy question** (EPIC-017), so nothing built here can be invalidated by it.

### In scope ✅
- **`OrganizationSite`** — identity, type, business functions, full postal address (country-linked), contact details, status + status date.
- **`Contact`** — name, title, roles, department, emails, phones, country, status + status date; optionally sited.
- **`OrganizationDivision`** — name, acronym, business functions.
- **Deepen `Organization` itself** — native-language name, acronym, organization identifier, business functions, status date (currently 3 of RIM's 16 attributes).
- **Directory queries** — sites by country/type, contacts by role, both across the tenant's whole registry, not only within one organization.
- Organization workspace UI (organization → divisions → sites → contacts), browser proof, ADR.

### Out of scope ⏸️ (deferred, with reason)
| Deferred | Why |
|---|---|
| **Wiring sites/contacts into License and Application** (approved sites, QP contacts, responsible contacts) | Those are fields on *other* aggregates in *other* epics. This epic makes the target exist and reachable by id; EPIC-006/010/017 attach to it. Building both halves here would fan this epic across four contexts. |
| **Mfg Business Operation** (site ↔ product ↔ operation type ↔ dates) | Needs the market-local product tier → **EPIC-010**. |
| **Site inspection history** | → **EPIC-006** (Inspection is an interaction object, and it anchors on Process Step). |
| **Organization hierarchy** (parent/subsidiary between organizations) | RIM does not model it; nothing asks for it. Add when a real portfolio needs it. |
| **Contact ↔ user account linkage** | A `Contact` is a regulatory-record person (often at another company or an HA); a `User` is someone who logs in. Conflating them would drag Platform identity into the regulatory domain. Revisit only if a real workflow needs it. |
| **Address validation / geocoding** | Infrastructure, not domain. `GpsLocation` is a stored string per RIM. |
| **Notifications to contacts** | → **EPIC-014**. |

### Definition of Done
- A site can be created under an organization with a complete postal address bound to a seeded `Country`, and appears in a tenant-wide site directory filterable by country and type.
- A contact can be created under an organization (optionally under a site), with one or more roles, and appears in a tenant-wide contact directory filterable by role.
- Sites and contacts are **tenant-isolated by their own fail-closed query filter** — proven by a test that a second tenant sees none of them (ADR-031/032).
- Deactivating a site or contact retires it from new work without hiding it (the `Organization`/`Product` precedent).
- Organization carries its RIM identity attributes (identifier, acronym, native name, business functions, status date).
- Browser proof: create organization → add division → add site → add contact → find that site in the country-filtered directory.
- ADR written for the aggregate-root call + tenant filtering of the new roots.

---

## Phase 2 — Domain design

### Entities

**`OrganizationSite`** — aggregate root, `src/Organization/RegOS.Organization.Domain/Aggregates/OrganizationSite/`

| Field | Type | Notes |
|---|---|---|
| `Id` | `OrganizationSiteId` | strongly-typed |
| `TenantId` | `TenantId` | owns the fail-closed filter |
| `OrganizationId` | `OrganizationId` | parent, `Restrict` FK |
| `Name`, `NameNativeLanguage?`, `Acronym?` | string | |
| `Identifier` | value object? | RIM "Organization Site Identifier" — in practice DUNS / FEI / SPL site id. **Scheme + value**, not a bare string |
| `Type` | `OrganizationSiteType` | manufacturing, packaging, testing, storage, HA office, sponsor office |
| `BusinessFunctions` | collection | RIM: Multiple, controlled list |
| `Address` | `PostalAddress` value object | Address1–3, City, StateProvince, `CountryId`, PostalCode |
| `GpsLocation?` | string | RIM stores free text |
| `Email?`, `Phone`, `Fax?` | string | |
| `LocalJurisdiction?` | string | |
| `DivisionId?` | `OrganizationDivisionId?` | RIM has site division/department as a controlled list; a link is better |
| `Status`, `StatusDate` | enum + `DateOnly` | |

**`Contact`** — aggregate root, same context

| Field | Type | Notes |
|---|---|---|
| `Id`, `TenantId` | | |
| `OrganizationId` | required | RIM: Parent, Single, Required |
| `OrganizationSiteId?` | optional | RIM says required; real contacts often aren't sited — **relax deliberately, note it** |
| `FirstName`, `LastName`, `Title?` | string | |
| `Roles` | collection | RIM: Multiple |
| `Department` | | |
| `Emails`, `Phones` | collections | RIM: Multiple for both |
| `CountryId` | | |
| `Status`, `StatusDate` | | |

**`OrganizationDivision`** — aggregate root, same context. `Id`, `TenantId`, `OrganizationId`, `Name`, `Acronym?`, `BusinessFunctions`.

**`Organization` (deepened)** — add `NameNativeLanguage?`, `Acronym?`, `Identifier`, `BusinessFunctions`, `StatusDate`.

### Decisions (approved 2026-07-31)

**1. Site, Contact and Division are aggregate roots, not children of `Organization`.** Apply EPIC-005's own test — users do not only ask *"load Organization X and inspect its sites"*, they ask *"which manufacturing sites are in India?"*. More decisively, other aggregates reference them **by id** (License → approved sites, Ingredient → manufacturing source, Application → QP contacts), which is the aggregate-root signal. They also carry independent status lifecycles. As children they would force a lock on `Organization` for every site status change.

**2. Each new root carries `TenantId` and its own fail-closed query filter.** *The ADR-worthy call — see the three shapes below.* Because they are **roots** — reachable directly, not only through a filtered parent — they do **not** inherit `Organization`'s filter the way `SubmissionDocument` or `DocumentVersion` inherit theirs. Site addresses and named contacts are exactly the competitively sensitive data ADR-032 was written for. Add all three to `ApplyTenantFilters` and assert the isolation with a test.

**3. Reference rows vs enums.** Phase-2 guiding rule says *prefer reference rows for anything a regulator or customer might extend*. Suggested split: **`BusinessFunctions` and `ContactRole` → reference data** (customers will extend these); **`OrganizationSiteType` and status → closed enums** (they drive behaviour, the EPIC-005 `RegistrationStatus` argument). Settle explicitly — it decides whether EPIC-012 inherits authoring work.

**4. `PostalAddress` as a value object,** not eight inline columns — matches the Email-as-value-object convention. Owned type in EF, so it stays inline in the table without leaking eight properties into the aggregate's surface.

**5. Division included here, not deferred to EPIC-006.** Reversal of an earlier lean: once this is a dedicated organization-depth epic rather than a slice bolted onto something else, keeping RIM's org triad together costs one small story and spares EPIC-006 from opening this bounded context.

**6. `Contact` is not a `User`.** Recorded so nobody "unifies" them later without a conversation. Different lifecycles, different ownership, different isolation rules.

### Change-case analysis

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| Licences must name approved manufacturing sites | **High** | Sites are roots with stable ids — a join table on the Registration side, no reshaping here |
| Customers extend site types / business functions | High | Reference rows for the extensible ones (decision 3) |
| A contact moves between organizations | Medium | Contact is a root; reassignment is a field change, not a re-parent |
| Site identifiers from multiple schemes (DUNS + FEI + SPL) | Medium | Identifier is scheme + value from day one; multiple ⇒ owned collection later, no migration of existing data |
| Mfg Business Operation (site ↔ product ↔ operation) | Medium | A new aggregate referencing `OrganizationSiteId`; nothing here changes |
| Contacts need per-application roles, not global roles | Medium | RIM already separates `Submission Role` from `Contact` — the seam is a join aggregate in EPIC-004, not a change here |
| Address formats vary by country | Low-Med | Value object; a country-aware formatter is a read concern |

---

### Phase 2 rulings (2026-07-31) — three amendments to the sketch

**A. Tenant filtering has three shapes, not two.** The decision for a new entity
is not *"filtered or not"* but *which of the three the codebase already uses*:

| Shape | Filter | Used by |
|---|---|---|
| **Fail-closed tenant-owned** | `CurrentTenant != null && x.TenantId == CurrentTenant` | Product, Organization, Submission, SubmissionSnapshot, ProductDocument, Registration, User |
| **Shared plus extensible** | `CurrentTenant != null && (x.TenantId == null \|\| x.TenantId == CurrentTenant)` | DocumentType, RegulatoryTemplate |
| **Global world facts** | none | Country, Authority, SubmissionType |

`OrganizationSite`, `Contact` and `OrganizationDivision` take the **first**
shape, and explicitly not the second: there is no such thing as a
system-provided manufacturing site or a shared contact. Naming this is the point
of the ADR — the shared-plus-extensible shape is the one a contributor would
reach for by analogy with `DocumentType`, and it would be a category error here.

**B. `BusinessFunctions` is deferred, not built.** It appears on three entities,
is RIM "Multiple, controlled", and has **no consumer in this epic** — nothing
queries by it and nothing branches on it. Building a reference table, three join
tables, a seed list and APIs for a field that is only ever displayed is
speculative completeness. It ships when a real query needs it, and is recorded
in the ADR as a deliberate deferral rather than a gap. `ContactRole` earns its
table today: *"who is the QP for this application?"* is in this epic's own
outcome, and EPIC-006 needs a correspondence contact.

**C. `IdentifierScheme` is reference data, not an enum.** An identifier scheme
is *vocabulary*, not *behaviour* — the EPIC-005 test that made `RegistrationStatus`
an enum (does it drive what may happen?) returns the opposite answer here.
DUNS, FEI, EU ORG-ID and SPL are jurisdiction-specific, externally governed and
occasionally extended; an enum would need a deployment to add one.

**D. The site directory ships with the site.** The tenant-wide directory *is*
the argument for making `OrganizationSite` a root, so it cannot arrive a story
later — EPIC-005's lesson that an aggregate should arrive with the capability
that justifies it.

---

## Phase 3 — Stories

| # | Story | Status |
|---|---|---|
| **STORY-001** | **`OrganizationSite`** — aggregate + `PostalAddress` value object + `IdentifierScheme` reference data + fail-closed tenant filter (and the stale `RegOSDbContext` remarks fix) + persistence + API + the country/type directory | 🟢 Complete |
| **STORY-002** | **`Contact`** — aggregate + `ContactRole` reference data + emails/phones + tenant filter + directory by role | 🟢 Complete |
| **STORY-003** | **`OrganizationDivision`** + deepen `Organization` (identifier, acronym, native name, status date) | ⚪ Not Started |
| **STORY-004** | **Capstone** — organization workspace (org → divisions → sites → contacts), browser proof, ADR-038, retro | ⚪ Not Started |

**ADR to write:** *Organization sites and contacts are aggregate roots, and the
three shapes of tenant filtering* — **ADR-038**.

### STORY-001 — `OrganizationSite` and the site directory (shipped)

**Decisions (approved 2026-07-31):**

1. **Status is an activation flag, so there is no history child.** Active/Inactive
   answers *do we still use this place?* — current operability, the same concept
   `Organization` and `Product` already carry without history. A `StatusDate` is
   the proportionate answer where the date still matters. The backlog's
   cross-cutting rule was **rewritten** to say *business lifecycles* rather than
   *statuses*, so it now explains why `Registration` got history and a site did
   not instead of leaving it to be inferred.
2. **Only the country is required in an address.** An in-licensed asset arrives
   as a manufacturer name and a country; refusing that would lose the fact
   entirely (the ADR-035 principle). The country is required because it is the
   only part the model *reasons* about — it is what the directory filters by.
   `PostalAddress` is therefore a deliberately weak value object whose job is
   encapsulation, not enforcement.
3. **Identifiers are a collection from day one.** A US plant holds an FEI *and* a
   DUNS number today; they are peers, not alternatives. One per scheme is an
   aggregate invariant **and** a unique index on
   `(OrganizationSiteId, SchemeId)` — the persistence model reinforcing the
   domain model rather than merely storing it.
4. **`IdentifierScheme` is a global world fact**, unfiltered like `Country` and
   `Authority`: a DUNS number does not become a different scheme because one
   tenant thinks about it differently. Migration path if a customer ever needs a
   private internal scheme is recorded in the type: a nullable `TenantId` and one
   filter, seeded rows keeping a null tenant.
5. **`OrganizationSiteType` stays a closed enum** — only a *manufacturing* site
   can be named on a licence as an approved manufacturer, so the vocabulary
   participates in regulatory rules rather than merely being displayed. The exact
   mirror of why `IdentifierScheme` is data.
6. **The directory ships with the aggregate**, because it is the argument for the
   aggregate being a root.

**The stale `RegOSDbContext` remarks are corrected.** They listed `Organizations`
as an unfiltered global directory — untrue since ADR-032, and contradicted by the
filter 35 lines below. Replaced with the **three shapes** (fail-closed
tenant-owned / shared-plus-extensible / global world facts), so the block now
teaches the decision a new entity actually faces.

**Structural change:** `Organization.Domain` now references
`ReferenceData.Domain`. No cycle — `ReferenceData.Domain` depends only on
SharedKernel, and `Registration.Domain` already does the same.

**API:** `POST /organizations/{id}/sites` · `GET /organization-sites/{id}` ·
`GET /organizations/{id}/sites` · `GET /organization-sites?countryId=&type=`.

**Verified:** 790 backend tests green (37 new: 23 domain, 14 integration in a new
`RegOS.Organization.Application.Tests` project); migration `AddOrganizationSites`
creates three tables with the unique `(OrganizationSiteId, SchemeId)` index and
`Restrict` FKs to Countries, Organizations and IdentifierSchemes. The isolation
claim is proved directly: a second tenant finds the site neither by id, nor
through the directory, nor through the detail read model.

### STORY-002 — `Contact` and the role directory (shipped)

**Decisions (approved 2026-07-31):**

1. **Status is an activation flag, as for a site.** *Do not use this contact* is
   configuration, not a regulatory event — a `StatusDate` and no history.
2. **`ContactRole` is shared plus extensible, unlike `IdentifierScheme`**, and
   the difference is ownership. A scheme describes the outside world; a role
   describes how a company organises people, and the vocabulary is genuinely
   mixed — *Qualified Person* is legislated, *APAC Regulatory Lead* is one
   company's word. Six platform roles ship with a null tenant; a tenant's own
   stay private to it.
3. **Roles, emails and phones are collections**, because multiple values are
   ordinary for all three. **No `preferred` or `primary` abstraction** — nothing
   yet needs to rank them, and inventing an order would be speculative.
4. **The site is optional**, though RIM requires it: an authority reviewer or a
   head-office regulatory lead has none, and refusing them would lose the person.
   The country is optional too — unlike a site's, which the directory filters by.
5. **A `Contact` is not a `User`**, recorded in the aggregate so nobody unifies
   them later without a conversation.

**API:** `POST /api/organizations/{id}/contacts` · `GET /api/contacts/{id}` ·
`GET /api/organizations/{id}/contacts` · `GET /api/contacts?roleId=`.

### The slice conventions arrived mid-story

`docs/engineering/slice-conventions.md` and `tests/Architecture/` landed while
STORY-002 was being written, and **all five backend failures were this story's
code**. Fixed rather than grandfathered, per the doc's own rule that a new entry
to unblock new code defeats the mechanism:

| Rule | What changed |
|---|---|
| SC-001 | Every contact route moved under `/api` |
| SC-002 | `IContactRepository` and `IOrganizationSiteRepository` moved to their Domain projects |
| SC-003 | Query records added for all six site and contact queries |
| SC-004 | Endpoint lambdas became named `HandleAsync` methods |
| SC-005 | `CreateContact.cs` and `ContactQueries.cs` split into one file per type |

**Four grandfathered entries were retired** while in the slice — STORY-001's
three site routes and its `OrganizationSiteQueryEndpoints.cs`. They were free to
move because the site UI does not exist until STORY-004, so no frontend caller
changed.

**Frontend SC-101/102/105 applied to the registrations feature** from EPIC-005,
which the conventions doc names as the ❌ example: `api/registrations.ts` split
into eight call files plus `problemDetail.ts`, `hooks/useRegistrations.ts` into
eight hook files, and `statusLabel.ts` / `expiry.ts` moved out of `components/`
into `constants/` and `utils/`.

**Verified:** 833 backend tests green (43 new: 21 domain, 12 integration, plus
the 10 architecture tests now all passing); migration `AddContacts` creates five
tables with unique indexes on `(ContactId, RoleId)` and `(TenantId, Code)`;
frontend typecheck, lint and build clean.
