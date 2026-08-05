# ADR-062 — A Language Is a World Fact, Not a Product Fact

**Status:** Accepted · **Date:** 2026-08-05 ·
**Related:** [ADR-017](ADR-017-shared-kernel-scope.md) (why this does not go to `SharedKernel`),
[ADR-018](ADR-018-rule-of-three.md) (the count, and why it is the weaker half here),
[ADR-058](ADR-058-substances-are-shared-facts-ingredients-are-roles.md) (`CodedConcept`, and shared facts vs roles),
[ADR-043](ADR-043-strongly-typed-identity.md) §2 (flat master data, which `Country` remains),
[ADR-059](ADR-059-clinical-statements-are-facts-labels-are-artifacts.md) (`LocalLabel.Language`, the consumer that could not be answered),
[EPIC-022](../product/epics/EPIC-022-country-depth.md) D2

## Context

`LanguageCode` lives in
[`src/Product/RegOS.Product.Domain/Product/`](../../src/Product/RegOS.Product.Domain/Product/LanguageCode.cs).
It was written for `TradeName` — one name per (medicinal product, language) —
and `LocalLabel` later took a dependency on it from the Labeling context, which
is already a context reaching into Product for a type that has nothing to do
with products.

**EPIC-022 makes `Country` the third consumer**, which by
[ADR-018](ADR-018-rule-of-three.md) is the occurrence at which to *evaluate*.
That count is true and it is not the reason this decision is being taken.

### The type predicted its own trigger

`LanguageCode`'s docstring, written in EPIC-017, drew the boundary explicitly:

> *"Nothing in the domain branches on language. It participates in identity —
> one trade name per (medicinal product, language) — but no rule asks whether a
> name is French. That is what makes it a value and not an enum, and what makes
> a governed `Language` table premature: **countries drive validation, authority
> selection and market identity, whereas language currently drives display.**"*

That sentence is a falsifiable claim with a named condition. **EPIC-022 S003
falsifies it.** A market that *requires* English and French, read against the
local labels actually recorded, is a rule that asks whether a label is French —
and it is a country that answers.

The word doing the work is **currently**. The docstring did not say language
never drives validation; it said it does not yet, and located where the change
would come from. It came from exactly there.

### The debt this closes

EPIC-018 shipped `LocalLabel.Language` and could not close its own gap: nothing
in RegOS can say which languages a market requires, so a user cannot be told
their Canadian label set is incomplete. **That is `Country`'s omission, not
Labeling's** — and Country cannot hold a language while the type lives in
Product, because `ReferenceData → Product` is the wrong direction and would
invert the dependency every other reference type respects.

## Decision

> **A language is a fact about the world, not a fact about a product.**
> `LanguageCode` moves to `RegOS.ReferenceData.Domain`.

### 1. It moves to ReferenceData, not to SharedKernel

[ADR-017](ADR-017-shared-kernel-scope.md) scopes the shared kernel to primitives
and abstractions with no domain meaning. A language code is controlled
terminology — ISO 639 — and belongs beside the other controlled terminology, for
the same reason
[`CodedConcept`](../../src/ReferenceData/RegOS.ReferenceData.Domain/Terminology/CodedConcept.cs)
is there rather than in `SharedKernel`.

The dependency then runs the way every other reference type already does:
`Product → ReferenceData` and `Labeling → ReferenceData`, with neither reaching
into the other.

### 2. It stays a value object, and does **not** become a `CodedConcept`

Regions and climatic zones become `CodedConcept` in the same epic, and language
deliberately does not. The difference is what `CodedConcept.System` exists to
record: *whose word is this?* A region's membership is published by the EU, by
ICH, by PIC/S — several authorities, disagreeing, replaceable by a licensed
register later. **ISO 639 has one authority and RegOS is not going to swap it**,
so a `System` column on every language row would carry the same constant
forever.

`LanguageCode` also already validates its own shape — two ASCII letters,
lower-cased — which a `CodedConcept` drawn from a seeded list would not.

### 3. It does **not** become a governed table

The docstring's other clause still holds: no aggregate branches on *which*
language, only on *whether the required set is satisfied*, and that set is held
by `Country`. A `Languages` reference table with rows, ids and a steward screen
would be a second place for a fact ISO already fixes.

**The trigger for revisiting**, stated so it is not re-argued: a rule that must
distinguish regional variants — `en-CA` from `en-US` — turns this into a locale.
The type says so already, and nothing in this ADR closes that door.

### 4. Required languages are advisory, and that is a decision about severity

`Country.Languages` says what a market needs. It does **not** refuse a label set
that lacks one. A Canadian label set mid-authoring is an ordinary state, and
EPIC-002 settled where blocking belongs: with a rule a blueprint states, not with
geography.

## Consequences

**A cross-context move with no behavioural change.** `TradeName`, `LocalLabel`
and their configurations change namespace and nothing else; the column, the
validation and the equality are identical. That is what makes it safe to do in a
story rather than an epic.

**`Country` gains a collection and stays flat master data.**
[ADR-043](ADR-043-strongly-typed-identity.md) §2 keeps `CountryId` a record
struct because Country has deterministic ids, no children and no lifecycle. An
owned collection of value objects is not a child with identity, so the reasoning
survives. **The falsifier:** if EPIC-012 gives Country a lifecycle —
active/inactive, merged, renamed — it becomes `Entity<CountryId>` and the
identity conversion comes with it.

**`Labeling` stops depending on `Product` for this.** One arrow removed from the
graph, in the direction the graph is supposed to run.

## What this ADR is really about

**A predicted architectural trigger firing is stronger evidence than reaching a
rule-of-three count.**

The count said *evaluate*. The prediction said *why*, *when*, and *what would
have to become true* — and then it became true. One of those is a threshold; the
other is an argument, and only the argument tells a future reader whether the
decision still holds.

This is the same lesson EPIC-010b learned from the other side. There, three
occurrences of *structured fact beside approved wording* were evaluated and the
abstraction **refused**, because the three differed in everything that mattered.
Taken together: **the rule of three is a trigger to think, and thinking can
return either answer.** A recorded prediction, when one exists, is the better
evidence — which is an argument for writing more of them, not for counting more
carefully.
