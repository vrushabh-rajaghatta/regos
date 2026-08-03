# EPIC-019 — Study registry

**Status:** 🟢 **S001–S004 shipped 2026-08-03** — Module 4 generates, so RegOS can file an IND. One item owed: the E24 continuity refusal (ADR-057 §2) · **Branch:** `epic/EPIC-019-study-registry` · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

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
| **`file-tag` per placement** | what role the document plays — synopsis, protocol, CRF. ✅ **Held since 2026-08-03**: `valid-values.xml` v6.0, **97 values** across `ich` / `us` / `jp` (**E33**) |
| **`ich-stf-v2-2.dtd`** | ✅ **Held since 2026-08-03.** An STF can now be written *and* validated — but see below: the DTD does not check the vocabulary |

**Both gaps closed on 2026-08-03**, and closing them corrected the entry above
them: this table named the DTD as the source of the `file-tag` list. It is not.
`file-tag/@name` is `CDATA`, so `xmllint` accepts `name="sinopsis"` without
complaint. The vocabulary lives in `valid-values.xml`, and **the ICH stylesheet
is what checks it** — painting unknown values red (**E34**). Two files, two
jobs.

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
| `FileTag?` | what role it plays in that study's report. ⚠ **The vocabulary is not held** — corrected 2026-08-03, and this is why S002 shipped without it |

**Both nullable, and null means the ordinary thing**: a placement outside 4.2.x
and 5.3.1.x–5.3.5.x reports no study. Generation refuses only where FDA requires
an STF, which is the refusal that already exists.

### 4. Scope — **the STF minimum first, the registry second**

**One epic, sequenced so the blocking half lands first.** Splitting into 019a and
019b would spend an epic number on a sequencing decision.

| | Story | Unblocks |
|---|---|---|
| **S001** ✅ | **`Study`** — a new context, `study-id` + `title`, two aggregates, persistence, API, minimal UI | nothing yet |
| **S002** ✅ | **placement → study** on `SubmissionDocument`, with the UI to set it | nothing yet |
| **S002b** ✅ | **`file-tag` per placement** — 97 values, realm-scoped, non-words refused | S003 |
| **S003** ✅ | **STF generation** — the projection ADR-054 describes, `stf-<study-id>.xml`, `append` chains derived like ADR-045's delta | **Module 4. The epic's reason to exist** |
| **S004** ✅ | citation from `RegulatoryApplication`, both directions queryable | **driver A — done** |
| **S005** ⬜ | the RIM attributes a real user asks for, and no more | **nothing to build: nobody has asked** |

> **S002 was planned as study + `file-tag` and shipped as study alone.** The
> `file-tag` vocabulary turned out **not to be held** — the plan said it was, and
> the plan was wrong ([correction](../../evidence/README.md#correction-2026-08-03--the-file-tag-vocabulary-is-not-held)).
> Every `file-tag` element in an STF carries a `name` from a closed ICH list we
> cannot enumerate, so S003 cannot write a valid STF and **S002b and S003 are
> both blocked on a document, not on work.**
>
> **Unblocked 2026-08-03 — by three files, not one.** The prediction that
> `ich-stf-v2-2.dtd` *"carries the enumeration in its `ATTLIST`"* was wrong:
> `file-tag/@name` is `CDATA`. `valid-values.xml` holds the 97 values (**E33**),
> the DTD validates structure (**E35**), and the stylesheet is the only thing
> that checks a tag is a real word (**E34**). All three are held at
> [`docs/evidence/EPIC-019/spec/`](../../evidence/EPIC-019/spec/).

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

## S002 — the placement reports a study · ✅ **shipped 2026-08-03**

`SubmissionDocument` gains a typed reference to the study its **placement**
reports, set from the content plan — the screen a document is filed on.

| | |
|---|---|
| Domain | `ClinicalStudyId?` + `NonClinicalStudyId?` on the placement; `ReportClinicalStudy` / `ReportNonClinicalStudy` / `ClearReportedStudy` on the aggregate |
| Application | `ReportStudyOnPlacement`, and the content plan now carries the study on every placed document |
| Persistence | two nullable FKs, `Restrict` on both, indexed for S003's grouping; `AddStudyToPlacement` |
| API | `PUT /api/submissions/{id}/documents/{documentId}/study` — a sibling of `/placement`, because both are facts about where the document sits |
| UI | a **Set study** control on every placed document, and the sponsor's code shown beside it |

### The exclusive-or is structural, not checked

Each writer clears the other, so **no sequence of calls produces a placement
reporting two studies**. The handler refuses a request naming both rather than
resolving it — a caller naming two studies has a bug, and picking one would file
the document under a study nobody chose.

### Two consequences of "a fact about the placement", made real

Both were prose in ADR-056. They are now behaviour, and both are covered in the
browser as well as the domain:

| | |
|---|---|
| **Unplaced reports nothing** | `ClearPlacement` takes the study with it, and an unplaced document is refused a study by name. Otherwise the reference outlives the placement it describes |
| **Moving keeps it** | the same document reporting the same study from 4.2.1 or 4.2.3 is ordinary; moving it is not a statement about which study it reports |

### What S002 found on the way

**A document filed in a section that expects a different type had no controls at
all** — it rendered as a sentence, *"Also filed here: …"*. That is precisely the
shape a study report's supporting files take in 4.2.x, so the study could never
be named on them and Module 4 would have been unfinishable through the UI. They
now carry the same per-document controls as required content.

### The `Retitle` guard S001 owed — and why it is not a guard

S001 recorded that `Retitle` becomes unsafe once a placement can cite a study,
and that it would need `ApplicationNumberPolicy`'s shape. **On inspection that
answer was wrong, and the better one costs nothing.**

A guard on `Study` would have to ask *"has any published sequence cited me?"* —
which points `Study` at `Submission` and inverts
[ADR-056 §4](../../adr/ADR-056-study-identity-is-owned-by-the-sponsor.md)'s
dependency direction for a rule neither context wants to own.

> **The project already has the right instrument: freeze at publish.**
> [ADR-047](../../adr/ADR-047-publication-metadata-exists-only-when-publication-makes-it-true.md)
> establishes that what a filing said is frozen when it is filed. An STF's
> `study-identifier` is part of what was filed, so **S003 freezes the identifier
> and title into the published placement** — after which a retitle cannot alter
> a filed STF.

#### Freezing is half of it — corrected on re-reading E24

**Freezing solves regeneration, not continuity, and those are two problems.**
FDA's TCG §4.4 names the failure explicitly: a duplicated study is *"caused by
an updated STF being submitted with incorrect metadata (**study-id and study
title not an exact match**)"*. So the title is part of the key FDA's tooling
matches on, and a retitle still drifts **forward**: sequence 0000 filed under
one title, 0001 under another, and the reviewer sees two studies. A frozen
snapshot keeps the old package honest and does nothing about the new one.

**It is still not a guard on `Study`, and the reason is better than the first
one.** The check belongs where every other EPIC-007a refusal lives — in the
generator, which already reads the previous published sequence to derive
`operation` ([ADR-045](../../adr/ADR-045-the-cumulative-dossier-and-the-derived-delta.md)).
It compares this sequence's `study-identifier` against the frozen one from the
last sequence that filed the same study and **refuses by name**. That reads
`Submission`'s own frozen data, adds no dependency in any direction, and puts
the authority's rule at the boundary that faces the authority.

| | |
|---|---|
| **Freeze at publish** | a filed STF cannot change when regenerated (ADR-047's instrument) |
| **Generator refusal** | a later sequence cannot file the same study under a different title (E24's rule, at the boundary) |

**ICH and FDA do differ here**, and it is worth recording rather than
smoothing over: ICH §V says *"the information contained in the study-identifier
section of the most recent STF will be deemed the most current"* — a supersede
mechanism — while FDA describes the same act as producing a duplicate. Both are
level 3. **The receiving authority governs**, and the first vertical is
US·FDA·IND, so RegOS takes the stricter reading.

**`Retitle` is therefore unreachable today** — the aggregate has it, nothing
calls it, and no screen offers it. Recorded rather than quietly left: it becomes
reachable when S003 has built both halves.

> **This wants [ADR-057](../../adr/) once S003 unblocks** — *an authority's
> cross-sequence continuity rule is enforced at the artifact boundary using
> frozen publication facts, never by a guard that inverts a context boundary*.
> Not written yet, because it would be recording a decision about code that
> cannot be written until the DTD arrives.

### Verification

18 suites, **1,160 tests**, 0 failures (8 new). **96 browser specs**, 0 failures
(2 new), on an isolated stack.

---

## S004 — which studies support a filing · ✅ **shipped 2026-08-03**

Driver A, the question this epic was originally scoped for, and its inverse.

| | |
|---|---|
| Domain | `ApplicationStudyCitation`, a child of `RegulatoryApplication`; `CiteClinicalStudy` / `CiteNonClinicalStudy` / `StopCitingStudy` |
| Application | `CiteStudy` · `StopCitingStudy` · `ListApplicationStudies` · **`ListStudyFilings`** |
| Persistence | `ApplicationStudyCitations`, cascade from the application, two filtered unique indexes |
| API | `GET/POST /api/applications/{id}/studies`, `DELETE …/studies/{studyId}`, `GET /api/studies/{id}/filings` |
| UI | a **Studies** tab in the application workspace, and *"Where is it filed?"* on each registry row |

### A child of the citing application, not a join aggregate

The Phase-1 sketch leaned toward a join aggregate *"owned by neither side,
because both directions are queried and neither is the natural owner"*. **The
second clause is false.** A citation is a claim the *application* makes —
nothing about the study changes when a filing cites it, and withdrawing one is
the application changing its mind.

And a join aggregate would be built for a third citer that does not exist.
`Registration → Clinical Study` and a commitment's study are both plausible and
neither has been asked for; **[ADR-056](../../adr/ADR-056-study-identity-is-owned-by-the-sponsor.md)'s
revisit trigger names exactly that moment**, and it has not fired.

### Where the inverse query lives, and why it is not obvious

*"Which filings cite this study?"* spans three contexts. It cannot live beside
`Study` — that would give the registry a dependency on both its citers, the
inversion ADR-056 §4 exists to prevent. It cannot live in
`RegulatoryApplication` either: reaching `Submission` from there would **close a
cycle**, since `Submission` already depends on it.

> **It lives in `Submission.Application`, the only context that already sees
> both.** ADR-039 principle 7 at its plainest: a real question spanning three
> contexts is a read, and a read grants nobody write ownership.

Two kinds of answer, labelled, because neither implies the other: an
**Application** cites a study, and a **Sequence** carries a placement reporting
one. A study can be cited before anything is filed about it, and a sequence can
report one the application never cited.

### What S004 found

**The repository did not `Include` the new collection**, so the aggregate's
idempotence check read an empty list and cited the same study twice. **The
unique index caught it** — which is the belt-and-braces pattern doing precisely
the job S001 claimed for it, on the first occasion it was tested.

### Verification

18 suites, **1,167 tests**, 0 failures (7 new). **98 browser specs**, 0 failures
(2 new), on an isolated stack. `study-citation.spec.ts` asserts the two
directions **against each other**: a citation visible on one screen and not the
other is the failure worth catching, and neither assertion alone would see it.

---

## What the STF documents changed — read 2026-08-03

*Three files arrived and were read before anything was built on them. Five
findings, and the first two correct this epic's own plan.*

### 1. The vocabulary is not in the DTD, and it is 97 values

`file-tag/@name` is `CDATA #REQUIRED`. `xmllint --valid` **accepts
`name="sinopsis"`** — verified, not assumed. The enumeration lives in
`valid-values.xml`, exactly where the DTD's own comment points: *"the list of
valid values for the name is controlled by the ICH default stylesheet"*.

| Realm | `file-tag` values |
|---|---|
| `ich` | 68 |
| `us` | 25 |
| `jp` | 4 |
| | **97** |

E29's *"~40 values"* is superseded (**E33**). And **FDA's *"22 controlled file
tags"* (E21) turns out to be the `us` realm** — the 25 today are those 22 plus
three `HF-validation-*` added in v5.0. Two summaries, one list, and neither
summary was the list.

### 2. There is a second oracle, and it covers what the first cannot

EPIC-007a's Level 2a rested on one parser answering *"is this legal?"*. The
stylesheet answers a different question — ***"is this a word?"*** — by resolving
every `file-tag`, `category` and `property` against `valid-values.xml` and
painting unknown ones `#FF6666`.

```
xmllint  + ich-stf-v2-2.dtd                  → structure        (E35)
xsltproc + stylesheet + valid-values.xml     → vocabulary       (E34)
```

Measured: `sinopsis` → **1 red row**; `synopsis` → **0**. It is third-party,
machine-checkable, and shipped by ICH — so S003 can claim vocabulary
correctness with an oracle rather than with its own opinion. **The rule
`ValidatorIndependenceTests` enforces extends unchanged**: this is a second
oracle at the same seam, not a dependency.

### 3. `duration` is a `us` category, not an `ich` one

Realms are **per category**, not uniformly `ich`:

| Category | Values | Realm |
|---|---|---|
| `species` | 9 | `ich` |
| `route-of-admin` | 8 | `ich` |
| **`duration`** | 3 | **`us`** |
| `type-of-control` | 5 | `ich` |
| `property` → `site-identifier` | 1 | `us` |

Emitting `duration` with `info-type="ich"` produces a file the DTD accepts and
the stylesheet flags — measured, 1 red row. **A hard-coded `info-type="ich"`
would be wrong for one category in four.**

### 4. Element order is fixed, and two constructs were not modelled

`study-identifier` is `(title, study-id, category*)` — **title first**;
`study-id` before `title` is rejected. Also present and previously unrecorded:
`doc-content` has an optional `title?`; `file-tag` may itself contain
`property*`; and `content-block` is a hierarchical alternative to `doc-content`
for multiple tags per file. The minimum needs none of the last three, but S003
should know they exist before choosing not to use them.

### 5. `util/style/` needs two files, not one

The stylesheet reads `document('valid-values.xml')` by a **relative** path, so
the vocabulary ships beside it. ADR-054 recorded `util/style/` as an absent
folder; this is what goes in it.

### What is still not held

**Which `file-tag` belongs on which document.** RegOS now has the words and can
refuse a non-word; it has no rule for choosing between `synopsis` and
`study-report-body` for a given file, and **should not invent one** — that is
the filer's judgement, and the closest thing to a rule is FDA regional guidance
this repository does not hold. S002b's UI therefore offers the list and records
the choice.

---

## S002b — what the placement contributes · ✅ **shipped 2026-08-03**

A placement records **which study, in what role** — ICH's `file-tag`.

| | |
|---|---|
| Vocabulary | `FileTagVocabulary` — 97 values with their realms, **transcribed by parsing the held file, never by hand** |
| Domain | `SubmissionDocument.FileTag`; both study writers take it, because the tag and the study are one fact |
| Application | the handler refuses a tag ICH does not publish, and one with no study |
| API | `PUT …/documents/{id}/study` carries it; `GET /api/study-tagging/file-tags` serves the list |
| UI | a second select in the same dialog, appearing only once a study is named |

### The vocabulary is a table in code, not seeded reference data

It looks like every other lookup in RegOS and is deliberately not one.
**ADR-055's promotion test fails for the list as a whole**:
`data-tabulation-dataset-sdtm` and `HF-validation-protocol` do not name anything
that would exist if ICH did not. They are wire tokens, and this project already
holds wire tokens in code — `TelephoneNumberTypes`, `ApplicantContactTypes`.
Nobody curates this list; it changes when ICH republishes.

**What replaces the seed is the same check the seed would have got.**
`FileTagVocabularyTests` parses `valid-values.xml` and compares — the move
`FdaWireVocabularyTests` makes for `application-type`, pointed at a table rather
than at a database.

### One column, because the realm is a function of the tag

All 97 values are distinct across `ich`, `us` and `jp`, so a placement stores
the tag and derives `info-type` from it. A second column could only ever
disagree with the first. **The test asserts that uniqueness** — if ICH ever
publishes one value in two realms, the derivation starts lying silently, and
that is the test that notices.

### Refused where the list lives

The aggregate takes the token as written; the handler checks it. The same
division `RecordApplicationNumber` draws, and here it earns itself twice over:

| | |
|---|---|
| `sinopsis` | **refused by name** — one keystroke from valid, accepted by the DTD (**E34**), unrecognised by a reviewer's tool |
| a tag with no study | **refused** — a `file-tag` exists inside an STF, and an STF exists for a study |

Both consequences follow through the aggregate too: naming a different study
drops the role, because it was a role in the *previous* study's report; and
unplacing the document clears both.

### What it does not do

**It does not say which tag belongs on which document.** RegOS has the words and
can refuse a non-word; choosing between `synopsis` and `study-report-body` is
the filer's judgement, and the guidance that would narrow it is not held. The
picker shows the token as published — not prettified — because that is what the
STF writes and what a reviewer's tool matches on.

### Verification

18 suites, **1,176 tests**, 0 failures (9 new). **99 browser specs**, 0 failures
(1 new), on an isolated stack.

---

## S003 — the Study Tagging File · ✅ **shipped 2026-08-03**

**The story this epic exists for.** A document in CTD 4.2.x that names a study
now generates rather than refusing, so **RegOS can file an IND** — which it
could not do this morning.

| | |
|---|---|
| Freeze | `FiledStudyIdentifier` / `FiledStudyTitle` on the placement, written once at publish |
| Projection | `StudyTaggingFileRenderer` — one file per **(study, eCTD element)**, never per study |
| Chain | `new` then `append`, derived by asking which earlier sequence filed one |
| Package | `util/dtd/ich-stf-v2-2.dtd` and `util/style/` — the stylesheet **and** the vocabulary it reads |
| Decision | **[ADR-057](../../adr/ADR-057-a-filed-artifact-is-projected-from-a-snapshot.md)** |

### The freeze boundary

```
Study (mutable)
      │
      ▼
Publication            ← the snapshot is taken here, once
      │
      ▼
Frozen STF projection  ← what this sequence said the study was
      │
      ▼
XML
```

The renderer never touches the `Study` tables — not by discipline, but because
the plan it is handed carries no study id it could look up with.
`RenamingAStudyAfterFiling_DoesNotChangeWhatTheSequenceSaid` asserts it the only
way that means anything: generate, rename, regenerate, compare bytes.

### Both oracles, and why one was not enough

| Oracle | Question | Verdict on `sinopsis` |
|---|---|---|
| `xmllint` + `ich-stf-v2-2.dtd` | is this **legal**? | **valid** |
| `xsltproc` + stylesheet + `valid-values.xml` | is this **a word**? | **1 red row** |

`AMisspelledFileTag_PassesTheDtd_AndTheStylesheetCatchesIt` asserts both halves
in one test, because the interesting claim is the *gap between them*.

### What the oracles found, immediately

**The first STF ever written was invalid twice**, and neither defect was visible
in the XML:

| | |
|---|---|
| `encoding="utf-16"` | `XmlWriter` takes its declared encoding from the sink, and a `StringBuilder` is UTF-16 — so the file announced one encoding while the bytes on disk were another |
| a DOCTYPE that resolved to nothing | the path to `util/dtd/` was hardcoded two levels up, and an STF sits **with the study's files** — three levels down for 4.2.3 |

Both would have produced a package a reviewer's tool could not open. Both were
found by running the parser, not by reading the code.

### Three refusals, in three different categories

| | Category | Why |
|---|---|---|
| a study-report document naming no study | **data completeness** | a user fixes it on the content plan — this refusal *changed category* in S003, from a capability gap to a missing fact |
| a placement in **4.2.3.1, 4.2.3.2, 4.2.3.4.1 or 5.3.5.1** | **domain capability** | ICH requires species, route, duration and type-of-control there, and a `Study` holds none. `category*` is optional in the DTD, so an empty one would *validate* and tell a reviewer nothing (E23's shape again) |
| a study identifier a filename cannot carry | **data completeness** | an STF is `stf-<study-id>.xml`, so the sponsor's code becomes a filename. **Refused, never slugged** — a slug puts a name in the package that is not the study's. The refusal S001 predicted when it declined to police the format in the domain |

And a fourth, for history: a sequence published before EPIC-019 has no snapshot,
and is **refused rather than back-filled from today's registry** — the same call
EPIC-007a made for sequences filed before regulatory activities were recorded.

### What S003 does not do

**The E24 continuity refusal is not implemented.** A study named in one
published sequence can still be renamed, and the next sequence will file the new
title — which FDA reads as two studies. ADR-057 §2 says where the check belongs
(the generator, from frozen columns, no new dependency) and why it is not here
yet: it needs a second sequence filing the same study, and a message that names
what the previous one said. **Recorded as owed rather than presented as safe.**

### Verification

18 suites, **1,179 tests**, 0 failures (3 new). Both oracles green on a
generated package. **100 browser specs**, 0 failures (1 new), on an isolated
stack.

`module-four-tagging.spec.ts` asks the question the backend tests cannot: **can
a user actually do it?** Worth asking separately, because this epic already
produced one defect only a browser could find — Module 4's supporting files
rendered with no controls, so the backend supported the workflow and the UI
quietly prevented finishing it.

Its closing assertion is deliberately **negative**. A green package needs facts
that spec does not set up — a DUNS, an application number, a reachable contact —
so success is not the claim. The claim is that **nothing is refused for want of
a study**, and that whatever refusal remains names something else.

---

## S005 — nothing to build

The story was *"the RIM attributes a real user asks for, and no more"*. **Nobody
has asked**, so by [ADR-056](../../adr/ADR-056-study-identity-is-owned-by-the-sponsor.md)
§3's own rule there is nothing here to write:

> *Additional attributes are admitted only when required by an external
> regulatory workflow or a demonstrated business capability.*

Building `phase`, `indication`, `therapeutic area` and `subject count` now would
be RIM's list arriving without a demand — the exact reasoning this epic was
re-scoped to reject. **The story stays open and empty**, which is a more useful
record than closing it: it is the place the next demand attaches to.

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
