# ADR-059 — Clinical Statements Are Regulatory Facts; Labels Are Published Editorial Artifacts

**Status:** Accepted · **Date:** 2026-08-04 ·
**Related:** [ADR-040](ADR-040-the-health-authority-interaction-context.md) (a new bounded context takes an ADR first; reads compose rather than a supertype),
[ADR-039](ADR-039-the-market-local-product-tier.md) (the market-local tier these hang from; principle 7),
[ADR-058](ADR-058-substances-are-shared-facts-ingredients-are-roles.md) (`CodedConcept` and where it lives),
[ADR-038](ADR-038-organization-depth-roots-and-the-three-filter-shapes.md) (filter shapes; a root is justified by a query),
[ADR-043](ADR-043-entity-identity-derives-from-the-kernel.md) (identity),
[ADR-018](ADR-018-rule-of-three.md) (rule of three),
[ADR-031](ADR-031-tenant-isolation-by-query-filters.md) (isolation),
[ADR-016](ADR-016-persistence-access-model.md) (persistence access model),
[EPIC-018](../product/epics/EPIC-018-labeling-and-product-information.md) D1–D6

## Context

RegOS knows what a product **is** — its markets, presentations and composition —
and what has been **filed** about it. It does not know what the product is
**approved to say**: what it treats, who must not take it, what it does to you,
what it clashes with. Today that lives in a PDF nobody can query, and *"which
markets is this indication approved in?"* is answered by opening ten documents.

EPIC-018 introduces **ten RIM objects**, the largest single block in the model
outside the IDMP tail, in two clusters: label artifacts (global label, local
label, artwork) and structured product information (indication,
contraindication, undesirable effect, interaction, interactant, population,
other therapy).

This ADR is written at **S001**, not at the capstone, because `src/Labeling/` is
created in S001 and a new bounded context takes an ADR *first* — the same
sequence [ADR-040](ADR-040-the-health-authority-interaction-context.md) followed.

**One decision the epic's sketch listed is already closed.** It proposed
modelling a `CodedConcept` value object "once, and reuse it everywhere". That
exists — [ADR-058](ADR-058-substances-are-shared-facts-ingredients-are-roles.md)
§3 put it in `ReferenceData.Domain`, and three vocabularies already sit beside
it. EPIC-018 inherits it rather than deciding it, and §7 records what it
inherits along with it.

## Decision

> **A clinical statement is a regulatory fact about a product in a market. A
> label is an editorial artifact that publishes some of those facts at a point in
> time. They are two things, they version on different clocks, and nearly every
> decision below follows from that one sentence.**

### 1. What the principle decides

It is stated first because it is load-bearing. A contributor who holds it can
re-derive the rest; a contributor who does not will reach for a foreign key.

| Question | The principle answers |
|---|---|
| Does an indication belong to a label? | **No.** The label *publishes* it. The fact exists whether or not any label has been written. |
| Do labels version independently of the statements? | **Yes.** A label revision may change wording without changing what is approved, and an approval may change without a label revision yet issued. |
| Does `ProductDocument` acquire market semantics? | **No.** A document stores content. Meaning is Labeling's job (§6). |
| Can a publication relationship arrive later without a migration? | **Yes** — because nothing points that way yet (§3). |

### 2. One bounded context: `src/Labeling/`

Two tests, and it passes both.

**Distinct ubiquitous language.** *Core data sheet, local label, artwork,
indication, contraindication, undesirable effect, interactant, population* —
none of these words appear anywhere in RegOS today, and none of `Registration`,
`Submission`, `Sequence` or `Commitment` appear in this cluster.

**Distinct primary users.** Regulatory Operations owns registrations,
submissions and correspondence. Medical Writing and Labeling own the core data
sheet and the clinical statements beneath it. Those are different people doing
different work on different cycles.

**And both clusters live in it, not one each.** Splitting label artifacts from
clinical statements would draw a context boundary through the middle of a single
editorial act: an indication, a contraindication and a warning all change
*through a label revision*. They are inseparable in practice, and RIM treats
them as one neighbourhood for the same reason. This is
[ADR-040](ADR-040-the-health-authority-interaction-context.md) §1's argument
applied to a different cluster.

**It is not `Product`.** `Product` answers what a product *is*;
`MedicinalProduct` answers where it is sold. What it is approved to *say* is a
third question with its own lifecycle, and folding ten objects into
`Product.Domain` would give that context a second job.

#### The local label is `LocalLabel`, not `Labeling`

**RIM calls the local label `Labeling`, and we deviate.** A context named
`Labeling` containing an aggregate named `Labeling` reproduces exactly the
namespace-equals-type collision that S000 removed fourteen `using` aliases to
delete — and which
[`MedicinalProduct`](../../src/Product/RegOS.Product.Domain/Product/MedicinalProduct.cs)
cites as the reason both product tiers share one folder.

`GlobalLabel` / `LocalLabel` also names the pair symmetrically, which `GlobalLabel`
/ `Labeling` does not. **This is the only place EPIC-018 departs from RIM's
noun**, and it departs for a mechanical reason rather than a modelling one.

### 3. Clinical statements hang off `MedicinalProduct`, and nothing links them to a label

`Indication`, `Contraindication`, `UndesirableEffect` and `Interaction` are
roots carrying a `MedicinalProductId` — the market-local tier
[ADR-039](ADR-039-the-market-local-product-tier.md) established — by id only
(ES-014). An indication is approved **for a product in a market**; the same
molecule may be approved for a different indication in another country, and that
is the ordinary case rather than the exception.

**The link from a label version to the statements it published is deliberately
absent**, and the absence is the decision rather than an omission. It sounds like
one nullable foreign key until someone asks:

| The question a `LabelVersionId` column would have to answer | |
|---|---|
| **Partial publication** | a label carries three of five approved indications — is the fourth unpublished, or withdrawn? |
| **Wording differences** | the approved fact and the printed sentence are not the same string, and both matter |
| **Withdrawn statements** | a statement removed from the label but still approved |
| **Historical wording** | what version 2 said, after version 3 changed it |
| **One statement split into two** | which is the continuation, for the purposes of history? |

Every one of those is a **versioning** question. Solving them accidentally — by
adding a column and discovering the semantics later — is how a model acquires a
shape nobody chose. When a user asks *"which indications are in the Japanese
label?"*, that question opens its own conversation and its own ADR.

**What this epic does answer is the other direction**, and it needs no such link:
*"which markets is indication X approved in?"* reads `Indication →
MedicinalProduct → Country`, structurally identical to EPIC-010a's substance
query and starting — for the same tenant-isolation reason — at a filtered root
rather than at a child.

### 4. `Population` is an owned entity, per parent, with its own identity

RIM gives `Population` four optional parent links, exactly one of which is set.
That is a **relational-modelling artifact, not a domain truth**. The domain truth
is *"a clinical statement applies to a population"*.

| Option | Rejected because |
|---|---|
| four nullable FKs on one table | needs a check constraint to express what an aggregate boundary expresses for free; every query knows about four columns |
| one table + discriminator | loses referential integrity, which is a poor trade in a record a regulator may audit |
| **owned collection per parent** | **chosen** |

**It is an entity, not a value object.** A population is not merely *"children,
2–12 years"* — it is added, edited, removed, reordered and eventually approved.
Anything with that lifecycle needs identity, and an identity-less value object
would make "remove *that* one" unexpressible.

**The shape is shared; the storage is not.** Each parent owns its own rows, and
that is not only cleanliness: EPIC-010a proved by defect that **an owned value is
tracked against exactly one owner** — one `CodedConcept` instance shared across
six substances persisted five nulls. `Population` carries coded values of its own
(gender, race, physiological condition), so the same trap sits inside it.

**Four near-identical mappings are accepted, and the duplication is answered in
persistence, not in the domain.** EF configuration helpers may centralise the
column mapping; a shared domain base type may not — that would be an abstraction
across four aggregate roots, which ES-014 forbids and
[ADR-018](ADR-018-rule-of-three.md) does not license. Four copies of
configuration is cheaper than a fake abstraction that cannot then evolve.

### 5. Versioning copies the pattern, not the implementation

`GlobalLabel` + `GlobalLabelVersion` is the same problem
`RegulatoryTemplate` + `RegulatoryTemplateVersion` already solved: draft →
publish → supersede, with effective dating and an immutable published version.

| Reused | Not reused |
|---|---|
| the two-level root-plus-version shape | the code |
| draft/publish/deprecate transitions | its `record struct` identity (ADR-043 §1 pending list) |
| effective-from / effective-to dating | its `ReferenceData` assumptions — a template is platform-shipped; a label is tenant-owned |

Identity is copied from
[`CommitmentId`](../../src/Interaction/RegOS.Interaction.Domain/Commitments/CommitmentId.cs),
never from the nearest id. Copying `RegulatoryTemplateId` would propagate one of
the fifteen record structs ADR-043 is retiring.

### 6. `ProductDocument` remains content storage; `Labeling` owns the regulatory meaning

> **A document is a file with a name and versions. What that file *means* —
> that it is the Japanese local label, effective from a date, derived from core
> version 4 — belongs to `Labeling` and nowhere else.**

```
GlobalProduct
  └── ProductDocument        the file: PDF, Word, artwork asset, SPL
        └── DocumentVersion

MedicinalProduct
  └── LocalLabel             the meaning: market, language, version, status
        └── ProductDocumentId   ← references the content
```

**`ProductDocument` does not gain a `MedicinalProductId`.** It is scoped to
`GlobalProductId` today, and a Japanese label's PDF stored against the global
product is not a compromise — the *label* is the market-local thing, and it is
the label that says so. Adding market scoping to the document store would be
speculative, and worse, it would start `ProductDocument` down the road of
accumulating regulatory semantics one epic at a time until two contexts both
half-know what a label is.

The dependency is `Labeling → ProductDocument`, by id, one way. `ProductDocumentId`
is one of the pending record-struct ids; **referencing an id is not working on
that context**, so EPIC-018 does not fold in ADR-043's migration for it.

### 7. The clinical vocabularies extend `ReferenceData`, and inherit its rule

Every clinical field is a coded name/value pair, so `ClinicalVocabulary` joins
`SubstanceVocabulary`, `PharmaceuticalVocabulary` and `MeasurementVocabulary` in
`ReferenceData.Domain/Terminology` — **not** in `Labeling`, for the dependency
reason [ADR-058](ADR-058-substances-are-shared-facts-ingredients-are-roles.md)
§3 already traced.

It inherits the rule as well as the location: **every lookup returns a fresh
instance**, guarded per vocabulary by an `EachResolutionIsItsOwnInstance` test.
And it inherits the honesty requirement — seeded values carry
`System = "regos-internal"`, because RegOS holds no MedDRA, SNOMED or ICD
licence. Swapping a seed list for real terminology must remain a data migration,
which is what `System` is for.

### 8. The screen's word and the domain's word

Recorded as pairs in `docs/domain-model/labeling.md` at S001, per the working
agreement that both are binding.

| Domain type | Screen |
|---|---|
| `GlobalLabel` | **Global label** |
| `LocalLabel` | **Local label** |
| `UndesirableEffect` | **Side effect** |
| `Population` | **Who it applies to** |
| `OtherTherapy` | **Used with** |

**The first pair is the one to watch, and today it is not a pair at all.**
Medical Affairs says *CCDS*; Regulatory says *Core Data Sheet*; vendors say
*Company Core Data Sheet*; engineers read *Global label* without difficulty.
Which of those belongs on screen is a user-research question that has not been
asked, so the screen keeps the plain word until it is — and **the type stays
`GlobalLabel` whatever the answer**, because changing a label is trivial and
renaming a domain model six months later is not. This is the one row in the
table expected to stop matching.

## Consequences

**A new context with the ordinary three projects** — `RegOS.Labeling.{Domain,
Application,Infrastructure}` — joining `RegOS.slnx`, with EF configuration in
`RegOS.Persistence` like every other context.

**Three cross-context edges, all by id and all one way:** `Labeling → Product`
(`MedicinalProductId`, `GlobalProductId`), `Labeling → ReferenceData`
(`CodedConcept`, `CountryId`), `Labeling → ProductDocument`
(`ProductDocumentId`). Nothing depends on `Labeling`.

**Every aggregate here is tenant-owned**, so each takes the first of
[ADR-038](ADR-038-organization-depth-roots-and-the-three-filter-shapes.md)'s
three filter shapes — fail-closed on `TenantId` —
and `TenantFilterArchitectureTests` will require it the moment the entity carries
one. Owned children (`Population`, `Artwork`) have no `TenantId` and therefore no
filter of their own: **reads must start from a filtered root**, the isolation
lesson EPIC-010a's capstone paid for.

**Four `Population` tables**, one per statement type, and four near-identical EF
configurations. That is chosen, not conceded (§4).

**Nothing answers *"which indications are printed in this label version?"***
This is a real capability gap, and it is stated here so that it is recognised as
a deferred decision rather than reported as a defect (§3).

**Completion of EPIC-018 does not imply SPL/PLR export, label change control, or
automated translation.** It provides the structured data those capabilities read;
they are EPIC-007, EPIC-008 and infrastructure respectively.

## Revisit when

- **Someone needs to know what a label version published.** That is the
  conversation §3 defers, and it opens with the five questions listed there —
  not with a foreign key.
- **A fifth parent needs `Population`.** Four owned collections are within
  [ADR-018](ADR-018-rule-of-three.md)'s tolerance; a fifth, or four mappings that
  have visibly drifted apart, is when to ask whether the EF helper should become
  something more.
- **Licensed clinical terminology is obtained** — MedDRA, SNOMED, ICD. Same
  `System` seam as ADR-058, and the same test: if the migration turns out not to
  be data, this decision was wrong and the ADR saying so should say which part.
- **A document genuinely belongs to one market and to no global product.** Then
  `ProductDocument`'s scoping is what is wrong, not `LocalLabel`'s reference to
  it — and the fix belongs in that context.
- **Users consistently say CCDS.** The screen word changes; `GlobalLabel` does
  not.
- **S005 (`Interaction`) does not ship.** EPIC-018's Definition of Done names
  interactions, so the epic would be incomplete — but nothing in this ADR depends
  on them, and the backbone stands without them.
