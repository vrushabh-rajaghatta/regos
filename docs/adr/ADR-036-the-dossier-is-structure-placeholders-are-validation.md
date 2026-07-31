# ADR-036 — The dossier hierarchy is organisational; placeholders are a validation construct

**Status:** Accepted · **Date:** 2026-07-30 · **Epic:** EPIC-003

## Context

[ADR-034](ADR-034-regulatory-templates-are-versioned-shared-blueprints.md) made a dossier blueprint *data*. [ADR-035](ADR-035-submissions-bind-to-a-published-template-version.md) bound a submission to an immutable published version of one and validated against it — but only by asking *"is a document of this type attached?"*. Nothing recorded **where** a document sat, so a submission was a flat bag of attachments judged against a tree.

That left two limits ADR-035 recorded and accepted: coverage was by document type rather than placement, and `SectionNotEmpty` could only be disclosed as unevaluable. Both are the same missing fact.

Adding that fact raises the questions this ADR answers: what the submission owns versus what it reads, what a document is placed *into*, and which reader owns which part of the truth.

## Decision

### 1. The blueprint is **referenced**, not materialised onto the submission

A submission gains one nullable column, `SubmissionDocuments.TemplateSectionId`. No section rows, no placeholder rows, no copy of the tree.

ADR-035 already pins a submission to a version that is immutable once published — **so the blueprint is already the snapshot**. Copying 38 sections per submission would mirror immutable data and create a second thing that can disagree.

The rule that replaces "eventually we'll materialise": **materialisation begins the moment the submission owns state the blueprint cannot express.** An N/A justification, a reviewer comment, per-placeholder workflow, cardinality progress, ad-hoc sections — each is a trigger. Until one arrives, reference.

### 2. Documents are placed into **sections**, not into `RequiredDocument`s

**The dossier hierarchy is organisational; placeholders are a validation construct. Documents belong to the hierarchy. Validation derives from how those documents satisfy placeholders — not the other way around.**

A section answers *where does this document belong?*. A required document answers *is there a minimum expected here?*. They are not the same question. Module 3.2.P.5 may require a Certificate of Analysis and legitimately also carry chromatograms, a supplier clarification, a statistical appendix. Binding placement to `RequiredDocumentId` would leave all of that with nowhere to live, and every real dossier contains it.

So placement records **where**, and satisfaction is derived.

### 3. Placeholder satisfaction is **derived, never stored**

Computed on every read from `(section, document type)`. There is no satisfaction column, so there is nothing to fall out of date and no reconciliation to write. It also means a placeholder's state and a validation verdict cannot drift apart — they are two readings of the same two facts.

### 4. Supporting content is first-class dossier content, not a finding

A document placed in a section that satisfies no placeholder is **legitimate**, and reported as such (`additionalDocuments`), never as a warning. A document **not placed at all** is disclosed as `Information` — untidy, not invalid.

Three states, one of which is a finding:

| State | Meaning | Validation |
|---|---|---|
| Placed, satisfies a placeholder | requirement fulfilled | — |
| Placed, satisfies no placeholder | supporting content | none |
| Not placed at all | not in the dossier | `Information` |

This splits validation's question in two — *is this placeholder satisfied?* drives completeness, *is this document anywhere?* drives organisation — and neither subsumes the other.

### 5. Coverage matches **exactly**; section-scoped rules see the **subtree**

Not a contradiction: two different predicates.

Coverage asks *"does this placeholder have a satisfying document?"* — one requirement, one section, matched exactly. A document in `3.2.S` does not satisfy a placeholder in `3.2.S.1`; regulators file into the leaf, and "close enough" completeness is worse than no check. Parent-level satisfaction, if ever wanted, should be an explicit blueprint rule rather than an inference.

A section-scoped rule asks *"what is in this part of the dossier?"* — a region, which naturally includes what is filed beneath it. An author writing `SectionNotEmpty` against `3.2.S` means "Drug Substance must contain content", not "a document must be filed directly on the parent node", which a well-organised dossier never does. For a rule targeting a leaf the readings are identical.

`BlueprintEvaluationContext.DocumentsIn(sectionId)` is the single expression of that scope, so a later `MaxDocumentsInSection` inherits the meaning instead of re-deriving it.

### 6. One placement per attached document, deliberately

eCTD allows the same document to appear under several sections (leaf reuse). That is a real regulatory capability, **deferred rather than overlooked**: nothing in the product exercises it, and a placement collection would immediately raise questions the rest of the system cannot yet answer — which placement satisfies the placeholder, what "move" means, how completeness counts.

The migration when it arrives is mechanical: one `SubmissionDocumentPlacement` row per existing non-null `TemplateSectionId`. No inference, no ambiguity, no data loss. Recorded here so a future reader knows it was decided, not forgotten.

### 7. Ownership: validation says *that*; the content plan says *what and where*

| Concern | Owner |
|---|---|
| Blueprint structure | `RegulatoryTemplateVersion` (immutable) |
| Placement | `Submission` aggregate |
| Dossier structure, placeholder state, completion progress | content plan read model |
| Findings and their severity | validation result |
| Publishability | validation result (`IsValid` = no `Error`) |

This is why the unplaced-documents issue carries a **count, not names or ids**: the content plan is already the authoritative structured answer to *which*, and teaching the validation response to reproduce dossier structure would create a second representation to keep in sync — and a message that grows without bound as a submission does. It is the opposite call to `UnevaluatedRuleTypes`, which carries structure precisely because nothing else can answer that question.

The same boundary holds in the UI: the dossier builder composes the two read models and interprets neither.

### 8. The rule engine extends by one evaluator and one registry entry

`SectionNotEmptyEvaluator` was added without the orchestrator, validator, result model, severity mapping, rule loop or disclosure mechanism changing. The one thing that had contradicted that claim — a DI registration list *and* `DefaultRuleEvaluators()`, two lists with nothing keeping them in step — was collapsed to a single registry, composed explicitly so a missing registration can never resolve to an engine with no evaluators that silently reports every rule as unevaluated.

## Consequences

**Benefits**
- **One place for dossier semantics.** Satisfaction, supporting content and completion are derived server-side; no client can hold a second opinion.
- **The ADR-035 limits are retired.** A type required by two sections now owes two documents, and `SectionNotEmpty` executes.
- **The capability disclosure retired itself.** With every rule type executable, `BlueprintRulesNotEvaluated` stopped appearing — and the disclosure code was never touched, which is what makes it a statement about capability rather than a hard-coded caveat.
- **Users see regulatory judgement, not implementation caveats.** A publishable IND now shows the blueprint's own advisory warnings instead of the engine confessing a gap.
- **Minimal state.** One nullable column carries the whole epic.

**Trade-offs we are consciously accepting**
- **A document can be placed in one section only.** eCTD leaf reuse is unavailable until a placement collection exists; the migration path is named above.
- **Placement is checked against the bound version, in the application layer.** The aggregate cannot see Reference Data, so the rule lives in `SectionPlacementPolicy` rather than in `Submission` — enforced on both write paths, but not by the aggregate itself.
- **Attaching without placing satisfies nothing.** Deliberate, and a behavioural change from EPIC-002; disclosed rather than silent, but users who think of attachment as completeness must relearn it.
- **Completion counts every placeholder, mandatory or not.** "12 of 13 filled" can read as incomplete when only optional placeholders remain; the mandatory subset is exposed alongside it for clients that need the publishability reading.
- **Placement is click-driven, not drag-and-drop.** The gesture the backlog imagined is deferred; the operation it stands for is not. Drag-and-drop, when added, will invoke the same placement command.

## Alternatives considered

- **Materialise the blueprint onto each submission at bind time.** Rejected: the bound version is already immutable, so this duplicates data that cannot drift, and pays storage and synchronisation for state no submission yet owns.
- **Place documents against `RequiredDocument` instead of `TemplateSection`.** Rejected: makes supporting content unrepresentable, and conflates *where a document belongs* with *whether a requirement is met*.
- **Store an `IsSatisfied` flag per placeholder.** Rejected: a cache of a two-line derivation, with a reconciliation bug waiting in it.
- **Report unplaced documents by id in the validation result.** Rejected: duplicates the content plan and slowly turns the validation response into a second content-plan API.
- **Treat a document placed in a section that requires nothing as a warning.** Rejected: real dossiers carry supporting content, and a validator that complains about correct behaviour teaches users to ignore it.
- **Let a placement in a parent section satisfy a child's placeholder.** Rejected: completeness would become subjective, and a regulator's blueprint names the leaf it expects.
