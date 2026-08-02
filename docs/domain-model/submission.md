# Submission

**The transmitted regulatory package.** One `Submission` is one sequence, filed
once into a `RegulatoryApplication` and never changed afterwards.

See [ADR-044](../adr/ADR-044-a-submission-is-a-transmitted-sequence.md).

## The word the domain uses, and the word the screen uses

| Domain | Screen | Why they differ |
|---|---|---|
| `Submission` | **"Sequence 0003"** | RIM and FDA both call one serial number a *submission*; a regulatory affairs manager says *"we're filing 0003 next week"*. The type keeps RIM's precision, the label keeps the user's habit. |

**Both are binding.** The screen's word must never reach a type, and the type's
word must never reach a label by default (CLAUDE.md). The label is formed in
exactly one place —
[`sequenceLabel`](../../web/regos-web/src/features/regulatory/submissions/utils/sequenceLabel.ts).

## What is *not* a Submission

**The enduring regulatory conversation.** *"The IND"*, *"the original NDA"*,
*"our MAA"* are
[`RegulatoryApplication`](../../src/RegulatoryApplication/RegOS.RegulatoryApplication.Domain/Aggregates/RegulatoryApplication/RegulatoryApplication.cs)
— scoped to `(GlobalProduct, Country, Authority)` and carrying
`ApplicationNumber`. Sequences are events inside it.

```
RegulatoryApplication      IND 123456 — survives every filing
    └── Submission         0000, 0001, 0002 …
```

**The regulatory activity**, if it turns out to exist. A US NDA supplement and
an EU variation each own a number, a clock and an outcome; a US IND has no such
tier — serial numbers are flat per application. It is carried as a hypothesis,
not modelled, and would sit *above* `Submission` rather than between it and the
transmitted thing.

## Number and expectation are different words

| | Persisted | Shown as |
|---|---|---|
| `SequenceNumber` | yes — **null until published** | *"Sequence 0003"* |
| the next number | **no** — derived from `MAX(published) + 1` | *"Will publish as next sequence (currently 0004)"* |

A draft labelled *"0004"* would assert a fact it has not earned: the number is
claimed at publish, and whichever draft publishes first takes it. Two drafts in
one application therefore show the same next number, which is true rather than
a defect.

## A submission is the whole dossier; the delta is derived

> **A `Submission` represents the complete regulatory dossier at the moment of
> publication. The transmitted delta is derived from successive cumulative
> submissions** ([ADR-045](../adr/ADR-045-the-cumulative-dossier-and-the-derived-delta.md)).

A real eCTD sequence 0001 carries one protocol. A RegOS sequence 0001 carries the
**whole dossier again**, one document of which is at a new version — and RegOS
works out that only one thing moved. The user maintains the regulatory state; the
system derives the increment.

This is why every mandatory placeholder must be filled in *every* sequence, and
why a placement the previous sequence had and this one lacks is a **withdrawal**
rather than an omission.

### A mandatory document cannot be withdrawn — and that is correct

The two rules above meet, and the result surprises people demonstrating the
feature:

> **If the blueprint requires a document, no later sequence can withdraw it.**
> Omitting it makes the dossier incomplete, and the validator refuses to publish.

So a withdrawal is only expressible for a **supporting** document — one placed
in a section the blueprint knows, but not answering a required-document slot.
Every one of the 48 slots in the seeded FDA IND blueprint is mandatory, which
means *nothing in the seed data can demonstrate a withdrawal* unless an extra
document is placed alongside the required set. EPIC-004 S006's browser spec does
exactly that.

**This is the model being right, not a limitation.** You cannot withdraw what
the dossier is required to contain; the regulatory act would be refused on the
other side too. Written down here because the alternative is someone concluding
withdrawals are broken.

| Screen | Domain |
|---|---|
| **"What changed"** | `SubmissionDocument.Operation` + `Submission.Deletions` |
| "carried forward unchanged" | `SubmissionContentOperation.Unchanged` |

`Unchanged` exists although eCTD has no such operation: a carried-forward
document is genuinely in the filing, and *nothing happened to it* has to be
distinguishable from *not filed yet*.

### The invariant, in the narrow form it actually holds

> **A published submission has an operation for every *placed* document.**

Not *every document*. An operation is a fact about a **placement** — a document
attached but never placed sits in no section, produces no leaf, and did nothing
to the previous sequence. Publishing with unplaced attachments is permitted (the
validator reports it as information, not an error), so the broader sentence would
be false the first time someone published one.

This is the kind of invariant that gets accidentally widened later. It is written
down narrow on purpose.
