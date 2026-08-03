# FDA — eCTD Technical Conformance Guide, v1.8 (November 2022)

**This is the document EPIC-007a had been citing without holding.** Every
reference to *"Tech Guide 3.1.1"* in this repository pointed here, from a
hand-written PoC, for a year before the document itself arrived.

## How it entered the repository

Supplied by the founder on **2026-08-03** as a PDF, read in full (31 pages).
Version 1.8, November 2022, CDER/CBER. *"Contains Nonbinding Recommendations."*
Companion to the guidance *Providing Regulatory Submissions in Electronic Format
— Certain Human Pharmaceutical Product Applications and Related Submissions Using
the eCTD Specifications*.

**The PDF is not committed** — it is prose, not a machine-readable artifact the
package ships. What is committed is this reading, with section numbers to check
it against.

## The citation that was outstanding

§3.1.1, verbatim:

> *"Submissions to CDER and CBER require us-regional.xml backbone version 3.3.*
>
> *If you are unable to acquire a DUNS number prior to submission, you may enter
> 999999999."*

**Section 3.1.1 — the PoC's citation was accurate to the section number.** The
claim was true the whole time; the evidence was simply absent, and on 2026-08-03
the absence was mistaken for the claim being ours. Both corrections are recorded
in [`ectd-mapping.md`](../ectd-mapping.md) and the epic.

**The condition is about the applicant, not about the software.** FDA permits the
value when *the filer cannot obtain a number* — not when a system has nowhere to
store one. RegOS emitting it unconditionally would assert something about the
filer that is usually false, which is why `Organization.DunsNumber` remains the
real answer and `999999999` remains a **recorded fallback**.

## What it still does not contain

| Wanted | Where it actually lives |
|---|---|
| `telephone-number-type` | **eCTD Backbone Files Specification for Module 1** (Reference #3), unread |
| `applicant-contact-type` | same |
| `form-type` | same |

`applicant-contacts` is mandatory and `applicant-contact-type` is `#REQUIRED` on
every contact, so **S006's wiring remains blocked** on one document.

## Findings

### 1. `append` — the scope distinction is FDA's own (§2.5)

> *"The use of 'append' is not common. You should avoid appending multiple
> documents to a single leaf and consider consolidating the information and using
> the 'replace' life cycle attribute to update the original file. However, it may
> be appropriate if, for example, you are adding a single page of information to a
> lengthy document. **Updated datasets should 'replace' the old dataset. Do not use
> 'append' when updating datasets.**"*

Documents: *avoid*, with a stated exception. Datasets: *do not*. The distinction
preserved in **E10** on 2026-08-03 — before this document was held — is the
authority's own, and the Study Data TCG's absolute wording is the dataset half
quoted alone.

### 2. The path limit is FDA's, and it is stricter than ICH's (§2.4)

> *"when naming folders and files, the length of the entire path must not exceed
> **150 characters**. The character limit on the leaf title field is 512
> characters."*

ICH Appendix 2 allows **230**. A path legal under ICH can be illegal to FDA, and
RegOS checked neither. Now **E22**, enforced in `SequenceFolderGenerator`.

§2.4 also states two rules about leaf titles RegOS already follows by accident:
titles are ≤512 characters, and *"You should not include the eCTD section number
in the leaf title."*

### 3. Node extensions are forbidden outright (§5, item 1b)

> *"Node extensions: Do not use node extensions to create new elements. Although
> this is described in the ICH eCTD specification, and may be acceptable in some
> regions, **it is not acceptable in any submissions to FDA**."*

`node-extension` appears in most ICH content models — `((leaf | node-extension)*)`
— so this is a construct the DTD permits and the authority refuses. Now **E23**,
asserted by a test rather than left to implementation habit.

### 4. Instance qualifiers must be stable *across sequences* (§4.1, §4.4, §5 item 2)

§4.1, on 3.2.S/3.2.P:

> *"This issue is caused by leafs being submitted with incorrect metadata ('name',
> 'manufacturer', and/or 'dosage form' which are **not an exact match to what was
> submitted previously**)."*

and §4.4, on STFs:

> *"caused by an updated STF being submitted with incorrect metadata (study-id and
> study title not an exact match)."*

**This adds a constraint the DTD cannot express.** E17 established that these
nodes are keyed; this establishes that the key is how **FDA's review tooling
identifies continuity between sequences**. A qualifier is therefore not only a
fact about a placement — it must be *the same* fact in every sequence that
touches that node, or the reviewer sees the section twice.

Recorded as **E24** and linked to
[ADR-053](../../adr/ADR-053-instance-qualifiers-belong-to-the-placement.md)
through the register's decision index. **ADR-053 is not edited** — repository
canon, and this is evidence constraining a design the ADR deliberately left
unmade.

§5 shows the qualifiers in use as FDA writes them:
`<m3-2-p-drug-product product-name = "Albuterol">`.

### 5. STFs are required for sections the blueprint already seeds (§2.8)

> *"Study Tagging Files (STFs) are required for all files in section 4.2.x and
> 5.3.1.x – 5.3.5.x. STFs are not required for 4.3 Literature references, 5.2
> Tabular listings, 5.3.6 Postmarketing reports and, 5.4 Literature references."*

**The FDA IND blueprint seeds 4.2.1, 4.2.2 and 4.2.3** — all inside `4.2.x`.
Every IND has Module 4 content, so this is not a future concern. §4.3 records
what happens without one:

> *"Issue: Not Applicable (N/A) or Unassigned Folders in Module 4 or 5. This issue
> is caused by leafs submitted without an STF in a section that requires STFs."*

**E21 is promoted from a future note to an active blocker.** Two mitigations: 5.2
is explicitly exempt (also §3.5.1), and bare 5.3 is outside the enumerated range.

An STF is *not* another document. §3.4.1: *"Individual study documents should be
referenced in an STF using the appropriate STF 'file-tag'."* It has its own
lifecycle (§2.5 defers to the STF specification), and deleting the leaves it
references deletes the STF itself from FDA's review tool (§4.2). **The ICH M2
specification that defines it — Reference #2 — is not held.**

### 6. Sequence numbering — FDA prose, twice, says begin at 0001

§2.3: *"Provide only new or changed information and **begin with sequence number
0001**."* §2.6: *"Any information submitted in eCTD format before the
'original-application' … **should start with sequence 0001**. A high submission
sequence series (e.g., 9000) should not be used."*

Both are scoped — one to transitions, one to presubmissions — and neither states
the general rule for an eCTD-native original application. But they are **prose,
not worked examples**, which is what E5 rested on, and they point the opposite way
from **E20**'s `0000` illustration.

**Left unresolved deliberately.** RegOS writes `0000` because that is the business
fact it holds (E4, ADR-044), and S008 exists to compare rather than absorb. This
sharpens that comparison; it does not settle it.

## Incidental

- §2.3: *"For INDs, there is no requirement to match up the sequence number with
  the serial number"* — the two are independent, which RegOS already assumes.
- §2.3.1/§2.3.2 restate the `submission-id` rule recorded as **E15**, from a
  second FDA source.
