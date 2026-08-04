# EPIC-010a — Substance & composition

**Status:** ⚪ Not Started · **Branch:** `epic/EPIC-010a-substance-and-composition` (cut at Phase 2) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

The first of three splits of **[EPIC-010](EPIC-010-idmp-product-data-depth.md)** — cluster **A**, the IDMP root. What a product is *made of*: which substances, in what role, at what strength, in what dose form, by what route.

> **Phases 2 and 3 below are settled**, in the Phase-2 conversation of 2026-08-03. The umbrella epic keeps the split rationale and the sketches for **10b** and **10c**, which stay sketches until each is pulled into Now.

**Delivers the standalone outcome** *"which products contain this API?"* — which is why cluster A can ship without B or C.

---

## Phase 2 — Domain design

### The questions this exists to answer

Phase 2 begins with the domain question, not the entity list ([ADR-038](../../adr/ADR-038-organization-depth-roots-and-the-three-filter-shapes.md): *a root justified by a query that does not exist yet is a demo of an empty table*).

| | Question | What it forces into existence |
|---|---|---|
| **Q1** | *"Which of our products contain substance X?"* | `Substance` as a **shared root**, not a string on an ingredient. This is the epic's Definition of Done. |
| **Q2** | *"What is in this product, at what strength, in what dose form, by what route?"* | `PharmaceuticalProductDetail` + `Ingredient` + a `Strength` **value object** |
| **Q3** | *"What does the patient physically receive — is this a kit, a pen, a vial plus a diluent?"* | `MedicinalProductComponent`, and only this question justifies its recursion |

**Q4 was asked and rejected.** *"Which products use a substance sourced from site Y?"* would justify RIM's `Ingredient → Manufacturing Source (Organization)` link. Nobody has asked it, sourcing lives in cluster **D** (10c), and the answer would be an empty column for the whole of 10a. **Recorded as a seam, not built** — a nullable `ManufacturingSourceOrganizationId` is an additive migration whenever 10c demonstrates the need.

### Is this one thing, or two facts?

Applied to every concept the design leans on, per the flow's second question.

| One term | Two facts | Verdict |
|---|---|---|
| "ingredient" | the **substance** (a scientific entity, tenant-independent) and the **role it plays in a product** (active/excipient, at a strength) | **Genuinely two.** `Substance` and `Ingredient` both exist, and the split is what makes Q1 answerable. |
| "what the product is" | the **administrable form** (composition, strength, dose form, route) and the **physical article** (a vial, a syringe, a kit) | **Genuinely two.** `PharmaceuticalProductDetail` and `MedicinalProductComponent` — the same distinction ISO IDMP draws as *PharmaceuticalProduct* vs *ManufacturedItem*. |
| "route of administration" | — | **One fact, and RIM models it twice.** See decision 6. |

---

### Decisions

#### D1 — Vocabulary strategy: seeded internal terminology behind a `CodedConcept` seam

*Founder decision, 2026-08-03. Recorded here verbatim because it is the epic's central limitation and must not become an implicit assumption.*

> RegOS models all controlled terminology through a `CodedConcept` abstraction. During MVP, the platform ships with a curated internal vocabulary sufficient for demonstration, testing, and early customer use. **These vocabularies are not represented as authoritative EDQM, WHO ATC, GSRS, or other licensed terminology.**
>
> Integration with licensed terminology providers (EDQM Standard Terms, WHO ATC, GSRS/UNII, etc.) is deferred until those datasets are legally obtained. Existing references are intentionally designed so that replacing seeded values with authoritative codes is a **data migration rather than a domain redesign**.
>
> Consequently, **completion of EPIC-010 does not imply regulatory-ready IDMP/xEVMPD submission.** It provides the domain model required to support those capabilities later.

**Why this is recorded as a decision and not an evidence entry:** `docs/evidence/` holds eCTD (EPIC-007a) and STF (EPIC-019) artifacts and **nothing** for EDQM, WHO ATC, ISO 11238, UNII/GSRS or xEVMPD. There is no external fact to record. What is recorded is our decision to proceed without one, and what that costs.

**The failure this is written to prevent** is the one EPIC-019 hit: a vocabulary assumed held that was not, discovered one story short of the epic's reason to exist. Here the gap is known up front, its consequence is stated, and no story claims a readiness it does not have.

**`CodedConcept`** — a value object, used everywhere RIM says *Controlled Vocabulary* or *Name/Value Pair*:

```
CodedConcept
  System   string   "regos-internal" during MVP; "edqm", "who-atc", "unii" later
  Code     string
  Display  string
```

`System` is present from day one and is what makes the swap a migration. A seeded row is `("regos-internal", "TAB", "Tablet")`; the same field later holds `("edqm", "10219000", "Tablet")`.

#### D2 — `Substance` is shared reference data with tenant extension

*Founder decision, 2026-08-03.*

- Shared/global substances ship with the platform (`TenantId` null).
- A tenant may add proprietary compounds (`TenantId` set) — innovators hold molecules before INN assignment.
- **A tenant may create proprietary substances but may not modify shared global substance definitions.**
- References always point at a single `Substance`, whichever kind.

This is the **shared plus extensible** filter shape, one of the three named in [RegOSDbContext.cs:288](../../../src/Persistence/RegOS.Persistence/RegOSDbContext.cs#L288) — `TenantId == null || TenantId == CurrentTenant`. **Copy [AuthorityDivision.cs](../../../src/ReferenceData/RegOS.ReferenceData.Domain/Regulatory/Authority/AuthorityDivision.cs)**, which is the closest existing case and carries the reasoning; do not copy the nearest file.

The argument is not the same as `AuthorityDivision`'s, and the difference is worth stating so the conclusion is not pattern-matched. `AuthorityDivision` is extensible because **RegOS has no authoritative source** for the world's divisions. `Substance` is extensible because an authoritative source exists and **the tenant's molecule is not in it yet**. Same shape, different reason, and the second one resolves itself when D1's licensed terminology arrives — the shared catalogue grows and proprietary rows migrate into it.

> ⚠️ **This introduces the first write path into `ReferenceData`.** That project is Queries-only today — no `Commands` folder, no `I*Repository`, nothing writable. The backlog assigns reference-data authoring to **EPIC-012**. 10a takes only what D2 requires: *create a tenant-owned substance*. It does **not** build steward CRUD, change control, or shared-row editing, all of which remain EPIC-012's. Recorded so the overlap is deliberate rather than discovered.

#### D3 — `Ingredient` has one parent, so there is no polymorphism to solve

The umbrella sketch says *"the same polymorphic-parent problem EPIC-018 solves for `Population` — reuse whatever decision that epic made."* **EPIC-018 has not been planned, so there is nothing to reuse** — and on inspection, 10a does not need the problem solved at all.

Read RIM closely: `Ingredient` is **required** on `Pharmaceutical Product Detail` (`Active Ingredient`, Child, Multiple, Required **Yes**) and **optional** on `Medicinal Product Components` (Child, Multiple, Required **N**). Only one parent is demonstrated by Q1 and Q2. Q3 does not ask what a *component* is made of; it asks what the patient receives.

**So `Ingredient` is an owned child of `PharmaceuticalProductDetail` only.** Component-level ingredients are not built.

This is ADR-018 applied honestly: **one demonstrated need is not two, and two is not three.** Whichever of 10a or EPIC-018 lands first sets a precedent for the *shape*; neither extracts a shared abstraction, and a third occurrence is what would justify one. Recorded as a **constraint**, not a decision: *do not introduce a polymorphic parent in 10a.*

#### D4 — `Strength` is a value object, never a string

```
Strength
  NumeratorValue     decimal
  NumeratorUnit      CodedConcept
  DenominatorValue   decimal?
  DenominatorUnit    CodedConcept?
```

RIM types it *"Complex values"*. Storing `"50mg/5ml"` as text makes every downstream comparison, conversion and xEVMPD render impossible. Nullable denominator covers the point strength (`50 mg`) as against the concentration (`50 mg / 5 mL`).

#### D5 — Recursion: `MedicinalProductComponent` only, adjacency list with a depth guard

10a carries **one** of the umbrella epic's two recursions; `Packaging` is 10b's. The umbrella says *"decide once, apply to both"* — 10a decides, 10b inherits or argues.

- **Adjacency list** (`ParentComponentId?`), not materialised path. Component trees are shallow (a kit contains a vial and a syringe); a path column is a write-time cost paid for a read that a two-level tree does not need.
- **Depth guard** enforced as an aggregate invariant, with a stated maximum. Depth-tested, per the umbrella's DoD.
- **Cycle prevention** at the same gate — a component may not be its own ancestor.
- Query via recursive CTE when depth ever exceeds what an `Include` chain handles. Not before.

#### D6 — `RouteOfAdministration` is an owned collection, not an object

RIM gives Route of Administration its own sheet with **two attributes**, one of which is the parent pointer — and *also* carries it as a multi-valued inline attribute on `Pharmaceutical Product Detail`. The same fact is expressed twice and the two disagree about which is authoritative. The standalone object is a relational artifact: it exists so a spreadsheet had somewhere to put a second row.

**RegOS models it once** — an owned collection of `CodedConcept` on `PharmaceuticalProductDetail`. Recorded because it is a deliberate divergence from the reference model, not an omission.

Note the same concept is typed **free text** on `Clinical Study` and `Non-Clinical Study` in RIM, and [EPIC-019](EPIC-019-study-registry.md) lists `RouteOfAdministration` as a coded study attribute. Both should resolve to `CodedConcept` so *"which studies used the intravenous route?"* and *"which products are given intravenously?"* are the same query. **10a establishes the shape; EPIC-019 S005 aligns to it.**

#### D7 — RIM's `Substance` sheet is defective, and the model resolves it explicitly

The source sheet lists **`Substance Class` and `Substance Type` twice each** — a duplication with no stated distinction, in a sheet where every other attribute appears once. Treated as a **source-model defect**: modelled once each.

It also carries three name-shaped fields, all typed `Specification`, all required, with no stated distinction: `Substance (API)`, `Substance Name`, `INN (generic name)`.

> **The ambiguity is recorded rather than resolved by interpretation.** The founder's working reading — *preferred/display name · alternate or local name · WHO INN* — is plausible and **unsupported by any evidence RegOS holds**. Baking it in would manufacture a distinction the source does not make.

**Decision:** model **two** fields, not three.

| Field | Holds |
|---|---|
| `Name` | the preferred scientific name — the one displayed |
| `Inn` (nullable) | the WHO International Nonproprietary Name, where one has been assigned |

`Substance (API)` and `Substance Name` collapse into `Name`. `Inn` is nullable because **a proprietary pre-INN compound is exactly the case D2 exists to serve** — the field's absence is meaningful, not missing. If a real filing later demonstrates that alternate/local names are a distinct requirement, that is an additive collection, not a redesign.

---

### Entities

All ids are `sealed class <X>Id : StronglyTypedId` inheriting `AggregateRoot<TId>` or `Entity<TId>` (ES-020, [ADR-043](../../adr/ADR-043-entity-identity-derives-from-the-kernel.md)). **Copy [CommitmentId.cs](../../../src/Interaction/RegOS.Interaction.Domain/Commitments/CommitmentId.cs)** — not the nearest id, several of which are legacy record structs pending migration.

#### `Substance` — root · `src/ReferenceData/` · shared plus extensible

| Column | Type | Notes |
|---|---|---|
| `Id` | `SubstanceId` | |
| `TenantId` | `TenantId?` | **null = platform-shipped, set = tenant proprietary** (D2) |
| `Name` | `string` | preferred scientific name (D7) |
| `Inn` | `string?` | null before INN assignment (D7) |
| `SubstanceClass` | `CodedConcept` | modelled once (D7) |
| `SubstanceType` | `CodedConcept` | modelled once (D7) |
| `CasNumber` | `string?` | |
| `UniiCode` | `string?` | the GSRS seam — null during MVP (D1) |
| `MolecularFormula` | `string?` | |
| `Description` | `string?` | RIM's *Chem/Bio Description* |
| `IsActive` | `bool` | lifecycle over deletion (ES-018) |

**Invariant:** a shared substance (`TenantId is null`) cannot be mutated through any tenant-facing path (D2).

#### `PharmaceuticalProductDetail` — root · `src/Product/` · tenant-owned

Hangs off **`MedicinalProduct`** (the market-local tier), per the umbrella's dependency on EPIC-017: strength and presentation are market-specific facts.

| Column | Type | Notes |
|---|---|---|
| `Id` · `TenantId` · `MedicinalProductId` | | fail-closed tenant filter |
| `Name` · `Description` | `string` · `string?` | |
| `DoseForm` | `CodedConcept` | |
| `UnitOfPresentation` | `CodedConcept?` | |
| `RoutesOfAdministration` | owned `CodedConcept` collection | D6 |
| `Ingredients` | owned `Ingredient` collection | D3 |
| `Version` | `string` | |

#### `Ingredient` — owned child of `PharmaceuticalProductDetail`

| Column | Type | Notes |
|---|---|---|
| `Id` · `SubstanceId` | | the link that answers **Q1** |
| `Role` | `CodedConcept` | active / excipient |
| `Strength` | `Strength` | D4 |

No `ManufacturingSourceOrganizationId` — Q4 was rejected; the seam is recorded, not built.

#### `MedicinalProductComponent` — root · `src/Product/` · recursive

| Column | Type | Notes |
|---|---|---|
| `Id` · `TenantId` · `MedicinalProductId` | | |
| `ParentComponentId` | `MedicinalProductComponentId?` | D5 |
| `ComponentType` · `Name` · `Description` | | |
| `Quantity` · `UnitOfPresentation` | `decimal` · `CodedConcept?` | |
| `DoseForm` | `CodedConcept?` | |

#### Value objects

`CodedConcept` (D1) · `Strength` (D4).

⚠️ **Corrected 2026-08-03 by [ADR-058](../../adr/ADR-058-substances-are-shared-facts-ingredients-are-roles.md) §3.** This said both live in `src/Product/`. `CodedConcept` cannot: `Substance` sits in `ReferenceData` and carries two of them, so `Product` as its home would require **`ReferenceData → Product`** and invert an established dependency. It lives in **`ReferenceData.Domain`** — not `SharedKernel`, which ADR-017 keeps to primitives with no domain meaning.

`Strength` is unaffected and stays in `src/Product/` until a second context demonstrates a need.

### What 10a changes in already-shipped aggregates

**This epic modifies existing code; it does not only add.** Named so it is priced in, not discovered.

- **`MedicinalProduct`** gains ATC code, and becomes the parent of both new roots. Its own class comment already assigns this: *"ATC code and strength (EPIC-010) … each arrives with the feature that reads it."* ([MedicinalProduct.cs:27-28](../../../src/Product/RegOS.Product.Domain/Product/MedicinalProduct.cs#L27-L28))
- **`RegOSDbContext`** gains three query filters — two fail-closed tenant-owned, one shared-plus-extensible — and the doc comment listing the three shapes needs its new members.
- **`ReferenceData`** gains its first `Commands` folder and its first repository (D2).

### Change-case analysis

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| Licensed terminology arrives (EDQM / WHO ATC / GSRS) | **High** | `CodedConcept.System` present from day one — a data migration, not a redesign (D1) |
| *"Which products contain substance X?"* | **High** | `Substance` is a shared root; `Ingredient` carries the FK |
| xEVMPD / IDMP export (EPIC-007b) | **High** | Completeness matters more than shape; `Strength` and `CodedConcept` are the render-critical two |
| A tenant's proprietary compound receives an INN | **High** | `Inn` is nullable and settable; the row does not move |
| Ingredients needed at component level | Medium | Additive owned collection on `MedicinalProductComponent`. **If this lands, it is the second occurrence — evaluate extraction then, not now** (D3) |
| Strength expressed as a range | Medium | Value object gains a range without changing callers (D4) |
| Component trees deeper than anticipated | Medium | Adjacency list + depth guard; recursive CTE if reads demand it (D5) |
| Substance sourcing per site | Medium | Nullable FK on `Ingredient`, added by 10c (Q4) |
| Alternate / local substance names | **Low** | Additive collection; deliberately not modelled on an unsupported reading (D7) |

### ADR to write

**[ADR-058](../../adr/ADR-058-substances-are-shared-facts-ingredients-are-roles.md) — *Substances are shared facts; ingredients are the roles they play*. ✅ Written 2026-08-03.** Forced, because it makes a cross-context ownership decision (`Substance` in ReferenceData, referenced from Product), establishes the `CodedConcept` seam the whole platform will use, and opens the first write path into reference data. Written **before** implementation, per canon.

---

## Phase 3 — Stories

Each is a vertical slice: domain → persistence → API → UI → test. Cut from `epic/EPIC-010a-substance-and-composition`.

### S001 — `Substance`, shared and extensible ✅ 2026-08-03
As a **regulatory user**, I want a substance catalogue I can search and extend, so that our proprietary compounds sit beside the ones everyone knows.
- [x] `Substance` root, shared-plus-extensible filter, copying `AuthorityDivision`
- [x] `CodedConcept` value object with `System` populated `regos-internal`
- [x] Seeded substance set + seeded class/type vocabulary
- [x] Create a **tenant-owned** substance; shared rows refuse mutation (D2)
- [x] Substance directory UI — search, filter shared vs proprietary
- [x] **ADR-058 written first**

**Two departures from the plan above, both narrowing and both additive to reverse.**

| | What shipped | Why |
|---|---|---|
| `IsActive` | **not built** | The founder scoped lifecycle management to EPIC-012, which leaves nothing in S001 able to write it. A persistent property with no acquisition path is the defect EPIC-007a spent three findings on. |
| name uniqueness | **built** — a tenant may not add a name already in the catalogue it can see | A unique index covers `(TenantId, Name)` and cannot express *"and not one the shared catalogue already carries"*. Without it the directory forks **Q1**'s answer on its first screen. Exact-name only; the fuzzy matching and merge workflow the backlog calls duplicate detection remains EPIC-012's. |

**How "shared rows refuse mutation" is satisfied:** structurally, not by a guard.
`ISubstanceRepository` adds and reads, and nothing loads a substance for
mutation or saves a change — so there is no path to stand a guard on. The guard
belongs on the first mutation that exists, which is EPIC-012's.

`UniiCode` ships and is writable by a tenant, so the GSRS seam is a column with
an acquisition path rather than a placeholder; **every seeded row leaves it
null**, which is the claim ADR-058 §6 requires.

### S002 — `PharmaceuticalProductDetail`, dose form and route ✅ 2026-08-04
As a **regulatory user**, I want to record what a product is in its market — dose form, route, unit of presentation — so that the market view says more than a name.
- [x] `PharmaceuticalProductDetail` as its own root, hanging off `MedicinalProduct`
- [x] Seeded dose-form, route and unit-of-presentation vocabulary
- [x] `RoutesOfAdministration` as an owned collection (D6)
- [x] Presentation panel on the market view
- [x] `AtcCode` on `MedicinalProduct`, as a value object over a string

**Three decisions, taken in the Phase-3 conversation of 2026-08-04.**

| | Decision | Why |
|---|---|---|
| **Root, not child** | `PharmaceuticalProductDetail` is its own aggregate | Composition and commerce move on different clocks. As a child it would drag `Ingredient` into the market aggregate, so every trade-name edit would load and re-save composition — and each load is one more `Include` to remember, which EPIC-019 has already paid for once. **This supersedes EPIC-017's change-case prediction**, and a correction note is recorded there. |
| **Several per market** | No uniqueness on `(MedicinalProductId, Name)` | 10 mg, 20 mg and 40 mg tablets are one commercial presence. Forcing 1:1 would make a tenant duplicate the whole market — its trade names, its history, its licences — to record the second strength. |
| **`Strength` moved to S003** | Not built here | The checklist above originally placed the value object in S002, but the entity table puts the field on `Ingredient`. S002 would have shipped a value object nothing constructs — the defect S001 declined for `IsActive`. The vocabularies do not overlap: unit of presentation counts articles, strength units measure quantity. |

**`Version` not built.** RIM carries one; nothing writes or reads it. Recorded as a seam.

**`AtcCode` is a value object over a string, not a `CodedConcept`** (founder refinement). `("who-atc", …)` would assert WHO named it, and RegOS holds no WHO ATC licence to check that against. The type validates the *shape* — the five-level alternation, partial codes accepted — and derives `Levels` so *"show me every analgesic"* is a prefix match. It cannot validate membership, and its refusal says so.

### S003 — `Ingredient` — composition
As a **regulatory user**, I want to state which substances a product contains and in what role, so that composition is data rather than a PDF.
- [ ] `Ingredient` as an owned child of `PharmaceuticalProductDetail` (D3)
- [ ] Role + `Strength` per ingredient; at least one active ingredient required
- [ ] Composition editor

### S004 — `MedicinalProductComponent` — the recursive tree
As a **regulatory user**, I want to describe a kit or a co-packaged presentation, so that what the patient receives is represented.
- [ ] Recursion via adjacency list, depth guard, cycle prevention (D5)
- [ ] **Depth test** — a component within a component within a component
- [ ] Component tree UI

### S005 — Capstone
As a **regulatory user**, I want to ask which products contain a substance, so that an impact assessment is a query.
- [ ] *"Which products contain substance X?"* across markets, through the API and the UI
- [ ] Browser proof: seed a substance → add a proprietary one → build a presentation with two ingredients → ask the question
- [ ] Retro, per Phase 5 — every decision above gets an outcome, including any that failed

**Done when:** tests green · browser-verified · epic branch not left broken · ADR-058 merged before S002.
