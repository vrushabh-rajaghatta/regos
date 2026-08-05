# EPIC-022 — Country depth

**Status:** 🟢 Complete · **Branch:** `epic/EPIC-022-country-depth` (cut at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

`Country` is the oldest table in RegOS and **the only reference entity whose every attribute is for display**. `Code` and `Name` are what you show in a dropdown. The three RIM attributes it is missing — climatic zone, languages, regions — are what you **decide** by. This closes that, plus the two ISO identity fields machine-readable output needs.

> **Phase 1 below is settled. Phase 2 was signed off on pull-in (2026-08-05)** —
> five of its six sketched decisions confirmed, one strengthened, and a seventh
> added that the sketch did not ask. Phase 3 was re-confirmed with S004 widened.
> The sketch's own wording is kept below the approved section, because a
> corrected prediction is worth more than a tidy document.

---

## Phase 1 — Epic plan

### Outcome
A country stops being a label on a dropdown and starts being a set of facts other capabilities reason from: *"does our stability data support this market?"*, *"which languages does this market's labelling need?"*, *"which of our markets are in the EU?"* — and machine-readable output can name the country the way ISO and the regulators do.

### The finding this responds to

Assessed against the DIA RIM object model, `Country` sits at **4 of 12 attributes (33%)** — the lowest of any object RegOS has had since Sprint 11. The reason is visible in git: **`Country.cs` has had no behavioural change since the commit that created it.** Its only two subsequent commits are a folder move (`0bd0d56`) and a repo-wide exception refactor (`77e3681`). The seed file has never had a data change at all.

Every other reference entity has been deepened since — `Authority` gained divisions, `DocumentType` gained tenant extension, `SubmissionType` gained sub-types, `Substance` arrived with coded class/type axes and a documented sourcing stance. Country was built when the only question anyone asked of it was *"which market is this application for?"*, and for that question a code and a name are complete.

**The split is exact:**

| Present | For |
|---|---|
| `Code` (ISO alpha-2), `Name` | display |
| **Climatic Zone, Languages, Regions** — all three **Multiple** in RIM | **decision** |

> RIM's *Climatic Zone* is answered in RegOS by **accepted stability
> conditions**, which is what the authoritative source publishes
> ([D6](#d6--amended-in-place-before-a-line-of-s004-was-written)). The question
> is the same one; the value is not a zone letter.

### What each one decides

| RIM attribute | RIM shape | Decides | Consumer |
|---|---|---|---|
| **Climatic Zone** | Controlled Vocab · Multiple · Opt | which stability data supports which market. ⚠ **RegOS does not hold a zone** — the authoritative source publishes the long-term **condition** each country accepts, and India's 30 °C/70% RH belongs to no zone. Amended by [D6](#d6--amended-in-place-before-a-line-of-s004-was-written); the line above is RIM's shape, not RegOS's | `ShelfLifeStorage` (EPIC-010b) |
| **Languages** | Controlled Vocab · Multiple · **Req** | which languages a market's labelling needs — Canada EN+FR, Belgium NL+FR, Switzerland DE+FR+IT | `LocalLabel.Language` (EPIC-018 ✅) |
| **Regions** | Controlled List · Multiple · Opt | which procedure and blueprint apply — EU, ICH, ASEAN, GCC, PIC/S, and they **overlap** | EPIC-020 country-scoped templates · EPIC-009 |
| **ISO 3-Char Code** | Controlled Vocab · Single · **Req** | machine-readable output | EPIC-007b (xEVMPD/IDMP) |
| **ISO Country Name** | Controlled Vocab · Single · **Req** | the official name those outputs require — *"Korea, Republic of"*, not *"South Korea"* | EPIC-007b |

### Two debts this pays

**1. EPIC-018 shipped a gap it could not close itself.** `LocalLabel.Language` exists; nothing can say which languages a market *requires*, so a user cannot be told their Canadian label set is incomplete. That is Country's omission, not Labeling's.

**2. `RegionCode` is a dead column.** `Country.Create` defaults it to `null`, **all eight seeds omit it**, there is no mutator, and `Country` has no update path — it can only ever be null. This is precisely the defect [`Substance`](../../../src/ReferenceData/RegOS.ReferenceData.Domain/Substances/Substance.cs) refuses by name: *"a persistent property with no acquisition path is the defect EPIC-007a spent three findings on."* Country predates that rule. RIM says Regions is **Multiple** anyway, so a single nullable string could not hold it even if something wrote to it.

### In scope ✅
- **`StabilityConditions`** — collection, the long-term conditions a market
  accepts. *Amended from `ClimaticZones` on 2026-08-05: [D6](#d6--amended-in-place-before-a-line-of-s004-was-written).*
- **`Languages`** — collection, ISO 639.
- **`Regions`** — collection, replacing `RegionCode`, which is **removed** rather than migrated (it has never held a value).
- **`IsoAlpha3Code`** and **`IsoName`** — the two identity fields.
- **`ShelfLifeStorage.TestedAt` and the match** — *amended from `Region`; see [D6](#d6--amended-in-place-before-a-line-of-s004-was-written) and [the carve-out](#the-carve-out--closed-before-phase-2-began)*.
- **The market view answers the label-language question** — required languages vs recorded local labels, advisory not blocking.
- **An evidence entry per vocabulary** — see [the sourcing question](#the-sourcing-question-settle-this-first).
- Browser proof, retro.

### Out of scope ⏸️ (deferred, with reason)
| Deferred | Why |
|---|---|
| **`Country → Process Plan Template`** (RIM attr #12) | → **EPIC-020**, which owns the object at the other end. Nothing to point at until it exists. |
| **Widening the seed beyond the eight countries** | This epic deepens what is there. Adding a ninth market is a seed change any epic can make — but note the shape now costs five facts per country instead of two, which is a reason to widen deliberately rather than casually. |
| **Steward CRUD over country data** | → **EPIC-012**, which owns the reference-data write side. This is depth of the *seed*, not a new authoring surface. |
| **Viewing countries, regions and languages as lists** | → **[EPIC-012](EPIC-012-reference-data-authoring-and-governance.md)**, which now owns the read half too and carries the founder's mockup (recorded 2026-08-05). **Moved there 2026-08-05.** The gap is real and bigger than this epic: nine vocabularies and ~18 governed lists exist and **no SPA route reaches any of them**. `GeographyVocabulary.Regions` is the sharpest case — five values, no endpoint, and two (`ASEAN`, `GCC`) referenced by no seeded country, so they cannot be observed by any means. **What stays here:** a market's page naming its own country's regions and required languages, which is this epic's DoD and does not need a browser. |
| **ISO 3166-2 subdivisions** (states, provinces) | Not in RIM. `PostalAddress.StateProvince` is free text and nothing reasons about it. Add when something does. |
| **Currency, timezone, calendar** | Not in RIM's Country. Regulatory fees carry their own unit on Application. |
| **Deriving required languages into a blocking validation** | Advisory only — see Phase-2 decision 4. Blocking belongs with a rule the blueprint states, not with geography. |
| **A country lifecycle** (`IsActive`, merged/renamed states) | RIM has none, nothing asks, and adding a flag nothing writes would repeat exactly the defect this epic exists to remove. |

### Definition of Done
- Each seeded country carries its ISO alpha-3 code, its ISO official name, its languages, its regions and the stability condition(s) it accepts — **all eight, no nulls standing in for "we didn't get to it"**.
- `RegionCode` is gone from the model and the schema.
- *"Which of our markets are in the EU?"* and *"does our stability data support this market?"* are answerable through the API.
- A market's page shows the languages that market requires beside the local labels actually recorded, and says which are missing — **advisory, not blocking**.
- A shelf life states the condition it was demonstrated under, and a pack in a market that does not accept that condition is **reported, not prevented** (the EPIC-005 expiry precedent: derive the interpretation, never block on it).
- Each vocabulary has an entry in `docs/evidence/` naming its source, and the seed file carries the same hand-curation statement `Substance` carries.
- ADR only if the `LanguageCode` move (decision 2) is taken.

### It closes no RIM objects

Stated plainly, the way [EPIC-007a](EPIC-007a-ectd-package-generation.md) was: this deepens one object from 33% to ~92% and closes **zero**. The [runway](../BACKLOG.md#the-runway) figure will not move. Coverage measures breadth; this is depth, and the case for it is the two debts above, not the number.

---

## The sourcing question — settle this first

Every one of the five is a **controlled vocabulary RegOS does not hold an authoritative register for**, and this project has been caught by exactly that once already — the `file-tag` correction, where *"we know there is a vocabulary"* was recorded as *"we hold the vocabulary"* on the strength of a sentence.

| Vocabulary | Authority | Held? |
|---|---|---|
| ISO 3166-1 alpha-3 + official names | ISO | Widely published; the full register is licensed |
| ~~ICH climatic zones~~ → **accepted stability conditions** | **WHO**, *Stability conditions for WHO Member States by Region* (update March 2021), previously Table 2 in Annex 2 to TRS 953 | **Fetched and read, 2026-08-05** — and it changed the model: it publishes conditions, not zone letters ([E39](../../evidence/EPIC-022/stability-conditions.md)) |
| ISO 639 language codes | ISO | Widely published |
| Regions (EU / ICH / PIC/S / ASEAN / GCC) | **No single authority** — each body publishes its own membership | Multiple sources, each small |

**The honest position, and the one that unblocks the epic:** for **eight countries** every value is hand-verifiable against public sources, and eight rows is not a register. So seed by hand, and say so in the file the way `Substance` does —

> *Demonstration seed data only. These records intentionally do not represent an authoritative geography, terminology or membership register.*

— then record each source in `docs/evidence/EPIC-022/`. That keeps the distinction the `file-tag` correction was written to protect: **a hand-curated eight-row seed and an authoritative register are different evidence levels**, and only one of them can be widened without going and fetching something.

> **Settled 2026-08-05, and not the way this section expected.** It said
> *"fetch ICH Q1A(R2) before S004"*. **ICH withdrew Q1F**, and the zone-letter
> mapping this epic wanted was never in Q1A(R2). What WHO publishes is the
> long-term testing **condition** each member state accepts — so the model holds
> conditions and no zone letter at all
> ([D6](#d6--amended-in-place-before-a-line-of-s004-was-written)).
>
> The instinct was right and the target was wrong: **the one value here a
> careful person cannot reconstruct from memory** turned out to be India's
> **30 °C/70% RH**, which belongs to no zone anybody publishes.

---

## The carve-out — closed before Phase 2 began

**It was not taken, and it cost almost nothing.**

This section asked EPIC-010b S003 to add `Region` to
[`ShelfLifeStorage`](../../../src/Product/RegOS.Product.Domain/Product/ShelfLifeStorage.cs)
while that type was still being authored. **S003 shipped the night before this
plan was written**, scoped to exactly the two concepts it had been signed off
for — `LegalStatusOfSupply` and `ShelfLifeStorage` — and the window closed
unnoticed.

The price this section quoted was *"a migration on a shipped table plus a
backfill nobody has the data for"*. **The backfill does not exist.** RegOS is
pre-customer: every pack ever recorded lives in a dev seed or a throwaway test
database. What is left is one nullable coded field on a value object, which is
the same cost the carve-out was trying to avoid.

The residue is real but small, and worth naming: **every pack recorded between
now and S004 carries no region.** For a product with no customers that set is
empty, and it stops growing the moment S004 lands.

**So S004 owns both halves** — the field on `ShelfLifeStorage` and the match
against the market's accepted conditions — which is arguably the better shape.
Stability becomes actionable in one story instead of being half-built in an epic
that had no way to use it.

**And owning both halves is what saved it.** Had S003 taken the carve-out, the
field would have shipped as `Region` holding a zone letter, and the source that
killed zones would have been read *after* there was data in the column. The
residue this section worried about was the cheap half; **the expensive half was
building the wrong abstraction early**, and the sequencing prevented it.

> **The original recommendation is preserved above in the revision history of
> this file rather than restated here.** It was right when written and was
> overtaken by ordinary sequencing, which is the most common way a plan is
> wrong.

---

## Phase 2 — Domain design *(approved 2026-08-05)*

### Shape

```
Country
├── Code           string   ISO 3166-1 alpha-2   (exists)
├── IsoAlpha3Code  string   ISO 3166-1 alpha-3   NEW
├── Name           string   common name          (exists)
├── IsoName        string   ISO official name    NEW
├── RegionCode     string?  ────────────────────  REMOVED
├── Regions        collection<CodedConcept>       NEW
├── Languages      collection<LanguageCode>       NEW
└── StabilityConditions                           NEW
                   collection<CodedConcept>       (amended from ClimaticZones — D6)
```

### The seven decisions

| # | Decision | Settled as |
|---|---|---|
| **D1** | **Regions and stability conditions are collections of `CodedConcept`** | ✅ approved — governed external vocabularies is exactly what `CodedConcept` is for, and `GeographyVocabulary` becomes the **eighth** class in `ReferenceData/Terminology/` (the sketch said seventh; `SupplyVocabulary` landed in 010b S003 after that count was written) |
| **D2** | **`LanguageCode` moves from `src/Product` to `src/ReferenceData`** | ✅ approved, **and on a stronger argument than the sketch's** — see below. **ADR-062**, because it is a cross-context change |
| **D3** | **`RegionCode` is dropped, not migrated** | ✅ approved — verified dead in the strongest sense: **no reader, no writer, no mutator, and all eight seeds omit it.** It can only ever have been null. The migration says so, so nobody goes looking for lost data |
| **D4** | **Required languages are advisory, never blocking** | ✅ approved — EPIC-002's severity argument. A label set mid-authoring must not be refused because Canada also needs French. Blocking belongs to a rule a blueprint states, not to geography |
| **D5** | **The match is derived on read, never stored** | ✅ approved — the EPIC-005 expiry precedent exactly. Store the conditions the pack's data was generated at and the conditions the market accepts; derive the interpretation. A stored `supported: true` rots the moment either side changes, and both sides do |
| **D6** | **Countries hold accepted stability *conditions*, not climatic zones** | ✅ approved as first written, then **amended in place on 2026-08-05 before S004 was built** — see below. The overlap survived; the thing being overlapped did not |
| **D7** | **`Country` stays flat master data, and `CountryId` stays a record struct** | ✅ approved — **the question the sketch did not ask.** See below |

### D2 — why the move, restated

The sketch justified it by [ADR-018](../../adr/ADR-018-rule-of-three.md): Country
would be `LanguageCode`'s third consumer after `TradeName` and `LocalLabel`.

**That is the weaker half of the argument.** The type's own docstring already
named the condition that would change the answer:

> *"countries drive validation, authority selection and market identity, whereas
> language currently drives display… what makes a governed `Language` table
> premature"*

**S003 makes language drive validation.** A market that requires EN and FR, read
against the local labels actually recorded, is precisely the transition that
sentence was written to anticipate. So this is **a recorded prediction being
falsified on schedule**, not a numerical threshold being crossed — and the count
of three becomes supporting evidence rather than the reason.

That distinction is the lesson worth carrying, and ADR-062 records it:
**a predicted architectural trigger firing is stronger evidence than reaching a
rule-of-three count.** It complements what EPIC-010b learned from the other
direction — that three occurrences is a trigger to *evaluate*, and the
evaluation may correctly return *no*.

### D6 — amended in place, before a line of S004 was written

**The sketch, and the approved decision that followed it, were both wrong about
the thing being matched.** Amended rather than superseded because this file is
unmerged and nothing had yet relied on it.

> **Countries define one or more accepted long-term stability conditions. Packs
> record the condition under which shelf-life was established. Suitability is
> determined by exact condition overlap. Climatic zone terminology is treated as
> presentation vocabulary only and is not persisted, because the authoritative
> source publishes conditions rather than zone classifications.**

**Why the prediction changed, which is the part worth recording:** investigation
showed the planned abstraction — *"zone"* — **was not the regulatory fact the
authoritative source publishes.** WHO's *Stability conditions for WHO Member
States by Region* (update March 2021) gives, per country, the long-term testing
condition that country accepts. It gives no zone letter per country, and **ICH
withdrew Q1F**, which was where zone letters came from. The design therefore
changed to model the published fact instead of an inferred classification.

**India is what decided it.** WHO's table says **30 °C/70% RH** — which is
neither Zone IVA (30 °C/65% RH) nor Zone IVB (30 °C/75% RH). Storing
`Zone = IVB` would not have been storing WHO's data; it would have been storing
*our reading* of WHO's data, with nothing to check it against. The plan's own
prerequisite note — *"getting India wrong means telling someone their stability
data supports a market it does not"* — was right about the risk and wrong about
where it came from: the danger was not misremembering the zone, it was the zone
existing in the model at all.

**Both halves of the original still stand.** Multiple values per country is
right — seven of the eight seeded markets accept *either* 25 °C/60% RH *or*
30 °C/65% RH, which is WHO's own wording — and the match is still *any overlap*
rather than equality. **The overlap survived; the thing being overlapped did
not.**

**Zone terminology is not banned, it is unpersisted.** A regulatory user will
say *"Zone IVB"*, and that remains a perfectly good thing to say. If a screen
ever needs the word it is a display alias computed at the edge — never a column,
because RegOS would then be publishing a classification it did not read.

**One consequence for the external prerequisite:** the dependency on **ICH
Q1A(R2) is removed**. It was never the source of a country-to-zone mapping. The
source used for the seed is WHO's table, recorded as
[E39](../../evidence/EPIC-022/stability-conditions.md) — including its per-row
provenance, which is not uniform: **Australia's row is footnote 2** (collated at
the 13th ICDRA, 2008) where the other seven are regulator-confirmed.

> **The original wording, kept:** *"Multiple zones per country, and the match is
> 'any overlap' — Brazil, India and Australia all span two in stability
> guidance. A single value would be wrong for exactly the markets where the
> answer matters."* Kept for the reason the carve-out's original recommendation
> is kept: a corrected prediction is worth more than a tidy document, and this
> one was corrected by reading rather than by argument.

### D7 — does `Country` stop being flat master data?

Three owned collections put pressure on
[ADR-043](../../adr/ADR-043-strongly-typed-identity.md) §2, which keeps
`CountryId` a permanent record struct because Country has *"deterministic ids,
no children, no lifecycle"*.

**It stays flat.** The distinction that decides it:

| Changes aggregate semantics | Does not |
|---|---|
| children with **identity** and a **lifecycle** | **owned value collections** |

Languages, regions and stability conditions have no independent identity, no
lifecycle, are
replaced as a whole, and mean nothing apart from the country that holds them —
the same shape `PharmaceuticalProductDetail.RoutesOfAdministration` already has.
ADR-043's test is about identity semantics, not about column count.

**The falsifier, named now rather than argued later:** if **EPIC-012** gives
Country a lifecycle — active/inactive, merged, renamed — it becomes
`Entity<CountryId>` and the id conversion comes with it. Until something asks,
adding either would repeat exactly the defect this epic exists to remove.

### The sketch's original wording, kept



**1. Collections of `CodedConcept`, not enums or strings.** Region and climatic zone are governed external vocabularies, which is what `CodedConcept` is for — and `GeographyVocabulary` joins the seven vocabulary classes already in `ReferenceData/Terminology/`. *Lean: yes for regions and zones.*

**2. `LanguageCode` moves from `src/Product` to `src/ReferenceData`.** It lives in the Product context today and is used by `TradeName` and `LocalLabel`; Country would be the **third** consumer, and the third is what ADR-018 says to wait for. A language is a world fact, not a product fact. *Lean: move it — and this is the one thing here that might force an ADR, being a cross-context change.*

**3. `RegionCode` is dropped, not migrated.** It has never held a value in any environment, so there is nothing to preserve and no backfill to design. Say this in the migration so a future reader does not go looking for lost data.

**4. Required languages are advisory.** Canada legally needs EN and FR, but a label set mid-authoring must not be blocked — the same severity argument EPIC-002 settled: report it, let the user decide, block only on what a blueprint rule states. *Lean: an `Information`-severity finding on the market view, never a refusal.*

**5. Zone matching is derived on read, never stored.** The EPIC-005 expiry precedent exactly: store the facts (pack's shelf-life region, market's zone), derive the interpretation. Storing "supported: true" would rot the moment either side changed.

**6. Multiple zones per country is right, and it is why RIM says Multiple.** Large countries span zones — Brazil, India and Australia are all cited as spanning two in stability guidance. A single value would force a wrong answer for exactly the markets where it matters. *Lean: collection, and the match is "any overlap", not equality.*

### Change-case analysis

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| xEVMPD/IDMP output needs alpha-3 and official name (EPIC-007b) | **High** | Both are stored fields; the renderer reads them |
| The seed widens to 30+ markets | **High** | Five facts per row instead of two — the cost is in curation, not shape |
| A licensed ISO register is bought | Medium | Fields are the target; loading replaces the seed, no model change (the `Substance` shape) |
| Regional membership changes (a country joins or leaves a bloc) | Medium | A collection edited by a steward (EPIC-012). **No effective dating** — nothing asks *"was it in the EU in 2019?"* yet, and that is the trigger to add it |
| Country-scoped plan templates (EPIC-020) | Medium | RIM points Process Plan Template at Country; regions are what make one template serve a bloc |
| A market's required languages differ by product type | Low-Med | Would move the fact off Country onto a rule — the blueprint engine's job, not geography's |
| Subdivision-level regulation (US states, Canadian provinces) | Low | Out of scope, and RIM does not model it either |

---

## Phase 3 — Stories *(confirmed 2026-08-05)*

| # | Story | Slice |
|---|---|---|
| **S001** ✅ | **ISO identity** — `IsoAlpha3Code` + `IsoName` on all eight, surfaced wherever a country is shown; evidence entry | domain → persistence → API → UI → test |
| **S002** ✅ | **Regions** — collection in, `RegionCode` out; *"which of our markets are in the EU?"* | full slice |
| **S003** ✅ | **Languages** — collection + `LanguageCode` moved to ReferenceData (decision 2); **the market view shows required vs recorded label languages**, advisory | full slice |
| **S004** ✅ | **Stability conditions** — collection, **`ShelfLifeStorage.TestedAt`**, and the match, reported not blocked. **Widened**: it absorbs the field the [carve-out](#the-carve-out--closed-before-phase-2-began) did not place in 010b. **Amended before it was built** — conditions, not zones ([D6](#d6--amended-in-place-before-a-line-of-s004-was-written)) | full slice |
| **S005** ✅ | **Capstone** — one product, two markets, identical inputs: the pack-derived facts are asserted **identical** and every difference traced to the country. Evidence complete, seed verified against the database, [retro](#retrospective) | test → docs |

**ADR:** only if decision 2 (`LanguageCode` across contexts) is taken. Next free number is **ADR-062**.

**Where to stop if it runs long:** after S003. S001–S003 pay both named debts;
S004 was the only story carrying **both** an external prerequisite *and* a change
to a shipped aggregate, which made it the clearest thing to let slip. It was not
let slip: the prerequisite was fetched, and reading it **changed the design
before any code was written** rather than after.

> **S001 is deliberately the thinnest possible vertical slice** — two scalar
> fields, seeded on all eight, surfaced where a country is shown. It is not a
> "collections and vocabularies" foundation story: this project delivers
> vertical slices, and a horizontal groundwork story would have nothing to prove
> at the end of it.

**The external prerequisite was met, and it changed the design.** This section
said *"ICH Q1A(R2) is a prerequisite, not an enhancement"* and that **India being
IVB rather than IVA** decides whether RegOS tells a user their stability data
supports a market it does not.

**It was fetched before S004's seed was written, and India is neither.** WHO's
table gives India **30 °C/70% RH** — not Zone IVA (30/65), not Zone IVB (30/75).
The guideline that carried zone letters, **Q1F, has been withdrawn**. So the
prerequisite did its job in the strongest possible way: it falsified the
abstraction rather than filling it in
([D6](#d6--amended-in-place-before-a-line-of-s004-was-written),
[E39](../../evidence/EPIC-022/stability-conditions.md)).

> **The lesson worth carrying, and it is the founder's wording:** when an
> authoritative source disagrees with your abstraction, **change the abstraction,
> not the source.** The instinct that made this a blocking prerequisite was
> right; what it protected against turned out to be bigger than a wrong value.

---

## Retrospective

### Did the capstone demonstrate what the epic promised?

> *"A country stops being a label on a dropdown and starts being a set of facts
> other capabilities reason from."*

Yes, and **the proof is built as a controlled experiment rather than a tour.**
[`country-depth.spec.ts`](../../../tests/Browser/specs/country-depth.spec.ts)
records **one global product in two markets and performs identical actions in
both** — same pack, same size, same legal status, same shelf life, same storage
precaution, same testing condition, same single English label, same licence.
Nothing in the helper reads the country name or branches on it.

Then it asserts **what does not change**:

| Held constant | Asserted |
|---|---|
| the pack-derived summary line | byte-identical in Canada and India |
| the authorisation | identical — *"Authorised under 1 licence"* in both |

…so that every difference on either screen has exactly one place it can have
come from:

| Country-derived | Canada | India |
|---|---|---|
| ISO identity (S001) | `CAN` | `IND` |
| groupings (S002) | ICH · PIC/S | **none** |
| expected languages (S003) | one English label leaves **French missing** | one English label **covers it** |
| accepts (S004) | 25 °C/60% RH **or** 30 °C/65% RH | **30 °C/70% RH** |
| the pack tested at 25/60 | **accepted** | **not accepted** |

**The equality assertions are guarded against passing vacuously** — two empty
summaries are also equal — by naming the facts the compared line must carry.

**One precision the spec states rather than glosses.** ADR-039 makes the market
tier market-local: Canada's pack and India's pack are two rows, not one shared
record. They are identical because a person entered them identically, and that
is what makes the comparison honest — **nothing is shared except the global
product and the country data**.

### Definition of Done

| | |
|---|---|
| All eight carry alpha-3, ISO name, languages, regions, accepted stability conditions | ✅ **verified against the database, not the seed file** — eight rows, no nulls. India's *zero regions* is the recorded empty answer (E37), not a gap |
| `RegionCode` gone from model and schema | ✅ S002 — and the migration says why nobody should look for lost data |
| *"Which of our markets are in the EU?"* answerable | ✅ S002 — region filter on the portfolio, asserted with PIC/S too so it is reading membership rather than hiding rows |
| *"Does our stability data support this market?"* answerable | ✅ S004 — `GET /api/medicinal-products/{id}/authorised-packs` |
| Expected vs recorded label languages, advisory not blocking | ✅ S003 |
| An unaccepted condition **reported, not prevented** | ✅ S004 — the India spec asserts all three: advice appears, supply saves, pack authorises |
| An evidence entry per vocabulary, and the seed's hand-curation statement | ✅ E36–E39, each naming what is **not** held; `Countries.cs` carries the `Substance` statement |
| ADR only if the `LanguageCode` move is taken | ✅ ADR-062 |

**It closed no RIM object, as forecast.** One object went from 33% to ~92% and
[the runway](../BACKLOG.md#the-runway) figure does not move.

### The lessons worth carrying past this epic

Four, and each spans more than this epic — which is what separates them from
story notes.

#### 1. Store the authoritative fact, not an inferred relationship *(EPIC-010b)*

A pack's licence was going to be a foreign key. It became `PackAuthorisation`
carrying `AuthorisedOn`, because a licence granted in 2021 routinely gains a pack
in 2024 and **a foreign key cannot say when**.

#### 2. Remove an abstraction the authoritative source does not support *(EPIC-022)*

**The complement of the first, and it deletes rather than adds.** The plan held
`Country.ClimaticZones` for two phases. WHO publishes the long-term testing
*condition* each member state accepts and **no zone letter per country**; ICH
withdrew Q1F. `Zone = IVB` for India would have been RegOS's reading of WHO
rather than WHO — and **India is 30 °C/70% RH, which is neither IVA nor IVB**, so
nothing in the system could ever have caught it.

> The two are worth stating as a pair. The first says *the world holds a fact
> your structure cannot carry — go and get it.* The second says *your structure
> holds a category the world does not publish — take it out.*

#### 3. Architectural predictions are stronger than counting occurrences

ADR-018's rule of three is a trigger to **evaluate**, and this epic pairs with
010b on it from both directions:

| | Trigger | Outcome |
|---|---|---|
| **010b S004** | *structured fact + approved wording* reached three uses | **evaluated and refused** — counting alone would have abstracted |
| **022 S003** | `LanguageCode`'s own docstring named the condition: *"language **currently** drives display"* | **the prediction fired** — Country made language drive validation |

The count of three was true for `LanguageCode` as well, and it is the weaker
half of the argument. A recorded prediction names *what would change the
answer*; a count only notices that something happened three times.
**Write the falsifier down when the decision is made** — it is what makes the
later change evidence rather than opinion.

#### 4. Amend an unmerged decision; supersede only what reached production

D6 was **approved and then amended in place**, and the file keeps its original
wording beside the amendment. The progression:

> original prediction → investigate the authoritative source → discover the
> abstraction is wrong → **amend before merge** → record *why* it changed

A superseding ADR for a decision that never ran in production buys a paper trail
about a thing nobody used, at the cost of a reader having to hold two documents
to understand one model. **The immutability rule protects decisions others have
relied on** — and nothing had relied on D6. ADR-061 §3 was corrected the same way
in 010b, which makes this the second instance and the point at which it is a
practice rather than a one-off.

### What went wrong, and what it cost

| | Cost |
|---|---|
| **S001's scaffolded migration was wrong.** EF generated `NOT NULL DEFAULT ''`, which on an already-seeded database gives all eight the same empty alpha-3 and fails the unique index | Caught before it ran. Every migration in the epic then carried a **hand-written backfill**, because the seeder is insert-if-empty and an existing database gets nothing otherwise |
| **Neither Germany nor France had a seeded authority**, so no EU market could hold a registration and the epic's own headline question had no demonstrable answer | Found by S002's browser proof rather than by review. BfArM and ANSM seeded — the national agencies, not EMA, since an `Authority` hangs off a `CountryId` |
| **`HasIndex("CountryId", "Language")` failed** — EF wants the *property* name, not the column name | Minutes. Recorded in the configuration so the next owned collection with a renamed column does not repeat it |
| **The pack supply dialog had no height cap.** S004's fourth checkbox group pushed *Save supply* below the fold with no way to reach it | Found by the browser proof before a person hit it. Not a stability defect at all — a workflow that stopped completing after a legitimate expansion of a form |
| **Fetching corrected memory four times** — Australia and India are ICH *observers*; India is not a PIC/S participant; Canada's bilingual rule is not a rule; India's stability condition is 30 °C/70% RH | The whole reason the lists were fetched. Two of the four **changed a design**, not just a value |

### Something the epic found and did not fix

**Migration drift, now at three independent observations.**

| | What it showed |
|---|---|
| **S001** | the seeder is insert-if-empty, so a migration must carry its own backfill |
| **S002** | the dev database fell five migrations behind and turned 18 of 19 suites red |
| **S004** | the dev database fell one migration behind and **the suite stayed green** |

The third is the one that settles it. 27 test files hard-code `Database=regos`
and nothing migrates it, so **a stale schema only turns a test red when a
migration happens to touch a read path some test already exercises.** Green means
"nothing collided", not "the schema is current" — the suite does not test the
assumption it depends on.

**The requirement, stated once:** *the automated test environment should always
execute against a schema produced from the current migration chain.* Per-run
databases are the likely implementation, and the requirement is what matters.
**Raised on `main` after this epic merges**, because it is planning evolution
rather than feature work.

### Carry-forward

| | Where |
|---|---|
| Migration drift → per-run test schema | **`main`, after merge** — its own backlog item, not a bullet inside EPIC-015 |
| Browsing the vocabularies and governed lists (nine vocabularies, ~18 lists, no SPA route reaches any of them) | **EPIC-012**, moved there during this epic with the founder's mockup |
| Effective-dated regional membership (*"was the UK in the EU in 2019?"*) | Unasked. The trigger is somebody asking |
| A country lifecycle → `Entity<CountryId>` | **EPIC-012.** The falsifier for ADR-043 §2 is named in `Country`'s own docstring |
| Seven of E38's eight rows are official language rather than read labelling law | Recorded in the entry. Safe only because the feature is advisory |
| Australia's stability row is WHO footnote 2 (ICDRA 2008), not a regulator statement | Recorded in E39. The first row to re-check |
