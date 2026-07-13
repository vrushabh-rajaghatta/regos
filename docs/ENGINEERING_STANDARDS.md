# Engineering Standards

These standards apply across the entire RegOS platform.

---

## ES-001

Feature-first frontend architecture.

---

## ES-002

One bounded context per capability.

---

## ES-003

Use business terminology.

Example

Product

RegulatoryApplication

Submission

---

## ES-004

Aggregate factories use

Create()

instead of Register().

---

## ES-005

Repositories are write-only abstractions.

---

## ES-006

Cross-capability reads use Readers.

Never repositories.

---

## ES-007

Capabilities expose Contracts.

Implementation stays internal.

---

## ES-008

Aggregates never cross capability boundaries.

Readers return immutable models.

---

## ES-009

Every Aggregate owns its invariants.

Application Layer orchestrates use cases.

---

## ES-010

Stay Within the Sprint.

Architecture is introduced only when required by the current sprint.

---

## ES-011

Every sprint must end with a demonstrable feature.

---

## ES-013

Pin the .NET SDK using `global.json`.

All developers and CI environments should build RegOS using the same SDK version to ensure reproducible builds.

---

## ES-014 — Prefer Identity References Across Aggregates

> Aggregates reference other aggregates by their strongly typed identifiers (e.g., `CountryId`, `ProductId`, `AuthorityId`) rather than navigation properties. Object relationships are materialized in the persistence layer, while the domain model remains persistence-agnostic and focused on business invariants.

---

## ES-015 — Master Data Records Use Deterministic Identities

> Master Data (Countries, Authorities, Languages, Dosage Forms, etc.) are assigned stable identifiers defined by the platform. Transactional aggregates (Products, Regulatory Applications, Submissions) continue to generate identities within the domain. This gives us two distinct identity strategies, each appropriate for its purpose.

---

## ES-016 — Platform Data Is Initialized via IDataInitializer

> Platform-owned data is initialized by `IDataInitializer` implementations during application startup. Today it is Master Data; tomorrow it may be default roles, workflow templates, document types, or regulatory taxonomies. Rather than inventing a new bootstrapping mechanism each time, we extend the initialization pipeline. Implementations must be idempotent.

---

## ES-018 — Prefer Lifecycle State Transitions Over Deletion

> Long-lived business entities (Products, Organizations, Regulatory Applications, Submissions) should generally move through lifecycle states (e.g. Active ↔ Inactive) rather than being physically deleted. This preserves history, supports auditability, and aligns with regulatory systems, where records are typically retained even when they are no longer active.

---

## ES-019 — Initializers Are Additive and Idempotent

> Every `IDataInitializer` has a single responsibility: ensure its capability has the minimum required platform-owned data. Initializers must be additive and idempotent — never deleting or overwriting existing customer data, and safe to run on every application startup. As RegOS grows, startup remains a simple loop over registered initializers, and each capability bootstraps itself independently without special orchestration logic.
