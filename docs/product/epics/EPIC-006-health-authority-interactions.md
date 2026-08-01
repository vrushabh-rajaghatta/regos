# EPIC-006 — Health-authority interactions

**Status:** 🟡 In Progress — S001 done, S001a next · **Branch:** `epic/EPIC-006-health-authority-interactions` · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

Everything that passes between the sponsor and the authority **after** a filing — letters, questions, meetings, commitments, inspections. In headcount terms this is what a regulatory affairs team actually does all day, and today it lives in inboxes and spreadsheets.

> **Phases 1–3 are settled.** Phase 2 was run on 2026-08-01 and **approved**; the sketch it replaced is gone, but the section that shaped it — *What EPIC-017 settles* — is kept as the reasoning trail. Ready for S001.

---

## Phase 1 — Epic plan

### Outcome
A regulatory user can record every interaction with a health authority against the application, submission or registration it concerns — and can answer the question that keeps them awake: **"what is due, to whom, and when?"** Nothing with a regulatory deadline is tracked in someone's mailbox.

### The concepts it introduces

| | Means | RIM object |
|---|---|---|
| **HA Correspondence** | a letter, email or formal communication, in either direction | HA Correspondence (24 attrs) |
| **HA Question** | a question raised inside a correspondence, and our response | HA Q&A (16) |
| **HA Meeting** | a scheduled interaction, its materials, minutes and outcomes | HA Meeting (19) |
| **Commitment** | something we promised the authority we would do, by a date | Commitment (19) |
| **Inspection** | an authority inspection, its subject and outcome | Inspection (16) |

Five RIM objects — the largest single block of RIM coverage available in one epic (**+5, roughly 16% → 37% when combined with EPIC-016/017**).

### Depends on
- **EPIC-016** — correspondence names an HA **division**, a **contact** and a contact role; meetings have HA attendees. Without contacts this epic invents a shadow person model.

### In scope ✅
- **`HaCorrespondence`** — name, description, date, ~~action,~~ format, ~~mode,~~ type, ~~category,~~ response-due date, ~~initiator/recipient~~ **direction**, health authority + division + contact + contact role; attachable content ~~(reuses `ProductDocument`)~~ **owned by the correspondence, sharing `IFileStorage`** — *Phase 2, decision 0. Five RIM classifications reduced to two; see Phase 2 vocabularies.*
- **`HaQuestion`** — child of a correspondence: question text, number, topic, response, response lead + contributors, date received, planned due date, mandated flag, CTD section(s) impacted, dated status.
- **`Commitment`** — title, subject, description, type, source, date given, due date, internal target and actual completion dates, owner, department, dated status.
- **`HaMeeting`** — subject, type, format, request date, meeting date, attendees, minutes, materials, outcomes, discipline, owner, stakeholders, dated status.
- **`Inspection`** — title, subject, description, type, source, due date, date of inspection, internal target/actual, owner, department, dated status.
- **Cross-links** to `RegulatoryApplication`, `Submission`, `Registration`, and between the five objects (a question arrives in a correspondence, produces a commitment, discussed at a meeting).
- **The "what's due" view** — everything with an open due date across all five, derived on read, sorted by urgency.
- Interactions UI, browser proof, ADR.

### Out of scope ⏸️ (deferred, with reason)
| Deferred | Why |
|---|---|
| **Notifications / reminder emails** | Showing *"due in 14 days"* is a domain capability; sending mail is infrastructure → **EPIC-014**. Same split EPIC-005 made for expiry. |
| **Process Step linkage** | RIM anchors all five on `Process Step`. That layer does not exist yet → **EPIC-020**, which wires them up. Model the nullable FK *seam* now, per the Phase-2 rule; do not build the workflow. |
| **Question response authoring / document assembly** | A response is content on the correspondence (Phase 2, decision 0). Structured response-package building is → **EPIC-007**. |
| **Approval workflow on responses** | → **EPIC-008**. |
| **Free-text search across correspondence** | Read-side concern; add when volume justifies it. |
| **Inbound email ingestion** | Infrastructure and a large surface of its own. |
| **Commitment ↔ study linkage** (post-marketing commitments citing a study) | Needs the study registry → **EPIC-019**. Nullable seam only. |

### Definition of Done
- A correspondence can be recorded against an application or submission, with the authority, division and contact who sent it, and one or more attached content items.
- Questions can be raised under a correspondence, each with its own due date, owner and dated status, and can be answered.
- A commitment can be created from a question, a meeting or standalone, and appears in the "what's due" view.
- A meeting can be scheduled, held, and closed with minutes and outcomes.
- An inspection can be recorded with its dated status.
- **Every status on all ~~five~~ four objects that has one carries a dated history** (`OccurredOn` / `RecordedOnUtc`) — RIM marks Commitment, Inspection and Question status "Single / Historical". *Corrected in Phase 2: `HaCorrespondence` has no status at all. It is an event, not a lifecycle.*
- Due-date proximity is **derived, never stored** (EPIC-005 precedent).
- Browser proof: log a correspondence → raise two questions → answer one → convert the other into a commitment → see both in the due view.
- ADR written for the interaction cluster's context boundary.

---

## What EPIC-017 settles *(added 2026-08-01 — the reasoning trail into Phase 2)*

EPIC-017 shipped after this epic's Phase 2–3 sketch was written, and
[ADR-039](../../adr/ADR-039-the-market-local-product-tier.md) answered four
questions the sketch had left open, added one it had not asked, and resisted one
simplification it was reaching for.

**This section drove the Phase 2 below and is kept for its reasoning, not its
authority.** Where the two differ, Phase 2 is the settled design. The
strikethroughs point at sketch decisions that no longer exist.

### 1. `HaQuestion` stays a child — the promotion pressure was imaginary

Phase-2 decision 2 says *"the pressure to promote it comes from the due view;
measure before promoting."* **ADR-039 principle 7 removes that pressure
entirely:** a read model may project across aggregate and context boundaries to
answer a user's question, and projecting does not imply write ownership. The due
view reads questions through `RegOSDbContext` (ADR-016) whether they are roots
or children.

So the question to settle is no longer *"can the due view reach it?"* but the
only one that ever decides a boundary: **does answering a question have to
change anything on the letter?** If not, the correspondence is a container, not
an invariant boundary — and `TradeName` (child, own identity, own commands
through the root) is the shipped precedent.

**The remaining argument for promotion is ADR-039 principle 6**, not the due
view: a question carrying an owner, a due date, contributors, a response and a
dated status may earn a working surface of its own. Decide it on *behaviour*,
and note that a page does not require a root — `MarketStatusTimeline` renders a
child's history on the market's page.

### 2. Reuse the *entry*. Do not reuse the *lifecycle*

Phase-2 decision 3 says *"reuse `RegistrationStatusEntry` verbatim."* ADR-039
decision 6 splits that in half, and the split is the decision:

| Shared | Owned by each concept |
|---|---|
| append-only entries | permitted transitions |
| `OccurredOn` / `RecordedOnUtc` | initial status |
| current-value projection | terminal statuses |
| chronology validation | business meaning of each state |

`MarketStatusEntry` matches `RegistrationStatusEntry` field for field **and has
no `RegistrationLifecycle` counterpart** — because a regulator's decision graph
is genuinely constrained and commercial reality is not.

**The default for all five objects is therefore: history, no transition table.**
A transition table must be argued for, per object. On a first read only
`HaMeeting` looks like a candidate — *requested → granted → held* contains an
authority's decision, which is the `Registration` shape. Commitment, Inspection
and Question statuses are records of **our own** process, which is the
`MarketStatus` shape.

### 3. Do not schedule the extraction first

The sketch says *"if this is the third occurrence, extract the shared shape."*
This epic brings occurrences **three through seven**, so the temptation is an
S000 that extracts before any of them exists — which is extracting on a
*predicted* need, exactly what ADR-018 forbids.

**Write the first EPIC-006 history by hand.** Then extract, with three real
consumers in front of you. ADR-039's own Revisit When hedges this deliberately:
*"if EPIC-006 ships and the extraction is still not obviously worth doing, the
shape was never the duplication we thought it was."* That sentence is only
falsifiable if the extraction is allowed to not happen.

### 4. Eleven vocabularies, three buckets — not two

Phase-2 decision 5 offers reference data *or* closed enums. ADR-039 decision 5
adds a third answer, and it is the one most of the eleven should get:

> **Governed reference data exists because the domain needs governed facts, not
> because dropdowns need labels.**

`LanguageCode` is a value object over a curated frontend constant list with
`Intl.DisplayNames` for display — no aggregate, no seed, no migration, no
EPIC-012 governance burden. Applied here:

| Bucket | Test | Likely members |
|---|---|---|
| **Enum** | a rule branches on it (ADR-038 decision 3) | all five statuses; correspondence direction |
| **Reference data** | the domain needs *governed, tenant-extensible* facts | Meeting Type (FDA Type A/B/C/D is authority-defined and legislated); Question Topic, if authority taxonomies turn out to matter |
| **Curated constant** | only a dropdown needs labels | Correspondence Format / Mode / Category; Inspection Source |

Eleven reference-data aggregates is eleven seeds, eleven migrations, eleven
queries and eleven governance surfaces. **Sort them before S001** — the sorting
is cheap and the reversal is not.

### 5. The new constraint the sketch does not carry: nothing duplicates its parent's facts

ADR-039 decision 1, generalised in the ADR itself:

> When aggregate B is defined partly *by* aggregate A, B stores the reference to
> A and **nothing A already owns**.

This is the sharpest constraint on the cross-link web, because that web is where
the temptation lives. A question arrives in a correspondence, so it stores
`HaCorrespondenceId` — **not** the authority, not the division, not the
application. A `Registration` no longer carries `CountryId` or `GlobalProductId`
for exactly this reason: a second copy with no transaction spanning the two
aggregates cannot be kept in agreement.

**The one place this genuinely bites:** `Commitment` may arise from a question,
from a meeting, or standalone — and all three sources are nullable, so it cannot
always derive its authority. It is the same shape of question EPIC-017 spent
S001 on, and it is now **Phase-2 decision 7**, framed as: *is the authority an
intrinsic fact of a commitment, or inherited context?* The second answer implies
a missing abstraction rather than a missing field, which is why it is worth
asking in that form.

### 6. Five histories is five chances to repeat EPIC-017's worst defect

[testing.md](../../engineering/testing.md) principle 8 — *can perform → can
observe* — was written because EPIC-017 S003 shipped a status history that was
written correctly and readable **nowhere**. This epic writes five.

**Every story's Definition of Done must name the surface that reads back the
history it writes**, in the same story. An unobservable fact is
indistinguishable from a fact that was never recorded.

Two smaller inheritances from the same retro:

- **Fixtures will fight the chronology rule.** Append-only histories reject
  out-of-order dates, and EPIC-017 hit this twice — a fixture created something
  dated today and then tried to give it a 2021 event. The domain was right both
  times. Give every fixture factory an explicit start date from the first story.
- **Accessible names are domain language** —
  [accessible-names.md](../../engineering/accessible-names.md). Five objects
  with a `Status`, a `Due date` and an `Owner` each, all rendered together in
  the due view, is the highest-collision surface yet built.

### 7. Build the vocabulary table before S001, in one sitting

The five statuses are the reason. ADR-039's vocabulary rule:

> **Never reuse a word for two concepts — but reusing a word for one concept
> across tiers is correct.**

`Withdrawn` was refused at the market tier only because portfolio views show
market status and registration status **side by side**. The *"what's due"* view
does the same thing to five statuses at once, so the same test applies with five
times the surface. Is a `Cancelled` meeting (did not happen) the same concept as
a `Cancelled` commitment (the authority released us)? Is a `Closed` question
(answered) the same as a `Closed` inspection (report issued)?

Answer all five vocabularies together, on one page, before the first is
implemented. Discovering the collision at S005 means migrating four.

And the corollary, which saves rules: **prefer the word whose semantics enforce
the constraint.** `Planned` needs no rule forbidding return to it, because a
market already entered cannot be intended.

### 8. The working surface comes early, not at the capstone

Phase 3 defers the interactions UI to *"a timeline on the application workspace"*
in S006. EPIC-017 S004 is the counter-example: `MedicinalProduct` accumulated
trade names, two statuses and a history while still being rendered as a row in
someone else's table, and the fix — its own route — was diagnosed late as an
interaction-design problem rather than a refactor.

`HaCorrespondence` will hold questions, attachments and a status history by S002.
It earns a page then. **The cadence EPIC-017 arrived at, and recommends here:**

```
vocabulary → identity → local concepts → business history → operability
           → working surface → projection
```

The capstone is then the *"what's due"* view and the narrative browser proof —
projection over aggregates that already have surfaces, not the first time a user
can see any of this.

### 9. One candidate principle, deliberately not yet written down

EPIC-017 made the same move four times, in four unrelated places:

| Instead of | It stored | And projected |
|---|---|---|
| `CountryId` + `GlobalProductId` on `Registration` | `MedicinalProductId` | country and product, joined through the tier |
| a persisted `LaunchDate` | the status history | the first entry reaching `Launched` |
| an `IsPrimary` trade name | all trade names | whichever the reader needs |
| a cross-aggregate ownership rule | aggregate-local writes | a cross-context read model |

Stated once:

> **Prefer storing canonical identity and projecting derived views over
> persisting convenience facts.**

**This is deliberately not in ADR-039.** Seven principles is already a
substantial set, and this one is a *generalisation over four instances in a
single epic* — which is exactly the kind of pattern that looks universal from
inside the epic that produced it.

**The test is this epic.** EPIC-006 offers at least four independent chances to
follow or break it: due-date proximity (derived, already required by the DoD),
a commitment's authority (decision 7), a question's *"days overdue"*, and
whatever the correspondence anchor turns out to be (decision 0). If EPIC-006
lands on the identity-and-projection side without being told to, it has earned a
place in **ADR-040** — possibly in preference to some of ADR-039's more
implementation-specific wording. If EPIC-006 has to fight it, it was an
observation about one epic and should stay one.

Record the outcome in the EPIC-006 retro either way. *An unfalsifiable principle
is just a preference with a citation.*

### Still open, and untouched by EPIC-017

Phase-2 decisions **1** (one context or several) and **4** (correspondence
direction) are unaffected. Decision **6** is not — it is superseded by the new
**decision 0**, which asks the prior question: *what is the natural owner of
authority correspondence?* ADR-039's consequences record that `ProductDocument`
stays anchored to the **global** product, and a letter from Health Canada about
a Canadian market presence is not obviously a product document. That is the one
decision here with reach beyond this epic, so it goes first.

---

## Phase 2 — Domain design *(approved 2026-08-01)*

Supersedes the sketch. Run in the order
[FEATURE-DEVELOPMENT-FLOW](../FEATURE-DEVELOPMENT-FLOW.md) now prescribes:
the domain question first, the entity list last.

### The question this phase opened with

Not *"what is an authority interaction?"* — that form has already conceded the
noun. The falsifiable form:

> **What does a user ask that spans a correspondence, a meeting and an
> inspection?**

| A regulatory user asks | Spans |
|---|---|
| *"What's due this week?"* | questions + commitments |
| *"Did we respond to the FDA's information request?"* | correspondence |
| *"What did we commit to at approval, and have we done it?"* | commitments |
| *"When is the EMA meeting, and what came out of the last one?"* | meetings |
| *"The inspection is next month — what is outstanding?"* | inspections + commitments |
| *"Show me everything that has happened on this application"* | **all of them** |

The answer is not *"nothing"*. One question genuinely spans them: the **activity
timeline**.

### Hypothesis: resolved, falsified

> *There may be an `AuthorityInteraction` abstraction — correspondence, meeting
> and inspection being three kinds of one thing.* Carried into Phase 2 from
> [ADR-039](../../adr/ADR-039-the-market-local-product-tier.md)'s review, with
> decisions 0 and 7 named as the pressure points.

**Falsified.** The spanning question is real, and
[ADR-039](../../adr/ADR-039-the-market-local-product-tier.md) principle 7
answers it without an aggregate: **reads compose**. An activity timeline over an
application is a read model, in exactly the way `ListMarketRegistrations`
projects across contexts while granting nobody write ownership.

Both pressure points released:

- **Decision 7** resolved to *intrinsic* (below), so the nullable sources are not
  evidence of a missing parent.
- **Decision 0** resolved one layer down (below), so no common parent is needed
  to own content.

RIM keeps all five separate; departing from it owed evidence, and the evidence
did not arrive. **Four roots and one child, no supertype.**

*Recorded as a success under the register: hypothesis → tested → falsified →
architecture stayed simpler. The abstraction was prevented, not discovered late.*

### How the open decisions resolved

| | Question | Resolution |
|---|---|---|
| **0** | The natural owner of authority correspondence | **The interaction owns it.** Storage is shared; the anchor is not. |
| **1** | One context or several | **One** — `src/Interaction/`. |
| **2** | `HaQuestion` child or root | **Child.** No behaviour of a question changes the letter; the due view is a read model. |
| **3** | Dated history on all five | **Four**, not five. No transition tables except `HaMeeting`. |
| **4** | Correspondence direction | **Explicit enum.** Every real query starts with *"did they write to us, or we to them?"* |
| **5** | Controlled lists | **Three buckets**, and five correspondence classifications become two. |
| **6** | Attachments reuse `ProductDocument` | **Superseded by 0.** The constraint survives; the prescription does not. |
| **7** | Authority intrinsic to a `Commitment`? | **Intrinsic.** |

### Decision 0 — the interaction owns its content

Reading the code decided this.
[ProductDocument](../../../src/ProductDocument/RegOS.ProductDocument.Domain/Aggregates/ProductDocument.cs)
is `GlobalProductId` + `DocumentTypeId` + `Draft → Active → Archived` + numbered
versions. A letter from Health Canada has **none of that**: no product anchor,
no CTD document type, no approval lifecycle, and it is received exactly once —
it does not have a v2. Forcing it in would mean a fictitious product, an
inapplicable lifecycle and unused versioning.

But the thing the constraint protects was never the aggregate. It is
[IFileStorage](../../../src/ProductDocument/RegOS.ProductDocument.Application/Storage/IFileStorage.cs),
and that port is **already anchor-agnostic** — a relative path and a stream.

```
ProductDocument   owns  product documents      ─┐
HaCorrespondence  owns  correspondence content ─┴─▶ IFileStorage
```

`IFileStorage` and `LocalFileStorage` move to a thin **`src/Storage/RegOS.Storage`**
module both contexts reference. **Not** `RegOS.SharedKernel`: ADR-017 rule 1
admits *concepts*, not patterns, and storage is infrastructure with no domain
meaning. One store, two anchors — the constraint honoured without fusing two
domains because both happen to hold files.

### Decision 7 — the authority is intrinsic

A commitment is *made to* an authority. That is constitutive, not inherited: a
commitment with no source letter is ordinary, and a commitment with no authority
is meaningless. Storing `AuthorityId` therefore does not violate ADR-039
decision 1, because there is no required parent that already owns it.

### Context

One bounded context: **`src/Interaction/RegOS.Interaction.{Domain,Application,Infrastructure}`**.
The five objects cross-link heavily and splitting them would make almost every
query cross-context; RIM treats them as one neighbourhood too.

Named `Interaction` rather than `HealthAuthority` because `Authority` is already
a `ReferenceData` aggregate and the collision would be read as the same thing. A
context named for a noun it deliberately does not contain is ordinary — a context
is a neighbourhood, not a type.

All aggregates carry `TenantId` and the **fail-closed tenant-owned** filter
shape (ADR-038 decision 2, ADR-031).

### Aggregates

**`HaCorrespondence`** *(root)* — an event, with no status of its own.

| Field | Note |
|---|---|
| `Id`, `TenantId` | |
| `Direction` | enum — `Inbound` / `Outbound` |
| `CorrespondenceTypeId` | reference data — Information Request, Deficiency Letter, Approval Letter… |
| `Subject` | |
| `OccurredOn` | the letter's own date |
| `ResponseDueOn?` | drives the due view |
| `AuthorityId` | |
| `OrganizationDivisionId?`, `ContactId?` | **ADR-038's absence-shaped prediction fires here** |
| `RegulatoryApplicationId?`, `SubmissionId?`, `RegistrationId?` | all nullable — an unfiled interaction is still real |
| `Attachments` | child collection (S002) |

Whether a letter is *"open"* is **derived**: an unmet `ResponseDueOn`, or
unresolved questions beneath it. Persist the fact, derive the interpretation
(ADR-037).

**`HaQuestion`** *(child of correspondence)* — `Number`, `Text`, `TopicId?`,
`OwnerUserId?`, `DueOn?`, `ResponseText?`, `RespondedOn?`, `CurrentStatus` +
history.

> The owner is a **`UserId`**, not a `ContactId`. A `Contact` (EPIC-016) is a
> person at an external regulatory party; the response lead is one of ours. The
> two are never the same person and must never share a field.

**`Commitment`** *(root)* — `Title`, `Description`, **`AuthorityId`**,
`RegistrationId?` / `RegulatoryApplicationId?` (what it is about),
`SourceCorrespondenceId?` / `SourceMeetingId?` (where it arose), `GivenOn`,
`DueOn`, `OwnerUserId`, `CurrentStatus` + history.

**`HaMeeting`** *(root)* — `Subject`, `MeetingTypeId`, `AuthorityId`,
`RequestedOn`, `ScheduledFor?`, `Minutes?`, `Outcome?`, anchors nullable,
`CurrentStatus` + history + **transition table**. Attendees are deferred — the
least-asked of the questions above, and a child collection is cheap to add.

**`Inspection`** *(root)* — `Title`, `InspectionTypeId`, `AuthorityId`,
**`OrganizationSiteId?`** (an inspection inspects a *site* — EPIC-016's),
`ScheduledFor?`, `ConductedOn?`, `Outcome?`, `CurrentStatus` + history.

### Statuses, and the collisions caught before they were built

| | Statuses | Transition table |
|---|---|---|
| `HaCorrespondence` | **none** | — |
| `HaQuestion` | `Open` → `Responded` → `Resolved` | no — our process |
| `Commitment` | `Open` → `InProgress` → `Fulfilled` / `Waived` | no — our process |
| `Inspection` | `Scheduled` → `InProgress` → `Completed` | no — our process |
| `HaMeeting` | `Requested` → `Granted` / `Declined` → `Held` / `Cancelled` | **yes** |

**`HaMeeting` is the exception, and the reason is not that it resembles
`Registration`.** It is that the graph itself contains a fork **the authority
chooses** — granted or declined is not our decision to record at will. Every
other lifecycle here is entirely our own process, which is the `MarketStatus`
shape (ADR-039 decision 6).

Three collisions refused, on ADR-039's vocabulary rule:

- **`Cancelled`** would have meant *the meeting did not happen* and *the
  authority released us from an obligation*. Two concepts → the commitment gets
  **`Waived`**. The test is **who performs the action**: fulfilment is something
  we perform; waiving is something the authority does. *Released* reads
  contractual rather than regulatory.
- **`Completed`** (the inspection event finished) is not **`Fulfilled`** (the
  obligation discharged).
- **`Closed`** is banned outright — the vaguest word available, and it would have
  carried three meanings across four objects.

`Open` is deliberately reused across question and commitment: one concept, two
tiers, which ADR-039's vocabulary rule permits and encourages.

### Vocabularies — three buckets

| Bucket | Members |
|---|---|
| **Enum** — a rule branches on it | `Direction`; all four statuses |
| **Reference data** — governed, tenant-extensible facts | `CorrespondenceType`, `MeetingType` (FDA Type A/B/C/D is authority-defined and legislated), `InspectionType`, `QuestionTopic` |
| **Curated constant** — only a dropdown needs labels | correspondence format (letter / email / portal) |

**Five correspondence classifications become two.** RIM has Action, Format, Mode,
Type and Category. The questions demand `Direction` and `Type`; format is a
constant. Action, Mode and Category wait for something to ask for them.

### Change-case analysis

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| Reminders and escalation (EPIC-014) | High | due dates are stored data; a scheduler reads the same rows |
| Process Step becomes the anchor (EPIC-020) | High | nullable `ProcessStepId` seam present from day one |
| The bitemporal history shape is extracted | High | four identical entries, cut line fixed by ADR-039 decision 6 |
| Commitments cite studies (EPIC-019) | Medium | nullable seam |
| Threaded follow-up questions | Medium | self-referencing link, no aggregate change |
| Authority-specific question taxonomies | Medium | `QuestionTopic` is reference data, authority-scopable |
| Meeting attendees | Medium | child collection, additive |
| Inbound email ingestion | Medium | correspondence is already the target shape; ingestion is an adapter |
| Volume growth → search | Medium | read-side only |

### Hypotheses this epic carries

Per the register in [FEATURE-DEVELOPMENT-FLOW](../FEATURE-DEVELOPMENT-FLOW.md).
**Phase 5 owes an outcome on every one, including the failures.**

1. **The bitemporal extraction** (ADR-039 decision 6). Occurrences three through
   six arrive here. *Write the first by hand; extract with real consumers
   visible.* Falsified if, at S007, the extraction still is not obviously worth
   doing.
2. **Identity over convenience facts** — *prefer storing canonical identity and
   projecting derived views over persisting convenience facts.* Four independent
   tests: due-date proximity, the commitment's authority, a question's days
   overdue, and the correspondence anchor. Confirmed only if the epic lands there
   **without being told to**. Earns a place in ADR-040 if so.
3. ~~**ADR-038's division prediction.** `OrganizationDivisionId` gets its first
   holder in S001, or that root's justification never materialised.~~
   **RESOLVED 2026-08-01 — falsified, before S001 was written.**

   `OrganizationDivision`'s own doc comment named this epic as its justification:
   *"the authority division that reviews a submission… EPIC-006 will point an
   Application, a Licence and an HA Meeting at this division."* It cannot.

   `Authority` is **ReferenceData** — global, seeded. `OrganizationDivision` and
   `Contact` hang off **`Organization`** — tenant-owned — and `OrganizationType`
   is `Manufacturer`, `Sponsor`, `MarketingAuthorizationHolder`,
   `ContractResearchOrganization`. **There is no way to express "FDA" as an
   `Organization`**, so an FDA division and an FDA reviewer are both
   unrepresentable today.

   Widening `OrganizationType` was considered and rejected: it would produce
   *FDA (reference data)* and *FDA (tenant organization)* — a canonical world
   fact duplicated across a boundary, which ADR-039 decision 1 forbids and which
   would regress the ADR-030/032 split.

   > **The two divisions share a name and not an identity.** One describes
   > regulators; the other describes companies. Different universes.

   So `HaCorrespondence` carries **no** `OrganizationDivisionId` — *better no
   field than a misleading one*. The prediction fails in the form ADR-038 wrote
   it. It may still be redeemed narrowly if the **sponsor-side** division earns a
   reference on a meeting (S005); that is a separate, weaker claim and is not
   assumed.

   **The process gets the credit, not the reviewer.** Phase 2 asked what a user
   needs to file, find and understand a letter. Had it started from the entity
   list, `OrganizationDivisionId` would have been wired in *because it already
   existed*.
4. **Event, not lifecycle** *(new, from this Phase 2)* — *if every apparent
   "status" of an object is really derived from related objects or dates, the
   object may be an event rather than a lifecycle.* `HaCorrespondence` is the
   first instance. **One example is not enough to promote it**; watch for a
   second here and record the outcome in the retro. Do not add it to the flow.

---

## Phase 3 — Stories *(approved 2026-08-01)*

Seven vertical slices, in EPIC-017's cadence: **vocabulary → identity → local
concepts → business history → operability → working surface → projection.** Each
carries its own working surface, and each **reads back the history it writes**
(testing.md principle 8).

The `S000` the sketch called for is gone — Phase 2 settled the vocabularies.

| # | Story | Slice |
|---|---|---|
| **S001** | ✅ **A letter, filed where it belongs** — the `Interaction` context, `HaCorrespondence` with direction, type, authority and nullable anchors; a list and its own page. **ADR-040.** | full slice |
| **S001a** | **Who at the authority** — `AuthorityDivision` under `Authority`; correspondence gains the division that actually sent it | full slice |
| **S002** | **The letter's content** — attachments; `IFileStorage` moves to `src/Storage`; decision 0 built | full slice |
| **S003** | **The questions inside it** — `HaQuestion` with owner, due date, response and the epic's first dated history, rendered on the correspondence page | full slice |
| **S004** | **What we promised** — `Commitment` from a question or standalone, dated history, its own page, **and the "what's due" view** | full slice |
| **S005** | **Meetings** — request → grant → hold → minutes and outcome; the one transition table | full slice |
| **S006** | **Inspections** — anchored to an `OrganizationSite`, dated history | full slice |
| **S007** | **Capstone** — the application activity timeline (the falsified supertype, as a read model), narrative browser proof, ADR-040, retro | UI → test → docs |

**Not split into 006a/006b.** The pull toward a split was the supertype question,
and Phase 2 answered it. Four additive greenfield aggregates are far cheaper than
EPIC-017's tier insertion — no migration of existing rows, no re-pointing of
foreign keys. If it does sprawl, the fracture line is **after S004**: S001–S004
is the daily work and ships alone.

**ADR-040 — *the health-authority interaction cluster is one bounded context*,
written at S001, not S007.** CLAUDE.md requires an ADR *before* a new bounded
context, and `src/Interaction/` is created in S001 — waiting until the capstone
would invert what an ADR is for. It covers the context boundary, why this is not
`Product` or `Registration`, why correspondence owns its own content, and why
storage is shared infrastructure rather than shared domain. S007 appends the
hypothesis outcomes and the retro observations, the way ADR-039 was staged.

> **S001a exists because S001 found a contradiction, not a missing field.**
> *"Which FDA office sent it?"* turned out to require reference-data modelling,
> seeded hierarchies and a governance question — enough design to deserve its own
> story. And its two questions are deliberately asked in order: **what is an
> authority division** first, **who may define one** second. Answer the domain
> question before the governance question; seeded-only may well be sufficient
> until EPIC-012.
