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

### What no level in this table measures

*Added 2026-08-03, after [ADR-054](../adr/ADR-054-a-study-tagging-file-is-a-projection-over-a-study.md)
found the first instance. **The observation is the founder's**, and it names a
failure mode nothing above was built to see.*

Every level answers **"was it rejected?"** — by a parser, by a rule engine, by a
comparison, by a gateway. **None answers "was it understood?"**

The levels are ordered by *who checked it*, and they shared an assumption that
went unstated until the STF broke it: that an authority which accepts a package
has received what the package says. A study-report leaf carrying no Study Tagging
File is **DTD-valid (2a)**, **breaks no business rule (2b)**, **resembles a
published example (3)**, and would be **accepted by the gateway (4)** — and FDA's
review tool then files it under *"Not Applicable (N/A) or Unassigned Folders"*
(**E21**). The submission arrives, passes every check this register can name, and
silently loses its nonclinical section.

> **Validity and correctness are orthogonal. Acceptance is not comprehension.**

**This is not a fifth level.** A level says how strong a check is; this says what
every check in the table is blind to. It cannot be reached by strengthening the
oracle, because no oracle in the column is looking — only by reading what the
authority says it will *do* with what it receives. Which is precisely why a
conformance guide is a different kind of document from a DTD, and why holding one
is not a weaker substitute for holding the other.

**Its signature in the register**: a source that is *guidance* rather than a
schema, and a consequence that is *filed elsewhere* rather than *rejected*. E21 is
the first. It is unlikely to be the last — any format with a review tool behind it
can have one, and EPIC-007b's IDMP and gateway work is where the next would come
from.

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
| **E10** | **FDA instructs applicants not to use `append`** — *"Do not use the eCTD 'append' lifecycle operator when submitting updated or changed content within study data files that were previously submitted. Updated files should be submitted as replaced and not submitted as new."* Previously recorded as discouragement only: *"the use of 'append' is not common… consider consolidating and using replace"* | FDA *[eCTD TCG](EPIC-007a/spec/fda-ectd-tcg-1-8.md)* v1.8 §2.5 · FDA *[Study Data TCG](EPIC-007a/spec/fda-study-data-tcg-6-2-1.md)* v6.2.1 §7.1.1 · ICH *[STF Spec](EPIC-007a/spec/ich-stf-2-6-1.md)* v2.6.1 §I, §IV | **3** | EPIC-004 **hypothesis 5** — **it supports RegOS not deriving `append` automatically**. ⚠ **Three scopes, all the authority's own.** *Documents*: avoid, with a stated exception (*"adding a single page to a lengthy document"*). *Datasets*: *"do not use"*. **Study Tagging Files: `append` is mandated** — the STF specification requires it for every STF after the first. The caveat recorded on 2026-08-03, before any of the three documents was in hand, was correct and turned out to be understated | EPIC-007a |
| **E11** | `application-type` and `submission-type` are **different attributes on different elements**. `application-type` (`fdaat4` IND, NDA, 510(k)) classifies the **application**; `submission-type` (`fdast1` original-application, `fdast5` annual-report) classifies the **regulatory activity** | [DTD](EPIC-007a/spec/us-regional-v3-3.dtd) + FDA *Submission Types and Subtypes* | **2a** / **3** | **[ADR-050](../adr/ADR-050-application-type-classifies-the-application.md)** — the catalogue was named for eCTD's `submission-type` while enumerating its `application-type`, and hung off `Submission` one tier too low. Renamed and moved in EPIC-007a S001; eCTD's actual `submission-type` still has **no home in RegOS**, and the name is reserved for it. ✅ **`fdaat4` = IND is evidenced as of 2026-08-03** — [`application-type.xml`](EPIC-007a/spec/application-type.xml) v1.1 states it outright (**E30**). RegOS asserted it for a year on its own authority and was right, which does not make the year of asserting it evidence. ⚠ **`fdast5` = annual report is still RegOS's own** — `submission-type.xml` is not held | EPIC-007a |
| **E12** | `submission-type` and `submission-sub-type` are `CDATA #REQUIRED` — **required but not enumerated**, unlike `operation`. A DTD-valid package can carry a meaningless token | [DTD](EPIC-007a/spec/us-regional-v3-3.dtd) lines 87–94 | **2a** | **EPIC-007a S003** *(designed, not yet shipped)* — the token vocabulary is Level 3 only and no parser we own can check it, so RegOS constrains it as curated reference data rather than free text | EPIC-007a |
| **E13** | Sub-type is **not derivable** from an activity's position. The tempting rule *opener ⇒ application, continuer ⇒ amendment* is falsified by FDA example #23: an **opener** whose sub-type is `report` | FDA, *Example Submissions … for Module 1* v1.4, examples #21–#24 | **3** | **EPIC-007a S003** *(designed, not yet shipped)* — sub-type is a business fact the user supplies, never inferred | EPIC-007a |
| **E14** | The operation enumeration is closed in the **ICH backbone too**, not only FDA's regional one — `operation="unchanged"` is rejected as *"not among the enumerated set"* by `ich-ectd-3-2.dtd` | [DTD](EPIC-007a/spec/ich-ectd-3-2.dtd) + `xmllint` · [reproduce](EPIC-007a/poc/how-to-reproduce.md) | **2a** | **[ADR-045](../adr/ADR-045-the-cumulative-dossier-and-the-derived-delta.md)** — E2 proved this for Module 1 only. Both backbones now refuse to say *unchanged*, so the derived delta is forced by the format everywhere, not just regionally | EPIC-007a |
| **E15** | FDA states the activity-grouping rule in prose: *"the submission-id should match the sequence number of the transition sequence"*; *"All subsequent amendment to the original application should have a sub-type of 'amendment' and sub-id = 0001"*; *"If the submission … is creating a new regulatory activity, the submission-id should match the sequence number"* | FDA, *eCTD Submission Types and Subtypes*, Tables 1–2 | **3** | **EPIC-007a S003** — upgrades **E6** from inference-off-worked-examples to the authority saying it outright. It is the `OriginatingSubmissionId` design, in FDA's own words | EPIC-007a |
| **E16** | The two backbones disagree on `checksum`: **`#REQUIRED`** in ICH `index.xml`, **`#IMPLIED`** in FDA `us-regional.xml` | [ICH DTD](EPIC-007a/spec/ich-ectd-3-2.dtd) + [FDA DTD](EPIC-007a/spec/us-regional-v3-3.dtd) | **2a** | **EPIC-007a's rendering design** — a backbone is a contract, not a shared ruleset; see [the epic](../product/epics/EPIC-007a-ectd-package-generation.md#a-backbone-is-a-contract-not-a-shared-ruleset) | EPIC-007a |
| **E30** | **The three attribute-value lists, held in full and machine-readable.** `telephone-number-type`: `fdatnt1` Business / `fdatnt2` Fax / `fdatnt3` Mobile. `applicant-contact-type`: `fdaact1` Regulatory / `fdaact2` Technical / `fdaact3` US Agent / `fdaact4` Promotional Labeling. `application-type`: ten codes, **`fdaat4` = IND** | FDA eCTD Submission Standards — [`telephone-number-type.xml`](EPIC-007a/spec/telephone-number-type.xml) 1.1 · [`applicant-contact-type.xml`](EPIC-007a/spec/applicant-contact-type.xml) 1.2 · [`application-type.xml`](EPIC-007a/spec/application-type.xml) 1.1 · [reading](EPIC-007a/spec/fda-attribute-lists.md) | **3** — but *complete and `status`-flagged*, so an **absence** in one of these lists is now evidence in its own right | **EPIC-007a S006's wiring** — the gate the DTD chain proved was single. **Confirms E11's outstanding assertion**: `fdaat4` = IND was RegOS's own for a year and turns out to be right. ⚠ **`form-type.xml` is still not held**, so `m1-1-forms` stays refused (E18) | EPIC-007a |
| **E31** | **`fdaact2` is the *Technical Contact*, not a manufacturing one** — and no code means *Authorised Representative*; `fdaact3` is a **United States Agent**, a specific FDA obligation on a foreign establishment | [`applicant-contact-type.xml`](EPIC-007a/spec/applicant-contact-type.xml) v1.2 | **3** | **Falsifies half of S006's contact mapping.** RegOS decided `REG → fdaact1, MFG → fdaact2` from the M1 spec's phrase *"the technical contact"*, read as a description when it was a name. **The mapping shrinks to one row.** Better evidence made RegOS emit *less*. **[ADR-055](../adr/ADR-055-when-an-authority-required-fact-becomes-a-domain-fact.md)** — this is the *boundary translation* half of the rule, and `ContactPhone.Kind` from the same day is the *promoted* half | EPIC-007a |
| **E32** | **A value list can carry business rules a schema cannot.** `fdaat7` (IDE), `fdaat9` (PMA) and `fdaat10` (510k) *"should only be used in the cross-reference-application-number element"*; `fdaat8` is *"Do not use. For FDA use only"*. **And there is no De Novo code at all** | [`application-type.xml`](EPIC-007a/spec/application-type.xml) v1.1, its own comments | **3** (prose) over a **2a**-shaped artifact | **Level 2b arriving without a validator** — obligations delivered by an enumeration, a shape this epic had not met. `FDA_510K` and `FDA_PMA` **must keep null tokens** despite codes existing. **`FDA_DENOVO`'s refusal changes meaning**: from *"we have not read the token"* to *"FDA's complete list has no code, so this cannot be filed in eCTD at all"* — a fact about the filing, not a gap in RegOS | EPIC-007a |
| **E29** | **An STF is a study-shaped view over leaves the backbone already holds** — `doc-content` points at `index.xml#leafID`; it carries no files. One per study *per eCTD element* per sequence, sometimes deliberately more. Needs a **study** (sponsor's id, title, and for four CTD sections species / route / duration / type-of-control) and a **`file-tag` per placement** saying what role the document plays. Lifecycle is `append`-chained, latest-`study-identifier`-wins, delete-then-reactivate | ICH M2 *[STF Specification](EPIC-007a/spec/ich-stf-2-6-1.md)* v2.6.1 §I–V | **3** | **ADR-054** — the `file-tag` is **[ADR-053](../adr/ADR-053-instance-qualifiers-belong-to-the-placement.md)'s instance qualifier arriving a fourth time**. The STF's *content* is a projection; its *lifecycle* is not, because it states things about earlier sequences. `ich-stf-v2-2.dtd` is **not held** | EPIC-007a |
| **E28** | **The sequence number starts at 0001 — in the specification**, not merely in guidance: *"should start at 0001, and should not exceed 9999."* Appendix 2 maps the schemes: new `sequence-number` **0001** ↔ old Sequence **0000**. **`0000` is the pre-v2.0 numbering, replaced** | FDA *[M1 Backbone Spec](EPIC-007a/spec/fda-m1-backbone-2-6.md)* v2.6 §III.B.2.b + App. 2 · FDA *[eCTD TCG](EPIC-007a/spec/fda-ectd-tcg-1-8.md)* §2.3, §2.6 | **3** | **⚠ [ADR-044](../adr/ADR-044-a-submission-is-a-transmitted-sequence.md) and S008** — RegOS writes `0000` on **E4**. **Deferred 2026-08-03 by the founder, and the reason is one word: *should*.** Not one source in either direction says **shall**. FDA's specification says a sequence number *"should start at 0001"*; ICH's own example numbers one `0000` and it validates (E4). A *should* is convention, which is Level 3, and this register's own rule is that **2a beats 3 on legality** — so nothing yet obliges RegOS to change, and nothing yet obliges it to stay. **This row remains evidence gathering, not implementation.** What would settle it: a normative *shall* from FDA, a rejected filing, or S008 finding that 0000 changes how an example behaves rather than how it reads | EPIC-007a |
| **E27** | **In Module 1, `modified-file` points at `us-regional.xml`, not `index.xml`** — *"`modified-file="../../../0001/m1/us/us-regional.xml#id34567"`"*, and in a grouped submission it also carries the owning application folder | FDA *[M1 Backbone Spec](EPIC-007a/spec/fda-m1-backbone-2-6.md)* v2.6 §V | **3** | **EPIC-007a S006 wiring** — the generator builds `../{sequence}/index.xml#{leaf}` for *every* backbone. Right for Modules 2–5, wrong for Module 1; unreached only because the wiring is paused | EPIC-007a |
| **E26** | **`us-regional.xml`'s header points at accessdata.fda.gov, not `util/`** — DOCTYPE and a stylesheet PI, both absolute URLs, in a header the spec calls *"always the same"*. Appendix 2 records that local `util/` references are what **v2.0 replaced** | FDA *[M1 Backbone Spec](EPIC-007a/spec/fda-m1-backbone-2-6.md)* v2.6 §II + App. 2 §E.17 | **3** | **EPIC-007a S006 — a defect in shipped code.** The renderer emitted `../../util/dtd/…` and no stylesheet, assuming a regional backbone resolves its DTD the way the ICH one does. Corrected. **It also puts FDA's network reference against the epic's offline Level 2a claim**: tests now validate a locally-rewritten copy and assert the shipped header separately | EPIC-007a |
| **E25** | **FDA permits `999999999` when a DUNS number cannot be obtained** — *"If you are unable to acquire a DUNS number prior to submission, you may enter 999999999."* **The condition is about the applicant, not about the filing system** | FDA *[eCTD TCG](EPIC-007a/spec/fda-ectd-tcg-1-8.md)* v1.8 §3.1.1 | **3** | **EPIC-007a S006** — `applicant-info/id` is mandatory and RegOS models no DUNS field. A **recorded fallback**, not a default: emitting it unconditionally asserts the filer could not obtain one. `Organization.DunsNumber` remains the real answer. *This row exists because the claim was cited from a hand-written PoC for a year before the document was held* | EPIC-007a |
| **E24** | **An instance qualifier must be identical across sequences.** FDA's review tooling identifies continuity by it: mismatched `name`/`manufacturer`/`dosage form` splits one 3.2.P section into two, and a mismatched STF `study-id`/title duplicates the study | FDA *[eCTD TCG](EPIC-007a/spec/fda-ectd-tcg-1-8.md)* v1.8 §4.1, §4.4 | **3** | **[ADR-053](../adr/ADR-053-instance-qualifiers-belong-to-the-placement.md)** — a constraint **no DTD can express**. E17 established the qualifier is required; this establishes it is also *stable*, which is a design constraint on the modelling ADR-053 deliberately left unmade | EPIC-007a |
| **E23** | **FDA forbids `node-extension` outright** — *"Do not use node extensions to create new elements. Although this is described in the ICH eCTD specification, and may be acceptable in some regions, it is not acceptable in any submissions to FDA."* ICH declares it in most content models as `((leaf \| node-extension)*)` | FDA *[eCTD TCG](EPIC-007a/spec/fda-ectd-tcg-1-8.md)* v1.8 §5 item 1b + [ICH DTD](EPIC-007a/spec/ich-ectd-3-2.dtd) | **3** / **2a** | **EPIC-007a rendering** — E16's shape again: the format permits, the authority refuses. Asserted by a test rather than left to implementation habit | EPIC-007a |
| **E22** | **FDA caps the whole path at 150 characters** — *"the length of the entire path must not exceed 150 characters"*. **ICH Appendix 2 allows 230**, so a path legal under ICH can be illegal to FDA | FDA *[eCTD TCG](EPIC-007a/spec/fda-ectd-tcg-1-8.md)* v1.8 §2.4 + ICH Appendix 2 | **3** | **EPIC-007a S004** — enforced in `SequenceFolderGenerator` over *every* emitted path, not only leaves. The stricter of two published limits wins, and RegOS previously checked neither | EPIC-007a |
| **E21** | **Study Tagging Files are required for all files in sections 4.2.x and 5.3.1.x – 5.3.5.x** — documents, not merely datasets. Not required for 4.3, 5.2, 5.3.6 or 5.4. Without one, leaves land in *"Not Applicable (N/A) or Unassigned Folders"* in FDA's review tool. An STF is not a document: study documents are *referenced* by it under a controlled `file-tag`, it has its own lifecycle, and deleting its leaves deletes it | FDA *[eCTD TCG](EPIC-007a/spec/fda-ectd-tcg-1-8.md)* v1.8 §2.8, §3.4.1, §4.2, §4.3 · FDA *[Study Data TCG](EPIC-007a/spec/fda-study-data-tcg-6-2-1.md)* v6.2.1 §7.1.5 (22 file tags) | **3** | **EPIC-007a — enforced.** Recorded 2026-08-03 as future work on the Study Data TCG's dataset-scoped wording; the eCTD TCG shows it applies to **documents**, and **the FDA IND blueprint seeds 4.2.1, 4.2.2 and 4.2.3**. Every IND has Module 4 content. **[ADR-054](../adr/ADR-054-a-study-tagging-file-is-a-projection-over-a-study.md)** says what an STF is; `SequenceFolderGenerator` refuses a placement in the tagged range by name, and permits a withdrawal there because E29 says a deletion submits no STF | EPIC-007a |
| **E20** | **FDA depicts an eCTD v3.2.2 sequence folder as `0000`** — Appendix E's worked structure is rooted at `NDA123456/0000/m4/…`. Footnote 96 adds that **v4.0** folders *"must not have leading zeros ('1', not '0001')"* | FDA *[Study Data TCG](EPIC-007a/spec/fda-study-data-tcg-6-2-1.md)* v6.2.1 App. E + fn. 96 | **3** | **EPIC-007a S008** — corroborates **E4** without displacing **E5**: an FDA illustration showing `0000` as a v3.x sequence folder, and a record that leading zeros are a v3.x convention v4.0 drops. Does **not** settle first-sequence numbering | EPIC-007a |
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
| [**ADR-044**](../adr/ADR-044-a-submission-is-a-transmitted-sequence.md) — a submission is a transmitted sequence | **E4**, **E20** | numbering from 0000 is legal (E4); **E20 adds an FDA illustration that draws one** — still not a statement about first-sequence numbering, which is what E5 contests |
| **EPIC-007a S008** — comparison against FDA's practice *(not yet started)* | **E4**, **E5**, **E20** | the epic's one known open divergence. Three sources, two directions, and S008 exists to explain rather than absorb it |
| [**ADR-055**](../adr/ADR-055-when-an-authority-required-fact-becomes-a-domain-fact.md) — when an authority-required fact becomes a domain fact | **E30**, **E31**, **E32** | the rule EPIC-007a had applied six times without naming: **promote it only if it is an ordinary business concept that would exist if the authority did not.** `telephone-number-type` forced the question — the *concept* (office / fax / mobile) is ordinary and was missing from `ContactPhone`; the *token* is not, and stays in the renderer |
| [**ADR-054**](../adr/ADR-054-a-study-tagging-file-is-a-projection-over-a-study.md) — an STF is a projection over a study | **E21**, **E29** | recorded on 2026-08-03 with no decision resting on it; **the same day, the eCTD TCG turned it into a blocker and the ICH specification arrived.** The STF tests ADR-049 harder than the ZIP did — it needs facts the submission does not hold — and the thesis survives because the answer is to hold the facts, not store the file. **It is also the first entry to expose [what no level in this table measures](#what-no-level-in-this-table-measures)**, which is recorded above rather than in the ADR because an accepted ADR is never edited |
| [**ADR-045**](../adr/ADR-045-the-cumulative-dossier-and-the-derived-delta.md) — the cumulative dossier and the derived delta | **E2**, **E7**, **E14** | **E14 broadened this from one backbone to both.** E2 alone supported *"the regional backbone cannot say `unchanged`"*; with E14 the claim is *"the eCTD format cannot say it anywhere"* |
| [**ADR-047 §6**](../adr/ADR-047-publication-metadata-exists-only-when-publication-makes-it-true.md) — sub-type is an independent axis | **E8** | |
| [**ADR-048**](../adr/ADR-048-the-people-on-a-filing-belong-to-the-filing.md) — the people on a filing belong to the filing | **E3** | reached independently, then corroborated |
| [**ADR-050**](../adr/ADR-050-application-type-classifies-the-application.md) — application-type classifies the application | **E11** | |
| **EPIC-007a S003** — the regulatory activity | **E12**, **E13**, **E15** | E15 arrived last and is the strongest: FDA states the grouping rule in prose rather than leaving it to be inferred from examples |
| **EPIC-007a rendering** | **E16**, **E17**, **E18**, **E19**, **E23** | E16 shaped it before a line was written — two renderers, not one with a flag. **E17–E19 arrived from the renderers themselves**: each is a placement the blueprint happily seeds and the DTD refuses, and no amount of reading a section list would have surfaced any of them. **E23 is E16's shape from the other direction** — a construct the DTD permits and the authority refuses |
| [**ADR-053**](../adr/ADR-053-instance-qualifiers-belong-to-the-placement.md) — an instance qualifier belongs to the placement | **E17**, **E18**, **E24** | Written *because* there were two. One would have been solved locally; two, in different backbones, a day apart, are a boundary. **E24 adds a constraint the ADR could not have known**: the qualifier must be *identical across sequences*, because that is how FDA's review tooling recognises the same node twice. The ADR is not edited — it deliberately models nothing, and this constrains the modelling when it comes |
| **EPIC-007a S004** — the sequence folder | **E4**, **E22** | E22 arrived after the story shipped and changed it: FDA caps a path at 150 characters where ICH allows 230, and the generator now enforces the stricter one |

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
