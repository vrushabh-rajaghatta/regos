# FDA — The eCTD Backbone Files Specification for Module 1, v2.6 (2025-03-31)

Supplied by the founder **2026-08-03**, read in full (47 pages). This is the
specification that defines `us-regional.xml` itself — Reference #3 of the eCTD
Technical Conformance Guide.

**The PDF is not committed** (prose plus an appendix copy of a DTD we already
pin). Appendix 1 reproduces `us-regional-v3-3.dtd`; spot-checked against
[our pinned copy](us-regional-v3-3.dtd) and identical in the elements RegOS emits.

## What it settles, and what it still defers

**Table 1 is the trap.** It names eleven attribute-type lists and then says:

> *"The attribute lists are maintained as **separate XML files** … Refer to the
> eCTD Submission Standards on the FDA website for the current versions of each
> list."*

So `applicant-contact-type.xml`, `telephone-number-type.xml`, `form-type.xml`
**are still not held**. What this document gives is *worked examples* — real codes
in real XML — which evidences some values and no complete enumeration.

| Code | Meaning | Source |
|---|---|---|
| `fdaact1` | *"a regulatory contact"* | §III.A.4 example |
| `fdaact2` | *"the technical contact"* | §III.A.4 example |
| `fdatnt1`, `fdatnt3` | **valid codes — meanings never stated** | §III.A.4 example |
| `fdaft2` | Form FDA 356h | §III.B.2.c example |
| `fdaft5` | Form FDA 2253 | §VI.B example |
| `fdaat1` | NDA | §III.B.1.a example |
| `fdaat5` | DMF | §III.B.1.b example |
| `fdast1` / `fdast2` / `fdast4` | Original Application / Efficacy Supplement / Labeling Supplement | Tables 5–8 |
| `fdasst2` / `fdasst3` / `fdasst4` | presubmission / application / amendment | Tables 5–6 |

**Still unevidenced, and RegOS asserts both:** `fdaat4` = IND, `fdaft1` = Form
FDA 1571. The document names NDA and DMF and never names IND's code. Table 10
places Form 1571 without giving its code.

> **`telephone-number-type` remains blocked, for a sharper reason.** The codes are
> now known to be real; **what they mean is not**. A phone number RegOS holds has
> no type, and tagging it `fdatnt1` would assert a classification whose definition
> lives in a file we do not have.

## Findings

### 1. The DOCTYPE is a URL, and the `util/` folder is the *old* way (§II, App. 2 §E.17)

> *"The header of the Module 1 eCTD Backbone File is always the same"* —
> ```
> <!DOCTYPE fda-regional:fda-regional SYSTEM "https://www.accessdata.fda.gov/static/eCTD/us-regional-v3-3.dtd">
> <?xml-stylesheet type="text/xsl" href="https://www.accessdata.fda.gov/static/eCTD/us-regional.xsl"?>
> ```

and Appendix 2, recording the v2.0 change:

> *"The us-regional.xml refers to and validates from supporting and required files
> (DTD, stylesheet, and value-type lists) located at **website addresses instead of
> local file paths** (previously required files were located in the util folder)."*

**RegOS emitted `../../util/dtd/us-regional-v3-3.dtd`.** Corrected (**E26**). The
stylesheet processing instruction was absent entirely; now emitted.

This creates a tension the epic has to hold deliberately: FDA wants a network
reference, and RegOS's Level 2a claim rests on **offline** validation against a
pinned DTD. Resolved by validating a copy whose DOCTYPE is rewritten to the
pinned file, and asserting separately that what ships carries FDA's URL.

### 2. `modified-file` in Module 1 points at `us-regional.xml`, not `index.xml` (§V)

> `modified-file="../../../0001/m1/us/us-regional.xml#id34567"`

and for a leaf first submitted under another application in a grouped submission:

> `modified-file="../../../../nda456789/0001/m1/us/us-regional.xml#id21342"`

RegOS builds `../{sequence}/index.xml#{leaf}` for **every** backbone. That is
right for Modules 2–5 and wrong for Module 1 (**E27**) — unreached today only
because the generator is unwired.

### 3. The sequence number starts at 0001, in the specification (§III.B.2.b)

> *"It must be a unique number with a maximum of four (4)-numeric digits, **should
> start at 0001**, and should not exceed 9999."*

and Appendix 2's migration table maps the old scheme to the new one explicitly:

| New Module 1 | Old Module 1 |
|---|---|
| `sequence-number` **0001** | Sequence: **0000** |
| `sequence-number` **0002** | Sequence: 0001 |

**`0000` was the pre-v2.0 numbering, replaced.** With eCTD TCG §2.3 and §2.6 this
is the third FDA statement, and the first from a *specification* rather than
guidance. Recorded as **E28**; it bears directly on **E4**, **ADR-044** and S008,
and is **not** resolved unilaterally.

### 4. Leaves belong only at the lowest level (§VI)

> *"**Leaf elements should only be referenced at the lowest level
> section/sub-section** of the hierarchy for each heading element. If a section
> heading does not contain references to files or documents, omit the element for
> that heading."*

**This is E19 stated as a rule rather than inferred from content models.** The
blueprint's five container-only Module 1 sections are refused for exactly the
reason FDA gives here.

### 5. Field constraints RegOS does not enforce

| | |
|---|---|
| `application-number` | *"six (6)-digit … only numeric digits, including any leading zeros … without letters or dashes"* (§III.B.1.a) |
| `telephone`, `email` | 64 characters each (§III.A.4) |
| `submission-description` | optional, 128 characters (§III.A.3) |
| `id` (DUNS) | nine digits; *"the same … for all submissions to an application"* (§III.A.1) |

### 6. S003's model, restated by the specification (§III.B.2.a, §III.B.3)

> *"The four (4)-digit submission-id number for each regulatory activity is
> determined by the sequence-number of the first submission to each new regulatory
> activity."*

E15 recorded this from FDA guidance. It is now in the backbone specification, with
two worked scenarios. `OriginatingSubmissionId` is that number.
