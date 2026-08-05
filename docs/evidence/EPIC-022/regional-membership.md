# Regional membership — EU, ICH, PIC/S

**Retrieved 2026-08-05, before the seed was written.**

**What this records:** which of the eight seeded countries belongs to which
regulatory grouping — and, more usefully, **two rows where a careful guess would
have been wrong.**

## Why this had to be fetched

Every other vocabulary in RegOS names one body that owns it: EDQM, UCUM, ISO.
**Regions have no single authority.** Each grouping publishes only its *own*
membership, so this is three sources rather than one, each read separately.

It is also the class of fact that *feels* recallable and is not. Membership
changes — the United Kingdom was EU — and the member/observer boundary is
invisible from outside.

## The sources

| Grouping | Source | Level |
|---|---|---|
| **EU** | [European Union — EU countries](https://european-union.europa.eu/principles-countries-history/eu-countries_en), 27 member states | **3** |
| **ICH** | [ICH — Members & Observers](https://www.ich.org/page/members-observers) | **3** |
| **PIC/S** | [PIC/S — Members](https://picscheme.org/en/members), with accession dates | **3** |

Level 3 throughout: the authority's published listing, read. Nothing
machine-readable was parsed and no third party checked the result.

## The eight, as seeded

| | EU | ICH | PIC/S | Recorded |
|---|---|---|---|---|
| **US** | — | ✅ Founding Regulatory Member (FDA) | ✅ 2011 | ICH, PIC/S |
| **CA** | — | ✅ Standing Regulatory Member | ✅ 1999 | ICH, PIC/S |
| **GB** | ❌ **not since 2020** | ✅ Regulatory Member (MHRA) | ✅ 1999 | ICH, PIC/S |
| **DE** | ✅ since 1958 | ✅ *via the EU* | ✅ 2000 | EU, ICH, PIC/S |
| **FR** | ✅ since 1958 | ✅ *via the EU* | ✅ 1997 | EU, ICH, PIC/S |
| **JP** | — | ✅ Founding Regulatory Member | ✅ 2014 | ICH, PIC/S |
| **AU** | — | ❌ **Standing Observer** (TGA) | ✅ 1995 | PIC/S |
| **IN** | — | ❌ **Observer** (CDSCO) | ❌ **not a participant** | *(none)* |

**ASEAN and GCC are in the vocabulary and have no members among the eight** —
none of these countries is in South-East Asia or the Gulf. The entries exist so
the list is not silently Western-only.

## The two corrections

> **Australia and India are ICH *observers*, not members.**

Both would have been recorded as ICH members from memory. They are not, and the
distinction is regulatory rather than clerical: an observer does not adopt ICH
guidelines by obligation, so *"do ICH guidelines apply here?"* — the question
this field exists to answer — gets the opposite answer for both.

**India therefore belongs to none of the five**, which makes the empty
collection a *recorded* answer rather than an unfilled field, and gives the
model a real case to prove rather than a hypothetical one.

## One derived claim, stated as derived

**Germany and France are tagged `ICH`, and neither appears on ICH's member
list.** The European Commission is the member; its member states adopt ICH
guidelines through the EU. The tag is therefore a *consequence* of EU
membership, not a register row.

It is recorded that way — rather than left to a derivation rule — because no
consumer asked for one, and a rule would have to be re-derived by every reader.
The seed file says the same thing at the point of use.

## What would change this

| If this turns out wrong | What breaks |
|---|---|
| A membership is wrong today | Every question keyed on grouping — *"which of our markets are in the EU?"*, and eventually which blueprint applies (EPIC-020) |
| **A membership changes** | **Nothing detects it.** RegOS records today's answer with **no effective dating**, deliberately — the trigger to add it is somebody asking what was true in 2019, and nobody has |
| The seed widens past eight | This entry stops covering it; each new country needs its own three lookups |

## The distinction this entry protects

> **RegOS does not hold a membership register.** It holds eight rows read off
> three published pages on one day, for a demonstration.

The seed file says so in its own docstring. Widening it means fetching again —
which is the honest cost of not holding a register, and the same position
[E36](iso-3166-1.md) takes on ISO 3166-1.
