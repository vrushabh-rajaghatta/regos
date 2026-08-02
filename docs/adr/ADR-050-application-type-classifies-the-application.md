# ADR-050 — `ApplicationType` Classifies the Application

**Status:** Accepted · **Date:** 2026-08-02 ·
**Amends the terminology of:** [ADR-031](ADR-031-tenant-isolation-by-query-filters.md),
[ADR-034](ADR-034-regulatory-templates-are-versioned-shared-blueprints.md),
[ADR-040](ADR-040-the-health-authority-interaction-context.md),
[ADR-043](ADR-043-entity-identity-derives-from-the-kernel.md),
[ADR-047](ADR-047-publication-metadata-exists-only-when-publication-makes-it-true.md),
[ADR-048](ADR-048-the-people-on-a-filing-belong-to-the-filing.md) ·
**Related:** [ADR-049](ADR-049-generation-derives-transmission-creates.md),
[ADR-018](ADR-018-rule-of-three.md),
[evidence E11](../evidence/README.md)

## Context

RegOS had a reference-data catalogue called `SubmissionType`, holding `FDA_IND`,
`FDA_NDA`, `FDA_510K`, `TGA_ARTG` and eight more, and `Submission` carried a
`SubmissionTypeId`.

**Three independent findings say that is the wrong name in the wrong place**, and
none of them is a matter of taste.

**1. The external evidence ([E11](../evidence/README.md), level 2a/3).** FDA's
regional DTD carries *two* attributes on *two* elements:

| eCTD | Attaches to | Example values |
|---|---|---|
| `application-type` | the application (`application-number`) | `fdaat4` — IND, NDA, 510(k) |
| `submission-type` | the **regulatory activity** (`submission-id`) | `fdast1` original-application, `fdast5` annual-report |

RegOS's `SubmissionType` catalogue enumerates the **first** of those and had
borrowed the name of the **second**. eCTD's actual `submission-type` had no home
in RegOS at all.

**2. The blueprint already agreed.** `RegulatoryTemplate` was scoped by this
value — and a CTD blueprint is *the dossier structure for an IND*. That is an
application kind. The concept had been doing application-level work in two
places while living on `Submission`.

**3. The shape.** A `RegulatoryApplication` carries one `ApplicationNumber`, so
one application is one IND, and **every sequence filed under it is an IND**. A
value invariant across every child of an aggregate belongs to the aggregate.

## Decision

### 1. `SubmissionType` is renamed to `ApplicationType`

Catalogue, id type, DTO, endpoint, route (`/api/reference-data/application-types`)
and frontend, with no compatibility layer. The API says what the model says.

`ApplicationTypeId` remains a `readonly record struct` — flat master data, no
children, no lifecycle ([ADR-043 §2](ADR-043-entity-identity-derives-from-the-kernel.md)).
A rename is not the moment to convert it.

### 2. Ownership moves to `RegulatoryApplication`

`Submission.SubmissionTypeId` is removed; `RegulatoryApplication.ApplicationTypeId`
is **required**. A blueprint is resolved through the submission's application
rather than from the submission itself — the same rule, one hop, correct owner.

### 3. The authority invariant moves with it — it is not new

`CreateSubmissionHandler` already enforced *"the type must belong to the
application's authority."* It ran **per sequence**, against a value that never
varied, **after** the application existed. It now runs **once**, in
`RegulatoryApplication.Create`, on the aggregate that holds both `AuthorityId`
and `ApplicationTypeId`.

The factory therefore takes the `ApplicationType` **entity**, not its id: a
factory that must reason about a thing needs the thing. Existence remains a
policy question (`IRegulatoryApplicationCreationPolicy`); belonging is the
aggregate's.

### 4. The name is vacated, not reused

**`SubmissionType` is reserved for eCTD's actual `submission-type`** —
original-application, annual-report, IND safety report — which classifies a
regulatory activity, not an application. It arrives in its own story, after this
migration has settled. One migration, one story.

## Consequences

- **The six ADRs listed above remain correct; their terminology does not.** Read
  `SubmissionType` in them as `ApplicationType`. Nothing they decided is
  reversed — this is a rename plus a change of owner, and their reasoning is
  untouched. They are not edited.
- **No behavioural change beyond ownership.** No rule is added or removed; one
  moves to where it belongs and consequently fires earlier.
- **A misclassified application is now impossible to create**, rather than
  discoverable when its first sequence is filed.
- **The migration refuses to invent data.** An application with no submission to
  infer a type from aborts the migration by id. Its `Down` is honestly lossy: a
  sequence that carried a type differing from its application's cannot be
  restored, because after the move that information no longer exists.
- **The backfill made the old defect visible.** Applications whose earliest
  sequence disagreed with their own identity now carry that disagreement as
  data — which is what a per-sequence classification always permitted, and what
  this decision prevents from recurring.

## Revisit When

- **eCTD's `submission-type` is introduced.** It must not land on
  `RegulatoryApplication` beside this one. They are orthogonal axes, and decision
  4 exists so the name is free when that story arrives.
- **An application legitimately changes its type.** Nothing supports
  reclassification today, deliberately — an IND that becomes an NDA is a new
  application with a new number, not the same one relabelled.
- **A second authority's catalogue contradicts the one-type-per-application
  shape.** Decision 2 rests on one application being one application number; an
  authority that files several kinds under one number would falsify it.
