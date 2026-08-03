# ADR-054 — A Study Tagging File Is A Projection Over A Study RegOS Does Not Yet Have

**Status:** Accepted · **Date:** 2026-08-03 ·
**Related:** [ADR-049](ADR-049-generation-derives-transmission-creates.md) (the package is a projection — this tests that claim harder than the ZIP did),
[ADR-053](ADR-053-instance-qualifiers-belong-to-the-placement.md) (**its revisit trigger has fired**),
[ADR-045](ADR-045-the-cumulative-dossier-and-the-derived-delta.md) (§5 — the derived delta, whose shape this reuses),
[E21, E29](../evidence/README.md)

## Context

**E21** recorded that FDA requires a Study Tagging File for every file in eCTD
sections 4.2.x and 5.3.1.x–5.3.5.x, and that the FDA IND blueprint seeds 4.2.1,
4.2.2 and 4.2.3. Every IND has Module 4 content, so this blocks generation.

The ICH M2 specification that defines an STF was obtained on 2026-08-03 and read
in full (**E29**). Before it, we knew what an STF *does*. It says what one *is*:

> *"the eCTD backbone files do not contain enough information on the subject
> matter of several documents (e.g., study report documents) to support certain
> regulatory uses. **This additional information is provided in the STF.**"*

And it carries no files. Its content is references:

```xml
<doc-content xlink:href="../../../../../index.xml#a101">
  <file-tag name="synopsis" info-type="ich"/>
</doc-content>
```

`doc-content` points at a **leaf ID in `index.xml`** — a leaf the backbone already
holds.

## Decision

> **A Study Tagging File is a projection over the placements in one sequence that
> belong to one study. It is not a document, not a section, and not a new kind of
> file RegOS stores.**

### 1. ADR-049's deletion test still passes — and this is the harder case

*"If deleting the file loses no business information, the file is not part of the
domain model."* Delete a generated STF and nothing is lost **provided** three
things are held elsewhere:

| | Where it must come from |
|---|---|
| the leaves it references | the sequence's own placements — already frozen at publish |
| which study each belongs to, and what that study *is* | **a Study. RegOS has none** |
| what role each document plays in that study report | **a `file-tag` per placement. RegOS has none** |

The ZIP tested ADR-049 against a file that merely repackaged what the submission
held. The STF tests it against a file that requires **facts the submission does
not hold at all** — and the thesis survives, because the answer is to hold the
facts, not to store the file.

### 2. A `file-tag` is an instance qualifier, and it is the third shape

ADR-053 said a qualifier distinguishing one occurrence of a section from another
belongs to the **placement**. `file-tag` answers *what role does this document
play in this study report* — synopsis, protocol, randomisation scheme, CRF — for a
document already placed in a section. **It is that, exactly.**

**ADR-053's revisit trigger has fired.** It said to revisit when a third case
appeared, and that the third case *"is what tells us whether E17's and E18's
shapes are one concept or two"*. Three shapes are now visible:

| | Shape | Vocabulary |
|---|---|---|
| **E17** | an attribute keying a **repeatable container** the leaf sits inside | free text (`substance`, `manufacturer`, `indication`) |
| **E18** | a **wrapper element** replacing the leaf, carrying required metadata | `form-type.xml` — **not held** |
| **E29** | a **classification of the placement itself**, in a separate file | `file-tag`, ~40 values — **held** |

They are one concept: *a fact about a placement, required by the format, absent
from the domain, and unrecoverable by any rule over sections.*

**The abstraction is still not built here.** Two of the three vocabularies are
files this repository does not hold, and abstracting over values we cannot yet
write would be designing against a guess — the error this epic has corrected
twice already. What ADR-018 permits after three demonstrated needs it does not
compel before the third need can be *expressed*.

### 3. A Study is a business entity, and naming it is not modelling it

The specification requires, per study:

- **`study-id`** — *"the internal alphanumeric code used by the sponsor to
  unambiguously identify this study"*. Not RegOS's id. The sponsor's.
- **`title`** — *"the full title of the study, not the title of each individual
  document"*.
- **`category`** — only for **4.2.3.1, 4.2.3.2, 4.2.3.4.1 and 5.3.5.1**: species,
  route-of-admin, duration, type-of-control, from closed ICH lists.

A study is a thing in the world that documents are *about*. It is not a
`Submission`, not a `ProductDocument`, and not a `TemplateSection`.

**This ADR does not decide which context owns it.** That is a bounded-context
question, and repository canon requires an ADR of its own for a new context or a
new cross-context dependency. What is decided is that it exists, and that the
three facts above are its minimum.

### 4. The mapping is `(study, eCTD element) → STF`, not `study → STF`

> *"there are certain situations where one study could generate more than one STF
> representation"* — distinct time-point analyses with their own lifecycles, or a
> study supporting two CTD subsections.

So the grouping key is a **pair**, and even that may be split deliberately by the
filer. Any model that assumes one STF per study is wrong on the specification's
own worked examples.

### 5. The lifecycle is derived, not stored — the same shape as ADR-045's delta

This is the part that looked like it would break the projection thesis. The STF's
leaf carries `operation="new"` for the first STF for a study in an element and
`"append"` thereafter, with `modified-file` pointing at **the most recently
submitted STF leaf** — statements about *earlier sequences*.

**RegOS already answers exactly this shape of question.** ADR-045 derives
`operation` for a document by comparing this sequence's placements against the
previous published sequence's. The STF asks the same question keyed differently:

| | Key | Question |
|---|---|---|
| ADR-045 | (document, section) | was this document here before? |
| **STF** | **(study, eCTD element)** | **was there an STF for this study here before?** |

Both are answered from **frozen publication facts**, not from a stored artifact.
The STF's leaf ID must therefore be derived — deterministically, from the sequence
and the pair — so that regenerating produces the same bytes and a later sequence
can point at it.

> **The STF is where `append` is mandated**, and E10 now records that as a third
> scope rather than a contradiction. FDA says avoid it for documents, forbids it
> for datasets, and the ICH specification *requires* it here.

### 6. Until a Study is modelled, generation refuses — by name

A document placed in 4.2.x or 5.3.1.x–5.3.5.x cannot be written into a package
today. This is the third refusal category again — *the specification asks for a
fact the domain does not carry* — and it now covers the whole of Module 4.

## Consequences

**The FDA IND blueprint cannot produce a complete package.** 4.2.1, 4.2.2 and
4.2.3 are seeded; every IND has nonclinical content; all of it needs an STF. This
is larger than E17's 3.2.S and E18's 1.1 combined, and it is not a Module 1
problem, so no amount of regional work reaches it.

**S007's Level 2a claim gets a third file to validate.** `index.xml`,
`us-regional.xml`, and an STF per study — and `ich-stf-v2-2.dtd` is **not held**,
so the STF cannot yet be checked by the oracle the other two are checked by.

**The `util/` folder gains a second occupant.** The STF specification puts
`ich-stf-v2-2.dtd` in `util/dtd/` and its stylesheet in `util/style/` — the
`util/style/` that S004 noted as absent and left alone.

## Revisit when

- **A Study is modelled**, which is the precondition for everything above and
  wants its own ADR for where it lives.
- **`ich-stf-v2-2.dtd` and `file-tag`'s regional values are held**, at which point
  an STF can be generated *and validated*, and the three qualifier shapes can be
  compared with all their vocabularies present rather than two of three.
- **A filer needs two STFs for one study** — the case §VI describes, and the one
  that would falsify any single-STF-per-study shortcut taken in the meantime.
