# EPIC-004 — Sequences & submission lifecycle

**Status:** ⚪ Not Started · **Branch:** `epic/EPIC-004-sequences-and-submission-lifecycle` (cut at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

A submission today is a titled bag of placed documents with two states. A **real** submission is sequence `0003` in eCTD format against DTD 3.2, where each leaf declares whether it is **new, replaced, appended or deleted** relative to what was filed before. This closes the gap.

> **Phase 1 below is settled.** **Phases 2–3 are a sketch**, written so this epic can be picked up months from now without re-deriving it — they are **not approved design**. Confirm, amend or replace them in the Phase-2 conversation when this epic is pulled into **Now**.

---

## Phase 1 — Epic plan

### Outcome
A submission carries its real regulatory identity — **sequence number, format, DTD versions** — and every piece of content declares its **eCTD lifecycle operation** against the previously published sequence. A regulatory user can see the lifecycle of one document across an application's whole filing history, and the submission's own state stops being a two-value enum.

### The RIM gap this closes

`Submission` is RegOS's **weakest mapped object — 4 of RIM's 30 attributes (13%)**. `Submission Content` sits at 4 of 18 (21%). The missing pieces cluster into four groups:

| Group | RIM attributes |
|---|---|
| **Identity** | Submission Number, Submission Format, DTD Version (ICH / Regional / STF), Gateway Format, Submission Sub-Type, Submission Country (Multiple) |
| **Lifecycle operation** | Submission Content **Operation** (new / replace / append / delete) — *the eCTD core, and RegOS has no equivalent at all* |
| **Two-sided status** | Submission Status + date **vs** HA Submission Status + date — what we did, vs what they said |
| **Operational pipelines** | QC Status, Publishing Status, Compilation Status, Validation Status — each with its own date; plus Reason for Delay |

[ADR-036](../../adr/ADR-036-the-dossier-is-structure-placeholders-are-validation.md) already gives us the thing that makes the operation derivable: **a sequence is a diff of placements**, not an inference over files.

### In scope ✅
- **Sequence numbering** — submissions are numbered within their application (`0000`, `0001`, …), assigned by the domain, never accepted from outside.
- **Submission identity** — format (eCTD / NeeS / paper), DTD versions (ICH, Regional, STF), gateway format, sub-type, submission countries.
- **Content operation** — each `SubmissionDocument` carries new / replace / append / delete, **derived from the placement diff** against the previous published sequence and frozen at publish.
- **Two-sided status** — our submission status and the authority's, each dated and historied, so "we filed it" and "they acknowledged it" are never conflated.
- **Lifecycle beyond Draft/Published** — the states a submission legitimately passes through, with the transitions the domain permits.
- **Submission Role** — named contacts with roles on a submission (depends on EPIC-016's `Contact`).
- **Document lifecycle view** — one document's history across an application's sequences.
- Browser proof, ADR.

### Out of scope ⏸️ (deferred, with reason)
| Deferred | Why |
|---|---|
| **Actual eCTD package generation** (XML backbone, folder structure, checksums, MD5) | That is the publishing engine → **EPIC-007**. This epic makes the *data* correct; EPIC-007 renders it. |
| **Gateway transmission (ESG/AS2)** | Infrastructure with its own compliance surface → EPIC-007/015. |
| **QC / Publishing / Compilation / Validation status pipelines** | See Phase-2 decision 3 — these are *internal operational workflow*, not regulatory fact, and they belong with review & approval → **EPIC-008**. Model the columns only if the decision goes the other way. |
| **Re-binding an in-flight submission to a newer template version** | Carried forward unresolved from EPIC-002; still needs a policy conversation, and it is not blocking here. |
| **Process Step linkage** | → **EPIC-020**. Nullable seam only. |
| **Cross-sequence document reuse / leaf reuse across sections** | Deliberately deferred in `SubmissionDocument` already; the migration is one row per placement, no inference. Do not pull it in here unless a story needs it. |

### Definition of Done
- A submission created under an application receives the next sequence number from the domain, and the numbering is proven contiguous under concurrent creation.
- A submission records its format and applicable DTD versions.
- Publishing a submission **computes and freezes** each document's operation against the previous published sequence: unchanged placements ⇒ no operation emitted, changed version ⇒ `replace`, new placement ⇒ `new`, removed placement ⇒ `delete`.
- The first sequence in an application yields all `new` — asserted, so the empty-baseline case is not an accident.
- Our status and the authority's status are separately recorded, each with a dated history.
- A user can see one document's lifecycle across every sequence of an application.
- Browser proof: publish sequence 0000 → replace one document → publish 0001 → the diff shows exactly one `replace` and everything else absent.
- ADR written for sequence numbering and operation derivation.

---

## Phase 2 — Domain design *(sketch — not approved)*

### Entities

**`Submission` (extended)**

| Field | Notes |
|---|---|
| `SequenceNumber` | `int`, assigned by the application-scoped numbering rule; rendered zero-padded to 4 |
| `Format` | eCTD / NeeS / paper |
| `DtdVersionIch?`, `DtdVersionRegional?`, `DtdVersionStf?` | strings |
| `GatewayFormat?` | |
| `SubTypeId?` | |
| `SubmissionCountries` | collection — RIM: Multiple (matters for EU centralised/mutual-recognition) |
| `HaStatus`, `HaStatusDate` | the authority's view, distinct from `Status` |
| `ReasonForDelay?` | RIM: free text, when actual is later than planned |
| status history | dated, both sides |

**`SubmissionDocument` (extended)** — `Operation` (`New` / `Replace` / `Append` / `Delete`), nullable while draft, frozen at publish.

**`SubmissionRole`** — new child: `SubmissionId`, `ContactId`, `Role`. Depends on EPIC-016.

### Decisions to settle (Phase 2, on pull-in)

**1. Sequence numbering scope and assignment.** *Lean: scoped to the `RegulatoryApplication`, assigned by the domain*, mirroring how `RegulatoryTemplate` assigns version numbers and `ProductDocument` assigns version numbers — *"the aggregate owns numbering; a number is never accepted from the outside."* The wrinkle: `Submission` is a root, not a child of Application, so the numbering authority sits **outside** the aggregate that consumes it. Needs either a unique constraint on (application, sequence) plus retry, or a numbering service. **Settle this — it is the one genuine concurrency question in the epic.**

**2. Operation: derived or stored?** *Lean: derived at publish, then frozen* into the submission (and its snapshot). It is a **fact about what was sent** — recomputing it later against mutated data would rewrite history, which the snapshot model already refuses to do. Draft submissions may show a *provisional* operation computed live.

**3. The four operational pipelines (QC / Publishing / Compilation / Validation).** *Lean: out of scope here.* They describe an internal production workflow — who checked it, who published it — not a regulatory fact about the filing. They belong with review & approval (**EPIC-008**). Recorded so the omission reads as a decision, not an oversight.

**4. What the lifecycle states actually are.** RIM leaves `Submission Status` as a controlled list. Candidate set: `Draft → InPreparation → ReadyToSubmit → Submitted → Acknowledged → UnderReview → Approved / Rejected / Withdrawn`. Follow EPIC-005's line — **a closed enum in code, because it drives behaviour** — and settle terminality and permitted transitions in the story, not here.

**5. `Append` may not be needed.** eCTD defines it, but many regions do not use it. Include the enum value; do not build derivation for it until a seeded blueprint demands it.

**6. The diff needs a "previous published sequence" concept.** Define it precisely: the highest sequence number in the same application with status published *and* a `PublishedAt` earlier than this one. Write the definition down — it is the kind of thing that silently rots.

### Change-case analysis

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| eCTD package generation reads these fields (EPIC-007) | **High** | Operation and sequence are the two things the backbone XML needs; storing them is the seam |
| Region-specific sequence rules (EU vs FDA numbering) | **High** | Numbering is a domain rule, not a database default — a policy object can vary it |
| Leaf reuse across sections | Medium | Already a known deferral with a clean migration path |
| A sequence is withdrawn or replaced wholesale | Medium | Lifecycle states model it; the diff base is defined by rule (decision 6), not by "the previous row" |
| Multi-country submissions (EU) | Medium | `SubmissionCountries` is a collection from day one |
| Operation needed for a *draft* preview | Medium | Derivation is a service; draft calls it live, publish freezes the result |

---

## Phase 3 — Candidate stories *(sketch — re-slice on pull-in)*

| # | Story | Slice |
|---|---|---|
| **S001** | **Sequence numbering** — domain-assigned, application-scoped, concurrency-proven; sequence shown wherever a submission is | domain → persistence → API → UI → test |
| **S002** | **Submission identity** — format, DTD versions (ICH/Regional/STF), gateway format, sub-type, submission countries | full slice |
| **S003** | **Content operation** — derive from the placement diff, freeze at publish; first-sequence-is-all-new asserted | full slice |
| **S004** | **Two-sided lifecycle** — our status and the authority's, each dated and historied, with permitted transitions | full slice |
| **S005** | **`SubmissionRole`** — named contacts with roles on a submission *(needs EPIC-016)* | full slice |
| **S006** | **Capstone** — one document's lifecycle across an application's sequences, browser proof of publish → replace → publish, ADR, retro | UI → test → docs |

**ADR to write:** *Sequence numbering and eCTD operation derivation* — next free number.

**Sequencing note:** this epic and **EPIC-017** are genuinely independent — sequences live inside `Submission` and never touch `ProductId`; EPIC-017 never touches submission internals. Order is a **value call**, not a dependency: EPIC-017 completes an epic already in flight (EPIC-005); this one completes nothing in flight but may be what a customer is waiting on. **S005 does depend on EPIC-016** and can be dropped or deferred if this runs first.
