# ADR-042 — What The Interaction Context Turned Out To Be

**Status:** Accepted · **Date:** 2026-08-01 ·
**Related:** [ADR-040](ADR-040-the-health-authority-interaction-context.md) (the context boundary, written before it existed),
[ADR-041](ADR-041-platform-contracts-and-the-identity-that-crosses.md) (`UserId` crosses as a contract),
[ADR-039](ADR-039-the-market-local-product-tier.md) (reads compose; vocabulary rule; the history-extraction prediction),
[ADR-038](ADR-038-organization-depth-roots-and-the-three-filter-shapes.md) (root justification, filter shapes),
[ADR-037](ADR-037-registrations-are-regulatory-assets-with-derived-visibility.md),
[ADR-018](ADR-018-rule-of-three.md)

## Context

ADR-040 was written at S001, before the context existed, and could only decide
its **boundary**. Six stories later the context has a shape, and this records
what that shape is — the decisions a future contributor would otherwise
re-derive or quietly reverse.

It records **what is now true**. How it was discovered is the epic's retro.

## Decision

### 1. Work outlives interactions; interactions often conclude

The five objects fall into two families, and the split is load-bearing:

| Generate ongoing work | Conclude |
|---|---|
| `HaCorrespondence`, `HaQuestion`, `Commitment` | `HaMeeting`, `Inspection` |

A meeting that was held and an inspection that finished are **not work**. Their
value is what they produced — commitments, follow-up questions, a recorded
position — not a continuing lifecycle. Both leave the lists that answer *"what
is coming?"*, and neither appears in the due view.

Consequently both aggregates are the smallest in the context. Designing them by
asking *"what work remains after this?"* rather than *"what happened at this?"*
is why.

### 2. `Commitment` is the durable work product

Every interaction reduces to obligations with dates and owners, and the
obligation is a `Commitment`. Its archetype is not an answer to a question — it
is the **post-marketing commitment**: an approval letter carries conditions that
outlive the letter by years.

It therefore has **three independent business origins** — correspondence,
meeting, inspection — as three nullable foreign keys, which are referentially
enforced, individually queryable and honest where a polymorphic
`(SourceType, SourceId)` pair is none of those.

> **A fourth *independent business origin* reopens the ownership discussion**
> that ADR-040 decision 3 closed. Worded as origins rather than columns: a
> `MeetingFollowUpId` would still be a meeting-derived commitment and does not
> trip it.

### 3. Correspondence remains actionable until its work has been decomposed

A letter with a response due is work until its questions exist; then the
questions are the work and the letter is not.

**Derived, never stored.** Nothing marks a letter as hidden — the read asks
whether it still represents work. Today decomposition means questions; if it
later also means commitments, the wording holds and only the query changes.

### 4. The judgements we do not own, we do not store

`CommitmentStatus` has **no `Failed`**. A commitment we did not do is `Open` and
past its date; **overdue is derived**. Whether that lateness matters is the
authority's judgement, recorded in a letter — which is correspondence. A failure
status would put someone else's conclusion in our database.

The same split runs through the context:

| Theirs | Ours |
|---|---|
| `HaMeeting.Outcome` — what the authority concluded | the `Commitment`s it obliged |
| `Inspection.Outcome` — what they found | the corrective actions |
| `HaQuestionStatus.Resolved` — they accepted | `Responded` — we replied |
| `CommitmentStatus.Waived` — they released us | `Fulfilled` — we performed |

Every pair is separated by **actor**, not by moment.

### 5. An observation is not a question

A Form 483 observation looks identical to an `HaQuestion` — numbered, texted,
responded to — and is a different kind of thing. **A question asks for
information and answering it *is* the work. An observation asserts a deficiency
and responding to it *creates* work**, which is a `Commitment` that already
exists.

There is therefore no observation entity. One would exist only to produce
commitments, which is the intermediary this context repeatedly declined to add.

### 6. One transition table, and only one

`HaMeeting` alone has a lifecycle table, because `Requested → Granted | Declined`
is a fork **the authority chooses**. Every other status graph here records our
own operational progression, where a table would encode one company's habits as
law (ADR-039 decision 6).

**The table models authority decisions, not our workflow** — the line to hold
when someone proposes adding *Minutes Uploaded* or *Attendees Confirmed*, which
are things we do and not statuses of a meeting at all.

`HaMeeting` and `Inspection` both take their **initial status as a parameter**:
we request some meetings and an authority calls others; they announce some
inspections and arrive at others unannounced. Forcing the second kind through
the first would put an event in the history that never happened. Two business
events deserve two beginnings.

### 7. The site is what was inspected

`Inspection.OrganizationSiteId` is not location metadata. **The authority
physically went somewhere**, and very often to a contract manufacturer's site
rather than ours — which is why `OrganizationSite` is a root with a
cross-organization directory (ADR-038).

Nullable, because *"the FDA will inspect us in March"* arrives before anyone
knows which of three plants. Naming the site later is its own business event.

### 8. The append-only history extraction is refused, not deferred

ADR-039 decision 6 predicted EPIC-006 would be the extraction point for the
bitemporal history shape. **Six consumers now exist and it was measured twice:**

| Candidate | Measured | Outcome |
|---|---|---|
| entry type | ~30 lines of code each, identical fields | not worth a generic EF-owned type |
| chronology rule | **one line**, identical everywhere | nothing to extract |
| EF configuration | **not one shape** — three `OwnsMany` blocks of 22–26 lines, two standalone configuration classes | a helper covers three, after migrating two |
| tests | **not duplicated** — each asserts its own domain's rules | evidence against a behavioural abstraction |

> **Structural similarity is not evidence of behavioural similarity.** The
> histories converged in shape and diverged in semantics, and the measured
> maintenance cost was never where the prediction said.

**Reopening this needs new evidence, not another occurrence.**

## Consequences

- `RegOS.Interaction.Domain` references five other domain projects for **ids
  only** (ES-014) and `Platform.Contracts` for `UserId` (ADR-041).
- Six aggregates' worth of tenant-owned data, all on the **fail-closed** filter
  shape. `CorrespondenceType` is a global world fact; `AuthorityDivision` is
  **platform-seeded, tenant-augmentable** — RegOS has no authoritative source
  for the world's authority divisions.
- `IFileStorage` lives in `src/Storage/RegOS.Storage`, shared by
  `ProductDocument` and this context, and owned by neither.
- The due view composes three aggregates in one read (ADR-039 principle 7) and
  stores no interpretation: proximity, *overdue* and *mine* are all derived at
  the point of asking.

## Revisit When

- **A fourth independent business origin for commitments appears.** Decision 2.
- **Decomposition comes to mean something other than questions.** Decision 3's
  wording already covers it; the query does not.
- **Someone proposes a `Failed` commitment status.** Decision 4 says why, and
  the answer is that the authority records that, in a letter we can already
  store.
- **A second lifecycle acquires a fork an authority chooses.** Then decision 6's
  table has a sibling, and the two should be compared before either is
  generalised.
- **A seventh append-only history arrives with a genuinely uniform
  configuration.** That is the new evidence decision 8 requires; another
  occurrence alone is not.
