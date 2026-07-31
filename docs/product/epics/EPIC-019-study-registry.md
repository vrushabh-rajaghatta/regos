# EPIC-019 — Study registry

**Status:** ⚪ Not Started · **Branch:** `epic/EPIC-019-study-registry` (cut at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

Clinical and non-clinical studies as first-class records that applications and submission content can **cite** — so *"which studies support this filing?"* is a query, and Study Tagging Files become possible.

> **Phase 1 below is settled.** **Phases 2–3 are a sketch**, written so this epic can be picked up months from now without re-deriving it — they are **not approved design**. Confirm, amend or replace them in the Phase-2 conversation when this epic is pulled into **Now**.

---

## Phase 1 — Epic plan

### Outcome
A regulatory user can register a clinical or non-clinical study once, cite it from every application and every piece of submission content that reports it, and answer *"which studies support this filing?"* and its inverse *"which filings cite this study?"* — the two questions that otherwise get answered by reading file names.

### The concepts it introduces

| | RIM object | Attrs |
|---|---|---|
| **Clinical Study** | Clinical Study | 23 |
| **Non-Clinical Study** | Non-Clinical Study | 18 |

Only two objects — but they are **peers of Application and Submission Content** in RIM, cited by both, and RegOS has no equivalent at all. This is the smallest epic in the RIM-alignment runway and has **no dependencies**, which makes it a good candidate to slot in whenever a larger epic needs to be broken up.

### In scope ✅
- **`ClinicalStudy`** — global and local identifiers, regional identifier, sponsor study number, title, description, phase, type, sub-type, indication, therapeutic area, type of control, route of administration, subject count, country, sponsor, dated status, start and closeout dates.
- **`NonClinicalStudy`** — the same shape minus phase, subject and indication.
- **Citation links** — study ↔ `RegulatoryApplication` (RIM: Peer, Multiple) and study ↔ `SubmissionDocument` (RIM: Submission Content → Clinical/Non-clinical Studies, Multiple).
- **Both directions queryable** — studies supporting a filing, and filings citing a study.
- Study registry UI, browser proof, ADR only if forced.

### Out of scope ⏸️ (deferred, with reason)
| Deferred | Why |
|---|---|
| **STF (Study Tagging File) generation** | → **EPIC-007**. This epic makes the data exist and be citable; the publishing engine renders the XML. `DTD Version - STF` is modelled in **EPIC-004**. |
| **Study results, endpoints, statistical data** | RegOS is a regulatory information system, not a CTMS or a clinical data repository. It records *that* a study exists and what it is about. Hard line — worth restating when someone asks. |
| **Site / investigator management** | CTMS territory. `Organization Site` (EPIC-016) covers the sponsor-side sites RIM actually references. |
| **Protocol / amendment versioning** | Not in RIM's study objects; add only if a real workflow needs it. |
| **ClinicalTrials.gov / EudraCT / CTIS registry sync** | Integration, not domain. The identifier fields are the seam. |
| **Commitment ↔ study linkage** (post-marketing study commitments) | Needs **EPIC-006**. Nullable seam only, from whichever ships second. |
| **Study ↔ registration linkage** | RIM has `License → Clinical Study` (Multiple). Add when a story asks; the seam is a join table. |

### Definition of Done
- A clinical study can be registered with its global, local and regional identifiers, and is findable by any of them.
- A non-clinical study can be registered.
- Either can be cited from a regulatory application and from a piece of submission content, with the citation visible from both ends.
- Dated status history on both (RIM marks study status "Single / Historical").
- Start and closeout dates recorded; RIM marks these historical too.
- Browser proof: register a study → cite it from an application → cite it from a submission document → see both citations from the study's own page.
- ADR only if a context-boundary decision is forced.

---

## Phase 2 — Domain design *(sketch — not approved)*

### Entities

**`ClinicalStudy`** — aggregate root, new context `src/Study/`.

| Field | Notes |
|---|---|
| `Id`, `TenantId` | fail-closed filter |
| `GlobalId`, `LocalId` | RIM has both, both required |
| `RegionalIdentifier?`, `SponsorStudyNumber?` | |
| `Title`, `Description` | |
| `Phase` | I / II / III / IV — closed enum, drives behaviour |
| `Type`, `SubType` | reference data |
| `Indication`, `TherapeuticArea` | coded — reuse EPIC-018's `CodedConcept` if it exists by then |
| `TypeOfControl`, `RouteOfAdministration` | coded |
| `SubjectCount?` | |
| `CountryId`, `SponsorOrganizationId` | |
| `Status` + dated history | |
| `StartDate`, `CloseoutDate?` | RIM: both historical |

**`NonClinicalStudy`** — same, minus `Phase`, `SubjectCount`, `Indication`.

**Citation links** — `ApplicationStudyCitation` and `SubmissionDocumentStudyCitation` join aggregates, or owned collections on the citing side. See decision 2.

### Decisions to settle (Phase 2, on pull-in)

**1. One aggregate or two?** RIM has two sheets with ~80% overlapping attributes. Options: two aggregates (RIM-faithful, some duplication); one `Study` with a `StudyKind` discriminator (less duplication, one nullable-field cluster). *Lean: two aggregates* — they are cited in different CTD modules (5 vs 4), they will diverge (clinical gains subjects, arms, populations; non-clinical gains species, GLP status), and the Rule-of-Three argument for merging has not been met. Record the reasoning; this is the one real modelling call.

**2. Citation direction.** *Lean: a join aggregate owned by neither side*, because both directions are queried and neither is naturally the owner. Making it a collection on `RegulatoryApplication` would put a Study dependency into the RegulatoryApplication context.

**3. Context.** *Lean: new `src/Study/`.* Studies are cited by Application, Submission Content, Registration and Commitment — four contexts — so parking them inside any one of those creates the wrong dependency direction.

**4. Study status is a closed enum**, following EPIC-005: it drives behaviour (can a study be cited in a filing before it has started?).

**5. Study identifiers are not unique across tenants** but should be unique **within** one. Assert it, or assert its absence, per the EPIC-005 precedent of testing the constraint you chose *not* to add.

### Change-case analysis

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| STF generation (EPIC-007) | **High** | Study id + type + citation is exactly the STF payload |
| Clinical and non-clinical diverge | **High** | Two aggregates from day one — merging later is cheaper than splitting |
| Registry sync (CT.gov, CTIS) | Medium | Distinct identifier fields per registry already |
| Commitments cite studies (EPIC-006) | Medium | Join aggregate pattern extends to a third citer without touching the study |
| Studies cited by registrations | Medium | Same |
| A study spans several countries | Medium | RIM says Single; if it becomes Multiple, an owned collection replaces the FK with no data loss |
| Coded indication/therapeutic area needs real terminology | Medium | `CodedConcept` with a `system` field (shared with EPIC-018) |

---

## Phase 3 — Candidate stories *(sketch — re-slice on pull-in)*

| # | Story | Slice |
|---|---|---|
| **S001** | **`ClinicalStudy`** — aggregate, dated status, tenant filter, persistence, API, study registry UI | domain → persistence → API → UI → test |
| **S002** | **`NonClinicalStudy`** — same shape, its own aggregate | full slice |
| **S003** | **Citations** — cite a study from an application and from a piece of submission content; both directions visible | full slice |
| **S004** | **Capstone** — *"which studies support this filing?"* on the application workspace, browser proof, retro (ADR only if forced) | UI → test → docs |
