# The languages a market's labelling is expected in

**Retrieved 2026-08-05, before the seed was written.**

**What this records:** which languages RegOS states for each seeded market — and
the reason the field is called *expected* rather than *required*.

## The finding that shaped the design

Canada is the case the story turns on, and reading the source **changed how the
feature works**.

Health Canada requires bilingual mock-ups of the labels, the package insert and
the Product Monograph at the time of submission — `C.01.014.1(2)(m.1)` and
`C.08.002(2)(j.1)`, in force since 13 June 2015. The Product Monograph for
prescription products is supplied in both official languages.

**But not every label.** Prescription-only products, products supplied only in
hospitals and clinics, and professional-use products **do not require bilingual
labelling**; those may be labelled in one language at the sponsor's choice. And
additional languages are permitted throughout, provided readability is not
obscured.

> **So "Canada requires English and French" is not true as a rule.** It is true
> of the monograph and of most labels, and false for a real and common class of
> product.

Which rule applies depends on **the product and the document** — and a country
knows neither. That is the argument for
[EPIC-022 D4](../../product/epics/EPIC-022-country-depth.md): the country states
what a market's labelling is normally in, the screen says what is missing, and
**nothing refuses anything**. Blocking belongs to a rule a blueprint states.

Had this not been read, the natural implementation would have been a validation
that refuses an incomplete Canadian label set — and it would have been wrong for
every hospital-only product.

## The sources

| | |
|---|---|
| Canada | [Health Canada — Labelling of pharmaceutical drugs for human use](https://www.canada.ca/en/health-canada/services/drugs-health-products/drug-products/applications-submissions/guidance-documents/labelling-pharmaceutical-drugs-humans.html) · [Product monographs FAQ](https://www.canada.ca/en/health-canada/services/drugs-health-products/drug-products/applications-submissions/guidance-documents/product-monograph/frequently-asked-questions-product-monographs-posted-health-canada-website.html) |
| The codes | ISO 639-1, two-letter. Widely published; the register is not held |

**Level 3** — the authority's published guidance, read.

## The eight, as seeded

| | Expected | Basis |
|---|---|---|
| **CA** | **en, fr** | **Read from Health Canada guidance** — the only multi-language row, and the one the browser proof turns on |
| US | en | official language of the market |
| GB | en | official language |
| DE | de | official language |
| FR | fr | official language |
| JP | ja | official language |
| AU | en | official language |
| IN | en | English is the language of Indian drug labelling |

> **⚠ Only the Canada row was read from a labelling source.** The other seven are
> the market's official language, which is a *reasonable* stand-in and is **not
> the same claim**. A country whose labelling law diverges from its official
> language — and they exist — would be wrong here. Stated plainly rather than
> left for a reader to assume the whole table was sourced alike.

Because the feature is advisory, a wrong row here produces **advice somebody
ignores**, not a refusal somebody cannot get past. That is the failure mode D4
was chosen for, and it is why seven unsourced rows were acceptable to ship and a
blocking rule would not have been.

## What would change this

| If this turns out wrong | What breaks |
|---|---|
| A market's expected languages are wrong | The advisory panel misleads. Nothing is refused, nothing is filed incorrectly |
| A market's labelling law diverges from its official language | The seven official-language rows above. Fix by reading that market's guidance, as Canada's was |
| The requirement becomes a rule somebody wants enforced | **That is a different feature.** It needs the product type and the document type, and belongs with the blueprint engine rather than with geography |

## The distinction this entry protects

> **RegOS does not hold labelling law.** It holds one market read from guidance
> and seven read from a map, in a demonstration seed that says so.
