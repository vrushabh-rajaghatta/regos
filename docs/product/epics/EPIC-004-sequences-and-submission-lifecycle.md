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
| **2** | Architecture | ~~**The snapshot is the publication record.**~~ | — | ~~S002~~ **RESOLVED — split in two, see the S002 note** |
| **3** | Architecture | ~~**The authority's status is correspondence, not a field.**~~ | — | ~~S003~~ **RESOLVED — supported**, see the S003 note |
| **4** | Regulatory evidence | A document that **moves section** is `delete` + `new`, not `replace`. | a real sequence showing otherwise | EPIC-007 |
| **5** | Regulatory evidence | **`Append` is unexercised** in FDA practice — enum value, no derivation. | a seeded blueprint or real sequence that uses it | EPIC-007 |
| **6** | Regulatory evidence | **`modified-file` is publication metadata** — frozen, not recoverable later. | it proves derivable post hoc from data we already keep | EPIC-007 |
| **7** | Regulatory evidence | **Lifecycle belongs to the placement**, not the document. | a real sequence where one document carries one operation across two sections | EPIC-007 |
| **8** | Architecture | ~~**The filtered unique index plus bounded retry is sufficient** for concurrent publishes.~~ | the concurrent-publish test failing within a bounded retry count | ~~S001~~ **RESOLVED — falsified**, see the S001 note below |
| **9** | Architecture | **Serialising publication per application with `pg_advisory_xact_lock` gives acceptable throughput while preserving aggregate invariants and without requiring broader transaction ownership.** | it needs transaction ownership beyond the publish path, or it does not close the same-submission double-publish window it widens | **S002, and only if** it needs transaction ownership anyway — otherwise not until real usage shows *"click Publish again"* is unacceptable |

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
| **S002** | ✅ **What changed since last time** — the placement diff, operation computed at publish and frozen, the replace pointer; first sequence is all-`New`, asserted. **Resolved hypothesis 2 — the snapshot went. ADR-045.** | full slice |
| **S003** | ✅ **The lifecycle we own** — three states, dated history per the cross-cutting status rule. **Resolved hypothesis 3 — supported.** Amends ADR-044. **ADR-046.** | full slice |
| **S004** | ✅ **What a filing is rendered as** — `SubmissionFormat`, frozen at publication; operation derivation proved format-independent. **Four of the six sketched fields refused, one shown unmodellable. ADR-047.** | full slice |
| **S005** | ✅ **The people on a filing** — `SubmissionRole`, frozen at publication; an application's contacts **derived** from the latest published sequence, with no application-level model. **ADR-048.** | full slice |
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

### The candidate principle S001 produced

> **A uniqueness constraint is not a serialisation strategy.** It protects
> correctness; it does not coordinate work. Under contention every competing
> writer except one loses — which is the index doing its job exactly, and is a
> different responsibility from deciding who goes next.

**One demonstrated instance. Not promoted.** The restraint that applied to
EPIC-006's *identity over convenience facts* applies here: a principle with a
single occurrence is an observation with a citation. If a second, independent
case arrives — anywhere a constraint is reached for where coordination was
meant — it earns a place in ADR-045.

### Discovered while building

- **A reference-type id makes an EF shadow foreign key optional**, and an
  optional FK severs the relationship instead of deleting the orphan. Caught by
  an existing test, not by review, during the ADR-043 conversion. Both shadow FKs
  are now explicitly `IsRequired` with the reason inline — **and the migration
  guidance in `IdentityConventionTests` was corrected**, because 15 ids remain
  and it had described the conversion as mechanical without naming this.
- **One shared fixture application across parallel test classes stopped being
  valid.** Before numbering, a shared application was a convenience; after it,
  the same fixture is **shared mutable numbering state**, and the classes were
  contending on the index for reasons unrelated to what they assert. *The fixture
  was not flaky — it had become architecturally invalid*, which is the more
  useful kind of test failure: a false assumption in the test infrastructure
  rather than in the production code.
- **S001 added exactly one DIA attribute** — *Submission Number*. Everything else
  in the story is invariant, policy, index, screens and the id conversion. The
  story was not spent adding columns; it was spent making one piece of state mean
  something.

---

## S002 — what a publication means *(2026-08-02)*

### The question, and the assumption it exposed

Opened on *what becomes true exactly once, at the instant of publication?*, with
one falsifier: **can this be reconstructed from immutable published sequences and
documents, with no loss of historical meaning?**

Three candidates fell out cleanly — the baseline is `SequenceNumber - 1`, the
document set is already frozen on an immutable `Submission`. The operation did
not, and **not for the reason expected**: every input survives, but the
*derivation rule* is not immutable. Hypotheses 4–7 are open and land at EPIC-007,
so a filing recomputed later under a changed rule would say something other than
what it said — and after EPIC-007 transmits, the operation is a fact the
authority holds too.

Then the question exposed something nobody had written down: **what a
submission's document set means.** `RequiredDocumentCoverageEvaluator` requires
every mandatory placeholder filled *per submission*, so a RegOS submission is the
**cumulative dossier**, not a delta. That was true by accident of the validator
and is now a decision — [ADR-045](../../adr/ADR-045-the-cumulative-dossier-and-the-derived-delta.md)
decision 1, the epic's product thesis.

### Hypothesis 2 — resolved by splitting, not by choosing

| Statement | Verdict |
|---|---|
| Publication facts exist that cannot safely be recomputed | ✅ **Supported** |
| `SubmissionSnapshot` is where those facts belong | ❌ **Falsified** |

*"Does the snapshot contain publication facts?"* turned out to be the weaker
question. The better one is **could it express them** — and without
`ProductDocumentId` and `TemplateSectionId` it has no identity that compares
across sequences. Giving it those does not evolve it; it duplicates
`SubmissionDocument`, and **duplication is not preservation**. `SubmissionSnapshot`
and its table are deleted.

### Discovered while building

- **`Unchanged` had to become a real operation value**, though eCTD has none.
  The publication-boundary rule requires null to mean exactly one thing, and in a
  cumulative dossier *"carried forward untouched"* must be distinguishable from
  *"not filed yet"*. The rule produced a modelling consequence within a day of
  being written.
- **A deletion had to be written down.** Under the cumulative model a withdrawal
  is only an absence, and **an absence cannot be frozen** — so `SubmissionDeletion`
  records it. Not a `SubmissionDocument` with a flag: that entity means *this
  dossier contains this document*.
- **An operation is a fact about a placement, not an attachment.** Publishing
  with unplaced documents is permitted (the validator reports information, not an
  error), so the invariant is narrower than it first looked. This is hypothesis 7
  appearing as a modelling consequence rather than as a regulatory question.
- **Latent domain behaviour, surfaced by a downstream capability.** `AddNewVersion`
  has sat on the `ProductDocument` aggregate since EPIC-003 with a comment saying
  it was modelled but not exposed; nothing reached it, so a revised document
  could not be recorded at all. Under the cumulative model that is **the most
  common gesture a sequence makes**, and the DoD's browser proof is unreachable
  without it. Added as `UploadDocumentVersion`, and kept in S002 deliberately.

  > The distinction worth preserving: this is not *adding capability because it
  > seems useful*. It is **exposing capability that the domain already owned,
  > because a story made it a prerequisite**. The aggregate owned the behaviour;
  > the story only made it reachable. That is why it did not become its own
  > epic — and ADR-018 is satisfied because nothing speculative was created.
- **`ISubmissionNumberingPolicy` became `ISubmissionPublicationBaseline`.** The
  number and the baseline are one question — *what does the next filing follow?*
  — and two services asking it would be two chances to disagree.
- **Test cleanup outlived its schema.** Two classes deleted from tables this
  story dropped, so cleanup threw, rows leaked, and 52 unrelated tests failed on
  the sequence index.

### Watch: the test harness carries implicit schema knowledge

**Two independent instances, one shape.** Not a defect pattern — a structural
observation about the fixtures:

| Story | What the harness assumed | What made it false |
|---|---|---|
| S001 | one shared fixture application is harmless | a sequence number turned it into shared mutable numbering state |
| S002 | cleanup can name tables directly in SQL | the story dropped two of them |

In both cases the production code was correct and the **test infrastructure
encoded an assumption that stopped being true**. Two examples is not an ADR and
not yet a refactor.

> **If a third story reveals the same shape, that is the trigger to ask whether
> the fixture architecture needs changing** — rather than fixing a third
> instance and moving on. Recorded here so the third one is recognised as a
> third rather than as a fresh surprise.

---

## S003 — the lifecycle we own *(2026-08-02)*

### What the code said before the argument

`SubmissionStatus` had **eight readers and all eight asked `!= Draft`**. It was a
boolean in disguise, so adding states to it was the first time the value would
mean anything — which is why it was worth asking what belongs in it rather than
which RIM statuses to copy.

### Hypothesis 3 — supported

No `HaStatus`. The burden was to find one authority-side fact correspondence
cannot carry, and there is none: an acknowledgement is an inbound
`HaCorrespondence` anchored to the submission, a refuse-to-file is a letter, an
approval is a letter *and* a `Registration` that already holds `Approved`,
`UnderReview`, `Withdrawn` and `Refused`. **Under review** is the persuasive
case, because it looks most like a status and is in fact a read over two facts.

Three of RIM's nine candidate states survived. `Withdrawn` was refused outright:
you cannot un-file a sequence, so withdrawal is a relationship between
submissions — EPIC-006's threading argument, third outing.

### `Filed`, and the amendment it forced

ADR-044 said a null sequence number means *"never transmitted"*. `Publish` only
freezes. **The ADR's word was stronger than the code's behaviour**, and the fix
is not to weaken the code: `Filed` is defined, nothing transitions into it, and
ADR-046 amends ADR-044 to say *published within RegOS* until EPIC-007 transmits.

The case for building it now was real — EPIC-006 records meetings without
holding them — and lost twice over. A letter someone types **is** the letter that
arrived; a RegOS submission is **not** the package that was sent. And deferring
costs one transition later against a button recording a dubious date now.

### The seventh history — measured, threshold met

[ADR-042](../../adr/ADR-042-what-the-interaction-context-turned-out-to-be.md)
named its own reopening condition. `SubmissionStatusEntry` is the seventh:

| Shape | Count | Size |
|---|---|---|
| `OwnsMany`, owner is the root | **4** | **22 lines each, line-for-line identical** |
| `OwnsMany`, nested one deeper | 1 (question) | 26 |
| standalone configuration class | 2 (market, registration) | different shape |

**Five blocks, four identical.** ADR-042's bar was *"revisit at five
configurations, and extract only the configuration"* — met. The verdict is
recorded in ADR-046 decision 6; **the extraction is its own change**, across four
configurations in three contexts, and is not folded into a story about lifecycle.

#### The ledger is closed

Done after the epic branch merged and before S004, so that a story about
submission identity carries no persistence cleanup.
`StatusHistoryMapping.OwnsStatusHistory` now maps **all five** owned histories —
the nested question block joined them, because its 26 lines were the same nine
statements wrapped one level deeper, and leaving one copy behind is the one
outcome worse than not extracting at all.

What did **not** move, and why:

| | Left alone | Because |
|---|---|---|
| the six entry types | ADR-042 stands: structural similarity is not behavioural similarity | an `InspectionStatus` is not a `CommitmentStatus` |
| the chronology rule | one line, six copies, measured as not worth it in S003 | extracting it would buy a call and cost a domain dependency |
| `MarketStatusEntry`, `RegistrationStatusEntry` | standalone configurations, not owned navigations | different shape — explicit column types and a chronological index the owned blocks do not have |

**The neutrality proof is the point.** `dotnet ef migrations add` against the
refactored model produced an **empty `Up` and `Down`**, and regenerated
`RegOSDbContextModelSnapshot.cs` byte-identical. A configuration refactor that
cannot demonstrate that is not behaviour-neutral; it is an untested schema
change.

One thing was traded: the four shared property names are matched by **string**
rather than by expression. The entry types share no interface, and giving them
one would have meant changing the entry types — the exact thing the extraction
refused. A rename now fails at model-build time, which every integration test
does before its first assertion.

### Discovered while building

- **`PublishedAt` was already in the history.** It is the `RecordedOnUtc` of the
  `Published` entry, so it stopped being a column. `Commitment.GivenOn` a second
  time, and discovered the same way — by writing the history and noticing.
- **The migration had to backfill before dropping.** The scaffold dropped the
  column first; every existing filing would have lost its date. A history that
  began the day the migration ran would be a worse record than the one it
  replaced.
- **No new cross-context edge was needed.** `ListCorrespondence` gained a
  `SubmissionId` filter — exposing an anchor `HaCorrespondence` has carried since
  EPIC-006 S001 — and the page composes the two lifecycles from two projections.
  Neither context learned anything about the other.

### The pattern these three stories share

Each one found that a single term was concealing two independent facts:

| Story | One concept | Turned out to be two |
|---|---|---|
| S001 | publication | **numbering at publication**, and transmission later |
| S002 | a document in a filing | the document, and **the publication's interpretation of it** |
| S003 | a submission's status | **our lifecycle**, and the regulatory conversation |

> **One overloaded term concealed two independent facts.** Once they are
> separated, the extra object or status almost always disappears — which is why
> these stories keep deleting concepts instead of adding them.

**Explanatory, not prescriptive.** It is a recurring discovery, not a rule, and
a future story should not be forced into the shape.

---

## S004 — the story whose most valuable work was refusing to build *(2026-08-02)*

S004 was scoped in Phase 2, before S001–S003 existed. Sorted by the tests those
stories produced, its six sketched fields turned out not to be one kind of thing.

| Sketched | Verdict |
|---|---|
| `Format` | **built** — true from the moment a filing is planned, and the filer makes it true |
| `DtdVersionIch/Regional/Stf` | **EPIC-007** — no fact until a package is built |
| `GatewayFormat` | **EPIC-007** — no fact until it is transmitted |
| `SubmissionCountries` | **hypothesis 1** — an application is already exactly one country |
| `ReasonForDelay` | **impossible** — `Submission` has no planned date to be late against |
| `SubTypeId` | **unresolved, and that is the answer** |

### The distinction that made it decidable

ADR-046 defined `Filed` and made nothing reach it, which looks like a licence for
four nullable columns. It is not, and naming why is what unblocked the story:

> **An enum value is vocabulary** — it names a state the model acknowledges
> exists, and a reader learns something true from it. **A null column is not
> vocabulary; it is an empty container**, and shipping one is a promise rather
> than a model.

A column reaches the schema, the DTO, the form and the screen. The first user who
finds a *DTD Version* field concludes it is needed and fills it in, and RegOS
then holds a regulatory attribute of unknown provenance.

### The sub-type is unresolved, not deferred

Two incompatible readings, and the model cannot distinguish them: a **taxonomy**
(`SubmissionType` gains parent/child, and nothing new belongs on `Submission` at
all) or an **independent axis** (type `IND`, sub-type `Annual Report`, both on
the submission). `SubmissionType` is flat today, so there is nothing for a
sub-type to hang from.

> **S004 deliberately does not introduce a sub-type model, because doing so
> would commit RegOS to one of two incompatible structures without evidence.**

That is stronger than *"later"* — it names what later is **for**.

### The finding that defends ADR-045

`Format` collided with S002 in a way the sketch did not anticipate: if a
submission is paper, does the operation derivation still run?

**Yes — and it matters that the answer is yes.** ADR-045 records the cumulative
dossier as the *product thesis*. If derivation ran only for eCTD, that thesis
would quietly become an eCTD implementation detail. A paper sequence still
changed something; it renders as a cover letter rather than an XML backbone.

Asserted at both levels rather than argued: a domain `Theory` over all three
formats, and a browser spec that publishes two **paper** sequences and reads
back exactly the one `Replace` the eCTD spec asserts.

### Three questions, now a modelling test

The publication-boundary rule began in ADR-045 as an implementation heuristic —
*a field meaningful only after transmission stays null until publication.* By
S004 it decides **whether the field exists at all**:

1. **When does this fact first become true?**
2. **Who makes it true?**
3. **Can the system honestly make it true today?**

Between them they have now removed `SubmissionSnapshot`, the HA status field,
the DTD columns and the gateway metadata.

### Discovered while building

- **The scaffolded migration would have written an invalid enum.** EF defaulted
  the new non-nullable column to `0`, which is not a defined `SubmissionFormat`
  — every existing row would have held a value the domain rejects on next write.
  Backfilled to `1` (`Ectd`), then the database default dropped, so an insert
  that omits the format fails loudly instead of silently becoming eCTD. The same
  trap as S003's `PublishedAt` migration, in the other direction.
- **Format continuity is recorded, not enforced.** Whether 0004 may be paper
  when 0003 was eCTD is unknown. No evidence, so no invariant — the third time
  this epic has declined to invent a rule, after ADR-044's contiguity limit and
  ADR-046's `Filed`.
- **Required, not defaulted, in the domain.** eCTD is the only format an FDA IND
  accepts today, which is exactly what would have made a default look harmless.
  The API states the default; the aggregate takes none.

---

## S005 — shared vocabulary is not a shared fact *(2026-08-02)*

The role is additive. **Where it lives is not**, and this was the first new
cross-context decision since the submission work began.

### The question the three questions did not answer

S004's three questions (*when does this fact become true, who makes it true, can
we honestly make it true*) eliminated nothing here — the story really was
additive, as predicted. A different question bit instead:

> **Shared vocabulary, or shared fact?**

`Contact` already carried roles, and EPIC-016 had anticipated *"an application's
QP"*. So there appeared to be one fact in three places. There are three facts:

| | Subject | Fact |
|---|---|---|
| `ContactRole` | — | **the vocabulary** — what roles exist |
| `Contact.Roles` | a person | what they are, in general |
| `SubmissionRole` | **a filing** | who was named on it, and as what |

**Reference data names concepts; aggregates record facts.** Easy to confuse with
S001–S003's *one fact or two?* — and not the same question.

*One occurrence. Recorded beside the earlier observation rather than promoted:
if another story finds the same distinction, it may be a heuristic.*

### The absence is the decision

There is no `ApplicationContact`, and ADR-048 exists to say why. Under the
cumulative model the latest published sequence **is** the current regulatory
state, so an application-level copy and "the contact on the latest sequence" are
one fact stored twice — and two copies can only differ by one being stale.

**The same argument that removed `SubmissionSnapshot` in S002**, applied to
people instead of documents. That symmetry is the strongest evidence the design
is internally consistent: in both cases the temptation was a convenient copy of
current state, and in both the cumulative model already had a single source.

The cost is accepted knowingly: an application that has published nothing has no
contacts. That is the absence of a filing, not missing data.

### Discovered while building

- **The repository did not load the collection the aggregate reasons over.**
  `SubmissionRepository` included `Documents` but not `Roles`, so `RemoveRole`
  searched an empty list and returned a silent 404, and `AssignRole`'s duplicate
  check was **vacuously true** — leaving the unique index to fail what the domain
  should have refused, as a 500 rather than a business rule.

  **No unit test could see it**: an in-memory aggregate always has its collection
  populated. It took the browser spec, and it was only *diagnosable* because the
  API returned 404 to a direct call — the page rendered `assign` errors but not
  `remove` ones, so a failed removal looked exactly like a successful one that had
  not refreshed. Both are fixed, and two round-trip tests now assert what the
  unit tests structurally cannot.

- **SC-003 caught a new query folder with no `<Name>Query.cs`.** Copied from
  `ListSubmissionDocuments`, which is on the grandfathered list. The fix was to
  write the query record, never to grow the list.

- **Carry-forward was left unbuilt on purpose.** Most sequences will name the
  same people, so inheriting them is closer to remembering regulatory state than
  to planning — but carry-forward was identified in S002 as its own capability,
  and adding it for contacts alone would leave two mechanisms and two mental
  models.

---

## S006 — the capstone *(2026-08-02)*

S006 introduced **no aggregate, no invariant, no column and no route**.
`Program.cs` is unchanged. That was the point: the capstone's job was to show
that S001–S005 compose, and a capstone that expands the model cannot show that.

`ListProductDocumentUsage` was **extended rather than duplicated** — checked
first, on the principle that inventing a parallel query would have been the
signal to stop. Its own note had named *sequence number* and *status* as the
fields it expected to grow, and that prediction held.

### The withdrawal asymmetry — predicted, and not friction

| | Write | Read |
|---|---|---|
| a placement | `SubmissionDocument`, frozen at publish | one row |
| an absence | **cannot be frozen** → `SubmissionDeletion` | one row |

Two tables and two shapes, because S002 found that *an absence cannot be
frozen*. Reading backwards they merge into one chronological stream, and the
merge is clean because **both carry the diff key** — `(ProductDocumentId,
TemplateSectionId)`. Nothing is reconstructed; nothing is matched by guesswork.

> **The write model splits them; the read reunifies them.** That is a read
> composing, not architectural debt.

### Discovered while building

- **Every slot in the seeded FDA IND blueprint is mandatory (48 of 48).** So a
  document the blueprint requires **can never be withdrawn** — the validator
  refuses the next filing for an incomplete dossier. Correct rather than
  friction: you cannot withdraw what the dossier is required to contain. It does
  mean a withdrawal is only expressible for a *supporting* document, which is
  what the capstone spec follows.
- **"Usage" became "In filings".** The document workspace already had a
  *History* page for its own audit trail, and two things called history on one
  screen is the muddle this project avoids.

---

# Phase 5 — Retro

**Six stories, three ADRs, one behaviour-neutral refactor, one engineering
standard and three conventions. Four persisted facts.**

## What shipped, against the Phase-1 Definition of Done

| Criterion | |
|---|---|
| A submission is a numbered sequence within its application | ✅ S001 — assigned at publish, contiguous, arbitrated by a filtered unique index |
| Every piece of content declares its operation against the previous sequence | ✅ S002 — derived at publish and frozen; `New` / `Unchanged` / `Replace` / withdrawal |
| A submission records its format and applicable DTD versions | **⚠️ half.** Format shipped (S004). **DTD versions deliberately do not exist** — ADR-047 |
| The submission's state stops being a two-value enum | ✅ S003 — three states, and `Filed` defined but unreachable |
| A user can see one document's lifecycle across an application's filings | ✅ S006 |

The one partial criterion is a **decision, not a shortfall**, and ADR-047 is
where it is answered.

## 1. Questions that sharpened the model

The epic's most reliable output was not an answer but a **better question**.
Two of the three headline results came from a question being reformulated rather
than resolved.

| | The question as asked | The question that worked |
|---|---|---|
| S001 | should numbering happen at creation or publish? | **what facts can the aggregate honestly own, and what belongs to workflow?** |
| S002 | *is* `SubmissionSnapshot` the publication record? | **can `SubmissionSnapshot` express publication facts?** |
| S003 | which statuses belong on `Submission`? | **which facts can change independently of anything the submission does?** |

Three diagnostics came out of it, in the order they were found:

1. **Is this one thing, or one name for two facts?** — S001, S002, S003 each
   found two. *(Now in FEATURE-DEVELOPMENT-FLOW Phase 2.)*
2. **When does this fact first become true? Who makes it true? Can the system
   honestly make it true today?** — S004. Eliminated four fields and one
   impossible one.
3. **Shared vocabulary, or shared fact?** — S005. One occurrence; recorded, not
   promoted.

## 2. Rules that removed structure

Every one of these **deleted a concept** rather than adding one.

| Rule | First stated | What it removed |
|---|---|---|
| **The cumulative dossier** — a submission is the whole dossier at publication; the delta is derived | ADR-045 | the user-maintained delta, and with it an entire authoring-tool model |
| **The publication boundary** — a fact meaningful only after publication stays null until then, and is immutable after | ADR-045 → **ADR-047** | `SubmissionSnapshot`; then, generalised, the DTD and gateway columns entirely |
| **Our lifecycle is only what we did** | ADR-046 | `HaStatus`, `HaStatusDate`, and `PublishedAt` |
| **The latest published sequence is the current state** | ADR-048 | `ApplicationContact`, before it was written |

**ADR-047 is the one that changed register.** It began as an implementation
heuristic about how a field behaves and became a test of **whether the field
exists at all** — *an enum value is vocabulary; a null column is an empty
container.*

## 3. Hypotheses — counted separately, as the register requires

### Architecture — these belong to EPIC-004 and get a verdict

| # | Hypothesis | Verdict |
|---|---|---|
| **2** | The snapshot is the publication record | **Split.** Publication facts exist (supported); the snapshot is where they belong (falsified). The aggregate was deleted |
| **3** | The authority's status is correspondence, not a field | **Supported**, by attempting falsification and finding no authority fact correspondence cannot express |
| **8** | A filtered unique index plus bounded retry is sufficient | **Falsified**, twice — the implementation falsifier bit before the throughput one |
| **1** | The regulatory activity is a real object | **Carried by design.** Its milestone was never this epic: the first **EU market** or **US supplement** |
| **9** | `pg_advisory_xact_lock` gives acceptable throughput | **Not exercised.** Its trigger was "if transaction ownership is needed anyway" — it was not |

### Regulatory evidence — these were never EPIC-004's to resolve

| # | Hypothesis | |
|---|---|---|
| **4** | A document that moves section is `delete` + `new` | **Carried to EPIC-007** |
| **5** | `Append` is unexercised in FDA practice | **Carried to EPIC-007** |
| **6** | `modified-file` is publication metadata, not recoverable later | **Carried to EPIC-007** |
| **7** | Lifecycle belongs to the placement, not the document | **Carried to EPIC-007** |

> **Carried is not unresolved and is not failed.** These resolve when a real
> filing supplies the evidence, and being wrong about one means *updating
> evidence, not architecture*. Counting them as EPIC-004 failures would make the
> epic look incomplete when it honoured its own scope exactly.

**Five architecture hypotheses, three resolved, two carried with named
milestones. Four regulatory-evidence hypotheses, all carried to EPIC-007.**

## What the change-case analysis got right, and wrong

- **Right:** *"eCTD package generation reads these fields"* — freezing the
  operation and the replace pointer at publish is the seam EPIC-007 needs.
- **Right:** *"region-specific numbering rules"* dissolved exactly as predicted —
  an application is already `(product, country, authority)`.
- **Wrong, and usefully:** *"multi-country submissions — `SubmissionCountries` is
  a collection from day one"* was **not built**. An application is one country,
  so the collection would have had nothing to hold. It returns with hypothesis 1.
- **Missed entirely:** nothing anticipated that **numbering would make a shared
  test fixture architecturally invalid**, or that a **reference-type id makes an
  EF shadow FK optional**. Both were found by tests, not review.

## Decisions to promote

Already promoted during the epic, each with its evidence:

- **ES-021** — a persistence refactor proves neutrality with EF's model differ.
- **Testing Principle 9** — a reloaded aggregate still enforces its rules.
- **SC-106** — a failed mutation is visible, and distinguishable from a stale read.
- **implementation-standards** — a repository hydrates what the aggregate needs
  to enforce its invariants.
- **FEATURE-DEVELOPMENT-FLOW Phase 2** — *is this one thing, or one name for two
  facts?*

Deliberately **not** promoted: *a uniqueness constraint is not a serialisation
strategy* (one demonstrated instance), and *shared vocabulary or shared fact?*
(one occurrence).

## Carry-forward

| | |
|---|---|
| **EPIC-007** | hypotheses 4–7; DTD versions, gateway format; the transition that makes `Filed` reachable and ADR-046's amendment to ADR-044 expire |
| **EPIC-020** | contacts *intended* for a future filing — planning, not regulatory state |
| **first EU market / US supplement** | hypothesis 1, and `SubmissionCountries` with it |
| **unscheduled** | the sub-type taxonomy (ADR-047 §6); general carry-forward, which contacts and documents must join together (ADR-048 §6); hypothesis 9 |
| **still open** | 15 legacy `record struct` ids; the nine-form EPIC-016 maintenance epic |

## The number worth keeping

Across six stories the model gained **four persisted facts** — `SequenceNumber`,
`Operation`, `ReplacesSubmissionDocumentId`, `Format` — plus two child tables
(`SubmissionStatusEntries`, `SubmissionRoles`) and **lost** one aggregate,
one snapshot table and three columns.

The submission domain was substantially reshaped. Almost none of that was new
data. **The stories mostly added meaning.**

