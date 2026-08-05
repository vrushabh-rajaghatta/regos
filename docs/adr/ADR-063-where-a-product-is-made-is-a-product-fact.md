# ADR-063 — Where a Product Is Made Is a Product Fact

**Status:** Accepted · **Date:** 2026-08-05 ·
**Related:** [ADR-061](ADR-061-a-pack-is-how-a-medicine-is-supplied.md) §3 (the cycle that refused the opposite shape, and the dated relationship it produced),
[ADR-039](ADR-039-the-market-local-product-tier.md) (why this attaches to the market-local tier),
[ADR-038](ADR-038-organization-depth-roots-and-the-three-filter-shapes.md) (`OrganizationSite` as a root other aggregates reference by id),
[ADR-058](ADR-058-substances-are-shared-facts-ingredients-are-roles.md) (`Ingredient` as a role, which this gives a source to),
[ADR-018](ADR-018-rule-of-three.md) (copy the dated-relationship pattern; do not abstract it at two),
[EPIC-010c](../product/epics/EPIC-010c-manufacturing.md) D1, D2, D7

## Context

EPIC-010c answers **"where is this product made, and is that site on the
licence?"** Both halves need an `OrganizationSiteId` beside a product id, and
**the Product context cannot currently see one.**

```
Product.Domain        →  SharedKernel, ReferenceData.Domain
Organization.Domain   →  SharedKernel, ReferenceData.Domain
```

They are siblings. Neither references the other, and every existing consumer of
`OrganizationSiteId` outside Organization — `Inspection`, `Contact`,
`Registration` — sits in a context that already depends on Organization.

Two approved decisions both require the edge:

| | Needs a site id in `Product.Domain` |
|---|---|
| **D1** — `ManufacturingOperation`: a market-local product, a site, an operation type, effective dates | yes |
| **D2** — `Ingredient.ManufacturingSourceSiteId`: where this active substance comes from | yes |

**This is a new cross-context dependency, so it is an ADR before it is code.**

### The comparison worth drawing: ADR-061 §3

That decision faced the same shape and reached the opposite outcome, and the
difference is the whole reason this one is written rather than assumed.

| | ADR-061 §3 | Here |
|---|---|---|
| Wanted edge | `Product.Domain` → `Registration.Domain` | `Product.Domain` → `Organization.Domain` |
| Reverse edge exists? | **Yes** — `Registration.Domain` already referenced Product | **No** — Organization references neither |
| Outcome | **cycle; the design was refused** and became `PackAuthorisation` in Registration | **acyclic; the edge is legal** |

> **A legal edge is not a correct edge.** The compiler refused the last one for
> us; it will not refuse this one, so the argument has to be made rather than
> discovered.

## Decision

### 1. `Product.Domain` may reference `Organization.Domain`

The direction is not a preference — **it is forced, and forced twice.**

**By the domain.** *"Where is this made?"* is a question asked **of a product**,
in a product screen, alongside what it contains and how it is packed. The
symmetric question *"what does this site do?"* is real too, and is answered by a
**read** that starts at the site — reads compose across contexts freely
(ADR-006), which is exactly why the write model does not have to.

**By D2.** `Ingredient` lives in `Product.Domain` and cannot move: it is a role
binding a substance to a component (ADR-058). Giving it a source site requires
this edge no matter where `ManufacturingOperation` ends up. Once that edge
exists, hosting the operation in Organization would need the **reverse** edge as
well — and that is a cycle.

So the sequence is: D2 forces `Product → Organization`; having forced it, the
operation belongs in Product too, because the alternative closes a cycle.

### 2. Product manufacture and ingredient source are different stages, deliberately

They are close enough to be merged by a future reader who has not been told why
they are not, so it is stated here rather than left to two docstrings.

| | Question | Grain |
|---|---|---|
| `ManufacturingOperation` | *which sites perform an operation for **this product**?* | the market-local product |
| `Ingredient.ManufacturingSourceSiteId` | *where does **this active substance** come from?* | one ingredient |

**They diverge in cases that are ordinary, not exotic:**

```
Finished product          made at Site Gamma      ← ManufacturingOperation
├── API A                 from Site Alpha         ← Ingredient source
└── API B                 from Site Beta          ← Ingredient source
```

and again under dual sourcing, where one API has two qualified suppliers and the
finished-product operation is unchanged by which one a batch used.

**Neither can be derived from the other.** An operation set cannot say which API
came from where; an ingredient source cannot say who packed the carton. A single
"manufacturer" field would answer neither question and would look like it
answered both.

### 3. One place says where work happens

RIM puts a `Manufacturer` on `Packaging` and on `Packaged Product`, and a site on
`Mfg Business Operation`. **RegOS keeps only the operation**, whose *type* —
API manufacture, finished product, primary packaging, secondary packaging, QC
testing, batch release, importation — already carries the distinction those
columns were making.

Three columns saying the same thing in three places is the duplication
[ADR-061](ADR-061-a-pack-is-how-a-medicine-is-supplied.md) §1's discriminator was
introduced to prevent, and the same call EPIC-022 made when it refused to store a
climatic zone beside the condition it was derived from.

### 4. What the licence approves stays in Registration

**The edge above does not move this.** A licence's approved manufacturing sites
are `Registration`'s, in `Registration.Domain`, which already references
Organization — no new dependency, and the relationship is dated because a site
is added to a licence **by variation, years after approval**.

That is the second occurrence of *licence + thing + `ApprovedOn`* after
`PackAuthorisation`. [ADR-018](ADR-018-rule-of-three.md) says **copy it**: two
occurrences is a pattern, three is the point at which to consider whether a
shared abstraction is earned. Nothing is generalised here.

## Consequences

**Accepted**

- One new project reference: `RegOS.Product.Domain` → `RegOS.Organization.Domain`.
  The graph stays acyclic and **Organization must never reference Product** —
  that edge is now permanently closed, and it is the thing to check if anyone
  later proposes hosting a product fact on a site.
- Product's test and persistence projects inherit the reference transitively.
- The divergence between operations and approved sites is **derived on read and
  reported, never enforced** — the EPIC-005 expiry precedent, used a third time
  after label languages and stability conditions.

**Refused, with reasons**

| | Why not |
|---|---|
| Hold the site as an untyped `Guid` on the product side | ADR-061 §3 refused exactly this: *"the first untyped cross-aggregate reference in the codebase — an escape hatch whose only justification is the cycle it dodges."* There is no cycle here, so there is not even that justification |
| Host `ManufacturingOperation` in `Registration`, beside the approvals | It would compile, and it would be **structure dictating the domain model** — EPIC-010b's own retro lesson. Manufacturing a product is not a fact about a licence, and a market makes product it has not yet licensed |
| A new `Manufacturing` bounded context | A context for one aggregate that reads two others. ADR-018 against speculative creation; the trigger would be manufacturing gaining a lifecycle of its own — qualification, audit, requalification — which is EPIC-008 territory |
| Model `ManufacturingProcess` / `Step` / `StepMaterials` | **Refused, not deferred.** The dossier already owns this: `RegulatoryTemplates` carries **3.2.S.2 Manufacture** and 3.2.P.3.3 as document sections, and structured rows would be a second, competing representation of narrative content. **Falsifier:** a variation-impact capability that must reason over individual process changes — then, and not before |

**Revisit when**

- A site performs an operation for a **global** product rather than a
  market-local one. Today every operation hangs off `MedicinalProductId`
  (ADR-039), because secondary packaging in particular is market-specific and
  the divergence question compares against one market's licence.
- A third *licence + thing + date* relationship appears. That is ADR-018's
  moment to evaluate a shared shape — and, on EPIC-010b's precedent, the
  evaluation may correctly return *no*.
