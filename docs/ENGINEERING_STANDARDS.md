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
