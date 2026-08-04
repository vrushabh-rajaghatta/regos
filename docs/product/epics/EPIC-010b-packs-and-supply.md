# EPIC-010b — Packs & supply

**Status:** 🟡 In Progress · **Branch:** `epic/EPIC-010b-packs-and-supply` (cut 2026-08-04) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

What a market actually sells: the **pack**, what is inside it, how it may be
supplied, how long it lasts — and which licence authorises it.

Cut from [EPIC-010](EPIC-010-idmp-product-data-depth.md) (clusters **B** and
**C**) on 2026-08-04. This document carries the settled Phase 2 and Phase 3 for
those clusters and **supersedes the umbrella's sketch for them**; the sketch
stands unchanged for **10c**.

> **Renamed on pull-in.** The umbrella called this split *"Presentation &
> packaging"*. **Presentation already shipped** — `PharmaceuticalProductDetail`,
> screen word *Presentation*, was EPIC-010a S002. What is left is packs and how
> they are supplied, so the epic is named for that.

---

## Phase 1 — Epic plan

### Outcome

> **"Which packs of this product are authorised in this market, how are they
> supplied, and how long do they last?"**

A market-local product can describe what it actually sells — a carton of three
blisters of ten tablets, prescription-only, 36 months below 25 °C, authorised
under licence EU/1/26/1234 — instead of that being a sentence in a PDF.

### Depends on

- **EPIC-010a** ✅ — composition, presentation, `UnitOfPresentation` and the
  `ComponentTree` pattern this epic copies.
- **EPIC-017** ✅ — a pack is a **market** fact. France sells 28s, the UK sells
  30s, and neither is a property of the global product.
- **EPIC-018** ✅ — settled the polymorphic-parent question the umbrella deferred
  to it, and left the artwork seam D6 pays.
- **EPIC-005** ✅ — `Registration`, which D2 deliberately does **not** change.

### In scope ✅

- **`PackagedProduct`** — description, pack size (quantity + `UnitOfPresentation`),
  the market's own pack code, dated marketing status.
- **`PackAuthorisation`** — in the **Registration** context: which packs a licence
  authorises, and the date each was authorised.
- **`PackageItem`** — the recursive containment tree: carton → blister → tablet,
  with **material**, depth-guarded and cycle-guarded.
- **`LegalStatusOfSupply`** — coded, **on the pack**.
- **`ShelfLifeStorage`** — the conclusion, never the study.
- **`PhysicalCharacteristics`** (screen word **Appearance**) — on the
  *presentation*, not the pack.
- The artwork → pack link EPIC-018 deferred here.

### Out of scope ⏸️ (deferred, with reason)

| Deferred | Why |
|---|---|
| **`OtherCharacteristics`** | **Refused, not deferred.** A name/value bag is the opposite of a coded model, and RIM's own word for it is *Other* — which usually means "we could not classify this". The same rule that bans `Common`/`Misc` folders applies to fields |
| **`Devices`** | **Refused.** A pre-filled pen is already a `MedicinalProductComponent` with a dose form and a quantity. A second aggregate for the same physical thing is the duplication this epic exists to avoid. If a device ever needs facts a component cannot hold — a UDI, a notified-body number — that is the demonstration, and it is additive |
| **In-use shelf life** | *"After first opening: 28 days"* is real and nobody has asked. `ShelfLifeText` can say it today; a second structured value waits for a reader |
| **xEVMPD / IDMP message generation** | → **EPIC-007b**. This epic makes the data correct; the publishing engine renders the messages |
| **Serialization / track-and-trace** (DSCSA, FMD) | A pack code is a recorded attribute, not a serialization system. Hard line, inherited from the umbrella |
| **Cross-market pack comparison** | → **EPIC-011**, on the same terms EPIC-018 S006 set: showing is not comparing |
| **Steward CRUD on the new vocabularies** | → **EPIC-012**, with every other vocabulary |

**Honest RIM accounting:** clusters B + C are **7 objects**. Two are refused
above, so **this epic closes 5 of 7** — stated here so the coverage figure is
never read as a claim about the other two.

### Definition of Done

- A market-local product has packs, each with a size, a pack code and a dated
  marketing status.
- A pack's contents are expressible as a depth-guarded tree with materials, and
  the depth and cycle rules are tested.
- Legal status of supply is recorded **per pack**, and two packs of one product
  can differ.
- Shelf life and storage conditions are recorded as the conclusion, with the
  label's own wording beside them.
- Appearance is recorded on the presentation.
- **A registration authorises packs, and a pack can exist without one.**
- *"Which packs are authorised in this market, and how are they supplied?"* is
  answerable through the API.
- Artwork can name the pack it is printed for.
- Browser proof; ADR-061 written before S001.

---

## Phase 2 — Domain design *(approved 2026-08-04)*

The sentence this epic is built on:

> **A pack is how a medicine is supplied, not what it is.**

Full argument in ADR-061. This section records *what was decided*; the ADR
records *why*.

### The six decisions

| # | Decision | Settled as |
|---|---|---|
| **D1** | **`PackagedProduct` + a recursive `PackageItem`** — a **second** tree, distinct from `ComponentTree` | ✅ approved — the discriminator is below. `PackagingTree` copies `ComponentTree`'s **pattern, not its code**, exactly as EPIC-018 D4 copied `RegulatoryTemplate`'s versioning. **Second occurrence: duplicate and observe** ([ADR-018](../../adr/ADR-018-rule-of-three.md)); no generic `RecursiveTree<T>` |
| **D2** | **A registration authorises many packs, as a dated relationship owned by the Registration context** | ✅ approved, **corrected at S001** — RIM says `License → Packaged Product`, *Single*, and that is wrong for real authorisations. The design first put a nullable `RegistrationId` on the pack; **the dependency graph forbids it**, and `PackAuthorisation(RegistrationId, PackagedProductId, AuthorisedOn)` is better anyway. `Registration` is still not touched, and `Product` stays independent. See [the correction](#d2-corrected-at-s001) |
| **D3** | **Legal status of supply is on the pack** | ✅ approved — a 16-tablet pack of paracetamol may be general sale while a 100-tablet pack is pharmacy-only. The restriction differs by presentation, not by active substance |
| **D4** | **Shelf life as value + coded unit, with the label's wording beside it** | ✅ approved — `ShelfLifeValue` + `ShelfLifeUnit`, `ShelfLifeText`, and storage conditions coded. **Not `ShelfLifeMonths`**: normalising *"3 years"* to `36` would be the first unit conversion in RegOS, against 10a's recorded position that `Strength` equality is literal |
| **D5** | **Appearance on the presentation; `OtherCharacteristics` and `Devices` refused** | ✅ approved — a tablet looks the same whichever carton it is in, which is **D1's discriminator applied a second time** |
| **D6** | **Artwork gains a nullable `PackagedProductId`, and both barcodes stay** | ✅ approved — EPIC-018 named this epic as the milestone and it has arrived. The pack's code is what the company registers; `DataCarrierCode` is what the **approved artwork prints**. They are *meant* to be able to disagree |

### D1's discriminator — the sentence the epic turns on

RegOS already has a recursive containment tree with quantity and unit:
`MedicinalProductComponent` + [`ComponentTree`](../../../src/Product/RegOS.Product.Domain/Product/ComponentTree.cs),
depth-guarded at 3 and cycle-safe. A carton → 3 blisters → 10 tablets could be
expressed in it today. So the question was never *how* to build RIM's second
recursion — it was **whether there is a second thing**.

> **Does it change when the same medicine is sold in a different pack size?**
> If **no**, it is a **component**. If **yes**, it is **packaging**.

A 30-tablet carton and a 100-tablet carton share an identical component tree —
one tablet — and differ entirely in packaging. Collapsing them would duplicate
the whole component tree once per pack size, which is a modelling smell, not an
economy.

**Stated as a pair:** *a component has a dose form; a package item has a
material.*

### Why `PackageItem` and not `PackagingComponent`

The obvious alternative reuses **the exact word that means the other tree**.
`MedicinalProductComponent` and `PackagingComponent` in one namespace is the
confusion D1 exists to prevent — the two must be distinguishable without
documentation.

`PackageItem` is also FHIR/IDMP's own term for this object — *a packaging item,
as a container for medically related items, possibly with other packaging items
within*. **A deliberate pick of IDMP's noun over RIM's `Packaging`**, and the
third mechanical rename in three epics ([EPIC-018's retro](EPIC-018-labeling-and-product-information.md#rims-nouns-are-named-in-isolation)
records why that keeps happening).

### Entities

| Root | Hangs from | Owns |
|---|---|---|
| **`PackagedProduct`** | `MedicinalProductId` | `PackageMarketingStatusEntry` — dated status history |
| **`PackageItem`** | `PackagedProductId`, and `ParentPackageItemId?` | — its own root, like `MedicinalProductComponent` (10a D5) |
| **`ShelfLifeStorage`** | owned by `PackagedProduct` | `StorageCondition` — coded, several may apply at once |
| **`PhysicalCharacteristics`** | owned by `PharmaceuticalProductDetail` | — |

`PackagedProduct` carries `LegalStatusOfSupply` (D3, coded) and **no
registration link at all** — authorisation lives in the Registration context
(D2, corrected).

| Root | Hangs from | Owns |
|---|---|---|
| **`PackAuthorisation`** | `RegistrationId` (Registration context) | — names a `PackagedProductId` and the date it was authorised |

### D2, corrected at S001

**The design said the link lives on the pack. It is not implementable.**
`Registration.Domain` already references `Product.Domain`, so
`PackagedProduct.RegistrationId` would close a dependency cycle — found by the
compiler while writing the aggregate, not by review.

Three options existed; the chosen one is better than what it replaced:

| | |
|---|---|
| **`PackAuthorisation` in the Registration context** | ✅ **taken.** Registration can name both types, `Registration` itself stays untouched, and the relationship gains **a date** — packs frequently arrive years after the original authorisation, by variation, and a foreign key cannot carry that |
| A raw `Guid?` on the pack | ❌ the first untyped cross-aggregate reference in the codebase, justified only by the cycle it dodges |
| `RegistrationId` into `SharedKernel` | ❌ ADR-017 scopes it to primitives and abstractions; a regulatory concept does not belong there |

**It also leaves somewhere to grow.** *Which variation authorised this pack?
Which submission introduced it? Which sequence first approved it?* are all
properties of the authorisation event and none of them properties of the pack.

ADR-061 §3 was **amended rather than superseded** — four commits old, unmerged,
relied upon by nothing, and an ADR should record what was built.

**Every root carries `TenantId`** and a fail-closed query filter
([ADR-031](../../adr/ADR-031-tenant-isolation-by-query-filters.md)). Reads start
at a filtered root; no `DbSet` is exposed for owned children.

### Screen words

| Domain type | Screen |
|---|---|
| `PackagedProduct` | **Pack** |
| `PackageItem` | **What's inside** |
| `LegalStatusOfSupply` | **Legal status** |
| `ShelfLifeStorage` | **Shelf life & storage** |
| `PhysicalCharacteristics` | **Appearance** |

Recorded in [docs/domain-model/product.md](../../domain-model/product.md) at S001.

### What the umbrella sketch got wrong

Kept rather than overwritten, because a corrected prediction is worth more than
a tidy document.

| The sketch said | What changed it |
|---|---|
| *"10b — **Presentation** & packaging"* | Presentation shipped in 10a S002. The name was written before the split it describes |
| *"Two self-referencing hierarchies… decide **once**, apply to both"* | Half right. One *pattern*, applied twice — but they are two **structures**, and the reason is D1's discriminator, which the sketch did not have |
| *Decision 5 — does `Registration` point at `PackagedProduct`? "It is a change to the Registration aggregate and touches EPIC-005's work"* | **It need not be.** Putting the link on the pack answers the same question, corrects RIM's cardinality, and leaves EPIC-005 untouched |
| *`Devices` and `OtherCharacteristics` listed in scope* | Refused. One is already modelled; the other is not a domain concept |

### Change-case analysis

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| xEVMPD / IDMP render (EPIC-007b) | **High** | The packaging tree is the payload's shape; `CodedConcept.System` is the seam for real terminology |
| A real supply-classification terminology (EDQM) replaces the seed | **High** | Same seam — a data migration, not a redesign |
| Pack-level shortage / discontinuation reporting | **High** | The dated marketing status is already per pack |
| An authorisation needs to name what carried it — a variation, a submission, a sequence | Medium | `PackAuthorisation` is where those go. None is built until one is asked for |
| In-use shelf life becomes structured | Medium | `ShelfLifeText` holds it today; a second value + unit is additive |
| Device-led combination products need a UDI | Medium | The demonstration D5 asks for. Additive on `MedicinalProductComponent` |
| Deeper packaging nesting than three levels | Low | The depth limit is a domain constant, so changing it is a decision rather than a migration — the same call `ComponentTree` made |

### ADR to write

**ADR-061 — *A pack is how a medicine is supplied, not what it is***, before
S001. Three sections, because those are the three questions a future reader will
arrive with:

1. **Why packaging is not composition** — the discriminator.
2. **Why RegOS has two recursive structures** — and why the second was
   duplicated rather than abstracted.
3. **Why authorisation is a dated relationship owned by Registration** — the
   departure from RIM's cardinality, and why the link is not a foreign key on
   either aggregate.

---

## Phase 3 — Stories

| # | Story | Slice | Status |
|---|---|---|---|
| **S001** | **`PackagedProduct`** — the pack: description, size, the market's pack code, dated marketing status | full slice | ⚪ |
| **S002** | **`PackageItem` + `PackagingTree`** — the recursion, depth- and cycle-guarded; material is what makes it not a component | full slice | ⚪ |
| **S003** | **`LegalStatusOfSupply` + `ShelfLifeStorage`** — how it is supplied, how long it lasts | full slice | ⚪ |
| **S004** | **`Appearance` on the presentation, and artwork's pack link** — the two describing facts, and EPIC-018's debt paid | full slice | ⚪ |
| **S005** | **Capstone** — a registration authorises packs; *"which packs are authorised here, and how are they supplied?"*; browser proof; retro | query → UI → test → docs | ⚪ |

> **S004 is where to stop if the epic runs long**, decided now rather than under
> pressure. Appearance and the artwork link are the least load-bearing: nothing
> depends on them, and cutting them leaves the Definition of Done short and the
> model whole — the better of the two failures available.

### The verification loop

`dotnet test RegOS.slnx` (19 reporting suites) · `dotnet test tests/Architecture`
· **`npm run build`** · the browser suite against an isolated stack.

> **`npm run build` is new to this list.** EPIC-018 S006 found it had been broken
> since that epic's first story, because the browser proof runs against
> `vite dev`, which does not typecheck. A gate nobody executes is a convention
> wearing a test's clothes.
