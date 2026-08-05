# EPIC-010b — Packs & supply

**Status:** ✅ Complete · **Branch:** `epic/EPIC-010b-packs-and-supply` (cut 2026-08-04) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

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

Recorded in [docs/domain-model/product.md](../../domain-model/product.md) — **at
S003, not S001 as planned.** The pairs were used in code from S001 and written
down two stories late; noted rather than quietly corrected, because "the screen
word is binding" is worth less if the register lags the code.

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
| **S001** | **`PackagedProduct`** — the pack: description, size, the market's pack code, dated marketing status | full slice | ✅ |
| **S002** | **`PackageItem` + `PackagingTree`** — the recursion, depth- and cycle-guarded; material is what makes it not a component | full slice | ✅ |
| **S003** | **`LegalStatusOfSupply` + `ShelfLifeStorage`** — how it is supplied, how long it lasts | full slice | ✅ |
| **S004** | **`Appearance` on the presentation, and artwork's pack link** — the two describing facts, and EPIC-018's debt paid | full slice | ✅ |
| **S005** | **Capstone** — a registration authorises packs; *"which packs are authorised here, and how are they supplied?"*; browser proof; retro | query → UI → test → docs | ✅ |

> **S004 was where to stop if the epic ran long**, decided in advance rather
> than under pressure. **It was not cut.** The architecture held across
> S001–S003 with no unresolved modelling questions, and S004 closes two pieces
> of declared work rather than adding a fifth concept: appearance completes the
> discriminator, and the artwork link retires the debt EPIC-018 deliberately
> carried and named this epic as the milestone for. Keeping that promise was
> the reason not to cut.

### S003 — what was decided while building

**The discriminator, used a third time — and it gave a third answer.** D1 put
the packaging tree on the pack, D5 put appearance on the *presentation*, and
S003 puts legal status and shelf life back on the pack. Three uses, three
independent answers, which is the first evidence the question is doing work
rather than confirming what was already assumed.

Shelf life is a pack fact for a physical reason, not a filing one: **the
container closure system is what the stability data was generated against.** The
same tablets in an alu-alu blister and an HDPE bottle keep for different lengths
of time — which is `PackageItem.Material` from S002 earning its place.

| | Decided | Why |
|---|---|---|
| **One value object, not two fields** | `ShelfLifeStorage` holds the period, the label's wording **and** the storage conditions | *"36 months"* alone is not a fact; *"36 months below 25 °C"* is. Split, a pack could keep a shelf life whose precondition had been deleted |
| **Required, not nullable** | `PackagedProduct.ShelfLife` is never null; `ShelfLifeStorage.NotStated` is the empty value | EF reads an *optional* owned reference back as null when every column it shares is null — and a pack whose only statement is *"protect from light"* has exactly that, so its conditions would vanish on reload while the write succeeded. `IsStated` then says out loud what a nullable navigation would only imply |
| **`NO_SPECIAL_PRECAUTIONS` is a value** | and it may not sit beside another condition | An SmPC that says *"does not require any special storage conditions"* has been **checked**; an empty list means nobody has said. Two different regulatory statements, and the model refuses to blur them |
| **A fourth vocabulary** | `SupplyVocabulary` — *how may it be handed over, and how must it be kept?* | Each list now answers one sentence. The period units are **not** in `MeasurementVocabulary`, and that vocabulary's own reason for existing is the argument: it is kept apart from presentation units so *"500 mg per tablet"* cannot be expressed, and `MONTH` beside `MG` would make **"500 months"** a legal strength |
| **Undated legal status** | recorded, with the seam named | A reclassification is a real regulatory event and nobody has asked to keep its history. If they do, the shape is `PackageMarketingStatusEntry`'s exactly — the **fourth** identical status history, and therefore ADR-018's demonstration |
| **Two domain methods, one command** | `Classify` and `StateShelfLife` stay apart; `StatePackSupply` submits both | The two facts move on different clocks and neither can make the other incoherent, which is the test `Describe` groups by. They share a form because one person states both in one sitting |

**`StateShelfLife` takes the whole value object, never its parts.** There is no
`SetShelfLifePeriod` beside a `SetStorageConditions`, because two setters would
let the period be changed without the conditions it is only true under.

**The falsifier ran against Postgres, not memory.** `pack-supply.spec.ts` states
storage conditions *and nothing else*, saves, and reloads the page — the exact
shape that would have exposed an optional owned reference silently dropping its
child table. A domain test could not have caught it.

### S004 — what was decided while building

**The discriminator now has three answers from four uses**, and appearance is
the one that points away from the pack: a tablet looks identical in a carton of
30 and a carton of 100, so how it looks is part of what the medicine *is*.

| | Decided | Why |
|---|---|---|
| **Colour is a set, shape is a choice** | `Colours` is an owned collection; `Shape` is one coded value | A capsule with a white body and a blue cap is two colours. A single field would force an invented *"white and blue"* vocabulary entry or prose — and prose is what the structured half exists to avoid. A tablet, by contrast, is round or oval and nothing is both |
| **The marking is its own field** | `Imprint`, beside `Description` | It is the one part of an appearance anybody looks a medicine **up** by. A poison centre with a loose tablet has the imprint and nothing else |
| **No fifth vocabulary** | `Colours` and `Shapes` join `PharmaceuticalVocabulary` | That list answers *what is this medicine?*, and colour and shape are properties of the administrable form exactly as dose form and route are |
| **Required owned reference, again** | `PhysicalCharacteristics.NotStated` | **The second use of the shape S003 introduced, for the identical reason** — an optional owned reference whose shared columns are all null is read back as null, and a presentation whose only statement is *"white"* has exactly that. ADR-018 says duplicate on the second and evaluate on the third; this is the second |
| **No `if (Type == Artwork)`** | the pack link is nullable on **every** local label | EPIC-018 D2 bought a real simplification by making artwork a label type rather than an aggregate, and recorded the price. The branch would also be **wrong**: a container label is printed per pack size, and a leaflet can be pack-specific |
| **The link is on the label, not a revision** | `LocalLabel.PackagedProductId` | Which pack a carton is printed for is what the document *is*. Revising the words on it does not make it a different pack's carton, so a correction here is a correction rather than a new revision |
| **Both barcodes stay** | `PackagedProduct.PackCode` and `LocalLabelRevision.DataCarrierCode` | One is what the company registers with the market, the other is what the approved artwork prints. They are *meant* to be able to disagree, and a single field would hide the day they do |

**A third occurrence noticed and deliberately not abstracted.** *Structured fact
beside approved wording* now appears three times — `Strength` with a
presentation's name, `ShelfLifeStorage` with its text, `PhysicalCharacteristics`
with its description. Recorded as an observed pattern rather than a candidate:
the three differ entirely in what the structured half is, and a shared "coded
value plus its wording" type would name the similarity while hiding every
difference.

**`SetNull`, not `Cascade`, on the pack link.** Deleting a pack must not take an
authority-approved document with it; the artwork outlives the pack record and
simply stops naming one.

**The cross-market refusal is the mistake worth catching.** A French carton
naming a UK pack has two real rows, both the tenant's, and nothing else would
notice — so the handler asks Product the question, the same read
`AttachGlobalLabelContent` makes of ProductDocument (ADR-059 §6).

### The verification loop

`dotnet test RegOS.slnx` (19 reporting suites) · `dotnet test tests/Architecture`
· **`npm run build`** · **`npm run lint`** · the browser suite against an
isolated stack.

> **`npm run build` is new to this list.** EPIC-018 S006 found it had been broken
> since that epic's first story, because the browser proof runs against
> `vite dev`, which does not typecheck. A gate nobody executes is a convention
> wearing a test's clothes.

> **`npm run lint` joined after S003**, and it was red on arrival — three errors
> in files nobody had touched for months. Each gate here catches a class the
> others cannot: tests catch domain regressions, browser specs catch workflow
> regressions, `build` catches types, `lint` catches the rest. **An omitted gate
> stays omitted indefinitely**, and a permanently-red one is omitted while
> looking present, so the three errors were fixed in the same change that added
> it.

---

## Retrospective

### Did the capstone demonstrate what the epic promised?

> *"Which packs of this product are authorised in this market, how are they
> supplied, and how long do they last?"*

Yes, and the shape of the answer is the evidence. One row of the capstone read
carries facts from **four aggregates across two contexts**, and **not one of them
is duplicated**:

| Fact | Story | Owned by |
|---|---|---|
| the pack, its size, its code, its dated marketing status | S001 | `PackagedProduct` |
| how many layers it holds | S002 | `PackageItem` |
| legal status, shelf life, storage conditions | S003 | `ShelfLifeStorage` |
| which licence, and **from when** | S005 | `PackAuthorisation` |

The browser proof asserts the negative as well: after authorising a pack, the
pack's own payload still contains no key mentioning a registration. **`Product`
never learned who authorised anything**, which is ADR-061 §3's entire claim.

### Definition of Done

| | |
|---|---|
| Packs with size, code, dated marketing status | ✅ S001 |
| Depth- and cycle-guarded contents with materials, tested | ✅ S002 — four layers, and a fifth refused |
| Legal status per pack, two packs able to differ | ✅ S003 |
| Shelf life and storage as the conclusion, with the label's wording | ✅ S003 |
| Appearance on the presentation | ✅ S004 |
| **A registration authorises packs, and a pack can exist without one** | ✅ S005 — every pack is listed, and *"Not yet authorised"* is a stated answer |
| The question answerable through the API | ✅ `GET /api/medicinal-products/{id}/authorised-packs` |
| Artwork can name the pack it is printed for | ✅ S004 |
| Browser proof; ADR-061 before S001 | ✅ 119 specs; ADR-061 at `6946429`, before S001 at `0926334` |

**5 of 7 RIM objects, as forecast.** `OtherCharacteristics` and `Devices` were
refused in Phase 1 and stayed refused — no story reached for either.

### The two lessons worth carrying past this epic

Both span several epics now, which is what separates them from story notes.

#### 1. Copy a pattern before abstracting it

| Occurrence | What was copied | What diverged |
|---|---|---|
| Versioning (EPIC-018 D4) | `RegulatoryTemplate`'s version/publish shape | approval dates, effective ranges |
| Recursive trees (010b D1) | `ComponentTree`'s guards and reading order | **depth 4 vs 3; quantity-descending vs alphabetical** |
| Structured fact + wording (010b S004) | — | **evaluated on the third occurrence and refused** |

The trees are the strongest case, because the divergence is now asserted:
`PackagingTreeTests` states that the two depth limits **differ**. A generic
`RecursiveTree<T>` written at the second occurrence would already be carrying a
conditional.

And the third row matters as much as the first two — *structured fact beside
approved wording* appears in `Strength`, `ShelfLifeStorage` and
`PhysicalCharacteristics`, which is exactly the count ADR-018 says to evaluate
at. It was evaluated **and refused**, because the three differ entirely in what
the structured half is. **ADR-018's rule of three is a trigger to think, not a
trigger to abstract**, and this epic is the first time the evaluation went the
other way and was written down.

#### 2. Persistence may not dictate the domain model

Three times, an infrastructure constraint pushed back on the design, and each
time the model was decided first and the mapping made to fit:

| Constraint | What it wanted | What was built |
|---|---|---|
| `Registration.Domain → Product.Domain` already exists | a raw `Guid?` on the pack | **`PackAuthorisation`** — a dated relationship in Registration, which is a better model *and* the one that compiles |
| EF nulls an optional owned reference when its columns are all null | splitting the conditions off the value object | **`ShelfLifeStorage.NotStated`** — required navigation, named empty value |
| The same, one story later | the same split, for colours | **`PhysicalCharacteristics.NotStated`** — the second use of the same shape |

The first is the clearest. The compiler refused the signed-off design at S001;
the response was to report it rather than reach for a workaround, and the
replacement gained a date a foreign key could never have carried. **The
constraint improved the model.**

The second and third are subtler and worth stating plainly: `NotStated` looks
like an EF workaround and is not. It removed a genuine ambiguity — *no statement
entered* versus *statement deliberately empty* — that a nullable navigation had
been hiding. The persistence problem is what made anyone look.

### What went wrong, and what it cost

| | |
|---|---|
| **The signed-off D2 was not implementable** | Found by the compiler at S001. ADR-061 §3 amended rather than superseded — unmerged, four commits old, relied upon by nothing. Cost: half a story. Worth it: the correction is a better design |
| **`--no-build` on `dotnet ef`, twice over two epics** | Generated an empty migration that looked like a pass, then `migrations remove` deleted **S002's committed migration**. Recovered from git. Now recorded against **ES-021**, whose primary proof — an empty `Up`/`Down` — is exactly what a stale assembly produces |
| **`npm run lint` was never in the loop** | Red on arrival with three standing errors. Fixed in `2db20f5` and added, because a permanently-red gate is omitted while looking present. Second gate this project has discovered by omission, after `npm run build` in EPIC-018 S006 |
| **The capstone read went stale after adding a pack** | A real cache bug, not a test artefact. Fixed by keying the capstone query **under the `["packs"]` prefix** rather than adding a sixth invalidation to five hooks — the one that forgets is the one that shows a stale screen |
| **Four browser-spec defects, all mine** | `Italy` is not seeded; Playwright's `hasText` is case-insensitive; `getByLabel("White")` also matches *Off-white*; the market list returns `medicinalProductId`, not `id`. Every one was a locator or payload assumption, never a domain error |

### Something the epic found and did not fix

**`POST /registrations/{id}/approval` has no `/api` prefix** — an SC-001
violation, already in `RouteConventionTests.Grandfathered`. S005's spec had to
call it without the prefix. Left alone deliberately: that list is shrink-only,
and shrinking it is its own change with its own frontend impact, not something
to smuggle into a capstone.

### Carry-forward

| Item | Where it goes |
|---|---|
| **`PackAuthorisation` has room for what carried an authorisation** — a variation, a submission, a sequence | Built when one is asked for. None was |
| **In-use shelf life** — *"after first opening, 28 days"* | `ShelfLifeText` holds it; a second structured value waits for a reader |
| **Legal-status history** | Undated today. If asked for, it is `PackageMarketingStatusEntry`'s shape — the **fourth** identical status history, and therefore ADR-018's demonstration |
| **Steward CRUD for `SupplyVocabulary` and `PackagingVocabulary`** | → EPIC-012 |
| **Cross-market pack comparison** | → EPIC-011, on EPIC-018 S006's terms: showing is not comparing |
| **The artwork watchpoint stays armed** | Tested once here and held. `LocalLabelTypeBranchTests` still quiet |
