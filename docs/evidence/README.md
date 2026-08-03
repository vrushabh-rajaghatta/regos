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
| **E11** | `application-type` and `submission-type` are **different attributes on different elements**. `application-type` (`fdaat4` IND, NDA, 510(k)) classifies the **application**; `submission-type` (`fdast1` original-application, `fdast5` annual-report) classifies the **regulatory activity** | [DTD](EPIC-007a/spec/us-regional-v3-3.dtd) + FDA *Submission Types and Subtypes* | **2a** / **3** | **[ADR-050](../adr/ADR-050-application-type-classifies-the-application.md)** — the catalogue was named for eCTD's `submission-type` while enumerating its `application-type`, and hung off `Submission` one tier too low. Renamed and moved in EPIC-007a S001; eCTD's actual `submission-type` still has **no home in RegOS**, and the name is reserved for it | EPIC-007a |
| **E12** | `submission-type` and `submission-sub-type` are `CDATA #REQUIRED` — **required but not enumerated**, unlike `operation`. A DTD-valid package can carry a meaningless token | [DTD](EPIC-007a/spec/us-regional-v3-3.dtd) lines 87–94 | **2a** | **EPIC-007a S003** *(designed, not yet shipped)* — the token vocabulary is Level 3 only and no parser we own can check it, so RegOS constrains it as curated reference data rather than free text | EPIC-007a |
| **E13** | Sub-type is **not derivable** from an activity's position. The tempting rule *opener ⇒ application, continuer ⇒ amendment* is falsified by FDA example #23: an **opener** whose sub-type is `report` | FDA, *Example Submissions … for Module 1* v1.4, examples #21–#24 | **3** | **EPIC-007a S003** *(designed, not yet shipped)* — sub-type is a business fact the user supplies, never inferred | EPIC-007a |
| **E14** | The operation enumeration is closed in the **ICH backbone too**, not only FDA's regional one — `operation="unchanged"` is rejected as *"not among the enumerated set"* by `ich-ectd-3-2.dtd` | [DTD](EPIC-007a/spec/ich-ectd-3-2.dtd) + `xmllint` · [reproduce](EPIC-007a/poc/how-to-reproduce.md) | **2a** | **[ADR-045](../adr/ADR-045-the-cumulative-dossier-and-the-derived-delta.md)** — E2 proved this for Module 1 only. Both backbones now refuse to say *unchanged*, so the derived delta is forced by the format everywhere, not just regionally | EPIC-007a |
| **E15** | FDA states the activity-grouping rule in prose: *"the submission-id should match the sequence number of the transition sequence"*; *"All subsequent amendment to the original application should have a sub-type of 'amendment' and sub-id = 0001"*; *"If the submission … is creating a new regulatory activity, the submission-id should match the sequence number"* | FDA, *eCTD Submission Types and Subtypes*, Tables 1–2 | **3** | **EPIC-007a S003** — upgrades **E6** from inference-off-worked-examples to the authority saying it outright. It is the `OriginatingSubmissionId` design, in FDA's own words | EPIC-007a |
| **E16** | The two backbones disagree on `checksum`: **`#REQUIRED`** in ICH `index.xml`, **`#IMPLIED`** in FDA `us-regional.xml` | [ICH DTD](EPIC-007a/spec/ich-ectd-3-2.dtd) + [FDA DTD](EPIC-007a/spec/us-regional-v3-3.dtd) | **2a** | **EPIC-007a's rendering design** — a backbone is a contract, not a shared ruleset; see [the epic](../product/epics/EPIC-007a-ectd-package-generation.md#a-backbone-is-a-contract-not-a-shared-ruleset) | EPIC-007a |
| **E19** | **The blueprint's tree and the regional backbone's tree disagree about which nodes hold documents.** Of the eight Module 1 sections the FDA IND blueprint offers as placement targets, **two** accept a `leaf` — `m1-2-cover-letters` and `m1-14-4-1-investigational-brochure`. Five are declared as child elements with no `leaf` at all (`m1-3`, `m1-4`, `m1-13`, `m1-14`, `m1-14-4`); the eighth is `m1-1-forms` (E18) | [FDA DTD](EPIC-007a/spec/us-regional-v3-3.dtd) + `xmllint` | **2a** | **EPIC-007a S006** — a section being *in* the CTD outline does not make it a place a document can go. **A validation finding, not a modelling one**: the blueprint may legitimately describe the outline, and leaf-capability is per-authority, so it is the renderer that must decide | EPIC-007a |
| **E18** | **`m1-1-forms` contains `form*`, never `leaf*`**, and each `form` carries `form-type` `CDATA #REQUIRED` (`fdaft1` = Form FDA 1571). A document in section 1.1 is not a leaf at all | [FDA DTD](EPIC-007a/spec/us-regional-v3-3.dtd) line 104 + `xmllint` | **2a** | **[ADR-053](../adr/ADR-053-instance-qualifiers-belong-to-the-placement.md)** — the same finding as E17 in the other backbone: the format wants a fact identifying *which occurrence*, and RegOS models the form as a document rather than as a form | EPIC-007a |
| **E17** | **Four backbone elements are keyed, repeatable nodes, not sections.** `m2-3-s-drug-substance` and `m3-2-s-drug-substance` are declared `*` and carry `substance` **and** `manufacturer` as `CDATA #REQUIRED`; `m2-7-3-summary-of-clinical-efficacy` and `m5-3-5-reports-of-efficacy-and-safety-studies` likewise require `indication`. The drug-**product** equivalents declare the same attributes `#IMPLIED` | [ICH DTD](EPIC-007a/spec/ich-ectd-3-2.dtd) lines 193–197, 155, 622 + `xmllint` | **2a** | **EPIC-007a S005** — the CTD outline RegOS models is not what the backbone encodes. One seeded section (3.2.S) sits on a keyed element, so its documents are **refused** rather than keyed with an invented value. The gap is neither historical nor unread specification: it is a fact the domain model does not carry | EPIC-007a |

---

## Which decisions rest on which evidence

The register above reads **evidence → decisions**: *if this is wrong, what
breaks?* This reads the other way — **decision → evidence** — because a reader
who arrives at an ADR cannot otherwise tell what external facts hold it up, or
that the support has since grown.

> **This index is where a decision's evidential basis is maintained.** An
> accepted ADR is never edited (repository canon), so when evidence broadens the
> support for a decision already made, the change is recorded here. The decision
> did not change; what we can prove about it did.

| Decision | Rests on | |
|---|---|---|
| [**ADR-044**](../adr/ADR-044-a-submission-is-a-transmitted-sequence.md) — a submission is a transmitted sequence | **E4** | numbering from 0000 is legal |
| [**ADR-045**](../adr/ADR-045-the-cumulative-dossier-and-the-derived-delta.md) — the cumulative dossier and the derived delta | **E2**, **E7**, **E14** | **E14 broadened this from one backbone to both.** E2 alone supported *"the regional backbone cannot say `unchanged`"*; with E14 the claim is *"the eCTD format cannot say it anywhere"* |
| [**ADR-047 §6**](../adr/ADR-047-publication-metadata-exists-only-when-publication-makes-it-true.md) — sub-type is an independent axis | **E8** | |
| [**ADR-048**](../adr/ADR-048-the-people-on-a-filing-belong-to-the-filing.md) — the people on a filing belong to the filing | **E3** | reached independently, then corroborated |
| [**ADR-050**](../adr/ADR-050-application-type-classifies-the-application.md) — application-type classifies the application | **E11** | |
| **EPIC-007a S003** — the regulatory activity | **E12**, **E13**, **E15** | E15 arrived last and is the strongest: FDA states the grouping rule in prose rather than leaving it to be inferred from examples |
| **EPIC-007a rendering** | **E16**, **E17**, **E18**, **E19** | E16 shaped it before a line was written — two renderers, not one with a flag. **E17–E19 arrived from the renderers themselves**: each is a placement the blueprint happily seeds and the DTD refuses, and no amount of reading a section list would have surfaced any of them |
| [**ADR-053**](../adr/ADR-053-instance-qualifiers-belong-to-the-placement.md) — an instance qualifier belongs to the placement | **E17**, **E18** | Written *because* there were two. One would have been solved locally; two, in different backbones, a day apart, are a boundary |

**A row moving from *inferred* to *stated by the authority* is worth recording
even though nothing is rebuilt.** E6 → E15 is that: the design did not change,
the confidence did, and only this register would ever show it.

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
