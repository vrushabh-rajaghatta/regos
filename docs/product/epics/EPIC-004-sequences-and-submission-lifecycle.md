# EPIC-004 — Sequences & submission lifecycle

**Status:** ⚪ Not Started · **Phases 2–3 approved 2026-08-01** · **Branch:** `epic/EPIC-004-sequences-and-submission-lifecycle` (cut at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

A submission today is a titled bag of placed documents with two states. A **real** submission is sequence `0003` in eCTD format against DTD 3.2, where each leaf declares whether it is **new, replaced, appended or deleted** relative to what was filed before. This closes the gap.

> **Phase 1 is settled.** The Phase-2/3 **sketch it was written with is superseded** by the approved design below. The sketch's own reasoning survives where it was confirmed and is named where it was overturned — most consequentially in what a `Submission` is.

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

## Phase 2 — Domain design *(approved 2026-08-01)*

Supersedes the sketch. Run in the order
[FEATURE-DEVELOPMENT-FLOW](../FEATURE-DEVELOPMENT-FLOW.md) prescribes: **the
domain question first, the entity list last.**

### The question this phase opened with

The first form was *"when a regulatory affairs manager says 'we're filing 0003
next week', what are they naming?"* That asks what regulators **call** things,
which is the wrong level — vocabulary follows structure, not the reverse. The
falsifiable form:

> **What business thing survives after sequence 0003 has been transmitted?**

| Someone says | And later | What still exists |
|---|---|---|
| *"We're filing sequence 0003 next week"* | | the thing 0003 is filed **into** |
| *"0003 was acknowledged"* | a month later | the acknowledgement is **about** 0003; 0003 itself never changes again |
| *"We're preparing 0004"* | six months later | the same thing 0003 was filed into |

**The application survives.** And it already exists:
[RegulatoryApplication](../../../src/RegulatoryApplication/RegOS.RegulatoryApplication.Domain/Aggregates/RegulatoryApplication/RegulatoryApplication.cs)
is scoped to `(GlobalProduct, Country, Authority)` and carries
`ApplicationNumber`. *"The IND"*, *"the original NDA"*, *"our MAA"* are not the
names of a missing tier — **they are application numbers.**

The long-lived regulatory conversation has been modelled since EPIC-003. What
Phase 2 actually found is a **word carrying two jobs**:

> **The sketch quietly assigned `Submission` the second job without noticing it
> had already been carrying the first.**

### The test a tier must pass

> **A tier earns existence by owning a fact.** Not a title, not a type, not a
> folder — **a business fact that neither `RegulatoryApplication` nor
> `Submission` can own without contradiction.**

*Contradiction*, deliberately, not *convenience*. Someone will eventually propose
grouping submissions. **Grouping is not ownership; folders are not facts.**

Applied to the candidate middle tier — the *regulatory activity* — the answer is
**vertical-dependent**, which is what makes it a decision plus a hypothesis
rather than a modelling preference:

| | Own number? | Own clock? | Own outcome? |
|---|---|---|---|
| **US · FDA · IND** — our first vertical | no — serial numbers are flat per application | no | no |
| US NDA supplement (S-001) | yes | yes | yes |
| EU variation (a procedure number) | yes | yes | yes |

For an IND there is nothing between IND 123456 and serial 0003. FDA's own word
for one serial number is *submission*, and the gateway acknowledges a submission.
**The tier owns nothing in the vertical we are building.**

### Decision 1 — `Submission` is the transmitted regulatory package

Not the conversation. The conversation is `RegulatoryApplication`.

```
RegulatoryApplication      the IND — the enduring regulatory conversation
    └── Submission         one transmitted sequence: 0000, 0001, 0002 …
```

**Vocabulary pair, both binding** (CLAUDE.md): the domain says `Submission`, the
screen says **"Sequence 0003"**. Precisely the `MedicinalProduct` ↔ *"Market"*
precedent — the domain owns precision, the UI owns familiarity. Recorded in
[docs/domain-model/](../../domain-model/) at S001, when the code makes it true.

**Corroborated by cost asymmetry** — the argument that decided *when*, having
lost the argument about *whether*:

- Choose this, and the activity turns out real → insert a tier **above** a root
  plus one nullable FK. No data migration; every IND submission legitimately
  belongs to no activity. (The backlog already reasons this way about
  `Product Family`.)
- Choose the opposite, and the activity turns out to be a folder → publish,
  validation, placement and the snapshot all move down a level and back. Worse:
  [`HaCorrespondence.SubmissionId`](../../../src/Interaction/RegOS.Interaction.Domain/Correspondence/HaCorrespondence.cs)
  would be anchored at the wrong grain **from day one**.

### Constraint — a sequence must stay addressable in its own right

An acknowledgement names a sequence. So does a refuse-to-file, so does a
validation report. EPIC-006 already anchors correspondence to a `SubmissionId`.

**A sequence demoted to a child entity cannot be anchored to.** This constraint
outlives decision 1: it holds whatever the activity tier turns out to be, and it
is the reason the activity — if it ever arrives — goes *above* `Submission` and
never *between* `Submission` and the transmitted thing.

And when it does arrive, EPIC-006 already gave us its likely shape: *"threading
is a relationship between correspondence records."* A multi-sequence activity is
plausibly a **relationship between sequences**, not a parent over them.

### Decision 2 — numbering is application-scoped, and the sketch's hard part dissolved

The sketch called sequence-numbering scope *"the one genuine concurrency question
in the epic."* Half of it was already answered: `RegulatoryApplication` is
scoped to `(GlobalProduct, Country, Authority)`, so **numbering scoped to the
application is already region-scoped** — no policy object, no new concept, no
region parameter. *One planned decision disappeared because the existing model
answered it.*

What remains is genuinely real:

- **The number is assigned by the domain, never accepted from outside** —
  as `RegulatoryTemplate` and `ProductDocument` already do for versions. But
  `Submission` is a root, not a child of the application, so the numbering
  authority sits **outside** the aggregate that consumes it. A unique constraint
  on `(ApplicationId, SequenceNumber)` plus retry is the cheapest honest answer;
  settle it in S001 against a concurrency test.
- **Invariant discovered in Phase 2 — number order *is* transmission order.** A
  sequence may only publish when every lower-numbered sequence in its application
  has already published. Without it, publishing 0003 before 0002 silently gives
  0002 the wrong diff base and rewrites what 0003 claimed. This is a domain rule,
  not a query detail.
- **Open, for S001:** whether the number is assigned at *creation* (users can say
  *"we're preparing 0004"*, but an abandoned draft leaves a gap regulators
  dislike) or at *publish* (contiguous by construction, but a draft has no name).
  Lean **creation**, with abandonment resolved by the lifecycle in S003 rather
  than by renumbering.

### Decision 3 — "the previous published sequence", defined once

> The published submission in the **same application** with the **highest sequence
> number lower than this one**.

Not *"the most recently published"* — the invariant above makes number order and
publish order the same thing, and defining it by number rather than by timestamp
means a clock skew can never change what a filing claimed.

### The central architectural hypothesis — what is the publication record?

The sketch's diff decision exposed something larger. Today **two objects claim to
be the immutable record of a publication**, and the smaller one holds strictly
less: [SnapshotDocument](../../../src/Submission/RegOS.Submission.Domain/Snapshot/SnapshotDocument.cs)
stores `DocumentVersionId` + `DisplayOrder` — **no placement, no
`ProductDocumentId`** — while `Submission` itself is frozen at publish and
carries both.

Apply the test: **can the publication be reproduced from the snapshot alone?**
Today, no. So today it is not the publication record; it is a projection of an
already-immutable aggregate.

**This epic is what earns the snapshot its existence, or ends it.** The operation
and the `modified-file` pointer are facts that are **only true at publish** and
are not properties of a draft — a `SubmissionDocument` cannot assert them without
claiming to know something it cannot. That is a fact of its own, which is exactly
what the snapshot currently lacks.

Two legitimate outcomes, both named in advance, **resolved at S002**:

| Outcome | Then |
|---|---|
| Publication-only facts land there | the snapshot **is** the publication record, and grows placement, operation and the replace pointer |
| It still owns no fact of its own | it is an implementation detail, not a domain object, and goes |

*Not pre-judged.* ADR-018 forbids speculative deletion as firmly as speculative
creation — the epic tests it, and S002 owes an answer either way.

### How the sketch's open decisions resolved

| | Question | Resolution |
|---|---|---|
| **1** | Sequence numbering scope and assignment | **Application-scoped** — already region-scoped by the existing model. Only the concurrency half survives (decision 2). |
| **2** | Operation derived or stored | **Derived at publish, then frozen.** *Where* it is frozen is the snapshot hypothesis. Drafts may show a provisional operation computed live. |
| **3** | QC / Publishing / Compilation / Validation pipelines | **Out of scope** — internal production workflow, not regulatory fact → EPIC-008. Unchanged from the sketch. |
| **4** | What the lifecycle states are | **S003**, with dated history per the backlog's cross-cutting status rule. |
| **5** | `Append` may not be needed | **Enum value, no derivation** — and reclassified as a *regulatory-evidence* hypothesis, not an architectural one. |
| **6** | "Previous published sequence" needs defining | **Defined** (decision 3), and by number rather than timestamp. |
| **7** | *(new)* Two-sided status | **Hypothesis, not a field.** EPIC-006 may already express it. |

### Hypotheses this epic carries

Per the register in [FEATURE-DEVELOPMENT-FLOW](../FEATURE-DEVELOPMENT-FLOW.md).
**Phase 5 owes an outcome on every one, including the failures.**

> **Two kinds of hypothesis, and they are not resolved the same way.**
> An **architecture** hypothesis is settled by building the model and seeing what
> it rejects — the evidence is in this repository. A **regulatory-evidence**
> hypothesis is settled by a real filing; if a customer sequence later proves it
> wrong, we are **updating evidence, not architecture**. Keep the retro honest by
> counting them separately.

| # | Type | Hypothesis | Falsifier | Resolved at |
|---|---|---|---|---|
| **1** | Architecture | **The regulatory activity is a real object.** | It owns a business fact that neither `RegulatoryApplication` nor `Submission` can own without contradiction. *Grouping is not ownership.* | the first **EU market** or the first **US supplement** — deliberately **not** EPIC-007 |
| **2** | Architecture | **The snapshot is the publication record.** | It acquires no publication-only fact when the operation and the replace pointer arrive. | **S002** |
| **3** | Architecture | **The authority's status is correspondence, not a field.** `HaStatus` + `HaStatusDate` should not exist. | A fact about what the authority did that cannot be expressed as an `HaCorrespondence` anchored to the submission. | **S003** |
| **4** | Regulatory evidence | A document that **moves section** is `delete` + `new`, not `replace`. | a real sequence showing otherwise | EPIC-007 |
| **5** | Regulatory evidence | **`Append` is unexercised** in FDA practice — enum value, no derivation. | a seeded blueprint or real sequence that uses it | EPIC-007 |
| **6** | Regulatory evidence | **`modified-file` is publication metadata** — frozen, not recoverable later. | it proves derivable post hoc from data we already keep | EPIC-007 |
| **7** | Regulatory evidence | **Lifecycle belongs to the placement**, not the document. | a real sequence where one document carries one operation across two sections | EPIC-007 |
| **8** | Architecture | ~~**The filtered unique index plus bounded retry is sufficient** for concurrent publishes.~~ | the concurrent-publish test failing within a bounded retry count | ~~S001~~ **RESOLVED — falsified**, see the S001 note below |

**Why 4–7 are carried rather than settled now:**

> **Deferred because the cost of an incorrect assumption is first paid in
> EPIC-007.** Until a backbone is generated, none of them changes behaviour. That
> is a justification; *"we don't know yet"* is not.

Hypothesis 3 is deliberately **not rejected**. EPIC-006 has earned enough trust
that introducing a stored `HaStatus` before proving correspondence cannot express
it would be the exact move [ADR-042](../../adr/ADR-042-what-the-interaction-context-turned-out-to-be.md)
decision 4 refuses — *persist the facts you own; do not persist someone else's
judgement.* Someone must prove it belongs.

### Entities *(last, and deliberately so)*

**`Submission` (extended)**

| Field | Notes |
|---|---|
| `SequenceNumber` | `int`, assigned by the application-scoped numbering rule; rendered zero-padded to 4 |
| `Format` | eCTD / NeeS / paper |
| `DtdVersionIch?`, `DtdVersionRegional?`, `DtdVersionStf?` | strings |
| `GatewayFormat?` | |
| `SubTypeId?` | |
| `SubmissionCountries` | collection — RIM: Multiple (matters for EU centralised/mutual-recognition) |
| ~~`HaStatus`, `HaStatusDate`~~ | **removed — hypothesis 3.** An acknowledgement is a letter, and EPIC-006 stores letters. |
| `ReasonForDelay?` | RIM: free text, when actual is later than planned |
| status history | dated, **our side only** unless hypothesis 3 is falsified |

**`SubmissionDocument` (extended)** — `Operation` (`New` / `Replace` / `Append` / `Delete`), nullable while draft, computed at publish. Plus a pointer to the **specific prior placement being replaced** (eCTD's `modified-file`), which is derivable only at publish and meaningless afterwards. **Which object holds both is hypothesis 2, not a decision.**

**`SubmissionRole`** — new child: `SubmissionId`, `ContactId`, `Role`. Depends on EPIC-016.

**The diff key.** Across sequences, the identity that persists is `(ProductDocumentId, TemplateSectionId)` — *the same document, in the same place*. `SubmissionDocumentId` is per-submission and cannot serve. Whatever object turns out to be the publication record must carry **both**; today the snapshot carries neither, which is how hypothesis 2 was found.

### Change-case analysis

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| eCTD package generation reads these fields (EPIC-007) | **High** | Operation, sequence and the replace pointer are what the backbone needs; freezing them is the seam |
| ~~Region-specific sequence rules (EU vs FDA numbering)~~ | — | **Dissolved.** An application is already `(product, country, authority)`; numbering scoped to it is region-scoped by construction |
| A regulatory activity turns out to be real | Medium | Hypothesis 1 — a tier **above** `Submission` plus one nullable FK, no data migration |
| Leaf reuse across sections | Medium | Known deferral, clean migration path — but note the diff key is `(document, section)`, so reuse and the operation meet here |
| A sequence is withdrawn or replaced wholesale | Medium | S003's lifecycle models it; the diff base is defined by **number** (decision 3), so a withdrawal cannot silently re-base a later sequence |
| Multi-country submissions (EU) | Medium | `SubmissionCountries` is a collection from day one |
| Operation needed for a *draft* preview | Medium | Derivation is a service; draft calls it live, publish freezes the result |

---

## Phase 3 — Stories *(approved 2026-08-01)*

Six vertical slices. **Full build in planned order — no MVP cut** (no customer is
waiting; this is roadmap priority, not escalation).

**One reorder, and Phase 2 earned it.** The sketch put identity second and the
operation third. The operation story is the epic's **hinge** — it resolves
hypothesis 2 and decides whether an existing aggregate lives — while the identity
fields are additive columns that cannot break anything. **Resolve the hinge
early; ship the inert data late.**

| # | Story | Slice |
|---|---|---|
| **S001** | ✅ **A submission is a sequence** — `SequenceNumber`, assigned at publish, application-scoped, contiguity enforced in the aggregate; *"Sequence 0003"* on screen and the vocabulary pair in [docs/domain-model/submission.md](../../domain-model/submission.md). **ADR-044.** Folds in the four ADR-043 id conversions. | domain → persistence → API → UI → test |
| **S002** | **What changed since last time** — the placement diff, operation computed at publish and frozen, the replace pointer; first sequence is all-`New`, asserted. **Resolves hypothesis 2 — the snapshot grows or goes. ADR-045.** | full slice |
| **S003** | **The lifecycle we own** — the states beyond `Draft`/`Published`, dated history per the cross-cutting status rule, permitted transitions. **Resolves hypothesis 3** by trying to express the authority's side as correspondence. | full slice |
| **S004** | **Submission identity** — format, DTD versions (ICH/Regional/STF), gateway format, sub-type, submission countries | full slice |
| **S005** | **`SubmissionRole`** — named contacts with roles on a submission *(EPIC-016 ✅)* | full slice |
| **S006** | **Capstone** — one document's lifecycle across an application's sequences; browser proof of publish → replace → publish; the register resolved; retro | UI → test → docs |

**Two ADRs, both written when the decision is made rather than at the capstone:**

- **ADR-044 at S001** — *A submission is a transmitted sequence, not a regulatory
  conversation.* Definitional: it closes off a tier, fixes the vocabulary pair,
  and states the numbering scope and the publish-order invariant. Written first
  because it is the decision every later story assumes.
- **ADR-045 at S002** — *Operation derivation, and what the publication record
  is.* Cannot be written earlier: hypothesis 2 has two legitimate outcomes and
  the ADR records which one the code produced.

**Fracture line, if it sprawls: after S003.** S001–S003 is a coherent shippable
whole — a numbered sequence that knows what changed and where it stands. S004–S005
are additive attributes and can follow separately.

**Carried in deliberately:** the four legacy `readonly record struct` ids in this
context (`SubmissionId`, `SubmissionDocumentId`, `SubmissionSnapshotId`,
`SnapshotDocumentId`) convert to `sealed class : StronglyTypedId` under
[ADR-043](../../adr/ADR-043-entity-identity-derives-from-the-kernel.md) inside
S001 — the largest refactor this aggregate is likely to receive is the cheap
moment to retire them, and the grandfathered list shrinks by four.

**Kept out deliberately:** the nine-form EPIC-016 mutation-defect maintenance
epic stays separate. This epic is invasive enough; behavioural change and
repository housekeeping do not belong in the same branch. *Unless* S001 naturally
touches one of those forms, in which case fix it there and say so.

**Sequencing note (historical, from the sketch):** this epic and EPIC-017 were
genuinely independent, and EPIC-017 ran first. S005's EPIC-016 dependency is
satisfied.

---

## S001 — what it settled, and the hypothesis it falsified *(2026-08-02)*

### Assignment at publish, not at creation — and why the precedent was a mirage

The sketch justified domain-assigned numbering by citing `ProductDocument` and
`RegulatoryTemplate`. Both do `_versions.Max(...) + 1` **inside the aggregate,
over an owned collection**, which is safe because the collection loads with the
root and optimistic concurrency protects it. `Submission` is a root; its siblings
are not in its aggregate. **The precedent is superficially true and structurally
false**, so neither option inherited it and the question was genuinely open.

Publish-time assignment won on four counts, of which the first is the one that
mattered: it makes *number order is transmission order* a **tautology instead of
a rule**. Also no gaps from abandoned drafts, a draft that asserts only what is
true, and a diff base for S002 that is simply `SequenceNumber - 1`.

### Hypothesis 8 — *the unique index plus bounded retry is sufficient*. **Falsified, twice over.**

**First, at implementation.** A retry needs to unwind an aggregate mutation that
EF is already holding: by the time the index rejects the write, `Publish` has run
and `Status` is no longer `Draft`, so the operation cannot simply be repeated.
Every way to fix that from the application layer costs either a domain method for
a state with no business meaning, or a unit-of-work reset this codebase does not
have — **an ADR-016 change to serve a retry**. So S001 shipped without one: a
collision is a third 409, `SequenceNumberTakenException`, saying *try again*.

**Then, on the numbers.** The 100-way test was widened to a `[Theory]` across
contention levels, because the interesting question was never the stress case
but *where between two and a hundred the design stops being adequate*:

| Simultaneous publishes, one application | Got through |
|---|---|
| 2 | **50 %** |
| 5 | 20 % |
| 20 | 5–10 % |
| 100 | 16–18 % *(measured, not committed — see below)* |

**The invariant held perfectly at every level** — distinct numbers, an unbroken
run from 0000, no duplicates. The index does exactly its job. But *two at once
loses one of them*, and that is the realistic case, not the stress case. The
index alone is a correctness mechanism, not a concurrency answer.

> **The named fallback is now owed evidence-backed consideration:**
> `pg_advisory_xact_lock(applicationId)` serialises rather than collides. It
> costs explicit transaction control — a concept this codebase does not yet have
> — and closes a same-submission double-publish window that the gate would
> otherwise widen. **That is a design conversation, not a fix**, so it is raised
> rather than built.

The committed test stops at 20. A hundred concurrent `DbContext`s exhaust a
local Postgres's `max_connections` while the rest of the suite runs — **the
fixture runs out before the design does**, which makes that case flaky rather
than stronger. It was measured, it agreed with the trend, and the finding was
already complete at two.

### Discovered while building

- **A reference-type id makes an EF shadow foreign key optional**, and an
  optional FK severs the relationship instead of deleting the orphan. Caught by
  an existing test, not by review, during the ADR-043 conversion. Both shadow FKs
  are now explicitly `IsRequired` with the reason inline.
- **One shared fixture application across parallel test classes became a test
  isolation defect** the moment numbering existed — the classes were sharing a
  numbering space and contending on the index for reasons unrelated to what they
  assert. Harmless before this story; wrong after it.
- **S001 added exactly one DIA attribute** — *Submission Number*. Everything else
  in the story is invariant, policy, index, screens and the id conversion.
