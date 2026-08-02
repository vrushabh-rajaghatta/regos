# ADR-045 — The Cumulative Dossier, And The Delta We Derive From It

**Status:** Accepted · **Date:** 2026-08-02 ·
**Related:** [ADR-044](ADR-044-a-submission-is-a-transmitted-sequence.md) (a submission is a transmitted sequence),
[ADR-036](ADR-036-the-dossier-is-structure-placeholders-are-validation.md) (placement is the unit of completeness),
[ADR-035](ADR-035-submissions-bind-to-a-published-template-version.md),
[ADR-042](ADR-042-what-the-interaction-context-turned-out-to-be.md) (do not store a judgement you do not own),
[ADR-018](ADR-018-rule-of-three.md)

## Context

ADR-044 settled what a `Submission` **is**. This settles what publishing one
**means**, and it turned out to be the larger question.

Phase 2 opened on *what becomes true exactly once, at the instant of
publication?* Working the candidates against a single falsifier — *can this be
reconstructed from immutable published sequences and documents, with no loss of
historical meaning?* — separated them cleanly, and then exposed an assumption
nobody had written down: **what a submission's document set actually means.**

## Decision

### 1. A submission is the complete dossier; the delta is derived

> **A `Submission` represents the complete regulatory dossier at the moment of
> publication. The transmitted delta is derived from successive cumulative
> submissions.**

Two systems could have been built here, and the difference is not technical:

| | Model A — an eCTD authoring tool | **Model B — a Regulatory Operating System** |
|---|---|---|
| the user owns | the delta | **the current regulatory state** |
| the system does | validate and transmit what was assembled | **derive the filing delta, then transmit it** |

RegOS was already Model B in every part except the part that had never been
asked. [`RequiredDocumentCoverageEvaluator`](../../src/Submission/RegOS.Submission.Application/Validation/Rules/RequiredDocumentCoverageEvaluator.cs)
requires **every mandatory placeholder filled, per submission**; product
documents have lifecycle and versions rather than being files attached to a
filing; published submissions are immutable; records move state rather than being
deleted. Those are all mechanisms for maintaining state, not for composing
deltas.

**This is the product thesis, and every later story inherits it.** A real eCTD
sequence 0001 carries one protocol; a RegOS sequence 0001 carries the whole
dossier, one document of which is at a new version, and RegOS works out that only
one thing moved.

**Its consequence is named here rather than discovered later:** for the
cumulative model to be true in the product and not only in the data, a sequence
must be able to begin from the one before it. Without that the user experience is
a delta while the model is cumulative, and the two contradict. See *Revisit
When*.

### 2. The operation is publication evidence, not a compilation artefact

Every input to the diff survives — versions are immutable, documents follow
lifecycle-over-deletion. What is **not** immutable is the derivation rule.

Whether a document that moved section is *delete + new* or *replace*, whether
`Append` is exercised at all — these are open regulatory questions that EPIC-007
answers, possibly differently from today. A filing recomputed in 2028 under a
rule changed in 2027 would say something other than what it said. And after
EPIC-007 transmits, the operation is a fact the **authority also holds**:
recomputation that disagrees with FDA's copy is not a cache miss, it is a
compliance defect.

**So the operation is computed once, at publish, and frozen.** With it, the
`modified-file` pointer to the specific prior leaf it supersedes.

### 3. It lives on `SubmissionDocument`, and `SubmissionSnapshot` is deleted

The hypothesis *"`SubmissionSnapshot` is the publication record"* resolved in two
parts, in opposite directions:

| | |
|---|---|
| Publication facts exist that cannot safely be recomputed | ✅ **supported** |
| `SubmissionSnapshot` is where they belong | ❌ **falsified** |

The snapshot stored `DocumentVersionId` and `DisplayOrder` — strictly less than
the submission it snapshotted, which is immutable after publish and carries
`ProductDocumentId` and `TemplateSectionId` as well. **The better question was
not whether it contained publication facts but whether it could express them**,
and it could not: without the identity that survives across sequences it has
nothing to compare.

Giving it that identity would not evolve it. It would duplicate
`SubmissionDocument`. **Duplication is not preservation** — it is two
representations of one thing, waiting to disagree.

The argument for keeping it was future immutability of published submissions. But
a partial duplicate table is not an answer to *how is publication evidence
preserved?* — append-only storage is, versioned publication records are,
database-level immutability is. If that capability is ever needed it deserves a
structure designed for it rather than one inherited by accident (ADR-018).

### 4. The publication boundary, as a rule for what comes next

> **If a property is meaningful only because a submission has been transmitted,
> it stays null until publication and is immutable thereafter.**

`SequenceNumber` (ADR-044), then `Operation` and `ReplacesSubmissionDocumentId`.
When someone proposes another publication-only field, the question is already
framed: *does this become true only at publication?* If yes it belongs beside
these; if no it probably belongs somewhere else entirely.

**One refinement the rule forced.** `Unchanged` is a real operation value, though
eCTD has none — because a null had to mean exactly one thing. In a cumulative
dossier a carried-forward document is genuinely in the filing, and *"nothing
happened to it"* must be distinguishable from *"not filed yet"*.

### 5. An operation is a fact about a placement, not about an attachment

A document attached but never placed sits in no section, produces no leaf, and
did nothing to the previous sequence. Publishing with unplaced documents is
permitted — the validator reports it as information, not an error — so the
invariant is the narrower one: **a published submission has an operation for
every *placed* document.**

### 6. A withdrawal is written down, because an absence cannot be frozen

Under the cumulative model a deletion is visible only as a placement the previous
sequence had and this one does not. That is an absence, and an absence cannot be
frozen — recomputing it later under a changed rule would silently rewrite what
the filing said.

So `SubmissionDeletion` records it: the document, the section, and the placement
it withdraws. **Not a `SubmissionDocument` with a delete flag** — that entity
means *this dossier contains this document*, and a withdrawal is precisely the
absence of that. Two collections, two questions: *what is in this filing* and
*what this filing removes*.

## Consequences

- The diff key is `(ProductDocumentId, TemplateSectionId)` — *the same document,
  in the same place*. A `SubmissionDocumentId` belongs to one submission and
  cannot compare across two.
- `ISubmissionPublicationBaseline` replaces S001's `ISubmissionNumberingPolicy`.
  The number and the baseline are one question — *what does the next filing
  follow?* — and two services asking it would be two chances to disagree.
- The rule lives in the aggregate; the facts come from outside it, exactly as
  ADR-044 decision 5 established for the sequence number.
- `SubmissionSnapshots` and `SubmissionSnapshotDocuments` are dropped. No UI or
  browser test consumed them.
- **`UploadDocumentVersion` was added to `ProductDocument`.** `AddNewVersion` had
  existed on the aggregate since EPIC-003 with a comment saying it was modelled
  but not exposed; nothing reached it, so a revised document could not be
  recorded at all. Under the cumulative model that is not a missing convenience
  but a missing gesture — *this document has a new version, file it again* is the
  most common thing a sequence does.

## Revisit When

- **Carrying a dossier forward from the previous sequence is built.** It is not
  convenience; **it is what makes the cumulative model operationally true**, and
  until it exists the product asks a user to reassemble a whole dossier by hand
  to file an amendment.
- **EPIC-007 answers the derivation questions.** A document that moved section
  reads today as delete + new because that is what the key says happened; a real
  filing may say otherwise. Nothing produces `Append`.
- **A publication-only fact appears that is not about a placement.** Decision 5
  draws the line at the placement; a fact about the filing as a whole would sit
  on `Submission` instead, and the distinction is worth keeping deliberate.
- **Published submissions become mutable.** Then decision 3's argument is live
  again — and the answer is a structure designed for preserving publication
  evidence, not a partial duplicate.
