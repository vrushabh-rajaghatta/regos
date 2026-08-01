# EPIC-006 — Health-authority interactions

**Status:** ⚪ Not Started · **Branch:** `epic/EPIC-006-health-authority-interactions` (cut at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

Everything that passes between the sponsor and the authority **after** a filing — letters, questions, meetings, commitments, inspections. In headcount terms this is what a regulatory affairs team actually does all day, and today it lives in inboxes and spreadsheets.

> **Phase 1 below is settled.** **Phases 2–3 are a sketch**, written so this epic can be picked up months from now without re-deriving it — they are **not approved design**. Confirm, amend or replace them in the Phase-2 conversation when this epic is pulled into **Now**.

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
- **`HaCorrespondence`** — name, description, date, action, format, mode, type, category, response-due date, initiator/recipient, health authority + division + contact + contact role; attachable `Content` (reuses `ProductDocument`).
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
| **Question response authoring / document assembly** | A response is `Content` — the existing ProductDocument machinery. Structured response-package building is → **EPIC-007**. |
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
- **Every status on all five objects carries a dated history** (`OccurredOn` / `RecordedOnUtc`) — RIM marks Commitment, Inspection and Question status "Single / Historical".
- Due-date proximity is **derived, never stored** (EPIC-005 precedent).
- Browser proof: log a correspondence → raise two questions → answer one → convert the other into a commitment → see both in the due view.
- ADR written for the interaction cluster's context boundary.

---

## What EPIC-017 settles *(added 2026-08-01 — amends the sketch below)*

EPIC-017 shipped after this sketch was written, and [ADR-039](../../adr/ADR-039-the-market-local-product-tier.md)
answers four questions the sketch left open and adds one it did not ask. Read
this section **before** Phase 2; where the two disagree, this section is later.

Everything here is still *sketch*. It narrows the Phase-2 conversation; it does
not replace it.

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

## Phase 2 — Domain design *(sketch — not approved · see "What EPIC-017 settles" above)*

### Context

*Lean: one new bounded context, `src/Interaction/`* (or `src/HealthAuthority/`). The five objects are a cohesive cluster with heavy mutual cross-links — splitting them into five contexts would make almost every query cross-context. RIM treats them as one neighbourhood too.

### Aggregate boundaries

| Object | Root or child | Why |
|---|---|---|
| `HaCorrespondence` | **root** | referenced by Application, Submission, Commitment, Meeting; own lifecycle |
| `HaQuestion` | **child of correspondence** | RIM: Correspondence is `Parent, Single, Required`. A question has no meaning without the letter it arrived in. *But* it carries its own due date, owner and status — if the "due view" needs to query questions directly and often, revisit. |
| `Commitment` | **root** | referenced from four other objects; long-lived, outlives its source |
| `HaMeeting` | **root** | scheduled independently, referenced by correspondence and commitments |
| `Inspection` | **root** | independent lifecycle, referenced by Process Step later |

All carry `TenantId` and a fail-closed query filter (ADR-031) — same reasoning as EPIC-016 decision 2.

### The cross-link web *(RIM's, worth reproducing)*

```
RegulatoryApplication ──┐
Submission ─────────────┼──▶ HaCorrespondence ──▶ HaQuestion
Registration ───────────┘         │  ▲                 │
                                  │  │                 ▼
                       HaMeeting ─┘  └──────────── Commitment
                                                        ▲
                                          Inspection ───┘  (both via Process Step, EPIC-020)
```

Most of these are `Multiple` in RIM and nullable — model as nullable FKs or join tables, and **do not** make any of them required. An interaction that cannot be filed against anything is still a real interaction.

### Decisions to settle (Phase 2, on pull-in)

**0. What is the natural owner of authority correspondence?** *No lean — settle
this first, before aggregate boundaries.*

Everything else in this epic can be built around whatever answer emerges:
aggregates, histories, vocabularies, working surfaces. **Correspondence has to
live somewhere**, and it is an anchor rather than a leaf — EPIC-018 (labeling)
and EPIC-010 (IDMP) will attach documents of their own, and they will attach
them wherever this decision puts them.

The pressure: `ProductDocument` is anchored to the **global** product
([ADR-039](../../adr/ADR-039-the-market-local-product-tier.md), Consequences —
`ProductDocument` deliberately did *not* move to the market tier). A letter from
Health Canada about a Canadian market presence is not obviously a *product*
document at all. Candidate answers, none yet argued:

- **The interaction owns it** — correspondence holds its own content, and
  `ProductDocument` stays what it is: a dossier artefact.
- **`ProductDocument` widens** — one document store, its anchor becomes
  polymorphic or moves to the market tier.
- **A third thing exists** — RIM's `Content` is broader than either, and
  neither of ours is it.

This is the only decision in EPIC-006 that plausibly changes epics other than
EPIC-006. Answer it first; the aggregate boundaries below get easier once it is
settled.

**1. One context or several.** *Lean: one.* Record the reasoning; it is the ADR.

**2. `HaQuestion` — child or root.** *Lean: child*, per RIM. ~~The pressure to promote it comes from the due view; measure before promoting.~~ → **amended, §1**: the due view is a read model and exerts no pressure at all. Decide on behaviour.

**3. Dated status history on all five.** *Recommended.* This epic is where the cross-cutting history rule earns its keep — four of the five statuses are "Single / Historical" in RIM. ~~Reuse `RegistrationStatusEntry` verbatim rather than inventing a per-aggregate shape; if this is the third occurrence, **extract the shared shape**.~~ → **amended, §2 and §3**: reuse the entry, not the lifecycle; default to no transition table; write the first one by hand before extracting.

**4. Correspondence direction.** RIM has `Correspondence Mode`/`Action` but no explicit inbound/outbound flag. *Lean: add one* — every real query starts with "did they write to us or we to them?", and deriving it from initiator/recipient names is fragile.

**5. Controlled lists.** Correspondence Action/Format/Mode/Type/Category, Meeting Type/Format/Status, Commitment Type/Source/Status, Question Topic, Inspection Type/Source — RIM makes all of these controlled lists. That is **11+ vocabularies**. Decide up front: reference data (feeds EPIC-012 authoring) or closed enums. ~~*Lean: reference data for the classifications, closed enums for the statuses*~~ → **amended, §4**: three buckets, not two. Most classifications need a curated constant, not a governed aggregate.

**6. Attachments reuse `ProductDocument`.** RIM points Correspondence at `Content`, which is what `ProductDocument`/`DocumentVersion` already is. Do not build a second document store — ~~*lean: reuse*~~ → **superseded by decision 0**, which asks the prior question. "Do not build a second store" survives as a constraint on the answer, not as the answer.

**7. Is the authority an intrinsic fact of a `Commitment`, or inherited context?** *No lean.* The one place ADR-039 decision 1 is under genuine pressure, and it resolves in one of two directions:

- If commitments **can genuinely originate independently**, the authority is intrinsic — a commitment is *made to* an authority, which is its own fact rather than a copy of a letter's, and storing it does not violate decision 1.
- If **every commitment ultimately originates from an interaction**, then the three nullable sources are not evidence of independence. They are pointing at a **missing abstraction** — some common notion of *the interaction this arose from* that we have not named yet.

Do not answer it here. Make sure Phase 2 does, because the second answer would change the model rather than a field.

> **Decisions 0 and 7 may be the same decision.** If the missing abstraction in
> the second branch above is *"the authority interaction this arose from"* —
> correspondence, meeting and inspection being three kinds of one thing — then it
> is also the natural owner of correspondence content, and decision 0 falls out
> of it. The epic's own title contains the word. **Notice this; do not build it.**
> A supertype spanning three roots on the strength of a symmetry is precisely
> what [ADR-018](../../adr/ADR-018-rule-of-three.md) forbids — *symmetry is not a
> demonstration.* Test it in Phase 2 against real questions, and if it survives,
> it is ADR-040's subject rather than a note in this file.

### Change-case analysis

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| Reminders and escalation (EPIC-014) | **High** | Due dates are stored data; a scheduler reads the same rows |
| Process Step becomes the anchor (EPIC-020) | **High** | Nullable `ProcessStepId` seam present from day one |
| Commitments cite studies (EPIC-019) | Medium | Nullable seam |
| Questions need threaded follow-ups | Medium | RIM has `Related Questions` (Multiple) — self-referencing link table |
| Authority-specific question taxonomies | Medium | Question Topic as reference data, authority-scopable |
| Inbound email ingestion | Medium | Correspondence is already the target shape; ingestion is an adapter |
| Volume growth → search | Medium | Read-side; no aggregate change |

---

## Phase 3 — Candidate stories *(sketch — re-slice on pull-in)*

| # | Story | Slice |
|---|---|---|
| **S001** | **`HaCorrespondence`** — record a letter against an application/submission, with authority + division + contact, and attach content | domain → persistence → API → UI → test |
| **S002** | **`HaQuestion`** — raise questions under a correspondence, assign owner + due date, answer them, dated status | full slice |
| **S003** | **`Commitment`** — create standalone or from a question, owner, due date, dated status; **the "what's due" view** across questions + commitments | full slice |
| **S004** | **`HaMeeting`** — request, schedule, hold, close with minutes and outcomes | full slice |
| **S005** | **`Inspection`** — record and track, dated status | full slice |
| **S006** | **Capstone** — the *"what's due"* view across all five, narrative browser proof of the full journey, ADR, retro | UI → test → docs |

> **Amended, §8.** Each of S001–S005 carries its own working surface and its own
> read-back of the history it writes; the capstone is the cross-aggregate
> projection, not the first time any of this is visible. And a story **S000** —
> the five status vocabularies settled on one page (§7) — comes before S001.

**ADR to write:** *The health-authority interaction cluster is one bounded context* — next free number (**ADR-040**).
