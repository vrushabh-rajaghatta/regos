# The stability conditions a market accepts

**Retrieved 2026-08-05, before the seed was written — and it changed the model.**

**What this records:** the long-term stability testing condition each seeded
market accepts, and the reason RegOS holds **conditions** rather than **climatic
zones**.

## The finding that shaped the design

EPIC-022's plan asked for a climatic zone per country — ICH zones
**I, II, III, IVA, IVB** — matched against a zone on the pack's shelf life.
Reading the source killed that design before a line of it was written.

**The authoritative table publishes storage conditions, not zone letters.** WHO
lists, per member state, the long-term testing condition that state accepts. It
does **not** publish a zone classification per country. And **ICH withdrew
Q1F**, which was the guideline zone letters came from — so no current ICH
document carries the mapping either.

> **India decides it.** WHO's table says **30 °C/70% RH**. That is *neither*
> Zone IVA (30 °C/65% RH) *nor* Zone IVB (30 °C/75% RH).

A `Zone = IVB` column would therefore not have held WHO's data. It would have
held **RegOS's interpretation of WHO's data**, with nothing to check it against
and no way for a reader to tell the two apart. So RegOS stores the condition, and
treats *"Zone IVB"* as a word a person may say rather than a value a database
holds ([EPIC-022 D6](../../product/epics/EPIC-022-country-depth.md#d6--amended-in-place-before-a-line-of-s004-was-written)).

Had this not been read, the natural implementation would have been a zone column
seeded from memory — and **India would have been wrong in a way nothing in the
system could detect**, because there is no published mapping to check it against.

## The source

| | |
|---|---|
| The table | WHO — [*Stability conditions for WHO Member States by Region*](https://cdn.who.int/media/docs/default-source/medicines/norms-and-standards/guidelines/regulatory-standards/trs953-annex2-appendix1-stability-conditions-table-2018.pdf), **update March 2021**. Previously Table 2 in Annex 2 to WHO Technical Report Series No. 953 |
| Not used | **ICH Q1A(R2)** — the epic's original prerequisite. It specifies storage conditions for stability studies; it does not map countries to zones. **ICH Q1F, which carried zone letters, has been withdrawn** |

**Level 3** — the authority's published table, read.

## The eight, as seeded

| | Accepts | WHO footnote |
|---|---|---|
| US · CA · GB · DE · FR · JP | 25 °C/60% RH **or** 30 °C/65% RH | ¹ |
| **AU** | 25 °C/60% RH or 30 °C/65% RH | **²** |
| **IN** | **30 °C/70% RH** | ¹ |

**The "or" is WHO's own word**, and it is why the match is an *overlap* rather
than an equality: a pack tested at either condition is supported in those seven
markets.

### The footnotes are not decoration — they are per-row evidence strength

WHO marks each row with how the value reached the table, and the three levels are
**not** interchangeable:

| | Marking | Where the value came from |
|---|---|---|
| **¹** | bold | regional harmonization groups (ASEAN, ICH, GCC) and **official communications from national medicines regulatory authorities to WHO** |
| **²** | normal type | collated at the **13th ICDRA, 16–18 September 2008, Berne** |
| **³** | italic | provided by IFPMA from meteorological references (Ahrens 2001; Kottek *et al.* 2006, Köppen-Geiger) |

> **⚠ Australia is footnote ², and the other seven are footnote ¹.** Its value is
> a 2008 conference collation rather than a regulator's own statement to WHO —
> **the same condition, on materially weaker evidence, and eighteen years older**.
> Stated plainly rather than left for a reader to assume the whole table was
> sourced alike. Footnote ³ rows are weaker still and none of the eight carries
> one.

## What this does not record

**RegOS holds no register of stability conditions.** Eight hand-verified rows are
not the table, and the table is not a register — WHO names one condition per
country and does not publish the set of conditions, exactly as each regional
grouping publishes only its own membership ([E37](regional-membership.md)). The
four terms in `StabilityVocabulary` are therefore RegOS's own choice of which are
worth recording, marked `regos-internal` (ADR-058 §6). WHO's table names others —
30 °C/35% RH, 30 °C/80% RH — and adding a market that accepts one is a data
change to that list.

**And no zone letter, anywhere.** Not in a column, not in a seed, not in a
rendered string. If a screen ever needs the word *"Zone IVB"* it is an alias
computed at the edge, because a persisted one would be RegOS publishing a
classification it did not read.

## What would change this

| If this turns out wrong | What breaks |
|---|---|
| A market's accepted conditions are wrong | The market view advises wrongly about which packs are supported. **Nothing is refused and nothing is filed incorrectly** — the verdict is reported, never enforced (D6) |
| WHO restates the table | Every row here is re-read. The table is dated *update March 2021*; nothing in RegOS detects a newer one |
| Australia's row proves stale | It is the weakest row in the seed (footnote ², 2008) and the first to re-check. Fix by reading TGA's own statement, the way Canada's languages were read from Health Canada ([E38](label-languages.md)) |
| A regulator accepts a condition RegOS does not list | The seed cannot express it. Add the term to `StabilityVocabulary` — a data change, not a model change |
| Somebody wants the match to stop being *"any overlap"* | One method changes: `Country.AcceptsStabilityDataFrom`. The rule was deliberately not written out at its call sites |

## The distinction this entry protects

> **RegOS stores what the authority published, not what the authority's data
> implies.** A zone letter would have been an inference we authored; a condition
> is a fact we read.
