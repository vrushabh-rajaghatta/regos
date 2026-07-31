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

## Phase 2 — Domain design *(sketch — not approved)*

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

**1. One context or several.** *Lean: one.* Record the reasoning; it is the ADR.

**2. `HaQuestion` — child or root.** *Lean: child*, per RIM. The pressure to promote it comes from the due view; measure before promoting.

**3. Dated status history on all five.** *Recommended.* This epic is where the cross-cutting history rule earns its keep — four of the five statuses are "Single / Historical" in RIM. Reuse `RegistrationStatusEntry` verbatim rather than inventing a per-aggregate shape; if this is the third occurrence, **extract the shared shape** (the `RegistrationCreationPolicy` note already says a third occurrence triggers extraction, not a fourth).

**4. Correspondence direction.** RIM has `Correspondence Mode`/`Action` but no explicit inbound/outbound flag. *Lean: add one* — every real query starts with "did they write to us or we to them?", and deriving it from initiator/recipient names is fragile.

**5. Controlled lists.** Correspondence Action/Format/Mode/Type/Category, Meeting Type/Format/Status, Commitment Type/Source/Status, Question Topic, Inspection Type/Source — RIM makes all of these controlled lists. That is **11+ vocabularies**. Decide up front: reference data (feeds EPIC-012 authoring) or closed enums. *Lean: reference data for the classifications, closed enums for the statuses* — the EPIC-005 status argument.

**6. Attachments reuse `ProductDocument`.** RIM points Correspondence at `Content`, which is what `ProductDocument`/`DocumentVersion` already is. Do not build a second document store.

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
| **S006** | **Capstone** — interactions timeline on the application workspace, browser proof of the full journey, ADR, retro | UI → test → docs |

**ADR to write:** *The health-authority interaction cluster is one bounded context* — next free number.
