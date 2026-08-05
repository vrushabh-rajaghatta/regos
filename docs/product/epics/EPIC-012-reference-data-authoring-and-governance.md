# EPIC-012 — Reference-data authoring & governance

**Status:** ⚪ Not Started · **Branch:** `epic/EPIC-012-reference-data-authoring-and-governance` (cut at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

RegOS governs eighteen-odd controlled lists and **a user cannot see any of them**. This epic makes reference data visible, then makes it governable — in that order, because the second is worthless without the first and the first is useful on its own.

> **Phase 1 below is settled.** The [recorded design](#the-recorded-design--2026-08-05) is a **mockup of how the UI should look** — structure and affordances, with placeholder content; read [how to read this record](#how-to-read-this-record) before comparing any of it to the database. Phases 2–3 are a **sketch** and are **not approved design** — confirm, amend or replace them in the Phase-2 conversation on pull-in.

---

## Phase 1 — Epic plan

### Outcome
Anyone doing regulatory work can **look up** any governed list without leaving the work — what it contains, where it came from, and what depends on it. A data steward can then **extend and correct** the lists a tenant is allowed to own, under change control, without a deployment.

### The two surfaces, and why they are one epic

The mockup makes a distinction worth keeping, and it is not the obvious one:

| | Surface | Lives under | Audience | Nature |
|---|---|---|---|---|
| **A** | **Reference** — read-only lookup | **Regulatory** | anyone doing the work | *"look something up without leaving the work"* |
| **B** | **Administration** — authoring & governance | **Administration** | data steward | CRUD, change control, tenant extension |

They are one epic because they read the same data and would otherwise disagree about how it is described. **They are two deliveries** — A ships alone and is useful; B without A would be an editor for lists nobody can see. Expect this to split the way EPIC-010 did if A runs long.

### What is not visible today

Ten read endpoints exist, under **three different prefixes** — `/api/master-data/…`, `/api/reference-data/…`, and bare `/api/measurement-units`. Roughly half the lists the mockup indexes have **no endpoint at all**, and **no route in the SPA reaches any of them**. Countries are fetched five times as dropdown options and never displayed as a subject.

The sharpest illustration is `GeographyVocabulary.Regions`: five values compiled into the binary, no endpoint, no screen. Two of them — `ASEAN` and `GCC` — are referenced by **no seeded country**, so they cannot be observed through any API call by any means. A user asking *"what regions can I use?"* would derive three from the countries list and be wrong.

### In scope ✅

**A — the Reference browser (read-only)**
- The **Reference index** and **detail pages** exactly as [recorded](#the-recorded-design--2026-08-05): grouped nav with counts, the definition sentence, the four-cell provenance strip, search, and the usage column.
- **Read endpoints for every list the index names**, including the vocabularies that have none.
- **Route-prefix consolidation** — one prefix for reference data, not three (SC-001).
- **`Referenced by` / usage counts** — the read models that answer *"what breaks if this changes?"*

**B — Administration (authoring & governance)**
- **Steward CRUD** over the lists a tenant may own — tenant-scoped rows only, never the platform's.
- **The shared-plus-extensible boundary made visible and enforced in the UI**: a null-`TenantId` row is the platform's and is read-only; a tenant's own is editable. `DocumentType`, `RegulatoryTemplate`, `ContactRole`, `AuthorityDivision` and `Substance` already carry this shape.
- **Change control** — who changed a governed list, when, and what it was before.
- **Tenant-authored / cloned templates and document types** — the write side deferred from EPIC-001.
- **The mutation guard `Substance` deferred here by name**: *"Steward editing, lifecycle and change control are EPIC-012's, and the guard belongs on the first mutation that exists rather than ahead of it."* This epic is that first mutation.

### Out of scope ⏸️ (deferred, with reason)
| Deferred | Why |
|---|---|
| **Editing the platform's own rows** | A tenant extends; it does not redefine. Letting a tenant edit a shared `Substance` or `Country` would create a second FDA able to disagree with the reference-data one — the argument [ADR-039 §1](../../adr/ADR-039-the-market-local-product-tier.md) already made. |
| **Widening the country seed** | A seed change, and not this epic's — it needs the sourcing conversation [EPIC-022](EPIC-022-country-depth.md) opened, since every added market now costs five facts rather than two. A browser shows whatever the seed holds. |
| **Bulk import / CSV upload** | A steward correcting a row is a different capability from loading a register. Loading GSRS, ISO 3166 or MedDRA is data-ops and belongs with whichever epic buys the licence. |
| **Approval workflow on a reference change** | → **EPIC-008**, which owns review and e-signature. Change *history* is here; change *approval* is not. |
| **Effective-dating reference rows** | Nothing asks *"was Germany in the EU in 2019?"* yet. `RegulatoryTemplateVersion` already shows the shape when something does. |
| **Deleting reference rows** | ES-018 — lifecycle over deletion. Where a list needs retirement it gets a status, and most of these have none by design. |

### Definition of Done
- Every governed list named in the recorded index is reachable from the UI, shows its provenance strip, and is searchable.
- **`GeographyVocabulary.Regions` is visible in full — all five**, including the two no seeded country references. This is the acceptance test for the whole read half: if a list can only be derived from its consumers, it is not visible.
- One route prefix serves reference data; the other two are gone.
- A steward can add a tenant-owned row to each extensible list, and **cannot** edit a platform row — proven by a test, not a disabled button.
- Every change to a governed list records who, when, and the prior value.
- Browser proof: look up a list from inside a piece of regulatory work, then extend it from Administration and see the extension in the work.
- ADR if the read/write split, the route consolidation, or the tenant-edit boundary forces one.

---

## The recorded design — 2026-08-05

**A mockup of how the UI should look**, given by the founder and written down here because a design held only as an image is a design nobody will find later. It is intent, not a specification of data.

### Navigation

Four panes, left to right — **section → work area → reference index → content**.

**Pane 1 — sections.** Regulatory · Quality · Clinical · Safety · Administration, with the sections not yet built shown and labelled rather than hidden.

**Pane 2 — the work area for the active section**, grouped, each entry carrying a live count:

| Group | Entries |
|---|---|
| WORK | Due work |
| PORTFOLIO | Products · Substances · Registrations · Studies |
| FILINGS | Applications · Submissions · Templates |
| AUTHORITY INTERACTIONS | Correspondence · Meetings · Inspections |
| PARTIES | Organizations · Sites · Contacts |
| REFERENCE | Reference |

**Pane 3 — the Reference index.** Its header carries the epic's thesis, and this wording is the design rather than placeholder:

> **Reference** — *Look something up without leaving the work. Governed lists live in Administration.*

Grouped, each entry carrying a count. The groups are the design; the entries drawn under them illustrate the grouping rather than enumerate the system:

| Group | Illustrative entries |
|---|---|
| GEOGRAPHY | Countries · Regions |
| AUTHORITIES | Authorities · Authority divisions · Correspondence types |
| FILINGS | Application types · Submission types · Document types · Blueprints |
| PRODUCT VOCABULARIES | Pharmaceutical · Packaging · Clinical · Label · Measurement units |
| SUBSTANCES | Substance register · Substance vocabulary |
| PARTIES | Contact roles · Identifier schemes |

Two entries are drawn with a dash instead of a count — an **empty/unavailable state** the index needs. What a dash should mean is a build-time question, not something the mockup settles.

### Detail page anatomy

Drawn with `Countries` as the example. **The anatomy is the specification; the country data filling it is placeholder.**

1. **Breadcrumb** — section · area · group (`REGULATORY · REFERENCE · GEOGRAPHY`)
2. **Title** + a right-aligned status pill stating who owns the list — e.g. `Read only — seeded with the platform`
3. **A definition, not a label.** A short paragraph saying what the concept *is* and why it is shaped as it is. The one drawn for Countries reads:
   > *A jurisdiction RegOS holds records for. Two names and two codes, for two audiences: a person picks from the common name, and a machine-readable submission carries the register's own wording. Neither can be derived from the other, which is why both are stored.*
4. **A four-cell provenance strip** — the strongest idea in the design:

   | Cell | Answers | Drawn as |
   |---|---|---|
   | **SOURCE** | *where did this come from?* | the external register, or that there isn't one |
   | **ROWS** | *how much is here?* | a count with a one-line gloss |
   | **REFERENCED BY** | *what breaks if it changes?* | the things that point at it |
   | **LIFECYCLE** | *can it change at all?* | e.g. "None — flat master data, no active or merged state" |

5. **Search**, with a live *"n of m"* result count and a placeholder naming the searchable fields
6. **Table** — the identifying columns, plus **a usage column** showing how many things reference each row (and `none` where nothing does)
7. **A `differs` badge** — an inline chip flagging a row where two columns that usually agree do not. Drawn on `ISO NAME` where the register's wording departs from the common name

### The parts to keep

**The provenance strip is what a CRUD screen never tells you.** Source, row count, dependants and lifecycle are the four things a steward needs before touching a governed list, and they are the four this project has repeatedly reconstructed by reading ADRs — `Substance`'s *"demonstration seed data only"*, `AuthorityDivision`'s *"no authoritative source exists"*, `GeographyVocabulary`'s *"nobody publishes the set of groupings"*. **The strip gives those statements a home in the product** rather than only in code comments.

**The `differs` badge earns two stored columns.** It is the visible justification for `IsoName` sitting beside `Name` — EPIC-022's argument shown rather than asserted. Where the two agree the second column looks redundant; the badge is what stops someone deleting it later.

**The definition paragraph is the unusual one.** Reference screens normally show a table and a title. Writing what the concept *is*, on the screen, is how a vocabulary stops being a dropdown's backing list.

### How to read this record

**It is a mockup: it specifies structure, hierarchy, page anatomy and affordances.** The counts, the country rows, the badge placements and the entries under each group are **placeholder content chosen to show the design working** — they are not scope, not a seed size, and not an inventory of what exists.

So: build the anatomy, and take the data from the system. Nothing here should be read as *"the UI must show 24 countries"* or *"these are the lists"*, and a future reader comparing the drawing to the database will find differences that mean nothing.

---

## Phase 2 — Domain design *(sketch — not approved)*

### Decisions to settle (Phase 2, on pull-in)

**1. Is the index generated or authored?** *Lean: generated.* Nine vocabularies exist and every one arrived with an epic — `Stability` and `Supply` with EPIC-010b, `Geography` with EPIC-022 — so a hand-authored index is a list someone must remember to edit each time, and the first forgotten edit is invisible. A registry each list registers itself into makes the index a fact about the system rather than a document about it.

**2. One route prefix.** Three exist. *Lean: `/api/reference-data/…` for all of it*, with `/api/master-data/…` and `/api/measurement-units` redirected or removed. Frontend `masterData/` feature folder renames with it.

**3. Do static vocabularies and persisted lists share a contract?** `GeographyVocabulary.Regions` is a compiled `IReadOnlyList<CodedConcept>`; `Country` is a table; `Substance` is a table with tenant extension. The browser wants one shape — code, name, description, source, count, editable-or-not — over all three. *Lean: one read contract, three providers.* This is the decision that makes the index generatable.

**4. Where does `Referenced by` come from?** Counting dependants means a query per list, and some cross contexts. *Lean: declared, not derived* — each list states what references it, and the **counts** are queried. A wrong count is a bug; a wrong dependency list is a lie, and declaring it keeps it reviewable.

**5. The tenant-edit boundary is enforced in the domain, not the UI.** Every extensible list already carries `TenantId?`. *Lean: a shared guard tested per aggregate* — this is the third-plus occurrence, so extraction is due (ADR-018), and it is what `Substance` deferred here by name.

**6. Does the browser live in `Regulatory` or `Administration`?** The mockup says **both, differently** — read-only lookup inside the work, governed authoring in Administration. *Lean: honour it.* A steward looking something up should not have to leave for the admin section, and someone doing regulatory work should not be one click from editing a vocabulary.

### Change-case analysis

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| A new vocabulary arrives with an epic (nine so far, all since EPIC-010a) | **Very high** | Generated index (decision 1) — it appears without anyone editing a list |
| A licensed register is loaded (GSRS, ISO 3166, MedDRA) | **High** | The provenance strip's SOURCE cell changes; the shape does not |
| Change approval / e-signature (EPIC-008) | Medium | Change history is recorded here; approval attaches to it |
| Effective-dated reference rows | Medium | Nothing asks yet; `RegulatoryTemplateVersion` is the shape when it does |
| Quality / Clinical / Safety sections arrive | Medium | The mockup already reserves them; Reference is per-section or shared — decide when the second section exists |
| A tenant needs to hide a platform row it never uses | Low-Med | Not deletion — a per-tenant suppression flag, and only when asked |

---

## Phase 3 — Candidate stories *(sketch — re-slice on pull-in)*

**A — the browser**

| # | Story |
|---|---|
| **S001** | One read contract over static vocabularies and persisted lists; route prefix consolidated; **`/api/reference-data/regions` — the list that started this** |
| **S002** | The Reference index — generated, grouped, counted |
| **S003** | The detail page — definition, provenance strip, search, table, `differs` badge |
| **S004** | `Referenced by` and the usage column |

**B — administration**

| # | Story |
|---|---|
| **S005** | Steward CRUD on tenant-owned rows; the platform/tenant boundary enforced in the domain and proven by test |
| **S006** | Change history — who, when, prior value |
| **S007** | Tenant-authored / cloned templates and document types (EPIC-001's deferred write side) |
| **S008** | Capstone — look up from inside the work, extend from Administration, see it in the work; browser proof; retro |

**Where to stop if it runs long:** after **S004**. The browser is the half with a standalone outcome, and B has never had a user waiting on it.
