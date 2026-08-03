# EPIC-019 — Study registry

**Status:** 🟢 **In flight** — S001 shipped 2026-08-03, S002 next · **Branch:** `epic/EPIC-019-study-registry` · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

Clinical and non-clinical studies as first-class records that applications and submission content can **cite** — so *"which studies support this filing?"* is a query, and Study Tagging Files become possible.

> **Phase 1 below was settled before EPIC-007a existed, and EPIC-007a changed
> what this epic is for.** The original outcome — *"which studies support this
> filing?"* — still stands and is no longer the urgent half. **Phases 2–3 remain
> a sketch and are not approved design**; the amendments are below, and the
> section that supersedes them is [Phase 1 reopened](#phase-1-reopened--2026-08-03).

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

## Phase 1 reopened — 2026-08-03

### Two drivers now, and they want different models

| | Driver | What it needs |
|---|---|---|
| **A** | **Citation** — the original. *"Which studies support this filing?"* and its inverse | a study record, plus links to `RegulatoryApplication` and `SubmissionDocument` |
| **B** | **Study Tagging File** — EPIC-007a, **blocking today** | the sponsor's `study-id`, a `title`, a link from each *placement* to a study, and a `file-tag` per placement |

**They are not the same epic's worth of work, and B is both smaller and
blocking.** A is a capability nobody is currently waiting on; B is why
`SequenceFolderGenerator` refuses every submission with Module 4 content.

### The scoping finding: B needs two facts, not twenty-three

The Phase-1 sketch lists ~23 attributes per study, taken from RIM. **Almost none
of them is required to generate an STF**, and the difference is not a detail.

ICH's STF specification requires `category` — species, route of administration,
duration, type of control — **for exactly four CTD sections**: 4.2.3.1, 4.2.3.2,
4.2.3.4.1 and 5.3.5.1 (**E29**).

> **The FDA IND blueprint seeds none of them.** It offers 4.2.1, 4.2.2, 4.2.3,
> 5.2 and 5.3 — and `category` applies to *none* of those. Checked against
> `RegulatoryTemplates.cs`, not inferred.

**So the minimum that unblocks Module 4 for the blueprint as it stands today is
`study-id` and `title`.** Everything else in the sketch — phase, indication,
therapeutic area, subject count, sponsor, dates, status history — is RIM's list
rather than a fact anything currently demands, and *"because RIM says so"* is
the reasoning this project does not accept.

**That is not an argument for never modelling them.** It is an argument for
letting each arrive with the thing that needs it, which is how `Token`,
`EctdFolder` and `EctdElement` all arrived.

### What blocks even the minimum

| | |
|---|---|
| **a `Study`** | the sponsor's alphanumeric code and a title. **Nothing exists** |
| **placement → study** | which study a *placement* reports. [ADR-053](../../adr/ADR-053-instance-qualifiers-belong-to-the-placement.md)'s shape: a fact about the placement, not the section |
| **`file-tag` per placement** | what role the document plays — synopsis, protocol, CRF. ~40 ICH values, and **this vocabulary is held** (E29) |
| **`ich-stf-v2-2.dtd`** | ⚠ **not held.** An STF can be modelled and written; it cannot be *validated*, so S007's per-package Level 2a would cover two files of three |

**Only the last is an evidence gap**, and it does not block modelling — it
blocks the claim. Recorded so the claim is not made by accident.

## What EPIC-007a discovered — recorded 2026-08-03

**This epic stopped being a filler.** It was scoped as *"no dependencies — good
candidate to slot in whenever a larger epic needs breaking up"*, and that is no
longer the whole truth.

FDA requires a **Study Tagging File for every file in eCTD 4.2.x and
5.3.1.x–5.3.5.x** (evidence **E21**). The seeded FDA IND blueprint offers 4.2.1,
4.2.2 and 4.2.3, and every IND has nonclinical content. So:

> **No package can be generated for any submission that places a document in a
> study-report section, and RegOS refuses one by name** rather than writing a
> package FDA would misfile
> ([ADR-054](../../adr/ADR-054-a-study-tagging-file-is-a-projection-over-a-study.md) §6,
> enforced in `SequenceFolderGenerator`).

That is not a rendering gap. **A study is a business entity that documents are
*about*, and nothing in RegOS knows one exists.**

### Two of the Phase-2 questions are already answered

ADR-054 was written for package generation and settled two things this epic
would otherwise have to decide from scratch:

| Question | Answered by ADR-054 |
|---|---|
| **Does the STF belong to the Study, or to the generated package?** | **Neither stores it.** It is a *projection* over the placements in one sequence that belong to one study — ADR-049's deletion test, applied to a file that needs facts the submission does not hold. The answer is to hold the facts, not the file |
| **Does lifecycle operate on Study identity or on documents?** | **On the pair.** The mapping is `(study, eCTD element) → STF`, not `study → STF`, because *"one study could generate more than one STF representation"* (E29 §VI). Its `new`/`append` chain is derived the way ADR-045 derives a document's operation, keyed differently |

### What the Study ADR must still answer — **answered by [ADR-056](../../adr/ADR-056-study-identity-is-owned-by-the-sponsor.md), 2026-08-03**

*The founder's list, 2026-08-03. It is broader than STF, which is why it does not
belong inside ADR-054 and gets its own number. Kept as written, because what the
questions were is part of why the answers are what they are.*

1. **Is `Study` an aggregate?** — Phase 2 §1 leans two aggregates, and E29 gives
   the first external reason: clinical and non-clinical carry *different*
   STF categories (species / route / duration / type-of-control apply to
   4.2.3.1, 4.2.3.2, 4.2.3.4.1 and 5.3.5.1 only).
2. **Is it an entity owned by `Submission`?** — E29 says no: the `study-id` is
   *"the internal alphanumeric code used by the sponsor"*, stable across
   sequences, and **E24** says an instance qualifier must be identical across
   sequences or FDA's tooling loses continuity. A per-submission entity cannot
   promise that.
3. **Does a document reference a Study, or does a *placement*?** — the sharper
   form of Phase 2 §2, and **[ADR-053](../../adr/ADR-053-instance-qualifiers-belong-to-the-placement.md)
   has already answered its sibling**: the `file-tag` — what role a document
   plays in a study report — belongs to the placement. Whether the *study* link
   sits at the same level is the open half.
4. **Where does it live?** — Phase 2 §3 leans a new `src/Study/`. A new bounded
   context needs an ADR before code either way (repository canon).

> **This is the first ADR that starts defining the clinical/non-clinical
> information model rather than package generation**, and that is the reason to
> take it deliberately rather than as a sub-clause of an eCTD story.

---

## Phase 2 — **approved 2026-08-03** · in flight

*The sketch below this section predates EPIC-007a and is superseded where the two
disagree. Four decisions, all four signed off, and the first is
**[ADR-056](../../adr/ADR-056-study-identity-is-owned-by-the-sponsor.md)** —
written before any code, per canon.*

> **Two strengthenings came with the sign-off, and both are in ADR-056.**
>
> 1. The ownership argument is *"study identity is owned by the sponsor, not by a
>    submission"* — not *"four contexts reference it"*, which is corroboration.
> 2. **What a `Study` may become is governed, not left open**: *additional
>    attributes are admitted only when required by an external regulatory
>    workflow or a demonstrated business capability.* Written so that no later
>    story can say *"RIM lists 19 more fields"* and have that count as a reason.

### 1. Where does a `Study` live? — **a new `src/Study/` context** ([ADR-056](../../adr/ADR-056-study-identity-is-owned-by-the-sponsor.md))

**Four contexts will cite a study**: `RegulatoryApplication`, `Submission`
(placements), `Registration` (RIM's `License → Clinical Study`) and `Interaction`
(post-marketing commitments). Parking it inside any one of them points three
dependencies the wrong way.

**And it is not Submission's.** A study exists whether or not anything has been
filed about it — the sponsor's `study-id` is *"the internal alphanumeric code
used by the sponsor"* (E29), stable across sequences, and **E24 requires it to be
identical across sequences or FDA's review tooling loses continuity**. An entity
owned by a submission cannot promise that.

> **Canon requires an ADR before code for a new bounded context**, and this is
> the decision ADR-054 deliberately left open. **ADR-056.**

### 2. One aggregate or two? — **two**, and EPIC-007a supplies the first external reason

The sketch leaned two on internal grounds (RIM has two sheets; they will
diverge). **E29 adds a reason from outside RegOS**: the STF's `category` — species,
route, duration, type of control — applies to **4.2.3.1, 4.2.3.2, 4.2.3.4.1** and
**5.3.5.1**. Three nonclinical sections and one clinical, and the values a
regulator expects differ by kind.

**Cost: real duplication, and it is accepted rather than unnoticed.** ADR-018
permits merging on a third demonstrated need; two sheets and one shared
category vocabulary is not that.

> **For S001's minimum the two aggregates differ by almost nothing but their
> type, and that is fine.** The separation exists because the domain differs,
> not because today's properties differ — so neither gets a shared base class, a
> `StudyKind` discriminator, or an abstraction invented to hold the duplication.
> ADR-056 §2.

### 3. What links a placement to a study? — **the placement, not the document**

A `ProductDocument` can be filed in two sequences and report the same study both
times; what changes is the *placement*. So `SubmissionDocument` gains **two**
facts, and this is **[ADR-053](../../adr/ADR-053-instance-qualifiers-belong-to-the-placement.md)'s
third shape arriving with its vocabulary in hand**:

| | |
|---|---|
| `StudyId?` | which study this placement reports |
| `FileTag?` | what role it plays in that study's report — ICH's ~40 values, **held** |

**Both nullable, and null means the ordinary thing**: a placement outside 4.2.x
and 5.3.1.x–5.3.5.x reports no study. Generation refuses only where FDA requires
an STF, which is the refusal that already exists.

### 4. Scope — **the STF minimum first, the registry second**

**One epic, sequenced so the blocking half lands first.** Splitting into 019a and
019b would spend an epic number on a sequencing decision.

| | Story | Unblocks |
|---|---|---|
| **S001** ✅ | **`Study`** — a new context, `study-id` + `title`, two aggregates, persistence, API, minimal UI | nothing yet |
| **S002** | **placement → study + `file-tag`** on `SubmissionDocument`, with the UI to set them | nothing yet |
| **S003** | **STF generation** — the projection ADR-054 describes, `stf-<study-id>.xml`, `append` chains derived like ADR-045's delta | **Module 4. The epic's reason to exist** |
| **S004** | citation from `RegulatoryApplication`, both directions queryable | driver A |
| **S005** | the RIM attributes a real user asks for, and no more | driver A |

> **S003 is where this epic is worth its cost**, and S001–S002 are the two facts
> it needs. If work stops after S003, RegOS can file an IND — which it cannot do
> today.

**What S003 cannot claim**: `ich-stf-v2-2.dtd` is not held, so the STF is
generated and **not validated**. S007's per-package Level 2a covers two files of
three, and that sentence goes in the epic rather than being discovered later.

---

## S001 — `Study` · ✅ **shipped 2026-08-03**

A new `src/Study/` context: two aggregates, two identities, two facts each.
Domain → persistence → API → registry UI, per
**[ADR-056](../../adr/ADR-056-study-identity-is-owned-by-the-sponsor.md)**.
The model is written up in
[docs/domain-model/study.md](../../domain-model/study.md).

| | |
|---|---|
| Domain | `ClinicalStudy` · `NonClinicalStudy`, `AggregateRoot<TId>` with `sealed class …Id : StronglyTypedId` (ADR-043) |
| Application | `RegisterClinicalStudy` · `RegisterNonClinicalStudy` · `ListStudies` · `ISponsorStudyIdentifierPolicy` |
| Persistence | two tables, two fail-closed tenant filters (ADR-031), `AddStudyRegistry` |
| API | `POST /api/studies/clinical` · `POST /api/studies/nonclinical` · `GET /api/studies` |
| UI | `/regulatory/studies` — a sibling of Products, because a study exists whether or not anything has been filed |

### Two decisions this story made that the ADR had left open

**1. Two identities, not one.** ADR-056 §2 forbids an abstraction invented to
hold the duplication, and a shared `StudyId` is exactly that — an identity space
neither aggregate owns, which is the supertype
[ADR-040 §3](../../adr/ADR-040-the-health-authority-interaction-context.md)
declined to build. Corroborated by the consumer: the STF's `category` vocabulary
is kind-specific, so a typed reference tells S003 what it is holding instead of
making it probe two tables. **It puts an exclusive-or on the placement, and that
is S002's to model explicitly.**

**2. One sponsor study identifier names one study, across both kinds.** ADR-056
required that whichever uniqueness rule was chosen got a test. This is the rule,
and the reason is external: **E24** says FDA's tooling recognises a study by its
`study-id`, so two studies sharing one are shown to a reviewer as **one** — the
STF writes `<study-id>` and no kind marker. A unique index covers one table, so
the rule lives in a policy with the two indexes closing the race beneath it.

### What it deliberately does not have

No status (nothing deletes a study, so ES-018 has nothing to say), no format rule
on the identifier (EPIC-007a settled that an authority's format check belongs at
the boundary — S003's), and none of RIM's other ~21 attributes.
`AStudy_HasNoLifecycle_BecauseNothingRetiresOne` asserts the property list
exactly, so admitting the next attribute is a decision someone makes rather than
a column that appears.

### Owed to S002

**`Retitle` is unguarded, and that becomes wrong the moment a placement can cite
a study.** E24 makes the *title* part of what FDA matches on too, so renaming a
study named in a published sequence would split it in two in the reviewer's
tool. Nothing can cite a study yet, so there is no such sequence to protect —
recorded here and in the aggregate rather than discovered later.

### Verification

18 test suites, **1,152 tests**, 0 failures (16 new). **94 browser specs**, 0
failures (2 new), on an isolated stack. `study-registry.spec.ts` proves the
cross-kind refusal through the browser and the trimming through the API — the
half a unique index could not catch.

---

## Phase 2 — Domain design *(sketch — predates EPIC-007a, superseded above where they disagree)*

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
