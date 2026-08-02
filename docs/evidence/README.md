# The Evidence Register

**An ADR records a decision. This records a fact that came from outside RegOS.**

The distinction matters because the two have different lifetimes and different
failure modes. A decision is ours and changes when we change our minds. A fact
about the regulated world is not ours, can be **wrong**, and can be **restated by
the authority that published it** — and when that happens, every decision resting
on it has to be re-examined.

Keeping them apart answers a question ADRs cannot:

> **If this turns out to be wrong, what breaks?**

Read the *Relied on by* column as the blast radius.

---

## The levels

Promoted out of EPIC-007a, which is where the taxonomy was worked out. **ADRs may
cite these directly.**

| Level | Evidence | Independent of us? | What it proves |
|---|---|---|---|
| **1** | RegOS's own tests | ✖ no | the implementation is internally consistent |
| **2a** | a **normative machine-readable artifact** (DTD, schema) checked by a **third-party parser** | ✅ yes | structural legality |
| **2b** | an independent implementation of the authority's **business rules** | ✅ yes | rule-level correctness a schema cannot express |
| **3** | the authority's **published prose or worked examples**, read | ✅ yes | expected convention |
| **4** | the authority itself accepts it | ✅ yes | it works |

Level 1 is not evidence for this register. It is the same reasoning that produced
the model, checking itself.

**2a and 2b are not interchangeable.** A package can be perfectly DTD-valid and
still break the rules the regulator actually applies. Any claim of "validated"
must name the level.

**2a beats 3 on legality; 3 beats 2a on convention.** They answer different
questions, and the sequence-numbering row below is the register's own proof that
conflating them would cause a mistake.

---

## Register

| # | Evidence | Source | Level | Relied on by | First recorded |
|---|---|---|---|---|---|
| **E1** | A conforming FDA Module 1 backbone validates against FDA's own DTD | `xmllint` (libxml2 20913) + [`us-regional-v3-3.dtd`](EPIC-007a/spec/us-regional-v3-3.dtd) · [reproduce](EPIC-007a/poc/how-to-reproduce.md) | **2a** | EPIC-007a's entire Level 2a claim | EPIC-007a |
| **E2** | The eCTD operation enumeration is **closed** — `operation="unchanged"` is rejected as *"not among the enumerated set"* | same, [negative control](EPIC-007a/poc/how-to-reproduce.md) | **2a** | **[ADR-045](../adr/ADR-045-the-cumulative-dossier-and-the-derived-delta.md)** — the cumulative dossier must be transmitted as a derived delta because the format admits nothing else | EPIC-007a |
| **E3** | At least one named contact, with a required role type, is **mandatory in every sequence** — omitting `applicant-contacts` is rejected | same, [negative control](EPIC-007a/poc/how-to-reproduce.md) | **2a** | **[ADR-048](../adr/ADR-048-the-people-on-a-filing-belong-to-the-filing.md)** — corroborates the people-on-a-filing model, reached independently | EPIC-007a |
| **E4** | Sequence number `0000` is **legal** — the DTD types it as `#PCDATA` and a package numbered 0000 validates | same | **2a** | **[ADR-044](../adr/ADR-044-a-submission-is-a-transmitted-sequence.md)** — numbering from 0000 is not a defect | EPIC-007a |
| **E5** | Every FDA worked example numbers sequences **from 0001** | FDA, *Example Submissions … for Module 1* v1.4, examples #21–#29 | **3** | nothing yet — **convention only.** Does **not** contradict E4 | EPIC-007a |
| **E6** | `submission-id` groups sequences into a **regulatory activity**; `submission-type` attaches to it, `submission-sub-type` to the sequence | [DTD](EPIC-007a/spec/us-regional-v3-3.dtd) + FDA examples #21–#24 | **2a** (structure) / **3** (usage) | EPIC-004 **hypothesis 1**; EPIC-007a Phase 2 opens on it | EPIC-007a |
| **E7** | A `delete` leaf carries **no file and an empty checksum** — *"there is no new file submitted in this case"* | ICH eCTD spec v3.2.2, App. 6, Table 6-3 | **3** | **[ADR-045](../adr/ADR-045-the-cumulative-dossier-and-the-derived-delta.md)** / EPIC-004 S006 — an absence is representable only as an absence | EPIC-007a |
| **E8** | `submission-type` and `submission-sub-type` are **required**, and their values are **opaque tokens** (`fdast1`, `fdasst4`), not readable phrases | [DTD](EPIC-007a/spec/us-regional-v3-3.dtd) + FDA *Submission Types and Subtypes* | **2a** / **3** | **[ADR-047 §6](../adr/ADR-047-publication-metadata-exists-only-when-publication-makes-it-true.md)** — the deferred sub-type is an independent axis, not a taxonomy beneath `SubmissionType` | EPIC-007a |
| **E9** | FDA's Module 1 **1.13 is the Annual Report**; the Investigator's Brochure is at `m1-14-4-1` | [DTD](EPIC-007a/spec/us-regional-v3-3.dtd) | **2a** | **contradicts** the seeded FDA IND blueprint (EPIC-001) — an open defect, see [the mapping §4.1](EPIC-007a/ectd-mapping.md) | EPIC-007a |
| **E10** | FDA discourages `append`: *"the use of 'append' is not common… consider consolidating and using replace"* | FDA *eCTD Technical Conformance Guide* v1.8 §2.5 | **3** | EPIC-004 **hypothesis 5** — guidance, **not usage data**; the hypothesis stays carried | EPIC-007a |

---

## Adding a row

1. **It must be checkable by someone who does not trust you.** A row whose source
   is "we reasoned that…" is a decision — write an ADR.
2. **Name the level, and only the level reached.** E10 is prose, not a run: it is
   3, not 2a.
3. **Cite the artifact, not a memory.** Level 2a rows carry a reproduction
   command; level 3 rows carry a document, version and section.
4. **Fill in *Relied on by* honestly, including "nothing yet."** E5 relies on
   nothing, and saying so is what stops it being mistaken for a requirement.

## When a row turns out to be wrong

Superseding evidence gets a **new row**; the old one is struck through and keeps
its number. Then walk its *Relied on by* column — those are the decisions that
have to be re-examined, and the reason this register exists rather than the facts
living scattered through the ADRs that happened to need them first.
