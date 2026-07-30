# EPIC-003 — Submission planning & content

**Status:** 🟡 In Progress (1 of 4 stories shipped) · **Branch:** `epic/EPIC-003-submission-planning-and-content` · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

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
| **STORY-002** | **Placement-aware coverage** — match on (section, type); disclose unplaced documents | ⚪ Not Started | ADR-035 trade-off: *coverage is by document type, not placement* |
| **STORY-003** | **`SectionNotEmptyEvaluator`** + section-scoped `FileFormat` | ⚪ Not Started | ADR-035 trade-off: *the validator advertises its own gaps* |
| **STORY-004** | **Dossier builder UI** + gap view + capstone browser proof + ADR-036 + retro | ⚪ Not Started | — |

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

**STORY-003 is the epic's architectural proof.** If adding `SectionNotEmptyEvaluator` costs one class, one DI registration, and its tests — and nothing else moves — then the evaluator seam built in EPIC-002 was genuinely extensible. If anything else has to change, that is worth knowing and recording in the retro.
