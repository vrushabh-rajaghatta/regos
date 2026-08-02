# Feature Development Flow

**Status:** Active · **Effective:** 2026-07-25 · **Applies to:** every human or AI agent building a feature in RegOS

This is the **product-level** lifecycle: how a feature goes from idea to shipped, done one story at a time. It sits above [engineering-workflow.md](../engineering/engineering-workflow.md) (the per-change lifecycle) and hands off to it during implementation. Read both.

Do not skip phases. Each phase reduces uncertainty before the next. The recurring failure mode this flow exists to prevent is **design drift** — endless re-planning that ships nothing. If you find yourself re-deciding something already settled, or designing work three stories ahead, stop and return to the current phase.

---

## The flow

```
1. Plan the epic
        ↓
2. Design the domain      ← entities, columns, and a change-case (future-proofing) analysis
        ↓
3. Plan the stories       ← vertical slices, one deliverable each
        ↓
4. Implement the stories  ← one at a time, branch → PR → green → merge (per engineering-workflow.md)
        ↓
5. Retro                  ← when the epic's stories are done
        ↓
   Merge epic branch → main
```

---

## Branching & integration

`main` is always releasable and only ever receives **completed epics**.

```
main
 └── epic/EPIC-XXX-<slug>            (integration branch, cut from main at Phase 1)
      ├── feat/EPIC-XXX-S001-<slug>  (one branch per story, cut from the epic branch)
      ├── feat/EPIC-XXX-S002-<slug>
      └── ...                         → each merged (small PR) back into the epic branch when green
 ← epic branch merged into main after the retro, at epic completion
```

- **One epic = one integration branch.** Cut `epic/EPIC-XXX-<slug>` from `main` at Phase 1.
- **One story = one branch off the epic branch = one small PR back into it.** Keep stories small and independently reviewable — reviewing a whole epic in a single merge is what we avoid (engineering-workflow: *"smaller, focused changes improve review quality"*).
- **Never leave the epic branch broken.** Same green-before-next discipline, one level down. `main` is broken-proof by construction because only completed epics land on it.
- **Keep the epic branch current.** Merge `main` into the epic branch regularly so the final merge isn't a conflict cliff, and keep epics small enough to complete before they drift.
- **Complete → retro → merge epic branch into `main`** (one PR) → flip the epic to 🟢 in `BACKLOG.md`.

---

## Three registers — decision, hypothesis, constraint

*Added 2026-08-01, from EPIC-017.* Planning documents in this repo record three
different kinds of thing, and **conflating them is how a project either forgets
what it noticed or builds what it merely suspected.** Name which one you are
writing.

| Register | Says | Written as | What changes it |
|---|---|---|---|
| **Decision** | *this is how it works* | a claim in the present tense, with the argument that forced it | a superseding ADR — never an edit |
| **Hypothesis** | *this may be how it works* | the observation, **plus what evidence would confirm or falsify it**, plus a named milestone | the evidence arriving, either way |
| **Constraint** | *whatever we do, not that* | a bound on the solution space, not a solution | a decision that meets it, or an argument that it was wrong |

### Why the middle one is the hard one

Most projects handle hypotheses in one of two failure modes: they never record
them, and rediscover the same observation three epics later; or they record them
by **implementing** them, at which point the hypothesis can no longer be wrong.

The discipline is that **noticing a possible abstraction is not introducing it**
— which is [ADR-018](../adr/ADR-018-rule-of-three.md) restated as a documentation
rule. A hypothesis earns its place by being *falsifiable*: if it cannot be
written with a named milestone and a stated way of losing, it is a preference
with a citation, and it should be left out.

> **Hypotheses are expected to fail.** Recording that a hypothesis was falsified
> is a **successful** outcome if it prevented premature architecture.

Say that out loud before writing one, because the instinct is to record only the
observations you expect to be vindicated — and those are the ones least worth
recording. The value of a hypothesis is in how **cheap it is to disprove**.

EPIC-017 produced all three, and they behaved differently on purpose:

- **Decision** — *a registration names only its medicinal product* (ADR-039
  decision 1). Argued, tested, immutable.
- **Hypothesis** — *prefer storing canonical identity and projecting derived
  views over persisting convenience facts.* Deliberately **not** promoted to a
  principle; recorded in EPIC-006 §9 with four independent tests, and it either
  earns ADR-040 or disappears.
- **Constraint** — *do not build a second document store.* Survived the collapse
  of the decision it was attached to. A constraint outlives the solution it was
  first written beside; a decision that prescribes a solution does not.

### Two kinds of hypothesis — label them

*Added 2026-08-01, from EPIC-004.* Hypotheses divide by **what settles them**, and
mixing the two makes a retro hard to read. Carry a `Type` column:

| Type | Settled by | If it turns out wrong, you are |
|---|---|---|
| **Architecture** | building the model and seeing what it rejects — the evidence is in this repository | changing the design |
| **Regulatory evidence** | a real filing, a seeded blueprint, a customer artefact | **updating evidence, not architecture** |

*"`Append` is unexercised in FDA practice"* is not an architectural claim; a real
sequence proving otherwise costs an enum value, not a model. *"The snapshot is the
publication record"* is architectural; being wrong deletes an aggregate. Count
them separately at the retro, and note that only the first kind is within the
epic's power to resolve.

A regulatory-evidence hypothesis still needs its milestone, and the strongest
form names the **cost**, not the ignorance:

> **Deferred because the cost of an incorrect assumption is first paid in
> EPIC-007.**

That is a justification. *"We don't know yet"* is a shrug.

### Applying it

- **Phase 2** — when a shape recurs, ask whether you are making a decision or
  noticing one. If noticing: write the milestone that settles it, then stop.
- **Phase 5** — the retro's *Decisions to promote* section is where hypotheses go
  to be resolved. Every hypothesis the epic carried gets an outcome recorded,
  **including the ones that failed** — a hypothesis quietly dropped is
  indistinguishable from one never raised.
- **Prefer constraints to premature decisions.** *"Do not build a second X"*
  leaves Phase 2 free to widen X, share X, or invent a third thing, while still
  ruling out the outcome nobody wants. *"Reuse X"* forecloses all three, and
  usually on less evidence than it appears.

---

## Phase 1 — Plan the epic

Define the **outcome**, not the implementation. Produce a one-page epic:

- **Outcome** — one sentence: who gets what value, and how we'll know it works.
- **In scope / Out of scope** — an explicit deferred list with the *reason* for each deferral (usually YAGNI or Rule of Three). This is the drift fence.
- **Definition of Done for the epic** — the observable end state.

Output: `docs/product/epics/EPIC-XXX-<slug>.md`. Add a one-line entry to `docs/product/BACKLOG.md`.

## Phase 2 — Design the domain (entities, columns, future-proofing)

**Begin with the domain question, not the entity list.** Ask what the user is
trying to answer or accomplish — *"what does a user ask that spans a
correspondence, a meeting and an inspection?"* **"Nothing" is a valid outcome**,
and usually the one that keeps the model smallest. Only once a question
demonstrates the need for a new concept should that concept become a candidate
entity. See [ADR-038](../adr/ADR-038-organization-depth-roots-and-the-three-filter-shapes.md)
for the cost of beginning from a predicted root instead of a demonstrated query:
*a root justified by a query that does not exist yet is a demo of an empty table.*

The order is directional, which is why it matters. **A question can produce a
hypothesis; a hypothesis can only go looking for confirmation.** *"Should there
be an `X`?"* has already conceded the noun. Phase 2 still **ends** with entities
and columns — it just does not start there.

### The second question, added 2026-08-02 from EPIC-004

> **Is this concept genuinely one thing, or are we using one name for two
> facts?**

Ask it of every concept the design leans on, including the ones that arrived
from a reference model and look settled. Three consecutive stories in EPIC-004
found the same thing, each time by accident:

| | One term | Two facts |
|---|---|---|
| S001 | publication | **numbering at publication**, and transmission later |
| S002 | a document in a filing | the document, and **the publication's interpretation of it** |
| S003 | a submission's status | **our lifecycle**, and the regulatory conversation |

**Once the two are separated, the object or status that existed only to hold the
ambiguity usually disappears** — which is why those stories kept deleting
concepts instead of adding them. `SubmissionSnapshot` went; `HaStatus` was never
built; `Withdrawn` turned out to be a relationship.

**This is a question, not a pattern to satisfy.** The answer does not have to be
*two*, and a story forced into the shape would be worse than one that never
asked. It earns its place here because it is cheap to ask and the failure it
prevents — a name quietly meaning two things — is expensive to find later.

Then design the data. For each entity:

1. **Entity + columns** — name, fields, types, ownership (global vs tenant-scoped — see ADR-030/031), identity (strongly-typed id), invariants.
2. **Change-case analysis (required).** For each entity, ask *"what is likely to change about this in the next 1–3 years, and does the shape accommodate it without a breaking migration?"* Fill the table:

   | Likely future change | Probability | How the design accommodates it |
   |---|---|---|
   | e.g. more submission types added | High | reference row, not an enum |
   | e.g. tenant-specific variants | Medium | nullable `TenantId` column present from day one |
   | e.g. new standard version | Medium | versioned + effective-dated |

   Guiding rules:
   - **Design the seam now, build the feature later.** Add the column/extension point that avoids a future migration; do *not* build the workflow until a story needs it (Rule of Three, ADR-018).
   - **High-probability + expensive-to-retrofit change → accommodate now.** Low-probability or cheap-to-add-later → defer and note it.
   - Prefer reference rows over enums for anything a regulator or customer might extend.
   - Never delete governed data — model status/lifecycle and effective dating instead.

3. **ADR** — if the design makes a decision affecting bounded contexts, cross-context dependencies, ownership, or public contracts, write an ADR in `docs/adr/` before implementation (ADR only when a decision is *forced* — code is the source of truth).

Output: the entity/column design + change-case tables live in the epic file.

## Phase 3 — Plan the stories

Break the epic into **vertical slices**. Every story:

- Ships **user-visible value end-to-end** (domain → persistence → API → UI where applicable). No backend-only stories; foundational data slices that truly have no UI are verified by API/integration/seed-integrity tests and culminate in a user-visible "explorer/read" story.
- Is independently shippable and independently reviewable.
- Uses the story template below and lives as a checklist item in the epic file.

```
### STORY-XXX — <title>
As a <role>, I want <capability>, so that <value>.
Slice: domain → persistence → API → UI → test
Acceptance:
- [ ] ...
Done when: tests green · browser/integration-verified · epic branch not left broken · ADR only if forced
Branch: feat/EPIC-XXX-SYYY-<slug>  (cut from epic/EPIC-XXX-<slug>)
Status: Backlog → Ready → In Progress → In Review → Done
```

## Phase 4 — Implement the stories (one at a time)

- **One story fully to green before the next.** One story = one branch off the **epic branch** = one small PR merged back into it. Never leave the epic branch broken (see [Branching & integration](#branching--integration)).
- Follow [engineering-workflow.md](../engineering/engineering-workflow.md) and [implementation-standards.md](../engineering/implementation-standards.md) inside each story.
- Verify to the story's Done bar (automated tests + browser/integration). Tests own the data they mutate — no dependence on ambient seeded rows.
- Update the story's checkbox and status in the epic file as you go.

## Phase 5 — Retro (per epic)

When the epic's stories are all done, write a short retro: `docs/product/epics/EPIC-XXX-<slug>.md` (Retro section) or `docs/product/retros/EPIC-XXX.md`.

- **What shipped** — the observable outcome vs the Phase-1 Definition of Done.
- **What the change-case analysis got right / wrong** — did anything we deferred bite us? Did any seam we added pay off?
- **Decisions to promote** — conventions worth an ADR or a standards-doc update (lessons drive standards, not preference — see engineering-workflow "Continuous Improvement").
- **Carry-forward** — anything deferred that the next epic inherits.

After the retro, **merge the epic branch into `main`** (one PR) and flip the epic to 🟢 in `BACKLOG.md`.

---

## Artifacts & locations

| Artifact | Location |
|---|---|
| Prioritized backlog (one line per epic/feature) | `docs/product/BACKLOG.md` |
| Epic (outcome, scope, entity design, stories, retro) | `docs/product/epics/EPIC-XXX-<slug>.md` |
| Architecture decisions | `docs/adr/` (single immutable series) |
| Engineering standards | `docs/engineering/`, `docs/ENGINEERING_STANDARDS.md` |

## Roles

Founder = final say on priority, scope, product calls. Engineering (human or AI) = decisive recommendation on every decision, implementation, verification. AI is never the authoritative source of architectural decisions (engineering-workflow Principle 3).
