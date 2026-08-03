# EPIC-007a — eCTD package generation

**Status:** 🟡 Phases 1 & 2 complete · S001–S003 shipped · **S004 paused on an evidence gap (ICH Appendix 4)** · **Branch:** `epic/EPIC-007a-ectd-package-generation` · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

> **The epic's named capability has not begun.** Everything shipped so far is
> preparatory: the specifications are pinned, the model has been corrected three
> times, and one hand-built package has been checked. **No RegOS code has ever
> produced an eCTD file.** The status line says so because the alternative is a
> reader inferring otherwise from three completed stories.

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

**The taxonomy has been promoted out of this epic** to
[docs/evidence/](../../evidence/README.md), which is now its canonical home and
where ADRs cite it from. It was worked out here; it is not about eCTD.

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
| 4 | **Produce a proof-of-concept package outside the domain model** | hand-built; proves the target before any RegOS code | ✅ **Level 2a reached** |
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

### Task 4 — the first external check RegOS has ever had

A hand-built FDA Module 1 backbone is **DTD-valid** against FDA's own published
DTD, checked by libxml2 — a parser that knows nothing about RegOS.
[`poc/how-to-reproduce.md`](../../evidence/EPIC-007a/poc/how-to-reproduce.md).

**Two negative controls make the pass mean something.** A validator that accepts
everything proves nothing, so both mutations were chosen to test a finding rather
than to fail arbitrarily:

| Control | Parser's verdict |
|---|---|
| the mandatory contact removed | *"content does not follow the DTD, expecting … `applicant-contacts`"* — **S005's requirement, enforced from outside** |
| `operation="unchanged"` | *"not among the enumerated set"* — **ADR-045, machine-checked** |

> The second is the one worth keeping. eCTD's operation enumeration is closed,
> and a parser will now say so on demand: **there is nowhere in the target format
> to express what ADR-045 refuses to transmit.** The cumulative model is not
> merely compatible with deriving the delta — the format admits nothing else.

It also settled the numbering question at the right level: the file validates
with sequence `0000`, so ADR-044 is legal (2a) and *FDA-starts-at-0001* is
convention (3). Without the split, one would have been mistaken for the other.

**What it did not do:** nothing in `index.xml` (the ICH DTD is not yet in
`spec/`), no FDA business rule, and nothing RegOS generated — deliberately.
Prove the target, then model the path to it.

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

**Business-rule validation (Level 2b) is not attempted here** — it is carried to
EPIC-007b, and two DoD lines keep that deferral honest rather than merely
recorded:

- **The absence is stated in the product, not only in the docs.** A DTD-valid
  package with a wrong `submission-type` token is perfectly legal XML that a
  gateway rejects, so structural validity is a weaker promise than it sounds,
  and no screen may imply otherwise. This is a **product requirement, not a
  documentation one** — it is part of the product's truthfulness.

  | Permitted | Forbidden until 2b, and arguably beyond |
  |---|---|
  | "Generate eCTD Package" | "FDA-ready" |
  | "Download Generated Package" | "Validated" |
  | | "Ready for submission" |

  The forbidden column is not a style preference. Each phrase asserts a level of
  evidence this epic does not reach.
- **A business-rule validator can be introduced later without redesign** — and
  the checkable form of that claim is: the generator's only output is a complete
  sequence folder on disk that an external tool is *pointed at*, and **no code in
  `src/` reads a verdict from any validator.** No `IEctdValidator` abstraction is
  created for a single implementation (ADR-018): the seam is the filesystem,
  which every validator that will ever exist already takes. `xmllint` lives in
  `tests/` as a harness, because that is what it is.

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

*Phase 1 closed. The regulatory-activity question is **decided** below; the
package artifact/projection question is still open.*

**Phase 2 opens on the question Task 3 raised**, not on the renderer:

> **Is the regulatory activity a real object, or is `submission-id` a grouping
> RegOS can derive?**

Everything the mapping lists as blocking hangs off the answer —
`submission-type` belongs to the activity if there is one and to the submission
if there is not, and `submission-sub-type` belongs to the sequence either way.
That is the same shape as EPIC-004's Phase 2, which opened on *what business
thing survives after sequence 0003 has been transmitted?* and found the answer
was already modelled.

### Decided: the activity is **derived**, and no aggregate is introduced

Signed off 2026-08-02. `Submission` gains one nullable self-reference:

```
OriginatingSubmissionId : SubmissionId?     null ⇒ this submission opens an activity
```

**Null is the origin marker, not a missing value.** The name describes the
*target* — provenance — so nothing about it implies amendment, continuation or
chronology beyond origin, and it deliberately does not contain the word
*activity*: **a pointer names the relationship that exists today, not the
aggregate that might exist tomorrow.**

Rendering needs no stored grouping:

```
submission-id   = (Originating ?? this).SequenceNumber
submission-type = (Originating ?? this).<the activity's type>
```

**Three invariants on the pointer**, the first of which is the founder's added
rule — *an activity begins when its opening sequence is published* — and not a
defensive check:

1. the target is **published**. A draft has no sequence number, and
   `submission-id` renders one, so an unpublished submission cannot originate an
   activity;
2. the target is in the **same application**;
3. the target's sequence number is **lower** — which, with (1), makes a cycle
   unconstructible rather than merely forbidden.

**Falsified if** an activity must exist *before* its opening sequence is
published, or acquires a fact no sequence carries — a status, an FDA-assigned
number distinct from the sequence number, a due date. **Milestone:** EPIC-007b
makes the first plausible; EPIC-020 makes it likely.

**Why the burden of proof sat where it did.** eCTD renders
`submission-id="0001"` — it borrows the opening sequence's number rather than
minting an identity. A `RegulatoryActivity` aggregate would manufacture an
identity that must then be projected away at every render. And EPIC-020 already
owns a grouping (`ProcessStep`, with submissions attaching to steps); a second
`Application → X → Submission` container introduced speculatively is how two
orthogonal groupings with slightly different meanings end up coexisting.

### E11 is accepted as a modelling defect, and it is fixed first, on its own

RegOS's `SubmissionType` catalogue enumerates **application** kinds (`FDA_IND`,
`FDA_NDA`, `FDA_510K`) and hangs off `Submission`. The value is invariant across
every submission in an application — `RegulatoryApplication` carries one
`ApplicationNumber`, so one application is one IND — which places it on the
aggregate root. eCTD's actual `submission-type` has no home in RegOS at all.

**Its own story, its own migration**, ahead of any eCTD work: domain, seed data,
reference data, APIs, migration, UI. Only once the name is vacated does the real
eCTD submission type get introduced. **One migration, one story.**

---

## S001 — Application classification belongs to the application ✅

Shipped 2026-08-02. [ADR-050](../../adr/ADR-050-application-type-classifies-the-application.md).
1,019 tests across 17 suites.

**The invariant was not added — it moved.** `CreateSubmissionHandler` already
enforced *"the type belongs to the application's authority"*, per sequence,
against a value that never varied, after the application existed. It now runs
once, in `RegulatoryApplication.Create`. **A textbook aggregate refactoring, not
a new rule** — worth recording because the first framing ("the rule had no
home") was wrong, and the correction is the more interesting fact: it had the
*wrong* home.

**What the scaffolder could not judge.** EF proposed dropping and recreating the
seeded reference table, and defaulting the new foreign key to all-zeros. Both
encode falsehoods — *"these rows are new"*, *"this application's type is
unknown"* — and neither is true. Replaced by a rename that preserves the rows
and a backfill that refuses to invent one.

> **A migration can assert a domain invariant.** An application with no
> submission to infer a type from aborts the run *by id*, rather than being
> given a meaningless value. It fired on the real dev database — 8 of 15
> applications — which is the guard working, not the guard failing.

**The `Down` is honestly lossy**, and says so: a sequence that carried a type
differing from its application's cannot be restored, because after the move that
information does not exist. A down migration cannot recreate what was
deliberately discarded.

**The backfill made the old defect visible.** Applications whose earliest
sequence disagreed with their own identity now carry the disagreement as data —
exactly what a per-sequence classification permitted.

**Two grandfathered lists shrank and neither grew.** The new query folder
satisfies SC-003 outright, and `/submission-types` came off the SC-001 route
exemption by moving under `/api`.

---

## S002 — FDA Module 1 corrected, through the versioning mechanism ✅

Shipped 2026-08-02. 624 domain/architecture tests, 91 browser tests.

It looked like fixing a typo. **The aggregate refused the shortcut, and that
refusal turned out to be the story.**

`AddSection` requires a **Draft**; the seeded version is **Published**. So the
correction could not edit the blueprint — it had to *supersede* it. The
mechanism was already there and had simply never been exercised.

| | |
|---|---|
| **v1** | retained verbatim, defect included, **Deprecated** |
| **v2** | corrected, **Published** — 1.13 becomes *Annual Report*, the brochure moves to **1.14.4.1** |

**Not a caption fix.** FDA's DTD (line 99) gives `m1-13-annual-report`, so a
v1-bound submission would render the Investigator's Brochure *into the annual
report node*. A wrong package, not a wrong label. `1.14` was always right.

**Three things this proved, none of which was the 1.13 correction:**

1. **Zero placement rows moved.** The 36 submissions bound to v1 keep pointing
   at v1's sections. Deprecation removes a version from *future* binding and
   touches nothing already bound — no FK rewrite anywhere.
2. **A clean clone and an upgraded database are identical.** The seed reproduces
   history — wrong v1, then corrected v2 — because a silently-fixed v1 would
   give two installations two different pasts. Verified by diffing a fresh
   database against a clone of the dev one: same versions, same statuses, same
   section counts.
3. **The initializer is now idempotent per *version*, not per template.** It
   used to skip any template whose id already existed, which meant a blueprint
   could never be corrected after its first insert. Existing databases now
   receive new versions from the same code a fresh clone runs — one source of
   truth, so the two cannot drift.

**Section ids are generated, not deterministic**, so nothing addresses "the 1.13
row" by id. Seed and database share exactly one stable identity: the version
*number*.

**Depth was the real implementation risk, and it was checked rather than
assumed.** `1.14.4.1` is RegOS's first four-deep section. Both the server's tree
builder and `ContentPlanSectionTree` proved genuinely recursive — and the
browser suite confirmed it renders, by failing first on a count of 40 where a
stale spec still expected 38.

### Deferred, deliberately

**Rebinding a draft to the current blueprint** is its own story. 33 drafts sit
on v1; moving them automatically would change what someone's draft means
underneath them. That is a user's decision, not a side effect of correcting
reference data.

---

## S003 — the regulatory activity ✅

**Design approved 2026-08-02; built the same day, schema first.** An earlier
attempt was reverted deliberately: the domain shape was written before the
database shape, which is the opposite of the order S001 and S002 succeeded in.
This build followed the signed-off order and the design below is what shipped —
with three departures, recorded at the end of this section.

**The first story in this epic that adds business facts rather than correcting
them.** Everything before it moved ownership, fixed lifecycle, corrected
reference data or proved versioning.

### What the DTD does *not* say, and why it decides the design

```
<!ATTLIST submission-id     submission-type      CDATA #REQUIRED >
<!ATTLIST sequence-number   submission-sub-type  CDATA #REQUIRED >
```

`CDATA`, **not an enumeration** — unlike `operation`, which is what let E2's
negative control prove `unchanged` unrepresentable. So the DTD proves only that
both are *required* (evidence **E12**). It will accept `fdast99` happily.

> **Requiredness is Level 2a; the vocabulary is Level 3 and unverifiable by any
> parser we own.** A wrong token is DTD-valid and gateway-rejected.

That places the constraint squarely in the domain: **curated reference data, not
strings on the aggregate**, because nothing downstream will catch a typo.

### The model

Three authority-scoped catalogues, one shape — `Code`, `Name`, **`Token`**:

| Catalogue | Business meaning | Wire token |
|---|---|---|
| `ApplicationType` | application classification | `fdaat*` |
| `SubmissionType` | the regulatory activity | `fdast*` |
| `SubmissionSubType` | what a sequence does to it | `fdasst*` |

`Token` is **stored, never derived from `Code`** (E8 — the readable phrase lives
only in an XML comment), and **nullable**, with a precise meaning:

> **A null token means *this authority's wire vocabulary has not yet been
> modelled*.** Not "unknown", and emphatically not "derive it". Only FDA's
> vocabulary is in hand; a TGA or CDSCO row has none because this project has
> never read those DTDs.

On `Submission`:

```
OriginatingSubmissionId : SubmissionId?        null ⇒ opens an activity
SubmissionTypeId        : SubmissionTypeId?    required iff Originating is null, forbidden otherwise
SubmissionSubTypeId     : SubmissionSubTypeId  required on every sequence
```

All three **frozen at publication**, as ADR-047 does for `Format`.

### Four invariants — and one theorem

1. **XOR**: `OriginatingSubmissionId is null ⇔ SubmissionTypeId is not null`.
   One fact, one home: a continuing sequence must not carry a second copy of its
   activity's type, because two copies can only differ by one being wrong.
2. The origin belongs to the **same `RegulatoryApplication`**.
3. The origin is **published** — eCTD renders `submission-id` as the origin's
   *sequence number*, and a draft has none.
4. The origin **is itself an origin**. FDA example #22 carries
   `submission-id="0001"`, pointing at the opener rather than a predecessor, so
   chains would need transitive resolution to render. This makes one
   unconstructible.

**Not an invariant:** *the origin's sequence number is lower*. It cannot be
checked at creation — the new submission has no number yet (ADR-044 assigns at
publish) — and it does not need to be. Rule 3 forces the origin to be published
and numbers are assigned monotonically at publish, so a lower origin number
follows. **A theorem of the publish lifecycle, not a validation rule.**

### Rendering, and how it fails

```
submission-id        = (Originating ?? this).SequenceNumber
submission-type      = (Originating ?? this).SubmissionType.Token
submission-sub-type  = this.SubmissionSubType.Token
```

No recursion, because of invariant 4.

**A package renders only if every wire token the target authority needs exists.**
Otherwise a domain error naming the gap — *"ApplicationType 'TGA_ARTG' has no
eCTD token defined"* — never malformed XML. The same philosophy as S001's
migration refusing to invent a classification.

### Sub-type is supplied, never inferred

The tempting rule — *opener ⇒ application, continuer ⇒ amendment* — is falsified
by FDA example #23, an **opener** whose sub-type is `report` (evidence **E13**).

So the UI asks, and records regulatory intent rather than guessing it:

```
Regulatory activity
  (•) Start a new activity
  ( ) Continue an existing activity  [ Activity opened by 0001 — Original IND ▾ ]
```

Activities are listed in business language, not bare sequence numbers.

### Implementation order

**Database shape first — that is the lesson of S001 and S002.** Once the schema
is right and seeded, everything above it is ordinary propagation; if the
migration is wrong, everything built on it is harder to reason about.

1. EF configuration 2. migration 3. seeds (FDA tokens only)
4. freeze-at-publish 5. API 6. UI 7. tests

### Three departures from the signed-off design

Each changed how a rule is enforced. **None changed which rules exist**, and
each is here because a reader comparing the design above with the code would
otherwise think one of them had drifted.

#### 1. The exclusive-or became unconstructible instead of checked

Invariant 1 is not a rule the aggregate applies. `SubmissionClassification` has
two factories — `Opens(type, subType)` and `Continues(origin, subType)` — and
neither can produce both facts, so *"a continuing sequence must not carry its
own activity type"* is a shape rather than a check.

> **This is the reasoning invariant 4 was already chosen for**, applied one
> level up: *"This makes one unconstructible"* is what the design says about
> pointing at an opener rather than a predecessor. The same argument reaches
> the pair itself.

There is therefore **no test for a violated XOR** — it cannot be written. What
the database does about rows that never pass through C# is a CHECK constraint,
verified against a real Postgres across all seven combinations.

#### 2. `SubmissionSubTypeId` is nullable in storage, required in behaviour

The design listed it non-nullable. **Every sequence filed before this story has
no sub-type, and none is recoverable** — E13 is precisely the finding that
position does not give it. So there was no honest backfill, and inventing one
would have put a value in front of a regulator that nobody chose.

| The three nulls, and what each means | |
|---|---|
| `OriginatingSubmissionId` null | this sequence **opens** an activity |
| `SubmissionTypeId` null | this sequence **continues** one |
| `SubmissionSubTypeId` null | this sequence **predates the model** |

Only the third is a gap. `Submission.Create` requires the value, so it can only
ever arise from history — and `IsClassified` names that state rather than
leaving callers to test a null. **The CHECK constraint treats it as a legitimate
fourth state, not a violation**, which is what stops the migration having to
choose between failing on real data and inventing a classification.

> This is the nullable-field smell the DoD discussion named — *a nullable field
> introduced after an aggregate refusal deserves explanation, not rejection.*
> The explanation is E13: the field is null exactly where the evidence says
> nothing can be known.

#### 3. Two rules live in the handler, not among the four invariants

A submission holds an `ApplicationId`, not an `AuthorityId`, so the aggregate
cannot see whether its chosen activity belongs to the right regulator. **An FDA
annual report filed under a TGA application is not a data-entry slip — it is not
a filing**, and it is the same rule S001 moved *onto* `RegulatoryApplication.Create`,
landing here in the one place it can be checked.

The second is narrower: **an unclassified sequence cannot be continued**, kept
apart from the aggregate's *"is it an opener?"* rule because the two failures
differ. One is about history; the other is about shape.

### What shipped

| | |
|---|---|
| Domain | `SubmissionType`, `SubmissionSubType` (three-catalogue shape with `Token`), `SubmissionClassification`, `OriginatingSubmission`, `Submission.Reclassify` |
| Schema | additive migration — two tables, three nullable columns, a self-referencing FK and `CK_Submissions_ActivityClassification` |
| Seeds | FDA only, and **only the rows whose token is in evidence** — 3 types, 3 sub-types, plus `fdaat4` on `FDA_IND` |
| API | `GET /api/reference-data/submission-types`, `…/submission-sub-types`, `GET /api/applications/{id}/submissions/continuable`; create takes the classification |
| UI | `RegulatoryActivityField` — start-or-continue, then what the sequence does, asked never inferred |
| ADR | **[ADR-051](../../adr/ADR-051-two-more-lookups-and-what-a-lookup-is.md)** — the identity carve-out could not grow without one |

**Verified against a real Postgres, on a throwaway database:** all seven
classification combinations against the CHECK constraint (3 accepted, 4
rejected); the migration applied, rolled back and re-applied; and a fresh clone
and an upgraded database converging on identical reference data — the S002
guarantee, re-proved for the token.

**Then verified on the development database**, which the founder authorised
migrating on 2026-08-02:

| | |
|---|---|
| **1,039 tests, 17 suites, 0 failures** | counted by *reporting* suites, because a failure count of zero also means nothing ran |
| **91 browser specs pass** | including the eight that post a submission and had to learn the new contract |
| 45 existing submissions | all retained, all in the legacy unclassified state the constraint permits |
| reference data | converged on the fresh-clone state exactly — `fdaat4` reconciled onto a row seeded before the column existed |

**The browser suite is the half that mattered.** The .NET suites were green
while eight specs still posted the old body shape; only the tests that speak the
contract could find that, which is the defect the Definition of Done was
amended for after S001.

### An observation, not part of this story

`Database=regos` is a `const` in roughly twenty test files, so the
database-backed suites can only ever run against the development database. That
is why S003's schema had to be proved by hand-written SQL on a throwaway
database rather than by the suite that should own that proof, and why nothing
could be verified until the founder's own database had been migrated.

> **A test suite that can only run against one named database cannot run twice
> at once**, and it makes every schema change a decision about someone's working
> environment rather than about the change. Candidate for EPIC-016 — recorded
> here because this story is where the cost was actually paid.

---

## A backbone is a contract, not a shared ruleset

*Recorded 2026-08-02, when both backbones were pinned. **A prediction about a
defect not yet written**, placed here so that if it is written anyway, this
paragraph is what identifies it.*

A package ships **two** backbone files, and it is natural to read them as one
format rendered twice. Evidence **E16** says they are not:

| | `checksum` on a leaf |
|---|---|
| ICH `index.xml` | **`#REQUIRED`** |
| FDA `us-regional.xml` | **`#IMPLIED`** |

A single `renderLeaf(...)` satisfying the looser rule produces a `us-regional.xml`
that validates beside an `index.xml` that does not — **and the package fails as a
whole while the file under test passes.** That is the worst shape a defect can
have: the evidence points at the wrong file.

> **So each backbone owns its own rendering rules, and shares only the
> projection beneath them** — the dossier, the placements, the derived
> operations. What is *in* the sequence is one question and is common; how a
> given backbone is obliged to state it is another, and is not.

**This is a prediction, not yet a decision.** It rests on exactly one observed
divergence, and [ADR-018](../../adr/ADR-018-rule-of-three.md) forbids abstracting
a boundary on one instance. If rendering is built and a second divergence never
appears, a shared renderer with one conditional may well be the honest answer —
and this note will have cost nothing. **What it must not do is pass silently:**
the two DTDs are both in `spec/`, so any rendering story validates against both
or claims neither.

---

## Phase 3 — the generator ⚪ decomposed, signed off, not started

*Decomposed 2026-08-03, before any generator code, on the principle the epic has
followed throughout: **decide what a story is meant to prove before deciding how
to implement it.***

Five stories, each retiring **one** class of uncertainty. The cut is along the
evidence hierarchy rather than along implementation convenience, which is why
two renderers are two stories rather than one file-writing story and one
XML story.

### One change to the shape, and the DTDs decide it

**The ICH backbone is rendered first, and the FDA regional backbone second** —
the reverse of the obvious order, because E16 is not a symmetric divergence:

| | `checksum` on a leaf | Emitting one anyway |
|---|---|---|
| ICH `index.xml` | **`#REQUIRED`** | — |
| FDA `us-regional.xml` | **`#IMPLIED`** | **legal** — the attribute is permitted, merely not demanded |

Whichever renderer is written second inherits the first one's habits. So the
order decides *how the inevitable leak fails*:

> **FDA first** teaches *"checksum is optional"*, and that habit carried into
> `index.xml` produces a file that is **invalid** — and invalid in the worst
> way, because `us-regional.xml` beside it still passes.
>
> **ICH first** teaches *"always emit a checksum"*, and that habit carried into
> `us-regional.xml` produces a **valid** file with an attribute it did not have
> to supply.

One order fails silently, the other cannot fail at all. **This is E16 earning
its place before a line of renderer code exists**, which is what recording it as
a prediction was for.

### The five stories

| | Story | Retires the uncertainty | Evidence level |
|---|---|---|---|
| **S004** | The sequence folder — structure, leaf placement, `util/dtd/`, checksums | *Can RegOS materialise the package filesystem faithfully and repeatably?* | — |
| **S005** | Render `index.xml` (ICH backbone) | *Can it render the shared backbone without any authority's rules in it?* | 2a, per file |
| **S006** | Render `us-regional.xml` (FDA Module 1) | *Can it render the authority-specific backbone — and does S003's model reach the wire?* | 2a, per file |
| **S007** | Assembly, delivery, and the epic's Level 2a claim | *Does a package **RegOS generated** satisfy the specification?* | **2a, per package** |
| **S008** | Compare generated output against FDA's examples | *Does it resemble regulatory practice, not merely legal XML?* | **3** |

---

### S004 — the sequence folder ⏸️ paused on ICH Appendix 4

**Proves:** a published `Submission` materialises as a deterministic directory
tree — `0000/m1/us/…`, `m2/`…, with `util/dtd/` populated from `spec/`, and an
MD5 for every leaf file.

> **This is where ADR-049's central claim becomes testable, and the test is one
> line.** *"The generated package is a projection, not a domain artifact"*
> predicts that **generating twice produces byte-identical output.** If it does
> not, the package holds something the submission does not, and ADR-049 is
> wrong — which is exactly the kind of failure the epic exists to find.

**Decisions this story forces:**

- **The sequence folder is named `0000`.** *Decided 2026-08-03.* E4 says it is
  legal; E5 says every FDA example starts at `0001`. **RegOS writes down the
  business fact it holds, and E5 stays convention** until EPIC-007b or a real
  filing gives a reason to adopt it. The two evidence levels answer different
  questions and this is the first place they collide as code — see S008, where
  the divergence is compared rather than assumed away.
- **Unplaced documents produce no leaf** (ADR-045 §5), and a `SubmissionDeletion`
  produces a leaf with **no file and an empty checksum** (E7) — the one place a
  filesystem story must know an XML fact.
- **Paper and NeeS must not reach here at all.** ADR-047 §4 asserted the
  *derivation* is format-independent; the DoD asks for proof the *rendering* is
  not. The entry point is where that is provable.

**Two refusals, two different sentences.** *Acceptance criterion, added
2026-08-03.* Generation can be impossible for two unrelated reasons, and a
single *"cannot generate package"* would collapse them:

| Gap | Says | Because |
|---|---|---|
| **Historical** | *this sequence predates the regulatory activity model* | it has no `SubmissionSubTypeId`, and E13 says nothing can recover one |
| **Evidence** | *this classification has no eCTD token* | the vocabulary is real; the wire value has never been read |

The first is about **our** history and can only be fixed by a person deciding.
The second is about the **authority's** vocabulary and is fixed by reading a
specification. **A user who gets the same sentence for both has been failed by a
message, not by a rule** — and the distinction is the evidence register showing
up in the product.

> **The check belongs at the entry point, not in the renderer that consumes the
> values.** A refusal after the folder is on disk leaves a misleading directory
> behind. S004 can state the rule without knowing any renderer's internals:
> *every reference-data value this submission points at that carries a `Token`
> must have one for the target authority.*

**Falsified by:** two runs differing; or a leaf path that cannot be derived from
the blueprint section, meaning placement carries information the model does not.

> **⚠ That second falsifier has already fired, before implementation. See
> below.**

---

### S004's open question — where does a leaf actually go?

*Found 2026-08-03, checking the plan against the repository before writing code.
**Not a modelling question that was missed — an evidence question that only
becomes visible when a path has to be written down.***

**What is known, and sourced.** `ectd-mapping.md` §3.4 already records the top
level and the naming rules:

```
ctd-123456/            same across all sequences
  0000/
    index.xml   index-md5.txt
    m1/us/   m2/   m3/   m4/   m5/
    util/dtd/   util/style/
```

lowercase, `[a-z0-9-]`, ≤64 chars a segment, ≤150 the whole path — ICH App 2 and
FDA Tech Guide 2.4.

**What is not in this repository:** the folder name for a *section*. Does
blueprint section `3.2.S` become `m3/32-body-of-data/32s-drug-substance/`? That
table is **ICH Appendix 4**, and only Appendix 8 — the DTD — was transcribed.

**And this is not a Module 1 problem.** The seeded FDA IND blueprint covers all
five modules — 26 · 25 · 85 · 15 · 10 numbered sections, plus five module roots
per version — **186 rows needing a path**, of which the 26 in Module 1 are the
only ones with even an inferable answer.

| Route | Verdict |
|---|---|
| Derive a folder from the section code — `1.14.4.1` → `114-4-1/` | ✖ **invention.** Nothing says FDA names them that way |
| Put every leaf at its module root — `m3/file.pdf` | ⚠ **DTD-valid** — `xlink:href` is `CDATA`, so nothing rejects it — and unconventional. Legal at 2a, wrong at 3 |
| Read the folder name out of the regional DTD's element names | ⚠ **Module 1 only, and Level 3.** `m1-2-cover-letters`, `m1-14-4-1-…` are *element* names; that FDA names folders identically is an inference from their examples, not something the DTD states |

#### Resolved: the shape ships, the values wait

*Decided and built 2026-08-03.* `TemplateSection.EctdFolder` exists — nullable,
validated against ICH Appendix 2 per segment, and **null in all 186 seeded
rows.**

> **Approved before the values were known, because the shape was.** That is
> exactly where `Token` stood before FDA's vocabulary arrived, and the null
> carries the identical meaning: *the specification that says so has not been
> read*. Not "unknown", and not "derive it from the section code".

**Three consequences, one of them surprising.**

1. **Filling it in is a new blueprint version, not an `UPDATE`.** The value is
   set at construction and a published version is frozen (S002), so Appendix 4
   arriving is a *versioning event*. That is not an inconvenience to route
   around — it is [ADR-045](../../adr/ADR-045-the-cumulative-dossier-and-the-derived-delta.md) §2's
   reasoning applied to placement: **a package regenerated under a rule that
   changed after transmission would put files somewhere other than where the
   authority received them.** A test asserts a published version cannot acquire
   folders.
2. **The naming rules are enforced where the value is created**, not trusted to
   the seed. This string becomes a filename; an illegal one is a package a
   regulator's tooling rejects, not a cosmetic defect.
3. **One section may carry two directories.** FDA's Module 1 root is `m1/us` —
   the regional level has no section of its own — so a value is a chain, and a
   leaf's path is the ancestor chain joined.

**Prediction, recorded not acted on.** Modules 2–5's folder names are *ICH's*
and identical for every authority, while Module 1's are the authority's own.
Storing both on `TemplateSection` therefore duplicates the ICH half once per
blueprint. **With one blueprint in existence that duplication does not yet
exist**, and ADR-018 forbids abstracting it away on a prediction. *Revisit when
a second authority's CTD blueprint is seeded* — that is the first moment the two
version axes (a blueprint's content vs. eCTD's directory table) visibly diverge.

#### The recommendation that led there, and why it is a schema question

> **The section-to-folder mapping is versioned regulatory knowledge, not
> renderer code** — which makes putting it in a `switch` statement the exact
> mistake this project exists not to make. It belongs on `TemplateSection`, and
> it has the same shape as `Token` on the three catalogues: **present where a
> specification has been read, null where it has not, and null means *not in
> evidence* rather than *derive it*.**

If that is right, S004 grows a migration and the rendering precondition extends
to cover it — a nullable path segment, seeded for FDA Module 1 from the regional
DTD's element names at Level 3, and null for Modules 2–5 until ICH Appendix 4 is
in the repository.

**Held for sign-off rather than assumed** — and approved. Everything else in
S004 — determinism, `util/dtd/`, checksums, the entry-point guards, both
refusals — is unaffected and could be built first. But *leaf placement* is the
story's middle name, so building around the gap would be delivering S004 in the
sense that matters least. **S004 is paused on Appendix 4**, with the schema in
place and empty.

#### Two kinds of stopping, and this is the second

*The founder's observation, recorded 2026-08-03 because it names something the
epic has been doing without a word for it.*

Four stories have now stopped before their code was written, and **they did not
stop for the same reason.**

| | Stopped by | What it meant |
|---|---|---|
| **S001** | the model — classification sat one tier too low | a weakness in **our** thinking |
| **S002** | the aggregate — `AddSection` refuses a published version | a weakness in **our** thinking |
| **S003** | the aggregate again — the XOR wanted to be a shape | a weakness in **our** thinking |
| **S004** | **missing external evidence** — Appendix 4 is not here | the edge of **our knowledge of the specification** |

> **That is a healthier place to be stopped.** The project is no longer
> discovering weaknesses in its own model; it is discovering precisely where its
> knowledge of the outside world runs out — which is what an evidence-first
> integration epic is *for*.

The distinction also changes what "unblocked" means. A domain constraint is
resolved by thinking harder. **An evidence gap is not**, and no amount of
further reasoning inside this repository will produce Appendix 4's table. Three
plausible ways to reason one up were available, and all three were rejected —
which is the discipline working rather than the discipline being obstructive.

---

### S005 — render `index.xml` (ICH)

**Proves:** the shared backbone renders from frozen values alone — `operation`
read from `SubmissionContentOperation`, never recomputed (ADR-045), and
`modified-file` from `ReplacesSubmissionDocumentId`.

**Deliberately first, and deliberately ignorant of FDA.** `index.xml` carries no
`submission-type`, no `submission-sub-type`, no `application-type` — **none of
S003's vocabulary appears in it.** A renderer that reaches for a wire token here
has reached across a boundary, and the story is over before it starts.

**Validated against** `spec/ich-ectd-3-2.dtd`, whose two negative controls are
already known to bite: a bad `operation` value and a missing `checksum`.

---

### S006 — render `us-regional.xml` (FDA)

**Proves:** the authority-specific backbone renders — and, incidentally, that
**S003 was right**. This is the first time `OriginatingSubmissionId` becomes
`submission-id`, and E15 stops being a quotation and becomes an attribute.

**The rendering precondition lands here and nowhere else.** *"A package renders
only if every wire token the target authority needs exists"* — a missing token is
a named domain error, never malformed XML. S005 cannot need it; `index.xml` has
no tokens.

**Two acceptance criteria are about restraint rather than output:**

1. **S005 is not refactored into a shared base.** Two renderers, one projection
   beneath them. [ADR-018](../../adr/ADR-018-rule-of-three.md) forbids
   abstracting a boundary on one demonstrated divergence, and this story is
   where the temptation is at its peak.
2. **The story records whether a second divergence appeared.** If two backbones
   turn out to differ in exactly one attribute, E16's prediction stays a
   prediction and *"a backbone is a contract"* is left unpromoted — which is a
   result, not a disappointment.

---

### S007 — assembly, delivery, and the Level 2a claim

**Proves the epic's outcome sentence**, and nothing before it does.

> **Per-file validity is not package validity, and E16 is the reason.** S005 and
> S006 each validate one file in isolation. **S007 validates both files from a
> single generated package** — the precise thing that passes when each half is
> checked alone and fails when they are checked together.

This is also where the epic's Level 2a evidence is **re-earned**. The existing
2a rests on `poc/ctd-987654/`, which **was hand-written**. It proves the target
can be hit; it says nothing about whether RegOS hits it. The PoC is kept, and
reclassified in the evidence directory as what it is.

**Delivery, with two rules that become tests rather than review notes:**

| | |
|---|---|
| The ZIP gets **no aggregate, no id, no status** | ADR-049 — deleting it loses no business information. It is a download, not a record |
| **Forbidden words asserted absent**, not merely avoided | *"FDA-ready"*, *"Validated"*, *"Ready for submission"* — a browser assertion, because the DoD calls this a product requirement rather than a documentation one |
| **No code in `src/` reads a validator verdict** | an architecture test. `xmllint` lives in `tests/` because that is what it is |

---

### S008 — Level 3, against FDA's own examples

**Proves** the generated package resembles what a regulator actually receives.
Produces [`comparison-to-fda-examples.md`](../../evidence/EPIC-007a/README.md),
the epic's last unfilled artifact.

**Differences are explained, never absorbed.** Where our output differs from
example #21–#24, the difference is either a defect we fix or a deliberate
divergence we record — and the sequence-numbering question (E4 vs E5) is the one
already known to be waiting.

**This is where hypotheses 4–7 may soften without resolving.** *"A validator
answers 'is this legal?'; none of the four is a legality question."* Level 3 is
the closest evidence this epic can reach, and it is still not an answer.

---

### What is already known to fail, on day one

The development database holds a published sequence that **cannot be rendered**,
and it will be the first thing anyone tries:

| | |
|---|---|
| `Initial NDA - 002`, sequence `0000` | **refused twice, for two different reasons** |
| no `SubmissionSubTypeId` | it predates S003 — a *history* gap, and unrecoverable (E13) |
| its application type `FDA_DENOVO` has **no `Token`** | an *evidence* gap — FDA prints that token nowhere we have read |

**Both refusals are the design working.** They are also a useful test of whether
the errors are worth reading: one should say *this sequence predates the model*
and the other *this classification has no eCTD token*, and a user who gets the
same sentence for both has been failed by a message, not by a rule.

*(The submission is also the row S001's migration classified as `FDA_DENOVO` from
its earliest sequence, which is the old model's defect arriving as data — a
business correction, still deliberately not made.)*

---

## What EPIC-007a has proved so far

*Recorded 2026-08-02, after S002. **This is neither a decision nor evidence** —
it is a conclusion drawn from the evidence gathered, and it is written down
because the next reader would otherwise infer a stronger one from the story
sequence.*

> **EPIC-007a has shown that RegOS's current submission model is stable enough
> to be tested against an external regulatory specification without collapsing
> into ad hoc changes. It has not shown that the model is complete.** Later
> epics (for example, process, studies, labeling and broader RIM coverage)
> remain expected to introduce new domain concepts. The evidence gathered so far
> is about the resilience of the existing submission model under external
> scrutiny, not the end of domain discovery.

**Why the narrower claim.** The stories so far read like a shift from *"what
concepts does RegOS need?"* to *"which apparent concepts are derivable?"*, which
invites the conclusion that discovery is ending. It is not: **EPIC-007a is a
format-mapping epic**, and mapping an existing model onto a fixed external
specification structurally produces derivability questions, because the target
cannot move and the model is what is under test. That is a property of this
epic's work, not of the domain's maturity — and
[BACKLOG.md](../BACKLOG.md) disagrees with the wider reading: EPIC-018, EPIC-019,
EPIC-010 and EPIC-020 are expansion, and EPIC-020 is RIM's spine.

### An observation, deliberately left as one

Three times in this epic the model refused a change **before any code was
written**, and each refusal contained the answer:

| Story | The shortcut | What refused | What the story became |
|---|---|---|---|
| S001 | invent an authority invariant | it already existed, in the wrong aggregate | relocation, not invention |
| S002 | edit the published blueprint | `AddSection` requires a draft | versioning, not mutation |
| S003 | validate ordering | the sequence number does not exist yet | a theorem, not a rule |

**Not a rule, and not a heuristic** — an observation attached to this epic. Every
instance so far has resolved the same way, which means the sample only shows one
side. The next refusal has two possible outcomes:

1. the refusal again reveals the correct design; or
2. **the correct answer is to change the aggregate.**

Only after the second has been seen is this a modelling practice rather than a
description of how one part of RegOS happened to behave. The same standard
[ADR-049 §6](../../adr/ADR-049-generation-derives-transmission-creates.md)
applies to its own test, and for the same reason.

**The condition under which it stops reporting:** a story that adds a nullable
field to get *past* an objection rather than to answer it. Provenance is the
signal — a nullable field introduced after a refusal deserves an explanation,
not rejection. S003's `SubmissionTypeId?` is the immediate case, and it is
legitimate: null is one side of an exclusive-or and means *this sequence
continues an activity*, which is a state rather than an absence. The same is
true of `OriginatingSubmissionId` (null ⇒ opens an activity) and `Token`
(null ⇒ this authority's wire vocabulary has not been modelled). None of those
is a missing value.
