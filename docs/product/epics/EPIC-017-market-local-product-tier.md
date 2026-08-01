# EPIC-017 — The market-local product tier

**Status:** 🟡 In Progress · **Branch:** `epic/EPIC-017-market-local-product-tier` (cut at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

> **Vocabulary.** The aggregate is `MedicinalProduct`; the UI calls it a
> **Market**. RIM's word keeps the model precise, the screen uses the word a
> regulatory user says out loud. The pair is recorded in
> [docs/domain-model/product.md](../../domain-model/product.md) and the rule in
> [CLAUDE.md](../../../CLAUDE.md).

The structural unlock. RegOS's `Product` is a **global** identity; the regulatory world is **market-local**. This inserts the missing tier and hangs the two facts users ask for first — **what it's called there**, and **whether it's actually on sale**.

> **Phases 1–2 are settled.** Phase 2 was reviewed against the code and approved on 2026-07-31 with **three amendments** — see *Phase 2 rulings* below. Phase 3 is the approved five-story slice.

---

## Phase 1 — Epic plan

### Outcome
EPIC-005's portfolio views stop answering *"what do we hold in Canada?"* with a global product code and start answering it the way a regulatory user asks it: **"Product X, sold there as *Brandname*, launched 2021, licence 12345, expires 2027."** Trade name and market status become first-class, market-scoped facts.

### The finding this epic responds to

Compared against the DIA RIM object model, the usual framing — *"RegOS collapsed the product hierarchy"* — is imprecise. Look at where RIM puts market-locality:

| RIM tier | Carries | RegOS |
|---|---|---|
| **Product Family** | molecule/compound family, therapeutic area, IBD dates, sponsor | — |
| **Global Product** | global identity, type, dosage forms, global label | **`Product`** ← we are here |
| **Medicinal Product** | **one trade name, one licence, market status, ATC code, strength, local label** | **missing** |

`Product` has no country and is referenced globally by documents and applications — it sits at **Global Product**. RegOS expresses market-locality on the *transactional* records instead (`CountryId` on `RegulatoryApplication` and `Registration`). So the tier genuinely absent is **Medicinal Product**, and it is where Trade Name, Market Status, Local Label, ATC code and strength all want to live. **About 18 further RIM objects hang below it.**

### The alternative we rejected — and why *(write this down; it will be asked again)*

*"`Registration` already carries (ProductId, CountryId, AuthorityId, HolderOrganizationId) — which is nearly RIM's Medicinal Product identity. Why not just put trade name and market status on `Registration`?"*

Because **a licence and a market presence are different facts with different lifecycles**:

- A market-local product can hold **several licences over time** (renewal, replacement, transfer) and, per EPIC-005 decision 4, **several at once** (strengths, presentations, partial divestment). Trade name would then be duplicated across them and could silently disagree.
- **Market status ≠ licence status.** A product can be `Approved` and *not launched*; `Approved` and *withdrawn from sale* commercially; `Suspended` while stock remains in the channel. RIM separates them deliberately, and so must we.
- A trade name can be **registered before any licence exists** and survives the licence it was granted under.

Collapsing them would be cheap now and wrong at the first renewal.

### In scope ✅
- **`MedicinalProduct`** — the market-local tier: global product + country, its type, ATC code, investigational flag, holder.
- **The reference re-pointing decision** — which tier every existing `ProductId` means, applied and migrated (see Phase 2; this is the real work of the epic).
- **Trade Name** — per country + language, on the medicinal product.
- **Market Status** — status, dated start/end, launch date, risk-of-supply flag + comment; with the dated history pattern.
- **Portfolio views enriched** — registration lists show trade name and market status alongside the licence.
- Browser proof, ADR.

### Out of scope ⏸️ (deferred, with reason)
| Deferred | Why |
|---|---|
| **`Product Family`** (the tier *above* Global Product) | Nothing yet asks a family-level question. Crucially, inserting a tier **above** an existing root is cheap — the existing root gains one nullable FK — whereas inserting one **between** existing objects (what this epic does) is not. Deferring costs almost nothing; deferring *this* tier would keep costing more. |
| **Strength, dosage form, ingredients, ATC beyond a code field** | → **EPIC-010**. This epic establishes the tier; EPIC-010 fills it. |
| **Packaged Product / Packaging / presentations** | → **EPIC-010**. |
| **Local Label / Labeling** | → **EPIC-018**, which depends on this tier existing. |
| **Splitting existing `RegulatoryApplication.ProductId`** | See Phase 2 decision 2 — the recommendation is that applications stay at the **global** tier. If that is overturned, it becomes a story here. |
| **Migrating historical data across tiers** | There is no production data. The migration creates one medicinal product per existing (product, country) pair found on registrations — deterministic, no inference. |

### Definition of Done
- A `MedicinalProduct` exists for a (global product, country) pair, and a `Registration` is created **against it** rather than against the global product.
- Every existing `ProductId` reference in the codebase has an explicit, documented tier — recorded in the epic and asserted by tests where it matters.
- A medicinal product carries **one trade name per language** for its country, and a **dated market-status history** (the `RegistrationStatusEntry` pattern: `OccurredOn` vs `RecordedOnUtc`).
- *"What do we hold in Canada?"* returns trade name + market status + licence + expiry in one read.
- Existing EPIC-005 registration tests still pass, re-pointed rather than rewritten.
- Migration verified on a fresh database **and** on a seeded one.
- ADR written for the tier model and the re-pointing.

---

## Phase 2 — Domain design

### The re-pointing table — **the core artifact of this epic**

| Existing reference | Recommended tier | Reasoning |
|---|---|---|
| `Registration.ProductId` | **Medicinal Product** | RIM: License → Medicinal Product (Parent). A licence is granted for a specific market-local product. **This is the one that must change.** |
| `RegulatoryApplication.ProductId` | **Global Product** (keep) | RIM lets an Application span a family (`Product Family`, Multiple, Inherited) while Medicinal Product lists Applications as children — i.e. RIM models it *both* ways. An application is filed *to reach* a market authorisation; the local product is often what the application **creates**. Keeping it global avoids a chicken-and-egg on creation. **The genuinely contested call — settle it explicitly.** |
| `ProductDocument.ProductId` | **Global Product** (keep) | A CMC or nonclinical document is about the product, not one market's presentation. |
| `Submission` (via `ApplicationId`) | follows Application | No direct `ProductId`; inherits whatever (2) decides. |
| `ProductDirectoryRow` | **Global Product** (keep) | The product master list is a global list. |
| `SubmissionSnapshot` | follows Submission | |

### Entities

**`MedicinalProduct`** — aggregate root. New context `src/MedicinalProduct/`, **or** a second aggregate in `src/Product/` (decision 3).

| Field | Type | Notes |
|---|---|---|
| `Id` | `MedicinalProductId` | |
| `TenantId` | | fail-closed filter |
| `ProductId` | `ProductId` | the global product it localises, `Restrict` FK |
| `CountryId` | `CountryId` | what makes it market-local |
| `Name` | string | RIM "Medicinal Product Name" — may differ from the global name |
| `AtcCode?` | string | RIM: Multiple; single to start, note the seam |
| `IsInvestigational` | bool | RIM: required |
| `MarketingAuthorizationHolderId?` | `OrganizationId?` | |
| `Status` | enum | product-record lifecycle, **not** market status |

**`TradeName`** — child entity of `MedicinalProduct`. `Id`, `Name`, `Language`. RIM also carries `Country` on Trade Name; here it is **inherited from the parent** — one of the reasons the tier is worth having.

**`MarketStatus`** — child entity of `MedicinalProduct`, **append-only history** plus a stored current value.

| Field | Notes |
|---|---|
| `Status` | RIM controlled list — e.g. NotLaunched, Launched, TemporarilyUnavailable, Withdrawn, Discontinued |
| `OccurredOn` / `RecordedOnUtc` | the EPIC-005 bitemporal pattern — reuse it verbatim |
| `LaunchDate?` | RIM: conditional |
| `RiskOfSupply`, `RiskOfSupplyComment?` | RIM: bool + conditional text |
| `Note?` | |

### Decisions (the sketch's six, as originally written)

**1. Naming.** RegOS already has `Product`. Options: (a) `Product` stays, add `MedicinalProduct` below; (b) rename `Product` → `GlobalProduct` and add `MedicinalProduct`. *Lean (b)* — RIM's vocabulary is the ubiquitous language, and "Product" next to "MedicinalProduct" will confuse every future reader about which is which. (b) is a mechanical rename plus a migration, cheapest now and never cheaper.

**2. Which tier `RegulatoryApplication` points at.** See the re-pointing table. *Lean: stays global.* This is the decision most likely to be revisited — record the reasoning either way.

**3. New bounded context or second aggregate in `src/Product/`?** *Lean: second aggregate in `src/Product/`*, following the Platform multi-aggregate precedent. The global and local tiers are one ubiquitous-language cluster, always queried together, and a context boundary between them would put a cross-context dependency on the hottest path in the system.

**4. Market status is a dated history, not a field.** Reuse `RegistrationStatusEntry` shape verbatim — append-only, `OccurredOn` vs `RecordedOnUtc`, stored current value for indexed reads. RIM marks Market Status "Single / Historical"; this is where the cross-cutting history rule gets applied first.

**5. Trade name uniqueness.** One per (medicinal product, language)? Or many? *Lean: one per language, enforced* — a market-local product with two simultaneous brand names in one language is a data error, unlike EPIC-005's deliberate multi-registration case. Assert the constraint with a test either way, per the EPIC-005 precedent.

**6. Creating a registration must be able to create its medicinal product.** Otherwise every registration flow gains a mandatory two-step. *Lean: the create-registration handler resolves-or-creates the (product, country) medicinal product.* Keeps the user's one action one action.

### Phase 2 rulings (approved 2026-07-31) — three amendments to the sketch

The six decisions above were read against the code before approval. Four stand
as written (2, 3, 4, 5). Two changed, and one measurement corrected a claim.

**Amendment A — the rename is real work, and it is its own story.**

The sketch called decision 1 *"a mechanical rename plus a migration, cheapest now
and never cheaper"*. Measured before approving:

| | Sites |
|---|---|
| `ProductId` in `src/` (excl. migrations) | **114** — only 16 inside the Product context |
| `productId` in the frontend | **142** |
| `productId` in browser specs | **30** |
| bare `Product` type in `src/` | **165** |

"Never cheaper" is true — every later epic compounds it. "Mechanical" is true.
**"Small" was wrong**, and the sketch should not have implied it.

Two things follow:

1. **A bounded context is not an aggregate.** `RegOS.Product` is the correct
   *context* name for a cluster holding both tiers, exactly as `RegOS.Organization`
   already holds four aggregates. So the rename touches **`Product` →
   `GlobalProduct` and `ProductId` → `GlobalProductId`, and nothing else** — not
   the projects, not the namespaces, not `ProductCode`/`ProductName`/`ProductType`.
2. **It ships as S000, its own commit, before the tier exists.** A
   semantics-preserving diff is reviewable precisely because it is mechanical;
   tangling it with the `Registration` re-pointing is what would make it
   dangerous. A reviewer can verify "no behaviour changed" without filtering
   domain logic out of the diff.

**Amendment B — decision 6 is rejected. The medicinal product is explicit.**

The sketch contradicted itself. These cannot both hold:

> *Change-case analysis:* several medicinal products may exist per
> (global product, country) — **no uniqueness constraint on the pair**.

> *Decision 6:* the create-registration handler **resolves-or-creates** the
> (product, country) medicinal product.

Resolution needs the lookup to return exactly one. Without a uniqueness
constraint the handler would be **choosing a business object on behalf of the
user**, non-deterministically.

The non-uniqueness is kept — it is the same reasoning approved in EPIC-005
decision 4 (strengths, presentations, partial divestment). So
`CreateRegistrationCommand` takes a **`MedicinalProductId`**. Pick-or-create
lives in the UI, so the user's one action stays one action; the write model does
not guess.

The domain argument is the decisive one, and it is about **dependency
direction**:

- A medicinal product means *"we market, or intend to market, this product in
  this jurisdiction."*
- A registration means *"this authority granted us authorisation."*

Those are different business events. A medicinal product can exist with **zero
registrations for years** — dossier preparation, labelling, artwork, pricing and
launch planning all precede authorisation. **The registration depends on the
medicinal product, not the reverse**, so creating one as a side effect of filing
the other is the wrong direction.

**Amendment C — ADR-039 records what `Registration` is, deliberately.**

The tier chain this epic builds is `GlobalProduct → MedicinalProduct →
Registration`. As the model grows, `Registration` will start to resemble the
*marketing authorisation itself* — the root that variations, renewals, sequences
and authority interactions hang from.

Nothing in this epic changes because of that; acting now would be premature.
But **ADR-039 states that `Registration` is intentionally the authorisation root
for now**, and that a separate `MarketingAuthorization` aggregate is considered
unnecessary unless the domain reveals a clear distinction. A future contributor
should read that as a deliberate simplification, not an oversight.

**Rule-of-three note on decision 4.** `MarketStatus` is the **second**
append-only status history, after `RegistrationStatusEntry`. That is not the
extraction trigger. EPIC-006 brings authority-question, commitment, inspection
and meeting statuses; **that** is where the shared pattern earns extraction.

### Change-case analysis

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| Strengths, dosage forms, presentations (EPIC-010) | **High** | Children of `MedicinalProduct` — the tier is precisely what makes them placeable |
| Local labels (EPIC-018) | **High** | Hangs off this tier; blocked without it |
| `Product Family` tier added above | Medium | Inserting **above** a root is one nullable FK — deliberately why it is deferred |
| Several medicinal products per (product, country) | Medium | No uniqueness constraint on the pair — same reasoning as EPIC-005 decision 4 |
| ATC codes become multiple | Medium | Single column now; owned collection later, no data loss |
| A product is marketed under different names in one country over time | Medium | Trade names are child rows; add effective dating if it becomes real |
| MAH transfer | Medium | Holder is a field, not an ownership edge (EPIC-005 decision 5) |

---

## Phase 3 — Stories

Approved 2026-07-31. Five stories — the sketch's four, with the rename lifted
out of S001 into a zeroth so that no story mixes a mechanical diff with a
semantic one.

| # | Story | Slice | One conceptual purpose |
|---|---|---|---|
| **S000** | **The rename** — `Product` → `GlobalProduct`, `ProductId` → `GlobalProductId`, across backend, frontend and specs. Projects, namespaces and sibling types unchanged | mechanical | *nothing behaves differently* |
| **S001** | **The tier** — `MedicinalProduct` aggregate, re-point `Registration` to it, migration, EPIC-005 tests re-pointed rather than rewritten | domain → persistence → API → test | *the tier exists and a licence is granted against it* |
| **S002** | **Trade Name** — one per (medicinal product, language), enforced; surfaced wherever a registration is shown | full slice | *what it is called there* |
| **S003** | **Market Status** — dated history + current value; launch date **derived**, risk of supply deferred | full slice | *whether it is actually on sale* |
| **S003a** | **Market deactivation** — `Activate`/`Deactivate`, completing the activation lifecycle S001 introduced. Operability, not market state | small slice | *this record is no longer in use* |
| **S004** | **Capstone** — portfolio views enriched, browser proof, ADR-039, retro | UI → test → docs | *"what do we hold in Canada?" answered properly* |

**ADR to write:** *The market-local product tier, and which tier each reference means* — next free number (expected **ADR-039**).

---

### S001 — The tier *(shipped)*

`MedicinalProduct` exists, `Registration` is granted over it, and every existing
registration was migrated onto the market it was already in.

**The design decision that shaped everything else.** The sketch had
`Registration` keep `CountryId`. It doesn't. Once a registration names a
medicinal product, the country is *the medicinal product's fact*, and a second
copy on the registration is duplicated domain state with no transaction able to
keep the two in agreement.

The same argument then applies to `GlobalProductId`, and it was followed:
**`Registration` carries neither the product nor the country — only
`MedicinalProductId`.** The dependency chain is `GlobalProduct →
MedicinalProduct → Registration`, each step narrowing context, and "Canadian
medicinal product, Australian registration" is now *unrepresentable* rather than
merely forbidden.

Read models are unchanged in shape — `countryId` and `countryName` still reach
every caller — because they now join through the tier.

**The aggregate, deliberately almost empty.**

| Field | Note |
|---|---|
| `Id`, `TenantId` | fail-closed filter, shape 1 (ADR-031/038) |
| `GlobalProductId`, `CountryId` | both immutable; `Restrict` FKs |
| `Status`, `StatusDate` | an **activation flag** — one date, no history |

Everything else arrives with the feature that reads it. `Name` was cut from the
approved shape on the founder's own test — *who reads this?* — and joins
`AtcCode`, `IsInvestigational` and `MarketingAuthorizationHolderId` in the
deferred column. **`Status` carries no transition methods yet**: adding
`Activate`/`Deactivate` with no endpoint would be the unreachable-capability
defect EPIC-016 shipped twice. The field exists because retrofitting a lifecycle
column later means backfilling a guess; the capability arrives in S003 with the
market lifecycle that makes it reachable.

**No MAH on the tier.** EPIC-005 decision 4 already established that partial
divestment puts different holders on two registrations in one market. A holder
on the tier would be wrong the first time that happens; a *default* holder is a
different concept and can be added when a flow wants one.

**No uniqueness on (global product, country)** — asserted by a test, the way
EPIC-005 asserted its own missing index. This is what makes resolve-or-create
impossible rather than merely unwise (Amendment B).

**The migration.** Additive. One medicinal product per distinct
`(tenant, global product, country)` already on a registration; status date taken
from the *earliest business date in that registration's own history*, never the
clock. Every row is derived from data that was already there.

The scaffolded migration would have **renamed** `GlobalProductId` to
`MedicinalProductId` and dropped `CountryId` — structurally valid and
semantically catastrophic. Rewritten by hand as create → backfill → tighten →
drop, in one SQL statement so `RETURNING` carries generated ids straight into
the `UPDATE`.

Verified three ways: on a **fresh** database; on a **clone of the dev database**
seeded with an extra registration sharing a market, where 6 registrations
collapsed to 5 markets with **0 orphans and 6/6 keeping their exact
(product, country)**; and by rolling `Down` and confirming all 6 came back
byte-identical.

**Reachability.** `POST`/`GET /api/products/{id}/medicinal-products` and
`POST /api/medicinal-products/{id}/registrations` ship with the tier, and the
product page gained a **Markets** section — a market is added as its own act,
and a registration is recorded against a market row. That is Amendment B's
"pick-or-create lives in the UI", built where it was always going to live.

**Two exemptions retired while in the neighbourhood.**

- **SC-002's grandfathered list is now empty.** `IProductRepository` moved to the
  Domain project; leaving it in `Application/Persistence/` while adding
  `IMedicinalProductRepository` to `Domain/` would have put two sibling
  interfaces in two projects.
- **`detailOf` is now shared.** The organizations copy had written its own
  trigger — *"across slices it is the second occurrence, and a shared
  `src/shared/api` helper waits for the third"*. Medicinal products is the
  third, so both copies collapsed into `shared/api/problemDetail.ts`. ADR-018
  firing as designed rather than on a symmetry argument.

**Also removed:** `IRegistrationRepository.ListByProductAsync`, which had no
callers and read aggregates for a query in defiance of ADR-016. Deleted with the
re-pointing that broke it rather than kept alive by it.

**Verification:** 863 backend tests (+7), 61 browser specs (+1), migration
verified three ways, CORS widening reverted with an empty `Program.cs` diff.

**Carried out of S001 — a finding, not a fix.** The EPIC-016 mutation
defect (`await mutateAsync` with no `catch`, so a server refusal renders
*and* escapes to the window as an unhandled rejection) is live in **nine more
forms** across users, products, applications, submissions and documents. EPIC-016
fixed six and the house rule was written, but the sweep was never run outside
that epic's slices. Fixing them is a coherent piece of work and belongs to
whoever schedules it, not to this story. **Approved as a small maintenance
epic after EPIC-017 lands** — the pattern, browser proof and rationale already
exist, so what remains is mechanical and is worth keeping away from domain work.

---

### S002 — Trade Name *(shipped)*

*What it is called there.* `MedicinalProduct` gains a `TradeNames` child
collection, one name per language, and nothing else moves.

**A child entity, not a root.** Against ADR-038's three justifications: nobody
quotes its id, it has no lifecycle, no query reaches it directly, and no
aggregate references it. *"What is this called in Canada?"* enters through the
market and stops there — so it carries no `TenantId` and needs no query filter.
The country is **inherited from the parent** rather than repeated, which is one
of the things the tier was worth having for.

**`LanguageCode` is a value object, and there is no `Language` table.** The
deciding test was ADR-038's own: *does a rule branch on it?* No. Language
participates in **identity** — `(market, language)` — but never in **behaviour**.
Countries drive validation, authority selection and market identity; language
currently drives display, and those are not equivalent. Governed reference data
exists because the domain needs governed facts, not because dropdowns need
labels — the picker's readable names come from `Intl.DisplayNames` over a
curated code list in `constants/` (SC-105).

> **ADR-039 will record:** `LanguageCode` intentionally models the minimum
> demonstrated requirement (ISO 639-1 language). If future domain rules
> distinguish regional variants — for example `en-CA` vs `en-US` — this value
> object may evolve into a locale **without changing aggregate semantics**. A
> reference-data aggregate is deferred until the domain requires governed
> language metadata rather than validated identifiers.
>
> The principle underneath, which this epic has followed throughout: **model the
> business concept, not the standard.** ISO 639-1 is implemented because the
> domain currently needs a language, not because ISO publishes a list.

The value object owns parsing — `Parse`, `TryParse`, `FromIso639_1`, value
equality — so **no caller anywhere handles a raw language string.** That is
precisely what makes the locale evolution above a change to one file.

**Add and Remove, no Rename.** Without effective dating a rename is
indistinguishable from remove-then-add, and offering one would imply a
historical identity the model does not keep. When regulators care that *Brand A
became Brand B*, that arrives as dating or status history and renaming becomes a
distinct act worth naming. Removing is therefore also how a name is corrected,
and a test proves removing frees the language again.

**Uniqueness on `(market, language)` — the deliberate opposite of the tier's own
rule**, enforced in the aggregate *and* as a unique index. Two market presences
in one country are two business objects a company may legitimately hold; two
English names for one market presence are two labels for one thing, so one of
them is wrong. Different concepts, different invariants — stated in both the
aggregate and the EF configuration, because a reader will otherwise ask why the
two rules disagree.

The test that matters is **`TheOneNamePerLanguageRuleSurvivesAReload`**. Proving
the aggregate rejects a duplicate in memory is easy; proving it still rejects
one through a fresh context validates the `Include`, the repository and the
handler as one slice. `IMedicinalProductRepository.GetByIdAsync` always loads
trade names, with the reason written into the interface.

**One trigger that did not fire.** EPIC-017 was the *named* milestone for
extracting scheme-plus-value from `SiteIdentifier` + `OrganizationIdentifier`.
`TradeName` is `(language, name)` with a different rule attached — not that
shape. **Both breadcrumbs stay standing and the third occurrence is still
ahead.** The Rule of Three fires when an abstraction emerges, not when two
classes happen to hold two properties.

**A wording fix the browser caught.** The dialog was titled *"Trade name in
Canada"* above a field labelled *"Trade name"* — a stutter, and indistinguishable
from its own field to anything addressing the page by accessible name. Retitled
*"Name in Canada"*. The second time a Playwright strict-mode violation has
surfaced a genuine redundancy rather than a test problem (after EPIC-016's
"Identifier" → "Identifier Value").

**Deferred to S004 as agreed:** portfolio-view enrichment. S002 answers exactly
one question — *can I manage trade names for a market?* — and stays almost
entirely inside one aggregate.

**Verification:** 886 backend tests (+23), 62 browser specs (+1), CORS widening
reverted with no `Program.cs` diff.

---

### S003 — Market Status *(shipped)*

*Whether it is actually on sale.* `MedicinalProduct` gains a stored
`CurrentMarketStatus` and the append-only history behind it — the second
bitemporal status history in the model, and the one that sets up EPIC-006's
extraction.

**`LaunchDate` is not a field.** The sketch had it stored. It is the
`OccurredOn` of the first entry reaching `Launched` — a second copy of a fact
the history already holds, which is the S001 argument one tier down. Deriving it
also dissolves the question rather than answering it: *"why does the launch date
precede approval in migrated data?"* cannot arise, because nobody types it. It
is **first commercial availability**, not authorisation effectiveness — that
already exists as `Registration.ApprovedOn`, one aggregate over. First launch
rather than most recent, because a relaunch is a different question (ADR-037).

**Exact parity with `RegistrationStatusEntry`** — field for field, table for
table, configuration for configuration. Kept identical on purpose: the more
alike they are, the more mechanical EPIC-006's extraction becomes.

**And a deliberate divergence, which is the sharpest thing this story
establishes.** There is **no transition table**. `RegistrationLifecycle` exists
because a regulator's decision graph is genuinely constrained; commercial
reality is not, and a product may be launched, become unavailable, return, and
be discontinued and relaunched years later without a single incoherent step.
Encoding one company's commercial history as universal law is exactly what that
lifecycle's own governing principle forbids.

> **For the retro and ADR-039:** the bitemporal append-only *shape* generalises;
> the constraint *graph* does not. When EPIC-006 extracts the pattern, that is
> the line it should cut along.

Two coherence rules survive, because they are not about process: a status cannot
be re-entered from itself, and business time only moves forward.

**`Planned`, not `NotLaunched`** — and the reason is structural, not stylistic.
The word carries **one meaning at both tiers** ("intended, not yet actual"),
which is why reusing it is fine where `Withdrawn` would not have been: that
would have meant *surrendered licence* at one tier and *no longer sold* at the
other, and the portfolio views show both at once. **The rule is not "never reuse
a word across tiers" — it is "never let one word carry two meanings."**

`Planned` is also **non-reentrant by its own semantics**: you cannot plan to
enter a market you have already entered. So the one genuinely incoherent
transition is enforced with a rule and a test, where `NotLaunched` — which reads
as a reversible observation — would have needed a warning in prose instead.

**Operability and commercial state cannot blur**, enforced by naming rather than
discipline:

| | Question | Shape |
|---|---|---|
| `Status` + `StatusDate` | do we use this record? | activation flag, one date |
| `CurrentMarketStatus` + history | is it on sale? | append-only, bitemporal |

A test asserts that discontinuing a market leaves `Status` untouched.

**Migration.** `CurrentMarketStatus` defaults to `Planned`, correct for every
existing row — but a current status with no history behind it would break the
aggregate's core invariant on first read. So each existing market gets exactly
the entry `Create` would have written, dated its own `StatusDate`, with
`RecordedOnUtc` set to now. That the two differ is the point of keeping both,
and this migration is precisely the case they exist to describe. Verified: 6
markets → 6 entries, none without history, every one dated from its own market.

**Deferred, and both were in the approved scope:** risk-of-supply is orthogonal
to status — a flag can be raised and cleared without the status moving — so it
would have been a third concept in a one-concept story. **Market-record
deactivation is its own follow-up story**, completing the activation lifecycle
S001 introduced; folding it in here would have blurred the very distinction the
table above protects.

**Verification:** 903 backend tests (+17), 63 browser specs (+1), migration
backfill verified, CORS widening reverted with no `Program.cs` diff.

**Operational note.** Docker Desktop stopped mid-story, taking Postgres with it;
restarted, no data lost. Separately, four integration tests failed on first run
because the *fixture* created markets dated today and then tried to launch them
in 2021 — the chronology rule refusing bad test data, not a defect. The domain
tests, which backdate properly, passed throughout and localised it immediately.

> **Keep this in the retro.** The fixture asked the system to *launch something
> before it existed*, and the domain refused. That is not a nuisance; it is
> evidence the chronology rule lives where it belongs. Had it been UI
> validation, the same fixture would have written incoherent history straight
> past it and the tests would have passed.

---

### S003a — Market deactivation *(shipped)*

Completes the activation lifecycle S001 opened. `Activate`/`Deactivate` mirror
`OrganizationDivision`, including refusing a no-op rather than being idempotent:
a caller asking for a state the record already holds is acting on a stale view.

**The rule this story deliberately does not impose.** Nothing consults the
registrations held in a market before retiring its record, for two reasons and
the second is the stronger:

1. It would reverse the dependency the epic exists to establish. `GlobalProduct
   → MedicinalProduct → Registration` runs one way, and a parent inspecting its
   dependants across an aggregate boundary is the first crack in it. It is also
   the same coupling S002 refused when it dropped `registrationCount` from the
   markets list — accepting it here would have made that decision arbitrary.
2. **"Has registrations" is not the invariant anyone means.** An expired
   registration should not block retirement; nor a withdrawn one; nor a
   superseded one. The rule immediately becomes *"has registrations whose
   current status is…"*, which is a policy over another aggregate's lifecycle.
   **The more a rule depends on `Registration` semantics, the more clearly it
   belongs with `Registration`.**

The UI warns — *"This market holds 1 authorisation"* — and proceeds. **Warnings
help humans; domain rules preserve truth**, and nothing here makes a truth
impossible to represent. If the rule is ever genuinely required, it arrives as
an application-level policy that reads registrations and then calls
`Deactivate`, and the aggregate stays ignorant.

**What deactivation means, stated so it cannot drift:**

> **Active** — this market record participates in normal operational workflows.
>
> **Inactive** — this market record is retained for history but intentionally
> excluded from operational workflows. **Deactivation implies no regulatory or
> commercial state**: it does not withdraw a licence, does not take a product
> off sale, and does not delete anything (ES-018).

**Tests as living documentation of the boundary.** Three questions are asked of
a market presence — what has the regulator done, is it on sale, should this
record be used — and each pair is asserted independent in both directions:
discontinuing leaves the record active; retiring leaves the sale status,
the derived launch date and the registrations untouched; restoring changes
neither; and an inactive record still accepts commercial history.

One of those tests needed a **test-only** reference from the Product test
project to `RegOS.Registration.Infrastructure`, so it could create a real
licence and prove it survives. Noted because it is the one place in the epic
where those two names appear together, and it is a test proving a negative —
the Product context itself still references nothing of Registration.

**Retired markets stay visible**, labelled, never hidden. Hiding is data loss
dressed as a default — the `ListMarketRegistrations` precedent.

**Verification:** 913 backend tests (+10), 64 browser specs (+1), CORS widening
reverted with no `Program.cs` diff.

---

## ADR-039 — staged material

Written as it was decided rather than reconstructed at the end. S004 lifts this
section; nothing here needs to be re-derived from the commits.

### Decisions to record

| # | Decision | Where it was made |
|---|---|---|
| 1 | The tier exists, and **`Registration` names only `MedicinalProduct`** — not the global product, not the country | S001 |
| 2 | **No uniqueness on (global product, country)**, which is what makes resolve-or-create impossible rather than merely unwise | S001 |
| 3 | **`Registration` is intentionally the authorisation root** for now; a separate `MarketingAuthorization` aggregate is unnecessary unless the domain reveals a clear distinction | Phase 2, Amendment C |
| 4 | **`LanguageCode` is a value object**, not a governed reference-data aggregate | S002 |
| 5 | **Only the history *shape* generalises**, never the transition graph | S003 |
| 6 | **Uniqueness on (market, language)** is the deliberate opposite of decision 2 — alternative labels vs distinct business objects | S002 |

### The vocabulary rule this epic established

> **Never reuse a word for two concepts — but reusing a word for one concept
> across tiers is correct, and preserves the vocabulary rather than diluting it.**
>
> `Planned` appears on both `RegistrationStatus` and `MarketStatus` and means
> the same thing at each: *intended, not yet actual*. `Withdrawn` was refused at
> the market tier for the opposite reason — it would have meant *authorisation
> surrendered* on one row and *commercial availability ceased* on the row beside
> it, and the portfolio views show both at once.
>
> This is a naming principle, not a fact about market status. It applies to
> every tier this model grows.

A corollary worth stating: an initial state whose meaning is **already consumed
by having moved on** needs no rule forbidding return to it. `Planned` cannot be
re-entered because a market already entered cannot be intended; `NotLaunched`
would have needed that written down instead. **Prefer the word whose semantics
enforce the constraint.**

### The extraction criterion for EPIC-006

When the third, fourth and fifth append-only status histories arrive
(authority question, commitment, inspection, meeting), the shared abstraction is
**not** `StatusHistory`. The line runs between shape and semantics:

| Shared | Owned by each concept |
|---|---|
| append-only entries | permitted transitions |
| `OccurredOn` / `RecordedOnUtc` | initial status |
| current-value projection | terminal statuses |
| chronology validation | business meaning of each state |

`RegistrationStatusEntry` and `MarketStatusEntry` are kept **identical** — field
for field, table for table, configuration for configuration — precisely so that
extraction is mechanical. `RegistrationLifecycle` has no counterpart at all, and
that absence is the evidence for where the boundary sits.

### Principles established

Not observations about EPIC-017 — principles the next epics inherit.

1. **Market identity is explicit.** `MedicinalProduct` is the authoritative
   owner of market-specific identity; downstream aggregates derive market and
   product context from it rather than duplicating those facts.
2. **Persist facts, derive interpretation.** Historical events are stored once;
   values such as launch date are projections over history, not independently
   persisted state. (Restates ADR-037 with a second demonstration.)
3. **Reuse shapes, not semantics.** Shared infrastructure may emerge for
   append-only bitemporal histories, but lifecycle transition rules remain owned
   by each domain concept.
4. **Model the demonstrated business concept.** Introduce the smallest
   abstraction the domain currently requires — `LanguageCode` rather than a
   governed `Language` reference model — and record what would falsify that.

Expected to guide **EPIC-006** (principle 3), **EPIC-010** and **EPIC-018**
(principles 1 and 4), and **EPIC-020**.

**Sequencing note:** this epic and **EPIC-004** are genuinely independent — sequences live inside `Submission` and never touch `ProductId`; this never touches submission internals. Neither makes the other harder. Order is a **value call**: this one completes an epic already in flight (EPIC-005); EPIC-004 completes nothing in flight but may be what a customer is waiting on.
