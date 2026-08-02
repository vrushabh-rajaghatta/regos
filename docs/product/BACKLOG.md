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
| **EPIC-017** | **The market-local product tier** — the missing Medicinal Product tier (**"Markets"** in the UI), + trade names and market status | 🟢 Complete | 7 stories, 7/7 DoD; [ADR-039](../adr/ADR-039-the-market-local-product-tier.md) → [`epics/EPIC-017-market-local-product-tier.md`](epics/EPIC-017-market-local-product-tier.md) |

---

## Now

**EPIC-007a — eCTD package generation.** 🟡 Phase 1 open, branch cut
2026-08-02 → [`epics/EPIC-007a-ectd-package-generation.md`](epics/EPIC-007a-ectd-package-generation.md)

**Phase 1 is investigative, and no package-generation code is written until it
closes.** Everything downstream depends on the external validator: which DTD
versions we target, which rules we satisfy, what *accepted* means, and what we
must emit are all decided **by** the oracle rather than before it.

> **The validator is an oracle, not a dependency.** RegOS must not depend on one
> in production. It exists to challenge our interpretation during development
> and release verification — never to define it. The source of truth stays the
> eCTD specifications, not one vendor's reading of them.

Targets **Level 2** confidence (an independent validator accepts the package),
aspires to **Level 3** (matches published FDA/ICH examples), and puts **Level 4**
(a real authority gateway) out of scope. **Task 1 can fail**: if no independent
validator is reachable, the epic's central claim collapses to self-validation
and the priority call gets reconsidered rather than the claim quietly weakened.

**Standing debt, carried deliberately and not attached to any epic:**

| | |
|---|---|
| the nine-form EPIC-016 mutation defect | its own maintenance epic, still unscheduled |
| 15 legacy `record struct` ids | ADR-043 migration, **a whole context at a time, when that context is being worked on anyway** |
| a clean-clone CI check | EPIC-015 — the rule is fixed, the class of defect is not |

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
| — | **EPIC-007a** | **eCTD package generation** | 🟡 **Phase 1 open** — see [Now](#now) | EPIC-004 ✅ |
| 2 | **EPIC-018** | **Labeling & product information** — global/local labels, artwork, indications, contraindications, undesirable effects, interactions, populations | ⚪ Not Started · planned | EPIC-017 ✅ |
| 3 | **EPIC-019** | **Study registry** — clinical & non-clinical studies, cited by applications and submission content | ⚪ Not Started · planned | none |

### Why EPIC-007a is recommended over the runway's next step

The [runway](#the-runway) says **EPIC-018**, and by RIM coverage it is plainly
right — 10 objects against 7a's zero. Three things outweigh that:

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
| **EPIC-018** | **Labeling & product information** — global/local labels, artwork, indications, contraindications, undesirable effects, interactions, populations | ⚪ Not Started | needs EPIC-017 · planned → [`epics/EPIC-018-labeling-and-product-information.md`](epics/EPIC-018-labeling-and-product-information.md) |
| **EPIC-019** | **Study registry** — clinical & non-clinical studies, cited by applications and submission content | ⚪ Not Started | no dependencies — good filler when a larger epic needs breaking up · planned → [`epics/EPIC-019-study-registry.md`](epics/EPIC-019-study-registry.md) |
| **EPIC-010** | **IDMP / product data depth** — substances, ingredients, strength, presentation, packaging, manufacturing | ⚪ Not Started | needs EPIC-016 + EPIC-017 · **split into 10a/10b/10c before cutting a branch** · planned → [`epics/EPIC-010-idmp-product-data-depth.md`](epics/EPIC-010-idmp-product-data-depth.md) |
| **EPIC-020** | **Regulatory process & planning** — objectives, plan/step templates, live plans and dated steps; RIM's spine | ⚪ Not Started | needs EPIC-004 + EPIC-006 + EPIC-017 · deliberately last · planned → [`epics/EPIC-020-regulatory-process-and-planning.md`](epics/EPIC-020-regulatory-process-and-planning.md) |
| **EPIC-007b** | **Publishing — transmission, STF & message formats** — gateway transmission (ESG/AS2), study tagging files, xEVMPD/IDMP messages | ⚪ Not Started | needs EPIC-010 + EPIC-019 · **carries the `Filed` transition**: ADR-046 named EPIC-007 as the milestone, and it belongs to whichever half transmits |
| **EPIC-008** | **Review & approval workflow** — internal review, comments, approvals, e-signatures; the QC/publishing/compilation/validation status pipelines deferred from EPIC-004 | ⚪ Not Started | |
| **EPIC-009** | **Regulatory intelligence / requirements** — what's required per market & product type; keeps the blueprint current | ⚪ Not Started | feeds EPIC-001 |
| **EPIC-011** | **Reporting & dashboards** — portfolio status, submission readiness, activity, cross-market label divergence, Gantt | ⚪ Not Started | consumes EPIC-017, 018, 020 |
| **EPIC-012** | **Reference-data authoring & governance** — data-steward CRUD, change control, tenant-authored/cloned templates & document types | ⚪ Not Started | deferred write-side from EPIC-001; grows with every vocabulary EPIC-006/010/018 add |
| **EPIC-013** | **Audit & activity history** — cross-cutting audit trail (`LastModifiedOn` was deferred to here) | ⚪ Not Started | see the status-history rule below — most of this should never reach here |
| **EPIC-014** | **Notifications** — email & in-app | ⚪ Not Started | EPIC-005 (expiry), 006 (due dates), 020 (slipping steps) all defer their "tell someone" half to here |
| **EPIC-015** | **Production readiness & security** — rate limiting (SEC-001), email delivery, token-table cleanup jobs, **a CI job proving a clean clone builds** | ⚪ Not Started | The clean-clone check is carried debt from EPIC-006 S002: an unanchored `storage/` in `.gitignore` kept `IFileStorage.cs` and `LocalFileStorage.cs` out of the repository entirely. Local builds passed; a fresh clone did not build, and nothing said so. The rule is fixed — the **class** of defect is not. |

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

### The runway

| # | Epic | RIM objects closed | Running coverage | |
|---|---|---|---|---|
| 1 | **EPIC-016** Organization depth | 3 | 16% → ~21% | 🟢 |
| 2 | **EPIC-017** Market-local product tier | 3 | → ~28% | 🟢 |
| 3 | **EPIC-006** HA interactions | 5 | → ~37% | 🟢 |
| 4 | **EPIC-004** Sequences & lifecycle | deepens Submission (13% → high) + 1 | **→ ~39%** | 🟢 |
| 5 | **EPIC-018** Labeling & product information | 10 | → ~55% | ⚪ |
| 6 | **EPIC-019** Study registry | 2 | → ~59% | ⚪ |
| 7 | **EPIC-010** IDMP depth (10a/10b/10c) | 16 | → ~87% | ⚪ |
| 8 | **EPIC-020** Process & planning | 6 | → ~98% | ⚪ |

> **EPIC-007a closes no RIM objects, and that is the honest cost of
> recommending it.** RIM is an object model; a package builder produces a
> *file*. Coverage measures how much of the domain we can describe — it says
> nothing about whether what we describe is correct, and the four
> regulatory-evidence hypotheses EPIC-004 carried are exactly the part this
> table cannot see. Taking 007a first trades a coverage step for the first
> external check on work already done.

Remaining after all eight: `Product Family` (deliberately deferred — inserting a tier *above* a root is cheap) and a handful of RIM relational artifacts we model differently.

### The cross-cutting rule: status history

RIM marks about **ten** statuses "Single / Historical" — Application, Pathway, Submission, HA Submission, Global Label, Market, Commitment, Inspection, Question, Clinical Study, and every Process status. We do this properly on exactly **one** aggregate today (`RegistrationStatusEntry`).

**This is a rule, not an epic:** every time an epic touches an aggregate whose status represents a **business lifecycle**, that status gets the `RegistrationStatusEntry` treatment — append-only, `OccurredOn` vs `RecordedOnUtc`, stored current value for indexed reads. EPIC-017 hits Market Status; EPIC-006 hits four; EPIC-004 hits two. Done opportunistically it costs one child entity per epic. Deferred to **EPIC-013** it costs a migration per aggregate *and* an unwinnable argument about what the historical dates were.

**Activation flags are exempt, and the distinction is the point.** A *lifecycle* records regulatory events — a position an authority took, on a date, that a regulator could ask about later. An *activation flag* records current operability: **do we still use this?** `Registration` (`Planned → Submitted → Approved → Suspended`) is the first; `Organization.Active`, `Product.Archived` and `OrganizationSite.Active` are the second, and none of them carries history. Where a date matters for an activation flag, a single `StatusDate` is proportionate.

Stated this way the rule explains *why* Registration got history and Site did not, rather than leaving future contributors to infer it from examples — and it stops `RegistrationStatusEntry` being cargo-culted onto every boolean.

Per the Rule-of-Three note in `RegistrationCreationPolicy` — **the third occurrence triggers extraction of the shared shape, not the fourth.**

---

_**Now/Next** epics are planned to Phase 1–2 depth. **Later** epics with a linked file are planned to Phase 1 with a Phase 2–3 **sketch** — enough to resume cold after months, explicitly **not approved design**; confirm or replace it in the Phase-2 conversation on pull-in. Later epics without a file are still deliberately coarse placeholders._
