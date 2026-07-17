# Architecture Backlog

This document tracks architectural improvements that have been intentionally deferred.

These are not bugs. They are engineering improvements that should be completed before the platform reaches production maturity.

---

## AB-001 - Consolidate EF Core DbContexts

Status: Planned

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

Status: Planned

Priority: Medium

Description

Introduce a consistent exception handling middleware.

Current state

Unhandled exceptions are returned directly from ASP.NET.

Desired state

Map domain/application exceptions to ProblemDetails responses.

Concrete instances observed

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

Status: Planned

Priority: High

Description

Applications require:

- Country
- Authority
- Applicant Organization

These capabilities should replace temporary assumptions in the Application workflow.

Target Sprint

Sprint 11-13

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

Status: Planned

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
