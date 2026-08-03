# FDA — Study Data Technical Conformance Guide, v6.2.1 (June 2026)

**What this is, and what it is not.** This is the **Study Data** Technical
Conformance Guide: it governs SDTM, SEND and ADaM **datasets submitted inside
Modules 4 and 5**. It is *not* the **eCTD** Technical Conformance Guide, which is
a separate document and the one that carries Module 1's regional vocabularies.

> The document names the other one itself, at footnote 72: *"See 'eCTD Technical
> Conformance Guide' for further details about submitting in eCTD v3.2.2."*

## How it entered the repository

Supplied by the founder on **2026-08-03** as a PDF, read in full (97 pages).
Version 6.2.1, June 2026, CDER/CBER. Marked *"Contains Nonbinding
Recommendations."* Docket FDA-2018-D-1216.

**The PDF itself is not committed.** Unlike the two DTDs — which are normative
machine-readable artifacts a parser consumes, and which the package ships — this
is prose. What is committed is this reading, with the section numbers a reader
can check it against. Any claim below that matters is quoted rather than
paraphrased.

## What it does *not* contain

Checked deliberately, because the reason it was obtained was to close S006's
remaining gap. **None of these appears anywhere in the document:**

| Wanted | Where it actually lives |
|---|---|
| `telephone-number-type` | the eCTD TCG, unread |
| `applicant-contact-type` (`fdaact1`, `fdaact2`) | the eCTD TCG, unread |
| `form-type` (`fdaft1` = Form FDA 1571) | the eCTD TCG, unread |
| the DUNS placeholder for `applicant-info/id` | the eCTD TCG, unread |

**This is what forced the DUNS correction.** RegOS's PoC cites *"999999999 is the
placeholder FDA permits (Tech Guide 3.1.1)"* — and every occurrence of that
sentence in this repository is in a file **RegOS wrote**. No Technical
Conformance Guide of any kind was held when it was written. The claim may well be
true; it is not evidenced here.

## Findings

### 1. FDA instructs applicants not to use `append` (§7.1.1)

> *"Do not use the eCTD 'append' lifecycle operator when submitting updated or
> changed content within study data files that were previously submitted. Updated
> files should be submitted as replaced and not submitted as new."*

**Scope, preserved deliberately: this is about study data files.** It is not a
statement about every document an eCTD may carry. The generalisation is tempting
and is exactly the kind of over-reading this register exists to prevent — see
**E10**, which this upgrades without widening.

### 2. FDA depicts a v3.2.2 sequence folder as `0000` (Appendix E, footnote 96)

Appendix E's worked folder structure is rooted at:

```
NDA123456
  0000
    m4
      datasets
```

and footnote 96 adds:

> *"If submitting in eCTD v4.0, the application type should be lowercase
> (nda123456) and the sequence folder must not have leading zeros ('1', not
> '0001')."*

**This does not resolve E4 against E5.** It is one more source, and it is FDA's,
showing `0000` as a v3.x sequence folder in an illustration. It also records that
the leading-zero convention is a **v3.x** one that v4.0 drops — which is a fact
about the format's history rather than about first-sequence numbering.

Two incidental divergences from RegOS's PoC, both for S008 rather than now: the
application folder is `NDA123456` (uppercase) where the PoC writes `ctd-987654`,
and v4.0 changes the case rule.

### 3. Module 4/5 study data requires a Study Tagging File (§7.1.1, §8.1.2, Appendix F)

For any file referenced in a study section of Module 4 or 5:

> *"a STF and ts.xpt must be present to identify the study ID and SSD to which the
> file belongs. The ts.xpt needs to contain either a study ID (STUDYID) or Sponsor
> Reference ID (SPREFID) value that matches with the STF study ID."*

Enforced by **automated validation on receipt** — the Technical Rejection
Criteria — not by human review. §7.1.5 lists 22 controlled **file tags**
(`study-data-reviewers-guide`, `weight-of-evidence`, `qt-clinical-study`, …).

**RegOS models none of this**: no STF, no `[study-id]`, no file tags, no
`ts.xpt`. It is outside EPIC-007a, whose Module 4 and 5 content is documents
rather than datasets — and it is recorded now because it is the kind of
requirement that is cheap to know about and expensive to discover during a
filing.

## Incidental corroborations

- **§2.1** names eCTD sections **1.13.9** *General Investigational Plan* and
  **1.20** *General Investigational Plan for Initial IND* — both declared in
  `us-regional-v3-3.dtd`, now with FDA prose saying what goes in them.
- **§3.1** requires `.xml` as the extension for XML files and states define.xml
  must not be compressed.
