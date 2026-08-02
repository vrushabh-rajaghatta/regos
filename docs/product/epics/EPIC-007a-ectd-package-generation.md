# EPIC-007a — eCTD package generation

**Status:** 🟡 Phase 1 open · **Branch:** `epic/EPIC-007a-ectd-package-generation` (cut at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

Split from EPIC-007. The eCTD backbone needs only [EPIC-004](EPIC-004-sequences-and-submission-lifecycle.md), which is shipped; STF and the xEVMPD/IDMP messages need EPIC-010 and EPIC-019 and stay in **EPIC-007b** with gateway transmission.

> **This epic exists to be told it is wrong.** Every previous epic was judged
> against RegOS's own reasoning — its tests, its ADRs, its reviews. EPIC-004
> ended with four hypotheses that no amount of further reasoning can settle, and
> a product thesis (ADR-045) about a file nobody has produced. **The defining
> achievement of this epic is not a feature. It is the first time RegOS is
> checked by something that did not come from RegOS.**

---

## Phase 1 — Epic plan

### Outcome

> **RegOS can generate a regulator-valid eCTD package from its `Submission`
> aggregate, and that package is accepted by an independent validator.**

Note what that sentence does **not** claim: not that FDA would accept it, not
that it matches any sponsor's practice, not that EPIC-004's modelling
hypotheses are now proven. Those need evidence that is not realistically
available at this stage, and the DoD says so rather than letting the epic be
written up later as having proved more than it did.

### The four levels of confidence

The scoping question was *"what makes a generated package right?"*, and the
answer is that there are four different rights.

| Level | Evidence | What it proves | Risk retired | Reachable |
|---|---|---|---|---|
| **1** | RegOS generates XML that passes its own tests | the implementation is internally consistent | software defects | ✅ |
| **2a** | XML is **DTD-valid**, checked by a third-party parser against FDA's published DTD | the package is structurally legal | **specification interpretation** | ✅ **free, offline** |
| **2b** | XML passes an independent validator's **FDA business rules** | the package satisfies the regulator's own criteria | interpretation of rules a DTD cannot express | ✖ needs commercial tooling |
| **3** | the package matches FDA's **published example submissions** | the implementation follows expected regulatory convention | **interpretation of convention** | ✅ examples are published |
| **4** | the package is accepted by a real authority gateway | it works in the real world | operational | ✖ no route |

> **EPIC-007a targets 2a and 3. 2b is carried to EPIC-007b; 4 stays out of
> scope.**

Level 1 is where every previous epic already sits. It is the same reasoning that
produced the model, checking itself — necessary, and worth nothing as external
evidence.

**The 2a/2b split is not a softening; it is a distinction the original table
missed.** A DTD says which elements may appear where. It cannot say that an
annual report must carry sub-type `report`, or that a `submission-id` must match
the sequence that started the activity. Those are business rules, and only a tool
implementing FDA's criteria checks them. Collapsing both into one "Level 2" would
have let a DTD-valid package be described as validated.

### The principle this epic introduces

> **The validator is an oracle, not a dependency.**
>
> RegOS must not depend on a validator in production. It exists to provide
> independent evidence during development, testing and release verification —
> **to challenge our interpretation, never to define it.**

The source of truth stays the eCTD specifications and the regulatory model. The
failure mode this forbids is building *whatever Lorenz accepts*, which would
quietly replace a public specification with one vendor's reading of it — and
would make the oracle useless as evidence, because it would no longer be
independent of us.

*One occurrence, so it lives here rather than in `ENGINEERING_STANDARDS.md`. If
EPIC-007b brings a second oracle (an IDMP message validator), it is promoted.*

### Phase 1 is investigative, and deliberately so

Unusually for this project, **Phase 1 has work in it** — and no package-generation
code may be written until it is done. Everything downstream depends on the
oracle: which DTD versions we target, which validation rules we satisfy, what
*accepted* means, and which outputs we must emit are all decided **by** the
validator, not before it.

| # | Phase 1 task | Output | |
|---|---|---|---|
| 1 | **Select and document the external validator** — or document why none is reachable | a named tool, or a written absence | ✅ **failed as scoped, replaced** |
| 2 | **Determine the supported specification and version** | the versions we target | ✅ **eCTD 3.2.2 + regional 3.3** |
| 3 | **Map the specification to the model** | element by element, with the gaps ordered | ✅ [`ectd-mapping.md`](../../evidence/EPIC-007a/ectd-mapping.md) |
| 4 | **Produce a proof-of-concept package outside the domain model** | hand-built; proves the target before any RegOS code | ⚪ **next** |
| 5 | **Only then design the generation pipeline** | Phase 2 | ⚪ |

**Task 1 failed as scoped, and the failure is recorded rather than worked
around.** LORENZ eValidator Basic is commercial, Windows-only, and no licence is
available to this project. What the epic said would happen if Task 1 failed was
to say so — not to describe self-validation as external evidence.

It did not collapse the epic to Level 1, because the primary sources arrived
instead: **FDA's actual `us-regional-v3-3.dtd`**, the ICH v3.2.2 specification,
FDA's worked example submissions, and the submission type/sub-type tables. A DTD
plus any third-party parser is Level 2a, free and offline; published examples are
Level 3. Only FDA's business rules (2b) remain blocked, and they are carried.

**Task 2 was incomplete as first recorded.** It pinned one version where there
are two: the ICH backbone and the FDA regional backbone version independently,
and `submission-sub-type` — required on every sequence — exists only from
regional v3.3.

**Task 3 found more than a mapping**, and its findings are what Phase 2 must
answer. They are summarised below.

### Evidence is archived, not summarised

Phase 1's deliverables are architectural assets even with **zero production
code**: validator selected, specification pinned, minimal passing package
identified, proof-of-concept assembled, and **the validator's own report kept**.

`docs/evidence/EPIC-007a/` holds the report, the tool version, and the exact
package that was checked. The acceptance rule is written there:

> The epic may claim independent validation only when a report in that directory
> corresponds to a package in that directory, produced by a tool version named
> in that directory. **Anything less is Level 1 wearing Level 2's clothes.**

**Task 1 can fail, and a failure is a real result.** If no independent validator
is reachable, this epic's central claim collapses to Level 1 and the honest
response is to say so in the DoD and reconsider the priority call — not to
proceed and describe self-validation as external evidence.

### What Task 3 found

The mapping was written to be falsified. Reading it against the primary sources
changed three things and broke two.

**1. `submission-id` groups sequences into a regulatory activity — and the DTD
makes it mandatory.** `submission-type` attaches to the activity;
`submission-sub-type` attaches to the sequence. FDA's own IND examples show
sequences 0001–0002 as one activity and 0003–0004 as another.

> **That is EPIC-004's hypothesis 1**, which the retro carried with *the first EU
> market or US supplement* as its milestone. It arrived from the plain US IND
> case instead. On this evidence the activity owns a fact neither neighbour can:
> an application has many activities, a sequence has exactly one.
>
> **It is not settled.** RegOS could carry the type on the submission and derive
> the grouping; whether that is a contradiction or a denormalisation is the
> question Phase 2 now gets to ask with evidence in hand rather than in the
> abstract.

**2. `Unchanged` is dropped, and that is ADR-045 working.** The ICH operation
enumeration is exhaustive — `new | append | replace | delete` — with no
*unchanged*. A RegOS sequence holds the whole dossier; an eCTD sequence holds
only what changed. The renderer emitting nothing for `Unchanged` **is** the
cumulative-to-delta derivation, and the target format wants exactly the
increment ADR-045 said RegOS would derive.

**3. A withdrawal has no file, in the spec's words and ours.** ICH: *"there is no
new file submitted… the checksum attribute value will be empty."* S006's read
model returns a null version *"exactly when the event is a withdrawal — nothing
was placed."* Two independent models, the same absence, the same reason.

**4. The seeded FDA IND blueprint mislabels section 1.13.** Ours is
*Investigator's Brochure*; FDA's `m1-13` is the **Annual Report**, and the IB
lives at `m1-14-4-1`. A regulatory-accuracy defect in EPIC-001 seed data, latent
since seeding, found only because an external reference was finally consulted.
**Not fixed here** — changing a seeded section code moves deterministic ids and
every blueprint-bound submission, so it needs a story and a migration.

**5. RegOS numbers from 0000; every FDA example numbers from 0001.** ICH's own
example uses 0000, so this is legal (2a) and possibly unconventional (3) — the
clearest argument yet that separating those levels was worth doing.

### In scope ✅

- The **eCTD XML backbone** for a published sequence — leaf elements, operation
  attributes, the regional envelope.
- **Folder structure and file placement** as the specification requires.
- **Checksums** (MD5) for every leaf.
- **Lifecycle operations rendered**: `new`, `replace`, `delete` — read from the
  frozen `SubmissionContentOperation` (ADR-045), never recomputed at render time.
- The **`modified-file` pointer** rendered from `ReplacesSubmissionDocumentId`.
- **Independent validation** of at least one representative package (manual is
  acceptable — see DoD).
- Comparison against published FDA/ICH examples **where applicable** (Level 3).
- ADR.

### Out of scope ⏸️ (deferred, with reason)

| Deferred | Why |
|---|---|
| **Gateway transmission (ESG/AS2)** and the `Filed` transition | → **EPIC-007b**. See *the two questions Phase 2 carries*, below |
| **STF (study tagging files)** | needs EPIC-019's study registry → EPIC-007b |
| **xEVMPD / IDMP messages** | needs EPIC-010's product depth → EPIC-007b |
| **DTD versions and gateway format as stored fields** | ADR-047 §5 deferred them here, but they become true when a package is *built*; whether they are stored or derived is a **Phase 2 question**, not a Phase 1 assumption |
| **Automated validator integration in CI** | manual validation is sufficient for this milestone; automating it before we know the tool would be building a harness for an unchosen oracle |
| **Level 4 — authority acceptance** | no route to it, and claiming it would be false |

### Definition of Done

**Resolved by this epic:**

- A published sequence produces a package whose **backbone, folder layout and
  leaf placement** conform to the targeted eCTD specification.
- Lifecycle operations are **rendered from the frozen values**, and a paper or
  NeeS submission is proven not to reach the eCTD renderer (ADR-047 §4 asserted
  the derivation is format-independent; this asserts the *rendering* is not).
- **At least one representative package is DTD-valid (Level 2a)**, checked by a
  third-party parser against the DTDs held in `docs/evidence/EPIC-007a/spec/`,
  with the invocation documented so it is reproducible.
- **Compared against FDA's published example submissions (Level 3)** — where an
  example covers a construct, ours is diffed against it and differences are
  explained rather than absorbed.
- **No document describes a package as "validated" without naming the level.**
- ADR written for whatever Phase 2 decides the package *is*.

**Still carried, and stated so the epic cannot be overclaimed:**

| # | Hypothesis | Why a validator cannot settle it |
|---|---|---|
| **4** | a document that moves section is `delete` + `new` | both readings produce legal XML |
| **5** | `Append` is unexercised in FDA practice | legality says nothing about usage — though the Tech Guide now says *"the use of 'append' is not common… consider consolidating and using replace"*, which is guidance, not usage data |
| **6** | `modified-file` is publication metadata, not recoverable later | a validator checks the pointer resolves, not whether we could have reconstructed it |
| **7** | lifecycle belongs to the placement, not the document | both readings validate |

Plus one **added** by Task 3, and it is the largest:

| # | Hypothesis | Milestone |
|---|---|---|
| **1** | the regulatory activity is a real object | **moved.** Was *first EU market or US supplement*; the FDA regional DTD requires `submission-id` to group sequences into one, so it is now testable in **Phase 2 of this epic** |

> **A validator answers *"is this legal?"*. None of the four is a legality
> question** — they are all *"what does industry actually do"*, which is Level 3
> and Level 4 evidence. They stay carried, and Level 3 comparison may soften
> some of them without resolving any.

### The two questions Phase 2 carries

**1. Is the generated package an artifact, or a projection?**
This is the epic's central architectural hypothesis, and it rhymes exactly with
S002's.

> **Falsified if** everything the package needs can be regenerated from the
> published submission with no loss of meaning, and nothing about it must be
> preserved that the submission does not already hold.

A published submission is **frozen**, so regenerating its package can only
produce a different result if the *generator* changed — not the submission. That
argues for a projection. Against it: if the package must be stored, versioned,
hashed, downloaded or independently audited, it has become its own domain
concept and deserves an aggregate. **EPIC-007a is where that evidence first
exists**, and no status value is added in advance of it.

*Specifically: `SubmissionStatus.PackageGenerated` is **not** introduced. A
lifecycle state describes the business object; a generated package is an
artifact produced from it, and the two are not the same thing. Adding the state
now would pre-answer the hypothesis.*

**2. `Filed` moves to EPIC-007b — and ADR-049 owes the restatement.**
[ADR-046 §2](../../adr/ADR-046-a-submissions-lifecycle-is-only-what-we-did.md)
says *a sequence number means published within RegOS, not transmitted; EPIC-007
adds the transition that makes the stronger word true.* Splitting EPIC-007
changes nothing about that meaning — **the milestone simply belongs to whichever
half transmits, which is 007b.**

No accepted ADR is edited. **ADR-049 is written at Phase 2**, not now, so it can
record both this restatement *and* what the package turned out to be — one
decision document rather than a number spent on a single line.

### What this epic closes in RIM: nothing

EPIC-007a closes **zero** RIM objects, and that is the honest cost of taking it
before EPIC-018's ten. RIM is an object model; a package builder produces a
*file*. Coverage measures how much of the domain we can **describe** — it says
nothing about whether what we describe is **correct**, and the four carried
hypotheses are exactly the part the runway cannot see.

The trade, stated plainly: **a coverage step, for the first external check on
work already done.**

---

## Phase 2 — Domain design

*Not started. Phase 1 tasks 1–3 are closed; task 4 — a hand-built package that a
parser accepts — comes first, because the target should be proven before any
RegOS code assumes it.*

**Phase 2 opens on the question Task 3 raised**, not on the renderer:

> **Is the regulatory activity a real object, or is `submission-id` a grouping
> RegOS can derive?**

Everything the mapping lists as blocking hangs off the answer —
`submission-type` belongs to the activity if there is one and to the submission
if there is not, and `submission-sub-type` belongs to the sequence either way.
That is the same shape as EPIC-004's Phase 2, which opened on *what business
thing survives after sequence 0003 has been transmitted?* and found the answer
was already modelled.
