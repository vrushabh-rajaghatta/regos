# ADR-057 — A Filed Artifact Is Projected From A Snapshot, And Continuity Is Enforced At The Boundary

**Status:** Accepted · **Date:** 2026-08-03 ·
**Related:** [ADR-047](ADR-047-publication-metadata-exists-only-when-publication-makes-it-true.md) (the instrument this applies),
[ADR-054](ADR-054-a-study-tagging-file-is-a-projection-over-a-study.md) (§5 — the lifecycle half it completes),
[ADR-045](ADR-045-the-cumulative-dossier-and-the-derived-delta.md) (the derivation this reuses, keyed differently),
[ADR-056](ADR-056-study-identity-is-owned-by-the-sponsor.md) (§4 — the dependency direction this protects),
[ADR-049](ADR-049-generation-derives-transmission-creates.md) (generation derives),
[E24, E29, E34](../evidence/README.md)

## Context

A Study Tagging File names a study by `study-id` **and title**, and FDA's TCG
§4.4 says a duplicated study is *"caused by an updated STF being submitted with
incorrect metadata (**study-id and study title not an exact match**)"* (**E24**).
So both are part of a key the authority matches sequences on.

RegOS lets a study be retitled — a study registered before its protocol was final
is the ordinary case, and a typo nobody can fix is a debt this project has paid
once already. That creates two different problems, and EPIC-019 S002 recorded a
resolution that only solved one of them:

| | |
|---|---|
| **Regeneration** | regenerating sequence 0000 next year must reproduce the bytes FDA received — not today's title |
| **Continuity** | sequence 0001 must not file the same study under a different title, or the reviewer sees two studies |

**Freezing solves the first and does nothing about the second.** A snapshot keeps
the old package honest; the new one drifts anyway.

The obvious fix for continuity is a guard on `Study`: *may this be retitled?* It
would have to ask whether any published sequence names the study — which points
`Study` at `Submission` and **inverts [ADR-056](ADR-056-study-identity-is-owned-by-the-sponsor.md) §4**,
the whole reason a study is its own context.

## Decision

> **A filed artifact is projected from a snapshot taken at publication, never
> from the aggregate it describes. Cross-sequence continuity is enforced at the
> boundary that produces the artifact, using frozen publication facts — never by
> a guard that inverts a context boundary.**

### 1. The freeze boundary

```
Study (mutable)
      │
      ▼
Publication            ← the snapshot is taken here, once
      │
      ▼
Frozen STF projection  ← FiledStudyIdentifier, FiledStudyTitle on the placement
      │
      ▼
XML
```

Everything below `Publication` is immutable, and everything the renderer reads
lives below it. `StudyTaggingFileRenderer` never touches the `Study` tables —
not as a rule to remember, but because the plan it is handed contains no study
id it could look up with.

**This is [ADR-047](ADR-047-publication-metadata-exists-only-when-publication-makes-it-true.md)'s
instrument applied to a fact the aggregate does not own.** ADR-047 froze what
publication made true about a submission; this freezes what publication made
true about something in another context. The mechanism is identical — the
handler resolves, the aggregate stores, nothing recomputes.

**It is a deliberate duplication**, and worth naming as one. Everywhere else
RegOS refuses to copy a fact because two copies can disagree. Here they are
*meant* to: the disagreement between `FiledStudyTitle` and the registry is the
record of a study having been renamed since it was filed.

### 2. Continuity is the artifact boundary's rule, not the aggregate's

The generator already reads the previous published sequence to derive a
document's `operation` (ADR-045). It is therefore the one place that can see
both what this sequence says and what the last one said — so the E24 check
belongs there, phrased as a refusal:

> *this sequence files study X under a title the last sequence did not use.*

**No new dependency in any direction**: it reads `Submission`'s own frozen
columns. And it lands where every other EPIC-007a refusal lands, facing the
authority whose rule it is.

> **Not implemented in S003, and the omission is deliberate.** The refusal needs
> a second sequence filing the same study, and its message needs to name what
> the previous sequence said. It is the first thing EPIC-019's successor owes,
> and it is recorded here rather than left to be rediscovered by a reviewer.

### 3. `append` is derived, and nothing records that an STF existed

ADR-054 §5 predicted this and left it open. The chain is
`(study, eCTD element) → most recent sequence that filed one`, and it is
computed by asking which earlier sequences had a placement reporting that study
in that element. **An STF is a projection; a record that one existed would be
the file in another shape.**

Latest wins, not original: *"you should not continually 'append' to the original
STF"* (E29 §V).

### 4. Two oracles, because they answer different questions

EPIC-007a's Level 2a rested on one parser. It is not sufficient here, and the
DTD says so itself — `file-tag/@name` is `CDATA`, so `xmllint` validates
`name="sinopsis"` (**E34**).

| Oracle | Question | Verdict on a misspelling |
|---|---|---|
| `xmllint` + `ich-stf-v2-2.dtd` | is this **legal**? | valid |
| `xsltproc` + ICH stylesheet + `valid-values.xml` | is this **a word**? | one red row |

Both are third-party, machine-checkable, and shipped by ICH. **The rule
`ValidatorIndependenceTests` enforces is unchanged**: neither is referenced from
`src/`, the seam is still the filesystem, and both live in `tests/`. A second
oracle at the same seam is not a second dependency.

## Consequences

**Every published sequence carries the study identities it filed.** Two columns
on the placement, written once, never updated. A sequence published before
EPIC-019 has none — and is **refused by name** rather than back-filled from the
registry, which is the same call EPIC-007a made for sequences filed before
regulatory activities were recorded (E13).

**The package gains `util/style/`** — the folder ADR-054 recorded as absent. It
holds the stylesheet *and* `valid-values.xml`, because the stylesheet resolves
the vocabulary by a relative path and one without the other checks nothing.

**Retitling stays unguarded until the continuity refusal exists.** A study named
in one published sequence can still be renamed, and the next sequence will file
the new title. Recorded as owed rather than presented as safe.

## Revisit when

- **A second sequence files the same study** — the case §2's refusal is for, and
  the one that would demonstrate the gap rather than describe it.
- **A blueprint seeds 4.2.3.1, 4.2.3.2, 4.2.3.4.1 or 5.3.5.1.** Those four need
  `category` — species, route, duration, type-of-control — and a `Study` holds
  none of them. Generation refuses those sections today; that refusal is
  ADR-056 §3's admission rule about to fire, with a workflow behind it.
- **An authority publishes a `file-tag` value in two realms.** The placement
  stores the tag alone and derives `info-type` from it; `FileTagVocabularyTests`
  asserts the uniqueness that makes the derivation honest, and its failure is
  the signal to add a column.
- **A third artifact needs the same treatment.** Freeze-then-project has now
  been used for a submission's own metadata (ADR-047) and for a fact from
  another context (here). A third would be the point to ask whether it is a
  pattern with a name.
