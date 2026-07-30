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

---

## Now

| ID | Epic | Status | Notes |
|---|---|---|---|
| **EPIC-003** | **Submission planning & content** — place documents into the bound blueprint's sections; placeholder-shaped content plan / gap view (the dossier builder); placement-aware validation | 🟡 In Progress | 4 stories → `epics/EPIC-003-submission-planning-and-content.md` |

## Next

| ID | Epic | Status | Depends on |
|---|---|---|---|
| **EPIC-004** | **Sequences & submission lifecycle** — eCTD sequence numbering; lifecycle beyond Draft/Published | ⚪ Not Started | EPIC-003 |
| **EPIC-005** | **Registration tracking** — a product's registrations per market: status, license/approval numbers, key dates, renewals, variations (the RIM core) | ⚪ Not Started | EPIC-001 |

## Later

| ID | Epic | Status | Notes |
|---|---|---|---|
| **EPIC-006** | **Health-authority interactions** — correspondence, questions, commitments, deadlines | ⚪ Not Started | RIM core |
| **EPIC-007** | **Publishing & eCTD export** — package builder, technical validation, output formats | ⚪ Not Started | |
| **EPIC-008** | **Review & approval workflow** — internal review, comments, approvals, e-signatures | ⚪ Not Started | |
| **EPIC-009** | **Regulatory intelligence / requirements** — what's required per market & product type; keeps the blueprint current | ⚪ Not Started | feeds EPIC-001 |
| **EPIC-010** | **IDMP / product data depth** — substances, pharmaceutical products, packaging, xEVMPD | ⚪ Not Started | |
| **EPIC-011** | **Reporting & dashboards** — portfolio status, submission readiness, activity | ⚪ Not Started | |
| **EPIC-012** | **Reference-data authoring & governance** — data-steward CRUD, change control, tenant-authored/cloned templates & document types | ⚪ Not Started | deferred write-side from EPIC-001 |
| **EPIC-013** | **Audit & activity history** — cross-cutting audit trail (`LastModifiedOn` was deferred to here) | ⚪ Not Started | |
| **EPIC-014** | **Notifications** — email & in-app | ⚪ Not Started | |
| **EPIC-015** | **Production readiness & security** — rate limiting (SEC-001), email delivery, token-table cleanup jobs | ⚪ Not Started | |

---

_Later epics are deliberately coarse — placeholders so we don't lose them. Each is refined through Phase 1–2 of the flow when pulled into **Now**._
