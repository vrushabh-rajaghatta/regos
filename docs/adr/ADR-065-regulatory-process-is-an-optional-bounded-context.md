# ADR-065 — Regulatory Process Is an Optional Bounded Context

**Status:** Accepted · **Date:** 2026-08-06 ·
**Related:** [ADR-035](ADR-035-submissions-bind-to-a-published-template-version.md) (binding to a published version; its open re-binding question),
[ADR-016](ADR-016-persistence-access-model.md) (repositories write, `RegOSDbContext` reads),
[ADR-018](ADR-018-rule-of-three.md) (why the template machinery is mirrored rather than extracted),
[ADR-031](ADR-031-tenant-isolation-by-query-filters.md) (every new entity is filtered),
[ADR-038](ADR-038-organization-depth-roots-and-the-three-filter-shapes.md) (a root justified by a query that does not exist yet),
[ADR-041](ADR-041-platform-contracts-and-the-identity-that-crosses.md) (why the step id is not put in Contracts),
[ADR-042](ADR-042-what-the-interaction-context-turned-out-to-be.md) (a commitment's fourth business origin),
[ADR-043](ADR-043-entity-identity-derives-from-the-kernel.md) (identity form),
[ADR-061 §3](ADR-061-a-pack-is-how-a-medicine-is-supplied.md) (the cycle the compiler caught),
[EPIC-020](../product/epics/EPIC-020-regulatory-process-and-planning.md) I1–I4, D1–D9

> **Amendment log.** This ADR is EPIC-020's implementation contract and the epic
> is in flight, so it is corrected in place as implementation produces better
> evidence — ADR-064's precedent, and EPIC-022 D6's before that.
> **It freezes when the epic branch merges to `main`; after that it is superseded,
> never edited.** Stating the boundary because "amended twice" and "amendable
> forever" are one careless step apart.
>
> | | Change |
> |---|---|
> | **S003** | **I5 added** (instantiation is deterministic). **The ad-hoc-plan clause struck** — the pin is `NOT NULL`, because instantiation is the only way to create a plan. |
> | **S004** | **D10 recorded** (a plan belongs to exactly one objective — settled at S003 sign-off, written down here). **D11 added** (step completion is a human decision). **I6 added** (execution history is append-only). |
> | **S005** | **I7 and I8 added** — impact analysis never repairs the schedule, and never replaces it. |
> | **S006** | **I9 added** — attachments are descriptive, not constitutive. |
> | **S007** | **Nothing added.** Four more aggregates took the same shape and none of them needed a clause. Recorded because a repetition that required no amendment is the strongest evidence this document is finished. |
> | **S008** | **Nothing added — and every invariant now names a test.** The capstone audited the nine against the suite rather than against intent, and found **I1, I3 and I7 asserted nowhere**. All three were absence-shaped: true because nobody had written the code that would break them. The evidence table is in [EPIC-020](../product/epics/EPIC-020-regulatory-process-and-planning.md#the-evidence-table). |
>
> No amendment has touched a decision code already depends on.
>
> **From S008, this document is falsifiable.** Each invariant above cites
> executable evidence, so *"does I5 still hold?"* is answered by running a test
> rather than by reading this page. That is the boundary an ADR crosses when it
> stops being primarily a design document and becomes a description of
> architecture already demonstrated in running code.
>
> **The three inbound edges D2 authorises are now spent** — `Registration` and
> `Submission` at S006, `Interaction` at S007. A fourth context wanting a
> `ProcessStepId` is a new decision requiring a new ADR, not a remaining
> allowance under this one. `ContextDependencyTests` is what enforces that.
>
> **This ADR is the implementation contract for EPIC-020, not a summary of it.**
> It deliberately says nothing about story order, screens or sequencing, so that
> re-cutting S002 or S004 leaves it untouched. Where it and the epic plan
> disagree, this document wins; where the code and this document disagree, **the
> code wins and this document is superseded in the same PR.**

## Context

RegOS records **what happened**: applications filed, submissions transmitted, letters received, meetings held, commitments given, inspections closed, registrations held. It holds nothing that says **what we are trying to achieve, what we intend to do about it, and by when.** That layer lives today in a spreadsheet beside RegOS, where it drifts from the records it is supposed to describe.

**Six questions have no home in any existing bounded context:**

| | The question | Why no existing context answers it |
|---|---|---|
| Q1 | *"What do we have to do to file this, and in what order?"* | a blueprint says what a **dossier** must contain, never what a **team** must do |
| Q2 | *"What are we trying to achieve here, and why this route?"* | `RegulatoryApplication` is the vehicle; nothing holds the goal or the strategy |
| Q3 | *"Where are we, and what is next?"* | every context knows its own records; none knows the sequence they belong to |
| Q4 | *"What is late, and what does it block?"* | dependency between activities is modelled nowhere |
| Q5 | *"This submission — what plan is it part of?"* | the link does not exist |
| Q6 | *"We did this last year. Do it again."* | there is nothing to repeat *from* |

**Why now.** This capability connects objects rather than creating them, so building it early would have produced a planner with nothing to plan. Its three dependencies — **EPIC-004** (submissions and sequences), **EPIC-006** (meetings, correspondence, commitments, inspections) and **EPIC-017** (the market-local product tier) — have all shipped. *"Deliberately last"* described the ordering of its inputs, and its inputs are complete.

**Why it needs an ADR at all.** It introduces a **bounded context**, and — as [`ContextDependencyTests`](../../tests/Architecture/RegOS.Architecture.Tests/ContextDependencyTests.cs) made executable one epic ago — the dependency graph is a specification that changes only with an ADR. This is the first epic held to that.

## Decision

### 1. The template machinery is **mirrored, not extracted** *(D1)*

`RegulatoryTemplate → RegulatoryTemplateVersion → TemplateSection` and `ProcessDefinition → ProcessDefinitionVersion → ProcessStepDefinition` are the **second** occurrence of *versioned · scoped · published · frozen · instantiated*. They stay separate.

[ADR-018](ADR-018-rule-of-three.md) asks for three *demonstrated* needs, and the payloads differ more than the lifecycle matches — a section tree carrying eCTD folders and ICH elements against a step graph carrying predecessors and day offsets. **EPIC-006 measured a near-identical extraction candidate at six occurrences and still refused it**, because the shapes never converged; refusing at two requires less argument than that did.

**Recorded so the third occurrence has something to measure against: the shared surface is a version number, a `Draft → Published → Deprecated` status, effective dates, and a one-open-draft rule.** If a third versioned-published artefact arrives and the shared surface is still those four things, extract then.

### 2. Integration is by **nullable inward edges**, and the context is optional *(D2)*

The six artefacts that participate in a plan each carry a **nullable `ProcessStepId`**. `ProcessStep` holds no foreign key to any of them.

| | Chosen: artefact holds the step id | Rejected: step holds artefact ids |
|---|---|---|
| Cardinality | many artefacts → one step, natural | one of each kind per step, or a join table |
| Integrity | one typed FK per artefact | typed FKs, or a polymorphic `(Kind, Id)` pair — **[ADR-042](ADR-042-what-the-interaction-context-turned-out-to-be.md) refused one of those by name** |
| Direction | Process is a **consumer** of the regulatory domain | Process becomes its **hub**, which is backwards |

`Commitment` already carries five typed nullable foreign keys for exactly this reason, and the polymorphic alternative was argued and rejected there.

**Refused explicitly: putting `ProcessStepId` in `Platform.Contracts` to avoid the edges.** [ADR-041](ADR-041-platform-contracts-and-the-identity-that-crosses.md) holds that assembly to two types — *"three is a second kernel"*.

### 3. `ProcessObjective` and `ProcessPlan` are separate aggregates *(D3)*

They are 1:1 today, and an undemonstrated 1:1 would normally be one object. The demonstration is that **an objective is stateable with no schedule under it at all**: *FDA approval for Product X · CE MDR transition · expand an indication · renew an existing licence*. Each is nameable, ownable and reportable before anyone has scheduled anything.

**An objective is the goal; plans are attempts.** The objective carries what we intend and why — strategy, decisions, rationale, references — and survives a plan being abandoned and re-made. The plan carries what we will do and when.

**The cost asymmetry is why this is not left to be discovered:** collapsing two objects loses the distinction *and* the data that recorded it; splitting one later has to invent both. Deleting an object is cheaper than resurrecting one.

### 3a. A plan belongs to exactly one objective *(D10)*

`ProcessPlan.ProcessObjectiveId` is **required**. *Settled at S003 sign-off, recorded here at S004.*

**Not for referential integrity — for meaning.** A plan answers *how are we going to achieve something?* If nothing answers *what are we trying to achieve?*, it is not a regulatory plan. **It is a schedule, and RegOS is not a project-management tool.**

The deletion test both ways:

| Delete | Lost | Kept |
|---|---|---|
| the **plan** | schedule, dated steps, dependency graph | intended outcome, market, rationale, ownership |
| the **objective** | what the schedule is *for* | dates and steps — **and they no longer mean anything** |

**A plan that still made complete sense without its objective would be evidence that D3 had collapsed.** It does not, so it has not.

**Ad-hoc planning does not need objectiveless plans; it needs lightweight objectives.** *"Sketch a plan for what?"* — even *"evaluate filing in Japan"* is an objective, and it may stay `Proposed` forever.

### 4. Completion is a business decision, never a consequence *(D11)*

> **Step and plan completion are explicit business decisions recorded by a user. External lifecycle events may provide suggestions or readiness indicators, but never perform lifecycle transitions.**

That wording is deliberately wider than *"completion is manual"*, because the erosion never arrives calling itself automation. It arrives as *"the step is obviously done, why make them click?"* — and the sentence above answers every version of that question the same way, including the ones nobody has thought of yet.

**Three things it forecloses at once**, which is why it is stated as one rule rather than three:

| A submission reaches `Transmitted` | may **suggest** | may not complete |
| A meeting is recorded as held | may **suggest** | may not complete |
| Every predecessor completes | may show **ready** | may not complete |

**It also covers the plan**, not only the step — a plan does not complete because its last step did. *No further execution is expected* is a judgement, and steps may legitimately be `Skipped` rather than `Complete`.

**Because the two answer different questions.** A submission reaching `Transmitted` answers *what happened to the submission?*; a step completing answers *do we consider this part of the plan done?* Those legitimately diverge — the submission went out while the team is still checking the step's other obligations; the meeting happened but its follow-ups have not; **a CMC package is complete with no RegOS entity representing it at all.**

**And the architectural danger is not the read — it is the source of truth.** RegOS composes across contexts freely (ADR-039 §7). What derived completion changes is ownership:

```
   today                      derived completion
   ─────                      ──────────────────
   Submission                 Submission
       │ annotation               │ owns
       ▼                          ▼
   ProcessStep                ProcessStep
```

That violates [I2](#i2--process-never-owns-the-lifecycle-of-an-entity-outside-process) in the direction nobody notices, and **the next request follows from it rather than from bad judgement**: *"if the submission completes the step, why doesn't completing the step publish the submission?"*

**A rule that worked for submissions and not for CMC packages would not be a rule.** It would be a special case wearing one.

**Permitted, and expected once S006 exists: *suggested* completion.** A step may **show** that its linked submission has been transmitted. That is a read model getting richer while the domain model stays still — the honest half of the derived idea.

**Refused outright: completing a step because its predecessors completed.** That is a scheduler, and it is on this ADR's [Out of scope](#out-of-scope--refused-not-deferred) list.

### 5. Instantiation derives dates **once** *(D5)*

A `ProcessStepDefinition` carries its predecessors and an offset in days. Instantiating a plan walks that graph topologically from a single anchor date and writes planned dates **once**. From that moment a human owns them: **moving one step moves nothing else.**

**A cycle in the predecessor graph is rejected at publish, not at instantiation** — the definition is the thing that is wrong, and it must not be publishable in that state.

### 5. A superseded definition changes nothing, and says so *(D6)*

Publishing a new `ProcessDefinitionVersion` has **no effect on any existing plan**. What is new is **disclosure**: a plan whose pinned version has been superseded shows that it has been, derived on read and stored nowhere.

This is [ADR-035 §1](ADR-035-submissions-bind-to-a-published-template-version.md) reached from the other side. That ADR lists *"re-binding is unsolved"* among its trade-offs; **the disclosure is the half that was missing, and migrating a live plan is the half that stays deferred** — now with a comprehensible record underneath it (*"this plan came from v2; here is what v3 changed"*) rather than an archaeology exercise.

### 6. **Late** and **due** are two facts *(D7)*

| | **Due** | **Late** |
|---|---|---|
| Obligation | regulatory | internal execution |
| Who cares | the agency | the company |
| Missing it | compliance implications | affects forecasting |

[`ListDueWork`](../../src/Interaction/RegOS.Interaction.Application/Queries/ListDueWork/ListDueWorkHandler.cs) answers the first and is **not modified, extended or wrapped**. The plan's late view is a sibling that **shares no code with it**.

**They may share rendering later; they may never share behaviour.** A shared row component is a UI convenience. A shared query is a claim that these are one kind of thing, and they are not. A future unified *"my work"* surface **composes** both and replaces neither.

### 7. Terminology — `ProcessDefinition` in the model, **Playbook** on screen *(D9)*

> The authoritative, versioned description of a regulatory process is named **`ProcessDefinition`** in the domain model and **"Playbook"** in the user interface. A `ProcessDefinition` is immutable once published and is the artefact to which `ProcessPlan`s are permanently bound.

RIM's own word is *Process Plan Template*, and it is not used. **A template is a thing you copy and edit; a definition is a thing you conform to** — and invariant I4 designs the second. Naming it a template invites precisely the mental model I4 forbids, and the invitation strengthens as behaviour accumulates: as governance arrives, *Published Definition* and *Superseded Definition* are what regulated systems call such artefacts, while *Approved Template* reads as a mistake.

**The prefix and the role are both kept** — never bare `Process`:

| What we conform to | What we are doing |
|---|---|
| `ProcessDefinition` · `ProcessDefinitionVersion` · `ProcessStepDefinition` | `ProcessObjective` · `ProcessPlan` · `ProcessStep` |

The pair is recorded in [`docs/domain-model/process.md`](../domain-model/), per CLAUDE.md: **the screen's word must never reach a type, nor the type's word a label.**

## Architectural invariants

**These must remain true for as long as this context exists.** A change that violates one is not a bug to be fixed afterwards — it is a change that requires this ADR to be superseded first.

### I1 — Regulatory Process is optional

> **Existing regulatory workflows remain valid when no process plan exists. Every integration is nullable and additive.**

RegOS with an empty `Process` schema behaves, on every existing screen, exactly as it does without this context. This replaces the weaker and untrue claim that Process is *deletable*: five contexts reference it, so it is not — but nothing *requires* it, and that is the property worth holding.

### I2 — Process never owns the lifecycle of an entity outside Process

> **The links are annotations, not ownership.**

A step may record that a submission belongs to it. It may never create, transition, publish, withdraw or delete one. Decision 2's edge direction makes this structural — the step holds no handle to act through — and the rule is stated anyway, because the failure it forbids arrives as a convenience: *"mark step complete → publish the submission"*, which is a second lifecycle for a regulated record.

### I3 — Each context owns itself and *optionally* references Process

```
   Submission ────┐
   HaMeeting ─────┤
   HaCorrespondence┼──► ProcessStep        each context owns itself
   Commitment ────┤                        each optionally references Process
   Inspection ────┤                        nothing requires Process
   Registration ──┘
```

### I4 — A plan is permanently bound to a published definition version

> **A `ProcessDefinitionVersion` is immutable once published. A `ProcessPlan` pins the version it was instantiated from, forever. No plan is ever automatically rebased, and no plan may point at "latest".**

The two halves reinforce each other: immutability means a version cannot be edited under a plan already running against it; permanent pinning means *"why did this milestone move?"* always has an answer, and the answer is never *"the playbook changed under us."*

### I5 — Instantiation is deterministic

> **Given the same published `ProcessDefinitionVersion`, the same objective and the same anchor date, `InstantiateFrom` always produces the same plan** — the same steps, the same dates, the same predecessor graph. No runtime state, no wall clock, no database ordering and no randomness may influence the result.

*Added at S003, 2026-08-06 — the first story that creates plans.*

**This is I4, publication's certificate and EPIC-024's ordering rule meeting in one method.** Each is necessary and none is sufficient:

| Without it | What breaks |
|---|---|
| the pinned, frozen version (I4) | the *input* changes under a rerun |
| publication's DAG certificate | the walk has no guaranteed order to find |
| deterministic ordering | two runs schedule the same tie differently |

**The test is a rerun, not an inspection.** Instantiating twice from the same three inputs must produce two plans that differ only in their ids — which is checkable, and is the form the S003 tests take.

**What it forbids, concretely:** reading `DateTime.UtcNow` inside the derivation, ordering steps by whatever the database returned, resolving "the current version" instead of taking one, or letting an already-created plan influence the next. **Each of those is a plausible convenience**, which is why the invariant is stated rather than assumed.

**It is what makes *"why is this milestone on this date?"* answerable at all** — the question I4 exists to protect. A pinned version and a stated anchor are only an explanation if the same pair always yields the same answer.

### I6 — Execution history is append-only

> **A step's and a plan's execution history records facts. A correction is a new dated entry, never a rewrite of an old one.** Business dates are supplied by the person recording them; the system clock records only when RegOS learned.

*Added at S004, 2026-08-06 — the first story where a plan has a past.*

**Because execution state is the part most likely to be treated as ordinary CRUD.** Planned dates are obviously a record; *"mark this step complete"* feels like a checkbox, and a checkbox is a thing you untick. The invariant says what actually happens: unticking is **another dated fact**, and both survive.

| | Kept | Why it matters |
|---|---|---|
| the entry that was wrong | ✅ | *"we thought this was done on the 3rd"* is itself regulatory history |
| the correction | ✅ | with its own business date and its own `RecordedOnUtc` |
| **who believed what, when** | ✅ | the two clocks ADR-037 established, doing the job they exist for |

**This is the eighth dated history in RegOS and the pattern is now load-bearing** — [ADR-037](ADR-037-registrations-are-regulatory-assets-with-derived-visibility.md) established the two clocks, ADR-046 §6 shared their mapping, and every context that records *what happened* has followed. **Execution is not the place to break it for convenience.**

**What it does not forbid:** editing a step's *planned* dates. A schedule is a statement of intent that a human owns and may simply change (D5). The distinction is that **planned dates are a current value; execution is a sequence of events** — and only the second is history.

### I7 — Impact analysis never repairs the schedule

> **Impact analysis is derived entirely from the current dependency graph and execution state. It never mutates the plan and never proposes an alternative schedule.**

*Added at S005.* It answers ***"if nothing changes…"*** and only that. **The moment it suggests what to move, it has become a scheduler** — which this ADR's [Out of scope](#out-of-scope--refused-not-deferred) list already refuses, and which is a different bounded context wearing a helpful face.

### I8 — Impact analysis is observational

> **Impact analysis derives hypothetical consequences from the current state of a plan. It never creates, modifies or persists scheduling information, and never becomes the authoritative schedule.**

*Added at S005.* **I7 says do not repair the schedule; I8 says do not replace it.** Those protect different things, which is why they are two invariants rather than one carefully worded one.

**The projection is ephemeral by construction:**

```
   the plan            unchanged, on disk, authoritative
       │
       ├─► read request ─► projection computed ─► returned ─► discarded
       │
   the plan            still unchanged
```

**Nothing is written, nothing is cached as a new schedule, and re-running it on the same inputs gives the same answer** — which is I5's property applied to a read. *The forecast is not the ledger.*

**And it is why the projection may recalculate at all.** Recalculation is forbidden as a *stored* thing (D5, I4): a playbook change must never move a milestone. Deriving *what today's facts imply* breaks neither, because the plan's own dates never change and the answer is labelled as a projection everywhere it appears.

### I9 — Attachments are descriptive

> **Attaching a regulatory artefact to a `ProcessStep` records that the artefact contributes to execution of that step. The attachment changes neither the artefact's meaning nor the plan's execution. Adding or removing one changes discoverability only.**

*Added at S006, the first story where anything attaches.*

**The drift it forbids is a sentence, not a feature:** *"this submission is not attached, therefore it is not part of the plan."* **No** — it may simply not be annotated. **An attachment is descriptive, not constitutive**, and the distinction matters most exactly when the data is incomplete, which is always.

| | |
|---|---|
| an attachment **may** | make a record discoverable from the step, and the step from the record |
| an attachment **may not** | complete a step (D11), gate a transition, change what the artefact means, or make its absence mean anything |

**It is what keeps I1 true once integration exists.** If an unattached submission were *less valid*, Process would have stopped being optional — it would have become a thing every context had to satisfy.

## Architectural consequences — the properties a change must preserve

**These are not decisions.** Each falls out of the invariants above, and they are stated separately so that a reviewer can ask *"does this change preserve the consequences?"* without re-opening the decisions.

| Consequence | Falls out of |
|---|---|
| Existing bounded contexts remain independently usable | I1 |
| Process owns no external lifecycle | I2 · D2 |
| Published definitions are immutable | I4 · D9 |
| **Plans are historical records, not projections** | I4 |
| Existing "due work" semantics remain unchanged | D7 |
| **No lifecycle transition happens as a consequence of another context** | I2 · D11 |
| **No execution fact is ever overwritten** | I6 |
| **No analysis ever becomes the schedule** | I7 · I8 |
| **An absent attachment means nothing** | I1 · I9 |

### The fourth is the one the others reduce to

> **Plans are historical records, not projections.**

A plan is not a live view over its definition. It is a dated record of what was scheduled, from which version, on which day. Four otherwise-separate design choices follow from that single property:

| Design choice | Because |
|---|---|
| a plan pins `ProcessDefinitionVersion`, never `ProcessDefinition` | a record names the thing it was made from |
| dates are derived **once**, at instantiation | a record does not recompute itself |
| re-binding is explicit, never automatic | changing a record is an act, not a side effect |
| recalculation features are **absent, not missing** | a projection would want them; a record must not have them |

A contributor who reads only that sentence should make the same four calls unprompted. That is the test of whether it was worth writing down.

## Bounded-context relationships

**The context is `src/Process/RegOS.Process.{Domain,Application,Infrastructure}`.**

### Edges this ADR authorises

| Direction | Edge | For |
|---|---|---|
| **outbound** | `Process.Domain → ReferenceData.Domain` | country, authority, application type — the scope of a definition |
| **outbound** | `Process.Domain → Product.Domain` | the objective's target product |
| **outbound** | `Process.Domain → RegulatoryApplication.Domain` | the objective's nullable vehicle *(decision 3)* |
| **inbound** | `Submission.Domain → Process.Domain` | the nullable step id |
| **inbound** | `Interaction.Domain → Process.Domain` | the nullable step id, on four aggregates |
| **inbound** | `Registration.Domain → Process.Domain` | the nullable step id |

**No edge beyond this list is authorised.** Adding one amends this ADR first, which is what `ContextDependencyTests` already requires and now enforces for this context too.

**These edges close no cycle**, and that is checked rather than asserted: `Process` reaches only `ReferenceData`, `Product` and `RegulatoryApplication`, none of which reaches `Process`. [ADR-061 §3](ADR-061-a-pack-is-how-a-medicine-is-supplied.md) is what happens when a direction is assumed instead — there the compiler caught it, and a compiler catches cycles but never directions.

### Ownership rules

1. **The artefact holds the foreign key; the step never does.** *(I3)*
2. **Process reads other contexts; it never mutates them.** No repository outside `Process.Domain` is written to by Process code, and no aggregate outside Process is loaded for modification by it. *(I2)*
3. **A cross-context read composes over `RegOSDbContext`** with `AsNoTracking()`, per [ADR-016](ADR-016-persistence-access-model.md). A Process query handler never loads a foreign aggregate through its repository.
4. **A `ProcessStepId` on a foreign aggregate is set and cleared by that aggregate's own behaviour**, not by Process. Attaching work to a step is the artefact's decision to record; the step is told nothing.

## The versioning model

```
   ProcessDefinition                    identity of the playbook; owns its versions,
        │                               assigns their numbers, permits one open draft
        ├── ProcessDefinitionVersion    Draft → Published → Superseded
        │        │                      IMMUTABLE once published
        │        └── ProcessStepDefinition   predecessors · parent/child · offset days
        │
        ▼  instantiate  (dates derived once)
   ProcessPlan ──── pins ────► one ProcessDefinitionVersion, permanently
        │
        └── ProcessStep         planned/actual dates · dated status history
                                predecessors · the step definition it came from
```

**The pin targets the *version*, not the definition** — the same deliberate exception [ADR-035 §2](ADR-035-submissions-bind-to-a-published-template-version.md) makes for submissions, and for the same reason: **the version is the governance artefact**, so pointing at the root would leave *"which version?"* unanswered at every point that matters. The database foreign key is `Restrict`; a version a plan was scheduled from can never be deleted.

~~**A plan with no definition is legitimate.** An ad-hoc plan pins nothing, exactly as [ADR-035 §4](ADR-035-submissions-bind-to-a-published-template-version.md) allows an unbound submission: **missing reference data must never block the business.**~~

> **Amended at S003, 2026-08-06 — the pin is required.** `ProcessPlan.ProcessDefinitionVersionId` is **`NOT NULL`**, because instantiation is the only way to create a plan and every plan therefore has one. The clause above described a capability nobody has built, and carrying a nullable column for it would be the speculative seam this ADR refuses elsewhere — the same discipline that kept `ProcessObjectiveGroup` out, left five authorised edges undeclared, and deleted a duplicated DTO field.
>
> **If ad-hoc plans become a real requirement, the migration is `ALTER COLUMN … DROP NOT NULL`** — and ADR-035 §4's argument is waiting for it, unchanged. **Nothing about the reasoning was wrong; it was answering a question nobody had asked yet.**

## RIM adoption

> **RegOS adopts the RIM's questions and concepts, not necessarily its object model.**

The test, as it has actually been applied:

1. Does this object answer a real regulatory question?
2. Does RIM's modelling of it fit RegOS's architecture?
3. **If not, preserve the concept and change the shape.**

This keeps RIM a **reference model** without treating it as **executable architecture**, and it is the standing answer to *"because RIM says so"*, which is not a reason in this project.

| Instance | RIM said | RegOS holds | Status |
|---|---|---|---|
| `Artwork` | its own object | a `LocalLabel` of type `ARTWORK` with dated revisions | **shipped** (EPIC-018) |
| `PackAuthorisation` | `License → Packaged Product`, *Single* | a dated relationship, because one authorisation covers several packs | **shipped** (EPIC-010b) |
| `ProcessDefinition` | `Process Plan Template` | a versioned authority you conform to, not a thing you copy | **decided, implementation pending** |

**Two shipped, one decided.** The pattern is recorded here; **promoting it to [`implementation-standards.md`](../engineering/implementation-standards.md) waits until the third instance is real** — ADR-018 asks for three *demonstrated* needs, and a decision is not a demonstration. That is the same discipline applied to the architecture that it is applied to the objects.

**This ADR also refuses one RIM object outright.** `Process Objective Group` — a programme spanning markets — answers a question nobody asks, and RegOS holds no product with objectives in two markets. Building it would be [ADR-038](ADR-038-organization-depth-roots-and-the-three-filter-shapes.md)'s *"a root justified by a query that does not exist yet is a demo of an empty table"*. **Revisit when a second objective exists for one product in a second market, and somebody asks how the programme is going.**

## Out of scope — refused, not deferred

**This ADR is defined as much by what it refuses as by what it decides.** Each item below is architecturally excluded. **Introducing any of them requires amending or superseding this ADR first**; none may arrive as an implementation detail inside a story.

| Refused | Which invariant it would break |
|---|---|
| **Automatic rebasing of existing plans** onto a newer definition version | I4 |
| **Recalculation of instantiated dates** — critical path, lead/lag, calendars, resource levelling | I4, and the consequence that plans are records |
| **Process owning the lifecycle of an external entity** — any create, transition, publish or delete outside Process | I2 |
| **`ProcessObjectiveGroup`** and cross-market programme rollups | none — refused on ADR-038, and revisited by evidence rather than by amendment |
| **A unified work queue replacing `ListDueWork`** | D7 — composition is permitted, replacement is not |
| **Editing a published `ProcessDefinitionVersion`** | I4 |

Each of these arrives sounding like a convenience. That is precisely why the bar is an amendment rather than a code review.

## Deferred decisions

*Open, and expected to be settled against real code rather than by argument.*

| | Deferred | Settled by |
|---|---|---|
| **D4** | the final shape of the Process projects' internal structure, and confirmation that the authorised edge list above is sufficient | S001 wiring the first slice; **an edge beyond the list amends this ADR** |
| **D8** | whether a `ProcessObjective` targets `GlobalProductId + CountryId` or the market-local `MedicinalProduct`. Lean: the former, since an objective routinely precedes the market-local product, with a nullable `MedicinalProductId` seam | S002, on a real objective |
| **live re-binding** | migrating an in-flight plan — or submission — to a newer version | ADR-035's own open question; **answer once, for both, when someone needs it** |

## Consequences

**Benefits**

- **A capability that can be ignored.** No existing screen, query or workflow changes behaviour when no plan exists — a property most "spine" layers cannot claim.
- **Auditability by construction.** *"Why is this milestone on this date?"* resolves to a version number and an anchor date, with no recalculation history to reconstruct.
- **The graph stays acyclic and stated.** Six edges, authorised here, enforced by a test written one epic ago.
- **The next artefact type is cheap.** It adds a nullable column and touches no Process code.
- **RIM's questions without RIM's object model**, now as a recorded pattern rather than three separate judgement calls.

**Trade-offs we are consciously accepting**

- **Five contexts gain a dependency on Process.** It is optional at runtime and structural at compile time; the context cannot be removed without touching all five.
- **Six migrations on shipped tables.** Cheap only because RegOS is pre-customer, and this is the window.
- **Dates go stale.** Moving one step moves nothing downstream, so a plan can misrepresent its own schedule until a human corrects it. **This is I4 working, not a defect** — and it is the trade-off most likely to be argued against later.
- **Two objects in a 1:1** until an objective demonstrably holds two plans.
- **The second versioned-template implementation**, knowingly duplicated, with the shared surface recorded so the third occurrence can be measured rather than argued.

## Implementation notes

*Not architecture. Recorded here because each is a guard this context is the first new code to meet.*

- **S001 records the terminology pair** in `docs/domain-model/process.md`, because S001 names the first file and [SC-005](../engineering/slice-conventions.md) makes the name structural.
- **[`ContextDependencyTests`](../../tests/Architecture/RegOS.Architecture.Tests/ContextDependencyTests.cs)** gains a `Process` entry in `DomainMayReference`, plus `Process` on the three inbound contexts' lists. Its negative control asserts one entry per domain project, so the entry is not optional.
- **[`DeterministicOrderingTests`](../../tests/Architecture/RegOS.Architecture.Tests/DeterministicOrderingTests.cs)** — a plan board is the most ordering-dense read in RegOS. Every ordering terminates in a unique key or carries a `// Deterministic:` comment stating the invariant that makes it total.
- **[ADR-064](ADR-064-the-test-suite-provisions-its-own-schema.md)** — the new test assembly declares a `RegOSTestDatabase` subclass; `TestProjectDependencyTests` fails it if it does not.
- **[ADR-043](ADR-043-entity-identity-derives-from-the-kernel.md) / ES-020** — every id here is `sealed class <X>Id : StronglyTypedId`. Copy [`CommitmentId`](../../src/Interaction/RegOS.Interaction.Domain/Commitments/CommitmentId.cs); **never** the Blueprint ids this context mirrors, which are legacy `record struct` and pending migration.
- **[ADR-031](ADR-031-tenant-isolation-by-query-filters.md)** — every entity carrying a `TenantId` gets a fail-closed query filter, or `TenantFilterArchitectureTests` fails.
- **[ADR-042 decision 2](ADR-042-what-the-interaction-context-turned-out-to-be.md)** — adding `ProcessStepId` to `Commitment` **does not** trip the fourth-business-origin rule. That rule fires on a fourth place a commitment *arose*; a step is what a commitment *serves*.
