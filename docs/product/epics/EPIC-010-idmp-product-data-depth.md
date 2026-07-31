# EPIC-010 — IDMP / product data depth

**Status:** ⚪ Not Started · **Branch:** `epic/EPIC-010-idmp-product-data-depth` (cut at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

What the product actually **is**: substances, composition, strength, presentation, packaging, shelf life, and who manufactures which step where. The long tail of the DIA RIM model — **16 objects**, the largest remaining block.

> **Phase 1 below is settled.** **Phases 2–3 are a sketch**, written so this epic can be picked up months from now without re-deriving it — they are **not approved design**. Confirm, amend or replace them in the Phase-2 conversation when this epic is pulled into **Now**.
>
> ⚠️ **This epic is too large to run as one.** See [Splitting](#splitting) — expect to break it into three before cutting a branch.

---

## Phase 1 — Epic plan

### Outcome
A market-local product can describe itself the way a regulator asks it to: **which substances, at what strength, in what dose form, by what route, in what pack, made where**. This is what makes xEVMPD/IDMP submission, ATC reporting and substance-level impact analysis (*"which products contain this API?"*) possible.

### Depends on
- **EPIC-017** — everything here hangs off the **market-local product tier**. Strength, presentation and packaging are market-specific facts; without that tier they would attach to a global product and be wrong.
- **EPIC-016** — `Mfg Business Operation` and `Manufacturing Process` reference **`OrganizationSite`**.

### The 16 objects, in four clusters

| Cluster | RIM objects |
|---|---|
| **A — Substance & composition** | Substance, Ingredient, Pharmaceutical Product Detail, Route of Administration, Medicinal Product Components |
| **B — Physical & device** | Physical Characteristics, Other Characteristics, Devices, Shelf Life-Storage |
| **C — Packaging & supply** | Packaged Product, Packaging, Legal Status of Supply |
| **D — Manufacturing** | Manufacturing Process, Manufacturing Process Step, Mfg Process Step Materials, Mfg Business Operation |

### Splitting

Run as **three epics**, cut when this is pulled into Now. Suggested IDs at that time:

| Split | Clusters | Rationale |
|---|---|---|
| **10a — Substance & composition** | A | The IDMP root. Unblocks *"which products contain this API?"* on its own — a genuine standalone outcome. |
| **10b — Presentation & packaging** | B + C | What the patient receives. Depends on 10a for composition. |
| **10c — Manufacturing** | D | Depends on 10a (materials) and EPIC-016 (sites). The most self-contained; can slip without blocking the others. |

Do not attempt all three in one branch — it would be the largest epic in the project by a wide margin and would sit unmerged for months, against the flow's *"keep epics small enough to complete before they drift."*

### In scope ✅ *(across the three splits)*
- **`Substance`** — substance name, class, type, CAS number, INN, chem/bio description, molecular formula, UNII code + name.
- **`Ingredient`** — role (active/excipient), links substance ↔ component ↔ pharmaceutical product detail, manufacturing source organization.
- **`PharmaceuticalProductDetail`** — name, description, route(s) of administration, strength, unit of presentation, dose form, version.
- **`MedicinalProductComponent`** — recursive component tree: type, name, description, unit of presentation, quantity, dose form, lot serialization.
- **`PhysicalCharacteristics`**, **`OtherCharacteristics`**, **`Devices`**, **`ShelfLifeStorage`**.
- **`PackagedProduct`**, **`Packaging`** (recursive), **`LegalStatusOfSupply`**.
- **`ManufacturingProcess`**, **`ManufacturingProcessStep`**, **`MfgProcessStepMaterials`**, **`MfgBusinessOperation`**.
- Wiring `Registration` → `PackagedProduct` and `MfgBusinessOperation` → `OrganizationSite` (both RIM links RegOS currently cannot express).

### Out of scope ⏸️ (deferred, with reason)
| Deferred | Why |
|---|---|
| **xEVMPD / IDMP message generation** | → **EPIC-007**. This epic makes the data correct and complete; the publishing engine renders the ISO IDMP messages. |
| **Substance master-data sourcing** (GSRS, UNII loading) | Integration. UNII/CAS fields are the seam; loading a real substance register is a procurement and data-ops question. |
| **Bill-of-materials / batch genealogy** | Manufacturing execution, not regulatory information. Hard line. |
| **Specifications and analytical methods** | Belongs in the CMC document, not as structured RIM data. RIM does not model it either. |
| **Serialization / track-and-trace (DSCSA, FMD)** | `Lot Serialization` is a recorded attribute on the component, not a serialization system. |
| **Stability study data** | `Shelf Life-Storage` records the *conclusion* (36 months at 25 °C); the study behind it is EPIC-019 territory. |

### Definition of Done *(per split; the epic completes when all three do)*
- A market-local product's composition is expressible down to the substance, with roles and strengths, and *"which products contain substance X?"* is answerable.
- Recursive structures (components within components, packaging within packaging) are supported and depth-tested — RIM makes both self-referencing.
- A packaged product exists with its packaging tree, legal status of supply and market status, and a `Registration` can name it.
- A manufacturing business operation links a product, an operation type, a date range and an `OrganizationSite`.
- Browser proof per split.
- ADR written for the composition model (recursion + the substance/ingredient split).

---

## Phase 2 — Domain design *(sketch — not approved)*

### The two recursions — the defining structural feature

RIM has **two self-referencing hierarchies** here, and getting them wrong is expensive to undo:

1. **`Medicinal Product Components`** → `Component Parent ID` (a component contains components — e.g. a kit containing a vial and a syringe).
2. **`Packaging`** → `Packaging` (Parent) — a pallet contains cartons contain blisters contain tablets.

Both need: a depth guard, cycle prevention, and a query strategy (recursive CTE, or a materialised path). Decide **once**, apply to both. This is the ADR.

### Cluster A entities *(abbreviated)*

**`Substance`** — root, its own context or reference data? See decision 1. Substance name, class, type, CAS number, INN (generic name), chem/bio description, molecular formula, UNII code + name.

**`Ingredient`** — the join with meaning: `SubstanceId`, `Role` (active/excipient), `ManufacturingSourceOrganizationId`, and a parent that is *either* a `MedicinalProductComponent` *or* a `PharmaceuticalProductDetail` (RIM: both are Parent, Multiple, Required — the same polymorphic-parent problem EPIC-018 solves for `Population`; **reuse whatever decision that epic made**).

**`PharmaceuticalProductDetail`** — name, description, routes of administration (multiple), strength (complex value: numerator/denominator with units), unit of presentation, dose form, version.

**`Strength`** — a value object, not a string. Numerator value + unit, denominator value + unit. RIM types it "Complex values"; storing "50mg/5ml" as text makes every downstream comparison impossible.

### Decisions to settle (Phase 2, on pull-in)

**1. Is `Substance` reference data or tenant-owned master data?** *Lean: reference data with tenant extension* — the same shared-plus-extensible shape as `DocumentType` and `RegulatoryTemplate` (nullable `TenantId`, shared rows visible to all). Substances are world facts (paracetamol is paracetamol), but tenants will have proprietary compounds pre-INN. This is the highest-leverage decision in the epic.

**2. The two recursions** (above) — one strategy, applied twice.

**3. Polymorphic parents for `Ingredient`.** Same problem as EPIC-018's `Population`. **Do not solve it twice differently.**

**4. `Strength` as a value object** with numerator/denominator + units.

**5. Does `Registration` point at `PackagedProduct`?** RIM says yes (`License → Packaged Product`, Parent, Single). This is what finally lets a licence say *which pack* it authorises. Confirm at 10b; it is a change to the Registration aggregate and touches EPIC-005's work.

**6. Split boundaries** — confirm the 10a/10b/10c cut before cutting any branch.

### Change-case analysis

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| xEVMPD / IDMP export (EPIC-007) | **High** | The whole epic is the payload; completeness matters more than shape |
| Real substance register loaded (GSRS/UNII) | **High** | Reference-data shape with tenant extension (decision 1) means loading is a seed, not a migration |
| *"Which products contain substance X?"* impact analysis | **High** | Substance as a shared root, not a string on an ingredient |
| Deeper component/packaging nesting than anticipated | Medium | Recursion strategy chosen once, with a depth guard |
| Strength expressed as a range, not a point | Medium | Value object can gain a range without changing callers |
| Device-led combination products | Medium | `Devices` is modelled; `ProductType.CombinationProduct` already exists |
| Country-specific legal status of supply | Medium | RIM already scopes `Legal Status of Supply` by jurisdiction |

---

## Phase 3 — Candidate stories *(sketch — re-slice at the split)*

**10a — Substance & composition**

| # | Story |
|---|---|
| S001 | `Substance` as shared-plus-extensible reference data + substance directory |
| S002 | `PharmaceuticalProductDetail` + `Strength` value object + routes of administration |
| S003 | `Ingredient` — substance ↔ product with role and source (the polymorphic-parent decision lands here) |
| S004 | `MedicinalProductComponent` — the recursive component tree, depth-guarded |
| S005 | Capstone — *"which products contain substance X?"*, browser proof, ADR, retro |

**10b — Presentation & packaging**

| # | Story |
|---|---|
| S001 | `PackagedProduct` — name, type, contained quantity, dated status |
| S002 | `Packaging` — the recursive packaging tree, materials |
| S003 | `LegalStatusOfSupply` + `ShelfLifeStorage` |
| S004 | `PhysicalCharacteristics`, `OtherCharacteristics`, `Devices` |
| S005 | Capstone — `Registration` names its packaged product, browser proof, retro |

**10c — Manufacturing**

| # | Story |
|---|---|
| S001 | `ManufacturingProcess` + `ManufacturingProcessStep` |
| S002 | `MfgProcessStepMaterials` |
| S003 | `MfgBusinessOperation` — product ↔ site ↔ operation type ↔ effective dates *(needs EPIC-016)* |
| S004 | Capstone — *"which sites make this product?"*, browser proof, retro |
