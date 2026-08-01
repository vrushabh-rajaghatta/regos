# ADR-039 — The Market-Local Product Tier, And What A Registration Names

**Status:** Accepted · **Date:** 2026-08-01 ·
**Related:** [ADR-037](ADR-037-registrations-are-regulatory-assets-with-derived-visibility.md) (persist facts, derive interpretation),
[ADR-038](ADR-038-organization-depth-roots-and-the-three-filter-shapes.md) (root justification, filter shapes, enum-vs-data),
[ADR-018](ADR-018-rule-of-three.md) (rule of three),
[ADR-016](ADR-016-persistence-access-model.md) (persistence access model),
[ADR-031](ADR-031-tenant-isolation-by-query-filters.md) (isolation enforcement),
[ADR-012](ADR-012-shared-semantic-exception-model.md) (semantic exceptions)

## Context

RegOS's `Product` was a **global** identity — one record for the whole world.
The regulated world is **market-local**: a product is called something different
in Canada than in France, is on sale in one and not the other, and holds a
separate licence in each.

The usual framing — *"RegOS collapsed the product hierarchy"* — was imprecise.
Measured against the DIA RIM object model, `Product` sat exactly at **Global
Product**, and market-locality was expressed on the *transactional* records
instead: `CountryId` lived on `RegulatoryApplication` and `Registration`. The
tier genuinely absent was **Medicinal Product**, and it is where trade name,
market status, local label, ATC code and strength all want to live. About
eighteen further RIM objects hang below it.

Inserting a tier **between** existing aggregates is the expensive kind of
change. EPIC-017 did it in seven stories, and the decisions below are the ones
a future contributor would otherwise re-derive — or, worse, quietly reverse.

## Decision

### 1. The tier exists, and a registration names only it

`MedicinalProduct` sits between `GlobalProduct` and `Registration`:

```
GlobalProduct  →  MedicinalProduct  →  Registration
```

**`Registration` carries neither the global product nor the country. Only
`MedicinalProductId`.**

Both were the tier's facts. A second copy on the registration is duplicated
domain state with no transaction spanning the two aggregates to keep them in
agreement — so "Canadian medicinal product, Australian registration" is now
**unrepresentable** rather than merely forbidden.

The read models are unchanged in shape: `countryId` and `countryName` still
reach every caller, joined through the tier.

> The reasoning generalises. When aggregate B is defined partly *by* aggregate
> A, B stores the reference to A and nothing A already owns.

### 2. No uniqueness on (global product, country)

A medicinal product is identified by **its own identity**, not by the pair.
Several may exist for one pair when the business distinguishes them —
presentations, strengths, or the two halves of a partial divestment. This is
the same call EPIC-005 made for `Registration`, one tier up.

The absence is asserted by a test, and it is load-bearing: **it is what makes
resolve-or-create impossible rather than merely unwise.** A handler resolving a
(product, country) pair would be choosing a business object on the user's
behalf, non-deterministically. `CreateRegistrationCommand` therefore takes an
explicit `MedicinalProductId`, and pick-or-create lives in the UI.

The decisive argument is **dependency direction**: a medicinal product means
*"we market, or intend to market, here"* and can hold that meaning for years
with zero registrations — dossier preparation, labelling, artwork, pricing and
launch planning all precede authorisation.

### 3. Uniqueness on (market, language) — the deliberate opposite

One trade name per language, enforced in the aggregate *and* as a unique index.

Two market presences in one country are two business objects a company may
legitimately hold. Two English names for one market presence are two labels for
one thing, so one of them is wrong. **Different concepts, different
invariants** — stated in both places, because a reader will otherwise ask why
the two rules disagree.

### 4. `Registration` is intentionally the authorisation root

As the model grows, `Registration` will resemble the marketing authorisation
itself — the root that variations, renewals, sequences and authority
interactions hang from. A separate `MarketingAuthorization` aggregate is
considered **unnecessary unless the domain reveals a clear distinction**.

Read this as a deliberate simplification, not an oversight.

### 5. `LanguageCode` is a value object, not reference data

> `LanguageCode` intentionally models the minimum demonstrated requirement
> (ISO 639-1 language). If future domain rules distinguish regional variants —
> for example `en-CA` vs `en-US` — this value object may evolve into a locale
> **without changing aggregate semantics**. A reference-data aggregate is
> deferred until the domain requires governed language metadata rather than
> validated identifiers.

Applying ADR-038's own test: **does a rule branch on it?** No. Language
participates in *identity* — `(market, language)` — but never in *behaviour*.
Countries drive validation, authority selection and market identity; language
drives display, and those are not equivalent.

**Governed reference data exists because the domain needs governed facts, not
because dropdowns need labels.** The picker's readable names come from
`Intl.DisplayNames` over a curated code list in `constants/`.

The value object owns `Parse`, `TryParse`, `FromIso639_1` and equality —
including the EF converter, which reads the column back through
`FromIso639_1` rather than a constructor. **No caller anywhere handles a raw
language string**, which is exactly what makes the locale evolution above a
change to one file.

### 6. Only the history *shape* generalises

`MarketStatusEntry` matches `RegistrationStatusEntry` field for field, table for
table, configuration for configuration — kept identical so that EPIC-006's
extraction is mechanical.

**`RegistrationLifecycle` has no counterpart, and that absence is the
decision.** A regulator's decision graph is genuinely constrained; commercial
reality is not. A product may be launched, become temporarily unavailable,
return, and be discontinued and relaunched years later without a single
incoherent step. Encoding one company's commercial history as universal law is
what `RegistrationLifecycle`'s own governing principle forbids.

When the third, fourth and fifth histories arrive, the line runs here:

| Shared | Owned by each concept |
|---|---|
| append-only entries | permitted transitions |
| `OccurredOn` / `RecordedOnUtc` | initial status |
| current-value projection | terminal statuses |
| chronology validation | business meaning of each state |

### 7. Three questions, three answers, never merged

A market presence is asked three different questions, and each has its own
field with its own shape:

| Question | Field | Shape |
|---|---|---|
| What has the regulator done? | `Registration.CurrentStatus` | append-only history + transition table |
| Is the product on sale? | `MedicinalProduct.CurrentMarketStatus` | append-only history, no transition table |
| Should this record be used? | `MedicinalProduct.Status` | activation flag, one `StatusDate` |

Enforced by naming rather than discipline, and each pair asserted independent
in both directions by tests. In particular:

> **Active** — this market record participates in normal operational workflows.
>
> **Inactive** — this market record is retained for history but intentionally
> excluded from operational workflows. **Deactivation implies no regulatory or
> commercial state**: it does not withdraw a licence, does not take a product
> off sale, and does not delete anything (ES-018).

### 8. The launch date is derived, never stored

It is the `OccurredOn` of the **first** entry reaching `Launched`. A stored
field would be a second copy of a fact the history already holds — decision 1,
one tier down — and deriving it dissolves a question rather than answering it:
*"why does the launch date precede approval in migrated data?"* cannot arise,
because nobody types it.

It means **first commercial availability**, not authorisation effectiveness.
That already exists as `Registration.ApprovedOn`, one aggregate over. First
rather than most recent, because a relaunch is a different question.

## Principles established

Not observations about EPIC-017 — principles the next epics inherit and should
cite.

1. **Market identity is explicit.** `MedicinalProduct` is the authoritative
   owner of market-specific identity; downstream aggregates derive market and
   product context from it rather than duplicating those facts.

2. **Persist facts, derive interpretation.** Historical events are stored once;
   values such as launch date are projections over history, not independently
   persisted state. (ADR-037, with a second demonstration.)

3. **Reuse shapes, not semantics.** Shared infrastructure may emerge for
   append-only bitemporal histories, but lifecycle transition rules remain owned
   by each domain concept.

4. **Model the demonstrated business concept.** Introduce the smallest
   abstraction the domain currently requires — `LanguageCode` rather than a
   governed `Language` reference model — and record what would falsify that.

5. **A cross-aggregate rule belongs with the lifecycle it depends on.**
   *The more a rule depends on another aggregate's semantics, the more clearly
   it belongs with that aggregate.* Apply it whenever a rule is tempted across a
   boundary — *"can I archive a product if…", "can I delete a site if…"*. If
   answering requires understanding another aggregate's **business lifecycle** —
   not merely whether rows exist, but which of their states count — the rule
   belongs in orchestration or in that other aggregate.

6. **Interaction surfaces follow aggregate boundaries.** When an aggregate
   accumulates independent behaviour and history, the UI should present it as a
   first-class working surface rather than continuing to compress it into a
   summary row. Not every aggregate earns a page — but the moment a row grows a
   fifth action, the domain has already decided and the UI is lagging. This is
   not a React decision; it is a consequence of the model.

7. **Writes remain owned; reads compose.** A read model may project across
   bounded contexts to answer a user's question. **Projection does not imply
   write ownership, nor does it justify cross-aggregate invariants.**
   `ListMarketRegistrations` reads `MedicinalProduct` and `TradeName` to answer
   *"what do we hold in Canada?"* — and nothing about that licenses `Product` to
   validate a `Registration`.

### The vocabulary rule

> **Never reuse a word for two concepts — but reusing a word for one concept
> across tiers is correct, and preserves the vocabulary rather than diluting
> it.**

`Planned` appears on both `RegistrationStatus` and `MarketStatus` and means the
same thing at each: *intended, not yet actual*. `Withdrawn` was refused at the
market tier for the opposite reason — it would have meant *authorisation
surrendered* on one row and *commercial availability ceased* on the row beside
it, and the portfolio views show both at once.

A corollary: **prefer the word whose semantics enforce the constraint.** An
initial state whose meaning is already consumed by having moved on needs no rule
forbidding return to it. `Planned` cannot be re-entered because a market already
entered cannot be intended; `NotLaunched` would have needed that written down
instead.

The same rule now governs screens, not only types —
[accessible-names.md](../engineering/accessible-names.md).

## Consequences

- **Every `ProductId` in the codebase has an explicit tier**, recorded in the
  epic's re-pointing table. `RegulatoryApplication`, `ProductDocument` and
  `ProductDirectoryRow` stay **global**; only `Registration` moved.
- **The migration was additive.** One medicinal product per distinct
  (tenant, global product, country) already on a registration; verified on a
  fresh database, on a clone of the dev database, and by rolling `Down`.
- **EPIC-018 (labels), EPIC-010 (IDMP) and EPIC-020 attach to the tier**, not to
  the global product — and to the market's working surface, not to the product
  summary.
- **`RegOS.Product.Domain` now references `RegOS.ReferenceData.Domain`**, the
  fourth context to do so. Not a new kind of edge.
- SC-002's grandfathered list is **empty**; `detailOf` is shared on its third
  demonstrated consumer.

## Revisit When

- **EPIC-006 brings authority-question, commitment, inspection and meeting
  statuses.** That is the extraction point for the bitemporal history shape —
  and decision 6 is the line it should cut along. *If EPIC-006 ships and the
  extraction is still not obviously worth doing, the shape was never the
  duplication we thought it was.*
- **A domain rule distinguishes `en-CA` from `en-US`.** `LanguageCode` becomes a
  locale, or the `Language` reference aggregate finally earns itself.
- **A registration needs to outlive or precede its medicinal product**, or a
  variation needs a root of its own. That is when decision 4 is revisited and
  `MarketingAuthorization` may become real.
- **Two medicinal products for one (product, country) pair are never created in
  anger.** Decision 2 carried real cost — an explicit id on every command, and
  pick-or-create in the UI. *This entry is deliberately **absence-shaped**: if
  the pair turns out to be effectively unique in practice, nothing will fail,
  and only looking will reveal it.*
- **A market record is deactivated while holding a live licence and someone
  calls it a bug.** That is when principle 5's escape hatch — an
  application-level policy, not an aggregate rule — gets built.
