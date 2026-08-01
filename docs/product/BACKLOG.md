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
| **EPIC-005** | **Registration tracking** — what the business *holds*: a product's market authorisations, their status over time, licence numbers and key dates (the RIM core) | 🟢 Complete | 4 stories; [ADR-037](../adr/ADR-037-registrations-are-regulatory-assets-with-derived-visibility.md) → `epics/EPIC-005-registration-tracking.md` |

---

## Now

*Nothing in flight.* EPIC-016 and EPIC-017 both shipped; the next epic is the
open call below — and a small maintenance epic is queued behind them (nine
forms still carry the EPIC-016 mutation defect; see the EPIC-017 retro).

## Next

Three candidates, **planned to Phase 1 depth** (see [RIM alignment](#rim-alignment) for why these three). Order among them is an open priority call — see the note below the table.

| ID | Epic | Status | Depends on |
|---|---|---|---|
| **EPIC-016** | **Organization depth** — sites, contacts, divisions; deepen Organization itself | ✅ **Complete** | ADR-038 · deactivation deferred with a reason → [`epics/EPIC-016-organization-depth.md`](epics/EPIC-016-organization-depth.md) |
| **EPIC-017** | **The market-local product tier** — the missing Medicinal Product tier (**"Markets"** in the UI), + trade names and market status | ✅ **Complete** | ADR-039 · seven stories, 7/7 DoD → [`epics/EPIC-017-market-local-product-tier.md`](epics/EPIC-017-market-local-product-tier.md) |
| **EPIC-004** | **Sequences & submission lifecycle** — eCTD sequence numbering, content operation (new/replace/append/delete), lifecycle beyond Draft/Published | ⚪ Not Started | EPIC-003 (placement makes a sequence a diff of placements, not an inference); S005 needs EPIC-016 → [`epics/EPIC-004-sequences-and-submission-lifecycle.md`](epics/EPIC-004-sequences-and-submission-lifecycle.md) |

> **Open call — EPIC-017 vs EPIC-004.** They are genuinely independent: sequences live inside `Submission` and never touch `ProductId`; the tier work never touches submission internals. Neither makes the other harder, so this is a **value** decision, not a dependency one. EPIC-017 completes an epic already in flight (EPIC-005's portfolio views currently answer *"what do we hold in Canada?"* with a global product code); EPIC-004 completes nothing in flight but may be what a customer is waiting on. **EPIC-016 goes first either way** — it blocks EPIC-006 and part of EPIC-004, and is blocked by nothing.

## Later

| ID | Epic | Status | Notes |
|---|---|---|---|
| **EPIC-006** | **Health-authority interactions** — correspondence, Q&A, meetings, commitments, inspections; the "what's due" view | ⚪ Not Started | needs EPIC-016 · planned → [`epics/EPIC-006-health-authority-interactions.md`](epics/EPIC-006-health-authority-interactions.md) |
| **EPIC-018** | **Labeling & product information** — global/local labels, artwork, indications, contraindications, undesirable effects, interactions, populations | ⚪ Not Started | needs EPIC-017 · planned → [`epics/EPIC-018-labeling-and-product-information.md`](epics/EPIC-018-labeling-and-product-information.md) |
| **EPIC-019** | **Study registry** — clinical & non-clinical studies, cited by applications and submission content | ⚪ Not Started | no dependencies — good filler when a larger epic needs breaking up · planned → [`epics/EPIC-019-study-registry.md`](epics/EPIC-019-study-registry.md) |
| **EPIC-010** | **IDMP / product data depth** — substances, ingredients, strength, presentation, packaging, manufacturing | ⚪ Not Started | needs EPIC-016 + EPIC-017 · **split into 10a/10b/10c before cutting a branch** · planned → [`epics/EPIC-010-idmp-product-data-depth.md`](epics/EPIC-010-idmp-product-data-depth.md) |
| **EPIC-020** | **Regulatory process & planning** — objectives, plan/step templates, live plans and dated steps; RIM's spine | ⚪ Not Started | needs EPIC-004 + EPIC-006 + EPIC-017 · deliberately last · planned → [`epics/EPIC-020-regulatory-process-and-planning.md`](epics/EPIC-020-regulatory-process-and-planning.md) |
| **EPIC-007** | **Publishing & eCTD export** — package builder, technical validation, output formats, STF, xEVMPD/IDMP messages | ⚪ Not Started | consumes EPIC-004, 010, 019 |
| **EPIC-008** | **Review & approval workflow** — internal review, comments, approvals, e-signatures; the QC/publishing/compilation/validation status pipelines deferred from EPIC-004 | ⚪ Not Started | |
| **EPIC-009** | **Regulatory intelligence / requirements** — what's required per market & product type; keeps the blueprint current | ⚪ Not Started | feeds EPIC-001 |
| **EPIC-011** | **Reporting & dashboards** — portfolio status, submission readiness, activity, cross-market label divergence, Gantt | ⚪ Not Started | consumes EPIC-017, 018, 020 |
| **EPIC-012** | **Reference-data authoring & governance** — data-steward CRUD, change control, tenant-authored/cloned templates & document types | ⚪ Not Started | deferred write-side from EPIC-001; grows with every vocabulary EPIC-006/010/018 add |
| **EPIC-013** | **Audit & activity history** — cross-cutting audit trail (`LastModifiedOn` was deferred to here) | ⚪ Not Started | see the status-history rule below — most of this should never reach here |
| **EPIC-014** | **Notifications** — email & in-app | ⚪ Not Started | EPIC-005 (expiry), 006 (due dates), 020 (slipping steps) all defer their "tell someone" half to here |
| **EPIC-015** | **Production readiness & security** — rate limiting (SEC-001), email delivery, token-table cleanup jobs | ⚪ Not Started | |

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

| # | Epic | RIM objects closed | Running coverage |
|---|---|---|---|
| 1 | **EPIC-016** Organization depth | 3 | 16% → ~21% |
| 2 | **EPIC-017** Market-local product tier | 3 | → ~28% |
| 3 | **EPIC-006** HA interactions | 5 | → ~37% |
| 4 | **EPIC-004** Sequences & lifecycle | deepens Submission (13% → high) + 1 | → ~39% |
| 5 | **EPIC-018** Labeling & product information | 10 | → ~55% |
| 6 | **EPIC-019** Study registry | 2 | → ~59% |
| 7 | **EPIC-010** IDMP depth (10a/10b/10c) | 16 | → ~87% |
| 8 | **EPIC-020** Process & planning | 6 | → ~98% |

Remaining after all eight: `Product Family` (deliberately deferred — inserting a tier *above* a root is cheap) and a handful of RIM relational artifacts we model differently.

### The cross-cutting rule: status history

RIM marks about **ten** statuses "Single / Historical" — Application, Pathway, Submission, HA Submission, Global Label, Market, Commitment, Inspection, Question, Clinical Study, and every Process status. We do this properly on exactly **one** aggregate today (`RegistrationStatusEntry`).

**This is a rule, not an epic:** every time an epic touches an aggregate whose status represents a **business lifecycle**, that status gets the `RegistrationStatusEntry` treatment — append-only, `OccurredOn` vs `RecordedOnUtc`, stored current value for indexed reads. EPIC-017 hits Market Status; EPIC-006 hits four; EPIC-004 hits two. Done opportunistically it costs one child entity per epic. Deferred to **EPIC-013** it costs a migration per aggregate *and* an unwinnable argument about what the historical dates were.

**Activation flags are exempt, and the distinction is the point.** A *lifecycle* records regulatory events — a position an authority took, on a date, that a regulator could ask about later. An *activation flag* records current operability: **do we still use this?** `Registration` (`Planned → Submitted → Approved → Suspended`) is the first; `Organization.Active`, `Product.Archived` and `OrganizationSite.Active` are the second, and none of them carries history. Where a date matters for an activation flag, a single `StatusDate` is proportionate.

Stated this way the rule explains *why* Registration got history and Site did not, rather than leaving future contributors to infer it from examples — and it stops `RegistrationStatusEntry` being cargo-culted onto every boolean.

Per the Rule-of-Three note in `RegistrationCreationPolicy` — **the third occurrence triggers extraction of the shared shape, not the fourth.**

---

_**Now/Next** epics are planned to Phase 1–2 depth. **Later** epics with a linked file are planned to Phase 1 with a Phase 2–3 **sketch** — enough to resume cold after months, explicitly **not approved design**; confirm or replace it in the Phase-2 conversation on pull-in. Later epics without a file are still deliberately coarse placeholders._
