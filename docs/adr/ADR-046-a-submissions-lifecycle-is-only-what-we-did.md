# ADR-046 — A Submission's Lifecycle Is Only What We Did

**Status:** Accepted · **Date:** 2026-08-02 ·
**Amends:** [ADR-044](ADR-044-a-submission-is-a-transmitted-sequence.md) (decision 4's wording — see decision 2 below) ·
**Related:** [ADR-045](ADR-045-the-cumulative-dossier-and-the-derived-delta.md),
[ADR-042](ADR-042-what-the-interaction-context-turned-out-to-be.md) (the actor test; the extraction it refused),
[ADR-039](ADR-039-the-market-local-product-tier.md) (principle 7 — reads compose),
[ADR-037](ADR-037-registrations-are-regulatory-assets-with-derived-visibility.md) (two clocks),
[ADR-018](ADR-018-rule-of-three.md)

## Context

`SubmissionStatus` was `Draft` and `Published`, and **every one of its eight
readers asked the same question**: `!= Draft`. It was a boolean in disguise, so
giving it states is the first time the value means anything — which made it worth
asking what belongs in it before adding to it.

RIM offers nine candidates. The question that sorted them was not *which statuses
does a submission have?* but:

> **Which of these are states of the submission, and which are states of the
> regulatory conversation?**

## Decision

### 1. Three states, and the test that produced them

*Can this change independently of anything the submission does?*

| | Actor | Independent? | |
|---|---|---|---|
| `Draft` | us | no | **the submission** |
| `Published` | us | no | **the submission** |
| `Filed` | us | no | **the submission** (decision 2) |
| in preparation, ready to submit | us | no — but internal | **EPIC-008** |
| acknowledged, under review, approved, refused | **them** | **yes** | the conversation |
| withdrawn | us | — | **neither** |

Internal production states fall to the argument ADR-045's epic already made about
the QC and publishing pipelines: they describe how a team works, not what was
filed.

**`Withdrawn` is not a state.** You cannot un-file a sequence — a *later* sequence
withdraws an earlier one. That is a relationship between submissions, the same
shape as EPIC-006's *"threading is a relationship between correspondence
records."*

### 2. `Filed` is defined and unreachable — and this amends ADR-044

ADR-044 decision 4 says a null sequence number means **"never transmitted."**
`Publish` only freezes the dossier; transmission is a later step it explicitly
does not perform. **The ADR's word was stronger than the code's behaviour, and
the ADR was wrong.**

> **A sequence number means *published within RegOS*, not *transmitted*.**
> EPIC-007 adds the transition that makes the stronger word true.

`Filed` exists as a value because the state is real, and **nothing transitions
into it**. Until EPIC-007 generates the package, the artefact reaching the
authority is assembled outside RegOS, so marking a RegOS submission filed would
record that *something related* went — a fact the system cannot honestly make
true.

The case against was genuine: EPIC-006 records meetings without holding them, so
recording a filing that happened elsewhere would be consistent. It loses on two
counts. A letter someone types *is* the letter that arrived; a RegOS submission
is **not** the package that was sent. And the cost is asymmetric — deferring
costs one transition later, building costs a button recording a date of dubious
provenance now.

*ADR-044 is not edited. Its decision stands; this refines when it becomes true,
and the index records the amendment — the same treatment ADR-038 and ADR-039's
predictions received.*

### 3. The authority's side is correspondence, not a status

`HaStatus` is not added. Every authority-side fact is already expressible, and
the burden was to find one that is not:

| The authority did | Where it already lives |
|---|---|
| acknowledged the sequence | inbound `HaCorrespondence` anchored to the `SubmissionId` |
| refused to file | a letter — correspondence |
| approved | correspondence **and** a `Registration`, which already carries `Approved`, `UnderReview`, `Withdrawn`, `Refused` |
| is reviewing | **derived** — acknowledged, and nothing final yet |

*Under review* is the persuasive one because it looks most like a status and is
in fact a read over two facts. Storing it would put someone else's judgement in
our database (ADR-042 decision 4), in a second place, where `Registration`
already holds it.

### 4. The two lifecycles are composed at the edge, not joined in a context

The submission page shows its own history beside what the authority said. **That
composition happens on the page**, from two projections:

- `Submission` gains no knowledge of correspondence.
- `Interaction` gains no knowledge of submission status.
- `ListCorrespondence` gains a `SubmissionId` filter — exposing an anchor
  `HaCorrespondence` has carried since EPIC-006 S001, inside the context that
  owns it. **No new cross-context dependency exists**; the one edge involved,
  Interaction → Submission, was already there.

A dependency would only be justified if the *domain* needed to enforce an
invariant on acknowledgement. Nothing does.

### 5. `PublishedAt` is derived

It is exactly the `RecordedOnUtc` of the `Published` history entry, so it is read
from the record rather than kept as a column that could disagree with the history
beside it. The `Commitment.GivenOn` call again — and reached the same way, by
writing the history and noticing the field was already inside it.

The migration **backfills before dropping**: every existing submission gets a
`Draft` entry from `CreatedOn` and, where it was published, a `Published` entry
from the old column. A history that began the day the migration ran would be a
worse record than the one it replaced.

### 6. The seventh history — measured, and the threshold is met

ADR-042 refused the bitemporal extraction and named its own reopening condition:
*a seventh append-only history with a genuinely uniform configuration.*
`SubmissionStatusEntry` is the seventh. The measurement:

| Shape | Count | Size |
|---|---|---|
| `OwnsMany` block, owner is the root | **4** — commitment, meeting, inspection, submission | **22 lines each, structurally identical** |
| `OwnsMany` block, nested one level deeper | 1 — question, inside correspondence | 26 lines |
| standalone `IEntityTypeConfiguration` | 2 — market, registration | different shape |

**Five `OwnsMany` blocks, four of them line-for-line the same.** ADR-042 set the
bar at *"revisit at five configurations, and extract only the configuration"* —
and it is met.

> **The verdict is extract, and the scope is the EF configuration only.** Not the
> entry type, and not the behaviour: ADR-042's finding that *structural
> similarity is not evidence of behavioural similarity* is unchanged, and the
> tests still assert seven different domains' rules.

This ADR records the verdict; the extraction is its own change, across four
configurations in three contexts, and deserves to be reviewed as one.

## Consequences

- `SubmissionStatusEntry` is the **first history written as a sealed-class
  identity** (ES-020). The other six predate the rule and are on the
  pending-migration list; copying one would have grown it.
- The status enum has a test asserting its exact membership, so adding the
  authority's vocabulary to it is a deliberate act rather than a quiet extension.
- The history begins at **creation**, not publication — becoming a draft is a
  step, and a record that started midway through a submission's life would be a
  worse one.

## Revisit When

- **EPIC-007 transmits.** Then `Filed` gets its transition, and decision 2's
  amendment to ADR-044 is itself superseded by the stronger word becoming true.
- **Someone proposes a status the authority owns.** Decision 3 says where it
  already lives; the test is whether it can change with nothing happening to the
  submission.
- **A domain invariant depends on acknowledgement.** That, and only that, would
  justify the dependency decision 4 avoided.
- **The configuration extraction lands.** Decision 6's verdict is recorded, not
  executed; until it is done this ADR describes an obligation rather than the
  code.
