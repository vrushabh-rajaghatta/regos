# ADR-058 — Substances Are Shared Facts; Ingredients Are The Roles They Play

**Status:** Accepted · **Date:** 2026-08-03 ·
**Related:** [ADR-038](ADR-038-organization-depth-roots-and-the-three-filter-shapes.md) (the three filter shapes; a root is justified by a query),
[ADR-017](ADR-017-shared-kernel-scope.md) (why this is not kernel material),
[ADR-043](ADR-043-entity-identity-derives-from-the-kernel.md) / [ADR-051](ADR-051-two-more-lookups-and-what-a-lookup-is.md) (what a lookup is, and what is not one),
[ADR-018](ADR-018-rule-of-three.md),
[ADR-055](ADR-055-when-an-authority-required-fact-becomes-a-domain-fact.md) (the promotion test),
[EPIC-010a](../product/epics/EPIC-010a-substance-and-composition.md) D1–D3

## Context

EPIC-010a asks *"which of our products contain substance X?"* — a question no
current model can answer, because nothing in RegOS names a substance. Answering
it forces three decisions that outlive the epic, and canon requires them before
code: a cross-context ownership call, the value object every controlled
vocabulary in the platform will use, and the first write path into
`ReferenceData`.

The gap this is written into is unusual for RegOS. `docs/evidence/` holds eCTD
and STF artifacts and **nothing** for ISO 11238, GSRS/UNII, EDQM or WHO ATC.
There is no external fact to record — only a decision to proceed without one,
and what it costs.

## Decision

> **A substance is a scientific fact that exists independently of any product.
> An ingredient is the role a substance plays in one particular product, at one
> strength. They are two things, and the split is what makes the question
> answerable.**

### 1. Why they are two, and not one "ingredient" row

An ingredient row carrying a substance *name* would answer *"what is in this
product?"* and nothing else. **Q1 asks the question backwards** — from the
substance to the products — and a name repeated per product cannot be asked
backwards without matching on strings.

| | Belongs to | Changes when |
|---|---|---|
| `Substance` | the world | chemistry does, or a name is assigned |
| `Ingredient` | one product's composition | the formulation does |

This passes [ADR-055](ADR-055-when-an-authority-required-fact-becomes-a-domain-fact.md)'s
test plainly: paracetamol is paracetamol whether or not a regulator exists.

### 2. `Substance` is shared plus extensible, and the reason is not `AuthorityDivision`'s

`TenantId is null` means platform-shipped; set means a tenant's own. A tenant may
**add** a proprietary compound and may **never mutate** a shared one. This is the
second of [ADR-038](ADR-038-organization-depth-roots-and-the-three-filter-shapes.md)'s
three filter shapes — `TenantId == null || TenantId == CurrentTenant`.

**The shape matches `AuthorityDivision`; the argument does not, and pattern-matching
the conclusion would hide that.**

| | Extensible because |
|---|---|
| `AuthorityDivision` | **no authoritative source exists** for the world's divisions |
| `Substance` | an authoritative source exists and **the tenant's molecule is not in it yet** |

The second resolves itself. An innovator holds a compound before INN assignment;
when licensed terminology arrives, the shared catalogue grows and proprietary
rows migrate into it. `AuthorityDivision`'s reason never resolves.

**`Substance` is not flat master data.** It has a lifecycle (`IsActive`), it is
loaded and mutated as an aggregate, and it is tenant-scoped — so
[ADR-043](ADR-043-entity-identity-derives-from-the-kernel.md) §2's carve-out does
not apply and `SubstanceId` is `sealed class : StronglyTypedId` like any other
root. It lives in `ReferenceData` because it is shared across tenants, not
because it is a lookup.

### 3. `CodedConcept` lives in `ReferenceData.Domain`

```
Product.Domain  →  ReferenceData.Domain  →  SharedKernel
```

The graph already answers this. `Substance` sits in `ReferenceData` and carries
`SubstanceClass` and `SubstanceType` as coded values, so **`CodedConcept` in
`Product` would require `ReferenceData → Product`** — inverting an established
dependency for the platform's most widely shared value object. EPIC-010a's plan
said `src/Product/`; it was written before that consequence was traced, and this
supersedes it.

**And not `SharedKernel`.** [ADR-017](ADR-017-shared-kernel-scope.md) keeps the
kernel to primitives with no domain meaning. Controlled regulatory terminology
is important and is not fundamental — the distinction the kernel's scope exists
to hold.

```
CodedConcept
  System   "regos-internal" during MVP; "edqm", "who-atc", "unii" later
  Code
  Display
```

**`System` is the whole seam.** It is what makes replacing seeded values with
licensed ones a data migration rather than a redesign, and it is why every
seeded row must carry `regos-internal` rather than an empty string.

### 4. `ReferenceData` gains an Infrastructure project

It is Queries-only today: no `Commands`, no `I*Repository`, no Infrastructure
project at all. S001 changes that, and the change is made as **the ordinary
shape every other context already has** rather than as an exception:

```
src/ReferenceData/
  RegOS.ReferenceData.Domain          ISubstanceRepository (SC-002)
  RegOS.ReferenceData.Application     Commands/ beside Queries/
  RegOS.ReferenceData.Infrastructure  SubstanceRepository, DI      ← new
```

A repository implemented anywhere else would be a special case, and a special
case is a thing the next contributor copies.

### 5. The write path is one capability, and stops there

> **10a takes only what §2 requires: *create a tenant-owned substance*.**

Not steward CRUD, not change control, not shared-row editing, not vocabulary
authoring — all of which are **EPIC-012**, and none of which this ADR licenses.
The boundary is stated because the overlap is real and should be deliberate
rather than discovered: an Infrastructure project in `ReferenceData` makes
EPIC-012 cheaper, and that is a side effect, not a scope change.

### 6. The seed is demonstration data, and says so in the file

Six well-known compounds, `System = "regos-internal"`, **every external
identifier null** — no UNII, no CAS, no ATC. The seed file carries the statement
in the file, not only in an epic:

> *Demonstration seed data only. These records intentionally do not represent
> the authoritative GSRS/UNII or ISO 11238 substance registry. Licensed and
> authoritative terminology is introduced separately.*

**This is EPIC-019's lesson applied before it can repeat.** That epic assumed a
vocabulary was held, found it was not one story short of the thing the epic
existed for, and had to correct its own register. Here the absence is declared
at the start, and a null `UniiCode` is a fact about what we do not have rather
than a field nobody filled in.

**Completion of EPIC-010a therefore does not imply IDMP or xEVMPD readiness.**
It provides the model those capabilities need later.

## Consequences

**One new project and one new write path**, both in a context that had neither.
`RegOS.ReferenceData.Infrastructure` joins `RegOS.slnx`; `RegOSDbContext` gains
`Substances` with the shared-plus-extensible filter, which
`TenantFilterArchitectureTests` will require the moment the entity carries a
`TenantId`.

**`CodedConcept` becomes visible to every context that already depends on
`ReferenceData`** — which is most of them. That is intended, and it is also why
its shape must not acquire anything specific to substances.

**A shared substance cannot be corrected by a tenant**, and there is no steward
UI to correct it either until EPIC-012. A wrong shared row is a seed fix and a
deployment. Accepted for MVP, and worth knowing before someone reports it as a
bug.

**`Ingredient` is not decided here.** EPIC-010a D3 settles it as an owned child
of `PharmaceuticalProductDetail` on the grounds that only one parent is
demonstrated; that belongs to S003 and is recorded there.

## Revisit when

- **Licensed terminology is obtained** — EDQM, WHO ATC, GSRS/UNII. The `System`
  field is the seam, and the migration is data. If it turns out not to be, this
  decision was wrong and the ADR that says so should say which part.
- **A second context needs `CodedConcept` to carry something new** — a validity
  period, a translation, a version. Three shapes is when the Rule of Three asks
  whether it is still one value object.
- **A tenant needs to correct a shared substance.** That is EPIC-012's
  conversation, and the answer is unlikely to be "let them" — more likely a
  proposal workflow, which is a different thing entirely.
- **The first proprietary substance is superseded by a licensed one.** Two rows
  then name one molecule, and nothing here says which wins. Deliberately: the
  answer depends on data that does not exist yet.
