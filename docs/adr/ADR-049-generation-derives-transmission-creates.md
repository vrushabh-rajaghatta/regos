# ADR-049 — Generation Derives, Transmission Creates

**Status:** Accepted · **Date:** 2026-08-02 ·
**Related:** [ADR-044](ADR-044-a-submission-is-a-transmitted-sequence.md) (a submission is a transmitted sequence),
[ADR-045](ADR-045-the-cumulative-dossier-and-the-derived-delta.md) (the derived delta),
[ADR-046](ADR-046-a-submissions-lifecycle-is-only-what-we-did.md) (§2 — the `Filed` milestone this restates),
[ADR-047](ADR-047-publication-metadata-exists-only-when-publication-makes-it-true.md) (§5 — the deferral this resolves),
[ADR-018](ADR-018-rule-of-three.md)

## Context

EPIC-007a builds eCTD packages, and three questions arrived with it:

1. Where does the `Filed` milestone belong now that EPIC-007 has been split?
2. Is a generated package an artifact RegOS owns, or a projection?
3. Where do DTD version and gateway format live — deferred to "when a package is
   built" by [ADR-047 §5](ADR-047-publication-metadata-exists-only-when-publication-makes-it-true.md)?

They look like three unrelated amendments. They are one boundary, seen from
three sides.

## Decision

### 1. The boundary

> **Generation derives artifacts from frozen business facts. Transmission
> creates new business facts.**

Everything below follows from which side of that line a fact falls on.

### 2. A generated package is a projection, not a domain artifact

The test is not *can it be regenerated*. Regenerability is a property of the
generator, and a generator can change. The test is stronger:

> **No business fact exists exclusively within the package.**

| In the package | Already owned by |
|---|---|
| sequence identity | `Submission` (ADR-044) |
| operations | frozen `SubmissionContent` (ADR-045, ADR-047) |
| placements | the bound blueprint |
| document bytes | immutable document versions |
| checksums | a deterministic function of those bytes |
| the contact block | the submission's roles (ADR-048) |
| folder layout, XML | the renderer |
| DTDs | a generator input, pinned in `docs/evidence/` |

**If deleting the ZIP loses no business information, the ZIP is not part of the
domain model.** No `GeneratedPackage` aggregate is introduced, and
`SubmissionStatus.PackageGenerated` is **not** added — a lifecycle state
describes the business object, and a package is something produced *from* it.

### 3. Generator metadata belongs to the generation event

DTD version and gateway format become true when a package is built, which makes
them properties of **a generator run**, not of the submission that run consumed.
They are therefore **derived, not stored** — ADR-047 §5's deferral resolved in
the epic it was deferred to.

Two runs of the same published submission may legitimately disagree if the
generator or the pinned DTD changed between them. **That is not a defect.** It
becomes one only when someone must prove what was sent — see decision 4.

### 4. `Filed` belongs to transmission, which is EPIC-007b

[ADR-046 §2](ADR-046-a-submissions-lifecycle-is-only-what-we-did.md) says a
sequence number means *published within RegOS*, not *transmitted*, and that
EPIC-007 adds the transition making the stronger word true. **Splitting EPIC-007
changes nothing about that meaning.** The milestone belongs to whichever half
transmits, and generation does not.

No accepted ADR is edited; this restates where the milestone lands.

At transmission — and only there — facts arise that the `Submission` aggregate
cannot hold because they did not exist when it was published: which exact bytes
went, through which gateway, under which DTD revision, at what timestamp, and
what acknowledgement came back. **Those are regulatory history.** They are the
first legitimate reason to persist a package, and EPIC-007b owns the question.

### 5. v1 stores nothing

Packages are generated on demand and streamed. See
[implementation-standards — *a cache is not an aggregate*](../engineering/implementation-standards.md);
the first proposal to store generated packages for speed is a caching decision,
and answering it as a modelling decision is the mistake this records.

### 6. The test used here is applied, not promoted

*Does this concept own business facts that exist nowhere else?* has now decided
three questions: `SubmissionSnapshot` (EPIC-004 S002), `RegulatoryActivity`
(EPIC-007a Phase 2), and decision 2 above. Each time the answer was **no**, and
each time the concept was not created.

**It is deliberately not written into the standards.** Three uses of a heuristic
that has only ever returned one answer is not evidence that the heuristic is
sound — it may only be evidence that this codebase has been over-modelled in the
same direction three times. ADR-018 permits abstraction on the third
*demonstrated* need; a modelling test demonstrates itself by being **wrong at
least once**, and it has not yet had the chance.

## Consequences

- EPIC-007a introduces **no new aggregate, no new status value, and no new
  persisted field for the package**. As in EPIC-004 S006, an empty persistence
  diff on the render stories is evidence the boundary is right, not evidence the
  story was small.
- A package regenerated years later may differ byte-for-byte from the one a user
  downloaded. Acceptable, and stated in the product: the download is a
  derivation, not a record.
- Nothing RegOS renders may call a generated package *validated*, *FDA-ready* or
  *ready for submission* — those assert evidence levels this boundary does not
  reach (see the EPIC-007a Definition of Done).
- EPIC-007b inherits a coherent scope rather than a leftover one: `Filed`, the
  acknowledgement, and package persistence are the same decision.

## Revisit When

- **Transmission is built (EPIC-007b).** Decision 2's falsifier is not
  regeneration — it is an authority receiving something. *What exactly did we
  send* is a fact the submission does not hold, and it may justify persisting the
  transmitted package as business history.
- **A performance ticket proposes storing generated packages.** Decision 5 is the
  answer, and the question to ask is whether the stored copy is *authoritative*
  or merely *faster*.
- **A regulator or customer requires proof of exact transmitted bytes.** That
  falsifies decision 2 directly, and only at the transmission boundary.
- **A fourth question is resolved by decision 6's test — or one is resolved
  against it.** The second is worth more than the first. A test that has said
  *no* four times has still not been shown to be capable of saying *yes*.
