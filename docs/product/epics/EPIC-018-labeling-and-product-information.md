# EPIC-018 — Labeling & product information

**Status:** ⚪ Not Started · **Branch:** `epic/EPIC-018-labeling-and-product-information` (cut at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

The approved **content** of a product — what it treats, who must not take it, what it does to you, what it clashes with — held as structured data per market rather than as a PDF nobody can query.

> **Phase 1 below is settled.** **Phases 2–3 are a sketch**, written so this epic can be picked up months from now without re-deriving it — they are **not approved design**. Confirm, amend or replace them in the Phase-2 conversation when this epic is pulled into **Now**.

---

## Phase 1 — Epic plan

### Outcome
A regulatory user can hold the **global label** (the company core data sheet) as a governed, versioned artifact, derive **local labels** per market from it, and query the structured product information — indications, contraindications, undesirable effects, interactions, each scoped to a **population** — instead of reading a document. *"Which markets is this indication approved in?"* becomes a query.

### The concepts it introduces

| Cluster | RIM objects | Attrs |
|---|---|---|
| **Label artifacts** | Global Label, Labeling (local), Artwork | 15 + 13 + 10 |
| **Structured product information** | Indication, Contraindications, Undesirable Effects, Interactions, Interactant | 15 + 12 + 8 + 10 + 3 |
| **Shared qualifiers** | Population, Other Therapy | 11 + 4 |

**10 RIM objects — the largest single block in the whole model** outside the IDMP tail.

### Depends on
- **EPIC-017** — RIM hangs `Global Label` off **Global Product** and `Labeling` (local label) off **Medicinal Product**. Without the market-local tier there is nowhere to put a local label, and the whole point of this epic is that labels differ by market.
- Reuses **`ProductDocument`/`DocumentVersion`** as RIM's `Content` — the file behind a label. Do not build a second document store.

### In scope ✅
- **`GlobalLabel`** — name, type, versioned with dated status, language(s), responsible department/person, change summary, phonetic spelling, US suffix; linked to `Content`.
- **`Labeling`** (local label) — language, NDC labeler code, SKUs, translations, version; derived-from link to another local label; per-country, on the market-local product.
- **`Artwork`** — language, data-carrier code, SKUs, version, dated status; child of a local label; linked to packaging (seam only until EPIC-010).
- **`Indication`** — disease/symptom/procedure, full text, language, disease status, comorbidity, intended effect, duration, category, dated status.
- **`Contraindication`**, **`UndesirableEffect`**, **`Interaction`** (+ **`Interactant`**) — the remaining structured sections.
- **`Population`** — age + unit + range, gender, race, physiological condition — attachable to any of the four above.
- **`OtherTherapy`** — relationship type + therapy, attachable to indications and contraindications.
- Label workspace UI, browser proof, ADR.

### Out of scope ⏸️ (deferred, with reason)
| Deferred | Why |
|---|---|
| **Label authoring / rich-text editing** | RegOS holds the *structured facts* and points at the document. A label editor is a different product. |
| **Automated translation** | Infrastructure. `Translations` is a recorded fact, not a service. |
| **Label change-control workflow** (review, approve, effective-date a change) | → **EPIC-008**. Versions and dated statuses are modelled here; the approval *process* is not. |
| **Artwork ↔ packaging component linkage** | Needs `Packaging` → **EPIC-010**. Nullable seam only. |
| **Coding the vocabularies to MedDRA / SNOMED / ICD** | The *shape* is a coded name/value pair from day one; licensing and loading a real terminology is a procurement question, not a modelling one. Seed a small controlled list, keep the seam. |
| **Cross-market label comparison / divergence reports** | → **EPIC-011**. This epic delivers the data those charts would read. |
| **Structured Product Labeling (SPL) / PLR export** | → **EPIC-007**. |

### Definition of Done
- A global label exists for a global product, versioned, with a dated status history and a linked content file.
- A local label exists for a **market-local** product, records the global label it derives from, and carries its own language and version.
- Artwork can be attached to a local label with its own dated status.
- Indications, contraindications, undesirable effects and interactions can be recorded against a market-local product, each optionally scoped to a population.
- The same population shape serves all four — proven by a test that exercises it under at least two different parents.
- *"Which markets is indication X approved in?"* is answerable through the API.
- Browser proof: create a global label → derive a local label for one market → add an indication with a paediatric population → see it on the product's market view.
- ADR written for the label hierarchy and the shared-qualifier (`Population`) modelling.

---

## Phase 2 — Domain design *(sketch — not approved)*

### The interesting modelling problem: shared qualifiers

RIM's `Population` has **four optional parent links** — Indication, Contraindication, Undesirable Effect, Interaction — exactly one of which is set. `Other Therapy` has two. This is a polymorphic-parent shape, and it is the design decision of this epic.

Three options, to be settled on pull-in:

| Option | Shape | Trade-off |
|---|---|---|
| **A — four nullable FKs** | RIM's literal shape | Faithful; needs a check constraint that exactly one is set; every query knows about four columns |
| **B — owned child per parent** | `Indication.Populations`, `Contraindication.Populations`, … | Cleanest aggregate boundaries, no polymorphism; the `Population` *shape* is shared as a value object, the *table* is not |
| **C — one table + discriminator** | single `Population` table with parent type + id | Fewest tables; loses referential integrity, which is a poor trade in a regulated record |

*Lean: **B***. It keeps each clinical statement a self-contained aggregate, preserves FK integrity, and shares the shape where sharing actually helps (the value object). RIM's four-nullable-FK shape is a **relational-modelling artifact**, not a domain truth — the domain truth is "a clinical statement applies to a population."

### Entities *(abbreviated — full RIM attribute lists in the source model)*

**`GlobalLabel`** — root, on **Global Product**. Name, type, version number, status + status date (historical), language(s), responsible department/person, change summary, global trade name phonetic spelling, US suffix, sponsor (inherited), `ContentId`s.

**`Labeling`** (local label) — root, on **Medicinal Product**. Language, NDC labeler code, SKUs, translations, version number, `DerivedFromLabelingId?` (RIM: "Local/Regional Label Derived From"), `CountryId`s, `ContentId`s, links to packaged product (seam).

**`Artwork`** — child of `Labeling`. Language, data-carrier code, SKUs, version, status + status date.

**`Indication`** — root, on Medicinal Product. Coded disease/symptom/procedure, full text, language, disease status, comorbidity (multiple), intended effect, duration, category, identifier, status + date. Owns `Population`s and `OtherTherapy`s.

**`Contraindication`**, **`UndesirableEffect`** (symptom/condition/effect, classification, frequency of occurrence), **`Interaction`** (type, effect, incidence, management, severity) + **`Interactant`** (item, optional `SubstanceId` — seam to EPIC-010).

**`Population`** — value object / owned entity: age + age unit, age range low/high, gender, race, physiological condition.

### Decisions to settle (Phase 2, on pull-in)

1. **Population modelling — A, B or C** (above). *Lean B.*
2. **Context placement.** New `src/Labeling/` vs extending `src/Product/`. *Lean: new context* — the clinical-content cluster is large enough (10 objects) to stand alone, and it depends on Product rather than being part of it.
3. **Do indications live on the label or on the product?** RIM says both (`Medicinal Product → Indication` child, and `Application → Indication` controlled vocabulary). *Lean: on the market-local product*, with labels referencing them — an indication is approved for a product in a market, and appears *in* the label.
4. **Coded values.** Every clinical field is a RIM `Name/Value Pair` (code + display). Model a `CodedConcept` value object once (`system`, `code`, `display`) and reuse it everywhere, so swapping a seed list for real MedDRA later touches loading, not the model.
5. **Versioning.** `GlobalLabel` is versioned with historical status; `Labeling` has a version number. *Lean: reuse the `RegulatoryTemplate`/`RegulatoryTemplateVersion` pattern* — it already solves draft → publish → freeze with effective dating, and this is the same problem.

### Change-case analysis

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| Real terminology (MedDRA/SNOMED) replaces the seed list | **High** | `CodedConcept` carries a `system` from day one |
| Cross-market divergence reporting (EPIC-011) | High | Structured per-market data is exactly the input |
| Label change control / approval (EPIC-008) | High | Versioned + dated status already; the workflow attaches |
| SPL / PLR export (EPIC-007) | Medium | Structured sections map to SPL sections |
| Artwork tied to specific packaging components | Medium | Nullable seam to `Packaging` (EPIC-010) |
| A population qualifier needed on a new statement type | Medium | Option B: add the owned collection to the new aggregate — no shared-table migration |
| Indications approved on different dates per market | Medium | Indication lives on the market-local product; RIM even has "approval date for each indication in a country" on Application |

---

## Phase 3 — Candidate stories *(sketch — re-slice on pull-in)*

| # | Story | Slice |
|---|---|---|
| **S001** | **`GlobalLabel`** — versioned, dated status, linked content, on the global product | domain → persistence → API → UI → test |
| **S002** | **`Labeling`** (local) + **`Artwork`** — per market, derived-from link, own version and status | full slice |
| **S003** | **`Indication`** + **`Population`** (the shared-qualifier decision lands here) + `OtherTherapy` | full slice |
| **S004** | **`Contraindication`** + **`UndesirableEffect`** — reusing the population shape | full slice |
| **S005** | **`Interaction`** + **`Interactant`** | full slice |
| **S006** | **Capstone** — label workspace on the market view, *"which markets is this indication approved in?"*, browser proof, ADR, retro | UI → test → docs |

**ADR to write:** *The label hierarchy, and how shared clinical qualifiers are modelled* — next free number.
