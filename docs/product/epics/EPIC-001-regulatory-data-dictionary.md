# EPIC-001 — The Regulatory Data Dictionary

**Status:** 🟡 Planning · **Branch:** `epic/EPIC-001-regulatory-data-dictionary` · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

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
### Area E — Dossier blueprint (template → version → section → required doc → rule)  _TBD_

---

## Phase 3 — Stories

**Pragmatic re-sequence (2026-07-26):** to reach a working app fastest we defer the governance-shape *retrofit* of the existing four vocabularies (Area A) and the RIM-breadth areas (B/C/D). Governance fields are baked directly into the new blueprint entities (greenfield, no migration pain); the retrofit + breadth return as later stories/epics.

| # | Story | Status |
|---|---|---|
| **STORY-001** | Seed pharma taxonomy — FDA `IND` & `NDA` submission types + CTD document types (Cover Letter, FDA 1571, IB, Nonclinical/Clinical Overview, Drug Substance 3.2.S, Drug Product 3.2.P); make reference-data seeding **additive + idempotent** | ✅ Done — build green, live-verified via API, 466 tests green |
| **STORY-002** | `RegulatoryTemplate` + `RegulatoryTemplateVersion` (governed, versioned, publish→immutable) + read API | ⚪ Next |
| **STORY-003** | Template sections (CTD module tree) | ⚪ |
| **STORY-004** | Required documents per section (typed by DocumentType) | ⚪ |
| **STORY-005** | Validation rules (data only — closed rule-type set) | ⚪ |
| **STORY-006** | Seed the published FDA IND (CTD) blueprint | ⚪ |
| **STORY-007** | Reference Data / Blueprint viewer page (bare-bones, browser-verified) | ⚪ |

## Phase 5 — Retro
_At epic completion._
