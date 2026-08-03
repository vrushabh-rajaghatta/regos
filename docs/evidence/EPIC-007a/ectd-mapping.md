# eCTD → RegOS mapping

**Target: ICH eCTD v3.2.2 backbone + FDA us-regional DTD v3.3.**

Grounded in the primary sources: the ICH specification (16-July-2008), FDA's
[us-regional-v3-3.dtd](spec/us-regional-v3-3.dtd) — held in this repository
because **every package must ship it in `util/dtd/`** — FDA's *Example
Submissions using the eCTD Backbone Files Specification for Module 1* v1.4, the
*eCTD Submission Types and Subtypes* tables, and the *eCTD Technical Conformance
Guide* v1.8.

## Confidence

| | |
|---|---|
| **spec** | read off the DTD or a cited specification passage |
| **model** | confirmed against the RegOS codebase |
| **assumed** | still my reading — the rows most likely to be wrong |

> **No validator has run.** Structural conformance to the DTD is checkable
> offline with any parser and is the epic's near-term evidence; FDA's *business*
> rules are not, and nothing here claims otherwise.

---

## 1. The finding that is not about the mapping

**`submission-id` groups sequences into a regulatory activity, and the DTD makes
it mandatory.**

```
submission-information (submission-id, sequence-number, form?)
  submission-id    @submission-type      #REQUIRED
  sequence-number  @submission-sub-type  #REQUIRED
```

The two attributes are **not both per-sequence**, which is what I assumed before
reading the DTD. They hang off different elements:

| | Attaches to | Answers |
|---|---|---|
| `submission-type` | `submission-id` — **the activity** | what regulatory activity is this? (`fdast1` original-application, `fdast5` annual-report, `fdast9` IND safety report) |
| `submission-sub-type` | `sequence-number` — **the sequence** | what is this sequence doing to that activity? (`fdasst3` application, `fdasst4` amendment, `fdasst6` report) |

FDA's own IND examples show the grouping directly:

| Example | Description | submission-id | sequence | type / sub-type |
|---|---|---|---|---|
| #21 | Initial IND | **0001** | 0001 | original-application / application |
| #22 | Protocol Amendment | **0001** | 0002 | original-application / **amendment** |
| #23 | Initial Safety Report | **0003** | 0003 | IND safety report / report |
| #24 | Safety Report Follow-up | **0003** | 0004 | IND safety report / **amendment** |

> Sequences 0001 and 0002 are **one** regulatory activity. 0003 and 0004 are
> another. The `submission-id` is the sequence number that *started* the
> activity, and every later sequence amending it repeats that number.

### Why this matters beyond eCTD

**This is EPIC-004's hypothesis 1** — *the regulatory activity is a real object* —
and it now has evidence. The epic recorded it as carried, falsified or confirmed
**at the first EU market or US supplement**. It arrived earlier, and from the
plain US IND case the model was built on.

The founder's test for a tier was that it must **own a business fact neither
neighbour can own without contradiction**. On this evidence it owns one:
`submission-type` is a property of the activity, not of the sequence, and not of
the application — an application has many activities, and a sequence has exactly
one.

**This does not settle it.** RegOS could carry `submission-type` on the
submission and derive the grouping, and whether that is a contradiction or merely
a denormalisation is a Phase 2 question, not a Phase 1 conclusion. What has
changed is that the hypothesis is now **testable here** rather than waiting for a
market we do not serve.

### Correction to record

My earlier note said the eCTD vocabulary values were words like
`original-application`. They are not. **The attribute values are opaque tokens** —
`fdast1`, `fdasst4`, `fdaat4` — and the readable phrase appears only in an XML
comment beside them. Anything RegOS seeds must store the token; the phrase is a
label.

---

## 2. Backbone element → RegOS source

Two backbone files per sequence, two DTDs, one placement model. Module 1 leaves
live in `us-regional.xml`; Modules 2–5 live in `index.xml`. `index.xml`'s m1
element holds exactly one leaf pointing at the regional file, and per ICH
Appendix 6 **its operation is always `new`**.

### `index.xml` — the ICH backbone (Modules 2–5)

| eCTD | RegOS source | | |
|---|---|---|---|
| module/section elements | `TemplateSection.Code` → `m2-3-quality-overall-summary` etc. | spec | ✔ |
| `leaf/@operation` `(new\|append\|replace\|delete)` | `SubmissionDocument.Operation`, frozen at publish | spec | ✔ |
| `leaf/@modified-file` | `ReplacesSubmissionDocumentId` + the prior `SequenceNumber` | spec | ✔ |
| `leaf/@checksum`, `@checksum-type` | MD5 over the stored bytes via `IFileStorage` | spec | ✔ |
| `leaf/@xlink:href` | relative path within the sequence folder | spec | ✔ |
| `leaf/title` | `ProductDocument` name — ≤512 chars, no section number | spec | ✔ |
| `leaf/@ID` | `SubmissionDocumentId`, **prefixed** — see §3.1 | spec | ✔ |
| `@dtd-version` | fixed `"3.2"` | spec | ✔ |
| `index-md5.txt` | MD5 of `index.xml`, beside it | spec | ✔ |

### `us-regional.xml` — FDA Module 1

| eCTD | RegOS source | | |
|---|---|---|---|
| `applicant-info/id` | **DUNS number** — `Organization.Identifiers` where scheme is `DUNS` | model | ✔ **corrected 2026-08-03 — RegOS has modelled this all along.** This row said *"no field"* from Phase 1 and was never checked against the aggregate; `Organization` carries `scheme + value` identifiers, `IdentifierSchemes.Duns` is seeded, `AddOrganizationIdentifier` writes one, and `OrganizationDetails` already prints *"DUNS 150483782"*. Two founder decisions were taken on the false premise. **The same failure as the DUNS citation, in the opposite direction**: a claim asserted and never verified — that one about FDA's document, this one about our own code. FDA's fallback below stays a fallback: | *"If you are unable to acquire a DUNS number prior to submission, you may enter 999999999"* — **[eCTD TCG](spec/fda-ectd-tcg-1-8.md) §3.1.1** (**E25**). ⚠ *The condition is about the **applicant**, not the filing system; `Organization.DunsNumber` is still the real answer. This row was briefly marked unevidenced on 2026-08-03, when the citation was found to trace only to our own PoC — the document arrived the same day and the section number was exactly right.* |
| `applicant-info/company-name` | `Organization.LegalName` of the applicant | model | ✔ |
| `submission-description?` | `Submission.Title` | model | ⚠ optional; ours is free text |
| `application-number` `@application-type` | `RegulatoryApplication.ApplicationNumber`; type from `SubmissionType` → `fdaat4` (IND) | model | ⚠ **nullable, no fixture sets one** |
| `cross-reference-application-number*` | — | spec | ✖ gap (DMF references; not needed for MVP) |
| `submission-id` `@submission-type` | **the regulatory activity** — see §1 | spec | ✖ **gap** |
| `sequence-number` `@submission-sub-type` | `Submission.SequenceNumber` + sub-type | model/spec | ⚠ number ✔, sub-type ✖ |
| `application-containing-files` | `true` — grouped submissions are out of scope | spec | ✔ |
| `form?` `@form-type` | the primary form; **`fdaft1` = Form FDA 1571 for an IND** | spec | ✖ gap — no form concept |

### `applicant-contacts` — and S005

```
applicant-contacts (applicant-contact+)              ← at least one, mandatory
  applicant-contact (applicant-contact-name, telephones, emails)
    applicant-contact-name  @applicant-contact-type  #REQUIRED
    telephones (telephone+) @telephone-number-type   #REQUIRED
    emails (email+)
```

**S005 is corroborated.** The regional envelope requires **at least one named
person, each carrying a required role type**, in every sequence's own file —
which is precisely *the people named on a filing, frozen at publication*. That
design was reached for entirely internal reasons (ADR-048), with no eCTD input,
and the spec independently demands the same shape.

Two honest qualifications:

- **The vocabulary is not ours.** FDA's types are `fdaact1` regulatory,
  `fdaact2` technical, `fdaact4` promotional. RegOS seeds *Qualified Person*,
  *Regulatory Contact*. `ContactRole` is a real controlled vocabulary — it is
  simply **a different one**.

  > **Decided 2026-08-02: map it, do not distort `ContactRole` to match.** The
  > internal taxonomy answers *who is this person to us*; FDA's answers *which
  > box does this go in on their side*. Reshaping ours to theirs would let one
  > authority's format redefine the domain model, and the next authority would
  > redefine it again. The translation belongs in the renderer — an
  > anti-corruption layer, not a schema change.
- **Telephone and email are mandatory** (`telephones (telephone+)`,
  `emails (email+)`). `Contact` must be checked for both; if either is optional
  or absent, a package cannot be built from a contact that lacks it.

---

## 3. What the specification settled

### 3.1 Leaf IDs — resolved, and simpler than feared

The open question was whether an eCTD leaf ID must be stable *across* sequences.
It must not. ICH Appendix 6:

> `modified-file="../0001/index.xml#a1234567"`

The pointer carries **the sequence folder and the target leaf's ID**. Each leaf
gets its own ID in its own sequence; nothing is reused. So
`SubmissionDocumentId` — per placement, exactly what a leaf is — can be the
emitted ID.

One constraint: an XML `ID` must begin with a letter or underscore, and the spec
says so explicitly. A GUID beginning with a digit is invalid, so the emitted
value needs a fixed prefix. **The ID we emit is the ID we store, with a letter in
front of it.**

### 3.2 `Unchanged` is dropped — and that is the product thesis working

The ICH DTD is exhaustive: `operation (new | append | replace | delete)
#REQUIRED`. There is no *unchanged*.

Under ADR-045 a RegOS sequence 0001 holds the **whole dossier**, most of it
`Unchanged`. An eCTD sequence 0001 holds **only what changed**. So the renderer
emits a leaf for `New`, `Replace` and `Delete`, and emits nothing at all for
`Unchanged`.

> That is not an impedance mismatch to work around. **It is the reason the
> cumulative model can produce eCTD at all**: the user maintains regulatory
> state, RegOS derives the increment, and the increment is exactly what the
> specification wants. ADR-045's central claim is confirmed by the shape of the
> target format.

### 3.3 A withdrawal has no file, and the spec says so in our words

ICH Appendix 6, Table 6-3, on `delete`:

> *"There is no new file submitted in this case… As there is no file being
> submitted, the checksum attribute value will be empty i.e., double quotation
> marks with no entry between."*

S006's read model returns `versionNumber: null` and `attachedOnUtc: null`
"exactly when the event is a withdrawal — nothing was placed." The spec emits
`checksum=""` and `xlink:href=""` for the same reason. **Two independent models,
the same absence, for the same stated reason.**

### 3.4 Folder structure and naming

```
<application>/            e.g. ctd-123456 — same across all sequences
  0000/                   4-digit sequence folder
    index.xml
    index-md5.txt
    m1/us/us-regional.xml
    m2/ m3/ m4/ m5/
    util/dtd/  util/style/
```

| Rule | Source |
|---|---|
| lowercase only; `[a-z0-9-]`; no space, dot, underscore | ICH App 2 |
| ≤64 chars per folder/file name | ICH App 2 |
| **≤150 chars for the whole path** | FDA Tech Guide 2.4 *(stricter than ICH's 230)* |
| leaf title ≤512 chars, and **must not contain the section number** | FDA Tech Guide 2.4 |

---

## 4. Two defects this comparison found in RegOS

### 4.1 The seeded FDA IND blueprint mislabels section 1.13

`RegulatoryTemplates.cs` seeds Module 1 as
`1.1 Forms · 1.2 Cover Letter · 1.3 Administrative Information · 1.4 References ·
1.13 Investigator's Brochure · 1.14 Labeling`.

Against the FDA DTD:

| Ours | FDA regional element | |
|---|---|---|
| 1.1 Forms | `m1-1-forms` | ✔ |
| 1.2 Cover Letter | `m1-2-cover-letters` | ✔ |
| 1.3 Administrative Information | `m1-3-administrative-information` | ✔ |
| 1.4 References | `m1-4-references` | ✔ |
| **1.13 Investigator's Brochure** | `m1-13-annual-report` | ✖ **wrong** |
| 1.14 Labeling | `m1-14-labeling` | ✔ |

FDA's 1.13 is the **Annual Report**. The Investigator's Brochure lives at
`m1-14-4-1-investigational-brochure`, beneath Labeling.

This is EPIC-001 seed data — a regulatory-accuracy defect, not a code defect, and
exactly the kind only an external reference can find. It has been latent since
the blueprint was seeded, and every FDA IND fixture in the test suite carries it.

**Not fixed here.** Correcting a seeded section code changes deterministic ids
and every blueprint-bound submission in the database; it belongs in a story with
a migration, not in a Phase 1 documentation pass.

### 4.2 Sequence numbering starts in a different place

RegOS numbers the first sequence **0000** (ADR-044). ICH's own example does the
same (`ctd-123456/0000` = Original Submission). But **every FDA example numbers
from 0001**, and the Tech Guide says *"begin with sequence number 0001"*.

A Level 2 question (is 0000 legal?) and a Level 3 question (is it what FDA
expects?) with possibly different answers — which is the clearest argument yet
that the two levels are worth separating.

---

## 5. What RegOS does not have

Ordered by how much of a package is impossible without it:

| Missing | Needed for | Weight |
|---|---|---|
| `submission-type` / `submission-sub-type` | every sequence | **blocking** |
| the regulatory activity that `submission-id` names | every sequence | **blocking** |
| DUNS number on the applicant organization | every sequence | blocking (placeholder permitted) |
| an application number that is actually set | every sequence | blocking (nullable today) |
| the FDA form (1571/356h) as a first-class placement | every sequence | blocking |
| contact telephone and email | every sequence | blocking if `Contact` lacks them |
| a mapping from `ContactRole` to `applicant-contact-type` | every sequence | blocking |
| cross-reference application numbers (DMF) | some | deferrable |
| STF | Modules 4–5 study reports | **EPIC-007b** |

---

## 6. What would falsify this document

- a parser rejects a package built to this mapping against the DTDs in `spec/`
- FDA's published example XML differs structurally from what we emit
- `submission-type` turns out to be derivable from `SubmissionType` + context
  *(would collapse §1 and make ADR-047 §6 moot)*
- the regulatory activity turns out to own nothing a submission cannot
  *(would leave hypothesis 1 carried, and §1 would be a denormalisation note)*

---

## Sources

Supplied by the founder 2026-08-02 and read in full:

- ICH, *Electronic Common Technical Document Specification v3.2.2*, 16-July-2008 — Appendices 2, 4, 6, 8
- FDA, `us-regional-v3-3.dtd` — held at [`spec/us-regional-v3-3.dtd`](spec/us-regional-v3-3.dtd)
- FDA, *Example Submissions using the eCTD Backbone Files Specification for Module 1*, v1.4
- FDA, *eCTD Submission Types and Subtypes* — Tables 1 and 2
- FDA, *eCTD Technical Conformance Guide*, v1.8, November 2022
