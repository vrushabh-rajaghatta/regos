# Architecture Backlog

This document tracks architectural improvements that have been intentionally deferred.

These are not bugs. They are engineering improvements that should be completed before the platform reaches production maturity.

Statuses were reconciled against the codebase on **2026-07-20**. Three items had
been completed without being closed here. An item is `Done` only when the code
was checked, and the check is named.

| Item | Status |
|---|---|
| AB-001 Consolidate EF Core DbContexts | Done |
| AB-002 Global Exception Handling | Done |
| AB-003 Replace Temporary Reference Data | Done |
| AB-004 Pin .NET SDK with global.json | Done |
| AB-005 Product Document deferred enhancements | Planned (mixed) |
| AB-006 Extract Shared Workspace Pattern | Planned |
| AB-007 Reject out-of-range enum values in aggregates | Planned |

---

## AB-001 - Consolidate EF Core DbContexts

Status: **Done** (verified 2026-07-20)

Verified by: a single `RegOSDbContext` in `RegOS.Persistence` is the only
context in the solution; every module's query handlers inject it. See
[ADR-016](adr/ADR-016-persistence-access-model.md).

Priority: High

Description

Currently each capability owns its own DbContext.

- ProductDbContext
- RegulatoryApplicationDbContext

Before introducing additional capabilities (Submission, Authority, Organization, Country, etc.) these should be consolidated into a single:

RegOSDbContext

inside

RegOS.Persistence

Reason

- Single migration history
- Single database model
- Simpler schema evolution
- Easier transaction management

Target Sprint

Sprint 10

---

## AB-002 - Global Exception Handling

Status: **Done** (verified 2026-07-20)

Verified by: `ExceptionHandlingMiddleware` registered at `Program.cs:92`; the
three shared exception types of
[ADR-012](adr/ADR-012-shared-semantic-exception-model.md); and **zero of 35
endpoints containing a try/catch** — the duplication described below is gone.
`GetProductEndpoint.cs:16` carries a comment explaining the absence.

Which rejection maps to which status is now decided by
[ADR-009](adr/ADR-009-command-validation-model.md) rather than by handler-author
preference.

Priority: Medium

Description

Introduce a consistent exception handling middleware.

Original state (resolved)

Unhandled exceptions are returned directly from ASP.NET.

Desired state (achieved)

Map domain/application exceptions to ProblemDetails responses.

Concrete instances observed (all resolved)

- Product Document upload with a duplicate name (same ProductId + Name)
  hits the unique constraint. The orphaned file is cleaned up correctly,
  but the response is a raw 500 rather than a 409 Conflict with a clear
  "a document with this name already exists" message. A global filter
  should map DbUpdateException (unique violation) -> 409.
- Every command endpoint currently repeats a local try/catch mapping
  (ProductNotFound -> 404, BusinessRuleViolation -> 400). The Sprint 18.8
  lifecycle endpoints add a DocumentNotFound -> 404 and an invalid-
  transition InvalidOperationException -> 409 mapping in the same repeated
  style. These should all collapse into one middleware once this item is
  implemented.

Target Sprint

TBD

---

## AB-003 - Replace Temporary Reference Data

Status: **Done** (verified 2026-07-20)

Verified by: `Country`, `Authority`, `DocumentType` and `SubmissionType`
aggregates in `RegOS.ReferenceData.Domain`, each with a list query and endpoint;
`Organization` in `src/Organization`, with `ListOrganizations`.

Priority: High

Description

Applications require:

- Country
- Authority
- Applicant Organization

These capabilities should replace temporary assumptions in the Application workflow.

Target Sprint

Sprint 11-13

---

## AB-004 - Pin .NET SDK with global.json

Status: Done

Reason

When the project introduces CI/CD or multiple contributors,
pin the .NET SDK version using global.json to ensure
consistent builds across environments.

Target

Before first production deployment.

---

## AB-005 - Product Document Capability - Deferred Enhancements

Status: Planned

Priority: Mixed (see per-item notes)

Description

Sprint 18 delivered the Product Document capability as a foundation
(domain, reference data, persistence, upload, workspace). The following
enhancements were intentionally deferred to keep the sprint a clean
vertical slice. Captured here so they are not forgotten.

Document lifecycle & versions

- Activate / Archive lifecycle - DONE in Sprint 18.8 (aggregate guards,
  application commands, POST .../activate|archive endpoints, workspace
  actions). Activation now requires a current version.
- Version 2+ uploads (AddNewVersion is modelled in the aggregate but not
  exposed via any command/UI yet).
- Document download / file retrieval endpoint.
- Document deletion / retirement policy (soft delete, archival rules).
- Audit trail (status changes, metadata updates) - backs the workspace
  History tab.
- Electronic signatures.

Reference data & organization

- Organization-specific Document Types (the DocumentType model already
  supports a nullable OrganizationId; org-scoped code uniqueness and the
  management UI are deferred).

Discovery & reuse

- Document search and filtering.
- Document templates.
- Submission Document reuse - Sprint 19 (SubmissionDocument will reuse the
  Product Document patterns).
- Usage tracking ("where is this document used?") - backs the workspace
  Usage tab; becomes populated once Submission Documents exist.

AI / processing

- AI processing pipeline (triggered on DocumentUploaded).
- OCR and metadata extraction - backs the workspace AI Insights tab.

Storage & security

- Blob storage provider (Azure/S3) behind the existing IFileStorage
  abstraction - the seam is already in place; only a new adapter is needed.
- Virus scanning on upload.

Target Sprint

Distributed across Sprint 19+ per the notes above.

---

## AB-006 - Extract Shared Workspace Pattern

Status: Planned (re-verified 2026-07-20 — still outstanding)

Verified by: four `*WorkspaceLayout.tsx` / `*WorkspaceNavigation.tsx` pairs still
exist under `features/regulatory/{products,applications,submissions,documents}`,
and `web/regos-web/src/shared/` contains no `workspace/` directory.

Priority: Medium

Description

The workspace shell (header + entity title + status badge + sidebar
navigation + content outlet, plus breadcrumbs) is now implemented five
times: Product, Application, Submission, and Product Document workspaces,
with list pages following a matching page/table pattern.

The pattern is proven (well past the "abstract after the third example"
threshold). Extract a shared:

- shared/workspace/WorkspaceLayout
- shared/workspace/WorkspaceHeader
- shared/workspace/WorkspaceNavigation
- shared/workspace/WorkspaceBreadcrumbs

Reason

- Remove duplication across four+ concrete workspaces.
- One place to evolve workspace UX.

Constraint

Do this as an isolated refactor sprint/milestone, not mixed with feature
work, so the change is low-risk and diffable against four stable examples.

Target Sprint

After Sprint 18 (dedicated refactor).

---

## AB-007 - Reject Out-of-Range Enum Values in Aggregates

Status: Planned (found 2026-07-20 during ORG-001)

Priority: Medium

Description

ASP.NET model binding turns an out-of-range integer into an enum value
without complaint, so a request body can persist a value that has no
name. `POST /api/products` with `{"type": 99}` returns **201 Created**
and stores a product whose `ProductType` renders as `99`.

`Organization.Create` was fixed during ORG-001 with `Enum.IsDefined`,
raising `DomainException` — decidable from the request alone, therefore
400 ([ADR-009](adr/ADR-009-command-validation-model.md)). The same guard
is missing elsewhere.

Known instances

- `Product.Register` — `ProductType`.
- Audit every other aggregate factory and behavior method that accepts an
  enum: ProductDocument, RegulatoryApplication and Submission were not
  checked.

Reason

An unnamed enum value is invalid data that every read path downstream has
to tolerate, and it is silently accepted at the one place designed to
reject invalid input.

Constraint

Not fixed during ORG-001 deliberately — it is outside that slice. Do it
as one sweep with a test per aggregate, so the guard is applied
consistently rather than wherever someone happened to notice.

Target Sprint

Milestone 6 (Hardening), or sooner if a bad value reaches production
data.
