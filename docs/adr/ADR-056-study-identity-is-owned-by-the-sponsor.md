# ADR-056 — Study Identity Is Owned By The Sponsor, Not By A Submission

**Status:** Accepted · **Date:** 2026-08-03 ·
**Related:** [ADR-054](ADR-054-a-study-tagging-file-is-a-projection-over-a-study.md) (**the question it deliberately left open**),
[ADR-053](ADR-053-instance-qualifiers-belong-to-the-placement.md) (the placement carries the qualifier),
[ADR-030](ADR-030-tenant-is-its-own-aggregate.md) / [ADR-032](ADR-032-organizations-are-tenant-owned.md) (the precedent for who owns an identity),
[ADR-018](ADR-018-rule-of-three.md) (duplication is permitted, not compelled),
[ADR-055](ADR-055-when-an-authority-required-fact-becomes-a-domain-fact.md) (the promotion test this decision passes),
[E21, E24, E29](../evidence/README.md)

## Context

ADR-054 established that a Study Tagging File is a **projection** over the
placements in one sequence that belong to one study, and that the projection
needs facts RegOS does not hold — chiefly a study. It then stopped:

> *"**This ADR does not decide which context owns it.** That is a bounded-context
> question, and repository canon requires an ADR of its own."*

This is that ADR. It is also, as EPIC-019 notes, **the first decision that starts
defining the clinical / non-clinical information model** rather than package
generation — which is why it is taken deliberately rather than as a sub-clause of
an eCTD story.

Three facts constrain it, and all three come from outside RegOS:

| | |
|---|---|
| **E29** | the `study-id` is *"the internal alphanumeric code used by the **sponsor** to unambiguously identify this study"*. Not RegOS's id, and not FDA's |
| **E24** | an instance qualifier **must be identical across sequences**, or FDA's review tooling splits one study into two. A constraint no DTD can express |
| **E21** | an STF is required for every file in 4.2.x and 5.3.1.x–5.3.5.x, and the FDA IND blueprint seeds 4.2.1, 4.2.2 and 4.2.3 — so this blocks every IND |

## Decision

> **Study identity is owned by the sponsor, not by a submission.** A study lives
> in its own bounded context, `src/Study/`, as two aggregates — `ClinicalStudy`
> and `NonClinicalStudy` — beginning with the sponsor's identifier and a title.

### 1. Ownership is the argument; the citation count is only corroboration

The tempting justification is *"four contexts will cite a study, so it belongs to
none of them"* — `RegulatoryApplication`, `Submission` (placements),
`Registration` (RIM's `License → Clinical Study`) and `Interaction`
(post-marketing commitments). That is true, and it is the weaker half.

**The real argument is that the identity is not ours and not the filing's.** A
study exists, is run, and is named by its sponsor whether or not anything has ever
been filed about it. RegOS records a code somebody else assigned — the same
relationship it has to a DUNS number or an FDA application number, and precisely
[ADR-055](ADR-055-when-an-authority-required-fact-becomes-a-domain-fact.md)'s
test: *an ordinary business concept that would exist if the authority did not*.

E24 turns that from a modelling preference into a **constraint**. The sponsor's
`study-id` must be byte-identical in sequence 0000 and sequence 0007, years apart,
or FDA's reviewer sees two studies. **An entity owned by a submission cannot
promise that** — each submission would hold its own copy, and nothing in the model
would make two copies agree. Ownership by the sponsor is what makes the stability
an invariant instead of a hope.

This follows the ADR-030 / ADR-032 shape exactly: an identity that outlives and
crosses the things that reference it is its own aggregate in its own context, and
the things that reference it hold an id.

### 2. Two aggregates, and the reason is the domain, not today's field list

`ClinicalStudy` and `NonClinicalStudy` are separate aggregates.

The original sketch leaned this way on internal grounds — RIM has two sheets, they
will diverge. **E29 supplies the first reason from outside RegOS**: the STF's
`category` — species, route of administration, duration, type of control — applies
to **4.2.3.1, 4.2.3.2, 4.2.3.4.1 and 5.3.5.1**. Three nonclinical sections and one
clinical, and the values a regulator expects differ by kind.

> **For the minimum in §3, the two aggregates differ by almost nothing but their
> type. That is accepted, not overlooked. The separation exists because the
> domain differs, not because today's properties differ.**

**Do not over-generalise them in advance.** Neither aggregate gets a shared base
class, a `StudyKind` discriminator, or a common abstraction invented to hold the
duplication. [ADR-018](ADR-018-rule-of-three.md) permits merging on a third
*demonstrated* need; two RIM sheets and one shared vocabulary is not that, and
symmetry between the two is explicitly not a demonstration.

### 3. What a Study begins as — and the rule for what it becomes

> **A `Study` begins as the smallest sponsor-owned identity capable of supporting
> regulatory filing. Additional attributes are admitted only when required by an
> external regulatory workflow or a demonstrated business capability.**

So it begins as **the sponsor's `study-id` and a `title`**, and nothing else.

That is not a guess at a minimum; it is what the seeded blueprint actually
demands. ICH requires `category` for exactly four CTD sections, and **the FDA IND
blueprint seeds none of them** — checked against `RegulatoryTemplates.cs`, not
inferred. The ~23 attributes in EPIC-019's Phase-1 sketch are RIM's list, and
*"because the reference model has these fields"* is not a reason this project
accepts.

The rule above exists to keep that true later. `phase`, `indication`,
`therapeutic area`, `subject count`, `sponsor`, `status history`, `start` and
`closeout` dates are all plausible and all currently unrequested. Each arrives
with the workflow that needs it — the way `Token`, `EctdFolder` and `EctdElement`
each arrived — and the ADR that admits it should be able to name what demanded it.

**The hard line is unchanged and worth restating here**: study results,
endpoints, arms, populations and statistical data are not admitted by this rule.
RegOS is a regulatory information system, not a CTMS. It records *that* a study
exists and what it is about.

### 4. Nothing depends on Study except through an id, and the placement carries the link

`Study` sits below the contexts that cite it and depends on none of them. In
`Submission`, the link is on the **placement**, not the document:
`SubmissionDocument` gains a nullable reference to the study it reports and a
nullable `FileTag`.

That is [ADR-053](ADR-053-instance-qualifiers-belong-to-the-placement.md)'s
answer applied to its third shape — and the ownership works out cleanly in both
directions:

- **A study does not know where it is filed.**
- **A document does not know where it is filed.**
- **The placement does.**

So refiling the same document, or reporting the same study from a second section,
changes a placement and never touches the `Study` aggregate.

Both are **nullable, and null means the ordinary thing**:
a placement outside 4.2.x and 5.3.1.x–5.3.5.x reports no study. Generation refuses
only where FDA requires an STF, which is the refusal ADR-054 §6 already installed.

## Consequences

**One new context and one new cross-context dependency.**
`src/Study/RegOS.Study.{Domain,Application,Infrastructure}`, and
`RegOS.Submission.Domain → RegOS.Study.Domain` for `StudyId`. Persistence stays
centralised in `RegOS.Persistence`, and the aggregates carry a `TenantId` with a
fail-closed query filter ([ADR-031](ADR-031-tenant-isolation-by-query-filters.md))
like every other tenant-scoped entity.

**Two aggregates means two identities: `ClinicalStudyId` and
`NonClinicalStudyId`.** Both `sealed class … : StronglyTypedId` per
[ADR-043](ADR-043-entity-identity-derives-from-the-kernel.md) — a study has a
lifecycle and is not flat master data — copied from `CommitmentId`, not from the
nearest id.

> **There is no shared `StudyId`.** One identity type spanning two aggregates is
> an identity space neither of them owns, which is a supertype in all but name —
> the abstraction §2 just declined to invent.
> [ADR-040](ADR-040-the-health-authority-interaction-context.md) §3 is the
> precedent: five RIM objects that genuinely share a timeline, and still *"four
> roots and one child"*, because a real read that spans them is a read model and
> not a parent.

**The consumer is better served by this, which is the corroboration rather than
the reason.** The STF's `category` vocabulary is kind-specific (§2), so
generation needs to know the kind of study a placement reports. A typed reference
carries that; a shared id would make it a lookup, and a lookup that has to probe
two tables to find out what it is holding.

**It puts an exclusive-or on the placement, and that is S002's to model
explicitly** — a placement reports at most one study, of one kind. Named here so
it is designed rather than discovered.

**The uniqueness of `study-id` is a decision this ADR does not make.** E24 requires
it to be *stable*, which is not the same as requiring it to be *unique within a
tenant*. Following the EPIC-005 precedent, whichever is chosen gets a test —
including a test of the constraint deliberately **not** added.

**EPIC-019's two drivers separate cleanly.** The STF half (S001–S003) needs this
ADR; the citation half (S004–S005) needs only that the aggregate exists. If work
stops after S003, RegOS can file an IND, which it cannot do today.

**This does not make an STF verifiable.** `ich-stf-v2-2.dtd` is **not held**, so a
generated STF can be written and not validated — an evidence gap that blocks the
*claim*, not the modelling, and recorded so the claim is not made by accident.

## Revisit when

- **A third citer arrives** — `Registration` or `Interaction` — which tests
  whether the citation link is a join aggregate or a collection on the citing
  side. This ADR deliberately settles only the `Submission` half, because that is
  the half something is waiting on.
- **An attribute is admitted under §3** and the reason turns out to be *"RIM lists
  it"* rather than a named workflow. That is the failure this ADR is written to
  make visible.
- **The two aggregates have been identical for three demonstrated needs**, at
  which point ADR-018 permits — and only then — collapsing them.
- **A study needs to be reported by two sponsors, or renamed by one.** Sponsor
  ownership is the premise of the whole decision; either case would test it.
