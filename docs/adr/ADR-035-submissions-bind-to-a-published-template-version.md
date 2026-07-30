# ADR-035 — Submissions bind to a published template version, and are validated against it

**Status:** Accepted · **Date:** 2026-07-30 · **Epic:** EPIC-002

## Context

[ADR-034](ADR-034-regulatory-templates-are-versioned-shared-blueprints.md) made a dossier blueprint *data*: a `RegulatoryTemplate` owning versions that freeze on publish, carrying a section tree, the documents each section requires, and validation rules.

Nothing consumed it. A submission's readiness was decided by three rules written in C# — "not already published", "has at least one document", "attached versions still exist" — which said nothing about what a regulator actually expects an IND to contain.

Connecting the two raises questions the code has to answer: *which* blueprint governs a given submission, what happens when a newer version of that blueprint is published mid-flight, and what the validator should say about checks it cannot yet perform.

## Decision

### 1. A submission is bound to a template **version**, and the binding is persisted

`Submission.BoundTemplateVersionId` is resolved once, at creation, and stored.

Resolving live on every validation would mean a submission's verdict could change without the submission changing — publish a new template version and yesterday's compliant dossier is suddenly short two documents. A regulated record must be judged against a fixed, identifiable standard, and be able to say which one.

### 2. The binding targets `RegulatoryTemplateVersion` — a child entity, not the aggregate root

The usual guidance is to reference aggregate roots. This deliberately does not, because **the version is the governance artifact**. Referencing `RegulatoryTemplate` would leave "which version?" unanswered at every point that matters — validating, rendering sections, comparing history.

It is defensible here because the version has its own identity, is immutable once published, and is only ever *read* by the Submission context. The database foreign key is `Restrict`: a version a submission was judged against can never be deleted.

### 3. Resolution is deterministic: tenant-owned before shared, newest effective published version

Candidate templates are those targeting the submission's type. A tenant's own template shadows the platform-shared one, so the first customisation (EPIC-012) takes effect without changing resolution logic and an ambiguous match is impossible by construction. Within the chosen template, the published version effective today wins, highest version number first.

### 4. Binding is optional, and missing reference data never blocks the business

A submission type with no published blueprint — every device type today — produces an **unbound** submission rather than a failure. So does a template that exists but has no published version: it may still be being authored.

Those are configuration and operational states, not user mistakes. A steward forgetting to publish must not stop a regulatory team from working.

### 5. Blueprint severity is **mapped**, never cast

The blueprint grades a rule (`Error`, `Warning`); the validator grades an issue's effect on readiness (`Information`, `Warning`, `Error`). Two concepts, two bounded contexts, aligned only by coincidence — and not even numerically: blueprint `Error` is `1`, issue `Information` is `1`. A cast would silently downgrade every blocking regulatory rule to a note and publish submissions that should have been stopped. `BlueprintSeverityMapper` makes the translation explicit, fails closed on an unrecognised grading, and is tested on its own.

Readiness follows from severity: **`IsValid` means no `Error`-severity issue**, not "no issues".

### 6. The engine reports three states: passed, failed, **not evaluated**

Rule types this engine cannot execute yet (today `SectionNotEmpty`, which needs document placement) produce a single `Information` issue carrying a structured list of those rule types. It is phrased as a statement about the validator's capability and deliberately says nothing about how the blueprint graded them — "an Error rule was not evaluated" invites a reader to conclude they have an error, which is precisely what is unknown.

The same principle covers an unbound submission, which reports that its completeness was not checked. Silence would make "not checked" indistinguishable from "checked and clean".

### 7. Issue codes stay a closed set; rule codes travel beside them

Consumers switch on `Code`, so it keeps its fixed vocabulary (`BlueprintRuleViolation`). The blueprint rule's own code (`FDA-IND-PDF`) travels on a nullable `RuleCode`, preserving regulatory traceability without making the closed set open.

## Consequences

**Benefits**
- **Immutable governance** — a submission can always name the exact standard it was judged against, and that answer never changes retroactively.
- **Tenant extensibility** — customer-specific blueprints shadow shared ones with no change to resolution.
- **Honest validation** — passed, failed and not-evaluated are three distinct, visible states, all the way to the browser.
- **Open for extension** — a new rule type is one evaluator plus one registration; the orchestrator and existing evaluators are untouched.
- **Testability** — the orchestrator owns all persistence and hands evaluators an immutable context, so rule logic is unit-testable without a database.

**Trade-offs we are consciously accepting**
- **A submission may exist without governance.** Unbound submissions are legitimate, so "valid" can mean "nothing was checked". Mitigated by disclosure, not prevented.
- **The validator advertises its own gaps.** Users see that `SectionNotEmpty` is unexecuted — including, today, an `Error`-severity FDA rule on Module 1.1. Honest, but it is a visible incompleteness until EPIC-003.
- **A child-entity foreign key crosses the aggregate-root guideline**, intentionally, and future maintainers will meet a reference that does not follow the usual pattern.
- **Coverage is by document type, not placement.** A type required by two sections is satisfied by one attachment. Nothing is masked in today's blueprints, but the limit is real until placement exists.
- **Re-binding is unsolved.** An in-flight submission stays on its original version; migrating it to a newer one needs a policy decision that has not been made.

## Alternatives considered

- **Resolve the template live at validation time.** Simpler, no column — rejected: it makes a submission's compliance non-deterministic across time, which a regulated record cannot be.
- **Bind to the template, not the version.** Rejected: pushes "which version?" into every consumer and reintroduces the same non-determinism.
- **Share one `ValidationSeverity` enum across both contexts.** Rejected: it couples two bounded contexts through a shared type and turns a policy decision into an implicit cast.
- **Fail submission creation when no blueprint is found.** Rejected: makes incomplete reference data a business outage.
- **Silently skip rule types that cannot be executed.** Rejected: it reports "valid" while blocking rules went unrun.
