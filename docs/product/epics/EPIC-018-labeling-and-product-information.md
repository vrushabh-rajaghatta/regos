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
| **S001** | **`GlobalLabel` + `GlobalLabelVersion`** — the new context, versioned with dated status and effective dating, linked content, on the global product | context → domain → persistence → API → UI → browser proof | ✅ |
| **S002** | **`LocalLabel` + `LocalLabelRevision`** — the market's own controlled document, its revision history, and artwork as a type rather than an aggregate | full slice | ✅ |
| **S003** | **`Indication` + `Population` + `OtherTherapy`** — D2 lands here, once, on one parent | full slice | ⚪ |
| **S004** | **`Contraindication` + `UndesirableEffect`** — the second and third uses of the population shape, and where ADR-018's question is asked out loud | full slice | ⚪ |
| **S005** | **`Interaction` + `Interactant`** — **the stop-or-continue point** | full slice | ⚪ |
| **S006** | **Capstone** — *"which markets is indication X approved in?"* end to end, label workspace on the market view, browser proof, retro | query → UI → test → docs | ⚪ |

> **S005 is where to stop if the epic runs long**, and that is decided now rather
> than under pressure. Nothing else depends on interactions; every story before
> it establishes the backbone. Cutting it leaves the Definition of Done unmet and
> the architecture whole — which is the better of the two failures available.

**ADR:** [ADR-059](../../adr/ADR-059-clinical-statements-are-facts-labels-are-artifacts.md) — written before S001, as canon requires for a new bounded context.

---

## S002 — the workflow question, and its answer

The story was parked on one question, because its shape could not be settled
from RIM:

> **Can the identity of the current local label change while its parent global
> label version stays the same?**

**Answered 2026-08-04: yes, and not marginally.** The founder's answer is
recorded here at length because it is the justification for a versioned local
model, and a future reader should be able to check the reasoning rather than
inherit the conclusion.

### Why, in the industry's terms rather than the model's

> **A core label is the company's scientific position. A local label is a
> regulatory artifact approved by one health authority.** Related, and not the
> same document — which is why the regulator regulates the second.

```
CCDS v7 published
      │  translation · artwork · assessment · submission
      │  PMDA review · questions · approval
      ▼
Japan Label Revision 14      effective 3 Oct 2026, derived from CCDS v7
```

Japan already had thirteen revisions. They are Japan's regulatory history, not a
projection of the company's.

**And the local artifact changes without the global one.** A translation
correction, a typo, an artwork problem, a distributor address — the CCDS is
untouched and Japan issues Revision 15. Meanwhile CCDS v8 is adopted by France
immediately, by Brazil six months later, by Australia with extra wording, and by
Japan next quarter. Every market holds a different current revision, approval
date and effective date.

**The operational question, restated better than we first asked it.** We had
planned to ask *"do you replace the PDF?"* — a software question. The regulatory
one is:

> *"When a local approved label changes for any reason, do you issue a new
> controlled revision, or overwrite the existing approved label?"*

Overwriting an approved labelling document is a governance failure. Approved
labelling is a controlled document and historical versions are retained. The
commercial RIM systems separate Company Core Data Sheet, Country Label and
Country Label Revision for exactly this reason.

**Artwork is the strongest case, not the weakest.** Manufacturer changes,
printer changes, barcodes, serialisation, QR codes, local legal statements,
distributor information — none touches a CCDS, and every one produces newly
approved local labelling.

### What this does not license

**Not symmetry.** Both tiers are versioned, and the reasons are unrelated:

| | Versioned because |
|---|---|
| `GlobalLabel` | the company's scientific position evolves |
| `LocalLabel` | each authority approves, delays, amends and republishes that position independently |

They intersect only through a derived-from link. Nothing inherits, and the two
status vocabularies are not assumed to match until a rule says they do.

**Not the approval workflow.** Submission, review, questions and approval are
the process; EPIC-018 records the *dated facts* it produces, and the process
itself stays EPIC-008's (see Out of scope).

### The four decisions that followed *(approved 2026-08-04)*

| # | Decision | Settled as |
|---|---|---|
| **D1** | **`LocalLabelRevision`, not `LocalLabelVersion`** | ✅ — the asymmetry is the point. *Version* is the company's word for its evolving position; *revision* is the authority's word for a controlled document it approved. Two names in one context is a standing reminder that the rules differ |
| **D2** | **Artwork is a `LocalLabel` type, not a fourth aggregate** | ✅ **for this epic**, with a watchpoint below. Prescribing information, patient leaflet and carton artwork are all controlled, authority-approved, market-specific, derived and revision-controlled — one lifecycle, one approval model, one set of APIs and one browser experience |
| **D3** | **`DerivedFromGlobalLabelVersionId` is nullable** | ✅ — a migrated portfolio will not know which core version Revision 9 came from, and a local-first company holds approved labelling before any core label exists here. Required would force people to invent history, which is always the wrong trade |
| **D4** | **`ApprovedOn` is separate from `EffectiveFrom`/`EffectiveTo`** | ✅ — *approved 12 May, effective 1 June* and *approved 12 May, effective immediately* both occur. **A revision cannot enter force without an approval date**: the local analogue of *publishing requires a document*, and a statement about the artifact's truth rather than a workflow step |

### The artwork watchpoint

> **Split when artwork develops its own persistent invariants — not when it
> acquires more attributes.**

Nullable columns are not the signal. `AtcCode` on `MedicinalProduct` is one
already, and it has caused nobody any trouble. The signal is **branching**:

```csharp
if (Type == Artwork)   // ← more than occasionally, and the aggregate is asking to split
{
    …
}
```

If pack size, SKU, GTIN, barcode, printer and pack configuration stop being
optional metadata and become *mandatory business concepts*, they are no longer
decorations on a label — they are the identity of a different thing, and
`CartonArtwork` should be its own root.

**The test to apply:** does every invariant apply equally to every local label
type? While the answer is yes, one aggregate. The moment the domain is written
with type checks, that answer has changed.

*S002 arms this as `LocalLabelTypeBranchTests` once `LocalLabelType` exists — a
count over `src/Labeling/**/Domain`, not a judgement call made a year from now
by whoever is reading.*

### Two things settled on the way

**Independent document evolution creates the history; audit merely obliges us to
retain it.** Not the other way round. An earlier draft of this analysis had *"a
regulator may ask what the Japanese label said on 3 March 2025"* as the reason to
version, which would have let a compliance requirement justify an aggregate.

**`LocalLabel` is not a projection over `GlobalLabel`** — adoption lag alone
settles that, whatever else it holds.

---

## S003 — the criterion it will be reviewed against

S003 is where D2's original decision — **`Population` is an owned entity, not a
value object** — gets tested the way S002 tested `LocalLabelRevision`.

> **The test is whether the aggregate naturally develops operations, not whether
> it holds a collection.**

| If `Indication` grows | Then |
|---|---|
| `AddPopulation`, `AmendPopulation`, `RetirePopulation` | the entity decision was right: a qualifier that is corrected in place has identity |
| only `AddPopulation`, with amendment meaning remove-and-re-add | it is a value object, and the decision should be reversed while it is cheap |

Build it **once, on one parent**. The second and third uses arrive in S004, and
that is when [ADR-018](../../adr/ADR-018-rule-of-three.md)'s question gets asked
out loud — not before.

---

## S002 — what was decided while building it

| Decision | Why |
|---|---|
| **A revision cannot take effect before it was approved** | Not in the design, and not a workflow rule: a label in force ahead of its own approval is not a state that exists. Same day is allowed — *effective immediately* is ordinary |
| **`LocalLabelRevisionStatus` is its own enum**, three identical words to the global one | Merging them would let a rule added to one lifecycle silently reach the other, which is the exact coupling D1 renamed the type to prevent |
| **`PrepareRevision` restates rather than patches** | Document, derivation, artwork code and summary are settled together. A caller able to change the document without the derivation could point a translation of core v7 at a file that says v8 |
| **A `core-versions` query, flattened across the product's core labels** | The question a person asks while preparing a Japanese revision is *"which core version is this?"*, not *"which core label, then which version"*. Drafts are excluded — a market cannot descend from something the company has not issued. Superseded ones are included, because a market catching up is the ordinary case |
| **`DataCarrierCode` only; no SKUs** | Artwork's one identifying attribute is one nullable column. Pack size, SKU and GTIN are EPIC-010's packaging model, and building a second one here would be the speculative creation ADR-018 forbids |
| **Markets are created through the UI in the browser proof** | The `/master-data/countries` route is unprefixed (an SC-001 grandfathered entry) and resolving it from a spec would bake that in. Clicking through the market page is what a user does anyway |

**Verified:** 19/19 suites, 0 failed, **1355 tests** · **107/107 browser specs**
against an isolated stack · CORS reverted and confirmed absent from `src/`.

---

## S001 — what was decided while building it

Recorded here rather than only in the commit, because each was a call the Phase-2
design did not make.

| Decision | Why |
|---|---|
| **No `Status` on `GlobalLabel`** | A label's meaningful lifecycle lives in its versions. "Retire this label" is a capability nobody asked for, and a column that is always `Active` is a field nobody filled in — the call `Substance` made on `IsActive`. **Known gap: a label cannot be renamed or retired**, stated so it is recognised as deferred rather than reported as a defect |
| **`DiscardDraft`, the one deletion here** | Without it a draft started by mistake is permanent: the one-open-draft rule blocks a replacement, and publishing needs content nobody intends to attach. It does not contradict ES-018 — a draft has never been in force, was never cited, and never described what the company said. The guard is `Draft`, not "not in force" |
| **Publishing requires content** | A version with no document is a number, and a number is not a label. This is what makes the `ProductDocumentId` link load-bearing rather than decorative, and it is the rule the browser proof exercises first |
| **Publish and supersede are one act** | A label family with two versions in force is not a state a company can be in. The supersede date is computed — the day before the replacement takes effect — because a caller who could supply it could leave a gap or an overlap |
| **`Aggregates/GlobalLabels/`, plural** | A singular folder makes the namespace equal the type name, which is the collision S000 removed fourteen `using` aliases to delete. Recorded in [slice-conventions](../../engineering/slice-conventions.md) v1.2, and `RegOSDbContext` names `GlobalLabel` with no alias as a result |
| **`CodedConceptDto` consolidated** | The label vocabulary was the **third** consumer, and the second had quietly written its own copy under a different name (`PharmaceuticalConceptDto`). Same on the frontend — `CodedConcept` and `CodedValue` are now one type in `shared/types/`. ADR-018's demonstrated need, retired opportunistically because this slice was already in those files |

**What the design did not predict, and cost a build cycle:** `GlobalLabel` needs
a *parameterless* constructor where `GlobalProduct` beside it does not — EF binds
constructor parameters from mapped properties, and an owned value object is not
one. That is the third persistence-shaped surprise in two epics, and it was in
EPIC-010a's retro. **Reading the retro would have caught it; the design review
did not.**

**Verified:** 19/19 test suites, 0 failed, 1335 tests · 106/106 browser specs
against an isolated stack (API 5301, web 5174, throwaway database) · CORS
widening reverted and confirmed absent from `src/`.
