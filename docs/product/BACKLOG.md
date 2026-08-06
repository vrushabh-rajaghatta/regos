# RegOS Product Backlog

The master list of epics. Nothing gets built that isn't recorded here first. Process: [FEATURE-DEVELOPMENT-FLOW.md](FEATURE-DEVELOPMENT-FLOW.md).

**Status legend:** 🟢 Completed · 🟡 In Progress · ⚪ Not Started
**Epic IDs are stable** (an identifier, not a priority). Order within a section = priority. Pull the top ⚪ into 🟡 one epic at a time; break it into stories via the flow.

---

## Shipped foundation (pre-backlog)

Built before this backlog existed; recorded here so the map is complete. Authority: git history + `docs/adr/` (ADR-001…033).

- 🟢 **Platform & Identity** — users, authentication (JWT cookies), sessions, invitations, password reset/change
- 🟢 **Multi-tenancy & isolation** — Tenant aggregate, fail-closed EF global query filters, three roles (ADR-030–033)
- 🟢 **Organization registry** — tenant-owned organizations (ADR-032)
- 🟢 **Product master** — register / update / archive; `ProductType` incl. Drug & Biologic
- 🟢 **Product Documents** — upload, versioning, lifecycle, local file storage
- 🟢 **Regulatory Application** — create + creation policy (thin: no lifecycle commands exposed yet)
- 🟢 **Submission** — create, attach/remove documents, publish, snapshot (validator is hardcoded, not yet metadata-driven)
- 🟢 **Reference Data — taxonomy** — Country, Authority, SubmissionType, DocumentType (read-only, seed-driven; device-flavored seed only)

## Shipped epics

| ID | Epic | Status | Notes |
|---|---|---|---|
| **EPIC-001** | **The Regulatory Data Dictionary** — complete Reference Data as the governed, standards-aligned controlled-vocabulary + dossier-blueprint backbone; seeded for FDA IND (CTD) + CA/AU/IN | 🟢 Complete | 8 stories; merged to `main` (PR #5) → `epics/EPIC-001-regulatory-data-dictionary.md` |
| **EPIC-002** | **Submission validates against the blueprint** — bind a Submission to a published template version; metadata-driven validation engine; publishing gated on it | 🟢 Complete | 4 stories; [ADR-035](../adr/ADR-035-submissions-bind-to-a-published-template-version.md) → `epics/EPIC-002-submission-validates-against-blueprint.md` |
| **EPIC-003** | **Submission planning & content** — place documents into the bound blueprint's sections; placeholder-shaped content plan / gap view (the dossier builder); placement-aware validation | 🟢 Complete | 4 stories; [ADR-036](../adr/ADR-036-the-dossier-is-structure-placeholders-are-validation.md) → `epics/EPIC-003-submission-planning-and-content.md` |
| **EPIC-006** | **Health-authority interactions** — correspondence, Q&A, meetings, commitments, inspections; the "what's due" view | 🟢 Complete | 8 stories; [ADR-040](../adr/ADR-040-the-health-authority-interaction-context.md) · [ADR-041](../adr/ADR-041-platform-contracts-and-the-identity-that-crosses.md) · [ADR-042](../adr/ADR-042-what-the-interaction-context-turned-out-to-be.md) → [`epics/EPIC-006-health-authority-interactions.md`](epics/EPIC-006-health-authority-interactions.md) |
| **EPIC-005** | **Registration tracking** — what the business *holds*: a product's market authorisations, their status over time, licence numbers and key dates (the RIM core) | 🟢 Complete | 4 stories; [ADR-037](../adr/ADR-037-registrations-are-regulatory-assets-with-derived-visibility.md) → `epics/EPIC-005-registration-tracking.md` |
| **EPIC-016** | **Organization depth** — sites, contacts, divisions; deepen Organization itself | 🟢 Complete | [ADR-038](../adr/ADR-038-organization-depth-roots-and-the-three-filter-shapes.md) · deactivation deferred with a reason → [`epics/EPIC-016-organization-depth.md`](epics/EPIC-016-organization-depth.md) |
| **EPIC-004** | **Sequences & submission lifecycle** — a submission is a numbered sequence; content operation derived and frozen at publish; lifecycle beyond Draft/Published; format; the people named on a filing | 🟢 Complete | 6 stories; [ADR-044](../adr/ADR-044-a-submission-is-a-transmitted-sequence.md) · [045](../adr/ADR-045-the-cumulative-dossier-and-the-derived-delta.md) · [046](../adr/ADR-046-a-submissions-lifecycle-is-only-what-we-did.md) · [047](../adr/ADR-047-publication-metadata-exists-only-when-publication-makes-it-true.md) · [048](../adr/ADR-048-the-people-on-a-filing-belong-to-the-filing.md) · **DTD versions and gateway format deliberately absent** (ADR-047) → [`epics/EPIC-004-sequences-and-submission-lifecycle.md`](epics/EPIC-004-sequences-and-submission-lifecycle.md) |
| **EPIC-007a** | **eCTD package generation** — the sequence folder, both backbones, delivery; and **the first time RegOS was checked by something that did not come from RegOS** | 🟢 Complete | 7 stories, **closed at Level 2a** · [ADR-049](../adr/ADR-049-generation-derives-transmission-creates.md)…[055](../adr/ADR-055-when-an-authority-required-fact-becomes-a-domain-fact.md) · **S008 (Level 3) carried to EPIC-007b — FDA's example packages are not held and the claim is not made** → [`epics/EPIC-007a-ectd-package-generation.md`](epics/EPIC-007a-ectd-package-generation.md) |
| **EPIC-019** | **Study registry** — sponsor-owned studies, placement → study, ICH's `file-tag`, **and the Study Tagging File that unblocked Module 4** | 🟢 Complete | S001–S004 + S002b; **S005 deliberately empty — nobody asked** · [ADR-056](../adr/ADR-056-study-identity-is-owned-by-the-sponsor.md) · [ADR-057](../adr/ADR-057-a-filed-artifact-is-projected-from-a-snapshot.md) · E33–E35 · **owes the E24 continuity refusal → EPIC-021** → [`epics/EPIC-019-study-registry.md`](epics/EPIC-019-study-registry.md) |
| **EPIC-017** | **The market-local product tier** — the missing Medicinal Product tier (**"Markets"** in the UI), + trade names and market status | 🟢 Complete | 7 stories, 7/7 DoD; [ADR-039](../adr/ADR-039-the-market-local-product-tier.md) → [`epics/EPIC-017-market-local-product-tier.md`](epics/EPIC-017-market-local-product-tier.md) |
| **EPIC-010a** | **Substance & composition** — the IDMP root: shared substances, presentations, composition, the component tree, and the query the epic existed for — *"which products contain substance X?"* | 🟢 Complete | 5 stories + [ADR-058](../adr/ADR-058-substances-are-shared-facts-ingredients-are-roles.md); merged to `main` (PR #18) · **EPIC-017's change-case prediction corrected there, not only here** · **does not imply IDMP/xEVMPD readiness (D1)** → [`epics/EPIC-010a-substance-and-composition.md`](epics/EPIC-010a-substance-and-composition.md) |
| **EPIC-018** | **Labeling & product information** — global/local labels, artwork, indications, contraindications, undesirable effects, interactions, populations, and the question it existed for — *"which markets is this product approved for this condition in?"* | 🟢 Complete | 6 stories, DoD audited line by line; [ADR-059](../adr/ADR-059-clinical-statements-are-facts-labels-are-artifacts.md) · **artwork shipped as a label type rather than a child aggregate — capability met, shape changed** · **nothing links a label version to the statements it publishes, and that is a decision (ADR-059 §3)** → [`epics/EPIC-018-labeling-and-product-information.md`](epics/EPIC-018-labeling-and-product-information.md) |
| **EPIC-010b** | **Packs & supply** — the pack, its contents, how it is supplied, how long it keeps, what it looks like, and the question it existed for — *"which packs are authorised in this market, and how are they supplied?"* | 🟢 Complete | 5 stories, DoD line by line; [ADR-061](../adr/ADR-061-a-pack-is-how-a-medicine-is-supplied.md) — **amended at S001, not superseded**, when the dependency graph refused the signed-off design · **closes 5 of cluster B+C's 7; `OtherCharacteristics` and `Devices` refused, not deferred** · merged to `main` (PR #20) → [`epics/EPIC-010b-packs-and-supply.md`](epics/EPIC-010b-packs-and-supply.md) |
| **EPIC-022** | **Country depth** — a country stops being a label on a dropdown: ISO identity, regulatory groupings, expected label languages, accepted stability conditions | 🟢 Complete | 5 stories, DoD line by line; [ADR-062](../adr/ADR-062-a-language-is-a-world-fact.md) · E36–E39 · **D6 amended in place before it was built** — WHO publishes the stability *condition* a market accepts and no climatic zone, and India's 30 °C/70% RH is neither IVA nor IVB (E39) · **closes no RIM object, as forecast** — one object 33% → ~92% · merged to `main` (PR #22) → [`epics/EPIC-022-country-depth.md`](epics/EPIC-022-country-depth.md) |
| **EPIC-023** | **The test suite runs against its own schema** — every database-touching assembly provisions its own database, migrated from the current chain and seeded by the real initializers | 🟢 Complete | 4 stories, DoD line by line; [ADR-064](../adr/ADR-064-the-test-suite-provisions-its-own-schema.md) — **amended in place at S001**, when the measurement it rested on turned out to be the wrong layer · **27 files naming a database → 1**, and a full run leaves the developer's database byte-identical (17,716 sessions before, 17,716 after) · **the idempotent deployment artifact was broken and had never been run** — found while planning, fixed in S003, verified idempotent · **closes no RIM object, as forecast** · 28 s → 39.5 s, and the epic doc says what that bought → [`epics/EPIC-023-test-schema.md`](epics/EPIC-023-test-schema.md) |
| **EPIC-010c** | **Manufacturing** — where a product is made, which sites its licences approve, where each ingredient comes from, and the question the epic existed for: *"is the site we manufacture at on the licence?"* | 🟢 Complete | 4 stories, DoD line by line; [ADR-063](../adr/ADR-063-where-a-product-is-made-is-a-product-fact.md) — **the first `Product.Domain` → `Organization.Domain` edge**, and D2 decided D7 before anybody looked · **builds 1 of cluster D's 4 RIM objects and refuses 3** — the dossier already owns manufacturing narrative at 3.2.S.2 · **three recorded predictions fired**, none of them a count · a misdiagnosed "flake" turned out to be a missing `ORDER BY` tie-breaker → [`epics/EPIC-010c-manufacturing.md`](epics/EPIC-010c-manufacturing.md) |
| **EPIC-024** | **The invariants nothing checks** — the architecture written down and made executable: the bounded-context graph, deterministic ordering on every read path, and the test-database rule | 🟢 Complete | 4 stories, 5/5 DoD; **no ADR — D2 held, and nothing was decided that was not already decided** · **planning changed two of the three items before a line was written**, and the third was vacuous on first contact with the `.csproj` graph · **the graph got *simpler***: 26 edges + 2 exceptions → **26 edges, none** · **orderings terminating uniquely 1 of 124 → 124 of 124**, and 42 carry a written invariant instead of a redundant key · **all four guards demonstrated failing on a deliberate violation** — including EPIC-010c's exact defect, now caught by file and line in 0.3 s · architecture suite 25 → 36 · merged to `main` (PR #25) → [`epics/EPIC-024-invariants-nothing-checks.md`](epics/EPIC-024-invariants-nothing-checks.md) |

---

## Now

🟡 **[EPIC-020 — regulatory process & planning](epics/EPIC-020-regulatory-process-and-planning.md)
— 5 of 8 stories.**
Taken 2026-08-06. RIM's **spine** — the layer that turns a record system into one
that tells you what to do next. Phase 1 approved the same day, and
[ADR-065](../adr/ADR-065-regulatory-process-is-an-optional-bounded-context.md) has
been the implementation contract since.

| | Story | |
|---|---|---|
| 🟢 | **S001** playbooks — versioned, published, frozen; US·FDA·IND seeded | knowledge |
| 🟢 | **S002** objectives — what we intend and why | strategy |
| 🟢 | **S003** instantiation — pinned version, dates derived once | planning |
| 🟢 | **S004** execution — step status, actual dates, the plan board | operation |
| 🟢 | **S005** impact — what a delay actually costs | visibility |
| ⚪ | **S006–S007** wiring — six aggregates gain a nullable `ProcessStepId` | integration |
| ⚪ | **S008** capstone — browser proof, retro | — |

**Two findings worth carrying out of the epic already.** The FDA IND **critical
path is not the 150-day CMC package** — it runs through the pre-IND meeting
track, because FDA's calendar contributes 30 days twice before a protocol can be
written. A test asserting otherwise was **wrong by 33 days**, and that is the
first time this project's model has produced a fact about the domain rather than
about the code. And **each story has found a defect in the one before it** —
S005's impact tests caught an S004 bug that made recording a step *completed
early* impossible.

**ADR-065 was amended twice, both times to make the model smaller** — a seam
removed, a nullable removed, a duplicated field removed. It now carries an
amendment log and a **freeze rule**: corrected in place while the epic is in
flight, superseded and never edited once the branch reaches `main`.

**The philosophy the stories converged on, not planned for:** *record what is
known, not what can be inferred.* Verification at S005: **21 suites green,
architecture 41/41, build and lint clean.**

**Planning checked the six-week-old sketch against the code first, and two of its
three load-bearing claims were wrong:**

- **There is no `ProcessStepId` seam.** The sketch said EPIC-004 and EPIC-006 were
  *"told to leave one; this epic fills it"*. `grep -rn "ProcessStep" src/` returns
  **zero** — and EPIC-006's retro records the absence under *what the change-case
  analysis got right*. So the wiring is **six migrations on shipped tables**,
  cheap only because RegOS is pre-customer. **That window does not widen**
- **"Optional and deletable" was never available.** Inward seams make Process
  *depended upon by five contexts*. The property actually on offer is
  **runtime-optional** — every link nullable, RegOS fully usable with no plan in
  it — and it is now **I1**, with a Definition-of-Done item as its test
- **`ListDueWork` already exists.** EPIC-006 shipped it. *Due* and *late* are two
  facts: an obligation a regulator is waiting on, and our own plan slipping. They
  **share no code**, and *"`ListDueWork` is unchanged and still passes"* is a DoD
  item

**Four RIM objects, not six.** The runway below says EPIC-020 closes 6; it closes
**4**. `ProcessObjectiveGroup` is deferred with its milestone named — nobody asks
its question, and RegOS holds no product with objectives in two markets, which is
[ADR-038](../adr/ADR-038-organization-depth-roots-and-the-three-filter-shapes.md)'s
*demo of an empty table*. **Recorded before implementation rather than discovered
at the retro.**

> **The one sentence the epic turns on**, and the four design choices reduce to
> it: **plans are historical records, not projections.** A plan pins a
> `ProcessDefinitionVersion` forever; dates are derived **once**; re-binding is
> explicit; recalculation is **absent, not missing**.

**`ProcessDefinition`, not RIM's `ProcessPlanTemplate`** — *template* means a
thing you copy and edit, *definition* means a thing you conform to, and the
second is what was designed. The UI calls it a **Playbook**. This is the **third**
time RegOS kept a RIM concept and refused its shape or name, after `Artwork`
(EPIC-018) and `PackAuthorisation` (EPIC-010b); **promotion of that pattern to
`implementation-standards.md` waits for this epic's retro**, because two are
shipped and the third is only decided.

**Split point declared, not held in reserve:** S001–S005 touch no existing
context; S006–S007 are where five get migrations. If velocity slips it splits
there into 020a/020b, and nothing has to be rethought.

**EPIC-010 is finished** — 10a, 10b and 10c are all shipped, which closes the
Product depth work.

### External prerequisites

*Documents RegOS does not hold, that block work no amount of engineering can
unblock. Tracked here rather than inside a story, because their value is to
whoever can go and fetch one.*

| Document | Authority | Unblocks | Why nothing else will do |
|---|---|---|---|
| ~~`ich-stf-v2-2.dtd`~~ **+ `valid-values.xml` + the ICH stylesheet** | ICH M2 | ~~EPIC-019 S002b and S003~~ — **✅ ARRIVED 2026-08-03** | **Three files, not one, and the entry named the wrong one as the vocabulary.** `file-tag/@name` is `CDATA`, so the DTD validates a misspelled tag (**E34**); the enumeration is in `valid-values.xml` (**E33**) and the stylesheet is what checks it. Held at [`docs/evidence/EPIC-019/spec/`](../evidence/EPIC-019/spec/) |
| `form-type.xml` | FDA | eCTD section **1.1 forms** (`m1-1-forms`, refused today) | Same shape: a closed vocabulary named by a wire attribute (**E18**) |
| FDA *Example Submissions for Module 1* v1.4 | FDA | **EPIC-007b S008** — the Level 3 comparison EPIC-007a did not make | A worked example is the only thing that shows convention rather than legality |
| ~~**ICH Q1A(R2)**~~ → **WHO,** *Stability conditions for WHO Member States by Region* | ~~ICH~~ · WHO | ~~EPIC-022 S004~~ — **✅ ARRIVED 2026-08-05** | **The entry named the wrong document, and the right one changed the design.** Q1A(R2) specifies study conditions; it never mapped countries to zones, and **Q1F — which carried zone letters — is withdrawn**. WHO's table publishes the *condition* each member state accepts, so `Country.ClimaticZones` became `Country.StabilityConditions` and no zone is persisted. **India is 30 °C/70% RH — neither IVA nor IVB** (**E39**). ⚠ The table grades its own rows: Australia's is a 2008 ICDRA collation, the other seven are regulator statements |

> **We know there is a controlled vocabulary** is not **we possess the
> controlled vocabulary**. Those are different evidence levels, and the second
> is the one you can build on. The `file-tag` list was recorded as held on the
> strength of a sentence saying it has *"~40 values"*
> ([correction](../evidence/README.md#correction-2026-08-03--the-file-tag-vocabulary-is-not-held));
> four example values appear anywhere in this repository, and four is not a
> vocabulary.
>
> The three options for a closed external code list are **invent values**,
> **accept arbitrary text**, or **wait for the authoritative source**. Only the
> third is available: a free-text box lets someone type `sinopsis` and produce a
> package FDA rejects at the gateway — a worse failure than not shipping the
> feature, because it fails after filing rather than on screen.

> **Historical — the EPIC-019-before-EPIC-018 call, made 2026-08-03.** Both have
> shipped and the call is recorded rather than live. EPIC-007a had changed the
> facts underneath the table: FDA requires a Study Tagging File for every file in
> eCTD 4.2.x and 5.3.1.x–5.3.5.x (**E21**), so **no package could be generated
> for any submission with Module 4 content** — and nothing in RegOS knew a study
> existed. That was not a gap eCTD work could close. The reasoning is kept
> because the *shape* of it recurs: **a downstream epic can discover that an
> upstream one is blocked on an entity nobody planned.**

**Standing debt, carried deliberately and not attached to any epic:**

| | |
|---|---|
| the nine-form EPIC-016 mutation defect | its own maintenance epic, still unscheduled |
| 15 legacy `record struct` ids | ADR-043 migration, **a whole context at a time, when that context is being worked on anyway** |
| a clean-clone CI check | EPIC-015 — the rule is fixed, the class of defect is not |
| **no contact edit screen** | a phone recorded before `ContactPhone.Kind` existed cannot be given one — EPIC-016's surface, found by EPIC-007a |
| **`ContactRoleAssignment`'s uniqueness does not hold** | **Behavioural, not relational — the reason this sits above the four below.** Its unique index on `(ContactId, RoleId)` reads as *one role per contact* and is not: `ContactId` is nullable, and Postgres treats NULLs as distinct, so `(NULL, Reviewer)` may be inserted without limit. An integrity gap the aggregate is currently the only thing preventing |
| four more nullable child foreign keys | `ContactEmail`, `ContactPhone`, `OrganizationIdentifier`, `SiteIdentifier` — same missing `IsRequired()`, no unique index behind it, so the consequence stops at "an orphan is representable". Found with the row above by `AggregateChildArchitectureTests` (EPIC-018 S001) and grandfathered rather than fixed: `NULL → NOT NULL` on a shipped table is a behavioural change, not a formatting one |
| **`npm run lint` fails at baseline** | Six problems, none from any current epic: `react-refresh/only-export-components` in two shadcn files, three *"incompatible library"* compiler warnings, and a `setState`-in-effect error in `ReportStudyDialog`. **The reason this is listed rather than fixed:** EPIC-018 S006 found that `npm run build` had been broken since S001 and no story ran it — a gate nobody executes is a convention wearing a test's clothes. `build` is now in the loop; `lint` needs its baseline cleared before it can join, and that is not a story's side job |

## Next

**Order in this table is priority.**

> **Call made 2026-08-02 — EPIC-007a before EPIC-018.** Stated as a lean rather
> than a certainty, and the reasoning is worth keeping because it is not the
> reasoning that recommended it:
>
> **The project's biggest unknowns are no longer modelling questions. They are
> integration questions.** Submission identity, sequence history, lifecycle,
> validation, placement, content and withdrawals are all built. The next thing
> worth knowing is whether that architecture can emit a regulator-ready package
> — and if it cannot, that is far cheaper to discover now than after ten more
> RIM objects are layered on top.
>
> **What reverses it:** a customer waiting on labeling, or a decision that
> breadth of platform capability is the risk to retire first. EPIC-018 needs no
> new argument if so — it is next in the table either way.

| # | ID | Epic | Status | Depends on |
|---|---|---|---|---|
| — | ~~**EPIC-023**~~ | **The test suite runs against its own schema** | 🟢 **Shipped 2026-08-05** | — |
| — | ~~**EPIC-024**~~ | **The invariants nothing checks** | 🟢 **Shipped 2026-08-06** (PR #25) | — |
| — | ~~**EPIC-020**~~ | **Regulatory process & planning** | 🟡 **In progress — 5 of 8** (2026-08-06) — in [Now](#now) | EPIC-004 ✅ · EPIC-006 ✅ · EPIC-017 ✅ |
| 1 | **EPIC-021** | **Cross-sequence continuity** — the checks no DTD can express | ⚪ Not Started | EPIC-019 ✅ · scoped by [ADR-057 §2](../adr/ADR-057-a-filed-artifact-is-projected-from-a-snapshot.md) · **and one thing no epic supplies** — see below |
| — | ~~**EPIC-010c**~~ | **Manufacturing** | 🟢 **Shipped 2026-08-05** (PR #23), closing EPIC-010 | — |
| — | **EPIC-025** | **[Random Issues](epics/EPIC-025-random-issues.md)** — the register for defects found incidentally | 🟡 **Standing** — never closes, holds no slot | — |

> **EPIC-025 carries no priority number on purpose.** It is a standing register,
> not a unit of work competing for the next slot: bugs are recorded when found
> and fixed inside whatever branch is already touching that code. It is listed
> here so that "nothing gets built that isn't recorded here first" stays true of
> bug fixes too. The epic states its own rules for what belongs in it — and
> rule 4, *promote when it stops being random*, is what keeps it from becoming
> a place problems go to be forgotten.

> **The 2026-08-05 ordering, and the reason it is not simply "hardening first".**
> EPIC-023 and EPIC-024 were both raised unnumbered, because placement is the
> founder's; they were numbered 1 and 2 on the argument that **a foundation is
> strengthened before invariants are made executable on top of it**, and that
> both get more expensive to justify the longer their evidence sits.
>
> **EPIC-021 moved down, and not because it is worth less.** It sat at #1 on an
> ordering argument that remains sound, and it is the only epic in this table
> still waiting on a precondition **nothing in this backlog creates**: a second
> sequence filing the same study. Two hardening epics ahead of it cost days; the
> thing EPIC-021 waits for is not bought by working on EPIC-021.

### Why EPIC-022 enters at the top — and the part of it that should not wait

> Stated as a recommendation. **Placement in this table is the founder's**, and the argument below is deliberately narrow: it is not that country depth is more valuable than continuity, it is that one slice of it is cheapest *this week* and gets more expensive every week after.

**`Country` is the only reference entity in RegOS whose every attribute is for display.** `Code` and `Name` are what you put in a dropdown. The three RIM attributes it lacks — **climatic zone, languages, regions**, all *Multiple* — are what other capabilities **decide** by. Git confirms why: `Country.cs` has had **no behavioural change since the commit that created it**; its only two later commits are a folder move and a repo-wide exception refactor. Everything else in `ReferenceData` has been deepened since.

It pays two debts nothing else will:

| Debt | Why it is Country's to pay |
|---|---|
| **EPIC-018 shipped `LocalLabel.Language` with no way to know which languages a market requires** | Canada needs EN+FR, Belgium NL+FR, Switzerland DE+FR+IT. Labeling cannot answer this; only geography can |
| **`RegionCode` is a dead column** — defaulted to null, omitted by all eight seeds, no mutator, no update path | Exactly the defect [`Substance`](../../src/ReferenceData/RegOS.ReferenceData.Domain/Substances/Substance.cs) refuses by name — *"a persistent property with no acquisition path"*. Country predates the rule |

**The carve-out closed, and cost almost nothing.** This paragraph asked EPIC-010b S003 to add `Region` to `ShelfLifeStorage` while that type was still being authored. **It did not** — S003 shipped the night before this plan was written, scoped to the two concepts it was signed off for. The feared price was *"a migration plus a backfill nobody has the data for"*, and the backfill does not exist: RegOS is pre-customer, so every pack ever recorded is in a dev seed or a throwaway test database. **S004 now owns the field and the match together**, which is arguably the better shape — stability becomes actionable in one place instead of two. **And owning both halves is what saved it:** had S003 taken the carve-out, the field would have shipped as `Region` holding a zone letter, and the source that killed zones would have been read after there was data in the column.

**What reverses the ordering:** a second sequence filing the same study (which makes EPIC-021 genuinely urgent rather than correct-but-early), or a judgement that a 33%-complete lookup table is not where attention belongs while packs are half-built. Both are value calls.

**One external prerequisite, and it is small:** ICH **Q1A(R2)** for the zone boundaries — see [External prerequisites](#external-prerequisites). Nothing else in the epic needs a document RegOS does not hold.

> **EPIC-010b left this table on 2026-08-04** and shipped on 2026-08-05, taken on
> the recommendation below.

> **EPIC-022 left this table on 2026-08-05** and shipped the same day (PR #22).
> It entered at the top on the argument below, **two** clauses of which were
> overtaken before they were acted on: the carve-out, and the external
> prerequisite — which named ICH Q1A(R2) for climatic-zone boundaries that
> document does not contain. Both kept rather than deleted: a corrected
> prediction is worth more than a tidy document.

> **EPIC-018 left this table on 2026-08-04** and shipped the same day. It sat at
> the top of it from the day the runway was written, and nothing ever displaced
> the argument that put it there — EPIC-007a and EPIC-019 were both explicitly
> *"EPIC-018 is next either way"*.

### The recommendation that took EPIC-010b

> **EPIC-010b — before EPIC-021.** Stated as a recommendation rather than a
> decision, because reordering the runway is the founder's. **Recorded and taken
> 2026-08-04.**

**EPIC-018 paid three debts into 10b**, which is the change since this table was
last ordered:

| Deferred by EPIC-018 | With the reason |
|---|---|
| Artwork ↔ packaging component linkage | *"Needs `Packaging` → EPIC-010. Nullable seam only."* |
| SKU, pack size, GTIN | *"EPIC-010's packaging model, and building a second one here would be the speculative creation ADR-018 forbids"* |
| `LocalLabelRevision.DataCarrierCode` | shipped alone — a barcode with no pack to be on |

**And 10b's one open modelling question was answered last week.** The umbrella
epic wrote that `Ingredient`'s polymorphic parent is *"the same problem EPIC-018
solves for `Population` — **reuse whatever decision that epic made**"*. It made
it, and proved it on four owners: **one CLR type, owned per parent, its own
table, an EF configuration helper earned by demonstrated schema equivalence, and
no shared domain base type.** 10b inherits a settled answer rather than
re-deriving one.

It also carries the umbrella's **open decision 5** — *does `Registration` point
at `PackagedProduct`?* — which is what finally lets a licence say **which pack**
it authorises, and is a real gap in EPIC-005's model.

By the test recorded at the foot of this section: RegOS knows what is *in* a
product and **nothing about what it is sold as**. That is an absent area, not a
coherent one being deepened.

**Why EPIC-021 moves down, having been #1.** Its argument was *"the longer RegOS
files sequences without it, the more filings exist for it to be wrong about"* —
and **that premise is not true yet.** RegOS has filed nothing, so the debt is not
compounding, and [ADR-057](../adr/ADR-057-a-filed-artifact-is-projected-from-a-snapshot.md)'s
own Revisit-When names the trigger as *"a second sequence files the same study"*,
which nothing has done. It closes zero RIM objects and deepens an area that is
already coherent. **Correct work, wrong week** — it becomes urgent the day a real
second sequence exists, and its architecture is settled so it stays cheap to take.

**What reverses this:** a customer filing sequences, or a judgement that closing
EPIC-019's owed refusal matters more than opening packaging. Both are value
calls, and value calls are the founder's.

### Historical — why EPIC-007a was recommended over the runway's next step

> **Made 2026-08-02, taken, and shipped.** Kept because the three arguments are
> the reusable part: they are how to weigh *proof* against *coverage*, and that
> question recurs every time the runway says one thing and the risk says another.

The [runway](#the-runway) said **EPIC-018**, and by RIM coverage it was plainly
right — 10 objects against 7a's zero. Three things outweighed that:

1. **Four carried hypotheses resolve there and nowhere else.** Hypotheses 4–7
   are *regulatory evidence*: whether a moved document is `delete`+`new`,
   whether `Append` is ever exercised, whether `modified-file` is recoverable
   after the fact, whether lifecycle belongs to the placement. **No amount of
   thinking settles them** — only a generated package does. They are the only
   debt in the project that cannot be paid down by reasoning.
2. **The product thesis is unproven until something renders it.** ADR-045 says
   RegOS owns cumulative regulatory state and *derives* the transmitted
   increment. Nothing has ever transmitted one. Until a backbone exists, the
   central claim of EPIC-004 is a well-tested assertion about a file nobody has
   produced.
3. **Two decisions are currently defined-and-unreachable**, waiting on exactly
   this: `SubmissionStatus.Filed` (ADR-046 §2, which also expires ADR-044's
   amendment) and the DTD/gateway metadata (ADR-047 §5). Both were deferred
   *with EPIC-007 named as the milestone*.

**The split is what makes this possible.** EPIC-007 as written consumes
EPIC-004, 010 and 019 — but only STF and the xEVMPD/IDMP messages need 010 and
019. The eCTD backbone needs EPIC-004 alone, which is now shipped.

**What would reverse it:** a customer waiting on labeling, or a judgement that
breadth of RIM coverage beats depth of proof right now. Both are value calls,
and value calls are the founder's.

> **Historical — the ordering call made 2026-08-01.** EPIC-006 was taken before
> EPIC-004 on the argument that RegOS knew *what we submitted* and *what we
> hold*, but not *what is happening with the authority*. Both are now complete
> and the call is recorded rather than live. The reasoning is still the test to
> apply: **where does a regulatory affairs team actually spend its day, and
> which epic opens an area that is absent rather than deepening one that is
> already coherent?**

## Later

| ID | Epic | Status | Notes |
|---|---|---|---|
| **EPIC-010** | **IDMP / product data depth** — substances, ingredients, strength, presentation, packaging, manufacturing | ⚪ Not Started | needs EPIC-016 + EPIC-017 · **split into 10a/10b/10c before cutting a branch** · umbrella → [`epics/EPIC-010-idmp-product-data-depth.md`](epics/EPIC-010-idmp-product-data-depth.md) |
| **EPIC-021** | **Cross-sequence continuity** — the checks FDA's review tooling needs and no DTD can express: a study filed twice under two titles, an instance qualifier that drifts, a `study-id` that changes | ⚪ Not Started | **owed by EPIC-019**, scoped by [ADR-057 §2](../adr/ADR-057-a-filed-artifact-is-projected-from-a-snapshot.md) — the check belongs in the generator, reading frozen publication facts, adding no dependency in any direction. **Architecture settled; implementation deferred** because it needs a second sequence filing the same study. E24, E17, E18 |
| ~~**EPIC-020**~~ | **Regulatory process & planning** — objectives, process definitions, live plans and dated steps; RIM's spine | 🟡 **In progress — 5 of 8** — in [Now](#now) | *"Deliberately last" meant its dependencies came first; EPIC-004, 006 and 017 have all shipped, so last now means next* |
| **EPIC-007b** | **Publishing — transmission, STF & message formats** — gateway transmission (ESG/AS2), study tagging files, xEVMPD/IDMP messages | ⚪ Not Started | needs EPIC-010 + EPIC-019 · **carries the `Filed` transition**: ADR-046 named EPIC-007 as the milestone, and it belongs to whichever half transmits |
| **EPIC-008** | **Review & approval workflow** — internal review, comments, approvals, e-signatures; the QC/publishing/compilation/validation status pipelines deferred from EPIC-004 | ⚪ Not Started | |
| **EPIC-009** | **Regulatory intelligence / requirements** — what's required per market & product type; keeps the blueprint current | ⚪ Not Started | feeds EPIC-001 |
| **EPIC-011** | **Reporting & dashboards** — portfolio status, submission readiness, activity, cross-market label divergence, Gantt | ⚪ Not Started | consumes EPIC-017, 018, 020 |
| **EPIC-012** | **Reference data — the browser, then the governance.** Two surfaces: **Reference** (read-only lookup, inside the work) and **Administration** (steward CRUD, change control, tenant-authored/cloned templates & document types) | ⚪ Not Started | deferred write-side from EPIC-001; grows with every vocabulary EPIC-006/010/018 add · **now also owns the read half** — nine vocabularies and ~18 governed lists exist and **no route in the SPA reaches any of them** · **founder's mockup recorded 2026-08-05** → [`epics/EPIC-012-reference-data-authoring-and-governance.md`](epics/EPIC-012-reference-data-authoring-and-governance.md) |
| **EPIC-013** | **Audit & activity history** — cross-cutting audit trail (`LastModifiedOn` was deferred to here) | ⚪ Not Started | see the status-history rule below — most of this should never reach here |
| **EPIC-014** | **Notifications** — email & in-app | ⚪ Not Started | EPIC-005 (expiry), 006 (due dates), 020 (slipping steps) all defer their "tell someone" half to here |
| **EPIC-015** | **Production readiness & security** — rate limiting (SEC-001), email delivery, token-table cleanup jobs, **a CI job proving a clean clone builds** | ⚪ Not Started | Carried debt of one class: a thing nobody runs, so nothing says it is broken. **(1)** The clean-clone check, from EPIC-006 S002 — an unanchored `storage/` in `.gitignore` kept `IFileStorage.cs` and `LocalFileStorage.cs` out of the repository entirely; local builds passed, a fresh clone did not, and nothing said so. ~~**(2)** The test database~~ → **moved to [EPIC-023](epics/EPIC-023-test-schema.md) on 2026-08-05** and **shipped the same day**. **That raised this epic's value rather than lowering it:** EPIC-023 made a green suite trustworthy and did not make anything run it, so a CI job now collects a benefit that is currently latent. |

---

### EPIC-023 — the test suite runs against its own schema

*Raised as its own item on 2026-08-05, after a **third** independent observation
inside one epic. It began as a bullet inside EPIC-015 and was moved out because
it is now evidenced rather than suspected — and because it is nobody's story and
everybody's problem.*

> **Taken and shipped the same day** —
> [`epics/EPIC-023-test-schema.md`](epics/EPIC-023-test-schema.md). **Everything
> below is the entry as it was raised**, kept because it **corrects itself in
> three places**: the design (a measurement), the claim about backfills, and —
> recorded at close — the fact that the measurement which overturned the design
> was *itself* the wrong layer. **27 files naming a database → 1**; a full run
> leaves the developer's database byte-identical; the idempotent deployment
> artifact was broken, and is not any more.

> **The requirement, stated once:** *the automated test environment should
> always execute against a schema produced from the current migration chain.*
>
> Per-run databases are the likely implementation. **The requirement is what
> matters**, and it is what a future reader should hold this against.

**27 test files hard-code `Database=regos`** — the developer's own working
database. Nothing migrates it, so the suite silently assumes somebody already
did.

**Three observations, and the third is the one that settles it:**

| | What it showed |
|---|---|
| **EPIC-022 S001** | the seeder is insert-if-empty, so a migration must carry its own backfill — and today those backfills are only ever proved by hand |
| **EPIC-022 S002** | the database had drifted **five migrations** behind and **18 of 19 suites went red** |
| **EPIC-022 S004** | the database was **one migration behind and the suite stayed green** |

The first two look like an operational lapse. The third is the actual defect:
**a stale schema only turns a test red when a migration happens to touch a read
path some test already exercises.** EPIC-010b added three tables and stayed
green throughout, because its new tests were domain tests and its new tables
were read by nothing older; S004 added two more and did the same. The first
change to an *existing* read path — `ListRegistrationMarkets` reaching
`CountryRegions` — went red immediately.

> **Green means "nothing collided", not "the schema is current".** The suite
> does not test the assumption it depends on: that the schema in the database
> matches the migrations in source control. It assumes somebody has already made
> that true — and when they have not, it usually cannot tell.

**The direction is a per-run database**, not auto-migration on startup. Both
make today's symptom go away; only one fixes the cause:

| | |
|---|---|
| **A database per test run** ✅ | Every run **executes the migrations**, so drift is impossible and the migrations themselves are exercised — ~~including their backfills, which today are only ever proved by hand~~. Tests stop sharing state. It is also what CI must do anyway |
| Auto-migrate in Development | Moves the question from *"did you migrate?"* to *"did you restart?"*. Convenient, and it leaves the schema still unverified by anything |

**What it costs:** the application tier's tests currently lean on seeded
reference data being present, so a per-run database has to seed too — which is
the same initializer the API runs, and therefore a second thing worth proving
rather than assuming.

> **⚠ Struck above, on 2026-08-05, before a line was written.** *"Including their
> backfills"* **is false**, and the distinction is material. A backfill only
> fires on a database that already holds rows; on an empty one, EPIC-022's
> country backfill matches nothing and the **seeder** supplies the stability
> conditions instead. What the epic proves is **schema creation and seeding** —
> not that any migration's backfill does what it says, and not that a populated
> database survives an upgrade.
>
> **Proving migrations both ways stays manual**, as EPIC-010c established. The
> trigger that changes it is named rather than left open: **the first customer
> database.** Until one exists, every populated database is a dev seed or a
> throwaway and an upgrade costs minutes to hand-prove; after one exists, it
> cannot be hand-proved at all.
>
> Left visible so nobody later reads *"the test suite runs the migrations"* and
> concludes the upgrade path is covered.

> **The measurement that changed the design.** The whole 85-migration chain
> applies to an empty database in **0.165 s**; the 14 s first measured was
> `dotnet ef`'s build and startup, not migration. Template databases clone in
> the same 0.15 s, so they buy nothing — and the epic builds the naive
> per-assembly implementation because the measurement permits it, not because it
> is simpler. **"Per run" also has no implementation**: `dotnet test` gives every
> project its own process and nothing coordinates them, so the unit is the
> assembly and the plan says so out loud.

> **A fourth observation, found while planning rather than while building.**
> `dotnet ef migrations script --idempotent` **does not run.** Two migrations
> call `migrationBuilder.Sql()` with no terminating semicolon — which
> `Migrate()` tolerates, because it sends each command separately, and a
> concatenated `DO $EF$ … END $EF$` script does not. The supported idempotent
> deployment artifact has never been executed by anyone.
>
> It is fixed inside this epic and not moved to EPIC-015: **EPIC-015 is about CI
> infrastructure, this is about the correctness of the migration chain**, and a
> story called *"the migration chain is proved"* cannot honestly complete while
> one of the two supported ways of executing it fails.

---

### EPIC-024 — the invariants nothing checks

*Raised 2026-08-05, at EPIC-010c's close. **Its own entry rather than a third
bullet on EPIC-023**, and the split is by subject: EPIC-023 is about the
**environment** a test runs in, this is about **rules the ADRs state that no
test reads**. Same kind, different subject — and folding them together would
make either one harder to schedule alone.*

> **The requirement, stated once:** *an architectural rule the project relies on
> should be enforced by something that runs, not by a person remembering it.*

**Two found, both by EPIC-010c, and neither by review.**

#### 1. The bounded-context dependency graph is not checked

S001 added the first `Product.Domain` → `Organization.Domain` reference and
**the architecture suite stayed green** — because none of its 21 tests looks at
which context may reference which. The edge is held by
[ADR-063](../adr/ADR-063-where-a-product-is-made-is-a-product-fact.md) and a
`.csproj` line.

That ADR closes the **reverse** edge permanently: Organization must never
reference Product, because it would close a cycle. **Nothing would stop someone
opening it.** They would add a project reference, the solution would build, and
the first sign of trouble would be a design that no longer makes sense.

> Worth noting what *did* catch the equivalent one epic earlier: **the
> compiler**, when ADR-061 §3's proposed edge closed a cycle. A cycle is
> self-enforcing; a *direction* is not.

#### 2. A query handler's ordering is not checked for totality

`ListManufacturingOperations` ordered by *(current, `EffectiveFrom` desc)* and
nothing else. Two operations at one site starting the same day tie on every key,
and Postgres may return them either way round — so **a list reorders itself
between reloads for no reason a user can see.**

**`PersistedCollectionOrderTests` exists for exactly this class of defect and
does not cover it:** it governs owned collections in EF configurations, not the
`orderby` in a read handler. The defect surfaced as an intermittent browser
failure that read convincingly as environmental, and cost most of a session
before it was chased properly rather than re-run.

#### 3. Nothing says a database test must use the test database

*Added 2026-08-05, at EPIC-023's close, by that epic's own honest reading of what
its guard proves.*

[`TestDatabaseConventionTests`](../../tests/Architecture/RegOS.Architecture.Tests/TestDatabaseConventionTests.cs)
proves **no file under `tests/` names a database**. It does not prove **every
database test uses `RegOSTestDatabase`**. A new assembly could build a context
from `TestPostgres.Server` directly and pass the guard, because that connection
string lives in the one file the rule permits.

**The rule that would hold is a `.csproj` scan of perhaps fifteen lines:** *any
test project referencing `RegOS.Persistence` must also reference
`RegOS.TestSupport`.*

> The same shape as the two above, and found the same way — not by review, but by
> asking at close what the green tick actually asserted. **A guard that catches
> the defect you had is not the same as a guard that describes the architecture
> you want**, and the gap between them is invisible until someone writes both
> sentences down.

#### Why this is a real epic and not a chore

Both are **decisions that already exist and are already written down**. Neither
needs a design; both need a test. That is unusually cheap for the confidence it
buys — and the argument for doing it while the reasoning is fresh is that a
third instance would be found the same expensive way the second one was.

---

## RIM alignment

The DIA **Regulatory Information Management Reference Model** is the industry's object model for this domain. We are not implementing it wholesale — but it is the best available map of what a complete RIM contains, and measuring against it tells us what we are missing and in what order it matters.

**Where we stand (assessed 2026-07-31, against the RIM object model's 56 objects):** roughly **9 objects (16%)** have a RegOS counterpart, carrying **8–33%** of their RIM attributes each — call it **5–8% of the total attribute surface**.

That number is less interesting than *which* 16%: it is the transactional spine (Application → Submission → Content → License), the hardest part to model well. And the naming already lines up — `RegulatoryApplication` ≡ Application, `Registration` ≡ License-Registration, `ProductDocument` ≈ Content, `SubmissionDocument` ≈ Submission Content.

### Where we deliberately differ

Three divergences are **not** gaps and should be defended, not closed:

1. **The dossier blueprint engine.** `RegulatoryTemplate` → `Version` → `TemplateSection` → `RequiredDocument` → `ValidationRule` has **no RIM equivalent**. RIM's nearest neighbour (Process Plan Template) is a *process timeline* template, not dossier content structure. RIM assumes a content plan is authored per submission; we derive it from governed metadata. **That gap is the product.**
2. **Tenancy.** RIM is a single-enterprise model with no tenant concept. `TenantId` + fail-closed filters (ADR-030–032), and the Tenant/Organization split, are additions — and a better answer than RIM's, which conflates "us" with "a regulatory party".
3. **Bitemporal status history.** RIM annotates attributes "Single / Historical" and stops. `RegistrationStatusEntry` distinguishes `OccurredOn` from `RecordedOnUtc`, so a migrated 2019 authorisation reads honestly. **Better than the spec.**
4. **A licence authorises many packs, and authorisation is a dated relationship.** RIM says `License → Packaged Product`, *Parent, **Single***. One EU marketing authorisation covers several pack sizes and one US NDA covers several package configurations, so *Single* is wrong. RegOS models `PackAuthorisation(RegistrationId, PackagedProductId, AuthorisedOn)` in the Registration context — **because a pack exists before its licence does** (the reasoning EPIC-017 used for markets) and because packs frequently arrive years later by variation, which a foreign key cannot date. `Registration` is untouched, `Product` stays independent, and *"which packs are still unlicensed?"* needs no invented planned registration (EPIC-010b D2, ADR-061 §3).

### The runway

| # | Epic | RIM objects closed | Running coverage | |
|---|---|---|---|---|
| 1 | **EPIC-016** Organization depth | 3 | 16% → ~21% | 🟢 |
| 2 | **EPIC-017** Market-local product tier | 3 | → ~28% | 🟢 |
| 3 | **EPIC-006** HA interactions | 5 | → ~37% | 🟢 |
| 4 | **EPIC-004** Sequences & lifecycle | deepens Submission (13% → high) + 1 | **→ ~39%** | 🟢 |
| 5 | **EPIC-018** Labeling & product information | 10 | → ~55% | 🟢 |
| — | *taken out of order 2026-08-03* — EPIC-019 shipped before 018, because Module 4 was blocked and labeling was not | | | |
| 6 | **EPIC-019** Study registry | 2 | → ~59% | 🟢 |
| 7 | **EPIC-010** IDMP depth (10a 🟢 / 10b 🟢 / 10c 🟢) | **11 built · 5 refused** | *see below* | 🟢 |
| 8 | **EPIC-020** Process & planning | ~~6~~ **4 built · 2 refused** | *see below* | 🟡 |

> **EPIC-020's four, corrected at Phase 1 rather than at the retro.** This row
> said **6 → ~98%** from the day the runway was written, and the epic plan closes
> **4**. `Process Objective Group` is deferred with its milestone named — nobody
> asks its question — and RIM's `Process Plan Template` becomes
> **`ProcessDefinition`**, a different shape for the same concept, so it is
> counted as built rather than as RIM's object. **The running-coverage figure is
> therefore left blank rather than restated**: it was arithmetic on a number that
> was wrong, and inventing a replacement percentage would repeat the mistake in a
> new digit. Coverage is a map of the domain, not an implementation target.

> **EPIC-018's ten, counted honestly.** Nine aggregates cover the ten RIM
> objects: **Artwork is not its own aggregate** — it is a `LocalLabel` of type
> `ARTWORK` with its own dated revisions, because a printed carton proved to be
> another controlled local label rather than a child entity. The capability is
> there; the shape is not RIM's. Recorded so the coverage figure is not read as
> a claim about structure.

> **EPIC-007a closes no RIM objects, and that is the honest cost of
> recommending it.** RIM is an object model; a package builder produces a
> *file*. Coverage measures how much of the domain we can describe — it says
> nothing about whether what we describe is correct, and the four
> regulatory-evidence hypotheses EPIC-004 carried are exactly the part this
> table cannot see. Taking 007a first trades a coverage step for the first
> external check on work already done.

> **EPIC-010's figure is a count, not a percentage, and that is the point.**
> *Restated 2026-08-05 at 010c's planning.* Two of its three splits refused
> objects **on the record, each with a reason and a named falsifier**:
> `OtherCharacteristics` and `Devices` (10b), and `Manufacturing Process`,
> `Manufacturing Process Step` and `Mfg Process Step Materials` (10c, because
> the blueprint already carries that narrative as document sections).
>
> **RIM object count is a map of the domain, not an implementation target.**
> *"87% complete"* invites a reader to go looking for the missing 13%; **"11
> built, 5 refused"** says what happened.
>
> **The convention, stated once so it is not read as special pleading for one
> epic:** *where architectural review deliberately refuses parts of the reference
> model, runway reporting distinguishes **implemented** objects from
> **intentionally refused** objects, rather than reporting a completion
> percentage.* A refusal carrying a reason and a written trigger is a stronger
> artifact than a number, and a percentage silently converts one into the
> other.

Remaining after all eight: `Product Family` (deliberately deferred — inserting a tier *above* a root is cheap) and a handful of RIM relational artifacts we model differently.

### The cross-cutting rule: status history

RIM marks about **ten** statuses "Single / Historical" — Application, Pathway, Submission, HA Submission, Global Label, Market, Commitment, Inspection, Question, Clinical Study, and every Process status. We do this properly on exactly **one** aggregate today (`RegistrationStatusEntry`).

**This is a rule, not an epic:** every time an epic touches an aggregate whose status represents a **business lifecycle**, that status gets the `RegistrationStatusEntry` treatment — append-only, `OccurredOn` vs `RecordedOnUtc`, stored current value for indexed reads. EPIC-017 hits Market Status; EPIC-006 hits four; EPIC-004 hits two. Done opportunistically it costs one child entity per epic. Deferred to **EPIC-013** it costs a migration per aggregate *and* an unwinnable argument about what the historical dates were.

**Activation flags are exempt, and the distinction is the point.** A *lifecycle* records regulatory events — a position an authority took, on a date, that a regulator could ask about later. An *activation flag* records current operability: **do we still use this?** `Registration` (`Planned → Submitted → Approved → Suspended`) is the first; `Organization.Active`, `Product.Archived` and `OrganizationSite.Active` are the second, and none of them carries history. Where a date matters for an activation flag, a single `StatusDate` is proportionate.

Stated this way the rule explains *why* Registration got history and Site did not, rather than leaving future contributors to infer it from examples — and it stops `RegistrationStatusEntry` being cargo-culted onto every boolean.

Per the Rule-of-Three note in `RegistrationCreationPolicy` — **the third occurrence triggers extraction of the shared shape, not the fourth.**

---

_**Now/Next** epics are planned to Phase 1–2 depth. **Later** epics with a linked file are planned to Phase 1 with a Phase 2–3 **sketch** — enough to resume cold after months, explicitly **not approved design**; confirm or replace it in the Phase-2 conversation on pull-in. Later epics without a file are still deliberately coarse placeholders._
