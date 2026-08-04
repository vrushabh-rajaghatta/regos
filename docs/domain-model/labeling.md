# Labeling

---

Title: Labeling Domain Model

Owner: Architecture Review Board

Status: Approved

Version: 1.2

Last Reviewed: 2026-08-04

Related Documents:

- product.md

Related ADRs:

- [ADR-059](../adr/ADR-059-clinical-statements-are-facts-labels-are-artifacts.md)

---

# The one sentence this context is built on

> **A clinical statement is a regulatory fact about a product in a market. A
> label is an editorial artifact that publishes some of those facts at a point
> in time.**

A contributor who holds that sentence can re-derive the rest of this document.
It is why indications will hang off `MedicinalProduct` and not off a label, why
labels version on their own clock, and why `ProductDocument` stays generic.

---

# Vocabulary — domain word and screen word

They differ here on purpose, and both are binding.

| Domain (code, ADRs, this document) | UI (navigation, labels, headings) |
| --- | --- |
| `GlobalLabel` | **Global label** |
| `LocalLabel` *(S002)* | **Local label** |
| `GlobalLabelVersion` | **Version** |
| `UndesirableEffect` *(S004)* | **Side effect** |
| `Population` *(S003)* | **Who it applies to** |
| `OtherTherapy` *(S003)* | **Used with** |

**`GlobalLabel` is the pair to watch, and today it is not a pair at all.**
Medical Affairs says *CCDS*; Regulatory says *Core Data Sheet*; vendors say
*Company Core Data Sheet*; engineers read *Global label* without difficulty.
Which belongs on screen is a user-research question nobody has asked, so the
screen keeps the plain word until they do — and the type stays `GlobalLabel`
whatever the answer, because changing a label is trivial and renaming a domain
model six months later is not.

**`LocalLabel` is the one place this context departs from RIM's noun.** RIM
calls it `Labeling`; a context named `Labeling` holding a type named `Labeling`
reproduces the namespace-equals-type collision S000 removed fourteen `using`
aliases to delete. Mechanical reason, not a modelling one — and it names the
pair symmetrically with `GlobalLabel`.

The screen's word must never reach a type, and the type's word must never reach
a label without a reason to prefer it.

---

# What exists today (S001–S002)

```
GlobalProduct
  └── GlobalLabel                the core data sheet and its siblings
        └── GlobalLabelVersion   draft → in force → superseded
              └── ContentId ──→ ProductDocument

MedicinalProduct  (the market)
  └── LocalLabel                 what this authority approved
        └── LocalLabelRevision   draft → in force → superseded
              ├── ContentId ──→ ProductDocument
              └── DerivedFromGlobalLabelVersionId?  ──→ GlobalLabelVersion
```

**The two tiers are versioned for unrelated reasons**, and the different words
are the reminder:

| | Versioned because | Word |
| --- | --- | --- |
| `GlobalLabel` | the company's scientific position evolves | **version** |
| `LocalLabel` | each authority approves, delays, amends and republishes that position independently | **revision** |

They intersect only through `DerivedFromGlobalLabelVersionId`. Nothing inherits,
and neither status enum is shared — a rule added to one must never reach the
other by accident.

## `GlobalLabel`

Tenant-owned, held for a `GlobalProductId`, carrying a name and a `LabelType`
drawn from `LabelVocabulary`. It is created with its first draft already open —
a label with no version is a name with nothing behind it.

**Nothing enforces one label per product per type.** A company may hold two
patient leaflets for one product where the audiences differ, and uniqueness we
cannot justify is uniqueness that will be wrong for somebody. The same call
`MedicinalProduct` made on `(GlobalProductId, CountryId)`.

**There is no `Status`.** A label's meaningful lifecycle lives in its versions;
"retire this label" is a capability nobody has asked for, and a column that is
always `Active` is a field nobody filled in. The same call `Substance` made on
`IsActive`.

## `GlobalLabelVersion`

| State | Means |
| --- | --- |
| `Draft` | being written; the only state in which anything can change |
| `InForce` | published and current; **at most one version is ever here** |
| `Superseded` | was in force, and a later version replaced it |

**The act is *publish*; the state is `InForce`.** Two words on purpose — a
regulatory user asks which version is in force on a date, not which was
published, and those diverge the moment a version is approved in March to take
effect in June. Both dates are stored.

Three rules carry the aggregate:

1. **At most one open draft.** The next draft is numbered one past the highest
   ever issued, so a discarded draft's number is reissued and a cited one never
   is.
2. **Publishing requires content.** A version with no document is a number, and
   a number is not a label. This is what makes the `ProductDocumentId` link
   load-bearing rather than decorative.
3. **Publishing and superseding are one act.** The replaced version's
   `EffectiveTo` is computed as the day before the new one takes effect — the
   ranges meet exactly, with no gap, no overlap, and no day on which two
   versions were both in force. A caller who could set that date could produce
   one.

**A draft is the one thing that can be deleted**, and that does not contradict
ES-018: lifecycle-over-deletion protects records that were once true, and a
draft has never been in force, was never cited, and never described what the
company said. The guard is `Draft` — not "not in force" — so a superseded issue
is as untouchable as the current one.

---

# `LocalLabel`

A market's own controlled labelling document, hanging off `MedicinalProductId`.
**It carries no country** — the market-local tier already answers which
jurisdiction this is (ADR-039), and a second copy could disagree.

One `LocalLabel` is one *(document, language)* pair: a market with two languages
holds two labels, because each is separately approved. Nothing enforces
uniqueness on the triple, for the reason `GlobalLabel` and `MedicinalProduct`
enforce none either.

## Carton artwork is a type, not an aggregate

Prescribing information, patient information leaflet, carton artwork and
container label are all `LabelType` values. A printed carton is a controlled
document an authority approved, revised on its own history and derived from a
core position — which is what every other entry is. Giving it its own root would
duplicate the revision logic, the approval rules, the effective dating, the
derivation, the API and the browser proof in order to hold a nullable column.

> **The watchpoint: split when artwork develops its own persistent invariants,
> not when it acquires more attributes.** Nullable columns are not the signal;
> `if (Type == Artwork)` is. `LocalLabelTypeBranchTests` counts those branches so
> the question is asked by the build.

## `LocalLabelRevision`

| State | Means |
| --- | --- |
| `Draft` | being prepared; the only state in which anything can change |
| `InForce` | approved and current in this market; **at most one** |
| `Superseded` | replaced by a later revision, and retained |

Four rules carry it, and only the first two mirror the global tier:

1. **At most one draft**, numbered one past the highest ever issued.
2. **Publishing and superseding are one act**, with the retired revision's
   `EffectiveTo` computed as the day before its replacement takes effect.
3. **Approval and effect are separate facts.** *Approved 12 May, effective 1
   June* and *approved 12 May, effective immediately* both occur. A revision
   **cannot enter force without an `ApprovedOn`** — a label in force that no
   authority approved is a false statement about a regulated document — and it
   cannot take effect before the day it was approved.
4. **The derivation is optional.** A migrated portfolio does not know which core
   version revision 9 came from, and a local-first company holds approved
   labelling before any core label exists here. Requiring it would force somebody
   to invent history.

**A draft is the only thing that can be discarded.** An approved labelling
document is a controlled record, and overwriting one is a governance failure
rather than an edit — which is why the refusal says *"start a new revision"*
rather than *"not allowed"*.

`DataCarrierCode` is artwork's one identifying attribute. SKUs and pack
configuration are deliberately absent: packaging is EPIC-010's, and this is the
seam rather than a second packaging model.

---

# Three kinds of change, kept apart

The context has now met all three, one story at a time, and keeping them
distinct is what will make it age well.

| The thing | Its history | Because |
| --- | --- | --- |
| `GlobalLabel` | **version** | the company's scientific position evolves, and each issue is an edition of it |
| `LocalLabel` | **revision** | one authority approves, delays, amends and republishes that position on its own clock |
| `Indication` | **status history** | approved, expanded, restricted, withdrawn are successive *decisions*, not successive editions |
| `Contraindication` | **the label revision that published it** | it is content inside an approved label, not something an authority acts on directly |
| `UndesirableEffect` | **the label revision that published it** | same, plus a `Frequency` — recorded as the label states it, never computed |
| `DrugInteraction` | **the label revision that published it** | same again, and the only statement that must name what it is *with* |
| a clinical concept | **a code** | *Type 2 diabetes mellitus*, *Diabète sucré de type 2* and *Diabetes mellitus Typ 2* are one thing, and only a code says so |

**Five answers, because they are different kinds of change.** The first two are
separate rows on purpose: a generic versioning abstraction over both would let a
rule added to one lifecycle reach the other.

## The rule the last three rows encode

> **A clinical statement owns history only when the authority regulates *it*.
> Otherwise its historical context is the label revision that published it.**

This is why three aggregates that all look like "structured clinical statements"
are not shaped alike, and the difference is regulatory practice rather than a
preference for asymmetry:

| | What the authority acts on | So |
| --- | --- | --- |
| `Indication` | **the authorisation itself** — a new indication is granted, a paediatric extension approved, a use restricted, an indication withdrawn | it owns the history of those decisions |
| `Contraindication`, `UndesirableEffect` | **the approved labelling document** — nobody files a variation to *"withdraw contraindication #4"*; they file a revised SmPC or package insert | history belongs to the `LocalLabelRevision`, and giving them one of their own would invent a lifecycle no regulator operates |

**The falsifier, recorded because it is the thing that would overturn this:** a
market withdrawing a single contraindication with no new approved labelling
document. That is not how labelling works — the authority controls the approved
label, and the label is the regulated artifact.

**The discriminator between the first two:** *is the wording the regulated
object, or is the approval the regulated object?* For a label, wording is the
artifact. For an indication, approval is the artifact, and the wording is how
the label communicates it. That is why `LocalLabelRevision` exists and
`IndicationRevision` does not.

---

# The boundary with `ProductDocument`

> **Documents remain content storage. Labeling owns the regulatory meaning.**

`ProductDocument` is scoped to `GlobalProductId` and has no market dimension.
A local label's PDF will therefore sit against the global product, and the
*label* is what says it is the Japanese one, effective from a date, derived from
core version 4.

`GlobalLabelVersion.ContentId` is a plain nullable column — no navigation
property and no database foreign key. `ProductDocument` owns that record's
lifecycle, and a constraint here would make Labeling's schema a party to it. The
handler checks, at write time, that the document exists, is this tenant's, and
is held for the same product the label is; that is the anti-corruption check,
and it is the reason a label held for product A cannot quote a file belonging to
product B.

The dependency is `Labeling → ProductDocument`, by id, one way. Nothing depends
on `Labeling`.

---

# Isolation

Every aggregate here is tenant-owned and takes the first of ADR-038's three
filter shapes — fail-closed on `TenantId`.

**`GlobalLabelVersion` carries no `TenantId` and therefore no filter of its
own.** There is deliberately no `GlobalLabelVersions` set on `RegOSDbContext`:
every read of a version starts at `GlobalLabels` and reaches versions through
it. A read that began at the version table would cross every tenant.

---

# `Indication`

What a product is approved to treat in one market. Hangs off
`MedicinalProductId`; nothing points at a label version (ADR-059 §3).

**A dated history of decisions, not revisions.** `Approved`, `Expanded`,
`Restricted`, `Withdrawn` are successive regulatory decisions, and the history
is append-only — an indication must not silently *be* withdrawn, it must have
*become* withdrawn on a date. There is no transition table: restricted, expanded
again, withdrawn years later is a coherent sequence, and encoding one company's
history as universal law is what `RegistrationLifecycle`'s own governing
principle forbids.

**The condition is coded; the wording is not.** The code is the join key that
makes the same authorisation recognisable in Japan and France; `LabelText` is
what this market's label says. `RestateLabelText` leaves the status history
untouched, which is the whole reason this aggregate has no revisions.

`Population` is an owned entity with identity, and `AmendPopulation` is what
earns it: correcting 12+ to 6+ keeps the same id, because it is the same
qualifier. `RemovePopulation` — not retire; a qualifier has no lifecycle, and the
regulatory history lives in the status entries.

`OtherTherapy.Therapy` is free text: it may be a substance RegOS knows, a drug
class it does not, or a procedure that is no product at all.

---

# `Contraindication` and `UndesirableEffect`

Structured statements inside this market's approved label. Both hang off
`MedicinalProductId`, both carry a coded condition and the label's wording, and
**neither owns a history** — see the rule above.

`UndesirableEffect.Frequency` — *very common* through *very rare*, plus *not
known* — is the only attribute the three statement types do not share. It is
**recorded, never computed**: the bands have numeric thresholds resting on trial
data RegOS does not hold, and deriving one would assert a calculation nobody
performed. Null is ordinary.

## `Population`, mapped three times

One CLR type in `Aggregates/ClinicalStatements/`, owned by each statement as its
own collection, in its own table. **Confirmed rather than assumed:** S004's
hypothesis was that the second and third configurations would differ from the
first only by table and foreign-key name, and the migration that added them left
`IndicationPopulations` completely untouched — the shared helper reproduces the
original mapping exactly.

**Owned, not a standalone entity**, because three aggregates cannot own one
entity type with three tables: EF scopes an owned type per owner, which is what
turns one class into three entity types. A useful consequence — owned
collections load with their owner, so no repository needs an `Include` for them,
and the "rule reads a collection that was never loaded" failure is unreachable
by construction.

**This is not the shared base type across aggregate roots ADR-059 §4 forbids.**
Nothing couples the three roots; either may grow a rule the other does not, and
at that point it stops being one shape and should stop being one class.

`OtherTherapy` stays on `Indication` alone. Phase 1 allows it on contraindications
too; nobody asked for it, so it remains at one use.

# `DrugInteraction`

**Named for a namespace, not a model.** RIM calls it `Interaction`;
`RegOS.Interaction` is already the health-authority context (ADR-040), so the
bare noun would force an alias wherever both are visible. Second mechanical
departure from RIM's noun after `LocalLabel`, same reason. The screen says
**Interactions**.

**The one invariant this context did not already have: an interaction must name
at least one `Interactant`.** Every other statement is meaningful alone. An
interaction with nothing to interact with is not under-specified — it is not a
statement — so the first interactant is supplied at creation and the last one
cannot be removed.

`Interactant.SubstanceId` is **optional, and beside the text rather than instead
of it**. Most interactants are not compounds RegOS knows: grapefruit juice,
alcohol, *CYP3A4 inhibitors*. Set the link and *"which of our products interact
with warfarin?"* is a join; leave it null and the label still says what it says.
This is the seam `OtherTherapy` said would arrive when someone needed the
question asked backwards.

---

# Not here yet, and why

| | |
| --- | --- |
| Renaming or retiring a `LocalLabel` | nobody has asked; same call as `GlobalLabel` |
| SKUs, pack size, GTIN, printer | EPIC-010's packaging model, not a second one here |
| `Indication`, `Contraindication`, `UndesirableEffect`, `Interaction`, `Population` | S003–S005, hanging off `MedicinalProduct` |
| **What a label version published** | deliberately absent. It is five versioning questions — partial publication, wording, withdrawal, historical wording, splits — and ADR-059 §3 names them rather than answering them with a foreign key |
| Renaming or retiring a label | nobody has asked; the gap is known, not hidden |
| Real clinical terminology (MedDRA, SNOMED, ICD) | the `System` field on every `CodedConcept` is the seam; RegOS holds no licence |

---

# Change History

| Version | Date       | Summary |
| ------- | ---------- | ------- |
| 1.2     | 2026-08-04 | EPIC-018 S003–S004: `Indication`'s dated decisions, `Contraindication` and `UndesirableEffect` without histories, and `Population` mapped three times. |
| 1.1     | 2026-08-04 | EPIC-018 S002: `LocalLabel`/`LocalLabelRevision`, artwork as a type, and the approval-versus-effect split. |
| 1.0     | 2026-08-04 | EPIC-018 S001: the `Labeling` context, `GlobalLabel`/`GlobalLabelVersion`, and the domain-word/screen-word pairs. |
