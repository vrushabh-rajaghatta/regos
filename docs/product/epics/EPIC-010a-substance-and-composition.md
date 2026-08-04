# EPIC-010a — Substance & composition

**Status:** 🟢 Complete — S001–S005 delivered, retro below · **Branch:** `epic/EPIC-010a-substance-and-composition` (cut at Phase 2) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

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

### S003 — `Ingredient` — composition ✅ 2026-08-04
As a **regulatory user**, I want to state which substances a product contains and in what role, so that composition is data rather than a PDF.
- [x] `Ingredient` as an owned child of `PharmaceuticalProductDetail` (D3)
- [x] Role + `Strength` per ingredient
- [x] `Strength` value object (D4), moved here from S002, with its own measurement vocabulary
- [x] Composition editor

**Three decisions, taken in the Phase-3 conversation of 2026-08-04.**

| | Decision | Why |
|---|---|---|
| **Strength is orthogonal to presentation** | Both units come from a `MeasurementVocabulary` that shares no code with `UnitsOfPresentation` | *Founder instruction.* A denominator that could name an article would make *"500 mg per tablet"* expressible — repeating what the presentation already says, in a second place that can disagree. A point strength has no denominator, and the reader composes it with the dose form. |
| **The numerator/denominator shape is kept** | Not collapsed to `{Value, Unit}` | A point strength *is* `{Value, Unit}` — a strength with no denominator. Dropping the denominator would make *10 mg/mL* unrepresentable, and S002's own browser spec already put a solution for injection on screen. |
| **`IngredientRole` is an enum, not a `CodedConcept`** | Departure from the epic's entity table | The test applied: *does a rule branch on this value?* Nothing branches on dose form, route or substance class. Two rules branch on role — a composition may not lose its last active, and an active must declare a strength. A coded concept whose code a rule string-matches is an enum in a costume. **Revisit when a role arrives that no rule branches on** (adjuvant, stabiliser). |

**"At least one active ingredient" is split into two rules**, because the checklist's single sentence turned out to mean two things.

- **An anti-corruption invariant, enforced:** a composition that has an active may not be left with excipients and no active. Removing or demoting the last one is refused while others remain; emptying the composition entirely is allowed, because starting over is a different act from hollowing out.
- **A completeness statement, not enforced:** `HasAnActiveIngredient` is exposed and the screen says *"this composition does not say what the product works by."* Requiring an active on every edit would dictate the order a user types a formulation in, and RegOS settled long ago that completeness belongs at a gate.

**An active must declare a strength; an excipient need not.** An excipient's quantity is routinely *q.s.*, so its absence is a fact rather than a gap.

**`CodedConceptLookup` extracted.** `MeasurementVocabulary` is the third vocabulary, which is the occurrence ADR-018 was waiting for — S002 deliberately duplicated the resolution and recorded the trigger.

### S004 — `MedicinalProductComponent` — the recursive tree ✅ 2026-08-04
As a **regulatory user**, I want to describe a kit or a co-packaged presentation, so that what the patient receives is represented.
- [x] Recursion via adjacency list, depth guard, cycle prevention (D5)
- [x] **Depth test** — a component within a component within a component, and the fourth refused
- [x] Component tree UI

**The design question S004 had to answer first:** the depth and cycle rules need
the whole tree, but each component is its own root and a root can only see
itself. The answer is **`ComponentTree`** — a pure domain structure over one
market's components, passed into every operation that changes the shape:

```
component.ReparentTo(newParentId, tree)   →  the tree refuses a cycle or an over-deep move
```

Nothing is encoded in persistence (Postgres cannot express acyclicity for an
adjacency list without a trigger), the guard and the mutation cannot be
separated, and there is **one home for traversal** rather than recursive helpers
accumulating across handlers — the read uses the same walk, so a row's depth on
screen is the depth the guard measured.

| | Decision | Why |
|---|---|---|
| **`MaxDepth = 3`** | a domain constant, not a schema limit | The schema represents whatever tree it is given; the domain decides which trees are accepted, so changing it is a decision rather than a migration. Three is one level past anything demonstrated — a pen is one, a kit and its contents is two. |
| **Move is built, though the epic did not list it** | `PUT /api/components/{id}/parent` | Without it a cycle is impossible by construction and the guard would be vacuous. It is also the correction path for an article put at the top level by mistake. |
| **Remove refuses rather than cascades** | a component holding others must be emptied first | Removing a kit and silently taking its contents is quiet data loss. |
| **`ComponentTypes` is its own vocabulary** | overlaps `UnitsOfPresentation` almost entirely | One says what a strength is counted in, the other what the patient is handed. Merging them would put `KIT` in a strength picker, and *"10 mg per kit"* is not a sentence. |

**A test that could not be written, and what replaced it.** An attempt to force
two components into a cycle — to exercise the visited-set guards in the walks —
failed: every route goes through `RequireCanReparent`, and swapping the order
only changes which move is refused. The test now asserts *that*, and the guards
stay unexercised on purpose — they protect against database state the domain
cannot produce, and a walk that hangs starves a thread pool.

### S005 — Capstone ✅ 2026-08-04
As a **regulatory user**, I want to ask which products contain a substance, so that an impact assessment is a query.
- [x] *"Which products contain substance X?"* across markets, through the API and the UI
- [x] Browser proof of the whole chain, in the order a person would live it
- [x] Retro, per Phase 5 — below

**Done when:** tests green ✅ · browser-verified ✅ · epic branch not left broken ✅ · ADR-058 merged before S002 ✅

`GET /api/substances/{id}/products`, and the panel that asks it sits on the
substance's own row. The handler lives in **`Product.Application`**: `Product →
ReferenceData` is an established edge and the reverse is not, so placing it
beside `Substance` would invert a dependency for a read. The same reasoning
ADR-058 §3 used for `CodedConcept`, and the shape EPIC-019 used for
`ListStudyFilings` — *a real question spanning two contexts is a read, and a
read grants nobody write ownership.*

**One isolation decision worth knowing.** The walk starts at the presentation,
not at the ingredient. A query filter applies per entity type, and `Ingredient`
is a child with no `TenantId` and therefore no filter — `Set<Ingredient>()`
would read every tenant's compositions. Starting from the fail-closed
`PharmaceuticalProductDetails` is what confines the join to the caller.

**The answer carries market status**, because an impact assessment is about what
is on sale. A planned market and a launched one are very different phone calls.

---

## Phase 5 — Retro

### What shipped, against the Phase-1 question

| | Question | Answered? |
|---|---|---|
| **Q1** | *"Which of our products contain substance X?"* | ✅ End to end, and browser-proved across two markets |
| **Q2** | *"What is in this product, at what strength, in what dose form, by what route?"* | ✅ Presentation + composition on the market page |
| **Q3** | *"What does the patient physically receive?"* | ✅ Component tree, three levels, depth-tested |
| **Q4** | *"Which products use a substance sourced from site Y?"* | **Rejected in Phase 2, and stayed rejected.** No column was added. |

**1319 tests, 18/18 suites · 105/105 browser specs · five additive migrations.**

### Four decisions changed during implementation

Not mistakes — each is a case where building the next story revealed something
the plan could not have known. Recorded so future planning documents are read as
what they are: the best guess before the work.

| Decision | Planned | Implemented | What changed it |
|---|---|---|---|
| `CodedConcept` location | `src/Product/` | `ReferenceData.Domain` | Dependency analysis: `Substance` carries two of them, so `Product` as its home would invert `Product → ReferenceData`. Corrected in **ADR-058 §3** before any code. |
| `PharmaceuticalProductDetail` | child of `MedicinalProduct` (EPIC-017's prediction) | its own root | `Ingredient` established a separate consistency boundary. As a child it would drag composition into the market aggregate, so every trade-name edit would load and re-save it. Correction note recorded **in EPIC-017**, not only here. |
| `Strength` | S002 | S003 | Its first consumer is `Ingredient`. Shipping it in S002 would have been a value object nothing constructs. |
| `IngredientRole` | `CodedConcept` | `enum` | Two rules branch on it. A coded concept whose code a rule string-matches is an enum in a costume. |

**The test that produced all four:** *does a rule branch on this value, and what
is the smallest thing that can enforce it?* It placed `CodedConcept`, split
`Strength` from presentation units, made `IngredientRole` an enum, and produced
`ComponentTree`.

### What the change-case analysis got right, and what it missed

**Right.** `CodedConcept.System` earned its place three times over — every
vocabulary shipped is `regos-internal` and says so, in code, in the seed file and
on screen. The nullable `Inn` was exactly the field a proprietary compound
needed. Ingredients-at-component-level stayed unbuilt and nothing missed them.

**Missed — three things the table did not predict:**

1. **An owned `CodedConcept` cannot be shared between owners.** EF tracks one
   against exactly one owner, so the six-compound seed persisted five substances
   with a null class. Caught by the API tests, and now guarded in one place.
2. **A shadow foreign key is nullable by default**, which is the
   *optional-FK-severs-instead-of-deleting* trap the identity conventions
   already name. Hit on `Ingredient`.
3. **`Ingredient` needed a parameterless constructor** that `TradeName` beside
   it does not — owned types cannot bind to constructor parameters.

All three are persistence-shaped, none was visible from the domain model, and
each cost one build-test cycle. **That is the argument for the browser suite
rather than for a better model.**

### Decisions to promote

- **"Does a rule branch on it?"** is the test that separates terminology from a
  domain type. Worth a line in the implementation standards; it is not currently
  written anywhere.
- **A rule about a structure belongs on a type that *is* the structure.**
  `ComponentTree` is the instance; the general form is that an invariant no
  single aggregate can see needs a domain type that can. Candidate ADR when a
  second one appears — not yet.
- **`CodedConceptLookup` was extracted on the third occurrence, not the
  second**, and the second copy carried a comment saying so. ADR-018 worked
  exactly as written, and that is worth knowing.

### Carry-forward

| | Inherited by |
|---|---|
| `Version` on `PharmaceuticalProductDetail` — seam recorded, not built | whichever epic demonstrates a reader |
| Sourcing (`Ingredient → Organization`) — Q4, rejected | **EPIC-010c** (cluster D) |
| Ingredients beneath a component — one parent demonstrated, not two | evaluate at the **second** occurrence (D3) |
| Steward CRUD, change control, shared-row editing on `Substance` | **EPIC-012** |
| Unit conversion — `Strength` equality is literal, `10 mg/mL ≠ 1 g/100 mL` | whichever epic needs comparison or xEVMPD render |
| `RouteOfAdministration` on studies should resolve to the same `CodedConcept` | **EPIC-019 S005** — 10a established the shape |
| **Completion of EPIC-010a does not imply IDMP or xEVMPD readiness** (D1) | stated at the start, still true at the end |
