# ADR-047 — Publication Metadata Exists Only When Publication Makes It True

**Status:** Accepted · **Date:** 2026-08-02 ·
**Related:** [ADR-045](ADR-045-the-cumulative-dossier-and-the-derived-delta.md) (the cumulative dossier),
[ADR-046](ADR-046-a-submissions-lifecycle-is-only-what-we-did.md) (`Filed`, defined and unreachable),
[ADR-044](ADR-044-a-submission-is-a-transmitted-sequence.md),
[ADR-018](ADR-018-rule-of-three.md)

## Context

DIA's model gives a submission six identity attributes beyond its number:
format, three DTD versions (ICH, Regional, STF), gateway format, and a sub-type.
EPIC-004 S004 was planned to add all of them as additive columns.

Sorting them by the tests S001–S003 produced, they are not one kind of thing.
**One is a fact we hold today; four are containers for facts that do not exist
yet; one cannot be modelled at all without inventing a taxonomy.** Adding them
together would have said the opposite.

## Decision

### 1. The rule

> **A fact that only becomes true when a submission is published is not modelled
> until something can publish.** Not as a nullable column, not as a field left
> blank on a form.

This generalises the publication-boundary heuristic from ADR-045 — *a property
meaningful only because a submission has been transmitted stays null until
publication and is immutable thereafter* — from **how a field behaves** into
**whether the field exists at all.**

Three questions decide it, and every candidate answers all three:

| | Question |
|---|---|
| 1 | **When does this fact first become true?** |
| 2 | **Who makes it true?** |
| 3 | **Can the system honestly make it true today?** |

A *no* to question 3 is not a reason to store null. It is a reason not to have
the column.

### 2. Why this is not simply ADR-046's `Filed`

ADR-046 defined `Filed` in the model and made nothing transition into it. The
same reasoning does **not** produce four nullable columns, and the distinction is
worth stating because it looks like an inconsistency:

> **An enum value is vocabulary.** `Filed` costs one line and names a state the
> model acknowledges exists, so a reader learns something true from it.
> **A null column is not vocabulary — it is an empty container**, and shipping
> one is a promise rather than a model.

A column reaches the schema, the DTO, the form and the screen. The first user who
finds a *DTD Version* field will conclude it is needed and fill it in, and RegOS
will then hold a regulatory attribute of unknown provenance. That is a worse
outcome than the field's absence.

### 3. `SubmissionFormat` is built — and it is a rendering concern

`Format` (`Ectd` / `Nees` / `Paper`) answers all three questions: it is true from
the moment a filing is planned, the filer makes it true, and RegOS can state it.

It belongs to the **sequence**, not to the application: real applications moved
from paper to eCTD mid-life, so an application-level format would misdescribe
its own history.

It is **required at creation, not defaulted.** eCTD is the only format an FDA
IND accepts today, which is exactly what would make a default look harmless —
and would let a caller omit a real decision and have the model answer for them.
The *API* states the default; the domain takes none.

It is **frozen at publication**, and the draft guard in `ChangeFormat` is that
freeze. No second mechanism, because a rule with two homes gets two behaviours.

### 4. The delta is domain; the format is rendering

Operation derivation (ADR-045) runs for **every** submission, whatever the
format. A paper sequence still changed something relative to the one before it;
it renders as a cover letter listing the changes rather than an XML backbone.

This is not a detail. ADR-045 records the cumulative dossier as the **product
thesis** — the user owns regulatory state, RegOS derives the transmitted
increment. **If derivation ran only for eCTD, that thesis would silently become
an eCTD implementation detail.** `SubmissionContentOperationTests` asserts a
paper submission derives operations identically, so the obvious future
"simplification" fails a test instead of passing review.

### 5. What is deferred, and what it costs

| | Question 1 — when true | Deferred to |
|---|---|---|
| `DtdVersionIch/Regional/Stf` | when a package is **built** against them | **EPIC-007** |
| `GatewayFormat` | when it is **transmitted** | **EPIC-007** |
| `SubmissionCountries` | when one filing covers several markets | **hypothesis 1** — an application is already exactly one country, and multi-country is the EU procedure |
| `ReasonForDelay` | when actual is later than **planned** | **not deferred — impossible.** `Submission` has no planned date, so the comparison the field describes cannot be made |

The cost is named rather than the ignorance: **an incorrect assumption about DTD
or gateway metadata is first paid in EPIC-007**, which is also the first thing
able to state either truthfully.

### 6. The sub-type is not deferred — it is unresolved

`SubTypeId` has two incompatible readings, and the current model cannot
distinguish them:

- **A taxonomy** — `SubmissionType` gains parent/child, `Submission` points at a
  leaf, and *nothing new belongs on `Submission` at all*.
- **An independent axis** — type `IND` and sub-type `Annual Report` are
  orthogonal, and `Submission` carries both.

> **S004 deliberately does not introduce a sub-type model, because doing so
> would commit RegOS to one of two incompatible structures without evidence.**

`SubmissionType` is flat today (`Code`, `Name`, `AuthorityId`), so there is no
taxonomy for a sub-type to hang from and no evidence for which one the
regulatory model requires. This is a stronger statement than *"later"*: it names
what later is **for**.

## Consequences

- The screen shows **"eCTD"**, **"NeeS"** and **"Paper"**; the domain says
  `Ectd`, `Nees`, `Paper`. Casing only, so no vocabulary pair is recorded —
  but the mapping lives in the client, and no label reaches a type.
- **Format continuity is recorded, not enforced.** Whether sequence 0004 may be
  paper when 0003 was eCTD is unknown; regulators may forbid it. No evidence is
  in hand, so no invariant is invented — the same call ADR-044 made about
  contiguity, and ADR-046 made about `Filed`.
- The migration backfills existing rows to `Ectd` (1), **not** the scaffold's
  `0`, which is not a defined enum value; and it drops the database default
  afterwards so an insert that omits the format fails loudly.
- Four DIA attributes are now **absent by decision rather than by oversight**,
  and this ADR is where that is answered.

## Revisit When

- **EPIC-007 generates a package.** It is the first thing that can state a DTD
  version or a gateway format truthfully; decision 5 expires there.
- **The first EU market.** `SubmissionCountries` returns with hypothesis 1, and
  the question is whether one filing covering several markets is a submission
  attribute or the regulatory activity finally earning its tier.
- **Evidence arrives on format continuity.** A real filing showing an
  application regressing from electronic to paper — or a regulator's rule
  forbidding it — turns decision 3's recorded fact into an invariant.
- **The sub-type taxonomy is settled.** Decision 6 names the two candidates; the
  first real sub-type in reference data decides which.
- **A planned date reaches `Submission`.** Only then does `ReasonForDelay`
  describe something that can be computed.
