# ADR-044 — A Submission Is A Transmitted Sequence, Not A Regulatory Conversation

**Status:** Accepted · **Date:** 2026-08-02 ·
**Related:** [ADR-035](ADR-035-submissions-bind-to-a-published-template-version.md) (a submission binds to a published template version),
[ADR-036](ADR-036-the-dossier-is-structure-placeholders-are-validation.md) (a sequence is a diff of placements),
[ADR-042](ADR-042-what-the-interaction-context-turned-out-to-be.md) (correspondence anchors to a submission; do not store someone else's judgement),
[ADR-018](ADR-018-rule-of-three.md),
[ADR-016](ADR-016-persistence-access-model.md)

## Context

`Submission` has existed since EPIC-003 as *a titled bag of placed documents with
two states*. EPIC-004 set out to give it a sequence number, and the Phase-2
question — **what business thing survives after sequence 0003 has been
transmitted?** — found that the word was already carrying two jobs.

The enduring regulatory conversation is
[`RegulatoryApplication`](../../src/RegulatoryApplication/RegOS.RegulatoryApplication.Domain/Aggregates/RegulatoryApplication/RegulatoryApplication.cs):
scoped to `(GlobalProduct, Country, Authority)` and carrying `ApplicationNumber`.
*"The IND"*, *"the original NDA"*, *"our MAA"* are application numbers. Adding
`SequenceNumber` to `Submission` would have quietly assigned it a second job.

This ADR states which job it has.

## Decision

### 1. A `Submission` is one transmitted sequence

```
RegulatoryApplication      the IND — the enduring regulatory conversation
    └── Submission         one transmitted sequence: 0000, 0001, 0002 …
```

A submission is the **regulatory package that gets filed**, once, and never
changes afterwards. The conversation it is filed into is the application, and
that relationship already existed.

### 2. A tier earns existence by owning a fact

The candidate object between the two — the *regulatory activity* — was tested
and rejected against this rule:

> A tier earns existence by owning a fact. Not a title, not a type, not a folder
> — **a business fact that neither `RegulatoryApplication` nor `Submission` can
> own without contradiction.**

**Contradiction, not convenience.** Someone will propose grouping submissions.
*Grouping is not ownership; folders are not facts.*

| | Own number? | Own clock? | Own outcome? |
|---|---|---|---|
| **US · FDA · IND** — the first vertical | no — serial numbers are flat per application | no | no |
| US NDA supplement | yes | yes | yes |
| EU variation | yes | yes | yes |

The tier is real elsewhere and owns nothing here, so it is **a live hypothesis
rather than a model element**, falsified or confirmed at the first EU market or
US supplement. When it arrives it goes **above** `Submission` — one nullable
foreign key, no data migration, since every IND submission legitimately belongs
to no activity.

### 3. The screen says "Sequence 0003"

The domain word and the screen word differ, and both are binding — the
`MedicinalProduct` ↔ *"Market"* precedent (CLAUDE.md). RIM and FDA both call one
serial number a *submission*; a regulatory user says *"sequence 0003"*. The
domain keeps precision, the UI keeps familiarity, and the pair is recorded in
[docs/domain-model/submission.md](../domain-model/submission.md).

### 4. The number is assigned at publish, and the invariant comes free

`SequenceNumber` is `int?` — **null means never transmitted**. Numbering starts
at **0000**.

Assigning at creation was the original lean and is wrong. Assigning at publish
means:

- **Number order *is* transmission order**, by construction rather than by rule.
  Nothing can publish out of order, so the diff base of a later sequence can
  never be silently rewritten.
- **No gaps.** An abandoned draft leaks nothing.
- **A draft asserts only what is true.** A draft labelled `0004` claims a fact it
  does not have. The UI shows *"will publish as next sequence (currently 0004)"*
  — a projection, derived from `MAX(published) + 1`, stored nowhere. Current
  sequence number is fact; predicted next sequence number is projection.

### 5. The domain owns uniqueness; the workflow owns assignment timing

`Publish` **accepts** a sequence number rather than choosing one, the same way it
already accepts its timestamp instead of reading the clock. The application layer
supplies the fact; the aggregate enforces the rule.

This is what keeps import viable. A customer arriving with an IND already at
sequence 0012 supplies the numbers that were really filed, and they are checked
by the same invariant rather than bypassing it. **Import will be a sibling entry
point with its own name, sharing a private implementation — never an `isImport`
flag on `Publish`.** It is not built here because there is no import command, no
workflow and no test that would mean anything (ADR-018).

### 6. Contiguity is a domain rule over a supplied fact — and this is what it does not prove

```
Publish(sequenceNumber, previousPublishedSequenceNumber, publishedAt)
    requires sequenceNumber == (previousPublishedSequenceNumber ?? -1) + 1
```

`Submission` is a root; its siblings are not inside its consistency boundary, so
it **cannot** verify that `0006` exists — the same wall `PlaceDocument` already
documents for template sections. Passing the previous number in is the strongest
form available, and its limit is stated rather than papered over:

- **A caller that lies about `previous` gets through.** The aggregate check is
  not a proof that the previous sequence exists.
- **What makes the pair sound is the division of labour.** The filtered unique
  index on `(ApplicationId, SequenceNumber)` makes duplicates impossible
  regardless of caller; the domain rule gives gaps one home and makes them
  testable, instead of a convention every future handler must remember.
- It pays for itself immediately: `null → 0000` makes *"the first sequence in an
  application is 0000"* a **domain** test rather than an integration test.

### 7. A sequence stays addressable in its own right

An acknowledgement names a sequence; so does a refuse-to-file, so does a
validation report. [`HaCorrespondence`](../../src/Interaction/RegOS.Interaction.Domain/Correspondence/HaCorrespondence.cs)
already anchors to a `SubmissionId`.

**A sequence demoted to a child entity cannot be anchored to.** This constraint
outlives decision 1 and holds whatever the activity tier turns out to be — it is
the reason an activity goes *above* `Submission` and never *between* `Submission`
and the transmitted thing. If a multi-sequence activity does arrive, EPIC-006
already suggests its shape: *threading is a relationship between records*, so it
is plausibly a relationship between sequences rather than a parent over them.

## Consequences

- `Submission` gains exactly **one** DIA attribute in S001 — *Submission Number*.
  Application, Submission Type, Submission Status and its date already existed.
  Format, sub-type, DTD versions, gateway format and countries are S004.
- The **concurrency answer is the database, not a lock.** Two simultaneous
  publishes in one application collide on the unique index; the loser retries.
  Carried as an S001 hypothesis with a named fallback —
  `pg_advisory_xact_lock(applicationId)` — if a 100-way concurrent publish test
  cannot pass within a bounded retry count.
- Numbering is **application-scoped and therefore already region-scoped**: an
  application is `(product, country, authority)`. No policy object, no region
  parameter. A planned Phase-2 decision disappeared because the model answered it.
- `RegulatoryApplication` is unchanged. It holds no counter; the next number is
  derived, never stored.

## Revisit When

- **A regulatory activity owns a business fact neither neighbour can own without
  contradiction.** Decision 2. Expected at the first **EU market** or **US
  supplement** — deliberately not EPIC-007. Grouping does not count.
- **Someone proposes an `isImport` flag on `Publish`.** Decision 5 says why it is
  a sibling method instead.
- **A caller needs to publish a number it did not get from the numbering policy,
  outside import.** That is the case decision 6's honesty is reserved for; look
  at why before widening the rule.
- **A second aggregate needs an application-scoped counter.** Then the numbering
  policy has a sibling, and the two should be compared before either is
  generalised (ADR-018).
