# EPIC-020 — Regulatory process & planning

**Status:** ⚪ Not Started · **Branch:** `epic/EPIC-020-regulatory-process-and-planning` (cut at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

RIM's **spine**. Everything RegOS builds — applications, submissions, correspondence, meetings, commitments, inspections — becomes a *step in a plan* that serves an *objective*. This is the layer that turns a record system into a system that tells you what to do next.

> **Phase 1 below is settled.** **Phases 2–3 are a sketch**, written so this epic can be picked up months from now without re-deriving it — they are **not approved design**. Confirm, amend or replace them in the Phase-2 conversation when this epic is pulled into **Now**.

---

## Phase 1 — Epic plan

### Outcome
A regulatory user states an **objective** (*"approve Product X in Japan"*), instantiates a **plan** from a country-specific **template**, and sees dated **steps** with dependencies — where each submission, meeting, correspondence, commitment and inspection is attached to the step that produced it. *"What's next, what's late, and what does it block?"* becomes answerable.

### The concepts it introduces

| | Means | RIM object |
|---|---|---|
| **Process Objective Group** | a programme spanning markets | Process Objective Group (3) |
| **Process Objective** | what we are trying to achieve, in one market | Process Objective (23) |
| **Process Plan Template** | the reusable, country-specific playbook | Process Plan Template (11) |
| **Process Step Template** | one step of that playbook, with its predecessors | Process Step Template (12) |
| **Process Plan** | a live plan, instantiated from a template version | Process Plan (16) |
| **Process Step** | a live, dated step with predecessors and successors | Process Step (22) |

### Why it goes last

It is the thing that **connects** the other objects. Build it before applications, submissions, meetings, correspondence, commitments and inspections exist and you have built a planner with nothing to plan. Build it after **EPIC-004** and **EPIC-006** and it wires up things that are already real.

### Depends on
- **EPIC-004** — submissions attach to steps (RIM: `Submission → Process Step`, and `Submission → Process Objective` Parent).
- **EPIC-006** — meetings, correspondence, commitments and inspections all anchor on `Process Step`. Those epics are told to leave a **nullable `ProcessStepId` seam**; this epic fills it.
- **EPIC-017** — objectives target a product in a market.

### The precedent to reuse *(the most useful thing in this file)*

**`Process Plan Template` / `Process Step Template` → `Process Plan` / `Process Step` is structurally the same pattern as `RegulatoryTemplate` / `TemplateSection` → a bound `Submission`.** Both are:

- versioned with a status and effective dates (RIM marks template version/status/status-date "Single / Historical"),
- country- and category-scoped,
- published/frozen before use,
- **instantiated** into a live object that pins the template version it came from (RIM's `Process Plan` carries `Process Plan Template Version` explicitly — exactly [ADR-035](../../adr/ADR-035-submissions-bind-to-a-published-template-version.md)'s binding).

EPIC-001 and EPIC-002 already solved draft → publish → freeze → bind → instantiate. **Do not re-invent it here; extend or mirror it.** If the shapes converge enough, this is the third occurrence and triggers extraction of a shared versioned-template abstraction — not a fourth.

### In scope ✅
- **`ProcessObjective`** (+ `ProcessObjectiveGroup`) — name, description, type, dated status, planned/actual start and end, product, country, strategy type + decisions + details + references, optional clinical trial and application links.
- **`ProcessPlanTemplate`** / **`ProcessStepTemplate`** — versioned, published, country- and product-category-scoped, with predecessor/successor and parent/child step structure.
- **`ProcessPlan`** — instantiated from a pinned template version; name, type, category, countries, dated status, planned/actual dates.
- **`ProcessStep`** — dated, with predecessor/successor and parent/child, dated status, template-version provenance.
- **Wiring the seams** — attach `Submission`, `HaMeeting`, `HaCorrespondence`, `Commitment`, `Inspection` and `Registration` to their steps.
- **The timeline view** — a plan's steps, what is late, and what it blocks.
- Browser proof, ADR.

### Out of scope ⏸️ (deferred, with reason)
| Deferred | Why |
|---|---|
| **Automatic date recalculation / critical-path scheduling** | Real project-management scheduling (lead/lag, calendars, resource levelling) is a product of its own. Model predecessors and dates; let a human move them. Revisit only with a concrete request. |
| **Resource assignment and capacity planning** | Not in RIM's process objects, and squarely a PPM concern. |
| **Gantt rendering** | A view concern → **EPIC-011**. This epic delivers the data. |
| **Notifications on slipping steps** | → **EPIC-014**, consistent with EPIC-005 and EPIC-006. |
| **Template authoring UI** | → **EPIC-012**, which owns reference-data authoring across the board. Seed templates here. |
| **Retro-fitting plans onto historical applications** | Plans are forward-looking. Nothing forces existing records into a plan. |

### Definition of Done
- A process plan template can be seeded, versioned and published for a country + product category, with steps carrying predecessors.
- An objective can be created for a product in a market, and a plan instantiated from a **pinned published template version** — the ADR-035 binding pattern.
- Steps carry planned and actual start/end dates and a dated status history.
- A submission, meeting, correspondence, commitment, inspection and registration can each be attached to a step, and the step shows them.
- *"What is late, and what does it block?"* is answerable from the plan, derived on read.
- A plan whose template version was superseded keeps working — the pin is the point.
- Browser proof: seed a template → create an objective → instantiate a plan → complete a step → attach a submission to the next one → see the timeline.
- ADR written for the plan/template instantiation model (or an amendment to ADR-035 if the abstraction is extracted).

---

## Phase 2 — Domain design *(sketch — not approved)*

### Decisions to settle (Phase 2, on pull-in)

**1. Extend the existing template machinery, or mirror it?** The choice: (a) generalise `RegulatoryTemplate` into a versioned-template abstraction both dossier blueprints and process plans use; (b) build a parallel `ProcessPlanTemplate` with the same shape. *Lean: (b) first, then extract* — premature generalisation across two very different payloads (sections vs steps) is a bigger risk than duplication, and the Rule-of-Three note in `RegistrationCreationPolicy` sets the precedent for when to extract. **Record whichever, with the reasoning.**

**2. Where does the process context live?** New `src/Process/`. It depends on almost everything, and almost nothing should depend on it — **the seams point inward** (other aggregates hold a nullable `ProcessStepId`; the step does not hold FKs to all six). That direction is the important call: it keeps Process optional and deletable.

**3. Step ↔ artifact linkage direction.** *Lean: the artifact holds the nullable `ProcessStepId`*, per (2). RIM draws these as peer links in both directions; pick one and be consistent.

**4. Objective vs Application.** RIM has `Process Objective → Application` (Peer, Conditional). An objective is *"get approved in Japan"*; the application is the vehicle. Keep them distinct — one objective may span several applications over time.

**5. Dated status history on plan, step and objective.** All three are "Single / Historical" in RIM. By this epic the shared history shape should already exist (EPIC-017 / EPIC-006); reuse it.

**6. What happens to a live plan when its template publishes a new version?** Exactly the question EPIC-002 carried forward unresolved for submissions. **Answer it here for both** — it has been deferred twice.

### Change-case analysis

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| Gantt / timeline visualisation (EPIC-011) | **High** | Dates + dependencies are stored; rendering is a read concern |
| Templates differ per authority as well as per country | **High** | RIM scopes templates by country + product category; add authority as a third scope dimension from day one |
| Step slippage notifications (EPIC-014) | High | Planned vs actual dates are stored; a scheduler reads them |
| A new artifact type needs attaching to steps | High | Seams point inward — the new artifact adds a nullable FK; Process is untouched |
| Sub-plans / nested programmes | Medium | `ProcessObjectiveGroup` and parent/child steps are both in RIM's shape |
| Template version supersedes a live plan | Medium | Pinned version (decision 6) |
| Plans without templates (ad-hoc) | Medium | Template link is nullable in RIM — mirror that; ADR-035's *"missing upstream data must never block the business"* |

---

## Phase 3 — Candidate stories *(sketch — re-slice on pull-in)*

| # | Story | Slice |
|---|---|---|
| **S001** | **`ProcessPlanTemplate` + `ProcessStepTemplate`** — versioned, published, country + category scoped, with step predecessors; seeded for one real market | domain → persistence → API → UI → test |
| **S002** | **`ProcessObjective`** (+ group) — what we are trying to achieve, for a product in a market | full slice |
| **S003** | **`ProcessPlan`** — instantiated from a pinned published template version (the ADR-035 pattern) | full slice |
| **S004** | **`ProcessStep`** — dated, predecessors/successors, parent/child, dated status history | full slice |
| **S005** | **Wire the seams** — attach submissions, meetings, correspondence, commitments, inspections and registrations to steps | full slice |
| **S006** | **Capstone** — the timeline view (*what is late, what does it block?*), browser proof, ADR, retro | UI → test → docs |

**ADR to write:** *Process plans instantiate from pinned template versions* — next free number, or an amendment to ADR-035 if the versioned-template abstraction is extracted.
