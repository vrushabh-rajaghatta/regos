# EPIC-007a — external validation evidence

**Decisions are backed by artifacts, not memories.** This directory holds the
evidence that something outside RegOS checked a RegOS-generated package — not a
claim that it did.

An epic that says *"it validated"* and keeps nothing has produced a memory. One
that keeps the report, the tool version and the exact package that was checked
has produced an **architectural asset**: a later reader can see what was
validated, by what, and when, and can tell whether it still holds.

---

## Task 1 — the oracle. Outcome: **partly failed, and replaced.**

### What was decided, and did not survive

| | |
|---|---|
| **Candidate oracle** | LORENZ eValidator Basic, US eCTD 3.2 profile |
| **Outcome** | ✖ **not obtainable** — commercial tooling, Windows-only, and no licence available to this project |
| **Decided** | 2026-08-02 |

The epic said Task 1 was allowed to fail and that the honest response was to say
so rather than describe self-validation as external evidence. **This is that
failure, recorded rather than worked around.**

### What replaced it

Failing to obtain eValidator does **not** collapse the epic to Level 1, because
the founder supplied the primary sources on 2026-08-02 — including the actual
`us-regional-v3-3.dtd`. That splits the old Level 2 into two, and only the
second half is blocked:

| | Oracle | Reachable now? |
|---|---|---|
| **2a — structural** | FDA's published DTD, checked by **any third-party XML parser** | ✅ **achieved 2026-08-02** — libxml2 20913, [`poc/how-to-reproduce.md`](poc/how-to-reproduce.md) |
| **2b — business rules** | eValidator's FDA validation criteria | ✖ **no** — needs the commercial tool |

**Level 2a is genuine external evidence.** The specification is FDA's, the DTD is
FDA's, and the implementation doing the checking is a standard parser that knows
nothing about RegOS. It is not the same as 2b — a package can be perfectly
DTD-valid and still break FDA business rules — and this directory will never
claim otherwise.

**Level 3 also became reachable.** FDA publishes complete `us-regional.xml`
examples, including two IND sequences and their amendment (#21–#24). Comparing
our output against those is convention evidence that needs no tool at all.

> **The principle is unchanged, and now cheaper to honour.** *The validator is an
> oracle, not a dependency.* A DTD held in `spec/` and read by a parser we do not
> own is the purest form of that: it can only ever tell us we are wrong.

### Carried to EPIC-007b

**2b — FDA business-rule validation.** The trigger is a licence becoming
available, or a customer engagement that supplies one. Until then no document in
this repository may describe a RegOS package as *validated* without saying
against which of 2a or 2b.

---

## Task 2 — the specifications, pinned

> **ICH eCTD v3.2.2** (the `index.xml` backbone) **and FDA us-regional DTD v3.3**
> (the Module 1 backbone).

Task 2 was recorded as one pin and was **incomplete**. The two backbones version
independently, and `submission-sub-type` — required on every sequence — exists
only from regional v3.3. FDA's current pairing is eCTD 3.2.2 with regional 3.3,
which is what every worked example in their own document uses.

**v4.0 stays out.** Supporting both would double the surface before one package
has ever been checked, and would make a failure ambiguous — we would not know
which target we had got wrong.

The regional DTD is held at [`spec/us-regional-v3-3.dtd`](spec/us-regional-v3-3.dtd),
which is not a convenience: **every eCTD package must ship its DTDs inside
`util/dtd/`** (ICH Appendix 4, rows 372–376), so the file is a build input, not
just a reference.

### ✅ Resolved — `ich-ectd-3-2.dtd` is pinned, and how it got here matters

Both backbones are now pinned. The blocker is closed: a conformant sequence
folder can be assembled, because `util/dtd/` can be populated.

**Provenance, stated plainly.** This file was **transcribed from Appendix 8 of
the ICH eCTD Specification v3.2.2 PDF**, not downloaded from ICH. Every
web route to the published file failed — `admin.ich.org` 404, `estri.ich.org`
does not resolve, `ich.org`'s page is JavaScript-rendered with no file links.

> **A transcription is not the same artifact as the publication.** If the
> transcription is wrong, every Level 2a claim resting on it is wrong too, and
> would look exactly as convincing. So it is verified rather than trusted, and
> **a byte-authentic copy from a real package should replace it** the moment one
> is available.

**What the verification was.** Not "it parses" — that only proves it is a DTD,
not that it is *this* DTD:

| Check | Result |
|---|---|
| ICH spec's own **Example 6-1** (Module 2, simple new submission) | ✅ valid |
| ICH spec's own **Example 6-3** (Modules 2 **and** 5, required `indication`) | ✅ valid |
| `operation="unchanged"` | ✖ rejected — *not among the enumerated set* |
| required `indication` removed | ✖ rejected |
| required `checksum` removed | ✖ rejected |

165 elements, 165 ATTLISTs. **Two instances the specification wrote itself both
validate, and three deliberate mutations are all caught** — which is a
substantially stronger claim than a clean parse.

### What the ICH backbone added that Module 1 could not

**E14 — `operation` is closed in *both* backbones.** E2 proved the enumeration
closed in FDA's regional DTD only. `unchanged` is now provably unrepresentable in
`index.xml` as well, so **[ADR-045](../../adr/ADR-045-the-cumulative-dossier-and-the-derived-delta.md)'s
derived delta is forced by the format everywhere**, not merely regionally.

**E16 — the two backbones disagree.** `checksum` is `#REQUIRED` in ICH and
`#IMPLIED` in FDA regional. One rule does not cover both files, and a renderer
that assumes it does will emit a `us-regional.xml` that passes and an
`index.xml` that does not.

---

## Task 3 — the mapping

[`ectd-mapping.md`](ectd-mapping.md) — element by element, with confidence marked
per row and the gaps ordered by how much of a package is impossible without them.

It found two defects in RegOS that only an external reference could find: the
seeded FDA IND blueprint mislabels section **1.13**, and RegOS numbers sequences
from **0000** where every FDA example starts at **0001**.

It also found that **`submission-id` groups sequences into a regulatory
activity** — which is EPIC-004's hypothesis 1, arriving from the US IND case
rather than the EU market that was predicted to settle it.

---

## What must land here before the epic claims anything

| Artifact | Claim it supports | |
|---|---|---|
| [`poc/validator-version.txt`](poc/validator-version.txt) | tool, version, level claimed **and level not claimed** | ✅ |
| [`poc/ctd-987654/`](poc/ctd-987654/) | the exact package checked, re-runnable | ✅ |
| [`poc/how-to-reproduce.md`](poc/how-to-reproduce.md) | the invocation, the output, **and the negative controls** | ✅ |
| `poc/negative-controls/` | proof the parser rejects — without which a pass means nothing | ✅ |
| [`spec/ich-ectd-3-2.dtd`](spec/ich-ectd-3-2.dtd) | the ICH backbone, transcribed from Appendix 8 and verified against the spec's own examples | ✅ |
| [`spec/ich-ectd-3-2-appendix-4.md`](spec/ich-ectd-3-2-appendix-4.md) | **the directory table** — which folder a CTD section is written to | ⚠️ **partial**, and Level 3 not 2a — see below |
| `comparison-to-fda-examples.md` | Level 3 — where we match FDA's published XML and where we differ | ⚪ |
| a 2b report | FDA business rules | ✖ carried |

### The gap that stopped S004 — Appendix 4

**Appendix 8 was transcribed. Appendix 4 was not, and it is the one that says
where a file goes.** The DTD constrains the backbone's *elements*; it types
`xlink:href` as `CDATA` and so has no opinion at all about paths. What a leaf's
directory should be is Appendix 4's table, and nothing in this repository
contains it.

`ectd-mapping.md` §3.4 already holds the top level (`m1/us`, `m2`…`m5`,
`util/dtd`) and Appendix 2's naming rules. **The per-section level is what is
missing**, and the seeded FDA IND blueprint spans all five modules — 186
sections — so this is not a Module 1 problem.

Three ways to proceed without it were considered and all three rejected, each
for the same reason:

| | |
|---|---|
| derive the folder from the section code | invention |
| put every leaf at its module root | DTD-valid, and knowingly wrong at Level 3 |
| read folder names from the regional DTD's element names | Module 1 only, and an inference from FDA's examples rather than a statement of the specification |

> **Appendix 4 is preferred over FDA's example packages, and the order matters.**
> Appendix 4 is the **specification**; the examples are **convention**. With the
> specification in hand, `comparison-to-fda-examples.md` later asks *"did FDA
> follow it?"* — without it, that comparison would silently become *"let us
> infer the specification from FDA"*, and the register's whole hierarchy would
> invert. The examples stay valuable as corroboration.

**The schema is already in place and empty.** `TemplateSection.EctdFolder`
exists, is nullable, and holds null in all 186 rows — the same *"not in
evidence"* a null `Token` carries. The shape was established by S004; only the
values are outstanding.

#### Appendix 4 arrived partially, and said something about itself

*2026-08-03.* An extract was supplied and is transcribed at
[`spec/ich-ectd-3-2-appendix-4.md`](spec/ich-ectd-3-2-appendix-4.md). It closes
less of the gap than expected, for three reasons the appendix states itself.

**1. It is Level 3, not Level 2a.** Its own preamble:

> *"The file and folder names shown within modules 2-5 are **not mandatory, but
> recommended**, and can be further reduced or omitted to avoid path length
> issues."*

A package that departs from these names is **not thereby invalid**, so no parser
can check us against them. It is still the best Level 3 available — the
specification's own recommendation outranks one regulator's examples — but the
expectation that it would supply canonical, checkable names does not survive
reading it.

**2. It stops at the door of Module 1.** Entries 3–7 give `m1` and one regional
directory per region, then say *"refer to regional guidance for details."*
**There are no Module 1 subsection folders.** For an FDA IND blueprint — whose
sections are almost entirely `1.x` — Appendix 4 supplies `m1/us` and nothing
else. **The first vertical is the part it does not cover.**

**3. Placeholders are unrecoverable from plain text.** The appendix says italic
names are examples the applicant replaces. The extract carries no italics, so
`substance-1-manufacturer-1`, `product-1`, `excipient-1` and
`32a3-excip-name-1` are identifiable only from their comments, and others may
not be identifiable at all.

**Still outstanding:** Module 4 from entry #203, all of Module 5, and `util/` —
the extract was truncated at 50,000 characters.

> **The appendix did close one thing, and it was a defect in RegOS.** Sections
> 2.7.1–2.7.6 have a file row and **no directory row**: their documents go in
> 2.7's folder. `TemplateSection` had been collapsing `""` into `null`, which
> would have made *"this section adds no directory of its own"* indistinguishable
> from *"nobody has read the specification"* — and two-thirds of Module 2
> unrenderable. Fixed, with `HasEctdPlacement` naming the difference.

> **The first external check RegOS has ever had.** It is narrow — one backbone
> file, structure only, hand-built — and it is real: the specification is FDA's,
> the DTD is FDA's, and the parser is not ours.
>
> Two negative controls make the pass mean something. One proves a sequence with
> no named contact is rejected (S005's requirement, enforced externally). The
> other proves `operation="unchanged"` is *"not among the enumerated set"* —
> **ADR-045's thesis, machine-checked: eCTD has nowhere to say what RegOS
> refuses to transmit.**

**The acceptance rule:** a claim of external validation requires a report here
that corresponds to a package here, produced by a tool version named here, at a
level stated here. Anything less is Level 1 wearing Level 2's clothes.
