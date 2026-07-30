# EPIC-002 — Submission validates against the blueprint

**Status:** 🟢 Complete (4 stories shipped; ready to merge to `main`) · **Branch:** `epic/EPIC-002-submission-validates-against-blueprint` · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

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

### Area B — Required-document coverage (STORY-002)

`BlueprintValidationEvaluator` — a collaborator of `SubmissionValidator`, not more branches inside it, so later capabilities (placement, cardinality, metadata, cross-document) arrive as sibling evaluators. It reads the bound version's mandatory `RequiredDocument`s, resolves attached documents' types (`SubmissionDocument → ProductDocument.DocumentTypeId`), and reports each uncovered type as an `Error` naming the document ("Required document 'Cover Letter' is missing."). No schema change.

**The correction this story carried:** `SubmissionValidationResult.IsValid` counted issues (`_issues.Count == 0`), which made *every* severity blocking and left the severity model — already defined, with a comment anticipating advisory rules — effectively unused. It now derives readiness from severity: **no `Error` ⇒ publishable**. Behaviour-preserving at the time (only errors existed), and a prerequisite for both the unbound-information issue and STORY-003's warnings. Treated as a bug fix.

**Decisions (approved 2026-07-30):**
1. **Coverage is by document type, deduplicated** — "is a document of this type attached?", not "is it in the right section". A type required by two sections is satisfied by one attachment; placement (EPIC-003) is what makes the finer question answerable. Documented in the evaluator itself rather than left implicit.
2. **An unbound submission emits an `Information` issue** rather than silently skipping the check — "not checked" must not look identical to "checked and clean". Non-blocking, because operating without a published blueprint is legitimate.
3. **`IsValid` = no `Error`-severity issues** (above), unit-tested directly on the result model, independent of any validator or database.
4. **Only mandatory requirements block.** The validator answers "can this proceed?"; "what else could go here" belongs to the content plan (EPIC-003).

**Consequence worth knowing:** an empty bound FDA IND submission now reports **13** specific missing-document errors alongside the generic `SubmissionHasNoDocuments`. Both are kept: the generic one reads as a summary, the specific ones are actionable. Grouping them is a presentation concern.

### Area C — Rule execution (STORY-003)

The engine gains an extension point. `BlueprintValidationEvaluator` becomes orchestration only: it gathers the facts once into a `BlueprintEvaluationContext` and runs two pipelines —

```
BlueprintValidationEvaluator            (orchestration, owns the queries)
 ├── RequiredDocumentCoverageEvaluator  ← derived from RequiredDocument rows
 └── rule loop over IBlueprintRuleEvaluator
      └── FileFormatEvaluator           ← CanEvaluate(rule)
```

Adding a rule type (Regex, MaxFileSize, Cardinality) is one class plus one DI registration; no switch grows and no existing evaluator changes. Because the context is passed as state rather than a `DbContext`, evaluators are pure and unit-testable without a database, and query count stays a property of the orchestrator.

**The asymmetry, deliberately preserved:** required-document coverage is derived from the blueprint's *structure* (`RequiredDocument` rows) and has no rule type, so it is **not** an `IBlueprintRuleEvaluator` — forcing it into `CanEvaluate(rule)` would be abstraction for its own sake. Two categories, two pipelines: derived semantics, and explicit rules.

**`BlueprintSeverityMapper` — correctness, not ceremony.** The two `ValidationSeverity` enums do not share ordinals: blueprint `Error = 1`, issue `Information = 1`. A cast (`(IssueSeverity)rule.Severity`) would have silently downgraded every blocking regulatory rule to a note, leaving `IsValid` true and publishing a submission that should have been stopped. The mapping is explicit, tested, and fails closed on an unrecognised grading.

**Decisions (approved 2026-07-30):**
1. **Disclosure over silence.** Rule types the engine cannot execute yet produce **one** `Information` issue carrying a structured `UnevaluatedRuleTypes` list — so clients never parse message text, and the list shrinks by itself as evaluators are added. Phrased purely as validator capability ("this validator does not yet execute these blueprint rule types: SectionNotEmpty"): it deliberately does **not** mention how the blueprint graded them, because "an Error rule was not evaluated" invites the reader to conclude they have an error, which is exactly what is not known. A regulated engine must distinguish **passed / failed / not evaluated**.
2. **Stable code plus dynamic rule code.** `Code = BlueprintRuleViolation` keeps the closed set consumers switch on; the blueprint rule's own code (`FDA-IND-PDF`) travels on a new nullable `RuleCode`, preserving regulatory traceability.
3. **Format detection: extension → content type → fail closed.** Filenames are what users and reviewers see; content types are assigned by whichever client uploaded and are unreliable for Office and archive formats. When neither establishes a format, the document is reported — inability to establish compliance is not compliance.
4. **Version-scoped `FileFormat` rules only.** A section-scoped rule asks which documents belong to that section, which needs placement (EPIC-003); such rules join the unevaluated disclosure rather than being applied dossier-wide as if version-scoped.

---

## Phase 3 — Stories

| # | Story | Status |
|---|---|---|
| **STORY-001** | Bind a submission to its published template version (resolve at creation, persist, expose on the read API) | ✅ Done — `BoundTemplateVersionId` on `Submission` (nullable, `Restrict` FK to the version); resolution at creation prefers tenant-owned then newest effective published version; `boundTemplate` (code/name/version) on the submission read API; migration `AddSubmissionTemplateBinding`; 2 domain + 4 integration tests against real seeded data (FDA IND binds, 510(k) stays unbound); 517 tests green |
| **STORY-002** | Required-document coverage — missing mandatory documents become validation errors, gating publish | ✅ Done — `BlueprintValidationEvaluator` reports each uncovered mandatory document type as a named `Error` (empty FDA IND ⇒ 13); unbound submissions report a non-blocking `Information` issue; **`IsValid` corrected to "no Error-severity issues"** (bug fix — severity was defined but unused); publish gate verified blocked; 6 result-model unit tests + 4 integration tests against the real seeded blueprint; 6 existing assertions re-scoped to errors; 527 tests green |
| **STORY-003** | `FileFormat` rule execution, severity-aware (Error blocks, Warning informs) | ✅ Done — `IBlueprintRuleEvaluator` + `FileFormatEvaluator`; coverage extracted to `RequiredDocumentCoverageEvaluator`; orchestrator gathers a `BlueprintEvaluationContext` once so evaluators are pure and DB-free; **`BlueprintSeverityMapper`** (the two severity enums do not share ordinals — a cast would have downgraded blocking rules to notes); unevaluated rule types disclosed as one `Information` issue with structured `UnevaluatedRuleTypes`; issues carry `RuleCode` (`FDA-IND-PDF`); 20 unit + 3 integration tests; 550 tests green |
| **STORY-004** | Capstone — validation UX grouped by severity, browser-verified end-to-end loop, ADR, retro | ✅ Done — validation page rebuilt: publishability and findings are independent (a ready submission still shows its notes), grouped Errors → Warnings → Information, `ruleCode` chips, structured `unevaluatedRuleTypes`; **issue ordering moved into the API contract**; browser journey `submission-validation.spec.ts` proves 14 blocking → attach via UI → 13 → complete → **Ready to publish (disclosure intact)** → published; [ADR-035](../../adr/ADR-035-submissions-bind-to-a-published-template-version.md); 554 backend + 49 browser tests green |

## Phase 5 — Retro

**Shipped (4 stories, 2026-07-30):** the metadata engine does real work. A submission binds to the published blueprint that governs it, is judged against that blueprint's required documents and rules, and cannot be published while anything blocks — visible end-to-end in the browser. This is the first vertical slice where reference data governs customer behaviour all the way to a user's decision.

**Final gate:** backend **554/554**; browser **49 pass, 1 pre-existing failure** (see Follow-ups).

**Definition of Done — reconciled**

| DoD bullet | Outcome |
|---|---|
| Submissions resolve and persist a bound published version; unmatched types stay unbound | **Met.** FDA IND binds; FDA 510(k) under the same authority stays unbound, verified against real seed data. |
| Validation reports required-document and rule issues with stable codes and severity | **Met.** 13 named missing-document errors + `FileFormat` execution, `RuleCode` for traceability. |
| Publishing blocked while an Error stands, permitted when clean, snapshot still captured | **Met**, proven in the browser journey. |
| Evaluator unit tests + browser verification of the full loop | **Met.** 20 of the rule tests need no database. |
| ADR for the binding model | **Met** — [ADR-035](../../adr/ADR-035-submissions-bind-to-a-published-template-version.md). |

**What went well**
- **The same bug, caught twice, at two layers.** `IsValid` counted issues, making every severity blocking and the severity model decorative; the validation page then rendered issues *only when invalid*, so a ready submission would have hidden its own disclosure. Both were found by asking "what does this claim when it has nothing to say?"
- **The severity mapper was correctness, not ceremony.** The two enums do not share ordinals (blueprint `Error` = 1, issue `Information` = 1), so a cast would have downgraded every blocking regulatory rule to a note. A guard test now asserts the collision exists, so nobody deletes the mapper as redundant.
- **Context-not-DbContext.** The orchestrator owning all persistence and handing evaluators immutable state made rule logic pure — most new tests need no database, and query count cannot grow with rule types.
- **The browser spec reads the blueprint to learn its own work**, so it asserts "every required document was satisfied" rather than "these thirteen strings appeared", and survives the template growing.

**What to watch / carried forward**
- **`SectionNotEmpty` remains unexecuted**, including an `Error`-severity FDA rule on Module 1.1. Disclosed rather than hidden, but the gap is real until placement exists (EPIC-003).
- **Coverage is by document type, not placement** — one attachment satisfies a type required by two sections.
- **Re-binding an in-flight submission to a newer template version** is unsolved and needs a policy decision.
- **A published submission does not record which blueprint version it was judged against.** The binding is on the submission, not the snapshot; worth adding when the snapshot is next touched.

**Follow-ups (tracked, not blocking)**
- **`seed-integrity.spec.ts` is stale, and my earlier diagnosis of it was wrong.** I first attributed the failure to residue from prior browser runs mutating the demo organizations. Running the suite against a *fresh* database disproved that: it fails there too. The demo organizations are each seeded into **their own tenant** (`TenantId == its own id`), so under the fail-closed query filter (ADR-031) the development user — who belongs to the Demo MAH tenant — can only ever see one of the three. The spec encodes a pre-tenant-isolation expectation. Fix is to assert only the organization the acting tenant can legitimately see; deliberately left out of this epic's commits.
