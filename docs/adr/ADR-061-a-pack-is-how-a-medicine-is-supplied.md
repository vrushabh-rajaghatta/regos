# ADR-061 — A Pack Is How a Medicine Is Supplied, Not What It Is

**Status:** Accepted · **Date:** 2026-08-04 ·
**Related:** [ADR-058](ADR-058-substances-are-shared-facts-ingredients-are-roles.md) (composition, `CodedConcept`, and the `ComponentTree` this one copies),
[ADR-039](ADR-039-the-market-local-product-tier.md) (the market-local tier a pack hangs from),
[ADR-037](ADR-037-registrations-are-regulatory-assets-with-derived-visibility.md) (the `Registration` §3 deliberately does not change),
[ADR-059](ADR-059-clinical-statements-are-facts-labels-are-artifacts.md) (artwork, and the label wording §4 borrows from),
[ADR-018](ADR-018-rule-of-three.md) (duplicate twice, abstract on the third),
[ADR-031](ADR-031-tenant-isolation-by-query-filters.md) (isolation),
[ADR-016](ADR-016-persistence-access-model.md) (persistence access model),
[EPIC-010b](../product/epics/EPIC-010b-packs-and-supply.md) D1–D6

## Context

RegOS knows what a medicine **is**: its markets, its presentation, its
composition down to the substance, and the physical articles a patient receives
— a vial, a syringe, a pre-filled pen — as a depth-guarded
[`ComponentTree`](../../src/Product/RegOS.Product.Domain/Product/ComponentTree.cs)
built in EPIC-010a.

It does not know what a market **sells**. A carton of three blisters of ten
tablets, prescription-only, thirty-six months below 25 °C, authorised under
EU/1/26/1234 — none of that is expressible, so *"which packs are authorised
here?"* is answered by reading a licence.

**The trap this ADR exists to disarm** is that RegOS already has a recursive
containment tree carrying quantity and unit. A carton → 3 blisters → 10 tablets
could be entered into `MedicinalProductComponent` today. Every future reader will
reach that observation, and most will reach it while under time pressure.

The [EPIC-010 umbrella](../product/epics/EPIC-010-idmp-product-data-depth.md)
anticipated two recursions and said *"decide once, apply to both"* — but it gave
no test for telling them apart, which is the part that actually matters.

This ADR is written **before S001**, because all three decisions below are
structural and each is expensive to reverse after data exists.

## Decision

> **A pack is how a medicine is supplied, not what it is.**

### 1. Why packaging is not composition

The discriminator, and the sentence to quote when the question comes back:

> **Does it change when the same medicine is sold in a different pack size?**
> If **no**, it is a **component**. If **yes**, it is **packaging**.

A 30-tablet carton and a 100-tablet carton of one product share an *identical*
component tree — one tablet, one dose form, one composition — and differ
entirely in packaging. Modelling packs inside `MedicinalProductComponent` would
therefore duplicate the whole component tree **once per pack size**, and every
correction to the composition would have to be applied to each copy. That is not
an economy; it is the same fact stored *n* times, which is what this codebase
treats as a defect.

Stated as a pair, so each type is recognisable on sight:

| | Distinguishing attribute |
|---|---|
| `MedicinalProductComponent` | has a **dose form** — it is part of what the medicine *is* |
| `PackageItem` | has a **material** — it is how the medicine is *held* |

**A corollary, applied immediately.** `PhysicalCharacteristics` — colour, shape,
score line, imprint — belongs to the **presentation**, not the pack: a tablet
looks the same whichever carton it is in. The discriminator answers that without
a second argument, which is the test of whether a discriminator is any good.

**And a refusal it also settles.** RIM's `Devices` is not modelled. A pre-filled
pen is already a `MedicinalProductComponent` with a dose form and a quantity, and
a second aggregate for the same physical object is exactly the duplication above.
If a device ever needs a fact a component cannot hold — a UDI, a notified-body
number — that is a demonstration and the change is additive.

### 2. Why RegOS has two recursive structures, and only one pattern

`PackageItem` is a **second root with a nullable parent**, and its rules live in
a `PackagingTree` — a non-persisted domain type built from every item of one
pack, carrying the depth guard, the cycle guard and the reading order.

That is `ComponentTree`'s **pattern, copied deliberately; not its code, and not
an abstraction over both.**

- **Copying the pattern** is what EPIC-018 D4 did with `RegulatoryTemplate`'s
  versioning, and for the same reason: the shape is right, the assumptions are
  not shared, and a rule added to one tree must not silently reach the other.
- **Not abstracting** is [ADR-018](ADR-018-rule-of-three.md) applied honestly.
  This is the **second** occurrence. A generic `RecursiveTree<T>` written now
  would be an abstraction over one demonstrated case and one predicted one, and
  the predicted one has already diverged — `PackagingTree` guards a different
  depth story and orders by a different key.

EPIC-010a's retro recorded *"a rule about a structure belongs on a type that is
the structure — candidate ADR when a second one appears, not yet."* **The second
has appeared, and this section is the evaluation it asked for.** The conclusion
is that the *placement rule* is worth stating and the *shared type* is not: two
trees, two guards, one idea, written down here so the third occurrence has
something to be measured against rather than a third invention.

**The depth limit is a domain constant, not a schema limit** — the same call
`ComponentTree` made. Changing it is a decision, not a migration.

### 3. Why `Registration` does not point at `PackagedProduct`

RIM says `License → Packaged Product`, **Parent, Single**. That is wrong, and
RegOS departs from it deliberately:

- One EU marketing authorisation covers **several pack sizes**.
- One US NDA covers **several package configurations**, each with its own NDC.

So the relationship is one-to-many. The remaining question is which side holds
it, and RegOS puts it on the **pack**:

```
MedicinalProduct
    └── PackagedProduct
            └── RegistrationId?        ← nullable
```

**Because a pack exists before its licence does.** That is not a convenience; it
is the same reasoning [ADR-039](ADR-039-the-market-local-product-tier.md) used
for markets — *a product is present in a market from the moment the company
intends to sell there, which is years before an authority agrees.* A pack is
designed, coded and costed long before it is authorised, and a model that cannot
hold an unlicensed pack forces either a fabricated registration or a pack that
appears only after approval. Both lose information the business has.

Three consequences, all wanted:

| | |
|---|---|
| *"Which packs does this licence authorise?"* | a filter on the pack |
| *"Which packs are not authorised yet?"* | `RegistrationId is null` — no *planned registration* had to be invented |
| **`Registration` is not modified** | EPIC-005's aggregate, its history and its tests are untouched. The umbrella sketch expected this decision to change it; it need not |

### 4. Two identifiers for one barcode, and they may disagree

A pack carries the code the company registers. `LocalLabelRevision.DataCarrierCode`
([ADR-059](ADR-059-clinical-statements-are-facts-labels-are-artifacts.md))
carries what the **approved artwork prints**. Both are kept.

They should agree — and **the fact that they can disagree is the point**. A pack
whose registered code and printed code differ is a labelling defect, and a model
that stored the fact once could not represent one. This is the same shape as
[ADR-057](ADR-057-a-filed-artifact-is-projected-from-a-snapshot.md) §1's
`FiledStudyTitle` against the study registry: two copies that are *meant* to be
able to diverge, because the divergence is the record.

Artwork gains a nullable `PackagedProductId` — the seam EPIC-018 deferred with
this epic named as its milestone.

### 5. `PackageItem`, not `PackagingComponent`

RIM's noun is `Packaging`; the type here is `PackageItem`, which is FHIR/IDMP's
term for the same object — *a packaging item, as a container for medically
related items, possibly with other packaging items within*.

The obvious alternative, `PackagingComponent`, **reuses the exact word that means
the other tree.** `MedicinalProductComponent` and `PackagingComponent` in one
namespace reproduce the confusion §1 exists to prevent, and the two must be
distinguishable without documentation.

**This is the third mechanical rename in three epics** — after `Labeling` →
`LocalLabel` and `Interaction` → `DrugInteraction`. The pattern EPIC-018's retro
named holds: **RIM names objects as though nothing else exists in the system, and
a bounded-context codebase names them in the presence of everything else.**

## Consequences

**A pack is market-local.** `PackagedProduct` hangs off `MedicinalProductId`,
carries `TenantId`, and is reached through a fail-closed query filter
([ADR-031](ADR-031-tenant-isolation-by-query-filters.md)). France's 28s and the
UK's 30s are different packs, not one pack with a country column.

**Legal status of supply sits on the pack**, because a 16-tablet pack of
paracetamol may be general sale while a 100-tablet pack is pharmacy-only. The
restriction differs by presentation, not by active substance.

**Shelf life is recorded as value + coded unit, never normalised.** *"3 years"*
is stored as `3` + `YEAR`, not as `36` months — EPIC-010a's carry-forward
established that `Strength` equality is literal and RegOS does not convert units,
and this is not the place to start. `ShelfLifeText` holds the label's own
wording beside it, the same coded-fact-versus-label-text split
[ADR-059](ADR-059-clinical-statements-are-facts-labels-are-artifacts.md) made for
clinical statements.

**RIM's `OtherCharacteristics` is refused, not deferred.** A name/value bag is
the opposite of a coded model, and a section RIM itself could only call *Other*
is a signal that the classification failed, not a domain concept.

**Cluster B+C is 7 RIM objects and this closes 5.** Stated so no coverage figure
implies otherwise.

## Revisit when

- **A pack is authorised under two licences.** Today's `RegistrationId?` is
  single. The second demonstration turns it into a child collection — additive,
  and only then.
- **A device needs a fact a `MedicinalProductComponent` cannot hold** — a UDI, a
  notified-body number, a device-specific lifecycle. That is the demonstration
  §1's refusal asks for.
- **A third recursive structure appears.** Then `ComponentTree` and
  `PackagingTree` have a third sibling, [ADR-018](ADR-018-rule-of-three.md)'s
  threshold is met, and the shared abstraction §2 declined becomes the question
  rather than the assumption.
- **Someone stores a pack's contents in `MedicinalProductComponent`.** §1's
  discriminator is the argument; if it stops being persuasive, this decision is
  what needs revisiting — not the data.
