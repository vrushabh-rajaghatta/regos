# ADR-009 — Command Validation Model

**Status:** Accepted · **Date:** 2026-07-20 · **Supersedes:** nothing ·
**Related:** [ADR-012](ADR-012-shared-semantic-exception-model.md) (shared
exception contract), [ADR-013](ADR-013-ambient-tenant-context.md) (tenant context)

> **Reference correction, 2026-07-20.** This ADR was written against a parallel
> numbering series in which the shared exception contract was "ADR-007" and the
> tenant context was "ADR-008". In the canonical series those numbers belong to
> Module Ownership and Composition Modules. The two decisions this ADR depends
> on are now ADR-012 and ADR-013; references below have been updated. The
> number of *this* ADR is unchanged — it is cited as ADR-009 in four source
> files.

## Context

RegOS rejects commands in two ways, both legitimate, and until now the
architecture did not say which to use.

**Exception-based.** The handler or aggregate raises; middleware maps it.
Used for duplicate emails, already-archived documents, invalid lifecycle
transitions.

**Validation-result-based.** A validator returns every unmet criterion and the
endpoint returns them as data. Used by `PublishSubmission` and by
`GET /submissions/{id}/validation`.

Left undecided, each new command author would pick by preference, and the
Product bounded context is about to add `Create Product`, `Archive Product`,
`Activate Product` and `Publish Product Version`.

## Decision

### The decision tree

```
Can this be decided from the request alone?
│
├─ Yes ──────────────────► DomainException (400)
│                          Required fields, formats, lengths, enum values.
│
└─ No — state must be consulted
   │
   ├─ Is the entity ADDRESSED by the route?
   │        └─ absent ──►  NotFoundException (404)
   │
   ├─ Is it validating a supplied reference or the request contract?
   │                 ──►  DomainException (400)
   │                      Unknown id in the body, cross-field mismatch.
   │
   └─ Is it enforcing lifecycle or business state?
                     ──►  BusinessRuleViolationException (409)
                          Already published, inactive, duplicate.
```

The 404 branch is not optional decoration: it is what distinguishes
`POST /api/products/{id}/documents` (product addressed → 404) from an unknown
product id supplied in a request body (→ 400). Omitting it reintroduces the
defect fixed in commit `cd79eed`.

### When a validation result is used instead of an exception

An exception is the default. A validation result is used only when **all** of
these hold:

1. The rule is a **completeness criterion** — the caller can fix it and retry.
2. Several such criteria can fail **at once**, and reporting only the first
   would make the caller iterate needlessly.
3. The same criteria are meaningful as a **standalone report**, independent of
   attempting the command.

`PublishSubmission` is the only command that qualifies today. Its readiness
rules exist independently at `GET /submissions/{id}/validation`, several can
fail together, and the UI renders them as a checklist.

A **lifecycle** rule never qualifies, even inside such a command: there is no
checklist to work through, only a transition that cannot succeed.

## Consequences

`PublishSubmissionHandler` now checks lifecycle before readiness:

| Condition | Before | After |
|---|---|---|
| Submission does not exist | 404 | 404 |
| Submission is not a draft | **400** + issue list | **409** `BusinessRuleViolationException` |
| Dossier incomplete | 400 + issue list | 400 + issue list |

`GET /submissions/{id}/validation` is unchanged and still reports
`SubmissionAlreadyPublished` — in a report, "this is already published" is
useful information; in a command, it is a conflict.

The web client keeps rendering the readiness checklist from the 400 body and
now surfaces the 409 reason instead of a generic failure.

### Audit

Every rejection point across Platform, ProductDocument, RegulatoryApplication
and Submission was classified against the tree. **All conformed except the
publish path**, because ARCH-001 had already aligned the rest. Representative
sample:

| Rule | Decided from request alone? | Result |
|---|---|---|
| Empty / malformed email | Yes | 400 |
| Name required, name too long | Yes | 400 |
| Empty file upload | Yes | 400 |
| Invalid status filter value | Yes | 400 |
| Product in route does not exist | No — addressed | 404 |
| User in route outside the tenant | No — addressed | 404 |
| Document type in body does not exist | No — reference | 400 |
| Submission type authority mismatch | No — contract | 400 |
| Duplicate email in organization | No — state | 409 |
| Organization inactive | No — state | 409 |
| Document type inactive | No — state | 409 |
| Document already active / archived | No — state | 409 |
| Duplicate attachment | No — state | 409 |
| Application closed | No — state | 409 |
| **Submission already published** | No — state | **409** (was 400) |
| Dossier has no documents | No — completeness | 400 + issues |
| Attached version missing | No — completeness | 400 + issues |

## Alternatives rejected

**Make every publish rejection a 409.** Simpler rule, but it deletes a working
feature: `SubmissionPublishingPage` renders `validation.issues` as a checklist.
Consistency is not worth removing the one place where reporting several
failures at once is the correct behaviour.

**Make every rejection a validation result.** Would require every handler to
return a result type, replacing exceptions we unified in ADR-012 one story ago.

**Introduce FluentValidation.** Explicitly out of scope. No new mechanism is
needed; the question was which of two existing mechanisms to use.
