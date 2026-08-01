# ADR-040 — The Health-Authority Interaction Context, And The Supertype That Isn't

**Status:** Accepted · **Date:** 2026-08-01 ·
**Related:** [ADR-039](ADR-039-the-market-local-product-tier.md) (reads compose; vocabulary rule),
[ADR-038](ADR-038-organization-depth-roots-and-the-three-filter-shapes.md) (root justification, filter shapes, enum-vs-data),
[ADR-037](ADR-037-registrations-are-regulatory-assets-with-derived-visibility.md) (persist facts, derive interpretation),
[ADR-018](ADR-018-rule-of-three.md) (rule of three),
[ADR-017](ADR-017-shared-kernel-scope.md) (kernel scope),
[ADR-016](ADR-016-persistence-access-model.md) (persistence access model),
[ADR-031](ADR-031-tenant-isolation-by-query-filters.md) (isolation)

## Context

RegOS knows **what we submitted** and **what we hold**. It does not know **what
is happening with the authority** — the letters, questions, meetings,
commitments and inspections that are what a regulatory affairs team does all
day, and that today live in inboxes and spreadsheets.

EPIC-006 introduces five RIM objects for that. This ADR is written at **S001**,
not at the capstone, because `src/Interaction/` is created in S001 and a new
bounded context takes an ADR *first*. S007 appends the hypothesis outcomes.

## Decision

### 1. One bounded context: `src/Interaction/`

Correspondence, questions, commitments, meetings and inspections cross-link
heavily — a question arrives in a letter, produces a commitment, is discussed at
a meeting. Splitting them would make almost every query cross-context for no
gain. RIM treats them as one neighbourhood too.

**Named `Interaction`, not `HealthAuthority`**, because `Authority` is already a
`ReferenceData` aggregate and the two would be read as the same thing.

### 2. It is not `Registration`, and not `Product`

`Registration` is what the business **holds** — an asset with a lifecycle. An
interaction is what **happened**, and most interactions never touch a
registration at all: they concern an application under review, a submission in
flight, or nothing filed yet. Folding them in would give `Registration` a second
job and make its aggregate boundary meaningless.

`Product` is further still. A letter is about a *dossier* far more often than
about a product.

### 3. There is no `AuthorityInteraction` supertype

Phase 2 opened on the question — *what does a user ask that spans a
correspondence, a meeting and an inspection?* — and the answer was **not
"nothing"**: the activity timeline genuinely spans them.

It still does not justify an aggregate. **[ADR-039](ADR-039-the-market-local-product-tier.md)
principle 7 answers it: reads compose.** A timeline over an application is a
read model, exactly as `ListMarketRegistrations` projects across contexts while
granting nobody write ownership.

Both places that pressed for a common parent released on their own:

- The authority is **intrinsic** to a `Commitment` — a commitment is *made to*
  an authority, which is constitutive rather than inherited — so its nullable
  sources were never evidence of a missing parent.
- Content ownership resolved one layer down (decision 5).

RIM keeps all five separate. Departing from it owed evidence, and the evidence
did not arrive. **Four roots and one child.**

> Recorded as a **successful falsification**, under the register in
> [FEATURE-DEVELOPMENT-FLOW](../product/FEATURE-DEVELOPMENT-FLOW.md): the
> abstraction was prevented rather than discovered late. Had Phase 2 begun from
> the entity list, it would have been built.

### 4. `HaCorrespondence` is an event, not a lifecycle — so it has no status

Every other object here evolves: a question is answered, a commitment fulfilled,
a meeting held. **A letter that has been received does not change.** What changes
is our response to it.

Whether correspondence is *"open"* is therefore **derived** — an unmet
`ResponseDueOn`, or unresolved questions beneath it (ADR-037). RIM agrees: it
marks four of the five statuses "Single / Historical", and correspondence is the
one it leaves out.

Consequently EPIC-006 adds **four** dated histories, not five.

### 5. Correspondence owns its content; storage is shared infrastructure

`ProductDocument` is a `GlobalProductId` + a CTD `DocumentTypeId` + a
`Draft → Active → Archived` approval lifecycle + numbered versions. **An inbound
letter has none of those** — no product anchor, no CTD type, no approval, and it
is received exactly once. Forcing it in would mean a fictitious product and two
unused mechanisms.

What must not be duplicated was never the aggregate. It is `IFileStorage`, and
that port is **already anchor-agnostic** — a relative path and a stream.

```
ProductDocument   owns  product documents      ─┐
HaCorrespondence  owns  correspondence content ─┴─▶ IFileStorage  (src/Storage)
```

**Not `RegOS.SharedKernel`** — [ADR-017](ADR-017-shared-kernel-scope.md) rule 1
admits *concepts*, not patterns, and storage is infrastructure with no domain
meaning. One store, two anchors. *(Built in S002.)*

> The constraint that survived this was **"do not build a second document
> store"**. The prescription it arrived as — *"reuse `ProductDocument`"* — did
> not. A constraint bounds the solution space; a prescription forecloses it.

### 6. `CorrespondenceType` is reference data; `Direction` is an enum

ADR-038 decision 3's test — **does anything branch on it?** Nothing branches on
whether a letter is an information request or an approval letter, and adding
*Refuse to File* must not require a deployment. Rules **do** branch on direction:
*"what have they asked us?"* and *"what have we told them?"* are different lists.

Unlike `SubmissionType`, `CorrespondenceType` is **not authority-scoped**. An IND
is genuinely not a CTA; every authority sends information requests. Scoping it
later is additive.

Of EPIC-006's eleven candidate vocabularies, S001 makes exactly **one** governed
data. Correspondence format is a curated frontend constant, and RIM's Action,
Mode and Category are not modelled at all — *governed reference data exists
because the domain needs governed facts, not because dropdowns need labels*
(ADR-039 decision 5).

### 7. All three anchors are nullable

An interaction that cannot be filed against anything is still a real
interaction — a guidance notification concerns no application. Requiring an
anchor would make users invent one, which is worse than a null.

## Consequences

- **ADR-038's absence-shaped prediction is falsified.** `OrganizationDivision`
  was justified by *"EPIC-006 will point an Application, a Licence and an HA
  Meeting at this division"*. It cannot: the division on a letter is the
  **authority's**, `OrganizationDivision` hangs off a tenant-owned
  `Organization`, and `OrganizationType` is `Manufacturer`, `Sponsor`,
  `MarketingAuthorizationHolder`, `ContractResearchOrganization`. Widening it
  would create a second FDA that can disagree with the reference-data one —
  ADR-039 decision 1. **The two divisions share a name and not an identity: one
  describes regulators, the other describes companies.** `HaCorrespondence`
  therefore carries no division at all rather than a misleading one, and S001a
  introduces authority-side structure under `Authority`.
- `RegOS.Interaction.Domain` references four other domain projects for their
  **ids only** (ES-014) — `ReferenceData`, `RegulatoryApplication`, `Submission`,
  `Registration`. Not a new kind of edge; `Registration.Domain` already
  references four.
- `HaCorrespondence` takes the **fail-closed tenant-owned** filter shape;
  `CorrespondenceType` takes the **global world fact** shape (no filter).
- `IHaCorrespondencePolicy` is the **fifth** parallel creation policy and still
  not the extraction trigger — ADR-038 decision 4 sets that at two of them
  needing the same *non-trivial* rule.

## Revisit When

- **The activity timeline is built (S007) and turns out to need a write.** That
  is the only thing that would reopen decision 3. A read that grows a mutation
  is an aggregate wearing a projection's clothes.
- **A second object here turns out to have no status either.** *"If every
  apparent status of an object is derived from related objects or dates, the
  object may be an event rather than a lifecycle."* One instance is not a rule;
  a second would earn it a place in the development flow.
- **S001a finds that authority divisions are tenant-authored rather than
  governed.** Then `AuthorityDivision` takes the *shared-plus-extensible* filter
  shape rather than the global one, and EPIC-012 inherits it.
- **Correspondence acquires an edit history.** Decision 4 says a letter does not
  change. If users start amending logged letters often enough to argue about
  what was changed, the aggregate has a lifecycle after all — and it will be
  about *our record of the letter*, not the letter.
