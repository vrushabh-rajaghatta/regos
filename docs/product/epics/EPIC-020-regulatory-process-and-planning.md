# EPIC-020 — Regulatory process & planning

**Status:** ⚪ Not Started — **Phase 1 approved 2026-08-06** · **Branch:** `epic/EPIC-020-regulatory-process-and-planning` (cut at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

RIM's **spine**. Everything RegOS builds — applications, submissions, correspondence, meetings, commitments, inspections — becomes a *step in a plan* that serves an *objective*. This is the layer that turns a record system into a system that tells you what to do next.

> **Phase 1 below is approved, and the epic is architecturally green.** The four [invariants](#the-four-invariants) and D2/D3/D6/D7/D9 are decided. **ADR-065 is the implementation contract, not another round of design exploration** — the remaining leans (D1, D4, D5, D8) are modelling choices to be validated against real code as S001–S003 progress, not identity questions to be reopened.

---

## What this file predicted, and what the code says

*This epic was sketched on 2026-07-25 and sat for six weeks. Three of its load-bearing claims were checked against the repository before anything was planned on top of them. **Two were wrong**, and both wrongs change the work.*

| The sketch said | The code says | What it changes |
|---|---|---|
| *"EPIC-004 and EPIC-006 are told to leave a nullable `ProcessStepId` seam; this epic fills it."* | **`grep -rn "ProcessStep" src/` returns zero.** No seam exists anywhere. And this was **deliberate** — [EPIC-006's retro](EPIC-006-health-authority-interactions.md) records it under *what the change-case analysis got right*: *"the nullable `ProcessStepId` seam was not added — Phase 2 chose to model it only when EPIC-020 arrives, and nothing has needed it."* | The wiring story is **six migrations on shipped tables**, not six columns already waiting. Cheap today because RegOS is pre-customer and every one is a nullable add; **this is the window, and it does not widen** |
| *"The seams point inward … it keeps Process optional and deletable."* | Inward seams make Process **depended upon by five contexts**. That is the opposite of deletable. | The property actually on offer is **runtime-optional** — every FK nullable, RegOS fully usable with no plan in it. Worth having, and worth not confusing with the other one. **D2 is now a real decision rather than a lean** |
| *"The timeline view — a plan's steps, what is late."* | [`ListDueWork`](../../../src/Interaction/RegOS.Interaction.Application/Queries/ListDueWork/ListDueWorkHandler.cs) already answers *"what work remains?"* across correspondence, questions and commitments. | **"Due" and "late" are two facts, not one** (D7). An obligation is owed to a regulator; a late step is our own plan slipping. Merging them is the mistake this epic is most likely to make |

**What the sketch got right, and it is the most useful thing in it:** the template → version → instantiate pattern is real, built, and reusable. [`RegulatoryTemplate`](../../../src/ReferenceData/RegOS.ReferenceData.Domain/Blueprint/RegulatoryTemplate.cs) owns versions, assigns their numbers, permits one open draft, freezes on publish, and deprecates without breaking anything already bound. [ADR-035](../../adr/ADR-035-submissions-bind-to-a-published-template-version.md) pins the *version*, not the template, and resolves tenant-owned before shared. **Do not re-derive any of that.**

---

## Phase 1 — Epic plan

### Outcome

A regulatory user states an **objective** (*"get Acme-123 approved in Japan"*), instantiates a **plan** from a published **playbook version**, and works dated **steps** with dependencies — with the real submissions, meetings, letters, commitments and inspections attached to the steps that produced them. ***"What is next, what is late, and what does it block?"*** becomes answerable, and the answer is not a spreadsheet.

### The questions it exists to answer

*Per [Phase 2's rule](../FEATURE-DEVELOPMENT-FLOW.md#phase-2--design-the-domain-entities-columns-future-proofing) — begin with the question, not the entity list. Applied here at Phase 1 because this epic's whole risk is importing six RIM objects that nobody asked for.*

| # | The question, in a user's words | Answered by | Needs a new concept? |
|---|---|---|---|
| Q1 | *"What do we have to do to file this, and in what order?"* | a published playbook | **Yes** — a definition, its versions, its steps |
| Q2 | *"What are we trying to achieve here, and why this route?"* | the objective's strategy | **Yes** — objective |
| Q3 | *"Where are we, and what is next?"* | the live plan's steps | **Yes** — plan + step |
| Q4 | *"What is late, and what does it block?"* | derived from steps + dependencies | **No** — a read |
| Q5 | *"This submission — what plan is it part of?"* | the step link | **No** — a nullable FK |
| Q6 | *"We did this last year. Do it again."* | instantiate the playbook | **No** — Q1 + Q3 |
| Q7 | *"How is the global programme going, across all markets?"* | — | **Deferred.** Needs objectives in ≥2 markets, and none exist. See *refused* below |

**Four concepts, from seven questions.** RIM offers six objects; two of them do not have a question yet.

### The workflow, end to end

The concrete path a story-by-story build has to make real. **US FDA IND, initial filing** — the vertical RegOS is proving.

```
1  STEWARD authors the playbook          "US FDA IND — initial filing", scoped to US + IND
   ├─ 12 step definitions, each with predecessors and an offset in days
   └─ publish v1                          → frozen, and now instantiable

2  RA LEAD states the objective          "Open an IND for Acme-123 in the US"
   ├─ strategy: 505(b)(1), pre-IND meeting first
   └─ target: first-patient-in by 2027-Q1

3  RA LEAD instantiates the plan         from playbook v1, anchored on 2026-09-01
   └─ 12 live steps, dates DERIVED ONCE from the offsets, then owned by a human
                                          ← the version is PINNED (ADR-035's rule)

4  THE TEAM works the plan               step "Pre-IND meeting request" → Complete, actual 2026-09-12
   └─ steps carry planned vs actual start/end and a dated status history

5  THE WORK ATTACHES ITSELF              the pre-IND meeting is booked in RegOS
   ├─ HaMeeting.ProcessStepId → the "Pre-IND meeting" step
   └─ the step shows the meeting; the meeting shows its step

6  THE PLAN ANSWERS Q4                   "IND submission" is 9 days late
   └─ and it blocks 4 downstream steps, transitively — derived on read, never stored
```

**Step 3 is the architecturally interesting one** and the only place a decision is forced (D5). Everything else is a shape this codebase has already built at least once.

**Steps 1 and 3 draw a line this epic must not blur.** Dates are derived **once**, at instantiation, from the playbook's offsets. After that a human owns them: moving a step moves nothing else. Ongoing recalculation — lead/lag, calendars, critical path — is a product of its own and stays out (below). Naming it here because *"the dates came from somewhere"* and *"the dates maintain themselves"* are one keystroke apart in a demo and a year apart in build.

> **Derive-once is not a simplification, it is [I4](#i4--a-plan-is-permanently-bound-to-a-published-definition-version).** The convenient alternative — a playbook edit rippling into every live plan — has to answer *"why did this milestone move?"*, and answering it needs audit history, recalculation rules, exclusions and rebasing semantics. None of those problems are solved here. **They are not created.**

### The concepts

| | Means | RIM object | Verdict |
|---|---|---|---|
| **`ProcessObjective`** | what we are trying to achieve in one market, and the strategy for it | Process Objective (23) | ✅ **build** — Q2 |
| **`ProcessDefinition`** / **`ProcessDefinitionVersion`** / **`ProcessStepDefinition`** | the authoritative, versioned, scoped playbook and its steps | Process Plan Template (11) · Process Step Template (12) | ✅ **build** — Q1, Q6 |
| **`ProcessPlan`** | a live plan, pinned to the `ProcessDefinitionVersion` it came from | Process Plan (16) | ✅ **build** — Q3 |
| **`ProcessStep`** | a live, dated step with predecessors and a status history | Process Step (22) | ✅ **build** — Q3, Q4 |
| **`ProcessObjectiveGroup`** | a programme spanning markets | Process Objective Group (3) | ⏸ **deferred, and named** — Q7 has no asker. RegOS holds no product with objectives in two markets, so this is [ADR-038](../../adr/ADR-038-organization-depth-roots-and-the-three-filter-shapes.md)'s *"a root justified by a query that does not exist yet is a demo of an empty table"*. **Milestone: the second objective for one product in a second market, plus somebody asking Q7** |

**That is 4 of RIM's 6, and the runway's "6 objects → ~98%" is therefore wrong by two.** Recorded now rather than discovered at the retro. The coverage figure is a map, not a target — and one of the two refusals is a genuine absence rather than a disagreement.

### In scope ✅

- **`ProcessDefinition` / `ProcessDefinitionVersion` / `ProcessStepDefinition`** — versioned, published, immutable once published; scoped by country + application type **+ authority** (change-case: high); steps carry parent/child, predecessors and a day offset. **Seeded** for US·FDA·IND.
- **`ProcessObjective`** — name, type, dated status history, planned/actual start and end, target product + country, strategy type, decisions, details, references; nullable link to the `RegulatoryApplication` that carries it.
- **`ProcessPlan`** — instantiated from a **pinned published template version**, with dates derived once; own dated status history; ad-hoc plans (no template) supported, because [ADR-035 §4](../../adr/ADR-035-submissions-bind-to-a-published-template-version.md) already ruled that missing reference data never blocks the business.
- **`ProcessStep`** — planned and actual start/end, dated status history, predecessors, parent/child, and the template step it came from.
- **The nullable `ProcessStepId` on six aggregates** — `Submission`, `HaMeeting`, `HaCorrespondence`, `Commitment`, `Inspection`, `Registration`.
- **The plan board** — the live plan, its steps, what is late, and what that blocks. Derived on read.
- **A `Process` context that a tenant can ignore entirely** — every link nullable, no existing screen changes behaviour when no plan exists.
- **ADR-065**, browser proof, retro.

### Out of scope ⏸️ (deferred, with reason)

| Deferred | Why |
|---|---|
| **Ongoing date recalculation, critical path, calendars, lead/lag** | A PPM product of its own. Derive once, then a human owns the dates. **Revisit on a concrete request, not on the first demo where it looks nice.** |
| **Resource assignment and capacity planning** | Not in RIM's process objects, and squarely PPM. |
| **Gantt rendering** | A view concern → **EPIC-011**. This epic delivers the dates and the dependencies, which is the part that cannot be added later. |
| **Notifications on a slipping step** | → **EPIC-014**, consistent with EPIC-005 and EPIC-006 deferring the same half. |
| **Playbook authoring UI** | → **EPIC-012**, which owns reference-data authoring across the board and now owns the read half too. **Seed here; author there.** |
| **`ProcessObjectiveGroup`** | Q7 has no asker — see the concepts table for the milestone. |
| **Retro-fitting plans onto historical records** | Plans are forward-looking. Nothing forces an existing submission into a plan, and every link is nullable so nothing has to. |
| **A unified "my work" view spanning plans and obligations** | D7. Two facts; the merge is EPIC-011's, and it composes two queries rather than replacing them. |
| **Re-binding a live plan to a newer playbook version** | D6 answers *what happens* (nothing, visibly). *Migrating* one is a policy decision, and [ADR-035](../../adr/ADR-035-submissions-bind-to-a-published-template-version.md) has it open for submissions too. **Answer it once, for both, when someone needs it.** |

### Definition of Done

1. A `ProcessDefinition` can be seeded, versioned and published for a country + authority + application type, with steps carrying predecessors and offsets — and the UI calls it a **Playbook** throughout ([D9](#d9--terminology-settled-2026-08-06--this-is-a-decision-and-it-goes-in-adr-065)).
2. An objective can be created for a product in a market, carrying its strategy.
3. A plan instantiates from a **pinned published version**, and its steps arrive dated.
4. Steps carry planned **and** actual start/end and a dated status history.
5. A submission, meeting, correspondence, commitment, inspection and registration can each be attached to a step, and **both ends show it**.
6. ***"What is late, and what does it block?"*** is answerable from the plan, derived on read, transitively.
7. A plan whose playbook version was later superseded **keeps working, and says so** — the pin is the point, and D6 makes it visible.
8. **`ListDueWork` is unchanged and still passes** — the proof that [D7](#d7--late-and-due-are-two-facts) held.
9. **RegOS with no plans in it behaves exactly as it does today**, on every existing screen — the test for [I1](#i1--regulatory-process-is-an-optional-bounded-context).
10. **A published definition version cannot be edited, and no plan can be repointed at another version** — both refused in the domain, both with a test naming [I4](#i4--a-plan-is-permanently-bound-to-a-published-definition-version).
11. **No Process code creates, transitions or deletes an entity outside Process** — [I2](#i2--process-never-owns-the-lifecycle-of-an-entity-outside-process). The FK direction makes this structural rather than merely intended.
12. Browser proof of the whole workflow above, steps 1→6, in one run.
13. ADR-065 written; `ContextDependencyTests` extended with the `Process` entry and every new edge argued in the ADR.

---

## The four invariants

*Settled at Phase-1 sign-off, 2026-08-06. Everything else in Phase 2 is still a lean; **these four are not**, and ADR-065 carries all four. They are stated together because each one protects a different thing, and each is cheap to violate by accident in a story that means well.*

### I1 — Regulatory Process is an **optional** bounded context

> **Existing regulatory workflows remain valid when no process plan exists. Every integration is nullable and additive.**

This replaces the sketch's *"optional and deletable"*, which was never available once five contexts point at Process. **The stronger claim is the one that is true**: RegOS with an empty `Process` schema behaves on every existing screen exactly as it does today. That is a property a test can hold — and DoD 9 is that test.

### I2 — Process **never owns the lifecycle** of an entity outside Process

> **The links are annotations, not ownership.**

A step may say *"this submission belongs to me"*. It may never create, transition, publish, withdraw or delete one. `Submission` owns submissions; a plan observes them. The direction of the FK (I3 below, D2) makes this natural rather than merely intended — the artifact points at the step, so the step has no handle to act through. **The failure this forbids** is the plan board growing a "mark step complete → publish the submission" button, which reads as convenience and is a second lifecycle for a regulated record.

### I3 — Each context owns itself and *optionally* references Process

```
   Submission ──┐
   Meeting ─────┼──► ProcessStep        each context owns itself
   Correspondence┤                       each optionally references Process
   Commitment ──┤                        nothing requires Process
   Inspection ──┤
   Registration ┘
```

The alternative — Process holding ids for all six — makes Process the centre of the regulatory model, which is backwards. Process is a **consumer** of the regulatory domain, not its hub.

### I4 — A plan is permanently bound to a **published** definition version

> **A process definition is immutable once published. A plan pins the version it was instantiated from, forever. No plan is ever automatically rebased to a newer version, and no plan may point at "latest".**

The two halves reinforce each other:

| | What it protects |
|---|---|
| **Immutable once published** | a version can never be edited under a plan that is already running against it — the frozen-on-publish rule `RegulatoryTemplateVersion` already enforces |
| **Pinned forever, never auto-rebased** | *"why did this milestone move?"* has an answer, always, and the answer is never *"the playbook changed under us"* |

**This is the same invariant as [ADR-035 §1](../../adr/ADR-035-submissions-bind-to-a-published-template-version.md)**, arrived at from a different direction: a submission must be able to name the standard it was judged against, and a plan must be able to name the playbook it was scheduled from. Both are regulated records; neither may have its past rewritten by a steward publishing v3.

**It is also what makes [derive-once](#the-workflow-end-to-end) coherent rather than merely simple.** The alternative — a playbook change rippling into every live plan — sounds convenient until a regulator asks why a milestone moved, and answering needs audit history, recalculation rules, exclusion lists and rebasing semantics. Deriving once and pinning forever removes all four problems by not creating them. **Re-binding, when someone eventually needs it, then becomes a comprehensible operation on a comprehensible record** — *"this plan came from playbook v2; here is what v3 changed"* — rather than an archaeology exercise. That is the deferred half of ADR-035, and it stays deferred with a much better foundation under it.

---

## Phase 2 — Decisions

*Sign-off settled **D2, D3, D6, D7 and D9**. **D1, D4, D5 and D8 remain leans** and are the Phase-2 conversation.*

### D1 — Mirror the template machinery, or generalise it?

`RegulatoryTemplate` and `ProcessDefinition` would be the **second** occurrence of *versioned-scoped-published-frozen-instantiated*. **Lean: mirror, do not extract.** [ADR-018](../../adr/ADR-018-rule-of-three.md) wants three demonstrated needs, the payloads are very different (a section tree with eCTD folders vs a step graph with offsets), and EPIC-006 measured a near-identical extraction candidate at six occurrences and **still refused it** because the shapes never converged. Record the refusal with the count so the third occurrence has something to measure against.

### D2 — Which way does the step ↔ artifact edge point? **(the epic's central decision)**

| | Artifact holds `ProcessStepId` | Step holds the artifact ids |
|---|---|---|
| Cardinality | many artifacts → one step, naturally | one of each kind per step, or a join table |
| Referential integrity | typed FK per artifact | typed FKs, or a polymorphic pair **[ADR-042 refused one](../../adr/ADR-042-what-the-interaction-context-turned-out-to-be.md)** |
| Dependency graph | **Submission, Interaction, Registration → Process** | **Process → six contexts** |
| Deleting the Process context | breaks five contexts | costs nothing |
| Cost of building | 6 migrations on shipped tables | 1 table, no migrations elsewhere |

**Settled at sign-off: the artifact holds the nullable `ProcessStepId`** — this is [I3](#i3--each-context-owns-itself-and-optionally-references-process), and [I2](#i2--process-never-owns-the-lifecycle-of-an-entity-outside-process) is the rule that keeps it honest. Cardinality is right without a join table, `Commitment` already carries five typed nullable FKs and ADR-042 refused the polymorphic alternative by name, and the migrations are nullable adds on a pre-customer database. **Verify no cycle before writing it down** — EPIC-024's guard now makes that mechanical, and ADR-061 §3 is what happens when it is not checked.

What remains open in D2 is only *how* — column names, index strategy, and whether the reverse read (*"what did this step produce?"*) is one query per artifact type or one composed read.

**Refused in advance:** putting `ProcessStepId` in `Platform.Contracts` to avoid the edges. [ADR-041](../../adr/ADR-041-platform-contracts-and-the-identity-that-crosses.md) holds it to two types — *"three is a second kernel"*.

### D3 — Is `ProcessObjective` genuinely distinct from `ProcessPlan`?

*The [second Phase-2 question](../FEATURE-DEVELOPMENT-FLOW.md#the-second-question-added-2026-08-02-from-epic-004), and the one most likely to delete an object.* They are 1:1 today, and two aggregates in an undemonstrated 1:1 is what the smallest-faithful-model rule refuses.

**Settled at sign-off: keep both.** The test was *can we describe a regulatory objective before a schedule exists?* — and it was answered at sign-off with four:

> **FDA approval for Product X · CE MDR transition · expand an indication · renew an existing licence.**

Every one of those is stateable, ownable and reportable with no schedule under it at all. That is the demonstration the object needed, and it arrived before a line of code rather than after.

**The distinguishing sentence, and it is the one to keep:** *an objective is the goal; **plans are merely attempts**.* Today the MVP will only ever create one plan per objective, and that is fine — a 1:1 that is *conceptually* 1:N is a different thing from a 1:1 that is 1:1.

**The cost asymmetry runs the opposite way to the usual instinct**, which is why this is worth writing down: deleting an object later is far cheaper than recreating one after collapsing it. Collapsing loses the distinction *and* the data that recorded it; splitting later has to invent both. **So the falsification is retained, not the doubt** — if a year of use produces no objective that ever held two plans and no objective that ever existed without one, collapse them then, cheaply.

### D4 — Where does the context live, and what may it reference?

`src/Process/RegOS.Process.{Domain,Application,Infrastructure}`. **This is the first epic to add a bounded context since [`ContextDependencyTests`](../../../tests/Architecture/RegOS.Architecture.Tests/ContextDependencyTests.cs) made the graph a specification** — so the graph gains an entry, its negative control forces one, and every new edge needs the ADR CLAUDE.md already requires. Working set to confirm: `Process.Domain → Product, ReferenceData, RegulatoryApplication`, plus D2's three inbound.

### D5 — How are dates derived at instantiation?

**Lean:** each `ProcessStepDefinition` carries `Predecessors` and `OffsetDays`; instantiation walks the graph topologically from a single anchor date and writes planned dates once. Deterministic, testable without a database, and it produces a plan that is useful the second it exists. A cycle in the predecessor graph is rejected **at publish**, not at instantiation — the playbook is the thing that is wrong.

### D6 — What happens to a live plan when its playbook publishes a new version?

*Deferred twice — [ADR-035](../../adr/ADR-035-submissions-bind-to-a-published-template-version.md) lists "re-binding is unsolved" under its trade-offs.*

**Settled at sign-off by [I4](#i4--a-plan-is-permanently-bound-to-a-published-definition-version): nothing happens, and the plan says so.** The pin holds exactly as ADR-035 §1 holds it for submissions. What is new is the **disclosure** — a plan whose pinned version has been superseded shows it, derived on read, stored nowhere. That is the half that was missing rather than the half that was deferred: the deferred half is *migrating* a live plan, which stays deferred until someone needs it. **The same read applies to submissions** and should be recorded as owed rather than built here.

### D7 — "Late" and "due" are two facts

| | **Due** | **Late** |
|---|---|---|
| Whose obligation | a regulatory commitment | internal execution |
| Who cares | **the agency** | **the company** |
| Missing it | compliance implications | affects forecasting |

**Settled at sign-off: build the plan's view as a sibling, share no code, and change `ListDueWork` in no way.** DoD 8 is the test.

**The wording that matters — they may share *rendering* later, never *behaviour*.** A shared row component is a UI convenience; a shared query is a claim that these are one kind of thing, and they are not.

**This makes EPIC-011 cleaner rather than harder**, because "My Work" becomes **composition instead of replacement**:

```
My Work
├─ Regulatory Due      FDA response due tomorrow
├─ Internal Plan       Draft protocol is 5 days late
├─ Meetings            Prepare briefing package
└─ Questions           Answer reviewer
```

Four sources, one screen, and each source stays answerable on its own. **Milestone: EPIC-011.**

### D8 — Objective ↔ application, and what an objective targets

RIM draws `Process Objective → Application` as *Peer, Conditional*. **Lean: keep them distinct and let the objective hold a nullable `RegulatoryApplicationId`** — an objective is *"get approved in Japan"*, an application is the vehicle, and one objective may run through several over years. **Open:** does the objective target `GlobalProductId + CountryId`, or the `MedicinalProduct` (EPIC-017's market tier)? Lean **the former**, because the objective routinely exists before anyone has created the market-local product — with a nullable `MedicinalProductId` seam for when they have.

### D9 — Terminology **(settled 2026-08-06 — this is a decision, and it goes in ADR-065)**

> **The authoritative, versioned description of a regulatory process is named `ProcessDefinition` in the domain model and "Playbook" in the user interface.** A `ProcessDefinition` is immutable once published and is the artefact to which `ProcessPlan`s are permanently bound. The screen term is chosen for usability; the type name reflects the immutable, versioned, authoritative semantics established by [I4](#i4--a-plan-is-permanently-bound-to-a-published-definition-version).

**RIM's own word — `Process Plan Template` — is deliberately not used**, and this is the third time RegOS has kept RIM's *concept* while refusing its *shape or name* (after `Artwork` in EPIC-018 and `PackAuthorisation` in EPIC-010b).

**The argument is that I4 changed what this object is.** *Template* carries a settled meaning:

```
   template                              definition
   ─────────                             ──────────
   copy it        →  edit the copy       conform to it   →  pin the version
   copies diverge freely                 immutable once published
```

**S003 does the right-hand thing.** A plan does not hold an editable copy of the steps; it is bound to a published version it may never rewrite. Naming that a *template* invites exactly the mental model I4 forbids, and the invitation gets stronger as behaviour accumulates.

**The lifecycle is the tell.** As governance arrives — `Draft → Under Review → Approved → Published → Superseded` — *Published Definition* and *Superseded Definition* are what regulated systems call those artefacts. *Approved Template* reads as a mistake.

**And the noun stays whole.** Not `Process` — every aggregate keeps the prefix *and* the role:

| | |
|---|---|
| `ProcessDefinition` · `ProcessDefinitionVersion` · `ProcessStepDefinition` | what we conform to |
| `ProcessObjective` · `ProcessPlan` · `ProcessStep` | what we are doing |

**The pin targets `ProcessDefinitionVersion`, not `ProcessDefinition`** — the same deliberate exception [ADR-035 §2](../../adr/ADR-035-submissions-bind-to-a-published-template-version.md) makes for submissions, and for the same reason: *the version is the governance artefact*, so referencing the root would leave *"which version?"* unanswered at every point that matters.

**The pair is recorded in `docs/domain-model/process.md`**, per CLAUDE.md — *never let the screen's word reach a type, or the type's word reach a label*. S001 writes it, because S001 is what names the first file.

### Change-case analysis

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| Gantt / timeline visualisation (EPIC-011) | **High** | dates + dependencies stored; rendering is a read concern |
| Playbooks differ per **authority** as well as per country | **High** | authority is a scope dimension **from day one** — the sketch's best catch |
| A regional playbook serves a bloc (EU, ASEAN) | **High** | EPIC-022 shipped `Country.Regions` for exactly this; scope resolution can widen to a region without a migration |
| Step slippage notifications (EPIC-014) | High | planned vs actual dates stored; a scheduler reads them |
| A seventh artifact type attaches to steps | High | D2 — the new artifact adds a nullable FK; Process is untouched |
| Someone wants dates to recalculate | **High** | out of scope, and the predecessor graph is stored, so it becomes a service over existing data rather than a migration |
| Sub-plans / nested programmes | Medium | parent/child on steps today; `ProcessObjectiveGroup` named with its milestone |
| A playbook version supersedes a live plan | Medium | D6 — pinned, and now visibly |
| Ad-hoc plans with no playbook | Medium | template link nullable, per ADR-035 §4 |
| **Objective collapses into plan** | Medium | D3 — and pre-customer, so the collapse is a migration nobody pays for |

---

## Phase 3 — Stories *(re-slice as Phase 2 lands)*

**Every story opens on its question, never on its entity list** — EPIC-006's cadence finding, which it named for this epic to inherit: *"six of the eight stories produced a smaller model than the entity-first version would have."*

| # | Story | The question it opens on | Slice |
|---|---|---|---|
| **S001** | **The playbook** — `ProcessDefinition` + `ProcessDefinitionVersion` + `ProcessStepDefinition`, versioned, immutable on publish, scoped country + authority + application type, predecessors + offsets, cycle rejected at publish; **seeded for US·FDA·IND**; writes [`docs/domain-model/process.md`](../../domain-model/) with the D9 pair | *"What do we have to do to file this, and in what order?"* | domain → persistence → API → read UI → test |
| **S002** | **The objective** — strategy, dated status, target product + market, nullable application link. **D3 is settled here or the object is deleted here** | *"What are we trying to achieve, and why this route?"* | full slice |
| **S003** | **Instantiation** — a plan pinned to a published version, steps dated once from the offsets. The ADR-035 pattern, second occurrence | *"We did this last year. Do it again."* | full slice |
| **S004** | **Working the plan** — step status history, actual dates, the plan board | *"Where are we, and what is next?"* | full slice |
| **S005** | **What is late, and what does it block** — transitive successors, derived on read; D6's superseded-playbook disclosure | *"What is late, and what does it block?"* | read model → UI → test |
| **S006** | **Wiring, part 1** — `Submission` and `Registration` attach to steps; both ends show it. **D2 is proved or reversed here**, on the two cheapest contexts | *"This submission — what plan is it part of?"* | migration → API → UI → test |
| **S007** | **Wiring, part 2** — the four Interaction aggregates. **Guard: a step is not a commitment's fourth business origin** — [ADR-042 decision 2](../../adr/ADR-042-what-the-interaction-context-turned-out-to-be.md) fires on a fourth *origin*, and a step is what a commitment *serves*, not where it *arose* | *"What did this step actually produce?"* | migration → API → UI → test |
| **S008** | **Capstone** — the workflow steps 1→6 in one browser run, ADR-065, `ContextDependencyTests` extended, retro | — | UI → test → docs |

**The split point, declared now so it is a decision and not a rescue** — and confirmed at sign-off:

```
020a   playbooks · objectives · instantiation · working plans · late board     S001–S005
─────────────────────────────────────────────────────────────────────────
020b   existing-context wiring · capstone                                     S006–S008
```

**S001–S005 touch no existing context** — they add `src/Process/` with outbound edges only, and RegOS behaves identically with no plan in it ([I1](#i1--regulatory-process-is-an-optional-bounded-context)). **S006–S007 are where five contexts get migrations.** These are genuinely independent milestones: **if velocity slips, nothing has to be rethought** — the line is already where the architecture changes.

Taken as **one epic** on the argument that a plan nothing attaches to is a spreadsheet with a database behind it. The attachment is what stops the plan drifting from reality, and it is the whole reason to build this inside RegOS rather than beside it.

**ADR to write: ADR-065** — and it is framed on the invariant rather than the mechanism:

> **Regulatory Process is an optional bounded context.** Existing regulatory workflows remain valid when no process plans exist. Integrations are nullable and additive. A `ProcessPlan` is permanently bound to a published `ProcessDefinitionVersion`; definitions are immutable once published, and no plan is ever automatically rebased.

Carries **I1–I4** plus D1, D3, D5 and D9.

### What ADR-065 must carry beyond its decisions

*Written here because the ADR is authored after the merge, and both items below were established at sign-off rather than derived while writing it.*

#### 1. An **Architectural consequences** section — consequences, not decisions

None of these are new choices. Each **falls out of** I1–I4 and D2/D7/D9, and stating them separately is what lets a future reviewer check that a change preserved the intended *properties* rather than only the mechanics.

| Consequence | Falls out of |
|---|---|
| Existing bounded contexts remain independently usable | I1 |
| Process owns no external lifecycle | I2, D2 |
| Published definitions are immutable | I4, D9 |
| **Plans are historical records, not projections** | I4 |
| Existing "due work" semantics remain unchanged | D7 |

**ADR-065 therefore has three layers, and they do different jobs.** A reviewer should be able to use it without re-opening it:

| Layer | Says | What a reviewer does with it |
|---|---|---|
| **Decisions** (D*) | the architectural choices that were made | reads them, does not re-litigate them |
| **Invariants** (I*) | properties that must remain true throughout implementation | checks the implementation still holds them |
| **Consequences** | observable characteristics of the running system | **checks a diff preserves them** |

That is what makes *"does this change preserve the consequences?"* answerable without an argument about intent.

#### The one sentence to lead with

> **Plans are historical records, not projections.**

**It is not one of the five consequences — it is the one the others fall out of**, and it is worth stating in those words because it carries real design force. Four otherwise-separate choices reduce to it:

| The design choice | Why, in one sentence |
|---|---|
| a plan pins `ProcessDefinitionVersion`, never the definition | a record names the thing it was made from |
| dates are derived **once**, at instantiation | a record does not recompute itself |
| re-binding is explicit, never automatic | changing a record is an act, not a side effect |
| recalculation features are **absent, not missing** | a projection would want them; a record must not have them |

A future contributor who reads only that sentence will make the same four calls unprompted. That is the test of whether a principle was worth writing down.

#### 2. The RIM-adoption pattern, recorded as a pattern

> **RegOS adopts the RIM's questions and concepts, not necessarily its object model.**

The test, as it has actually been applied:

1. Does this object answer a real regulatory question?
2. Does RIM's modelling of it fit RegOS's architecture?
3. **If not, preserve the concept and change the shape.**

This keeps RIM as a *reference model* without treating it as executable architecture — and it is the standing answer to *"because RIM says so"*, which is not a reason here.

| Instance | RIM said | RegOS shipped | Epic |
|---|---|---|---|
| `Artwork` | its own object | a `LocalLabel` of type `ARTWORK` with dated revisions | EPIC-018 ✅ |
| `PackAuthorisation` | `License → Packaged Product`, *Single* | a dated relationship, because one authorisation covers several packs | EPIC-010b ✅ |
| `ProcessDefinition` | `Process Plan Template` | a versioned authority you conform to, not a thing you copy | EPIC-020 — **decided, not yet shipped** |

**Count it honestly: two shipped, one decided.** ADR-065 records the pattern and its three instances; **promoting it to `implementation-standards.md` waits for the EPIC-020 retro**, when the third is real. [ADR-018](../../adr/ADR-018-rule-of-three.md) asks for three *demonstrated* needs, and a decision is not yet a demonstration — which is the same discipline that kept `ProcessObjectiveGroup` out of this epic.

### Guards this epic is the first to be held to

*Named because both landed in the last two epics and neither has met new code yet.*

- **[ADR-064](../../adr/ADR-064-the-test-suite-provisions-its-own-schema.md)** — the new test assembly declares a `RegOSTestDatabase` subclass, and `TestProjectDependencyTests` fails it if it does not.
- **[`DeterministicOrderingTests`](../../../tests/Architecture/RegOS.Architecture.Tests/DeterministicOrderingTests.cs)** — every read path here (a step list is *ordered by definition*) terminates in a unique key or states the invariant that makes it total. The plan board is the most ordering-dense read RegOS has built.
- **[`ContextDependencyTests`](../../../tests/Architecture/RegOS.Architecture.Tests/ContextDependencyTests.cs)** — the graph is a specification. This is the first epic that has to add to it.
- **[ADR-043](../../adr/ADR-043-entity-identity-derives-from-the-kernel.md) / ES-020** — every id here is `sealed class <X>Id : StronglyTypedId`. Copy [`CommitmentId`](../../../src/Interaction/RegOS.Interaction.Domain/Commitments/CommitmentId.cs), **never** the Blueprint ids this epic mirrors — all of Blueprint is still legacy `record struct` and pending migration.
