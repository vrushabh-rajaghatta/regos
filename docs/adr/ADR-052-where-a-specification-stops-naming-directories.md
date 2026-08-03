# ADR-052 — Where A Specification Stops Naming Directories, RegOS Names Them — And Says So

**Status:** Accepted · **Date:** 2026-08-03 ·
**Related:** [ADR-045](ADR-045-the-cumulative-dossier-and-the-derived-delta.md) (§2 — freeze what a recomputation would rewrite),
[ADR-051](ADR-051-two-more-lookups-and-what-a-lookup-is.md) (the last time a value's provenance decided a design),
[ADR-018](ADR-018-rule-of-three.md),
[docs/evidence/](../evidence/README.md) (the level taxonomy this leans on)

## Context

EPIC-007a S004 has to write a document to a path, and discovered that **no
specification in this repository says what that path is** for the sections RegOS
actually ships.

Three artifacts were obtained, and each said something different about its own
authority:

| Artifact | What it gives | What it says about itself |
|---|---|---|
| **ICH Appendix 4** | 113 CTD sections → directory | *"not mandatory, but recommended"* (Modules 2–5); italic names use the applicant's *"own naming conventions"* |
| **ICH Appendix 2** | naming **rules** — lowercase `a-z0-9-`, ≤64 a segment, ≤230 a path | binding |
| **FDA Module 1 Comprehensive ToC v2.3.2** | the Module 1 heading hierarchy | a table of *contents*, with **no directory column** |

Appendix 4 defers Module 1 to *"regional guidance"*; the regional guidance
supplies headings, not folders. So the eight Module 1 sections in the seeded FDA
IND blueprint — which is to say the whole first vertical — have no prescribed
directory anywhere.

**Three ways to proceed were rejected before this ADR was written**: deriving a
folder from the section code, flattening every leaf to its module root, and
reading folder names out of the regional DTD's element names. The third was the
most tempting, because the string would have been FDA's rather than ours — and
it fails outright: **11 Module 1 element names exceed Appendix 2's 64-character
segment limit, reaching 94.**

## Decision

### 1. Where nothing prescribes a directory name, RegOS generates one

> **Where an authority specification does not prescribe a directory name, RegOS
> generates one using the naming convention ICH Appendix 4 itself
> demonstrates: the CTD section identifier with its dots removed, followed by a
> concise kebab-case slug of the title.**

| Section | RegOS generates |
|---|---|
| `1.2` | `12-cover-letters` |
| `1.14.4` | `1144-investigational-drug-labeling` |
| `1.14.4.1` | `11441-invest-brochure` |

**This is not invention, and it is not evidence either.** Appendix 4's own
values follow exactly this shape — `3.2.S.4.1` → `32s41-spec`, `4.2.3.3.1` →
`42331-in-vitro` — so RegOS is continuing a visible convention where the
specification stops supplying values, rather than importing a foreign one. The
result satisfies Appendix 2, which *is* binding.

**The licence to choose comes from the specification, not from convenience.**
Appendix 4 states that applicants substitute names *"in accordance with their own
naming conventions"*. That sentence is what makes this a decision RegOS is
entitled to make; without it, generating a name would be inventing a
requirement.

### 2. Every folder records where it came from

> **A folder name and its provenance travel together or not at all.**

`TemplateSection` carries `EctdFolderSource`:

| | |
|---|---|
| `IchAppendix4` | the specification's own table |
| `RegionalSpecification` | an authority's own — **nothing carries this yet** |
| `RegOsConvention` | decision 1 |

**This is the load-bearing half of the ADR.** EPIC-007a has kept *evidence*,
*derived implementation* and *RegOS convention* apart in its documents since
Task 1; the moment RegOS began generating names, they had to be kept apart in the
data too. A single unqualified string would let a value we chose read exactly
like one ICH published.

> **Read the source column as the blast radius**, exactly as the evidence
> register's *Relied on by* column is read. If ICH restates Appendix 4, every
> `IchAppendix4` row is suspect and no other row is. If RegOS changes its own
> convention, the reverse. One column answers both questions; a bare string
> answers neither.

`RegionalSpecification` exists with nothing in it on purpose. If FDA is ever
found to prescribe Module 1 directory names, those rows must be distinguishable
from the ones RegOS chose in that specification's absence — and adding the member
later would mean re-examining every row already written.

### 3. Appendix 4's recommendations are emitted, not treated as optional

Appendix 4 says Modules 2–5 names are *recommended*. **RegOS emits them anyway.**

That the names are optional answers a regulatory question — *may an applicant
choose differently?* — not a product one. Emitting them buys deterministic
output, byte-identical regeneration (S004's acceptance criterion, and ADR-049's
projection thesis made testable), stable tests, and immediate Level 3
comparability against FDA's examples.

**A user wanting different directory names is asking for a different renderer
policy, not a different blueprint**, and no such policy is built until someone
asks (ADR-018).

### 4. A title is display; a folder is identity

`Title` and `EctdFolder` sit on the same entity and are not the same kind of
thing:

| | `Title` | `EctdFolder` |
|---|---|---|
| whose | the authority's, to restate | the specification's *or* RegOS's |
| changes when | wording changes | structure changes |
| a change means | a caption reads differently | **files move** |

FDA restated 1.13 once already (evidence E9), and wording can move again without
a single file moving with it. Both are frozen by the version they belong to —
but only one of them is part of what a regulator received.

**The name `CanonicalPath` was considered and rejected**, because decision 2
makes it false for a third of the rows: a value RegOS chose is not canonical, and
a field name asserting otherwise would undo the distinction the enum beside it
exists to draw.

## Consequences

- **Populating folders is a new blueprint version, never an `UPDATE`.** A
  published version is frozen (EPIC-007a S002), which is ADR-045 §2 applied to
  placement: a package regenerated under a rule that changed after transmission
  would put files somewhere other than where the authority received them.
- **Corrections batch into that version.** FDA's own sources say
  *"Investigational brochure"* and *"Cover letters"* where RegOS says
  *"Investigator's Brochure"* and *"Cover Letter"*. Wording and folders are both
  blueprint knowledge, so they land in one immutable version rather than two.
- **Rendering refuses on a null folder**, and the message distinguishes a
  historical gap from an evidence gap — the same two-refusal rule S003
  established for wire tokens.
- A `RegOsConvention` row **may not be cited as evidence** anywhere in
  `docs/evidence/`. It is what we chose in a specification's absence.

## Revisit When

- **FDA's eCTD Technical Conformance Guide is obtained.** If it prescribes
  Module 1 directory names, those rows become `RegionalSpecification` and
  decision 1 narrows. **If it prescribes placement but not naming — the likelier
  outcome — this ADR gets materially stronger**, because it can then rest on what
  the specification says *and* on what it declines to say, rather than on an
  absence nobody has confirmed is deliberate.
- **eCTD v4.0 (RPS) is targeted.** Directory naming is one of the things that
  changes with the format, which is why it is versioned data and not a method.
- **A second authority's CTD blueprint is seeded.** Modules 2–5 folders are
  ICH's and identical for everyone, so storing them per blueprint duplicates
  them. One blueprint exists today and ADR-018 forbids abstracting a duplication
  that has not happened.
