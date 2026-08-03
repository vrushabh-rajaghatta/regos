# FDA Module 1 — *Comprehensive Table of Contents Headings and Hierarchy*, v2.3.2

Supplied 2026-08-03. Sections **1.1 – 1.20** with their full sub-hierarchy and
headings, plus the backbone attributes a heading carries — `Form [form-type]`,
`1.15 Promotional material [promotional-material-audience-type]`.

**The headings are not reproduced here.** The full list is long, and the two
things RegOS needed from it are findings rather than rows.

---

## 1. It does not answer S004's question

> **This is a table of *contents*, not a table of *file organization*.** It has a
> number column and a title column and **no directory column anywhere.** ICH
> Appendix 4 defers Module 1 to "regional guidance"; this is that guidance, and
> what it supplies is the section hierarchy — not where a file goes.

S004 remains blocked on folder names. See §4 for why that may be the wrong way
to describe the situation.

## 2. It agrees with the DTD, exactly

Every heading matches an element in
[`us-regional-v3-3.dtd`](us-regional-v3-3.dtd) — `1.13 Annual report` ↔
`m1-13-annual-report`, `1.14.4.1 Investigational brochure` ↔
`m1-14-4-1-investigational-brochure`.

**That is a third independent FDA source for E9** (1.13 is the Annual Report and
the brochure sits at 1.14.4.1), after the DTD and the *Submission Types and
Subtypes* tables. E9 was already acted on in S002; this closes any doubt.

## 3. Two defects it exposes in the seeded blueprint

| Section | RegOS says | Both FDA sources say |
|---|---|---|
| **1.14.4.1** | "Investigator's Brochure" | **"Investigational brochure"** |
| **1.2** | "Cover Letter" | **"Cover letters"** |

S002 corrected the *placement* of the brochure and carried its old *title*
across unchanged. The wording is FDA's to choose and ours to copy — and
correcting it is another blueprint version, so it should be batched rather than
done alone.

**It also shows how small the blueprint is.** FDA's Module 1 runs 1.1 to 1.20
with deep sub-structure; the seeded FDA IND blueprint has eight sections. That is
a completeness question about the blueprint, entirely separate from folders, and
nothing in this epic has needed it yet.

## 4. What this suggests about the "gap"

Three statements, each from a specification rather than inferred:

| | |
|---|---|
| ICH Appendix 4 preamble | modules 2–5 folder names are *"not mandatory, but recommended"* |
| ICH Appendix 4 preamble | italicised names are replaced *"in accordance with their own naming conventions"* |
| ICH Appendix 4, Module 1 | *"refer to regional guidance"* — and the regional guidance gives headings, not folders |

> **Read together, these say folder naming is largely the applicant's own,
> bounded by Appendix 2's character and length rules.** If that is right then
> there is no FDA Module 1 directory table waiting to be found, and S004 is not
> blocked on missing evidence at all — it is waiting on a decision that belongs
> to RegOS.

That would be a different kind of unblocking, and it needs saying out loud
rather than being assumed, because the two are easy to confuse and only one of
them licenses us to choose a name.

**One convention is already ruled out.** Using the DTD's own element name as the
folder — attractive because the string is FDA's rather than ours — breaks
Appendix 2: **11 Module 1 element names exceed the 64-character segment limit**,
reaching 94.
