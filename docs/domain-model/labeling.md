# Labeling

---

Title: Labeling Domain Model

Owner: Architecture Review Board

Status: Approved

Version: 1.0

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

# What exists today (S001)

```
GlobalProduct
  └── GlobalLabel               the core data sheet and its siblings
        └── GlobalLabelVersion  draft → in force → superseded
              └── ContentId ──→ ProductDocument
```

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

# Not here yet, and why

| | |
| --- | --- |
| `LocalLabel`, `Artwork` | S002 |
| `Indication`, `Contraindication`, `UndesirableEffect`, `Interaction`, `Population` | S003–S005, hanging off `MedicinalProduct` |
| **What a label version published** | deliberately absent. It is five versioning questions — partial publication, wording, withdrawal, historical wording, splits — and ADR-059 §3 names them rather than answering them with a foreign key |
| Renaming or retiring a label | nobody has asked; the gap is known, not hidden |
| Real clinical terminology (MedDRA, SNOMED, ICD) | the `System` field on every `CodedConcept` is the seam; RegOS holds no licence |

---

# Change History

| Version | Date       | Summary |
| ------- | ---------- | ------- |
| 1.0     | 2026-08-04 | EPIC-018 S001: the `Labeling` context, `GlobalLabel`/`GlobalLabelVersion`, and the domain-word/screen-word pairs. |
