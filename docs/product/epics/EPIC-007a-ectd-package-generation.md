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

| Level | Evidence | What it proves | Risk retired |
|---|---|---|---|
| **1** | RegOS generates XML that passes its own tests | the implementation is internally consistent | software defects |
| **2** | XML passes an **independent** validator (DTD / schema / business rules) | the package is syntactically and structurally valid | **specification interpretation** |
| **3** | the package matches publicly documented FDA/ICH examples and guidance | the implementation follows expected regulatory convention | **interpretation of convention** |
| **4** | the package is accepted by a real authority gateway | it works in the real world | operational |

> **EPIC-007a targets Level 2, aspires to Level 3, and puts Level 4 explicitly
> out of scope.**

Level 1 is where every previous epic already sits. It is the same reasoning that
produced the model, checking itself — necessary, and worth nothing as external
evidence.

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
| 1 | **Select and document the external validator** — or document why none is currently reachable | a named tool and how it is run, or a written absence | 🟡 **decided, not yet verified** |
| 2 | **Determine the supported eCTD specification and version** | the version we target, chosen by what the oracle checks | ✅ **FDA eCTD v3.2.2** |
| 3 | **Identify the minimal package that validates** | the smallest thing that passes — the epic's first milestone | ⚪ blocked on 1 |
| 4 | **Produce a proof-of-concept package outside the domain model** | hand-built if necessary; proves the oracle and the target before any RegOS code | ⚪ blocked on 3 |
| 5 | **Only then design the generation pipeline** | Phase 2 | ⚪ |

**Task 1 chose LORENZ eValidator Basic against the US eCTD 3.2 (FDA) profile,
and the choice is recorded with its open questions in
[docs/evidence/EPIC-007a/](../../evidence/EPIC-007a/README.md).** It is
*decided* rather than *verified*: the tool is Windows software, the development
machine is macOS, and whether the free edition includes the profile and accepts
a package of our shape is unknown until it is in hand. Each of those can fail
Task 1, and the record lists them as checkboxes rather than assertions.

**Task 2 pins FDA eCTD v3.2.2**, deliberately not v4.0 as well. FDA supports
both; supporting both here would double the surface before one package has ever
validated, and would make a failure ambiguous — we would not know which target
we had got wrong.

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
- **At least one representative package is accepted by an independent
  validator.** Manual invocation is acceptable and must be documented so it is
  reproducible.
- Where published FDA/ICH examples cover a construct, ours is compared against
  them and differences are explained rather than absorbed.
- ADR written for whatever Phase 2 decides the package *is*.

**Still carried, and stated so the epic cannot be overclaimed:**

| # | Hypothesis | Why a validator cannot settle it |
|---|---|---|
| **4** | a document that moves section is `delete` + `new` | both readings produce legal XML |
| **5** | `Append` is unexercised in FDA practice | legality says nothing about usage |
| **6** | `modified-file` is publication metadata, not recoverable later | a validator checks the pointer resolves, not whether we could have reconstructed it |
| **7** | lifecycle belongs to the placement, not the document | both readings validate |

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

*Not started. Blocked on Phase 1 task 1 — the oracle decides the target, and the
target decides the design.*
