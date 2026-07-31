# EPIC-005 — Registration tracking

**Status:** 🟡 In Progress (Phase 1–2 approved; 0 of 4 stories shipped) · **Branch:** `epic/EPIC-005-registration-tracking` · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

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
| **STORY-001** | **The Registration aggregate** — create for a product in a market, record the grant (number + dates), optional application link; persistence, API, read model | ⚪ Not Started |
| **STORY-002** | **Lifecycle** — the transitions the domain permits, each dated, with an immutable history | ⚪ Not Started |
| **STORY-003** | **Portfolio views** — *where is this product registered?* / *what do we hold in this market?* + the registration UI | ⚪ Not Started |
| **STORY-004** | **Expiry & renewal visibility** (derived, no scheduler) + capstone browser proof + ADR-037 + retro | ⚪ Not Started |
