# EPIC-010c — Manufacturing

**Status:** 🟡 Planned · **Branch:** `epic/EPIC-010c-manufacturing` (cut from `main` at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

The third and last split of [EPIC-010](EPIC-010-idmp-product-data-depth.md), and
the one the umbrella called *"the most self-contained"*. It closes cluster **D**
— and closes it by **building one of its four RIM objects and refusing three**.

> **Phase 1 and Phase 2 were signed off together on 2026-08-05**, before a branch
> was cut. The one decision the plan did not name — **D7**, which context the
> operation lives in — surfaced while writing this document and is recorded with
> the others.

---

## Phase 1 — Epic plan

### Outcome

> **"Where is this product made, and is that site on the licence?"**

A **divergence** question, and the third of its kind: EPIC-018 asked *which
markets is this approved for this condition in*, EPIC-022 asked *does this market
accept our stability data*. It is what a regulatory affairs team asks before a
site transfer, and RegOS can currently answer **neither half**.

### Two recorded predictions fire here

Both were written down when the thing that would trigger them did not exist,
which is the strongest kind of evidence this epic is due
([EPIC-022's retro, lesson 3](EPIC-022-country-depth.md#3-architectural-predictions-are-stronger-than-counting-occurrences)).

| Written | Where |
|---|---|
| *"other aggregates will reference a site **by id** — a licence naming approved manufacturers, an ingredient naming its manufacturing source"* | [`OrganizationSite`](../../../src/Organization/RegOS.Organization.Domain/Aggregates/OrganizationSite/OrganizationSite.cs)'s own docstring, EPIC-016 |
| *"Wiring sites into License and Application… **EPIC-006/010/017 attach to it**"* · *"**Mfg Business Operation** — needs the market-local product tier → **EPIC-010**"* | [EPIC-016](EPIC-016-organization-depth.md)'s deferral table |

### Depends on

- **EPIC-016** ✅ — `OrganizationSite` is a root with a type, a postal address and
  registry identifiers (FEI, DUNS). Nothing new is needed from it.
- **EPIC-017** ✅ — operations attach to the **market-local** tier (D7).
- **EPIC-010a** ✅ — `Ingredient` exists to be given a source.
- **EPIC-010b** ✅ — `PackAuthorisation` is the pattern D1 copies.

### Cluster D — one built, three refused

| RIM object | | Why |
|---|---|---|
| **Mfg Business Operation** | ✅ **built** as `ManufacturingOperation` | Site ↔ product ↔ operation type ↔ dates. This *is* regulatory information — it is what a marketing authorisation lists |
| Manufacturing Process | ❌ **refused** | |
| Manufacturing Process Step | ❌ **refused** | |
| Mfg Process Step Materials | ❌ **refused** | |

**Refused, not deferred, and the reason is that RegOS already holds it.** A
manufacturing process description is CMC **document** content, and the blueprint
already routes it there: [`RegulatoryTemplates`](../../../src/Persistence/RegOS.Persistence/Initialization/ReferenceData/Blueprint/RegulatoryTemplates.cs)
carries **3.2.S.2 "Manufacture"** and 3.2.P.3.3 as document sections. Structured
step rows would be a **second, competing representation of narrative content** —
the duplication Phase 2 has spent three epics removing. It also sits on the far
side of the line the umbrella already drew at *"bill-of-materials / batch
genealogy — manufacturing execution, not regulatory information"*.

> **The falsifier, named now rather than argued later:** a **variation-impact**
> capability that must reason over *individual process changes* — *"step 4
> changed; which markets need a variation?"* Nothing else needs step-level
> granularity, and that capability belongs with EPIC-009/EPIC-020. Until it
> exists, the narrative belongs in the dossier.

This is EPIC-010b's precedent applied a second time: `OtherCharacteristics` and
`Devices` were refused there, not shelved.

### In scope ✅

- **`ManufacturingOperation`** — market-local product, site, operation type,
  effective dates.
- **`ManufacturingVocabulary`** — the tenth vocabulary class: API manufacture,
  finished product, primary packaging, secondary packaging, QC testing, batch
  release, importation.
- **The licence's approved manufacturing sites** — dated, in Registration,
  copying the `PackAuthorisation` shape (D1).
- **`Ingredient.ManufacturingSourceSiteId`** — owed by the umbrella's own in-scope
  list for `Ingredient` and never shipped by 010a. A **different stage of the
  supply chain** from the operation above, and D2 says why.
- **The divergence** — operations run against the sites a licence approves,
  reported and never blocking.
- Browser proof, retro. **ADR-063** before any code.

### Out of scope ⏸️ (deferred, with reason)

| Deferred | Why |
|---|---|
| **`Application → Master File Location / PSMF`** (the last RIM site link) | A pharmacovigilance and application attribute, not manufacturing. It belongs to whichever epic deepens `RegulatoryApplication` |
| **Site qualification, audit and requalification workflow** | → **EPIC-008**. This epic records *that* a site performs an operation, not the internal process by which it became allowed to |
| **A `Manufacturer` column on `PackagedProduct` and `PackageItem`** (RIM has both) | D3 — the operation's *type* already carries *primary packaging* and *secondary packaging*. Three ways to say one thing |
| **Batch, lot and genealogy** | The umbrella's hard line, unchanged: manufacturing execution, not regulatory information |
| **Supplier qualification status on an ingredient source** | The source is *where it comes from*. Whether that supplier is qualified is a quality-system fact, and the same EPIC-008 boundary |
| **Notifying anyone that an operation runs at an unapproved site** | → **EPIC-014**, which owns every "tell someone" half in this backlog |

### Definition of Done

- A market-local product names the sites that perform operations for it, each
  with an operation type and effective dates.
- A licence names the manufacturing sites it approves, **with the date each was
  added** — because sites arrive by variation years after approval.
- An ingredient names the site its substance comes from, and the model keeps that
  distinct from finished-product manufacture.
- *"Where is this made, and is that site on the licence?"* is answerable through
  the API, and an operation at an unapproved site is **reported, not prevented**.
- Browser proof; **ADR-063 written and accepted before S001**.
- Retro, and the umbrella's coverage claim restated as *implemented vs
  deliberately refused* rather than a percentage.

### What it does to EPIC-010's coverage claim

**Stated up front, because the number moves and the reason is a decision rather
than a shortfall.** The runway credits EPIC-010 with **16 RIM objects**. 010b
refused 2 of cluster B+C's 7; 010c refuses 3 of cluster D's 4.

| | |
|---|---|
| **Implemented** | **11 of 16** |
| **Refused, each with a recorded reason and a falsifier** | **5 of 16** |

The percentage is being replaced rather than recalculated. *"87%"* invites a
reader to see 13% of unfinished work; **"11 built, 5 refused"** says what
actually happened.

---

## Phase 2 — Domain design *(approved 2026-08-05)*

### Shape

```
MedicinalProduct                              (market-local — ADR-039)
 └── ManufacturingOperation        NEW        product ↔ site ↔ operation ↔ dates
                                              ▲ Product context (D7, ADR-063 §1)

Ingredient                                    (EPIC-010a)
 └── ManufacturingSourceSiteId     NEW        where this substance comes from
                                              ▲ a different stage (D2, ADR-063 §2)

Registration                                  (EPIC-005, untouched)
 └── SiteApproval                  NEW        licence ↔ site ↔ ApprovedOn
                                              ▲ Registration context (D1, §4)
```

### The seven decisions

| # | Decision | Settled as |
|---|---|---|
| **D1** | **A licence's approved sites are their own root carrying `ApprovedOn`**, not a collection on `Registration` | ✅ approved — a site joins a licence **by variation, years later**, and a foreign key cannot date that. The **second** occurrence of *licence + thing + date* after `PackAuthorisation`: [ADR-018](../../adr/ADR-018-rule-of-three.md) says **copy it**, and the third is when to think again |
| **D2** | **`Ingredient` gets its own manufacturing source**, even though `ManufacturingOperation` already names who makes the product | ✅ approved — **they answer different questions.** See below |
| **D3** | **One place says where work happens** — the operation, not a `Manufacturer` column on Packaging and Packaged Product as RIM has it | ✅ approved — operation *type* already carries the distinction those columns were making |
| **D4** | **Operation type is a `CodedConcept` from a new vocabulary**, not an enum | ✅ approved — **nothing branches on it in code**, which is the test `OrganizationSiteType`'s docstring records for going the other way |
| **D5** | **Effective dates, and no status history** | ✅ approved — *"approved for this operation from 2024-03-01"* is a dated fact, not a lifecycle. The [status-history rule](../BACKLOG.md#the-cross-cutting-rule-status-history) exempts it |
| **D6** | **The divergence is derived on read and reported, never blocking** | ✅ approved — the EPIC-005 expiry precedent, now used a **third** time after label languages (022 S003) and stability conditions (022 S004) |
| **D7** | **`ManufacturingOperation` lives in the Product context**, which requires a new `Product.Domain` → `Organization.Domain` dependency | ⚠ **Surfaced while writing this document, not in the signed-off plan.** See below — it is why **ADR-063 is required**, contrary to what the plan predicted |

### D2 — why an ingredient source is not the operation restated

The two are close enough that a future reader will try to merge them, so the
distinction is recorded in [ADR-063 §2](../../adr/ADR-063-where-a-product-is-made-is-a-product-fact.md)
as well as here.

```
Finished product          made at Site Gamma      ← ManufacturingOperation
├── API A                 from Site Alpha         ← Ingredient source
└── API B                 from Site Beta          ← Ingredient source
```

**They model different stages of the supply chain.** The operation set cannot say
which API came from where; the ingredient source cannot say who packed the
carton. And under **dual sourcing** — one API, two qualified suppliers — the
finished-product operation does not change at all.

Neither is derivable from the other, which is the test this project applies to
every "could these be one field?" question.

### D7 — the decision the plan did not name

The plan asserted an ADR probably would not be needed. **That was wrong, and the
compiler is the reason.**

```
Product.Domain        →  SharedKernel, ReferenceData.Domain
Organization.Domain   →  SharedKernel, ReferenceData.Domain
```

They are **siblings**. Every existing consumer of `OrganizationSiteId` outside
Organization — `Inspection`, `Contact`, `Registration` — lives in a context that
already depends on it. Product does not, so **both D1's operation and D2's
ingredient source need a new cross-context edge**, and
[CLAUDE.md](../../../CLAUDE.md) makes that an ADR before it is code.

**The comparison that makes this worth an ADR rather than a csproj line:**

| | [ADR-061 §3](../../adr/ADR-061-a-pack-is-how-a-medicine-is-supplied.md) | Here |
|---|---|---|
| Reverse edge already exists? | **yes** — the design was **refused** and became `PackAuthorisation` | **no** — the edge is legal |

> **The compiler refused the last one for us. It will not refuse this one**, so
> the argument has to be made rather than discovered. ADR-063 makes it: **D2
> forces the direction** — `Ingredient` cannot leave `Product.Domain`, so the
> edge exists whatever happens to the operation; and once it exists, hosting the
> operation in Organization would need the reverse edge and close a cycle.

### Change-case analysis

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| An operation's site changes (transfer) | **High** | Effective dates close the old row and open a new one; nothing is edited in place |
| A site is added to a licence by variation | **High** | Exactly what `SiteApproval.ApprovedOn` exists for |
| Dual sourcing of one API | **High** | Ingredient source is per-ingredient, and D2 keeps it separate from the product operation |
| xEVMPD/IDMP export needs the manufacturer list (EPIC-007b) | Medium | Operation + type is the payload; the site already carries FEI and DUNS |
| Someone wants the divergence to block | Medium | It cannot, by D6 — and blocking belongs to a rule a blueprint states |
| A site gains qualification status | Medium | → EPIC-008. Nothing here changes; the operation keeps recording *what happens*, not *whether it may* |
| Manufacturing gains a lifecycle of its own | Low | The trigger for a `Manufacturing` bounded context, refused in ADR-063 as speculative today |

---

## Phase 3 — Stories

| # | Story | Slice |
|---|---|---|
| **ADR-063** | The cross-context edge, the two stages, and the three refusals — **before S001** | doc |
| **S001** | **`ManufacturingOperation`** + `ManufacturingVocabulary` — *"which sites make this product?"* | domain → persistence → API → UI → test |
| **S002** | **The licence's approved sites**, dated (D1) | full slice |
| **S003** | **`Ingredient` names its source site** (D2) | full slice |
| **S004** | **Capstone** — the divergence, browser proof, retro, coverage restated | UI → test → docs |

**No external prerequisite.** The first epic since EPIC-019 for which that is
true — nothing here needs a document RegOS does not hold.

**Low migration risk, and worth saying after EPIC-022.** These are new tables
with no existing rows, so unlike every migration in 022, **none needs a
hand-written backfill**. The one exception to watch is S003, which adds a
nullable column to a shipped table (`Ingredients`) — nullable, so no backfill and
no default that lies about data.

### What S001 found

*Recorded here rather than in the backlog, because planning evolution lands on
`main` between epics. **Carried to the close.***

**The bounded-context dependency graph is not enforced by anything executable.**
S001 added the first `Product.Domain` → `Organization.Domain` reference and the
architecture suite stayed green — because none of its 21 tests looks at which
context may reference which. The edge is held by **ADR-063 and a `.csproj` line**
and nothing else, which means the reverse edge ADR-063 permanently closes could
be opened by anyone who adds a project reference and builds.

Not fixed here — it is not this epic's job, and inventing an architecture rule
mid-story is how a story stops being a vertical slice. **Carried as a candidate
for [EPIC-023](../BACKLOG.md#next) or a future architecture-hardening epic**,
raised on `main` at the close alongside the retro.

> It belongs with EPIC-023 by kind, not by coincidence: both are invariants the
> project *states* and does not *check*. One is "the schema matches the
> migrations", the other is "the contexts depend the way the ADRs say". Each is
> currently held by a person remembering.

**Three defects, none of them about manufacturing.** The site registry was empty
(nothing had ever seeded an `OrganizationSite`); the seeder ran before the
identifier schemes it depends on; and the demo sites were first seeded to *Demo
Manufacturer Ltd.* when the tenant that logs in is *Demo MAH Ltd.* All three were
found by running the browser proof, not by review — and the third is the
fail-closed tenant filter (ADR-031) working exactly as designed, refusing rows
that were real and correct and belonged to somebody else.

**Where to stop if it runs long:** after S002. S001 and S002 together answer the
epic's question; S003 is the refinement that makes the answer right for
multi-API and dual-sourced products, and it is the only story touching a shipped
aggregate.
