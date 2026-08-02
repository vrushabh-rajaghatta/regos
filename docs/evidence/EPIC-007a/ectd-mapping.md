# eCTD v3.2.2 → RegOS mapping

**Status: draft, unvalidated.** Every row is a claim that the oracle can refute.
That is the point — this is an evidence-producing artifact, not a design
proposal, and it is written to be **falsifiable** rather than to be right.

## How to read the confidence column

| | |
|---|---|
| **spec** | confirmed against a cited FDA/ICH source |
| **model** | confirmed against the RegOS codebase |
| **assumed** | my reading of eCTD, **not yet checked against the specification** — the rows most likely to be wrong |

> **Nothing here is confirmed by a validator.** Task 1 has not produced a run.
> Until it does, this document's status is *"what we believe we must emit"*,
> which is Level 1 — and the epic is explicit that Level 1 is not evidence.

---

## 1. The finding that changes the model

FDA requires **two** attributes on every sequence, and they are independent
axes:

> *"A Submission type attribute is required for every sequence, and an
> additional attribute of submission-sub-type is required when utilizing M1 DTD
> v3.3."* — FDA eCTD Technical Conformance Guide

| | Answers | RegOS |
|---|---|---|
| `SubmissionType` (ours) | **what regulatory application is this?** — IND, NDA, BLA, 510(k) | ✔ seeded reference data |
| `submission-type` (eCTD) | **what kind of sequence is this?** — original-application, amendment, supplement, report | ✖ **no field** |
| `submission-sub-type` (eCTD) | **which stage within that?** — e.g. presubmission vs application | ✖ **no field** |

**This is [ADR-047 §6](../../adr/ADR-047-publication-metadata-exists-only-when-publication-makes-it-true.md)'s
deferred sub-type, and it resolves the question S004 refused to guess at.**

S004 could not choose between two readings — a *taxonomy* beneath
`SubmissionType`, or an *independent axis*. FDA answers it: `submission-type`
sits beside our `SubmissionType`, not beneath it, and `submission-sub-type` is a
third level beneath *that*. Neither is a refinement of what we already hold.

> S004's refusal was correct, and this is why: **had we guessed, we would have
> guessed the taxonomy** — the reading that puts a sub-type under
> `SubmissionType`. FDA's model is the other one.

FDA publishes the controlled vocabulary as a dedicated document
([eCTD Submission Types and Subtypes](https://www.fda.gov/media/111237/download)),
which makes this **reference data**, not an enum.

---

## 2. Backbone element → RegOS source

### `index.xml` — the ICH backbone

| eCTD | RegOS source | Confidence | |
|---|---|---|---|
| module elements `m1`…`m5` | `TemplateSection.Code` (`1.1`, `2.3`, `3.2.S.2`) | model | ✔ |
| `leaf/@operation` | `SubmissionDocument.Operation` — **frozen at publish**, never recomputed | model | ✔ |
| `leaf/@modified-file` | `SubmissionDocument.ReplacesSubmissionDocumentId` | model | ✔ |
| `leaf/@checksum`, `@checksum-type` | derived from `IFileStorage.OpenRead` (MD5) | model | ✔ |
| `leaf/@xlink:href` | derived — the path within the sequence folder | assumed | ⚠ |
| `leaf/title` | `ProductDocument` name | model | ✔ |
| `leaf/@ID` | **no stable source.** `SubmissionDocumentId` is per-submission; an eCTD leaf ID must be stable *across* sequences for `modified-file` to resolve | assumed | ✖ **open** |
| `@dtd-version` | constant for the pinned version | spec | ✔ |

### `us-regional.xml` — FDA Module 1

| eCTD | RegOS source | Confidence | |
|---|---|---|---|
| application number | `RegulatoryApplication.ApplicationNumber` | model | ⚠ **nullable, and no fixture sets one** |
| sequence number | `Submission.SequenceNumber` | model | ✔ |
| `submission-type` | — | spec | ✖ **gap** |
| `submission-sub-type` | — | spec | ✖ **gap** |
| `submission-id` | relates to sequence number, rules differ by regulatory activity | assumed | ⚠ |
| applicant / sponsor | `RegulatoryApplication.ApplicantOrganizationId` → `Organization` | model | ⚠ attribute-level mapping unchecked |
| contacts | **`SubmissionRole`** (EPIC-004 S005) | assumed | ⚠ **worth checking** |

> The contacts row is the one I most want the spec to confirm. S005 modelled
> *the people named on a filing, frozen at publication* without any eCTD input.
> If the regional envelope's contact elements are what it maps to, that is
> independent corroboration of a design decision made for entirely internal
> reasons. If it maps to nothing, S005 is still correct and simply not
> rendered — but the claim should not be made either way until checked.

---

## 3. Open questions this raises

1. **Leaf ID stability.** `modified-file` in sequence 0003 points at a leaf filed
   in 0001. Our `SubmissionDocumentId` is per-submission, so the pointer
   resolves *within our model* (ADR-045 stores the prior `SubmissionDocumentId`)
   — but whether the **emitted XML** needs an ID stable across sequences is
   unchecked. If it does, the ID we emit is not the ID we store.
2. **The application number is nullable and unset.** Whether the validator
   requires one syntactically, checks it against a pattern, or accepts a
   documented placeholder is a Task 1 question — **not** something to hard-code
   as `000000` in advance.
3. **Two versions are pinned, not one.** Task 2 pinned *eCTD v3.2.2*, but the
   regional Module 1 DTD carries its **own** version (v3.3 in current FDA
   guidance), and `submission-sub-type` is conditional on it. **Task 2 is
   incomplete as recorded** and needs the M1 DTD version pinned separately.
4. **`Unchanged` has no eCTD equivalent.** ADR-045 kept it deliberately — a
   cumulative filing carries documents forward untouched, and *nothing happened
   to it* must be distinguishable from *not filed yet*. eCTD emits no operation
   for those leaves. **The renderer must drop them, and dropping them must not
   look like a bug.**

---

## 4. What would falsify this document

- the validator rejects a package built to this mapping
- the M1 DTD's actual element names or cardinalities differ from the rows above
- `submission-type` turns out to be derivable from something we already hold
  *(would collapse the gap in §1 and make ADR-047 §6 moot)*
- leaf IDs turn out to need no cross-sequence stability
  *(would close open question 1)*

---

## Sources

- [eCTD Technical Conformance Guide — Technical Specifications Document](https://www.fda.gov/media/93818/download)
- [eCTD Submission Types and Subtypes](https://www.fda.gov/media/111237/download)
- [Example Submissions using the eCTD Backbone Files Specification for Module 1](https://www.fda.gov/media/83809/download)
- [Electronic Common Technical Document (eCTD) — FDA](https://www.fda.gov/drugs/electronic-regulatory-submission-and-review/electronic-common-technical-document-ectd)

> **Two fetches failed while writing this** — `accessdata.fda.gov/static/eCTD/us-regional-v3-3.dtd`
> and the eCTD v3.2.2 submission-standards page both returned 404. The DTD's
> actual declarations are therefore **unread**, which is why every
> `us-regional.xml` row above is marked *assumed* rather than *spec*. Retrieving
> that DTD is the next thing that would raise this document's confidence, and it
> needs no validator.
