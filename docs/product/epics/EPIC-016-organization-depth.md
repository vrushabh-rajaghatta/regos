# EPIC-016 — Organization depth: sites, contacts, divisions

**Status:** ⚪ Not Started · **Branch:** `epic/EPIC-016-organization-depth` (cut at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

RegOS knows *that* a partner exists. It does not know **where they operate** or **who to talk to**. This adds the two things almost every other regulatory object needs to point at.

> **Phase 1 below is settled.** **Phases 2–3 are a sketch**, written so this epic can be picked up months from now without re-deriving it — they are **not approved design**. Confirm, amend or replace them in the Phase-2 conversation when this epic is pulled into **Now**. See [RIM alignment](../BACKLOG.md#rim-alignment) for why this epic is first.

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

## Phase 2 — Domain design *(sketch — not approved)*

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

### Decisions to settle (Phase 2, on pull-in)

**1. Site, Contact and Division are aggregate roots, not children of `Organization`.** *Recommended.* Apply EPIC-005's own test — users do not only ask *"load Organization X and inspect its sites"*, they ask *"which manufacturing sites are in India?"*. More decisively, other aggregates reference them **by id** (License → approved sites, Ingredient → manufacturing source, Application → QP contacts), which is the aggregate-root signal. They also carry independent status lifecycles. As children they would force a lock on `Organization` for every site status change.

**2. Each new root carries `TenantId` and its own fail-closed query filter.** *This is the ADR-worthy call.* Because they are **roots** — reachable directly, not only through a filtered parent — they do **not** inherit `Organization`'s filter the way `SubmissionDocument` or `DocumentVersion` inherit theirs. Site addresses and named contacts are exactly the competitively sensitive data ADR-032 was written for. Add all three to `ApplyTenantFilters` and assert the isolation with a test.

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

## Phase 3 — Candidate stories *(sketch — re-slice on pull-in)*

| # | Story | Slice |
|---|---|---|
| **S001** | **`OrganizationSite`** — aggregate + `PostalAddress` value object + tenant filter + persistence + API + site list under an organization | domain → persistence → API → UI → test |
| **S002** | **Site directory** — tenant-wide sites filterable by country and type (the query that justifies the root) | API → UI → test |
| **S003** | **`Contact`** — aggregate + roles + emails/phones + tenant filter + API + contact list, under an organization and under a site | full slice |
| **S004** | **`OrganizationDivision`** + **deepen `Organization`** (identifier, acronym, native name, business functions, status date) | full slice |
| **S005** | **Capstone** — organization workspace (org → divisions → sites → contacts), browser proof of the full journey, ADR, retro | UI → test → docs |

**ADR to write:** *Organization sites and contacts are aggregate roots with their own tenant filter* — next free number (expected **ADR-038**, after EPIC-005's ADR-037).
