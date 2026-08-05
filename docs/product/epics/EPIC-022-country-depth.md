# EPIC-022 — Country depth

**Status:** ⚪ Not Started · **Branch:** `epic/EPIC-022-country-depth` (cut at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

`Country` is the oldest table in RegOS and **the only reference entity whose every attribute is for display**. `Code` and `Name` are what you show in a dropdown. The three RIM attributes it is missing — climatic zone, languages, regions — are what you **decide** by. This closes that, plus the two ISO identity fields machine-readable output needs.

> **Phase 1 below is settled.** **Phases 2–3 are a sketch** — enough to resume cold, explicitly **not approved design**. Confirm, amend or replace them in the Phase-2 conversation on pull-in.

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

### What each one decides

| RIM attribute | RIM shape | Decides | Consumer |
|---|---|---|---|
| **Climatic Zone** | Controlled Vocab · Multiple · Opt | which stability data supports which market — ICH Q1A(R2) zones **I, II, III, IVA, IVB**. India is IVB; Japan is II; Canada is I | `ShelfLifeStorage` (EPIC-010b) |
| **Languages** | Controlled Vocab · Multiple · **Req** | which languages a market's labelling needs — Canada EN+FR, Belgium NL+FR, Switzerland DE+FR+IT | `LocalLabel.Language` (EPIC-018 ✅) |
| **Regions** | Controlled List · Multiple · Opt | which procedure and blueprint apply — EU, ICH, ASEAN, GCC, PIC/S, and they **overlap** | EPIC-020 country-scoped templates · EPIC-009 |
| **ISO 3-Char Code** | Controlled Vocab · Single · **Req** | machine-readable output | EPIC-007b (xEVMPD/IDMP) |
| **ISO Country Name** | Controlled Vocab · Single · **Req** | the official name those outputs require — *"Korea, Republic of"*, not *"South Korea"* | EPIC-007b |

### Two debts this pays

**1. EPIC-018 shipped a gap it could not close itself.** `LocalLabel.Language` exists; nothing can say which languages a market *requires*, so a user cannot be told their Canadian label set is incomplete. That is Country's omission, not Labeling's.

**2. `RegionCode` is a dead column.** `Country.Create` defaults it to `null`, **all eight seeds omit it**, there is no mutator, and `Country` has no update path — it can only ever be null. This is precisely the defect [`Substance`](../../../src/ReferenceData/RegOS.ReferenceData.Domain/Substances/Substance.cs) refuses by name: *"a persistent property with no acquisition path is the defect EPIC-007a spent three findings on."* Country predates that rule. RIM says Regions is **Multiple** anyway, so a single nullable string could not hold it even if something wrote to it.

### In scope ✅
- **`ClimaticZones`** — collection, ICH Q1A(R2) vocabulary.
- **`Languages`** — collection, ISO 639.
- **`Regions`** — collection, replacing `RegionCode`, which is **removed** rather than migrated (it has never held a value).
- **`IsoAlpha3Code`** and **`IsoName`** — the two identity fields.
- **`ShelfLifeStorage.Region` and the match** — *unless EPIC-010b S003 takes it first; see [the carve-out](#the-carve-out-worth-taking-early)*.
- **The market view answers the label-language question** — required languages vs recorded local labels, advisory not blocking.
- **An evidence entry per vocabulary** — see [the sourcing question](#the-sourcing-question-settle-this-first).
- Browser proof, retro.

### Out of scope ⏸️ (deferred, with reason)
| Deferred | Why |
|---|---|
| **`Country → Process Plan Template`** (RIM attr #12) | → **EPIC-020**, which owns the object at the other end. Nothing to point at until it exists. |
| **Widening the seed beyond the eight countries** | This epic deepens what is there. Adding a ninth market is a seed change any epic can make — but note the shape now costs five facts per country instead of two, which is a reason to widen deliberately rather than casually. |
| **Steward CRUD over country data** | → **EPIC-012**, which owns the reference-data write side. This is depth of the *seed*, not a new authoring surface. |
| **ISO 3166-2 subdivisions** (states, provinces) | Not in RIM. `PostalAddress.StateProvince` is free text and nothing reasons about it. Add when something does. |
| **Currency, timezone, calendar** | Not in RIM's Country. Regulatory fees carry their own unit on Application. |
| **Deriving required languages into a blocking validation** | Advisory only — see Phase-2 decision 4. Blocking belongs with a rule the blueprint states, not with geography. |
| **A country lifecycle** (`IsActive`, merged/renamed states) | RIM has none, nothing asks, and adding a flag nothing writes would repeat exactly the defect this epic exists to remove. |

### Definition of Done
- Each seeded country carries its ISO alpha-3 code, its ISO official name, its languages, its regions and its climatic zone(s) — **all eight, no nulls standing in for "we didn't get to it"**.
- `RegionCode` is gone from the model and the schema.
- *"Which of our markets are in the EU?"* and *"which markets are climatic zone IVB?"* are answerable through the API.
- A market's page shows the languages that market requires beside the local labels actually recorded, and says which are missing — **advisory, not blocking**.
- A shelf life states the region it was generated for, and a pack authorised in a market whose zone that region does not cover is **reported, not prevented** (the EPIC-005 expiry precedent: derive the interpretation, never block on it).
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
| ICH climatic zones | **ICH Q1A(R2)** + WHO TRS 953 Annex 2 | **Not held** — a real document someone must fetch |
| ISO 639 language codes | ISO | Widely published |
| Regions (EU / ICH / PIC/S / ASEAN / GCC) | **No single authority** — each body publishes its own membership | Multiple sources, each small |

**The honest position, and the one that unblocks the epic:** for **eight countries** every value is hand-verifiable against public sources, and eight rows is not a register. So seed by hand, and say so in the file the way `Substance` does —

> *Demonstration seed data only. These records intentionally do not represent an authoritative geography, terminology or membership register.*

— then record each source in `docs/evidence/EPIC-022/`. That keeps the distinction the `file-tag` correction was written to protect: **a hand-curated eight-row seed and an authoritative register are different evidence levels**, and only one of them can be widened without going and fetching something.

**Fetch ICH Q1A(R2) before S004.** The zone boundaries are the one value here that a careful person cannot reconstruct from memory, and getting India wrong means telling someone their stability data supports a market it does not.

---

## The carve-out worth taking early

**Climatic zone is cheapest this week and gets more expensive after.**

EPIC-010b is writing [`ShelfLifeStorage`](../../../src/Product/RegOS.Product.Domain/Product/ShelfLifeStorage.cs) **right now**, and as it stands it carries a period and storage conditions and **no region at all** — RIM's `Shelf Life Region` (Controlled Vocabulary, Single, **Required**) is absent. Without it, shelf life is not a regional fact, which is the only reason shelf life is interesting: data generated for Zone II does not support a Zone IVB market.

Two options, and 10b's Phase-2 owner should pick:

| | |
|---|---|
| **10b S003 adds `Region` now** | One field on a type being written today. This epic then only supplies `Country.ClimaticZone` and the match. **Recommended.** |
| **This epic adds both later** | A migration on a shipped table plus a backfill nobody has the data for, since the region was never captured at entry |

Either way the *match* — does this pack's shelf-life region cover the zone of the market it is authorised in? — belongs here, because it needs the country half.

---

## Phase 2 — Domain design *(sketch — not approved)*

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
└── ClimaticZones  collection<CodedConcept>       NEW
```

### Decisions to settle (Phase 2, on pull-in)

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

## Phase 3 — Candidate stories *(sketch — re-slice on pull-in)*

| # | Story | Slice |
|---|---|---|
| **S001** | **ISO identity** — `IsoAlpha3Code` + `IsoName` on all eight, surfaced wherever a country is shown; evidence entry | domain → persistence → API → UI → test |
| **S002** | **Regions** — collection in, `RegionCode` out; *"which of our markets are in the EU?"* | full slice |
| **S003** | **Languages** — collection + `LanguageCode` moved to ReferenceData (decision 2); **the market view shows required vs recorded label languages**, advisory | full slice |
| **S004** | **Climatic zones** — collection + the shelf-life region match, reported not blocked *(needs ICH Q1A(R2) in hand; scope depends on the [carve-out](#the-carve-out-worth-taking-early))* | full slice |
| **S005** | **Capstone** — browser proof of the two questions, evidence entries complete, seed statement in place, retro | UI → test → docs |

**ADR:** only if decision 2 (`LanguageCode` across contexts) is taken. Next free number is **ADR-062**.

**Where to stop if it runs long:** after S003. S001–S003 pay both named debts; S004 is the one with an external prerequisite and is the natural thing to let slip — decided now rather than under pressure.
