# ICH eCTD v3.2.2 — Appendix 4, *File Organization for the eCTD*

**Complete — entries 1–379.** Transcribed 2026-08-03 from the founder's extract
of the v3.2.2 PDF, supplied in two parts. Only the rows RegOS needs — those that
name a **directory** — are reproduced; the `File` rows, the `Comment` prose and
the element names are not, because a section's folder is the only thing the
blueprint stores.

**A transcription is not the publication.** Same standard as Appendix 8: if this
is wrong, everything resting on it is wrong and would look exactly as
convincing.

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
canonical name except by reading the comment beside it. **Six** are identifiable
from their comments and are marked **⟨example⟩** below; there may be others that
carry no comment to give them away.

### 3. Appendix 4 stops at the door of Module 1

Entries 3–7 give `m1` and one regional directory per region — `m1/eu`, `m1/jp`,
`m1/us`, `m1/xx` — and then say:

> *"Refer to regional guidance for details."*

**There are no Module 1 subsection folders here.** For an FDA IND blueprint,
whose sections are almost entirely `1.x`, Appendix 4 supplies `m1/us` and stops.
The rest is FDA's own guidance, which this repository does not hold.

### 4. Complete — entries 1–379

Supplied in two parts (the first truncated at #203) and reassembled here.

| | |
|---|---|
| Backbone (1–2), Module 1 regional roots (3–7) | ✅ |
| Module 2 (8–35) · Module 3 (36–136) · Module 4 (137–269) · Module 5 (270–369) | ✅ |
| `util/` (370–379) | ✅ |

**113 CTD sections have a directory.** What that covers of RegOS's own blueprint
is measured in §6.

### 5. Two more things the appendix says about itself

**The `util/` filenames are illustrative too** — #371's comment:

> *"File names in rows 372 - 379 are **illustrative only**. Please consult
> regional guidance for the current name and version of the files."*

So `util/dtd/ich-ectd-n.dtd` is a pattern, not a filename. The same comment
narrows what a package carries: *"it is not necessary to include regional
DTDs/Schemas other than the one for the region to which the application is being
made"* — for an FDA IND, the ICH DTD and `us-regional` only.

**Granularity differs between Modules 4 and 5.** Module 4 gives study reports as
**files** in the section's directory; Module 5 gives each study report its own
**directory** (#276: *"A directory should be created for each study and the
files associated with the study report should be organized within the
directory"*). A renderer that treats the two the same is wrong in one of them.

### 6. What it covers of the seeded FDA IND blueprint

Measured against the published versions, not estimated:

| | |
|---|---|
| distinct section codes in the blueprint | **40** |
| matched to an Appendix 4 directory | **27** |
| module roots — a coding mismatch, not a gap | **5** |
| **Module 1 subsections — the real gap** | **8** |

The five module roots are coded `M1`…`M5` here and `1`…`5` in Appendix 4. That
is RegOS's own naming choice, not missing evidence, and it means the mapping is
**not a plain string match** at the module level.

The eight that Appendix 4 genuinely does not reach are `1.1`, `1.2`, `1.3`,
`1.4`, `1.13`, `1.14`, `1.14.4`, `1.14.4.1` — **every Module 1 section in the
blueprint**, which is to say the whole of the first vertical.

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

### Module 4

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
| 203 | 4.2.3.4 | `…/423-tox/4234-carcigen` |
| 204 | 4.2.3.4.1 | `…/4234-carcigen/42341-lt-stud` |
| 208 | 4.2.3.4.2 | `…/4234-carcigen/42342-smt-stud` |
| 212 | 4.2.3.4.3 | `…/4234-carcigen/42343-other-stud` |
| 216 | 4.2.3.5 | `…/423-tox/4235-repro-dev-tox` |
| 217 | 4.2.3.5.1 | `…/4235-repro-dev-tox/42351-fert-embryo-dev` |
| 221 | 4.2.3.5.2 | `…/4235-repro-dev-tox/42352-embryo-fetal-dev` |
| 225 | 4.2.3.5.3 | `…/4235-repro-dev-tox/42353-pre-postnatal-dev` |
| 229 | 4.2.3.5.4 | `…/4235-repro-dev-tox/42354-juv` |
| 233 | 4.2.3.6 | `…/423-tox/4236-loc-tol` |
| 237 | 4.2.3.7 | `…/423-tox/4237-other-tox-stud` |
| 238 | 4.2.3.7.1 | `…/4237-other-tox-stud/42371-antigen` |
| 242 | 4.2.3.7.2 | `…/4237-other-tox-stud/42372-immunotox` |
| 246 | 4.2.3.7.3 | `…/4237-other-tox-stud/42373-mechan-stud` |
| 250 | 4.2.3.7.4 | `…/4237-other-tox-stud/42374-dep` |
| 254 | 4.2.3.7.5 | `…/4237-other-tox-stud/42375-metab` |
| 258 | 4.2.3.7.6 | `…/4237-other-tox-stud/42376-imp` |
| 262 | 4.2.3.7.7 | `…/4237-other-tox-stud/42377-other` |
| 266 | 4.3 | `m4/43-lit-ref` |

### Module 5

Note the granularity change: **each study report gets its own directory here**,
where Module 4 gave them as files (#276).

| # | Section | Directory |
|---|---|---|
| 270 | 5 | `m5` |
| 271 | 5.2 | `m5/52-tab-list` |
| 273 | 5.3 | `m5/53-clin-stud-rep` |
| 274 | 5.3.1 | `…/53-clin-stud-rep/531-rep-biopharm-stud` |
| 275 | 5.3.1.1 | `…/531-rep-biopharm-stud/5311-ba-stud-rep` |
| 279 | 5.3.1.2 | `…/531-rep-biopharm-stud/5312-compar-ba-be-stud-rep` |
| 283 | 5.3.1.3 | `…/531-rep-biopharm-stud/5313-in-vitro-in-vivo-corr-stud-rep` |
| 287 | 5.3.1.4 | `…/531-rep-biopharm-stud/5314-bioanalyt-analyt-met` |
| 291 | 5.3.2 | `…/53-clin-stud-rep/532-rep-stud-pk-human-biomat` |
| 292 | 5.3.2.1 | `…/532-rep-stud-pk-human-biomat/5321-plasma-prot-bind-stud-rep` |
| 296 | 5.3.2.2 | `…/532-rep-stud-pk-human-biomat/5322-rep-hep-metab-interact-stud` |
| 300 | 5.3.2.3 | `…/532-rep-stud-pk-human-biomat/5323-stud-other-human-biomat` |
| 304 | 5.3.3 | `…/53-clin-stud-rep/533-rep-human-pk-stud` |
| 305 | 5.3.3.1 | `…/533-rep-human-pk-stud/5331-healthy-subj-pk-init-tol-stud-rep` |
| 309 | 5.3.3.2 | `…/533-rep-human-pk-stud/5332-patient-pk-init-tol-stud-rep` |
| 313 | 5.3.3.3 | `…/533-rep-human-pk-stud/5333-intrin-factor-pk-stud-rep` |
| 317 | 5.3.3.4 | `…/533-rep-human-pk-stud/5334-extrin-factor-pk-stud-rep` |
| 321 | 5.3.3.5 | `…/533-rep-human-pk-stud/5335-popul-pk-stud-rep` |
| 325 | 5.3.4 | `…/53-clin-stud-rep/534-rep-human-pd-stud` |
| 326 | 5.3.4.1 | `…/534-rep-human-pd-stud/5341-healthy-subj-pd-stud-rep` |
| 330 | 5.3.4.2 | `…/534-rep-human-pd-stud/5342-patient-pd-stud-rep` |
| 334 | 5.3.5 | `…/53-clin-stud-rep/535-rep-effic-safety-stud` |
| 335 | 5.3.5 *(per indication)* | `…/535-rep-effic-safety-stud/indication-1` **⟨example⟩** |
| 336 | 5.3.5.1 | `…/indication-1/5351-stud-rep-contr` |
| 340 | 5.3.5.2 | `…/indication-1/5352-stud-rep-uncontr` |
| 344 | 5.3.5.3 | `…/indication-1/5353-rep-analys-data-more-one-stud` |
| 348 | 5.3.5.4 | `…/indication-1/5354-other-stud-rep` |
| 352 | 5.3.6 | `…/53-clin-stud-rep/536-postmark-exp` |
| 353 | 5.3.7 | `…/53-clin-stud-rep/537-crf-ipl` |
| 354 | 5.3.7 *(per study)* | `…/537-crf-ipl/study-1` **⟨example⟩** |
| 366 | 5.4 | `m5/54-lit-ref` |

### `util/`

**The filenames here are illustrative** — see §5.

| # | Path | |
|---|---|---|
| 370 | `util` | utilities |
| 371 | `util/dtd` | only the region being filed to needs its regional DTD |
| 372 | `util/dtd/ich-ectd-n.dtd` | `n` is the version, e.g. `3-2` |
| 373–376 | `util/dtd/{eu,jp,us,xx}-regional-n.{dtd,xsd}` | one of these |
| 377 | `util/style` | ICH and regional stylesheets |
| 378 | `util/style/ectd-n.xsl` | **RegOS holds no stylesheet** |
| 379 | `util/style/xx-regional-n.xsl` | |

---

## Two oddities in the source, noted rather than silently repaired

Entry **61** arrives as:

```
Element m3-2-s-4-3-validation-of-analytical-procedures (name, manufacturer)
```

`(name, manufacturer)` belongs on the `Title` line, not the element name — the
same class of PDF line-wrap damage that had to be repaired when Appendix 8 was
transcribed. The directory value on that row is unaffected.

Entries **358** and **362** carry the comment *"define element"* — an editorial
note left in the published document, not guidance. Recorded because a reader
who met it later would reasonably wonder whether it was transcription damage.

---

## Before any of this is seeded

**Nothing here has been loaded into a blueprint yet.** These values are Level 3,
partial, and carry at least four placeholder names that only a comment
distinguishes from real ones. A seeded value is one a package will be built
from, so each row needs to be read against its comment first — and the missing
Module 5 and `util/` rows obtained.
