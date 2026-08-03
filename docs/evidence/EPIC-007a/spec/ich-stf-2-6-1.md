# ICH M2 — The eCTD Backbone File Specification for Study Tagging Files, v2.6.1 (2008-06-03)

Supplied by the founder **2026-08-03**, read in full (22 pages). This is the
specification E21 said was missing, and the one ADR-054 needs.

**The PDF is not committed.** The DTD it controls — `ich-stf-v2-2.dtd` — is *not*
reproduced in it and is **not held**; the document points at ich.org.

## What an STF is, in the specification's own terms

> *"the eCTD backbone files do not contain enough information on the subject matter
> of several documents (e.g., study report documents) to support certain regulatory
> uses. **This additional information is provided in the STF.**"*

> *"An STF should be provided with the submission of any file, or group of files
> **belonging to a study** in Modules 4 and 5. STFs are **required by the United
> States**, are not required in Europe and are **not allowed in Japan**."*

**It carries no files.** Its content is references:

```
<doc-content xlink:href="../../../../../index.xml#a101">
  <file-tag name="synopsis" info-type="ich"/>
</doc-content>
```

`doc-content` points at a **leaf ID in index.xml**. So an STF is a second,
study-shaped view over leaves the backbone already holds.

### Its shape

| | |
|---|---|
| root | `ectd:study`, DTD `util/dtd/ich-stf-v2-2.dtd`, stylesheet `util/style/` |
| file name | `stf-<study-id>.xml`, placed **with the study's files** |
| `study-identifier` | `title` (the study's, not a document's), `study-id` (*"the internal alphanumeric code used by the sponsor"*), `category*` |
| `study-document` | `doc-content*` → optional `property`, then `file-tag` |
| `category` | only for **4.2.3.1, 4.2.3.2, 4.2.3.4.1, 5.3.5.1** — species, route-of-admin, duration, type-of-control |
| `file-tag` | ~40 values, `info-type` = `ich` / `us` / `jp` |
| `property` | only `site-identifier`, and **required** alongside `case-report-forms` and `subject-profiles` in the US |
| one per study per sequence | *"You should provide a separate STF for each study in a sequence"* |

### Its lifecycle — and the exception it creates to E10

> *"The operation attribute for this leaf should have a value of **'new'** for the
> first STF for that specific study in that eCTD element and **'append'** for any
> subsequent STF … The subsequent STF should always have a modified-file attribute
> that refers to the **most recently submitted** STF … (i.e., you should not
> continually 'append' to the original STF)."*

**Accumulative only** — the cumulative approach was removed in 2.6.1. Subsequent
STFs carry *only what changed*.

> **This is the one place `append` is mandated.** E10 records FDA saying avoid it
> for documents and forbidding it for datasets; the STF is neither, and its
> lifecycle *requires* it. Recorded as a third scope in **E10**, not a
> contradiction of it.

Two more lifecycle rules with no analogue in RegOS:

- **Deleting** a study document: delete the *leaf* in index.xml; submit **no** STF.
- **Correcting a file-tag**: delete the leaf, re-add the same file as `new` (*"there
  is no need to send a second copy of the file"*), then `append` an STF with the
  corrected tag.
- **The latest `study-identifier` wins outright**: *"there is no mechanism for
  comparing the information contained in the study-identifier sections … the
  information contained in the study-identifier section of the most recent STF will
  be deemed the most current."*

### And one study may need more than one STF (§VI)

When time-point analyses have distinct lifecycles, or when one study supports two
CTD subsections. So the mapping is **not** study → STF; it is
**(study, eCTD element) → STF**, and even that may be deliberately split.

## What this means for RegOS

Two things must exist before an STF can be generated, and RegOS has neither:

1. **A Study** — an identifier the sponsor chose, a title, and for four CTD
   sections a set of categories (species, route, duration, type-of-control). This
   is a *business* entity, not a document and not a section.
2. **A `file-tag` per placement** — *what role this document plays in this study
   report*: synopsis, protocol, CRF, randomisation scheme. That is
   **[ADR-053](../../adr/ADR-053-instance-qualifiers-belong-to-the-placement.md)'s
   instance qualifier**, arriving a fourth time: a fact about the placement, not
   about the section, required by the format, and absent from the domain.

**The founder's instinct is confirmed**: an STF is not a `TemplateSection` and not
a document. It is generated metadata over placements that share a study — which
makes it a *projection*, like the package itself (ADR-049), over facts RegOS would
have to start holding.

**Its lifecycle is not a projection**, though, and that is the hard part: `append`
chains, latest-identifier-wins, and delete-then-reactivate are all statements about
*previous sequences*. A pure projection of one sequence cannot produce them.
