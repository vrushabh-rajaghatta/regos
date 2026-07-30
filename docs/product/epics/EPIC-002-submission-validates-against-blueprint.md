# EPIC-002 — Submission validates against the blueprint

**Status:** 🟡 In Progress · **Branch:** `epic/EPIC-002-submission-validates-against-blueprint` · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

Bind a Submission to a published template version and replace the hardcoded readiness validator with the **metadata-driven engine**, so publishing is gated on the blueprint EPIC-001 made data.

---

## Phase 1 — Epic plan

### Outcome
A regulatory user validates a draft submission against the **published blueprint for its authority + submission type**, sees exactly which **required documents are missing** and which **blueprint rules fail** (with severity), and can publish **only when no blocking error stands**. The three hardcoded readiness rules become rules *derived from reference data* — the first time the metadata engine does real work on customer data.

### In scope ✅
- **Resolve** the applicable published template version for a submission (submission type + authority → template → effective-dated published version).
- **Bind** the submission to that version, persisted, so validation and publishing are deterministic.
- A **metadata-driven evaluator** reading the bound version:
  - **Required-document coverage** — every mandatory `RequiredDocument`'s DocumentType is present among attached documents.
  - **`FileFormat`** — attached documents match the required format (e.g. `pdf`).
  - Severity respected: `Error` blocks publishing, `Warning` informs.
- Keep the existing data-integrity guard (attached document version still exists).
- Blueprint-derived issues surfaced in the validate API and the Submission validation UI.

### Out of scope ⏸️ (deferred, with reason)
| Deferred | Why |
|---|---|
| **Section-level placement** — which slot each document fills | That is the dossier builder → EPIC-003. Here coverage is by **DocumentType**, not by section. |
| **`SectionNotEmpty` execution** | Honestly cannot be evaluated until documents are *placed into sections* (EPIC-003). The engine reads the rule and reports it as **not yet evaluated** — never silently passed. |
| Rule types beyond the seeded set | Added when seeded; `ValidationRuleType` is a closed governed enum (EPIC-001 decision 4). |
| Re-binding / migrating a submission to a newer template version | Needs a policy conversation (what happens to an in-flight dossier). Bind-at-creation now. |
| eCTD sequences & lifecycle beyond Draft/Published | → EPIC-004 |
| Template authoring | → EPIC-012 |

### Definition of Done
- A submission created for a seeded pharma submission type resolves and persists its bound published template version (FDA IND + CA/AU/IN); submissions with no matching template stay unbound and keep working.
- Validation reports required-document and rule issues derived from the bound blueprint, each with a stable code and severity.
- Publishing is blocked while any `Error` stands, permitted when clean, and still captures the snapshot.
- Evaluator unit tests + browser verification of the full loop (create → validate → attach → validate clean → publish).
- ADR written for the binding model / metadata-driven validation.

---

## Phase 2 — Domain design

### Area A — Blueprint binding (STORY-001)

`Submission` gains one field: `BoundTemplateVersionId` (`RegulatoryTemplateVersionId?`) — the published blueprint version the submission is judged against, pinned at creation. Nullable column + index + `Restrict` FK.

**Resolution** (in `CreateSubmissionHandler`, which already holds the application and validated submission type): active templates targeting the submission type → tenant-owned before shared → within the chosen template, the published version effective today → highest version number.

**Change-case analysis**

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| A newer template version publishes mid-flight | High | Binding **pins** the version — the reason it is persisted rather than resolved live |
| Submission types with no template (devices) | High (now) | Nullable; creation never fails on missing reference data |
| Re-bind / upgrade an in-flight submission | Medium | Column exists; needs a policy decision, so deferred |
| Tenant-owned templates (EPIC-012) | Medium | Resolution already prefers tenant-owned over shared |
| Snapshot should record the bound version | Medium | Read-side addition later; no schema churn |

**Decisions (approved 2026-07-30):**
1. **Binding is optional and a no-match is not an error.** Incomplete or unpublished reference data must never block creating a submission — that is an operational/configuration gap, not a user mistake.
2. **Tenant-owned templates shadow shared ones**, deterministically, so the first EPIC-012 customization takes effect without changing resolution logic and ambiguous matches are impossible by design.
3. **Template present but no published version → unbound**, same rationale as (1) (template still being authored, admin unpublished, seed incomplete).
4. **The FK targets `RegulatoryTemplateVersion`** — a child entity, not an aggregate root — because the *version* is the immutable governance artifact; referencing only the template would force "which version?" at every validate/render/compare. A deliberate departure from "reference roots only", recorded in the epic ADR (STORY-004).

**Not done here:** nothing is validated yet, and a bound submission behaves exactly as before. This story only establishes the seam S002/S003 evaluate against.

### Area B — Required-document coverage (STORY-002) _TBD_
### Area C — Rule execution (STORY-003) _TBD_

---

## Phase 3 — Stories

| # | Story | Status |
|---|---|---|
| **STORY-001** | Bind a submission to its published template version (resolve at creation, persist, expose on the read API) | ✅ Done — `BoundTemplateVersionId` on `Submission` (nullable, `Restrict` FK to the version); resolution at creation prefers tenant-owned then newest effective published version; `boundTemplate` (code/name/version) on the submission read API; migration `AddSubmissionTemplateBinding`; 2 domain + 4 integration tests against real seeded data (FDA IND binds, 510(k) stays unbound); 517 tests green |
| **STORY-002** | Required-document coverage — missing mandatory documents become validation errors, gating publish | ⚪ |
| **STORY-003** | `FileFormat` rule execution, severity-aware (Error blocks, Warning informs) | ⚪ |
| **STORY-004** | Capstone — validation UX grouped by severity, browser-verified end-to-end loop, ADR, retro | ⚪ |

## Phase 5 — Retro
_At epic completion._
