# ADR-034 — Regulatory Templates Are Versioned Blueprints, Owned By The Template And Shared By Default

**Status:** Accepted · **Date:** 2026-07-27 ·
**Related:** [ADR-026](ADR-026-lifecycle-owned-satellites.md) (lifecycle-owned satellites),
[ADR-030](ADR-030-tenant-is-its-own-aggregate.md) (tenant separation),
[ADR-031](ADR-031-tenant-isolation-by-query-filters.md) (isolation enforcement),
[ADR-018](ADR-018-rule-of-three.md) (rule of three)

## Context

EPIC-001 builds the metadata engine's blueprint layer: the machine-readable
answer to *"what does a submission of this type, to this authority, have to
contain?"* — the CTD section tree, its required documents and its validation
rules. STORY-002 is the first slice: the `RegulatoryTemplate` and its versions,
before any sections exist.

Two questions had to be settled before the schema:

1. **Where is the aggregate boundary** between a template and its versions? A
   published version is immutable and will grow large (a whole section tree with
   required documents and rules), while the template's *identity* — which
   authority, which submission type — is stable across versions.
2. **Who owns a template** in a multi-tenant system? The platform curates the
   canonical "FDA IND (CTD)" blueprint, but a customer may eventually want their
   own variant.

## Decision

**A `RegulatoryTemplate` aggregate root that owns its `RegulatoryTemplateVersion`
children, shared across tenants by default.**

1. **The template is the root; versions are its children.** This mirrors
   `ProductDocument → DocumentVersion` (ADR-026): the root assigns sequential
   version numbers, permits at most one open `Draft` at a time, and freezes a
   version on `Publish`. Numbering and the draft/publish transitions are
   invariants inside one consistency boundary — a version number is never
   accepted from outside. Sections, required documents and rules (later stories)
   will hang off a version *within this aggregate* and inherit its immutability.
   Reads never load the aggregate: query handlers project from `RegOSDbContext`
   directly (ADR-016), so the size of a published version is a write-side
   concern only.

2. **A published version is immutable; the seam for temporal validity exists
   now.** `Draft → Published` is one-way; re-publishing throws. Each version
   carries `EffectiveFrom`/`EffectiveTo` so "which blueprint applied on date X"
   is answerable later — the columns are here now because retrofitting temporal
   validity onto published regulatory records is expensive; the *query* logic
   that uses them is deferred (nothing needs it yet).

3. **Templates are platform-shared by default, tenant-extensible by shape.**
   `TenantId` is nullable: `null` ⇒ a shared blueprint visible to every
   authenticated tenant; a value ⇒ a tenant's own. This is exactly the
   `DocumentType` pattern, enforced by the same query filter
   (`CurrentTenant != null && (TenantId == null || TenantId == CurrentTenant)`),
   ADR-031. Tenant-authored and cloned templates are **not built** in this epic
   — but the ownership column is present from day one so that feature never
   needs a migration (ADR-018: build the seam, earn the feature).

4. **Governance lives on the template directly, not in a shared base type yet.**
   The template carries `Source` (provenance, e.g. `ICH eCTD`), a
   `Status` lifecycle (`Active`/`Deprecated` — retired, never deleted), and the
   effective-dating seam. The existing reference vocabularies (Country,
   Authority, SubmissionType, DocumentType) still carry the older ad-hoc shape;
   unifying them behind one `ReferenceItem` base is deliberately deferred to a
   later story rather than retrofitted speculatively here.

5. **A shared blueprint must not reference tenant-owned reference data.** A
   shared template's required documents (a later story) must point at *shared*
   document types, never a tenant's private ones — otherwise one tenant's data
   leaks into everyone's view. Recorded here; enforced when required documents
   arrive.

## Consequences

**Positive**

- The blueprint's invariants (sequential versions, one open draft, immutable
  once published) are enforced by the aggregate and, for version numbering, by a
  unique index — not by convention.
- Shared-by-default matches the product reality (the platform curates the
  canon) and reuses a proven isolation pattern, so no new tenancy mechanism was
  invented.
- Tenant-authored templates, cloning and temporal queries are all reachable
  later as pure additions — the columns and boundaries already accommodate them.

**Negative**

- The aggregate will grow large once a version owns a full section tree. This is
  accepted because templates are read-mostly reference data with no write
  contention, and reads bypass the aggregate entirely.
- Governance shape is now inconsistent across reference data (templates carry
  provenance/status/effective-dating; the older vocabularies do not) until the
  retrofit story runs. The inconsistency is visible and scheduled, not hidden.

## Revisit When

- A customer needs their own or a cloned template — that builds tenant-authored
  templates on the `TenantId?` seam and the shared-must-not-reference-tenant
  rule becomes executable.
- More than one effective published version has to coexist — that turns the
  `EffectiveFrom`/`EffectiveTo` columns into real temporal-selection logic.
- A third reference vocabulary needs the governance shape — that triggers
  extracting the shared `ReferenceItem` base (ADR-018) and the retrofit.
