# ADR-037 — Registrations are regulatory assets: stored facts, derived interpretation

**Status:** Accepted · **Date:** 2026-07-31 · **Epic:** EPIC-005

## Context

EPIC-001–003 made RegOS able to prepare, validate and assemble a filing. Nothing modelled what a filing *produced*.

[`02-domain-model.md`](../architecture/02-domain-model.md) once sketched "Market Registration" as a child entity of Product. That was reasonable before a submission model existed, but the model had moved on: an authorisation has an identity outsiders quote, a lifecycle of its own, dates that govern behaviour, and a portfolio question — *"what do we hold in Canada?"* — that is not asked product-first.

This ADR records what EPIC-005 settled, and names the pattern the whole platform has been converging on.

## Decision

### 1. Registration is its own aggregate, in its own bounded context

An application is something you **do**. A submission is something you **send**. A registration is something you **hold**.

The practical confirmation was the query. Users do not say *"load Product A and inspect its registrations"*; they say *"show me everything we hold in Canada"*. As a child entity that would be a scan across every product, taking a lock on the Product aggregate for every status change.

The architecture document was **updated to match the implemented model**, rather than the implementation bent to fit an outdated sketch.

### 2. Provenance distinguishes when it happened from when we learned

Every history entry carries two dates, and they are never conflated:

| | Means |
|---|---|
| `OccurredOn` (`DateOnly`) | when it happened **in the world** |
| `RecordedOnUtc` (`DateTime`) | when **RegOS learned** of it |

RegOS must not assume it witnessed every regulatory event. Acquisitions, in-licensed assets and migrated portfolios all carry authorisations granted before RegOS existed. Storing only one of these permanently destroys the ability to tell a late entry from a backdated one, and a migrated record's story would otherwise begin with a gap.

The same principle drives `RecordApproval` taking the **business date** rather than the clock, and creation refusing a privileged constructor: an import is create-then-record, and the resulting history reads honestly — *recorded today, occurred 2019-04-12.*

### 3. Lifecycle history is append-only

`RegistrationStatusEntry` has no mutating behaviour at all. The aggregate adds entries and never edits or removes one.

Current state lives on the registration; history records how it got there. **A regulated record whose history can be rewritten is not a history.**

The invariant that binds them: **every transition updates `CurrentStatus` and appends exactly one immutable history entry.** Nothing else writes either, so current state and the record of how it was reached cannot disagree. It is asserted for all 18 permitted transitions by theory, not for a chosen few, and all 46 forbidden pairs are asserted to change nothing.

### 4. Lifecycle policy is declared once, on the server

The permitted graph is a table in `RegistrationLifecycle`, not conditionals spread through the aggregate — so future capabilities arrive as edits to a matrix, and the graph stays exhaustively testable.

The governing principle: **forbid transitions that make the record incoherent; permit transitions that are merely unusual.** RegOS must not encode one regulator's process as universal law.

That yields **forward jumps from every pre-decision state** — a migrated 2019 authorisation never passed through `Submitted`, and recording it as granted is not skipping steps but recording that RegOS entered the story late — and **three terminal states for three different reasons**:

| State | Terminal because |
|---|---|
| `Refused` | permanently — nothing was ever granted, so there is nothing to suspend, expire or surrender |
| `Expired` | until renewal is modelled |
| `Withdrawn` | until restoration is modelled |

The latter two are **deliberate boundaries of the current domain, not assertions that all regulators prohibit those paths.**

**Clients are told the consequences, never the graph.** The read model carries `allowedNextStatuses`; there is no endpoint exposing the transition table. A UI asks *"did the server include Suspend?"*, never *"may I show Suspend?"* — so a terminal registration simply arrives with nothing to offer, and the graph can evolve without changing the contract.

One refinement the table alone could not express: `ChangeStatus` cannot perform the **first** grant, because it has no way to supply the registration number and validity dates. `Planned → Approved` is a legal *destination*; `RecordApproval` is the required *operation*. Returning to `Approved` from `Suspended` is a lift, not a grant, and needs neither.

### 5. Business time only moves forward

A status may not take effect before the one it replaces. Equal dates are allowed — a migration routinely produces two events on one day.

This invariant found an incoherence that had already been accepted as correct: five tests created a registration dated *today* and approved it in *2019*. The tests were corrected, not the rule.

Discovering an earlier event later is a **correction**, which is a separate concept from a transition. Solving both with one mechanism would mean opening transitions whose only purpose is undoing operator error.

### 6. The two portfolio axes are separate read models

*"Where is this product registered?"* and *"what do we hold in this market?"* are mirror images, not projections of one object. A single DTO carrying both axes would leave every consumer ignoring half its fields — coupling, not reuse.

Nothing is filtered server-side. *"What do we hold"* is not *"what is currently marketable"*: a withdrawn authorisation is still part of the regulatory portfolio. Everything is returned with live authorisations leading, and narrowing is presentation.

A registration has **one canonical URL**, flat rather than nested beneath a product — it is a regulatory asset, not product work, and both axes link to the same page.

### 7. Expiry visibility is derived, never persisted

The server returns `ExpiresOn`, `HasRunningValidity`, `DaysUntilExpiry` and `IsExpired`. It deliberately does **not** return `IsExpiringSoon`.

"Soon" is policy — ninety days today, a hundred and eighty tomorrow, market-specific after that, tenant-configurable eventually. `DaysUntilExpiry` never goes out of date; a threshold does. A stored "expiring soon" flag would be wrong the moment the clock moved and would need a job to keep it honest.

Two consequences worth naming:

- **Null when the lifecycle has ended.** A surrendered authorisation keeps the expiry date it was granted with, but it has left the validity timeline. Reporting a countdown for it would not be noise — it would be false. `HasRunningValidity` exists so a null countdown is self-explaining: *no date recorded* versus *no longer counting*.
- **Negative values are kept.** An approved registration whose expiry passed last month reports `-31` — lapsed in the world, not yet recorded here. That is the strongest attention signal the portfolio has, and clamping it to zero would discard exactly the information worth surfacing.

*"Which registrations deserve attention today?"* is answered by an objective set — **those whose validity is still running**, nearest expiry first — with no threshold and no limit. Ordering makes it useful; prioritising is left to the reader.

### 8. Renewal is not a lifecycle transition

Deferred out of this epic deliberately. A renewal keeps the status at `Approved` and moves the validity dates: it changes **authorisation validity**, not **status**.

Every operation in EPIC-005 changes status and appends exactly one history entry. Renewal would be the first that does not — a different class of operation, and forcing it in to be near expiry would weaken the invariant that makes the lifecycle easy to reason about.

**Renewal is not the last lifecycle story; it is the first authorisation-validity story.**

## The pattern this names

Across four epics the same split keeps appearing:

| Persisted fact | Derived interpretation |
|---|---|
| attached documents, placement | validation findings (ADR-035) |
| placement | placeholder satisfaction, content progress (ADR-036) |
| `CurrentStatus` | `allowedNextStatuses` |
| `ExpiresOn` | `DaysUntilExpiry`, `IsExpired` |

> **Persist regulatory facts. Derive regulatory interpretation.**

Facts are what a regulator would recognise as the record. Interpretation is what the platform says about them today, and it changes when the rules, the clock or the reader change. Storing interpretation creates something that can silently disagree with the facts it came from, and needs a job to keep it honest.

This is now one of RegOS's defining characteristics, and the default for any new read model.

## Consequences

**Good**

- The portfolio answers three questions — by product, by market, by urgency — without a scheduler, a cache or a materialised view.
- Lifecycle rules exist in exactly one place; the UI reimplements none of them and a terminal registration needs no special-casing.
- A migrated portfolio can state historical truth, and a late entry is distinguishable from a backdated one forever.
- The transition matrix makes renewal and restoration obvious future edits rather than scattered conditional changes.

**Costs, accepted**

- Derivation runs on every read. Expiry is arithmetic over rows already fetched; if a portfolio ever outgrows one response that becomes pagination, which fits on top of this shape.
- `Expired` and `Withdrawn` are terminal today, so a real restoration scenario requires a domain change rather than a data fix. That is the honest trade for not guessing at regulator behaviour.
- Corrections have no mechanism at all. A status entered in error currently needs a database intervention — accepted so the lifecycle stays about regulatory events, with corrections designed separately as superseded entries.

## Related

- [ADR-030](ADR-030-tenant-is-its-own-aggregate.md), [ADR-031](ADR-031-tenant-isolation-by-query-filters.md) — tenancy, and the fail-closed filters registrations rely on
- [ADR-035](ADR-035-submissions-bind-to-a-published-template-version.md) — missing upstream data must never block the business; the same reasoning makes the originating application optional
- [ADR-036](ADR-036-the-dossier-is-structure-placeholders-are-validation.md) — the derivation principle this generalises
