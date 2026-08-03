# ICH eCTD v3.2.2 — Appendix 4, *File Organization for the eCTD*

**Partial.** Transcribed 2026-08-03 from the founder's extract of the v3.2.2 PDF.
Only the rows RegOS needs — those that name a **directory** — are reproduced;
the `File` rows, the `Comment` prose and the element names are not, because a
section's folder is the only thing the blueprint stores.

---

## Read this before using any value below

### 1. For Modules 2–5 these names are **recommended, not mandatory**

Appendix 4 says so in its own preamble:

> *"The file and folder names shown within modules 2-5 are **not mandatory, but
> recommended**, and can be further reduced or omitted to avoid path length
> issues."*

**So Appendix 4 is Level 3, not Level 2a.** It is the specification's own
recommendation, which outranks FDA's worked examples as a source of convention —
but it is not a normative artifact a parser can check us against, and a package
that departs from it is not thereby invalid. The register's ordering still holds
(*"2a beats 3 on legality; 3 beats 2a on convention"*); this is simply better
Level 3 than the examples are.

Module 1's entries read differently — see §3.

### 2. Italic placeholders cannot be recovered from plain text

> *"Where file and folder names are presented in italics applicants would
> substitute these with appropriate file names in accordance with their own
> naming conventions."*

**The extract carries no italics**, so a placeholder is indistinguishable from a
canonical name except by reading the comment beside it. Four are identifiable
from their comments and are marked **⟨example⟩** below; there may be others.

### 3. Appendix 4 stops at the door of Module 1

Entries 3–7 give `m1` and one regional directory per region — `m1/eu`, `m1/jp`,
`m1/us`, `m1/xx` — and then say:

> *"Refer to regional guidance for details."*

**There are no Module 1 subsection folders here.** For an FDA IND blueprint,
whose sections are almost entirely `1.x`, Appendix 4 supplies `m1/us` and stops.
The rest is FDA's own guidance, which this repository does not hold.

### 4. What is missing from this extract

The source paste was truncated at 50,000 characters, mid-entry at **#203**.

| | |
|---|---|
| Module 1 regional roots (3–7) | ✅ complete |
| Module 2 (8–35) | ✅ complete |
| Module 3 (36–136) | ✅ complete |
| Module 4 (137–203) | ⚠️ **truncated** — #203's directory value is cut off |
| Module 5 | ✖ **absent** |
| `util/` | ✖ **absent** |

---

## Directory rows

`#` is Appendix 4's own sequential reference number, which the appendix warns
*"can change with each version"* — recorded to make the row findable, not as an
identifier.

### Backbone and Module 1

| # | Section | Directory |
|---|---|---|
| 3 | 1 | `m1` |
| 4 | — | `m1/eu` |
| 5 | — | `m1/jp` |
| 6 | — | `m1/us` |
| 7 | — | `m1/xx` *(ISO-3166-1 two-character code)* |

### Module 2

| # | Section | Directory |
|---|---|---|
| 8 | 2 | `m2` |
| 9 | 2.2 | `m2/22-intro` |
| 11 | 2.3 | `m2/23-qos` |
| 17 | 2.4 | `m2/24-nonclin-over` |
| 19 | 2.5 | `m2/25-clin-over` |
| 21 | 2.6 | `m2/26-nonclin-sum` |
| 29 | 2.7 | `m2/27-clin-sum` |

**Every other Module 2 section has a `File` row and no `Directory` row** — 2.3.S,
2.6.1…2.6.7, 2.7.1…2.7.6 all write into their parent's folder. They contribute
**no directory of their own**, which is a known fact and not a missing one.

### Module 3

| # | Section | Directory |
|---|---|---|
| 36 | 3 | `m3` |
| 37 | 3.2 | `m3/32-body-data` |
| 38 | 3.2.S | `m3/32-body-data/32s-drug-sub` |
| 39 | 3.2.S *(per substance/manufacturer)* | `…/32s-drug-sub/substance-1-manufacturer-1` **⟨example⟩** |
| 40 | 3.2.S.1 | `…/32s1-gen-info` |
| 44 | 3.2.S.2 | `…/32s2-manuf` |
| 51 | 3.2.S.3 | `…/32s3-charac` |
| 54 | 3.2.S.4 | `…/32s4-contr-drug-sub` |
| 55 | 3.2.S.4.1 | `…/32s4-contr-drug-sub/32s41-spec` |
| 57 | 3.2.S.4.2 | `…/32s4-contr-drug-sub/32s42-analyt-proc` |
| 61 | 3.2.S.4.3 | `…/32s4-contr-drug-sub/32s43-val-analyt-proc` |
| 65 | 3.2.S.4.4 | `…/32s4-contr-drug-sub/32s44-batch-analys` |
| 67 | 3.2.S.4.5 | `…/32s4-contr-drug-sub/32s45-justif-spec` |
| 69 | 3.2.S.5 | `…/32s5-ref-stand` |
| 71 | 3.2.S.6 | `…/32s6-cont-closure-sys` |
| 73 | 3.2.S.7 | `…/32s7-stab` |
| 77 | 3.2.P | `m3/32-body-data/32p-drug-prod` |
| 78 | 3.2.P *(per product)* | `…/32p-drug-prod/product-1` **⟨example⟩** |
| 79 | 3.2.P.1 | `…/32p1-desc-comp` |
| 81 | 3.2.P.2 | `…/32p2-pharm-dev` |
| 83 | 3.2.P.3 | `…/32p3-manuf` |
| 89 | 3.2.P.4 | `…/32p4-contr-excip` |
| 90 | 3.2.P.4 *(per excipient)* | `…/32p4-contr-excip/excipient-1` **⟨example⟩** |
| 97 | 3.2.P.5 | `…/32p5-contr-drug-prod` |
| 98 | 3.2.P.5.1 | `…/32p5-contr-drug-prod/32p51-spec` |
| 100 | 3.2.P.5.2 | `…/32p5-contr-drug-prod/32p52-analyt-proc` |
| 104 | 3.2.P.5.3 | `…/32p5-contr-drug-prod/32p53-val-analyt-proc` |
| 108 | 3.2.P.5.4 | `…/32p5-contr-drug-prod/32p54-batch-analys` |
| 110 | 3.2.P.5.5 | `…/32p5-contr-drug-prod/32p55-charac-imp` |
| 112 | 3.2.P.5.6 | `…/32p5-contr-drug-prod/32p56-justif-spec` |
| 114 | 3.2.P.6 | `…/32p6-ref-stand` |
| 116 | 3.2.P.7 | `…/32p7-cont-closure-sys` |
| 118 | 3.2.P.8 | `…/32p8-stab` |
| 122 | 3.2.A | `m3/32-body-data/32a-app` |
| 123 | 3.2.A.1 | `m3/32-body-data/32a-app/32a1-fac-equip` |
| 127 | 3.2.A.2 | `m3/32-body-data/32a-app/32a2-advent-agent` |
| 131 | 3.2.A.3 | `m3/32-body-data/32a-app/32a3-excip-name-1` **⟨example⟩** |
| 132 | 3.2.R | `m3/32-body-data/32r-reg-info` |
| 133 | 3.3 | `m3/33-lit-ref` |

### Module 4 — truncated

| # | Section | Directory |
|---|---|---|
| 137 | 4 | `m4` |
| 138 | 4.2 | `m4/42-stud-rep` |
| 139 | 4.2.1 | `m4/42-stud-rep/421-pharmacol` |
| 140 | 4.2.1.1 | `…/421-pharmacol/4211-prim-pd` |
| 144 | 4.2.1.2 | `…/421-pharmacol/4212-sec-pd` |
| 148 | 4.2.1.3 | `…/421-pharmacol/4213-safety-pharmacol` |
| 152 | 4.2.1.4 | `…/421-pharmacol/4214-pd-drug-interact` |
| 156 | 4.2.2 | `m4/42-stud-rep/422-pk` |
| 157 | 4.2.2.1 | `…/422-pk/4221-analyt-met-val` |
| 161 | 4.2.2.2 | `…/422-pk/4222-absorp` |
| 165 | 4.2.2.3 | `…/422-pk/4223-distrib` |
| 169 | 4.2.2.4 | `…/422-pk/4224-metab` |
| 173 | 4.2.2.5 | `…/422-pk/4225-excr` |
| 177 | 4.2.2.6 | `…/422-pk/4226-pk-drug-interact` |
| 181 | 4.2.2.7 | `…/422-pk/4227-other-pk-stud` |
| 185 | 4.2.3 | `m4/42-stud-rep/423-tox` |
| 186 | 4.2.3.1 | `…/423-tox/4231-single-dose-tox` |
| 190 | 4.2.3.2 | `…/423-tox/4232-repeat-dose-tox` |
| 194 | 4.2.3.3 | `…/423-tox/4233-genotox` |
| 195 | 4.2.3.3.1 | `…/4233-genotox/42331-in-vitro` |
| 199 | 4.2.3.3.2 | `…/4233-genotox/42332-in-vivo` |
| **203** | **4.2.3.4** | ✖ **cut off mid-entry — the extract ends here** |

### Module 5

✖ **Not received.**

---

## One extraction artifact, noted rather than silently repaired

Entry **61** arrives as:

```
Element m3-2-s-4-3-validation-of-analytical-procedures (name, manufacturer)
```

`(name, manufacturer)` belongs on the `Title` line, not the element name — the
same class of PDF line-wrap damage that had to be repaired when Appendix 8 was
transcribed. The directory value on that row is unaffected.

---

## Before any of this is seeded

**Nothing here has been loaded into a blueprint yet.** These values are Level 3,
partial, and carry at least four placeholder names that only a comment
distinguishes from real ones. A seeded value is one a package will be built
from, so each row needs to be read against its comment first — and the missing
Module 5 and `util/` rows obtained.
