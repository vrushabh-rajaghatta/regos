# EPIC-018 — Labeling & product information

**Status:** 🟡 In Progress · **Branch:** `epic/EPIC-018-labeling-and-product-information` (cut 2026-08-04) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

The approved **content** of a product — what it treats, who must not take it, what it does to you, what it clashes with — held as structured data per market rather than as a PDF nobody can query.

> **Pulled into Now 2026-08-04.** Phase 1 is unchanged. **Phases 2–3 were a
> sketch and have been replaced** by the design reviewed and signed off in the
> Phase-2 conversation, recorded as [ADR-059](../../adr/ADR-059-clinical-statements-are-facts-labels-are-artifacts.md).
> The sketch's own words are kept in [what the sketch got wrong](#what-the-sketch-got-wrong)
> rather than quietly overwritten.

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

## Phase 2 — Domain design *(approved 2026-08-04)*

The guiding principle, and the reason it is written before the entities:

> **A clinical statement is a regulatory fact about a product in a market. A
> label is an editorial artifact that publishes some of those facts at a point
> in time.**

Full argument in [ADR-059](../../adr/ADR-059-clinical-statements-are-facts-labels-are-artifacts.md).
This section records *what was decided*; the ADR records *why*.

### The six decisions

| # | Decision | Settled as |
|---|---|---|
| **D1** | **A new `src/Labeling/` bounded context**, holding *both* clusters | ✅ approved — distinct ubiquitous language, distinct primary users (Medical Writing, not Regulatory Operations). Splitting labels from clinical statements would draw a boundary through one editorial act |
| **D2** | **`Population` is an owned entity per parent**, with its own identity | ✅ approved — it is added, edited, removed, reordered and approved, which is lifecycle, not a value. Mapping duplication is answered by **EF configuration helpers**; a shared *domain* type is not introduced (ADR-018, ES-014) |
| **D3** | **Clinical statements hang off `MedicinalProduct`**; no link to a label version | ✅ approved — an indication is approved for a product in a market; the label publishes it. The publication link is five versioning questions in a trench coat, and is deliberately deferred |
| **D4** | **Versioning copies the `RegulatoryTemplate` *pattern*, not its code** | ✅ approved — reuse the root-plus-version shape, draft/publish/supersede and effective dating; reuse neither its `record struct` id nor its ReferenceData assumptions. Identity from `CommitmentId` (ADR-043) |
| **D5** | **`LocalLabel` references a `ProductDocumentId`**; `ProductDocument` gains nothing | ✅ approved, with the rationale stated as a rule: **documents remain content storage; Labeling owns the regulatory meaning.** Stated so `ProductDocument` does not accrete market semantics one epic at a time |
| **D6** | **Domain names and screen words are separate**, and the type never follows the screen | ✅ approved — `GlobalLabel` stays `GlobalLabel` whatever users call it out loud |

### One departure from the sketch, arising from D1

**RIM's local-label object is called `Labeling`; the aggregate here is
`LocalLabel`.** A context named `Labeling` containing a type named `Labeling`
reproduces the namespace-equals-type collision that S000 removed fourteen `using`
aliases to delete. It also names the pair symmetrically with `GlobalLabel`.
Mechanical reason, not a modelling one — and the only place this epic departs
from RIM's noun ([ADR-059](../../adr/ADR-059-clinical-statements-are-facts-labels-are-artifacts.md) §2).

### Aggregates

| Root | Hangs from | Owns |
|---|---|---|
| **`GlobalLabel`** | `GlobalProductId` | `GlobalLabelVersion` — number, status + status date, effective from/to, change summary, `ProductDocumentId`s |
| **`LocalLabel`** | `MedicinalProductId` | `Artwork` — language, data-carrier code, SKUs, version, status + date |
| **`Indication`** | `MedicinalProductId` | `Population`, `OtherTherapy` |
| **`Contraindication`** | `MedicinalProductId` | `Population`, `OtherTherapy` |
| **`UndesirableEffect`** | `MedicinalProductId` | `Population` |
| **`Interaction`** | `MedicinalProductId` | `Population`, `Interactant` |

`LocalLabel` also carries `DerivedFromGlobalLabelVersionId?` — which core version
this was written from. That is a **derivation** fact, not a publication one, and
is exactly the link D3 does *not* defer: it says where the text came from, not
which approved statements it prints.

### Screen words

Recorded in `docs/domain-model/labeling.md` at S001.

| Domain type | Screen |
|---|---|
| `GlobalLabel` | **Global label** |
| `LocalLabel` | **Local label** |
| `UndesirableEffect` | **Side effect** |
| `Population` | **Who it applies to** |
| `OtherTherapy` | **Used with** |

*Global label* is the plain word, held until real users are asked — Medical
Affairs says *CCDS*, Regulatory says *Core Data Sheet*. Whichever wins, the type
does not move.

### What the sketch got wrong

Kept rather than overwritten, because a corrected prediction is worth more than
a tidy document.

| The sketch said | What changed it |
|---|---|
| *"Model a `CodedConcept` value object once (`system`, `code`, `display`) and reuse it everywhere"* — listed as decision 4 | **Already built.** EPIC-010a S001 put it in `ReferenceData.Domain` per [ADR-058](../../adr/ADR-058-substances-are-shared-facts-ingredients-are-roles.md) §3. EPIC-018 inherits it — and inherits its trap: an owned coded value is tracked against exactly one owner, so every lookup returns a fresh instance |
| *"`Population` — value object / owned entity"* | **Entity, decided.** The sketch left it either/or; lifecycle settles it |
| *"`Labeling` (local label) — root, on Medicinal Product"* | **Renamed `LocalLabel`**, for the namespace collision above |
| *"links to `Content`"* — treated as a solved seam | **`ProductDocument` is scoped to `GlobalProductId` and has no market dimension.** Found during Phase 2, not during implementation. D5 is the answer, and it is a rule about ownership of meaning rather than a schema change |

### Change-case analysis

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| Real terminology (MedDRA/SNOMED/ICD) replaces the seed list | **High** | `CodedConcept.System` carries `regos-internal` from day one; the swap is a data migration |
| Someone asks which statements a label version printed | **High** | Nothing points that way yet, so the answer is additive. ADR-059 §3 names the five questions it must answer first |
| Label change control / approval (EPIC-008) | High | Versioned with dated status already; the workflow attaches to it |
| Cross-market divergence reporting (EPIC-011) | High | Per-market structured statements are exactly the input |
| SPL / PLR export (EPIC-007) | Medium | Structured sections map to SPL sections |
| A fifth statement type needs `Population` | Medium | Add the owned collection to the new root — no shared-table migration. A fifth is also the trigger to re-ask ADR-018's question |
| Artwork tied to specific packaging components | Medium | Nullable seam to `Packaging` (EPIC-010) |
| Indications approved on different dates per market | Medium | The statement already lives on the market-local tier |

---

## Phase 3 — Stories

| # | Story | Slice | Status |
|---|---|---|---|
| **S001** | **`GlobalLabel` + `GlobalLabelVersion`** — the new context, versioned with dated status and effective dating, linked content, on the global product | context → domain → persistence → API → UI → browser proof | ⚪ |
| **S002** | **`LocalLabel` + `Artwork`** — per market, derived-from link to a core version, own language, version and status | full slice | ⚪ |
| **S003** | **`Indication` + `Population` + `OtherTherapy`** — D2 lands here, once, on one parent | full slice | ⚪ |
| **S004** | **`Contraindication` + `UndesirableEffect`** — the second and third uses of the population shape, and where ADR-018's question is asked out loud | full slice | ⚪ |
| **S005** | **`Interaction` + `Interactant`** — **the stop-or-continue point** | full slice | ⚪ |
| **S006** | **Capstone** — *"which markets is indication X approved in?"* end to end, label workspace on the market view, browser proof, retro | query → UI → test → docs | ⚪ |

> **S005 is where to stop if the epic runs long**, and that is decided now rather
> than under pressure. Nothing else depends on interactions; every story before
> it establishes the backbone. Cutting it leaves the Definition of Done unmet and
> the architecture whole — which is the better of the two failures available.

**ADR:** [ADR-059](../../adr/ADR-059-clinical-statements-are-facts-labels-are-artifacts.md) — written before S001, as canon requires for a new bounded context.
