# EPIC-005 — Registration tracking

**Status:** 🟢 Complete (4 of 4 stories shipped) · **Branch:** `epic/EPIC-005-registration-tracking` · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

The first capability about the world **after** a submission. EPIC-001–003 made RegOS able to prepare, validate and assemble a filing; this makes it able to say what that filing produced, and what the business holds today across every market.

---

## Phase 1 — Epic plan

### Outcome
A regulatory user can answer *"where is this product approved, under what authorisation, and until when?"* — and the inverse, *"what do we hold in this market?"* Registration status is a tracked, dated, auditable fact rather than something reconstructed from filings.

### The concept it introduces

| | Means | Status |
|---|---|---|
| **RegulatoryApplication** | the *effort* — a filing project in a market | exists |
| **Submission** | an *artefact* sent within that effort | exists |
| **Registration** | the *authorisation* — what the business holds, and its state over time | **new** |

An application is something you *do*. A registration is something you *hold*.

### In scope ✅
- The **Registration aggregate** — product, country, authority, holder organisation, licence number, dates.
- **Lifecycle**: dated transitions, and an immutable history a regulator could read.
- **Optional link** to the `RegulatoryApplication` that produced it.
- **Portfolio views** — *where is this product registered?* and *what do we hold in this market?*
- **Expiry visibility** — "expires in 90 days", derived on read.
- Registration UI, browser proof, ADR-037.

### Out of scope ⏸️ (deferred, with reason)
| Deferred | Why |
|---|---|
| **Renewal reminders / notifications** | Showing *"expires in 90 days"* is a domain capability; sending mail is infrastructure → EPIC-014. |
| **Variations workflow** | A variation is a filing *against* a registration — it needs the registration to exist first → EPIC-006/007. |
| **Transfers between holders** | Real, and naturally anchored on this aggregate, but not needed to answer the epic's question. |
| **IDMP product depth** (strengths, presentations, packaging) | → EPIC-010. The model deliberately allows several registrations per market so this can arrive without reshaping. |
| **Dashboards & portfolio analytics** | → EPIC-011; this epic delivers the queries those would chart. |
| **Authority correspondence & commitments** | → EPIC-006. |

### Definition of Done
- A registration can be created for a product in a market, with or without an originating application, and cannot be created for a country/authority that do not agree.
- Its lifecycle transitions are enforced by the domain, each carrying the date it happened, and every transition is recorded in an immutable history.
- The licence number is the business identity; **no** uniqueness is imposed on (product, country).
- Both portfolio questions are answerable through the API and the UI.
- Expiry proximity is derived, never stored.
- Browser proof of the loop: record an approval → see it in both portfolio views → change its status → see the history.
- ADR-037 written.

---

## Phase 2 — Domain design

### The model

```
Product ──┐
Country ──┤
Authority─┼──▶ Registration (aggregate root)
Organization (holder)        │  CurrentStatus, RegistrationNumber?,
RegulatoryApplication? ──────┘  ApprovedOn?, ExpiresOn?
                                  └── RegistrationStatusHistory (immutable)
```

**Decisions (approved 2026-07-31):**

**1. Registration is its own aggregate, in its own bounded context.**
[`02-domain-model.md`](../../architecture/02-domain-model.md) previously sketched Market Registration as a child entity of Product. That was reasonable before a submission model existed; the model has since evolved. A registration has an identity outsiders quote (the licence number), a lifecycle of its own, dates, and future behaviours — renewals, variations, transfers, suspensions — that all anchor on it. The practical confirmation is the query: users do not ask *"load Product A and inspect its registrations"*, they ask *"show me everything we hold in Canada"*. That is registration-centric, and as a child entity it would be a scan across every product with a lock on the Product aggregate for every status change. **The document has been updated to match the implemented model rather than the implementation bent to an outdated sketch.**

**2. The originating application is optional.**
RegOS must not assume it witnessed every historical regulatory event. Acquisitions, mergers, in-licensed assets and migrated portfolios all carry authorisations that were filed elsewhere — or before RegOS existed. If an application is there, the link is recorded; if not, the registration is no less real. The same principle as [ADR-035](../../adr/ADR-035-submissions-bind-to-a-published-template-version.md) decision 4: missing upstream data must never block the business.

**3. Status is a closed enum in code, not reference data.**
Reference data answers *what exists*; code answers *what may happen*. Status drives behaviour — what may be renewed, what a variation may be filed against — so it belongs with the behaviour. Initial set: `Planned`, `Submitted`, `UnderReview`, `Approved`, `Suspended`, `Withdrawn`, `Expired`, `Refused`. Which are terminal, and which transitions the domain permits, is STORY-002's conversation and deliberately not settled here.

**4. No uniqueness on (product, country).**
Real portfolios hold several authorisations in one market — different strengths, different presentations, different holders after a partial divestment, legacy authorisations that were never surrendered. The **registration number** is the business identity; a constraint on (product, country) would become technical debt the first time a real portfolio was loaded.

**5. `HolderOrganizationId` is distinct from `TenantId`.**
Mirroring `RegulatoryApplication.ApplicantOrganizationId`. The platform keeps tenant, applicant and holder as three concepts rather than conflating them — which is what makes licensing, partnerships and divestitures expressible later.

**6. Current status is stored; history is immutable.**
`CurrentStatus` lives on the aggregate so portfolio queries stay a single indexed read, and `RegistrationStatusHistory` records one dated, append-only row per transition. Derived-on-read would make the portfolio view replay history for every row; history-free would leave a regulated record unable to say when it became what. The same coexistence of current state and immutable record the platform already uses for submissions and their snapshots.

### Change-case analysis

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| Variations filed against a registration | High | Registration is an aggregate with its own id to reference |
| Renewals producing new dates and history rows | High | Dated transitions and history already model it |
| Several registrations per market (strengths, presentations) | High | No (product, country) constraint |
| Transfer to a different holder | Medium | Holder is a field on an aggregate, not an ownership edge |
| Registrations migrated from a legacy system | Medium | Optional application link; status can start at `Approved` |
| Renewal notifications | Medium | Expiry is derived; a scheduler reads the same data (EPIC-014) |
| IDMP depth beneath a registration | Low-Medium | Child entities of the registration, or a reference to a product presentation |

---

## Phase 3 — Stories

| # | Story | Status |
|---|---|---|
| **STORY-001** | **The Registration aggregate** — create for a product in a market, record the grant (number + dates), optional application link; persistence, API, read model | 🟢 Complete |
| **STORY-002** | **Lifecycle** — the transitions the domain permits, each dated, with an immutable history | 🟢 Complete |
| **STORY-003** | **Portfolio views** — *where is this product registered?* / *what do we hold in this market?* + the registration UI | 🟢 Complete |
| **STORY-004** | **Expiry & renewal visibility** (derived, no scheduler) + capstone browser proof + ADR-037 + retro | 🟢 Complete |

### STORY-001 — The Registration aggregate (shipped)

A new bounded context, `src/Registration/`, mirroring the shape every other module uses: `Domain` / `Application` / `Infrastructure`, EF configuration in the central `Persistence` project, and a fail-closed tenant query filter (ADR-031).

**Decisions (approved 2026-07-31):**

1. **The status history arrives with the concept, not with the behaviour.** It looked like S002 material, but it is part of a registration's *identity and provenance* rather than its lifecycle: a record created today for an authorisation granted in 2019 cannot honestly exist without saying so. If history began at the first transition, every migrated record's story would start with a gap.
2. **Every entry carries two dates.** `OccurredOn` (`DateOnly`) is when it happened in the world; `RecordedOnUtc` is when RegOS learned of it. They answer different regulatory questions, and storing one permanently destroys the ability to tell a late entry from a backdated one.
3. **History entries are statuses, not a parallel event vocabulary.** The first entry *is* `Planned` rather than a `Created` event, so the history reads as one chronological sequence of the states held — one word for a thing rather than two.
4. **`Create` then `RecordApproval`, never a privileged constructor.** Creation always begins at `Planned`; the only route to `Approved` is the method that enforces what approval means. An import is create-then-record, and the resulting history is honest: *recorded today, occurred 2019-04-12.* `RecordApproval` takes the **business date**, never the clock.
5. **A parallel `RegistrationCreationPolicy`, not a shared validator.** It duplicates six reference-data checks from `IRegulatoryApplicationCreationPolicy`, and deliberately omits the seventh — *no duplicate per (product, country, authority)* — which is exactly the constraint this epic rejects. The two policies have already diverged on the rule that matters most, so extracting a common one would create a dependency between contexts meant to be independent, to save duplicating rules that are drifting apart. Noted in code: **a third occurrence triggers extraction, not a fourth.**

**Append-only from day one.** `RegistrationStatusEntry` has no mutating behaviour at all — the aggregate adds entries and never edits or removes one. Current state lives on the registration; history records how it got there, and a regulated history that can be rewritten is not a history.

**No unique index on (ProductId, CountryId)** — asserted by a test, not merely omitted, so a future reader sees it was a decision.

**API:** `POST /api/products/{id}/registrations` · `POST /registrations/{id}/approval` · `GET /registrations/{id}` · `GET /api/products/{id}/registrations`.

**Verified:** 641 backend tests green (35 new: 21 domain, 14 integration against the real seeded reference data); migration creates two tables with five `Restrict` foreign keys; the four endpoints exercised live on an isolated stack — including a second registration in the same market accepted (201), a mismatched authority/country rejected (400), a second approval refused (409), and the detail view showing the provenance split:

```
Planned   occurred 2019-01-15   recorded 2026-07-31   "Carried over from the legacy register."
Approved  occurred 2019-04-12   recorded 2026-07-31   "Original approval."
```

### STORY-002 — Lifecycle (shipped)

Where the platform stops merely recording regulatory state and starts enforcing
the rules that make that state meaningful. **No schema change** — the lifecycle
is pure behaviour over STORY-001's two tables.

**The governing principle (approved 2026-07-31):** *forbid transitions that make
the record incoherent; permit transitions that are merely unusual.* RegOS must
not encode one regulator's process as universal law.

**The transition table** — declared as a matrix in `RegistrationLifecycle`, not
as conditionals in the aggregate, so future capabilities arrive as edits to the
table and the permitted graph stays exhaustively testable:

| From | May become |
|---|---|
| `Planned` | `Submitted` · `UnderReview` · `Approved` · `Refused` · `Withdrawn` |
| `Submitted` | `UnderReview` · `Approved` · `Refused` · `Withdrawn` |
| `UnderReview` | `Approved` · `Refused` · `Withdrawn` |
| `Approved` | `Suspended` · `Expired` · `Withdrawn` |
| `Suspended` | **`Approved`** · `Expired` · `Withdrawn` |
| `Refused` · `Expired` · `Withdrawn` | — |

**Decisions (approved 2026-07-31):**

1. **Forward jumps are permitted from every pre-decision state.** A migrated
   authorisation granted in 2019 never passed through RegOS's `Submitted` or
   `UnderReview`. Recording it as approved is not skipping steps — it is
   faithfully recording that RegOS entered the story after those steps had
   already happened. Historical import and operational workflow are the same
   model, and a strict pipeline would break the very case STORY-001 was built
   for.
2. **`Suspended → Approved` is permitted.** Suspension is a reversible
   operational state, not the destruction of the authorisation: the grant still
   exists, it merely cannot be exercised. Refusing the lift would be the
   surprising choice.
3. **Three states are terminal, for three different reasons.** `Refused`
   permanently — no authorisation ever existed, so there is nothing to suspend,
   expire or surrender. `Expired` until renewal is modelled. `Withdrawn` until
   restoration is modelled. **The latter two are deliberate boundaries of the
   current domain, not assertions that all regulators prohibit those paths.**
4. **Renewal is deferred to STORY-004.** A renewal keeps the status at `Approved`
   and moves the validity dates — it changes *authorisation validity*, not
   *status*. Deferring it preserves a clean invariant for this story: every
   operation here changes status, and renewal would be the first that does not.
5. **Corrections are out of scope entirely.** A status entered by mistake is a
   data-quality problem wearing a lifecycle costume. Solving both with one
   mechanism would mean opening transitions like `Withdrawn → Approved` whose
   only purpose is undoing operator error. A future correction model can
   introduce superseded entries and amended effective dates without weakening
   the lifecycle.
6. **`RecordApproval` keeps its own endpoint but routes through the same
   validator.** Approval carries the registration number and validity dates —
   a distinct business operation, not a status with extra fields. Internally it
   passes the identical transition gate, so a refused registration cannot be
   quietly approved by a different door.

**Two invariants, tested directly:**

- **Every transition updates `CurrentStatus` and appends exactly one immutable
  history entry.** Asserted for all 18 permitted transitions by theory, not for
  a chosen few — and all 46 forbidden pairs are asserted to change nothing.
- **Business time only moves forward.** A status may not take effect before the
  one it replaces; equal dates are allowed, because a migration routinely
  produces two events on one day. Discovering an earlier event later is a
  correction, which is a separate concept.

**One guard the table alone could not express:** `ChangeStatus` cannot perform
the *first* grant, because it has no way to supply the number and validity
dates. Returning to `Approved` from `Suspended` is a lift, not a grant, and is
allowed. So the first entry into `Approved` is always through the operation that
establishes what approval means.

**The read model asks the domain, never restates it.** The detail view carries
`allowedNextStatuses` straight from the table, so a client offers exactly the
choices the domain would accept — and a terminal registration offers none.

**API:** `POST /registrations/{id}/status` — `{ status, occurredOn, note }`.

**Behaviour change:** a `Refused` registration was previously approvable. It now
returns 409. Nothing was ever granted, so there is nothing to grant.

**Verified:** 735 backend tests green (94 new: 89 domain, 5 integration);
`has-pending-model-changes` confirms no schema change; the full lifecycle
exercised live on an isolated stack — the first grant refused through
`/status` (409), `Planned → Suspended` refused (409), a backdated status refused
(400), a second grant refused after the status moved on (409), and a terminal
registration reporting no onward transitions:

```
Planned     occurred 2020-01-10   recorded 2026-07-31
Submitted   occurred 2020-03-02   recorded 2026-07-31
UnderReview occurred 2020-03-02   recorded 2026-07-31
Approved    occurred 2021-02-08   recorded 2026-07-31   "Original approval."
Suspended   occurred 2023-09-14   recorded 2026-07-31   "GMP non-compliance at the manufacturing site."
Approved    occurred 2024-01-30   recorded 2026-07-31   "Suspension lifted."
Withdrawn   occurred 2025-06-01   recorded 2026-07-31   "Surrendered on portfolio review."
```

The grant survives everything after it: `NDA-556677`, approved 2021-02-08,
expiring 2031-02-08, on a registration now `Withdrawn`.

**Five STORY-001 tests were corrected, not the rule.** They created a
registration dated *today* and then approved it in *2019* — a history that was
never coherent, and that only the chronology invariant made visible. Re-dated to
tell the migration story properly.

### STORY-003 — Portfolio views & the registration workspace (shipped)

The capability is *manage registrations*, not *view registrations*. Half the
server side already existed — `ListProductRegistrations` shipped in STORY-001 —
so this added the opposite axis and made both reachable.

**Decisions (approved 2026-07-31):**

1. **Two read models, not one.** `RegistrationSummary` (by product) and
   `MarketRegistrationSummary` (by market) are mirror images: one repeats the
   product in every row, the other the country. A single DTO carrying both axes
   would leave every consumer ignoring half its fields — coupling rather than
   reuse.
2. **A markets index, `GET /api/registrations/markets`.** Without it the market
   workspace has no entry point, because nobody starts by browsing two hundred
   countries to find the three they are in. Deliberately thin — *Canada (12)* —
   so it stays navigation rather than analytics, and EPIC-011 can add
   breakdowns without changing this contract.
3. **One canonical URL: `/regulatory/registrations/{id}`.** Flat, not nested
   under the product. A registration is an aggregate in its own right — a
   regulatory asset rather than product work — and nesting it would mint a
   second URL for the same thing. Registrations sit beside Products in the
   navigation for the same reason.
4. **No server-side status filtering.** *"What do we hold"* is not *"what is
   currently marketable"*: a withdrawn authorisation is still part of the
   portfolio. Everything is returned, ordered so live authorisations lead.
   Narrowing is presentation; hiding rows would be data loss dressed as a
   default.
5. **The grant dialog is chosen client-side from `registrationNumber`.** No
   extra server flag. That is not lifecycle policy — it is selecting the right
   interaction from the shape of the record. Revisit if a registration ever
   gains a second route to `Approved`.

**The UI reimplements no regulatory logic.** The detail page renders one button
per entry in `allowedNextStatuses`. It never asks *"may I show Suspend?"*, only
*"did the server include it?"* — so a terminal registration arrives with an
empty array and the page simply says the lifecycle has ended. The server's
refusals are shown verbatim, because they are written for a regulatory reader.

**One deviation from the plan, deliberately.** The filter is a status
dropdown whose options are derived from the rows on screen, not an *"Active
only"* toggle. Deciding that "active" means one set of statuses rather than
another would have put the terminal-state knowledge back in the client — the
exact policy STORY-002 kept on the server. The live-first ordering already
delivers the benefit the toggle was for.

**API:** `GET /api/countries/{countryId}/registrations` ·
`GET /api/registrations/markets`.

**Verified:** 739 backend tests green (4 new integration); frontend typecheck,
lint and build clean; **54 browser tests green** against an isolated stack
(API 5301, web 5174, throwaway database `regos_e005s3`), including the new
portfolio spec — create through the form, find it under the product, find it
under the market, and prove both routes reach the same URL.

**Two defects the browser found before release**, both in code written this
story: a rejected mutation escaped as an unhandled page error rather than being
caught and rendered, and the shared status badge carried a `data-testid` that
made "the status" mean every badge on the page at once. Both fixed at source.

### STORY-004 — Expiry visibility (shipped)

The third portfolio question — *which registrations deserve attention today?* —
answered without a scheduler, a cache or a stored flag.

**Decisions (approved 2026-07-31):**

1. **`DaysUntilExpiry`, never `IsExpiringSoon`.** "Soon" is policy: ninety days
   today, a hundred and eighty tomorrow, market-specific after that,
   tenant-configurable eventually. The number never goes out of date; a
   threshold does. The threshold lives in one frontend file
   ([`expiry.ts`](../../../web/regos-web/src/features/regulatory/registrations/components/expiry.ts))
   and nowhere else.
2. **Null once the lifecycle has ended.** A surrendered authorisation keeps the
   expiry date it was granted with, but it has left the validity timeline —
   reporting a countdown for it would not be noise, it would be false. Decided
   by `RegistrationLifecycle.IsTerminal`, so there is no second list of statuses
   beside the transition table.
3. **`HasRunningValidity`, so a null is self-explaining.** True with a null
   countdown means no expiry date was recorded; false means it stopped
   mattering. Two different facts that would otherwise look identical.
4. **Negative values are kept.** An approved registration whose expiry passed
   last month reports `-31` — *lapsed in the world, not yet recorded here*, the
   strongest attention signal the portfolio has. Clamping to zero would discard
   exactly the information worth surfacing.
5. **`IsExpired` alongside it**, though derivable, so no client re-implements
   the sign convention.
6. **The attention set is objective**: registrations whose validity is still
   running, nearest expiry first, **no threshold and no limit**. Ordering makes
   it useful; prioritising is the reader's. A silent "top ten" would hide the
   eleventh and read as completeness.

**API:** `GET /api/registrations/expiring`.

**Verified:** 753 backend tests green (14 new: 9 unit over the derivation, 5
integration); **55 browser tests green**, including the capstone journey —
planned → filed → assessed → granted → *needs attention* → suspended →
reinstated → backdating refused → surrendered → off the attention list → seven
history entries, every transition driven through the UI.

---

## Retro

### What the epic set out to do, and whether it did

| Definition of Done | Outcome |
|---|---|
| A registration can be created for a product in a market, with or without an originating application, and not for a country/authority that disagree | ✅ `RegistrationCreationPolicy`, seven rules, 400 on a mismatched authority |
| Lifecycle transitions enforced by the domain, each dated, every one recorded in an immutable history | ✅ `RegistrationLifecycle` as a declarative table; all 18 permitted transitions and all 46 forbidden pairs asserted |
| The licence number is the business identity; **no** uniqueness on (product, country) | ✅ and the *absence* of the index is asserted by a test, not merely omitted |
| Both portfolio questions answerable through the API and the UI | ✅ two read models, two pages, one canonical registration URL |
| Expiry proximity derived, never stored | ✅ and null once the lifecycle has ended, which is stricter than the DoD asked |
| Browser proof: record an approval → see it in both portfolio views → change its status → see the history | ✅ the capstone journey, plus the attention list appearing and clearing |
| ADR-037 written | ✅ [ADR-037](../../adr/ADR-037-registrations-are-regulatory-assets-with-derived-visibility.md) |

### What went well

- **Each story introduced exactly one concept and refused the next.** S1 the asset, S2 the lifecycle, S3 the workspace, S4 visibility. Renewal, corrections, notifications and analytics were all named and left — and each refusal made the next story simpler rather than harder.
- **The chronology invariant found a bug in already-accepted tests.** Five STORY-001 tests planned a registration *today* and approved it in *2019*. Nobody had noticed, because nothing had ever asked whether a history was readable in business time. The tests were corrected, not the rule — and the best of them became a genuinely complete migration proof.
- **The declarative transition table paid for itself immediately.** Exhaustive pairwise testing was possible only because the graph was data. Sixty-four assertions cost one theory each.
- **The client ended up holding no regulatory logic at all.** `allowedNextStatuses` meant the UI never needed to know what a status permits; deriving the status filter from the rows on screen meant it never needed to know which statuses are terminal. The only threshold in the system is one constant in one frontend file.
- **The derivation principle became explicit.** *Persist regulatory facts; derive regulatory interpretation.* It had been implicit since EPIC-002; ADR-037 names it, and it is now the default for new read models.

### What we would do differently

- **Exploratory verification should be routine, not occasional.** The committed S3 spec stops at `Planned` and passed while three real defects sat in code it never touched — an unhandled promise rejection, a `data-testid` on a reusable component, and an empty dialog shell during the close animation. All three were found by a throwaway spec written to walk the paths the committed one does not. **House rule: before shipping UI, drive every state the committed spec leaves uncovered, then delete the scratch spec.**
- **`launchSettings.json` silently overrode `ASPNETCORE_URLS`.** The isolated API tried to bind the founder's port 5225 and only failed because it was already taken. `--no-launch-profile` is now mandatory for any isolated run — the near miss was luck, not design.
- **Two non-component exports triggered the same lint rule twice** (`statusLabel`, then `expiry.ts`). The convention — helpers get their own file — should have been learned the first time rather than rediscovered.
- **Test placement followed the layer, not the style, and that had to be corrected mid-story.** `ExpiryVisibilityTests` was written into the Domain test project for code that lives in Application. Worth stating: the project is named for the layer it tests, and a pure unit test in an integration project is fine.

### Deferred, deliberately

| Deferred | Trigger to revisit |
|---|---|
| **Renewal** | The first authorisation-validity story. It changes validity, not status, and would be the first operation to break "every operation changes status and appends one entry". |
| **Restoration from `Withdrawn`** | A real regulator scenario. The state is terminal as a domain boundary, explicitly not as a claim about regulators. |
| **Corrections** (a status entered in error) | Needs superseded history entries and amended effective dates — its own design. Today it requires a database intervention. |
| **Renewal notifications** | EPIC-014. `/api/registrations/expiring` is the query a scheduler would read; nothing else is needed. |
| **Pagination on the expiring list** | A portfolio that outgrows one response. The unlimited ordered set is the honest shape to paginate later. |
| **Transfers between holders** | Real, naturally anchored on this aggregate, not needed to answer the epic's question. |
| **IDMP depth beneath a registration** | EPIC-010. The absent (product, country) constraint already allows it. |

### What EPIC-005 leaves for the next epic

- **A second aggregate with append-only status history.** `RegistrationStatusEntry` is now the reference implementation of the pattern BACKLOG names as a rule: any aggregate with a status gets `OccurredOn` / `RecordedOnUtc` and a stored current value.
- **A lifecycle table other aggregates can copy.** Submissions, applications and market status all have transitions currently expressed as conditionals.
- **`allowedNextStatuses` as a contract shape.** Any aggregate with a lifecycle can expose its consequences the same way, and keep its graph private.
- **An unresolved product-code question the market view exposed.** Answering *"what do we hold in Canada?"* currently shows a globally unique product code — the concern EPIC-017 exists to address, and the market view is where it first becomes visible to a user.
