# EPIC-018 — Labeling & product information

**Status:** ✅ Complete (2026-08-04) · **Branch:** `epic/EPIC-018-labeling-and-product-information` (cut 2026-08-04) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

The approved **content** of a product — what it treats, who must not take it, what it does to you, what it clashes with — held as structured data per market rather than as a PDF nobody can query.

> **Pulled into Now 2026-08-04.** Phase 1 is unchanged. **Phases 2–3 were a
> sketch and have been replaced** by the design reviewed and signed off in the
> Phase-2 conversation, recorded as [ADR-059](../../adr/ADR-059-clinical-statements-are-facts-labels-are-artifacts.md).
> The sketch's own words are kept in [what the sketch got wrong](#what-the-sketch-got-wrong)
> rather than quietly overwritten.

---

## Phase 1 — Epic plan

### Outcome
A regulatory user can hold the **global label** (the company core data sheet) as a governed, versioned artifact, derive **local labels** per market from it, and query the structured product information — indications, contraindications, undesirable effects, interactions, each scoped to a **population** — instead of reading a document. *"Which markets is this indication approved in?"* becomes a query.

### The concepts it introduces

| Cluster | RIM objects | Attrs |
|---|---|---|
| **Label artifacts** | Global Label, Labeling (local), Artwork | 15 + 13 + 10 |
| **Structured product information** | Indication, Contraindications, Undesirable Effects, Interactions, Interactant | 15 + 12 + 8 + 10 + 3 |
| **Shared qualifiers** | Population, Other Therapy | 11 + 4 |

**10 RIM objects — the largest single block in the whole model** outside the IDMP tail.

### Depends on
- **EPIC-017** — RIM hangs `Global Label` off **Global Product** and `Labeling` (local label) off **Medicinal Product**. Without the market-local tier there is nowhere to put a local label, and the whole point of this epic is that labels differ by market.
- Reuses **`ProductDocument`/`DocumentVersion`** as RIM's `Content` — the file behind a label. Do not build a second document store.

### In scope ✅
- **`GlobalLabel`** — name, type, versioned with dated status, language(s), responsible department/person, change summary, phonetic spelling, US suffix; linked to `Content`.
- **`Labeling`** (local label) — language, NDC labeler code, SKUs, translations, version; derived-from link to another local label; per-country, on the market-local product.
- **`Artwork`** — language, data-carrier code, SKUs, version, dated status; child of a local label; linked to packaging (seam only until EPIC-010).
- **`Indication`** — disease/symptom/procedure, full text, language, disease status, comorbidity, intended effect, duration, category, dated status.
- **`Contraindication`**, **`UndesirableEffect`**, **`Interaction`** (+ **`Interactant`**) — the remaining structured sections.
- **`Population`** — age + unit + range, gender, race, physiological condition — attachable to any of the four above.
- **`OtherTherapy`** — relationship type + therapy, attachable to indications and contraindications.
- Label workspace UI, browser proof, ADR.

### Out of scope ⏸️ (deferred, with reason)
| Deferred | Why |
|---|---|
| **Label authoring / rich-text editing** | RegOS holds the *structured facts* and points at the document. A label editor is a different product. |
| **Automated translation** | Infrastructure. `Translations` is a recorded fact, not a service. |
| **Label change-control workflow** (review, approve, effective-date a change) | → **EPIC-008**. Versions and dated statuses are modelled here; the approval *process* is not. |
| **Artwork ↔ packaging component linkage** | Needs `Packaging` → **EPIC-010**. Nullable seam only. |
| **Coding the vocabularies to MedDRA / SNOMED / ICD** | The *shape* is a coded name/value pair from day one; licensing and loading a real terminology is a procurement question, not a modelling one. Seed a small controlled list, keep the seam. |
| **Cross-market label comparison / divergence reports** | → **EPIC-011**. This epic delivers the data those charts would read. |
| **Structured Product Labeling (SPL) / PLR export** | → **EPIC-007**. |

### Definition of Done
- A global label exists for a global product, versioned, with a dated status history and a linked content file.
- A local label exists for a **market-local** product, records the global label it derives from, and carries its own language and version.
- Artwork can be attached to a local label with its own dated status.
- Indications, contraindications, undesirable effects and interactions can be recorded against a market-local product, each optionally scoped to a population.
- The same population shape serves all four — proven by a test that exercises it under at least two different parents.
- *"Which markets is indication X approved in?"* is answerable through the API.
- Browser proof: create a global label → derive a local label for one market → add an indication with a paediatric population → see it on the product's market view.
- ADR written for the label hierarchy and the shared-qualifier (`Population`) modelling.

---

## Phase 2 — Domain design *(approved 2026-08-04)*

The guiding principle, and the reason it is written before the entities:

> **A clinical statement is a regulatory fact about a product in a market. A
> label is an editorial artifact that publishes some of those facts at a point
> in time.**

Full argument in [ADR-059](../../adr/ADR-059-clinical-statements-are-facts-labels-are-artifacts.md).
This section records *what was decided*; the ADR records *why*.

### The six decisions

| # | Decision | Settled as |
|---|---|---|
| **D1** | **A new `src/Labeling/` bounded context**, holding *both* clusters | ✅ approved — distinct ubiquitous language, distinct primary users (Medical Writing, not Regulatory Operations). Splitting labels from clinical statements would draw a boundary through one editorial act |
| **D2** | **`Population` is an owned entity per parent**, with its own identity | ✅ approved — it is added, edited, removed, reordered and approved, which is lifecycle, not a value. Mapping duplication is answered by **EF configuration helpers**; a shared *domain* type is not introduced (ADR-018, ES-014) |
| **D3** | **Clinical statements hang off `MedicinalProduct`**; no link to a label version | ✅ approved — an indication is approved for a product in a market; the label publishes it. The publication link is five versioning questions in a trench coat, and is deliberately deferred |
| **D4** | **Versioning copies the `RegulatoryTemplate` *pattern*, not its code** | ✅ approved — reuse the root-plus-version shape, draft/publish/supersede and effective dating; reuse neither its `record struct` id nor its ReferenceData assumptions. Identity from `CommitmentId` (ADR-043) |
| **D5** | **`LocalLabel` references a `ProductDocumentId`**; `ProductDocument` gains nothing | ✅ approved, with the rationale stated as a rule: **documents remain content storage; Labeling owns the regulatory meaning.** Stated so `ProductDocument` does not accrete market semantics one epic at a time |
| **D6** | **Domain names and screen words are separate**, and the type never follows the screen | ✅ approved — `GlobalLabel` stays `GlobalLabel` whatever users call it out loud |

### One departure from the sketch, arising from D1

**RIM's local-label object is called `Labeling`; the aggregate here is
`LocalLabel`.** A context named `Labeling` containing a type named `Labeling`
reproduces the namespace-equals-type collision that S000 removed fourteen `using`
aliases to delete. It also names the pair symmetrically with `GlobalLabel`.
Mechanical reason, not a modelling one — and the only place this epic departs
from RIM's noun ([ADR-059](../../adr/ADR-059-clinical-statements-are-facts-labels-are-artifacts.md) §2).

### Aggregates

| Root | Hangs from | Owns |
|---|---|---|
| **`GlobalLabel`** | `GlobalProductId` | `GlobalLabelVersion` — number, status + status date, effective from/to, change summary, `ProductDocumentId`s |
| **`LocalLabel`** | `MedicinalProductId` | `Artwork` — language, data-carrier code, SKUs, version, status + date |
| **`Indication`** | `MedicinalProductId` | `Population`, `OtherTherapy` |
| **`Contraindication`** | `MedicinalProductId` | `Population`, `OtherTherapy` |
| **`UndesirableEffect`** | `MedicinalProductId` | `Population` |
| **`Interaction`** | `MedicinalProductId` | `Population`, `Interactant` |

`LocalLabel` also carries `DerivedFromGlobalLabelVersionId?` — which core version
this was written from. That is a **derivation** fact, not a publication one, and
is exactly the link D3 does *not* defer: it says where the text came from, not
which approved statements it prints.

### Screen words

Recorded in `docs/domain-model/labeling.md` at S001.

| Domain type | Screen |
|---|---|
| `GlobalLabel` | **Global label** |
| `LocalLabel` | **Local label** |
| `UndesirableEffect` | **Side effect** |
| `Population` | **Who it applies to** |
| `OtherTherapy` | **Used with** |

*Global label* is the plain word, held until real users are asked — Medical
Affairs says *CCDS*, Regulatory says *Core Data Sheet*. Whichever wins, the type
does not move.

### What the sketch got wrong

Kept rather than overwritten, because a corrected prediction is worth more than
a tidy document.

| The sketch said | What changed it |
|---|---|
| *"Model a `CodedConcept` value object once (`system`, `code`, `display`) and reuse it everywhere"* — listed as decision 4 | **Already built.** EPIC-010a S001 put it in `ReferenceData.Domain` per [ADR-058](../../adr/ADR-058-substances-are-shared-facts-ingredients-are-roles.md) §3. EPIC-018 inherits it — and inherits its trap: an owned coded value is tracked against exactly one owner, so every lookup returns a fresh instance |
| *"`Population` — value object / owned entity"* | **Entity, decided.** The sketch left it either/or; lifecycle settles it |
| *"`Labeling` (local label) — root, on Medicinal Product"* | **Renamed `LocalLabel`**, for the namespace collision above |
| *"links to `Content`"* — treated as a solved seam | **`ProductDocument` is scoped to `GlobalProductId` and has no market dimension.** Found during Phase 2, not during implementation. D5 is the answer, and it is a rule about ownership of meaning rather than a schema change |

### Change-case analysis

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| Real terminology (MedDRA/SNOMED/ICD) replaces the seed list | **High** | `CodedConcept.System` carries `regos-internal` from day one; the swap is a data migration |
| Someone asks which statements a label version printed | **High** | Nothing points that way yet, so the answer is additive. ADR-059 §3 names the five questions it must answer first |
| Label change control / approval (EPIC-008) | High | Versioned with dated status already; the workflow attaches to it |
| Cross-market divergence reporting (EPIC-011) | High | Per-market structured statements are exactly the input |
| SPL / PLR export (EPIC-007) | Medium | Structured sections map to SPL sections |
| A fifth statement type needs `Population` | Medium | Add the owned collection to the new root — no shared-table migration. A fifth is also the trigger to re-ask ADR-018's question |
| Artwork tied to specific packaging components | Medium | Nullable seam to `Packaging` (EPIC-010) |
| Indications approved on different dates per market | Medium | The statement already lives on the market-local tier |

---

## Phase 3 — Stories

| # | Story | Slice | Status |
|---|---|---|---|
| **S001** | **`GlobalLabel` + `GlobalLabelVersion`** — the new context, versioned with dated status and effective dating, linked content, on the global product | context → domain → persistence → API → UI → browser proof | ✅ |
| **S002** | **`LocalLabel` + `LocalLabelRevision`** — the market's own controlled document, its revision history, and artwork as a type rather than an aggregate | full slice | ✅ |
| **S003** | **`Indication` + `Population` + `OtherTherapy`** — the authorisation, its dated decision history, and the qualifier that is corrected in place | full slice | ✅ |
| **S004** | **`Contraindication` + `UndesirableEffect`** — the second and third uses of the population shape, and where ADR-018's question was asked out loud | full slice | ✅ |

> **S004's design, signed off 2026-08-04.** Stated as a hypothesis with a
> falsifier, so the outcome is reviewable rather than a judgement made at the
> end:
>
> **Hypothesis** — if `Population`'s second and third EF configurations differ
> only by table name and foreign-key name, the persistence helper is earned.
> **Falsifier** — if either aggregate introduces a different rule or a different
> shape, do not abstract it. **A shared *domain* type is not on the table either
> way** (ADR-018, ES-014, and the owned-entity ownership problem ADR-058 already
> paid for).
>
> **Neither aggregate gets a `StatusHistory`**, and that asymmetry is the design.
> See [the rule in the domain model](../../domain-model/labeling.md): an
> indication is an authorisation the authority acts on directly; a
> contraindication and an undesirable effect are content inside an approved
> label, so their history is the `LocalLabelRevision` that published them.
>
> **`UndesirableEffect.Frequency`** — *very common* … *very rare* — is a coded
> concept on that aggregate alone. Orthogonal to `Population`, and nothing
> branches on it.
>
> ### The review contract
>
> **S004 is reviewed by answering these, not by recapping the epic.** Written
> down before implementation so the answers cannot be chosen to fit what was
> built:
>
> 1. Did the three `Population` configurations differ **only** by owner and
>    table name?
> 2. Was the EF helper therefore **earned** — or refused?
> 3. Did `Contraindication` and `UndesirableEffect` stay free of independent
>    history?
> 4. Did any `if (Type == Artwork)`-style branching appear? (`LocalLabelTypeBranchTests`
>    answers this without anyone looking.)
> 5. Did the browser proof show **amendment** on the second parent — one row
>    corrected, not replaced?
| **S005** | **`DrugInteraction` + `Interactant`** — the fourth clinical statement, and the substance seam | full slice | ✅ |

> **Continued rather than stopped, 2026-08-04.** S005 was designated the clean
> stop point because nothing depends on it — and it still is. But *"a good place
> to stop if priorities change"* and *"a good place to declare the epic
> complete"* are different decisions, and no external pressure is forcing the
> second. Stopping here would make S006's retro say *"the architecture is
> complete, and the epic does not meet its own Definition of Done"* — honest, and
> only worth saying if something made it necessary.
>
> **Hypothesis.** S005 is an application of settled patterns, not a modelling
> exercise: a coded statement on the market-local tier, owned populations, no
> history of its own, and a browser proof. **Two things are new** — an
> `Interaction` must name at least one `Interactant` (the first *at-least-one*
> invariant in the context), and an interactant may optionally point at a
> `Substance`, which is the seam `OtherTherapy` said would arrive "beside the
> text, never instead of it".
>
> **Falsifier.** If an interaction needs its own history, a lifecycle, or a
> `Population` that differs from the other three, it is not an application of
> settled patterns and should be reviewed on its own merits.
| **S006** | **Capstone** — *"which markets is indication X approved in?"* end to end, label workspace on the market view, browser proof, retro | query → UI → test → docs | ✅ |

> **S006's design, signed off 2026-08-04. It is a verification story, not a
> modelling one** — and the hypothesis is written to be independently falsifiable:
>
> > **If the capstone query can be implemented entirely as a read over the
> > existing model, EPIC-018 captured the necessary regulatory facts without
> > introducing reporting-specific structures.**
>
> **Falsifier.** If answering it needs a stored field, a projection table, a
> denormalised summary or a join RegOS cannot express through its filtered roots,
> the model was incomplete — and the retro says where.
>
> Note what is under test. Not *"can we write the SQL"*, but **does the domain
> already hold every fact the question needs**.
>
> ### The question contains a false premise
>
> There is no cross-market *"indication X"*. France's indication and Canada's are
> separate aggregates with separate wording, populations and decision histories.
> **What they share is the condition code** — so the query is keyed on a code, not
> on an indication id, and is named `ListMarketsForCondition` for what it takes.
>
> [`IndicationSummary.ConditionCode`](../../../src/Labeling/RegOS.Labeling.Application/Queries/ListIndications/IndicationSummary.cs)
> has said so since S003: *"The join key. Type 2 diabetes mellitus and Diabète
> sucré de type 2 share it; the label texts do not."* This is
> [ADR-058](../../adr/ADR-058-substances-are-shared-facts-ingredients-are-roles.md) §1's
> backwards question finally being asked.
>
> ### The four decisions
>
> | # | Decision | Why |
> |---|---|---|
> | **D1** | **Product-scoped**, `/api/products/{globalProductId}/indications/{conditionCode}/markets` | Nobody asks *"where is diabetes approved?"* — they ask *"where is **this product** approved for diabetes?"*. A tenant-wide answer conflates product A in Japan with product B in Canada, which is not a regulatory question |
> | **D2** | **"Approved in" is a status filter** | The read must separate *approved here* from *was approved here once*. **The capstone is the first read that depends on S003's decision being right** — which is what makes it the falsifier rather than a demo |
> | **D3** | **Return every market that has an indication for that condition, with its current standing** | Not *"every market that records the condition"* — that is a persistence query. This is a regulatory fact. `Japan Approved · France Withdrawn · Canada Approved` is more informative than a silent filter, and it avoids inventing a second endpoint for the same question asked with the opposite sign |
> | **D4** | **The picker is the bundled vocabulary, and "no market" is a legitimate answer** | Eight `regos-internal` conditions, visibly a demonstration set. It also shows the query is driven by the coded condition rather than by whatever happens to be recorded |
>
> ### What it must not become
>
> EPIC-011 owns cross-market comparison. **One condition at a time, no matrix, no
> wording diff.** Each row shows that market's label text beside the shared code —
> ADR-059's principle visible in one table — but **showing is not comparing**.
>
> The pressure after S006 will be *"since we already have the data…"*: side-by-side
> wording, population comparison, a matrix, a diff. That is the boundary that moves
> when nobody names it, so it is named here.

> **For the retro, recorded now so it is not reconstructed later.** EPIC-018 used
> one loop five times, and it produced better decisions than design-first-defend-later:
>
> 1. **State the modelling hypothesis.**
> 2. **State the falsifier** — what would make it wrong.
> 3. **Implement.**
> 4. **Gather evidence**, ideally from something that is not an opinion: a
>    compiler, a migration diff, a browser assertion.
> 5. **Record why the hypothesis survived — or did not.**
>
> Applied to: local revisions (S002), `Population`'s identity (S003), the
> persistence helper (S004), the absence of a status history (S004), and the
> artwork watchpoint (armed, still unfired).
>
> **It is one epic's experience, so it is material for the retro and not yet a
> standard.** If S006 confirms it, `implementation-standards.md` is where it
> belongs — second use observe, third use evaluate, the same discipline the loop
> itself enforces.

> **The second retro theme, named 2026-08-04, and the stronger of the two.**
>
> > **EPIC-018 repeatedly replaced conventions with model-enforced correctness.**
>
> Publishing requires a document. A revision cannot take effect before it was
> approved. Interactants cannot become empty. Owned collections removed the
> `Include`-dependent correctness that broke EPIC-004 S005. Architectural tests
> replaced checklist items. Watchpoint tests detect drift nobody is watching for.
>
> It cuts across all six stories, and it is a stronger architectural observation
> than any single implementation decision in them — because each instance moved a
> correctness obligation off a developer's memory and into something that fails
> loudly.

> **S005 is where to stop if the epic runs long**, and that is decided now rather
> than under pressure. Nothing else depends on interactions; every story before
> it establishes the backbone. Cutting it leaves the Definition of Done unmet and
> the architecture whole — which is the better of the two failures available.

**ADR:** [ADR-059](../../adr/ADR-059-clinical-statements-are-facts-labels-are-artifacts.md) — written before S001, as canon requires for a new bounded context.

---

## S006 — the capstone, and what it proved

> **The hypothesis:** if the capstone query is a pure read over the existing
> model, EPIC-018 captured the necessary regulatory facts without introducing
> reporting-specific structures.

**It held, and the evidence is not an opinion:**

```
$ dotnet ef migrations has-pending-model-changes
No changes have been made to the model since the last migration.
```

No column, no projection table, no stored summary, no denormalised count. The
capstone is `ListMarketsForCondition` — one handler, one endpoint, one screen —
reading facts that S003 had already put in the right shape.

### What made it possible, named precisely

| The read needs | Which exists because |
|---|---|
| A key that means the same thing in two markets | **S003 coded the condition.** `IndicationSummary.ConditionCode` was documented as *"the join key"* three stories before anything joined on it |
| To tell *approved here* from *was approved here once* | **S003 gave an indication a dated decision history.** Until now that was a modelling claim; this is the first read that **depends** on it |
| To reach markets and countries from a statement | **The market-local tier (EPIC-017).** An indication hangs off `MedicinalProduct`, so "which markets" is a join, not a design problem |

### Decisions made while building

| Decision | Why |
|---|---|
| **`IsAnAuthorisation` is a static predicate on `Indication`**, not a comparison inside the query | *"Is this product approved for that?"* is a domain question. Three of the four statuses are authorisations — `Expanded` widened one and `Restricted` narrowed one — and a query that spelled out `!= Withdrawn` would let a fifth status answer by accident. A theory pins all four; a second test fails if the enum grows |
| **Static, not a computed property** | A get-only property on an aggregate is one more thing for EF to have an opinion about, and this epic has already paid three times for persistence-shaped surprises. A static method is not a mapped member under any convention |
| **The section sits on the product's *markets* page**, not with the global labels | The answer is about markets. `ProductLabelsPage` says of itself that it *"deliberately says nothing about markets"*, and that was worth preserving |
| **The picker offers the whole vocabulary**, not only conditions already recorded | A second "which conditions exist here" query would have been the cheaper-looking option and the less honest one. *"No market records this indication"* is an answer, and it is the one that shows the read is driven by the code |
| **The condition is URL-encoded in the route** | `PAIN-MOD` is safe today. The seam exists so a licensed terminology's codes do not become a routing bug later |

### The gate that was not being run

**`npm run build` had failed since S001** — a `Select` whose `onValueChange`
emits `string | null` against a handler typed for `string`. S002, S003, S004 and
S005 were each verified as complete with that break in the tree.

It survived because the verification loop was `dotnet test` plus Playwright, and
**the browser proof runs against `vite dev`, which does not typecheck.** The
frontend had a gate; nothing ran it. Fixed in `2a82753`, and the loop now ends
with `npm run build`.

`npm run lint` also fails at baseline — six problems, none of them from this
epic — and is left alone here rather than fixed inside a story that did not
cause it.

**Verified:** 19/19 suites, 0 failed, **1410 tests** · **111/111 browser specs**
against an isolated stack (API 5301, web 5174, `regos_s006`) · CORS reverted and
confirmed absent from `src/`.

---

## Definition of Done — the audit

Checked line by line rather than declared met.

| The DoD said | Outcome |
|---|---|
| A global label exists for a global product, versioned, with a dated status history and a linked content file | ✅ S001 |
| A local label exists for a market-local product, records the global label it derives from, and carries its own language and version | ✅ S002 |
| Artwork can be attached to a local label with its own dated status | ⚠️ **Capability met, shape changed** — see below |
| Indications, contraindications, undesirable effects and interactions recorded against a market-local product, each optionally scoped to a population | ✅ S003–S005 |
| The same population shape serves all four — proven by a test under at least two different parents | ✅ S004 proved the second and third parents; S005 the fourth |
| *"Which markets is indication X approved in?"* answerable through the API | ✅ S006 |
| Browser proof: global label → local label for one market → indication with a paediatric population → seen on the market view | ✅ S006, walked in one pass rather than assembled from five specs |
| ADR for the label hierarchy and the shared-qualifier modelling | ✅ ADR-059 |

### The one that changed shape

Phase 1 expected `LocalLabel` to **own** an `Artwork` child. What exists is
artwork **as** a `LocalLabel` of type `ARTWORK`, with its own dated revisions and
its own data-carrier code — a sibling, not a child.

**The capability matches the intent; the aggregate shape changed because artwork
proved to be another controlled local label rather than a child entity.** A
printed carton is a document an authority approved, on its own approval clock.
That is not a compromise, and it is not a Phase-1 error worth hiding: it is what
the epic learned.

The watchpoint armed at S002 — `LocalLabelTypeBranchTests`, which fails when
`if (Type == Artwork)` branching starts accumulating — **has not fired across
four stories**. If it does, the conversation it opens is extracting
`CartonArtwork`, and the evidence for that conversation will already be in the
test output.

---

## Retro

### What EPIC-018 did that is worth repeating

**1. It stated hypotheses that could fail, then went and looked.**

Five times: local revisions (S002), `Population`'s identity (S003), the
persistence helper (S004), the absence of a status history (S004), the capstone
read (S006). Each with a falsifier written *before* implementation, and each
resolved by something that is not an opinion — a compiler error, a migration
diff that contained `IndicationPopulations` zero times, a row count that stayed
at one through a correction, `has-pending-model-changes` reporting nothing.

**One criterion was only half met, and that is recorded too**: S003 asked for
`RetirePopulation` and got `Remove`. A qualifier has no lifecycle of its own.

> **Still not promoted to a standard.** One epic's experience. `implementation-standards.md`
> is where it belongs *if a second epic independently benefits* — second use
> observe, third use evaluate, which is the same discipline the loop enforces.
> The next epic to try it should say whether it helped.

**2. It repeatedly replaced conventions with model-enforced correctness.**

The stronger of the two themes, because each instance moved a correctness
obligation off somebody's memory and into something that fails loudly:

| Was a convention | Became |
|---|---|
| "Attach the document before publishing" | `PublishRequiresContent` — the version refuses |
| "Don't back-date an effective date" | The revision refuses to take effect before it was approved |
| "An interaction should name what it interacts with" | The last interactant cannot be removed |
| "Remember to `Include` the populations" | Owned collections load with their owner — the EPIC-004 S005 failure mode is unreachable by construction |
| A three-item aggregate checklist | `AggregateChildArchitectureTests` — and it immediately found five nullable foreign keys nobody was looking for |
| "Watch out for artwork branching" | `LocalLabelTypeBranchTests`, armed and quiet |
| "Only Withdrawn isn't an approval" | `Indication.IsAnAuthorisation`, with a test that fails if a fifth status appears |

**And one counter-example, which is why the theme is worth stating rather than
celebrating:** the frontend build gate existed the whole time and was simply not
run. A check that nobody executes is a convention wearing a test's clothes.

### RIM's nouns are named in isolation

Twice this epic, RIM's object name could not be used as-is:

- **`Labeling`** → `LocalLabel`, because the context is called `Labeling`.
- **`Interaction`** → `DrugInteraction`, because `RegOS.Interaction` is a bounded
  context (ADR-040).

Not two annoyances — one pattern. **RIM names objects as though nothing else
exists in the system; a bounded-context codebase names them in the presence of
everything else.** That is why some RIM nouns transfer directly and others need
adaptation, and it is a reason to expect the next RIM-derived epic to rename one
or two things for mechanical reasons. Both departures are recorded in ADR-059 §2
and here, so neither reads later as drift from the source model.

### What the next epic should carry forward

| | |
|---|---|
| **Read the previous retro before the design review.** | S001 lost a build cycle to EF's constructor binding — a surprise EPIC-010a's retro had already recorded. The design review did not catch it; the retro would have |
| **`npm run build` joins the verification loop.** | Alongside `dotnet test RegOS.slnx` and the browser suite |
| **The five nullable Organization foreign keys are still standing**, with `ContactRoleAssignment` elevated. | Its consequence is behavioural, not merely relational. Every removal from that grandfathered list should be a migration and a conscious review, never a quietly deleted string |
| **The deferred link stays deferred.** | Nothing points from a label version to the statements it published. ADR-059 §3 names the five versioning questions that must be answered first, and no one has asked for it |

---

## S002 — the workflow question, and its answer

The story was parked on one question, because its shape could not be settled
from RIM:

> **Can the identity of the current local label change while its parent global
> label version stays the same?**

**Answered 2026-08-04: yes, and not marginally.** The founder's answer is
recorded here at length because it is the justification for a versioned local
model, and a future reader should be able to check the reasoning rather than
inherit the conclusion.

### Why, in the industry's terms rather than the model's

> **A core label is the company's scientific position. A local label is a
> regulatory artifact approved by one health authority.** Related, and not the
> same document — which is why the regulator regulates the second.

```
CCDS v7 published
      │  translation · artwork · assessment · submission
      │  PMDA review · questions · approval
      ▼
Japan Label Revision 14      effective 3 Oct 2026, derived from CCDS v7
```

Japan already had thirteen revisions. They are Japan's regulatory history, not a
projection of the company's.

**And the local artifact changes without the global one.** A translation
correction, a typo, an artwork problem, a distributor address — the CCDS is
untouched and Japan issues Revision 15. Meanwhile CCDS v8 is adopted by France
immediately, by Brazil six months later, by Australia with extra wording, and by
Japan next quarter. Every market holds a different current revision, approval
date and effective date.

**The operational question, restated better than we first asked it.** We had
planned to ask *"do you replace the PDF?"* — a software question. The regulatory
one is:

> *"When a local approved label changes for any reason, do you issue a new
> controlled revision, or overwrite the existing approved label?"*

Overwriting an approved labelling document is a governance failure. Approved
labelling is a controlled document and historical versions are retained. The
commercial RIM systems separate Company Core Data Sheet, Country Label and
Country Label Revision for exactly this reason.

**Artwork is the strongest case, not the weakest.** Manufacturer changes,
printer changes, barcodes, serialisation, QR codes, local legal statements,
distributor information — none touches a CCDS, and every one produces newly
approved local labelling.

### What this does not license

**Not symmetry.** Both tiers are versioned, and the reasons are unrelated:

| | Versioned because |
|---|---|
| `GlobalLabel` | the company's scientific position evolves |
| `LocalLabel` | each authority approves, delays, amends and republishes that position independently |

They intersect only through a derived-from link. Nothing inherits, and the two
status vocabularies are not assumed to match until a rule says they do.

**Not the approval workflow.** Submission, review, questions and approval are
the process; EPIC-018 records the *dated facts* it produces, and the process
itself stays EPIC-008's (see Out of scope).

### The four decisions that followed *(approved 2026-08-04)*

| # | Decision | Settled as |
|---|---|---|
| **D1** | **`LocalLabelRevision`, not `LocalLabelVersion`** | ✅ — the asymmetry is the point. *Version* is the company's word for its evolving position; *revision* is the authority's word for a controlled document it approved. Two names in one context is a standing reminder that the rules differ |
| **D2** | **Artwork is a `LocalLabel` type, not a fourth aggregate** | ✅ **for this epic**, with a watchpoint below. Prescribing information, patient leaflet and carton artwork are all controlled, authority-approved, market-specific, derived and revision-controlled — one lifecycle, one approval model, one set of APIs and one browser experience |
| **D3** | **`DerivedFromGlobalLabelVersionId` is nullable** | ✅ — a migrated portfolio will not know which core version Revision 9 came from, and a local-first company holds approved labelling before any core label exists here. Required would force people to invent history, which is always the wrong trade |
| **D4** | **`ApprovedOn` is separate from `EffectiveFrom`/`EffectiveTo`** | ✅ — *approved 12 May, effective 1 June* and *approved 12 May, effective immediately* both occur. **A revision cannot enter force without an approval date**: the local analogue of *publishing requires a document*, and a statement about the artifact's truth rather than a workflow step |

### The artwork watchpoint

> **Split when artwork develops its own persistent invariants — not when it
> acquires more attributes.**

Nullable columns are not the signal. `AtcCode` on `MedicinalProduct` is one
already, and it has caused nobody any trouble. The signal is **branching**:

```csharp
if (Type == Artwork)   // ← more than occasionally, and the aggregate is asking to split
{
    …
}
```

If pack size, SKU, GTIN, barcode, printer and pack configuration stop being
optional metadata and become *mandatory business concepts*, they are no longer
decorations on a label — they are the identity of a different thing, and
`CartonArtwork` should be its own root.

**The test to apply:** does every invariant apply equally to every local label
type? While the answer is yes, one aggregate. The moment the domain is written
with type checks, that answer has changed.

*S002 arms this as `LocalLabelTypeBranchTests` once `LocalLabelType` exists — a
count over `src/Labeling/**/Domain`, not a judgement call made a year from now
by whoever is reading.*

### Two things settled on the way

**Independent document evolution creates the history; audit merely obliges us to
retain it.** Not the other way round. An earlier draft of this analysis had *"a
regulator may ask what the Japanese label said on 3 March 2025"* as the reason to
version, which would have let a compliance requirement justify an aggregate.

**`LocalLabel` is not a projection over `GlobalLabel`** — adoption lag alone
settles that, whatever else it holds.

---

## S005 — the hypothesis, answered

**It held.** A coded statement on the market-local tier, an owned population that
amends in place on a fourth parent, no history of its own, one browser proof.
The falsifier — *a history, a lifecycle, or a `Population` that differs* — was
not triggered: the fourth call to `ClinicalStatementConfiguration.Populations`
takes the same two strings as the other three.

### The two new things, both asserted

**An interaction must name at least one interactant.** The first *at-least-one*
invariant in the context, and it is genuinely different from the others: a
contraindication with no population applies to everyone, and an indication with
no therapy is simply unqualified — but an interaction with nothing to interact
with is not an under-specified statement, it is not a statement. The interactant
is therefore supplied to `Record` rather than added afterwards, and
`RemoveInteractant` refuses the last one.

**The substance seam arrived exactly as `OtherTherapy` predicted it would** —
*"an optional link beside the text, never instead of it"*. Most interactants are
not compounds RegOS knows: grapefruit juice, alcohol, *CYP3A4 inhibitors*. A
required `SubstanceId` would make the ordinary case unrecordable. Set, and
*"which of our products interact with warfarin?"* is a join; unset, the text
still says what the label says.

### One thing the compiler found

**RIM's `Interaction` collides with a bounded context.** `RegOS.Interaction` is
the health authority's letters, questions and meetings (ADR-040), so the bare
noun forces a `using` alias in every file that sees both — the collision this
epic renamed `Labeling` to `LocalLabel` to avoid. The aggregate is
`DrugInteraction`; the screen still says **Interactions**.

**Second mechanical departure from RIM's noun, and the second time the reason
was a namespace rather than a modelling judgement.** Worth noticing as a pattern:
RIM names objects as though nothing else exists in the system, and a codebase
with bounded contexts cannot take every noun at face value.

### Decisions made while building

| Decision | Why |
|---|---|
| **`Incidence` dropped from Phase 1's attribute list** | Frequency-shaped, and `UndesirableEffect` already carries frequency. Nobody asked for it here, and two ways to say how often something happens is one too many |
| **`Severity` and `Management` are both nullable** | Many labels describe an interaction and what to do about it without grading it. Inventing a grade would assert a clinical judgement nobody made |
| **`InteractionType` is recorded, not derived** | *St John's wort* is a herbal product and a CYP3A4 inducer; which one a label means is the label's statement, not ours to infer from the interactant |
| **The interactant FK is `Restrict`** | Deleting a substance must not silently rewrite what a label says |
| **`MarketInteractions` is its own component** | An interaction names what it is *with*, and that list is never empty — a different shape from the other two statements, so not a third section of theirs |

**Verified:** 19/19 suites, 0 failed, **1405 tests** · **110/110 browser specs**
· CORS reverted and confirmed absent from `src/`.

---

## S004 — the review contract, answered

| # | Question | Answer |
|---|---|---|
| 1 | Did the three `Population` configurations differ **only** by owner and table name? | **Yes**, and more strongly than expected — see below |
| 2 | Was the EF helper **earned**? | **Yes.** `ClinicalStatementConfiguration.Populations(builder, table, ownerKey)`, called three times with two strings |
| 3 | Did `Contraindication` and `UndesirableEffect` stay free of independent history? | **Yes**, and a test asserts it rather than a comment claiming it |
| 4 | Did any `if (Type == Artwork)`-style branching appear? | **No.** `LocalLabelTypeBranchTests` is green without anyone looking |
| 5 | Did the browser proof show **amendment** on the second parent? | **Yes** — and the third. One row through the correction, twice |

### The evidence for Q1 is stronger than a diff

The two new population tables are column-for-column identical to each other,
differing only in the foreign-key name. But the decisive fact is what the
migration **did not** contain:

> **`IndicationPopulations` appears zero times in the S004 migration.**

Converting S003's population from a standalone entity to an owned collection
*through the shared helper* produced **no schema change at all**. The helper does
not approximate the original mapping; it reproduces it exactly. If the shapes had
differed anywhere — a nullability, a length, an index — the migration would have
said so.

### What the implementation forced, and it was not a preference

Three aggregates cannot own one entity type with three tables: EF scopes an owned
type per owner, which is what turns one CLR class into three entity types. So
`PopulationConfiguration` had to become an `OwnsMany` helper or be copied twice.
**The hypothesis was answerable because the alternative was concrete.**

A side effect worth noting: owned collections load with their owner, so the three
repositories need no `Include` for populations — the EPIC-004 S005 failure mode
(a rule reading a collection that was never loaded) is unreachable by
construction rather than by discipline.

### Decisions made while building

| Decision | Why |
|---|---|
| **One `Population` CLR type, moved to `Aggregates/ClinicalStatements/`** | The third demonstrated need (ADR-018). This is **not** the shared *base type across aggregate roots* ADR-059 §4 forbids — nothing couples the three roots, and either may grow a rule the other does not, at which point it stops being one shape and should stop being one class |
| **`ClinicalCondition` is two static helpers, not a base class** | Inheritance would put a rule added for undesirable effects into the contraindication that never asked for one |
| **`OtherTherapy` is *not* on `Contraindication`** | Phase 1 allows it; the DoD does not require it; nobody asked. Its second use would have been speculative creation, so it stays at one and `OtherTherapy` stays in `Indications/` |
| **`ClinicalStatementErrors` split out of `IndicationErrors`** | So `Contraindication` does not reach into `Indications` to say *"that age needs a unit"*. The messages did not change — only where they live |
| **The frontend population form is parameterised by `StatementKind`** | Same story as the persistence helper, one layer up: one form, one schema, one save call, three route bases |

**Verified:** 19/19 suites, 0 failed, **1391 tests** · **109/109 browser specs**
· CORS reverted and confirmed absent from `src/`.

---

## S003 — what was decided while building it, and how the criterion came out

> **The criterion: does the aggregate grow operations, or only a collection?**

**It grew `AmendPopulation`, and that is the answer.** Correcting a band from
12+ to 6+ keeps the same `PopulationId` — asserted in the domain tests and again
in the browser proof, where the row count stays at one through the correction.
Remove-and-re-add would have said the label once applied to a population it
never applied to. **D2 stands: `Population` is an entity.**

**One word of the criterion did not survive.** It asked for `Retire`, and the
operation is `Remove`. A population qualifier has no lifecycle of its own — it
is part of the statement as it currently stands, and the regulatory history
lives in `StatusHistory` where the decisions are. A qualifier recorded in error
is a mistake to correct, not a fact to preserve. Recorded because a criterion
half-met is worth more said than smoothed over.

| Decision | Why |
|---|---|
| **`ClinicalConditionVocabulary`, not `ClinicalVocabulary`** | Contraindications, adverse reactions and physiological conditions are not obviously one list, and the broader name would have been stretched to hold them by whoever needed one first |
| **The condition is coded and the text is not** | The code is what makes the authorisation comparable across markets; the text is what this market's label says. Same split as `Ingredient`: a coded substance, a strength stated beside it |
| **`Gender` and `AgeUnit` are coded, not enums** | Nothing branches on either. The "does a rule branch on it?" test, applied a fourth time |
| **An age bound requires a unit, and a unit requires a bound** | *2 to 12* could be months or years, and a unit with no age says nothing at all. Both refusals name the ambiguity rather than the field |
| **`OtherTherapy.Therapy` is free text** | It may be a substance RegOS knows, a drug class it does not, or a procedure that is no product at all. A required `SubstanceId` would make two of those three unrecordable — and *"which indications name metformin?"* arrives as an optional link **beside** the text, never instead of it |
| **`RestateLabelText` is not a decision** | Wording and authorisation move on different clocks, which is the whole reason this aggregate has no revisions. Restating the text leaves the status history untouched, and a test says so |

### What S006 may and may not claim

> **"Demonstrates cross-market coded indication queries using the bundled
> demonstration vocabulary."**

Not *"supports clinical indication search"*. RegOS ships eight conditions, all
`regos-internal`, and nobody's real indication is among them. The architecture
supports a licensed terminology; today's vocabulary exists to exercise the model.
Stating the narrower claim is the difference between *"we intentionally shipped a
minimal controlled vocabulary"* and *"we implemented this incorrectly"*.

**Verified:** 19/19 suites, 0 failed, **1378 tests** · **108/108 browser specs**
· CORS reverted and confirmed absent from `src/`.

---

## S003 — the criterion it was reviewed against

S003 is where D2's original decision — **`Population` is an owned entity, not a
value object** — gets tested the way S002 tested `LocalLabelRevision`.

> **The test is whether the aggregate naturally develops operations, not whether
> it holds a collection.**

| If `Indication` grows | Then |
|---|---|
| `AddPopulation`, `AmendPopulation`, `RetirePopulation` | the entity decision was right: a qualifier that is corrected in place has identity |
| only `AddPopulation`, with amendment meaning remove-and-re-add | it is a value object, and the decision should be reversed while it is cheap |

Build it **once, on one parent**. The second and third uses arrive in S004, and
that is when [ADR-018](../../adr/ADR-018-rule-of-three.md)'s question gets asked
out loud — not before.

---

## S002 — what was decided while building it

| Decision | Why |
|---|---|
| **A revision cannot take effect before it was approved** | Not in the design, and not a workflow rule: a label in force ahead of its own approval is not a state that exists. Same day is allowed — *effective immediately* is ordinary |
| **`LocalLabelRevisionStatus` is its own enum**, three identical words to the global one | Merging them would let a rule added to one lifecycle silently reach the other, which is the exact coupling D1 renamed the type to prevent |
| **`PrepareRevision` restates rather than patches** | Document, derivation, artwork code and summary are settled together. A caller able to change the document without the derivation could point a translation of core v7 at a file that says v8 |
| **A `core-versions` query, flattened across the product's core labels** | The question a person asks while preparing a Japanese revision is *"which core version is this?"*, not *"which core label, then which version"*. Drafts are excluded — a market cannot descend from something the company has not issued. Superseded ones are included, because a market catching up is the ordinary case |
| **`DataCarrierCode` only; no SKUs** | Artwork's one identifying attribute is one nullable column. Pack size, SKU and GTIN are EPIC-010's packaging model, and building a second one here would be the speculative creation ADR-018 forbids |
| **Markets are created through the UI in the browser proof** | The `/master-data/countries` route is unprefixed (an SC-001 grandfathered entry) and resolving it from a spec would bake that in. Clicking through the market page is what a user does anyway |

**Verified:** 19/19 suites, 0 failed, **1355 tests** · **107/107 browser specs**
against an isolated stack · CORS reverted and confirmed absent from `src/`.

---

## S001 — what was decided while building it

Recorded here rather than only in the commit, because each was a call the Phase-2
design did not make.

| Decision | Why |
|---|---|
| **No `Status` on `GlobalLabel`** | A label's meaningful lifecycle lives in its versions. "Retire this label" is a capability nobody asked for, and a column that is always `Active` is a field nobody filled in — the call `Substance` made on `IsActive`. **Known gap: a label cannot be renamed or retired**, stated so it is recognised as deferred rather than reported as a defect |
| **`DiscardDraft`, the one deletion here** | Without it a draft started by mistake is permanent: the one-open-draft rule blocks a replacement, and publishing needs content nobody intends to attach. It does not contradict ES-018 — a draft has never been in force, was never cited, and never described what the company said. The guard is `Draft`, not "not in force" |
| **Publishing requires content** | A version with no document is a number, and a number is not a label. This is what makes the `ProductDocumentId` link load-bearing rather than decorative, and it is the rule the browser proof exercises first |
| **Publish and supersede are one act** | A label family with two versions in force is not a state a company can be in. The supersede date is computed — the day before the replacement takes effect — because a caller who could supply it could leave a gap or an overlap |
| **`Aggregates/GlobalLabels/`, plural** | A singular folder makes the namespace equal the type name, which is the collision S000 removed fourteen `using` aliases to delete. Recorded in [slice-conventions](../../engineering/slice-conventions.md) v1.2, and `RegOSDbContext` names `GlobalLabel` with no alias as a result |
| **`CodedConceptDto` consolidated** | The label vocabulary was the **third** consumer, and the second had quietly written its own copy under a different name (`PharmaceuticalConceptDto`). Same on the frontend — `CodedConcept` and `CodedValue` are now one type in `shared/types/`. ADR-018's demonstrated need, retired opportunistically because this slice was already in those files |

**What the design did not predict, and cost a build cycle:** `GlobalLabel` needs
a *parameterless* constructor where `GlobalProduct` beside it does not — EF binds
constructor parameters from mapped properties, and an owned value object is not
one. That is the third persistence-shaped surprise in two epics, and it was in
EPIC-010a's retro. **Reading the retro would have caught it; the design review
did not.**

**Verified:** 19/19 test suites, 0 failed, 1335 tests · 106/106 browser specs
against an isolated stack (API 5301, web 5174, throwaway database) · CORS
widening reverted and confirmed absent from `src/`.
