# ADR-053 — An Instance Qualifier Belongs To The Placement, Not To The Section

**Status:** Accepted · **Date:** 2026-08-03 ·
**Related:** [ADR-045](ADR-045-the-cumulative-dossier-and-the-derived-delta.md) (§5 — an operation is a fact about a placement, which is the same shape of claim),
[ADR-052](ADR-052-where-a-specification-stops-naming-directories.md) (the last time the blueprint met something a specification would not give it),
[ADR-018](ADR-018-rule-of-three.md) (why this is written before either case is solved),
[E17, E18](../evidence/README.md)

## Context

The blueprint answers **where a document belongs**: a tree of `TemplateSection`,
each carrying a folder and, since S005, an element name in each backbone. That
model has held for two stories of eCTD rendering.

EPIC-007a S005 and S006 each hit something it could not express, independently,
in different backbones, a day apart.

| | The element | What the DTD demands | Evidence |
|---|---|---|---|
| **S005** | `m3-2-s-drug-substance`, `m2-3-s-drug-substance` | `substance` **and** `manufacturer`, `#REQUIRED`, on a node declared `*` | **E17** |
| **S005** | `m2-7-3-summary-of-clinical-efficacy`, `m5-3-5-reports-of-efficacy-and-safety-studies` | `indication`, `#REQUIRED`, likewise `*` | **E17** |
| **S006** | `m1-1-forms` | contains `form*`, not `leaf*`; each `form` carries `form-type` `#REQUIRED` | **E18** |

Read together they are one finding, not two. **Each of these locations occurs
more than once in a real dossier**, and the specification requires a value that
says *which occurrence this is*: which substance, whose manufacture, which
claimed indication, which form.

RegOS models each as a **single** section — 3.2.S *Drug Substance*, 1.1 *Forms* —
because that is what the CTD's outline says, and the outline is the smallest
faithful model of it. **The outline is not what the backbone encodes.** A CTD
table of contents lists 3.2.S once; a dossier for a two-substance product
contains it twice, and the two are told apart by a fact about the substance, not
about the section.

The asymmetry inside E17 makes the point sharper than a rule would: the drug
**product** equivalents declare the same attributes `#IMPLIED`. ICH insists a
substance node be identified and merely permits it for a product. No rule
derivable from section codes produces that.

## Decision

> **A qualifier that distinguishes one occurrence of a section from another is a
> property of the document's placement into that section — never of the section
> itself, and never of the renderer.**

### 1. Not on `TemplateSection`

The blueprint is **versioned regulatory knowledge shared by every filing**
(ADR-045 §2, ADR-052). "Which substance" is not shared by every filing; it
differs between two sequences bound to the same blueprint version, and between
two documents in the same sequence. Putting it on the section would either
freeze one applicant's substance into reference data or force a blueprint
version per product.

### 2. Not defaulted in the renderer

A renderer that emitted `fdaft1` because today's seed happens to contain one
form would be **baking regulatory knowledge into code** — the precise thing this
project exists to avoid, and the thing [ADR-052](ADR-052-where-a-specification-stops-naming-directories.md)
refused for a value with far less consequence. A directory name RegOS invents is
merely unconventional. A `substance` or a `form-type` RegOS invents is **a claim
about a filing, in a file a regulator reads**.

### 3. Until it is modelled, generation refuses — by name

Both are refused today, and the refusals say which fact is missing rather than
that a package cannot be built:

- `SectionNeedsAFactRegOsDoesNotHold` — E17's four elements.
- E18's `form-type` is refused the same way, on the same error.

**This is a third kind of gap**, and the epic's two existing kinds do not cover
it:

| | Closed by |
|---|---|
| a gap in our history | asking whoever filed the sequence — unrecoverable if nobody knows (E13) |
| a gap in what we have read | reading a specification |
| **a gap in what we model** | **modelling something new** |

The specification has been read. It asks for a fact the domain does not carry.

### 4. This ADR does not model it

No schema, no field, no migration. It records **where the boundary falls** so
that the eventual design is not discovered a third time, and so neither E17 nor
E18 is solved locally by whoever meets it next.

[ADR-018](ADR-018-rule-of-three.md) is why. Two demonstrated needs is not three,
and the two differ in shape — E17 qualifies a *container* the leaf sits inside,
E18 replaces the leaf with a *wrapper element* that has children. Abstracting a
"placement qualifier" across both now would be designing for a third case
nobody has seen.

## Consequences

**Any document placed in 3.2.S, 1.1, or a seeded efficacy section is refused
today.** For the FDA IND blueprint that is 3.2.S and 1.1 — the whole of the
seeded Module 3 substance branch, and Forms. That is the honest position and it
is visible rather than silent.

**The blueprint stays a description of locations.** It gained folder and element
columns because those are facts about a location. A qualifier is not, and this
ADR is what stops the third column being added by analogy.

**When it is modelled, the rendering algorithm does not change.** The renderer
already walks placements; it will read a qualifier from the placement it is
already holding.

## Revisit when

- **A third case appears** — a fifth keyed element, or a second wrapper like
  `form`. Three demonstrated needs is when ADR-018 permits the abstraction, and
  the third case is what tells us whether E17's and E18's shapes are one concept
  or two.
- **A real IND is attempted end to end**, and the refusal blocks a filing rather
  than a fixture. That converts this from a known limit into a priority.
- **A user asks to file two drug substances**, which is the smallest business
  request that cannot be met without it.

> If none of those happens, this ADR has cost one file and prevented two local
> guesses. That is the trade it is making.
