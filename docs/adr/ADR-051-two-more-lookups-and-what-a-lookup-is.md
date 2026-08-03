# ADR-051 — Two More Lookups, And What Makes Something A Lookup

**Status:** Accepted · **Date:** 2026-08-02 ·
**Related:** [ADR-043](ADR-043-strongly-typed-identity-and-the-flat-master-data-carve-out.md) (admits the first eight; **§3 amended here**),
[ADR-050](ADR-050-application-type-classifies-the-application.md) (renamed the concept ADR-043 §2 called `SubmissionTypeId`),
[ADR-047](ADR-047-publication-metadata-exists-only-when-publication-makes-it-true.md) (§6 — sub-type is an independent axis),
[ADR-018](ADR-018-rule-of-three.md)

## Context

EPIC-007a S003 adds two reference-data catalogues — `SubmissionType` (what a
regulatory activity is) and `SubmissionSubType` (what one sequence does to it) —
and `IdentityConventionTests` refused them. That refusal is the mechanism
working: ADR-043 §2 admits flat master data to a permanent carve-out from ES-020,
and its list may not grow on an implementer's say-so.

**The refusal also caught a name that would have misled a reader.** ADR-043 §2
lists `SubmissionTypeId` among its eight, and after ADR-050 that entry means
`ApplicationTypeId` — the catalogue was renamed because it enumerated eCTD's
`application-type` under eCTD's word for something else (evidence E11). A reader
of ADR-043 alone would conclude `SubmissionTypeId` was already admitted. It was
not. **The name was admitted; this concept has never been looked at.**

## Decision

### 1. `SubmissionTypeId` and `SubmissionSubTypeId` are flat master data

Both meet ADR-043 §2's test, and it is worth checking rather than asserting:

| ADR-043 §2 requires | `SubmissionType` | `SubmissionSubType` |
|---|---|---|
| platform-assigned deterministic ids | ✅ `70000000-…`, `71000000-…` | ✅ |
| no child entities | ✅ | ✅ |
| no lifecycle beyond `Create` | ✅ | ✅ |
| never loaded as an aggregate to be mutated | ✅ | ✅ |
| does not inherit `Entity<TId>`, and does not want to | ✅ | ✅ |

They are the same shape as `ApplicationType`, which they sit beside — three
authority-scoped catalogues of `Code`, `Name` and a wire `Token`.

**The two are not one taxonomy admitted twice.** A sub-type is an independent
axis, not a refinement beneath a type (ADR-047 §6): `Amendment` appears under an
original application and under an annual report alike, and FDA's example #23
opens an activity with `Report` rather than `Application` (evidence E13). Two
catalogues, two admissions.

### 2. A curation method does not make a lookup an aggregate

`ApplicationType` gained `RecordToken(string?)` in the same story, so that a
database seeded before the token column existed converges with a fresh clone.
That is a mutator on an admitted lookup, and it is worth saying plainly why it
does not evict the type from this carve-out.

> **"No lifecycle beyond `Create`" means no states and no transitions — not
> "never written after insert".** Reference data is *curated*: it is corrected
> when the world turns out to be different from what we recorded. A correction
> is not a lifecycle, and forbidding one would mean a wrong row could only be
> fixed by deleting it, which is the opposite of what ES-018 asks.

What would evict a type is **children or a lifecycle**, exactly as ADR-043 §2
already says. `RegulatoryTemplate` is the standing example on the other side: it
owns versions, its versions have states, and it is the metadata engine the
product exists to be.

The alternative was to reconcile the token with a raw `ExecuteUpdate` from the
seeder, leaving the type `Create`-only. **Rejected**: it moves a write around the
type rather than removing it, skips the normalisation `Create` applies, and
teaches that a back door is available when a rule is inconvenient.

### 3. ADR-043 §3's "both lists shrink-only" is too strong, and is corrected

ADR-043 §3 describes `IdentityConventionTests` as holding "both lists
shrink-only". That is right for `PendingMigration` — a backlog that must only
ever get shorter — and wrong for `MasterDataLookups`, which the test's own
comment already describes as growing "only with an ADR". The code and the ADR
disagreed, and the code was right.

> **`PendingMigration` is shrink-only. `MasterDataLookups` grows only by ADR.**
> They are not two versions of one list. One records work not yet done; the
> other records a decision that something is not that kind of thing at all.

The distinction is what stops the carve-out becoming an escape hatch. A shrink-only
backlog with no admissions route would have forced every new lookup into a
migration queue it never belonged in; an exemption anyone could extend would have
made ES-020 optional. Requiring an ADR costs one document per admission and makes
each one answerable.

Both lists keep their stale-entry tests, which is the other half of the
mechanism: an exemption cannot outlive the thing it excused.

## Consequences

- `IdentityConventionTests.MasterDataLookups` gains `SubmissionTypeId` and
  `SubmissionSubTypeId` — ten entries, each traceable to ADR-043 or here.
- **ADR-043 §2's list is not edited.** Its eight entries stay as written,
  including `SubmissionTypeId` meaning what ADR-050 says it means. This ADR is
  where the reader learns the name was later reused for a different concept, and
  that the new one is admitted on its own merits.
- The next new lookup needs an ADR too. That is the intended cost.

## Revisit When

- **A lookup here grows children or a lifecycle.** Then it was never master
  data, and it moves to `PendingMigration` rather than staying excused. The
  likeliest candidate is `SubmissionType`, if a regulatory activity ever becomes
  something RegOS models rather than derives — EPIC-007a Phase 2 decided it does
  not, and named what would change that.
- **A third catalogue arrives with the same `Code`/`Name`/`Token` shape.** Three
  is where ADR-018 asks whether the shape itself wants a name. Note that these
  two were added together and count as one demonstration, not two.
