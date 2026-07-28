# EPIC-001 — The Regulatory Data Dictionary

**Status:** 🟢 Complete (all 7 stories shipped; ready to merge to `main`) · **Branch:** `epic/EPIC-001-regulatory-data-dictionary` · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

Complete Reference Data as the governed, standards-aligned controlled-vocabulary + dossier-blueprint backbone of the RIM, seeded for **FDA IND (CTD)**.

---

## Phase 1 — Epic plan

### Outcome
A Regulatory Data Steward can inspect a governed, standards-aligned set of controlled vocabularies and a versioned dossier blueprint for **FDA IND (CTD)** — every reference list carries provenance, status, effective dates, and correct global-vs-tenant ownership — so that later features (submission planning, validation, registration tracking) all reference **one trusted backbone**. No submission consumes it yet.

### In scope ✅
- A shared **governance shape** for reference data (provenance, status lifecycle, effective dating, ownership).
- Standards-aligned taxonomy: geography (Region / Country / Language), authorities & frameworks (US IND/NDA…), product classification, document types.
- The **dossier blueprint**: RegulatoryTemplate → Version (immutable, effective-dated) → Section tree (region-specific Module 1 + harmonized 2–5) → RequiredDocument → ValidationRule (rows only).
- Seed one **published FDA IND** template (thin CTD slice) + pharma vocab.
- A read-only **Regulatory Data Dictionary Explorer** page (user-visible capstone).

### Out of scope ⏸️ (deferred, with reason)
| Deferred | Why |
|---|---|
| Authoring / change-control UI for reference data | Platform-curated & seeded for now; no user authors yet (YAGNI) → EPIC-012 |
| Tenant-cloned/custom templates | Ownership column present day one; feature earned on demand (Rule of Three) → EPIC-012 |
| Full IDMP substance/product model | On the IDMP *path* via classification + core descriptors; depth later → EPIC-010 |
| `RegulatoryProgram` intermediate | IND maps fine via SubmissionType→Authority; add when a pathway needs it |
| Validation *execution*, submission binding, publishing gate | That's EPIC-002 — here rules are just data |

### Definition of Done (epic)
- All reference entities carry the governance shape; existing ones aligned.
- FDA IND blueprint exists as a published, immutable version with sections, required docs, and rules — verifiable via API + seed-integrity tests.
- The Explorer page renders the IND blueprint end-to-end, browser-verified.
- `main` never broken; ADRs written for the governance shape, ownership model, and any forced decisions.

---

## Phase 2 — Domain design

Designed one area at a time; each entity gets columns + a change-case (future-proofing) table.

### Area A — Governance foundation (the shared shape)  _PROPOSED — under review_

The common attributes every controlled vocabulary carries. Turns four inconsistent lookup tables into a governed dictionary.

| Attribute | Type | Purpose | Status today |
|---|---|---|---|
| `Id` | strongly-typed id | identity | exists (per entity) |
| `Code` | string, normalized, **immutable** | stable business/standard key | exists on all |
| `Name` | string | display label | exists on all |
| `Description?` | string? | optional detail | only DocumentType |
| `Source` | string (e.g. `ISO 3166-1 alpha-2`, `ICH eCTD`, `EDQM`, `RegOS`) | **provenance** — "says who" | **new** |
| `Status` | enum `Active \| Deprecated` | governed lifecycle; never delete, retire | replaces bare `IsActive` |
| `EffectiveFrom` / `EffectiveTo?` | date / date? | **temporal validity** — what was valid when | **new (seam)** |
| `TenantId?` | `TenantId?` (null = global-shared) | ownership (ADR-030/031) | only DocumentType |
| `CreatedOnUtc` | datetime | audit stamp (`LastModifiedOn` → EPIC-013) | only DocumentType |

**Change-case analysis**

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| Vocabulary values added/retired over time | High | reference rows + `Status=Deprecated`; never enums, never delete |
| A standard publishes a new version (eCTD v4, EDQM update) | Med-High | `Source` + `EffectivePeriod` let old & new coexist |
| Tenant needs its own variant of a list | Medium | `TenantId?` present from day one |
| "Which vocabulary was valid at submission time?" | Medium | `EffectiveFrom/To` (temporal seam) — columns now, query logic later |
| Multilingual display names | Medium | **defer** — add a translations table later (cheap); do NOT build now |
| Hierarchy (region→country→authority) | High | modeled per-entity via typed relations, **not** in the base shape |
| Full audit of reference-data changes | Medium | `CreatedOnUtc` now; complete trail via EPIC-013 |

**Design decision (ADR-worthy):** apply the shape via a small base type `ReferenceItem<TId>` in `ReferenceData.Domain` (+ an `EffectivePeriod` value object), rather than copy-paste columns. Rationale: 8+ entities share it → Rule of Three is well past satisfied; a base type makes the governance guarantee real, not aspirational. → propose **ADR: Reference-data governance shape**.

**Cost / seam-vs-build:** aligning the existing four (Country, Authority, SubmissionType, DocumentType) needs added columns + one migration + seed updates (SubmissionType/DocumentType `IsActive` → `Status`). We add `EffectiveFrom/To` **columns** now (cheap seam; expensive to retrofit) but build **no temporal-query logic** yet (YAGNI).

### Area B — Geography & markets  _TBD_
### Area C — Authorities & regulatory frameworks  _TBD_
### Area D — Product classification & core descriptors  _TBD_
### Area E — Dossier blueprint

**Template + Version** — built in STORY-002 (see [ADR-034](../../adr/ADR-034-regulatory-templates-are-versioned-shared-blueprints.md)).

**Template sections (STORY-003)** — `TemplateSection`, a node owned by a version. Columns: `Id`, `Code` (CTD code, case preserved), `Title`, `ParentSectionId?` (null = top-level module), `Order`. Invariants: code unique within a version; parent must belong to the same version; added only while the version is `Draft`. **Decisions (approved 2026-07-27):** (1) tree = adjacency list (parent pointer + order), not nested-set; (2) structure mutable only while `Draft`, frozen on publish; (3) dev blueprint re-seeded (reset) each story as it grows — dev reference data is disposable. The section↔document link is a `TemplateSectionId` FK on `RequiredDocument` (STORY-004), not on the section.

**Required documents (STORY-004)** — `RequiredDocument`, a placeholder owned by the version (flat), pointing *up* at its `TemplateSectionId` and typed by a `DocumentTypeId` from the controlled vocabulary (never a file). Columns: `Id`, `SectionId`, `DocumentTypeId`, `IsMandatory`, `Order`. Invariants: section must belong to the draft version; one requirement per `(section, documentType)`; added only while `Draft`. **Decisions (approved 2026-07-28):** (1) owned by the *version* (flat list with `SectionId`), consistent with sections — the section↔document FK lives on the placeholder; (2) `DocumentTypeId` is a real FK with `Restrict` (a document type a blueprint requires can't be deleted); `SectionId` stays a plain converted column; (3) uniqueness is presence-only — one requirement per `(section, documentType)` — deferring true cardinality (min/max copies) and conditionality; (4) the ADR-034 "shared-never-references-tenant" rule holds by construction for now (only shared templates reference shared document types); real enforcement waits for authoring (EPIC-012).

**Validation rules (STORY-005)** — `ValidationRule`, a checkable constraint owned by the version (flat) — beyond structure (sections) and content (required docs). Columns: `Id`, `Code` (unique per version), `RuleType` (closed enum), `Severity` (`Error`/`Warning`), `SectionId?` (null = whole version), `Parameters?` (rule-type payload, e.g. `pdf`), `Message`, `Order`. Invariants: draft-only; `Code` required & unique within the version; a section-scoped rule's section must belong to the draft. **Pure data — no execution; the engine that runs these is EPIC-002.** **Decisions (approved 2026-07-28):** (1) mandatory-presence stays *derived* from S004's `IsMandatory`, never restated as a `DocumentRequired` rule — one source of truth; (2) targeting is version- or section-scoped only (`SectionId?`); document-type targeting is a later nullable-column add; (3) `Parameters` is a single nullable string interpreted per `RuleType`, not typed columns per type; (4) `ValidationRuleType` is a deliberately **closed** code enum — new types arrive with code + tests + migration, never as user data — starting at `{ FileFormat, SectionNotEmpty }` with `Severity { Error, Warning }`.

---

## Phase 3 — Stories

**Pragmatic re-sequence (2026-07-26):** to reach a working app fastest we defer the governance-shape *retrofit* of the existing four vocabularies (Area A) and the RIM-breadth areas (B/C/D). Governance fields are baked directly into the new blueprint entities (greenfield, no migration pain); the retrofit + breadth return as later stories/epics.

| # | Story | Status |
|---|---|---|
| **STORY-001** | Seed pharma taxonomy — FDA `IND` & `NDA` submission types + CTD document types (Cover Letter, FDA 1571, IB, Nonclinical/Clinical Overview, Drug Substance 3.2.S, Drug Product 3.2.P); make reference-data seeding **additive + idempotent** | ✅ Done — build green, live-verified via API, 466 tests green |
| **STORY-002** | `RegulatoryTemplate` + `RegulatoryTemplateVersion` (governed, versioned, publish→immutable) + read API | ✅ Done — 2 aggregates + migration + seed (FDA IND v1) + read API; [ADR-034](../../adr/ADR-034-regulatory-templates-are-versioned-shared-blueprints.md); 14 domain tests; live-verified; 480 tests green |
| **STORY-003** | Template sections (CTD module tree) | ✅ Done — `TemplateSection` adjacency-tree, draft-only; migration; sections in read API; FDA IND seeded with a 7-section CTD slice; 11 domain tests; live-verified; 491 tests green |
| **STORY-004** | Required documents per section (typed by DocumentType) | ✅ Done — `RequiredDocument` owned by the version (points up at its section), draft-only, DocumentType FK `Restrict`, unique per (section, doc type); migration; flat `requiredDocuments` in read API; FDA IND seeded with 6 required docs; 9 domain tests; live-verified; 500 tests green |
| **STORY-005** | Validation rules (data only — closed rule-type set) | ✅ Done — `ValidationRule` owned by the version, draft-only, closed `ValidationRuleType {FileFormat, SectionNotEmpty}` + `Severity {Error, Warning}`, version- or section-scoped, `Parameters` string; unique code per version; migration; flat `validationRules` in read API; FDA IND seeded with 3 rules (PDF-format Error, M1 non-empty Error, M4 non-empty Warning); 11 domain tests; live-verified; 511 tests green |
| **STORY-006** | Seed the published FDA IND (CTD) blueprint | ✅ Done — representative CTD skeleton (section families, not every leaf; IB at 1.13, numbering template-driven): **38 sections** (M1 IND essentials + harmonized M2–M5, 3.2.S/3.2.P one level deep), **13 required docs**, **4 validation rules**; **21 document types** (+6 IND artifacts: FDA 1572/3674, protocol, QOS, nonclinical/clinical summaries); seed-only (no new schema); live-verified full tree via API; 511 tests green |
| **STORY-007** | Reference Data / Blueprint viewer page (bare-bones, browser-verified) | ✅ Done — `features/regulatory/templates/` slice (api/hooks/types/components/pages, matching house conventions); Templates list + blueprint detail lit up the pre-existing "Templates" nav stub at `/regulatory/templates`; flat→tree shaping client-side; doc-type IDs resolved via existing `useDocumentTypes`; `npm run build` green; Playwright `templates.spec.ts` asserts the FDA IND blueprint renders end-to-end (structure + required docs + rule badges) and captures a screenshot — screenshot eyeballed |

## Phase 5 — Retro

**Shipped (7 stories, 2026-07-26 → 2026-07-28):** the metadata backbone proven end-to-end on **US · FDA · IND (CTD)**. A governed, versioned, publish-immutable dossier blueprint — template → version → section tree → required documents → validation rules — seeded with a representative FDA IND blueprint (38 sections, 13 required docs, 4 rules, 21 document types) and rendered read-only in the app, browser-verified.

**Final gate:** backend **511/511**; browser suite **46 relevant pass** — incl. `templates.spec.ts` (blueprint renders end-to-end) and the new `blueprint-seed-integrity.spec.ts` (API-level canary: 38 sections / 13 docs / 4 rules, published + immutable). One pre-existing, branch-independent red — see Follow-ups.

**Definition of Done — reconciled against Phase 1**

| DoD bullet | Outcome |
|---|---|
| All reference entities carry the governance shape; existing ones aligned | **Partial — consciously descoped.** New blueprint entities carry it; the four legacy vocabularies (Country/Authority/SubmissionType/DocumentType) were *not* retrofitted (Area-A deferral). Moved out of this epic → follow-up. |
| FDA IND blueprint published + immutable with sections, required docs, rules — verifiable via API + seed-integrity tests | **Met.** 38/13/4, published + effective-dated; verified via API and two browser specs (`templates`, `blueprint-seed-integrity`). |
| Explorer renders the IND blueprint end-to-end, browser-verified | **Met.** Playwright `templates.spec.ts` + screenshot. |
| `main` never broken; ADRs for governance shape, ownership, forced decisions | **Met for ownership** (ADR-034) and `main` (untouched until merge). The *governance-shape* ADR (Area-A base type) was **not** written — it belongs with the deferred retrofit, not this slice. |

**What went well**
- **Vertical slices held.** Every story shipped build-green, tested, and live/browser-verified before commit; `main` was never touched until the merge. The domain grew one aggregate-child at a time (sections → required docs → rules) on the same `RegulatoryTemplate` root.
- **Greenfield governance beat retrofit.** Baking provenance/versioning into the *new* blueprint entities (and deferring the Area-A retrofit of the four legacy vocabularies) got us to a working slice without migration pain — the pragmatic re-sequence paid off.
- **Decisions captured as we went.** ADR-034 + the per-story decision logs (draft-only mutation, one-source-of-truth for mandatory presence, closed rule-type enum) mean the *why* survives.
- **The frontend already had a seam.** A dangling "Templates" nav stub meant the capstone lit up an existing hole rather than inventing navigation.

**What to watch / carried forward**
- **Seed is hardcoded C# builders.** Fine at this size, but the real RegOS thesis is *regulatory knowledge as versioned data, not code*. Seeding blueprints from a data file (JSON/YAML) is a real architectural evolution — **its own epic**, not a mid-epic detour. Flagged during S006, deferred deliberately.
- **Deferred debt, tracked:** Area-A governance-shape retrofit of Country/Authority/SubmissionType/DocumentType; RIM breadth (geography/authority/product-classification, Areas B–D); document-type-level rule targeting; true cardinality on required docs; temporal-query logic on the effective-dating seam. All are columns-now-logic-later seams, not rework.
- **Additive-by-id seeding never *updates* an existing row** — a changed blueprint needs a DB reset in dev. Acceptable for disposable dev data; revisit if/when reference data is authored rather than seeded (EPIC-012).
- **Ownership invariant is by-construction, not enforced.** "A shared blueprint must not reference tenant-owned reference data" (ADR-034) holds because everything seeded is shared; real enforcement waits for authoring.

**Verification approach that worked:** throwaway Postgres DBs + a second API instance on port 5301 (never disturbing the founder's running stack on 5225), and — for the capstone — the existing Playwright harness driving real Chrome against the real API, with the screenshot read back by eye. The blueprint now has its own API-level canary (`blueprint-seed-integrity.spec.ts`) that pins the seeded shape so a future seed drift fails loudly.

**Follow-ups (tracked, not blocking this epic)**
- **Area-A governance-shape retrofit** of the four legacy vocabularies + its ADR — the one descoped DoD item. Own story/epic.
- **Blueprint-from-data-file** (JSON/YAML seeding) — the "regulatory knowledge as data, not code" evolution flagged in S006. Own epic.
- **Dev-DB hygiene / org canary.** `seed-integrity.spec.ts` (demo *organizations*) fails on the shared dev DB: prior browser runs left "Browser … Org" residue and mutated away the demo Manufacturer + Sponsor orgs — a spec is violating the "own the data you mutate" rule (ADR-019). **Not an EPIC-001 change** (the epic branch touches zero org/tenant code, verified via `git diff main...HEAD`); it fails identically on `main`. Fix: reset the dev DB and/or the offending org spec, separately.
