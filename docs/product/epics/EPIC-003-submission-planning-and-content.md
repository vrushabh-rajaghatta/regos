# EPIC-003 — Submission planning & content

**Status:** 🟢 Complete (4 stories shipped) · **Branch:** `epic/EPIC-003-submission-planning-and-content` · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

Give the dossier a **structure**. EPIC-002 made a submission answer *"do I have the right documents?"*; this epic makes it answer *"is each document in the right place?"* — and turns the blueprint's section tree into the working surface where a regulatory user builds the dossier.

---

## Phase 1 — Epic plan

### Outcome
A regulatory user opens a submission and sees the bound blueprint's tree of **sections**, each showing the **placeholders** it expects. Each placeholder is either satisfied by a real document or visibly empty. Assigning a document into a section changes what validation says, and validation still gates publishing.

This is the epic that retires two limits EPIC-002 consciously accepted and recorded in [ADR-035](../../adr/ADR-035-submissions-bind-to-a-published-template-version.md): coverage by document type rather than placement, and `SectionNotEmpty` disclosed-but-unexecuted.

### In scope ✅
- **Placement** — a submission document records which `TemplateSection` of the bound version it sits in.
- **Content plan / gap view** — a placeholder-shaped read model over the bound version plus the submission's placements.
- **Placement-aware coverage** — a placeholder is satisfied by a document of the right type *in that section*.
- **`SectionNotEmpty` execution** and **section-scoped `FileFormat`** — now evaluable because placement exists.
- **Unplaced documents disclosed** as `Information` — attached, but nowhere in the dossier.
- The **dossier builder UI**: the placeholder tree, assign/unassign, and the gap view.

### Out of scope ⏸️ (deferred, with reason)
| Deferred | Why |
|---|---|
| **Cardinality** (min/max copies per placeholder) | The blueprint cannot express it yet; adding it is reference-data authoring → EPIC-012. |
| **N/A with justification** | The first genuine *submission-owned* state, and therefore the trigger for materializing placeholder rows. Not needed to build a dossier. |
| **Ad-hoc sections not in the template** | Same trigger. A submission that needs a section the blueprint does not have is a blueprint gap first. |
| **Reviewer comments / per-placeholder workflow** | → EPIC-008. |
| **Re-binding to a newer template version** | Still an open policy question, unchanged from ADR-035. |
| eCTD sequences & lifecycle | → EPIC-004 |
| Template authoring | → EPIC-012 |

### Definition of Done
- A submission document can be placed into a section of its bound template version, and unplaced again; placement is impossible into a section belonging to any other version.
- `GET /submissions/{id}/content-plan` returns the section tree with explicit placeholders, each carrying a stable id, its expected document type, whether it is mandatory, and what satisfies it.
- Required-document coverage is evaluated by **(section, document type)**; a type required by two sections needs two placements.
- `SectionNotEmpty` and section-scoped `FileFormat` execute; the "not evaluated" disclosure shrinks accordingly and disappears when nothing is left unexecuted.
- Attached-but-unplaced documents are reported as `Information`, never counted toward coverage, never silently dropped.
- Browser proof of the whole loop: create → assign → gaps close → validate clean → publish.
- ADR-036 written.

---

## Phase 2 — Domain design

### The model

```
RegulatoryTemplateVersion (immutable — the blueprint owns everything)
  └── TemplateSection            "where does a document belong?"
        └── RequiredDocument     "is a minimum expected here?"  ← the placeholder

Submission (owns only deviations)
  └── SubmissionDocument
        └── TemplateSectionId?   ← the only new state in this epic
```

**Decisions (approved 2026-07-30):**

**1. Reference the immutable blueprint; do not materialize it.**
[ADR-035](../../adr/ADR-035-submissions-bind-to-a-published-template-version.md) already pins a submission to an immutable `RegulatoryTemplateVersion` — **the blueprint is therefore already the snapshot**. Copying 38 sections into submission-owned rows would mirror immutable data and buy nothing. The rule that replaces "eventually we'll materialize": *materialization begins the moment the submission owns state the blueprint cannot express* — N/A justification, reviewer state, ad-hoc sections, cardinality progress. Until then, reference.

**2. Documents are placed into *sections*, not into `RequiredDocument`s.**
The dossier hierarchy is **organizational**; placeholders are **validation constructs**. A real section holds more than its minimum — 3.2.P.5 may require a Certificate of Analysis and legitimately also carry chromatograms, a statistical appendix, a supplier clarification. Binding placement to `RequiredDocumentId` would leave all of that with nowhere to live. So placement answers *where*, and placeholder satisfaction is **derived** from (section, document type).

**3. Three placement states, only one of which is a finding.**

| State | Meaning | Validation |
|---|---|---|
| Placed, satisfies a placeholder | requirement fulfilled | — |
| Placed, satisfies no placeholder | supporting content | **none** — legitimate dossier content |
| Not placed at all | not in the dossier | `Information` |

Consistent with EPIC-002: the validator reports *problems*, not facts. This also splits validation's question in two — *is this placeholder satisfied?* drives completeness; *is this document anywhere?* drives cleanup. They are complementary, and neither subsumes the other.

**4. The read model is placeholder-shaped from day one.**
Today `Placeholder == RequiredDocument`, but the API is defined around the concept, not today's storage: a placeholder carries a **stable id**, its section, its document type, whether it is mandatory, and what satisfies it. When `SubmissionPlaceholder` rows eventually appear, the backing implementation changes and clients do not.

### Change-case analysis — placement (STORY-001)

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| Documents legitimately exceed the required minimum | Certain | Placement targets the section, so supporting content has a home |
| A type is required by two different sections | High | Coverage keyed on (section, type) — the ADR-035 limit disappears |
| N/A with justification | Medium | Sparse `SubmissionPlaceholder` rows; placeholder ids already exposed |
| Cardinality (2 batch records required) | Medium | Placeholder is already the unit of completeness, not the section |
| Ad-hoc sections | Low-Medium | Needs submission-owned sections; the read model's shape survives |
| Bulk auto-placement by document type | Medium | Derivation rule is the same one a suggestion engine would use |

### Known behavioural change

Once coverage is placement-based, **attaching without placing satisfies nothing**. EPIC-002's capstone spec attaches documents and then expects *"Ready to publish"*, so it legitimately goes red in STORY-002 and is updated there — with a comment naming this epic and ADR-036, so a future reader does not read the change as a regression. Recorded here in advance rather than discovered later.

---

## Phase 3 — Stories

| # | Story | Status | Retires |
|---|---|---|---|
| **STORY-001** | **Placement** — `SubmissionDocument.TemplateSectionId`, place/unplace through the aggregate, and the placeholder-shaped content-plan read model | 🟢 Complete | — |
| **STORY-002** | **Placement-aware coverage** — match on (section, type); disclose unplaced documents | 🟢 Complete | ADR-035 trade-off: *coverage is by document type, not placement* |
| **STORY-003** | **`SectionNotEmptyEvaluator`** + section-scoped `FileFormat` | 🟢 Complete | ADR-035 trade-off: *the validator advertises its own gaps* |
| **STORY-004** | **Dossier builder UI** + gap view + capstone browser proof + ADR-036 + retro | 🟢 Complete | — |

### STORY-001 — Placement (shipped)

One nullable column, `SubmissionDocuments.TemplateSectionId`, with a `Restrict` FK to `TemplateSections`. Nothing else in the schema moved.

**Decisions (approved 2026-07-30):**

1. **One placement per attached document.** eCTD leaf reuse — the same document appearing under several sections — is a real regulatory capability, deliberately deferred rather than overlooked. Nothing in the product exercises it, and the migration when it arrives is *one row per existing placement*: no inference, no ambiguity, no data loss. Recorded in the deferred list below.
2. **Attach and place in one call.** `POST /submissions/{id}/documents` takes an optional `templateSectionId`. Requiring a second round-trip would manufacture an unplaced state that exists only because of API shape — and STORY-002 is about to start reporting that state as a finding.
3. **`PUT .../documents/{id}/placement` states the whole placement.** A null section clears it. This is the endpoint drag-and-drop calls in STORY-004, which is why it expresses a destination rather than a delta.
4. **The enforcement split.** The aggregate checks what it can see — the submission is a draft, and the document is *already attached to this submission*. That second one is a first-class invariant with its own test: accepting an unknown id would make placement a second, unguarded way to attach, bypassing product ownership, active status and version pinning. Whether the section belongs to the bound version is Reference Data's business and lives in `SectionPlacementPolicy`, shared by both write paths — a rule enforced on one path and not the other is not a rule.
5. **Placeholder satisfaction is derived, never stored.** `GetSubmissionContentPlanHandler` computes it from (section, document type) on every read. There is no satisfaction column to fall out of date.

**Deferred from this story**

| Deferred | Why |
|---|---|
| **Multi-placement (eCTD leaf reuse)** | A genuine regulatory need, not an oversight. Deferred until something exercises it; the migration path is mechanical. |
| Reordering within a section | `DisplayOrder` is submission-wide today. No user need yet. |

**Not done here:** nothing validates differently. Coverage is still by document type, exactly as EPIC-002 left it — this story only builds the seam STORY-002 evaluates against, the way EPIC-002's STORY-001 established the binding before anything read it.

**Verified:** 576 backend tests green (11 new domain, 11 new application against the real seeded FDA IND blueprint); all 51 browser tests green; and the three endpoints exercised live on an isolated stack — attach-with-placement, attach-then-place, move, clear, the content plan's derived placeholders, supporting content sitting beside a satisfied placeholder in the same section, and a section from another blueprint rejected with 409.

### STORY-002 — Placement-aware coverage (shipped)

The predicate changed from `DocumentType` to `(TemplateSection, DocumentType)`. The evaluator's responsibilities did not.

**Decisions (approved 2026-07-30):**

1. **Exact section match — no ancestor or descendant inference.** A document in `3.2.S` does not satisfy a placeholder in `3.2.S.1`. Regulators file into the leaf, and "close enough" completeness is worse than no check. If parent-level satisfaction is ever wanted it should be an explicit blueprint rule, not an inference made by the matcher.
2. **Issues name the section as well as the type** — *"Required document 'Form FDA 1571 (IND Application)' is missing from 1.1 Forms."* Now that placement decides the verdict, *where* is half the answer.
3. **Unplaced documents get their own evaluator.** Coverage asks *is every placeholder satisfied?*; `UnplacedDocumentEvaluator` asks *is every document somewhere?*. Two independent questions, two evaluators — merging them would have coverage accumulate responsibilities unrelated to completeness, and is what kept coverage's diff to the predicate.
4. **The unplaced issue carries a count, not names or ids.** The content plan is already the authoritative structured answer to *which* documents; teaching the validation response to reproduce dossier structure would create a second representation to keep in sync, and a message that grows without bound as a submission does. This is why it differs from `UnevaluatedRuleTypes`, which exists precisely because nothing else can answer that question. **Validation says something needs attention; the content plan says what and where.**

**The `.Distinct()` on document type is gone.** That dedupe *was* the ADR-035 limit — a type required by two sections satisfied by one attachment. This is not new support for duplicates; it is the evaluator finally validating what the blueprint has always expressed. Tested with a synthetic blueprint, because no seeded template contains that case yet.

**The deliberate behavioural change, asserted rather than absorbed.** One integration test and one browser step encoded *"attachment satisfies completeness"*. Both now assert the opposite, with comments naming this epic — the browser journey attaches through the UI, proves nothing was satisfied and the disclosure appeared, then places through the API and watches exactly one requirement clear. The test documents the new rule instead of surviving it.

**Diff surface** — the architectural check this story existed to run:

| Changed | Untouched |
|---|---|
| `RequiredDocumentCoverageEvaluator` (the predicate), `AttachedDocument` (+1 field), `BlueprintEvaluationContext` (+`SectionLabelFor`), orchestrator (3 edits), new `UnplacedDocumentEvaluator`, new issue code | `SubmissionValidator`, `SubmissionValidationResult`, `BlueprintSeverityMapper`, `IBlueprintRuleEvaluator`, `FileFormatEvaluator`, the rule loop, the disclosure mechanism, severity mapping, `IsValid` |

The honest caveat: the context had to start carrying placement. That is additive and the format evaluator never noticed, but it was not zero.

**Verified:** 592 backend tests green (16 new); 51/51 browser tests green against an isolated stack; the temporary CORS widening used for that run was reverted and confirmed absent from the commit.

### STORY-003 — The deferred section rules execute (shipped)

The story that tested whether EPIC-002's evaluator seam was real. It mostly was; the exercise found one thing that wasn't, and fixed it.

**Decisions (approved 2026-07-30):**

1. **Section-scoped rules see the subtree; placeholder coverage stays exact.** Not a contradiction — two different predicates. Coverage asks *"does this placeholder have a satisfying document?"*, which must be exact. A section-scoped rule asks *"what is in this part of the dossier?"*, which naturally includes what is filed beneath it. An author writing `SectionNotEmpty` against `3.2.S` means "Drug Substance must contain content", not "a document must be filed directly on the parent node" — which a well-organised dossier never does, because documents live in leaves. For a rule targeting a leaf the two readings are identical, so this costs nothing today and only shapes blueprints not yet written.
2. **`BlueprintEvaluationContext.DocumentsIn(sectionId)` is the shared semantic boundary** — *"documents placed in this section or any descendant"* — used by `SectionNotEmptyEvaluator` and section-scoped `FileFormat` alike, and documented so a future `MaxDocumentsInSection` inherits the meaning instead of re-deriving it.
3. **One evaluator registry, fixed in this story rather than deferred.** Adding an evaluator used to require two entries — a DI registration *and* `DefaultRuleEvaluators()` — two lists representing the same thing with nothing keeping them in step. It was the only part of the architecture that contradicted the extensibility story, so it was folded into the story already touching that code. Composition now reads from the registry, constructed explicitly so a missing registration can never resolve to an engine with **no** evaluators that silently reports every rule as unevaluated.
4. **A `SectionNotEmpty` rule with no section is disclosed, not widened.** "The dossier must not be empty" is a different rule, and not the one the author wrote.

**The disclosure retired itself.** `BlueprintRulesNotEvaluated` no longer appears for any seeded blueprint — and the disclosure code was not touched. That is what makes it a statement about engine capability rather than a hard-coded caveat.

**What the user sees instead.** A fully-placed FDA IND is now *"Ready to publish"* with **two warnings** — *"Drug Product stability data (3.2.P.8) is expected. No documents are placed in 3.2.P.8 Stability."* EPIC-002's principle that publishability and findings are independent survives, but its subject improved: the surviving finding used to be the engine confessing a gap in itself, and is now the blueprint's own advisory judgement.

**Tests that changed, and why none of it is weakening**

| Test | Change |
|---|---|
| `DoesNotEvaluate_SectionScopedRules` | → `Evaluates_SectionScopedFileFormatRules` — the deferral it guarded is lifted |
| two format-rule integration tests | re-scoped from "the only `BlueprintRuleViolation`" to `RuleCode == "FDA-IND-PDF"` — the blueprint now has other rules that report through the same code |
| `RuleTypesTheEngineCannotRunYet_AreDisclosed` | split in two: one asserts nothing is unevaluated for the seeded blueprint, the other runs the engine **with no evaluators at all** to prove the mechanism still distinguishes *could not evaluate* from *passed*. Better than depending on a permanently unimplemented enum member. |

**Cost, honestly:** one new evaluator, one registry entry, one additive context method, the planned lifting of `FileFormatEvaluator`'s deferral, and the registry cleanup. **Untouched:** the orchestrator, `SubmissionValidator`, `SubmissionValidationResult`, `BlueprintSeverityMapper`, `IBlueprintRuleEvaluator`, the rule loop, the disclosure mechanism, severity mapping, `IsValid`. The context helper needed the section tree, which STORY-002 already loads — so the orchestrator did not move.

**Verified:** 606 backend tests green (24 new); 51/51 browser tests green against an isolated stack; CORS widening reverted and absent from the commit.

### STORY-004 — The dossier builder (shipped)

The capstone: the architecture the first three stories built, exposed as something a person can use.

**Decisions (approved 2026-07-30):**

1. **Click-to-place now; drag-and-drop is a later enhancement.** The capability being shipped is *placement*, not a gesture. A regulated tool needs a keyboard-accessible route regardless, so drag-and-drop would sit *on top of* the click flow rather than replace it — which makes the click flow the real interaction model either way. It also keeps the epic's most valuable browser test failing for the right reason: "placement is broken", not "the drag sequence synthesised differently today". When drag-and-drop arrives it will invoke the same placement command.
2. **Content Plan is its own tab, beside Documents.** Documents is the dossier's **inventory** — what is attached. Content Plan is its **structure** — where each sits and what is still expected. Two questions, two pages.
3. **The page composes two read models and interprets neither.** The tree, satisfaction, supporting content and progress come from `content-plan`; findings and publishability from `validation`. The page never maps validation issues back onto placeholders — that would mean parsing messages and rebuilding dossier semantics in React, free to disagree with the server.
4. **Completion is computed server-side.** `ContentPlanProgress` — "12 of 13 placeholders filled", plus the mandatory subset that decides publishability. Counting in the browser would be a second implementation of completeness.
5. **Placement is reversible where it happens.** Every placed document carries **Remove**, clearing its placement without detaching it. A misfiled document whose only route back is the API is a trap.

**Attach-and-place is one call.** Filling an empty placeholder offers already-attached-but-unplaced documents first, then Product Documents — attached *and* placed in a single request, because "put this into 1.1 Forms" is one user action. The distinction is bookkeeping and stays out of the user's way.

**Browser proof.** `submission-content-plan.spec.ts` owns the builder: the tree matches the blueprint's section count, every placeholder starts empty, filling one through the UI flips it to satisfied and advances the progress line, and **Remove** returns it to the unplaced panel still attached. A second test proves an unbound submission renders "no dossier template governs this submission" rather than an error. And `submission-validation.spec.ts` now places **through the dossier builder** instead of the API, so the epic's one end-to-end journey is genuinely walkable by a user:

```
create → attach (UI) → still incomplete + disclosed → open Content Plan
       → place (UI) → placeholder satisfied → validate → publish
```

**Verified:** 606 backend tests green; 53/53 browser tests green against an isolated stack (2 new); web build clean; CORS widening reverted and absent from the commit.

---

## Retro

### What the epic set out to do, and whether it did

| Definition of Done | Outcome |
|---|---|
| Place a document into a section of the bound version; unplace again; never into another version's section | ✅ `PlaceDocument` / `ClearPlacement`, guarded by `SectionPlacementPolicy` (409 on a foreign section) |
| `GET /content-plan` returns the tree with explicit placeholders, stable ids, expected type, mandatory flag, and what satisfies them | ✅ plus `additionalDocuments`, `unplacedDocuments` and `progress` |
| Coverage evaluated by (section, document type) | ✅ the `.Distinct()` that *was* the ADR-035 limit is gone |
| `SectionNotEmpty` and section-scoped `FileFormat` execute; the disclosure shrinks and disappears | ✅ and the disclosure mechanism was never touched |
| Attached-but-unplaced reported as `Information`, never counted, never dropped | ✅ its own evaluator |
| Browser proof of create → assign → gaps close → validate → publish | ✅ and the placement step is driven through the UI |
| ADR written | ✅ [ADR-036](../../adr/ADR-036-the-dossier-is-structure-placeholders-are-validation.md) |

### What went well

- **The one-sentence model held all four stories.** *The dossier hierarchy is organisational; placeholders are a validation construct.* Every subsequent decision — placement targets sections, satisfaction is derived, supporting content is not a finding, exact vs subtree — fell out of it rather than being argued separately.
- **STORY-003 did its job as an architectural proof.** Adding an evaluator moved nothing above the seam, and the exercise found the one thing that *did* contradict the extensibility claim (two evaluator lists), which was fixed in the same story rather than filed.
- **The disclosure retired itself.** Engine capability rose, the `Information` line disappeared, and no disclosure code changed. That is the difference between a capability statement and a hard-coded caveat.
- **Predicted failures, and only those.** Every story named in advance which existing tests would legitimately break. Each time the prediction was exact — a sign the semantic changes were well-contained rather than leaking.
- **One nullable column carried the whole epic.**

### What we would do differently

- **The dev database drifted ahead of the running API twice.** Applying a migration for the integration tests leaves the founder's running process older than the schema. Harmless here (additive, nullable) but worth a convention: say so at the point it happens, every time.
- **Two forward references to ADR-036 shipped before it existed** (in STORY-002 and STORY-003 comments). Fine within one branch, but a reader on an intermediate commit would find a dangling pointer. Next time either write the ADR in the story that first needs to cite it, or cite the epic.
- **A test race of my own making.** STORY-002's browser step counted issues before the page finished loading and produced a confusing "expected 12, received 0". The existing steps had the wait; the new one didn't inherit it. Worth a house rule: every `page.goto` in this suite is followed by a wait for `validation-status` before any count.

### Deferred, deliberately

| Deferred | Trigger to revisit |
|---|---|
| **Multi-placement (eCTD leaf reuse)** | Publishing / eCTD export (EPIC-007), or a blueprint that needs it. Migration: one row per existing placement. |
| **N/A with justification** | The first genuine submission-owned placeholder state — and the trigger to materialise `SubmissionPlaceholder`. |
| **Cardinality (min/max copies)** | Requires blueprint authoring → EPIC-012. |
| **Ad-hoc sections not in the template** | Same materialisation trigger. |
| **Drag-and-drop** | UX polish; will call the same placement command. |
| **Reordering within a section** | `DisplayOrder` is submission-wide; no user need yet. |
| **Re-binding to a newer template version** | Unchanged from ADR-035 — still a policy question. |

### What EPIC-003 leaves for the next epic

A submission now has a **structure**, and every rule the seeded blueprints carry executes against it. EPIC-004 (sequences and lifecycle) inherits a dossier that knows where its contents are — which is the precondition for an eCTD sequence, since a sequence is a diff of placements between submissions.
