# FDA eCTD attribute-value lists — the three RegOS needed

Supplied by the founder **2026-08-03**. These are the *"separate XML files"* the
[M1 Backbone Specification](fda-m1-backbone-2-6.md) Table 1 defers to, and the
gate S006's wiring had been waiting on.

**Committed here, unlike the PDFs** — they are small, normative, machine-readable,
and RegOS's own reference data can now be checked against them.

| File | Version | AsOf |
|---|---|---|
| [`telephone-number-type.xml`](telephone-number-type.xml) | 1.1 | 2012-11-01 |
| [`applicant-contact-type.xml`](applicant-contact-type.xml) | 1.2 | 2013-08-23 |
| [`application-type.xml`](application-type.xml) | 1.1 | 2012-11-01 |

Each carries an internal DTD, an `AsOf` date and a `version-number`, and each
entry has a **`status`** attribute — so *"which codes are still valid"* is a
question the file answers rather than one a reader infers.

> **`form-type.xml` is still not held**, so `m1-1-forms` stays refused (E18).

---

## 1. `telephone-number-type` — the gate, lifted

| Code | Display |
|---|---|
| `fdatnt1` | Business Telephone Number |
| `fdatnt2` | Fax Telephone Number |
| `fdatnt3` | Mobile Telephone Number |

**This is the single file that blocked every regional backbone.** The DTD makes
`admin → applicant-info → applicant-contacts → applicant-contact → telephones →
telephone` mandatory at every step, and `telephone-number-type` is `#REQUIRED` on
the last, so no `us-regional.xml` could be written at all without it.

> **And it turns the blocker inside out.** The vocabulary is now held — and
> **`ContactPhone` is a bare number with no kind.** RegOS cannot say whether the
> number it holds is a business line, a fax or a mobile. The gate moved from
> *their* vocabulary to *our* model, which is the third refusal again.

## 2. `applicant-contact-type` — and it falsifies a mapping RegOS had made

| Code | Display |
|---|---|
| `fdaact1` | Regulatory Contact |
| `fdaact2` | **Technical Contact** |
| `fdaact3` | United States Agent |
| `fdaact4` | Promotional Labeling and Advertising Regulatory Contact |

RegOS decided on 2026-08-03, from the M1 spec's worked example, to map
**`REG → fdaact1`, `MFG → fdaact2`**. The first survives; **the second does
not.** `fdaact2` is the *Technical Contact* — the person who handles the eCTD
submission itself — and RegOS's `MFG` is the *Manufacturing Contact*, *"the named
point of contact at a manufacturing site"*. Those are different people, and the
example's phrase *"the technical contact"* was read as a description when it was
a name.

**Nor is `fdaact3` a home for `AR`.** A United States Agent is a specific FDA
obligation on a foreign establishment; RegOS's *Authorised Representative* *"acts
for a manufacturer established outside the market"* — the same shape, a different
jurisdiction's law. Mapping them would assert an equivalence nothing states.

> **So the mapping shrinks to one row: `REG → fdaact1`.** Better evidence made
> RegOS emit *less*, which is the direction that should be unsurprising and
> rarely is.

## 3. `application-type` — confirms RegOS's guess, and adds rules no DTD can

| Code | Display | |
|---|---|---|
| `fdaat1` | New Drug Application (NDA) | |
| `fdaat2` | Abbreviated New Drug Application (ANDA) | |
| `fdaat3` | Biologic License Application (BLA) | |
| **`fdaat4`** | **Investigational New Drug (IND)** | **confirms E11's outstanding assertion** |
| `fdaat5` | Drug Master File (DMF) | |
| `fdaat6` | Emergency Use Authorization (EUA) | |
| `fdaat7` | Investigational Device Exemption (IDE) | ⚠ cross-reference only |
| `fdaat9` | Premarket Approval Application (PMA) | ⚠ cross-reference only |
| `fdaat10` | Premarket Notification 510k (510K) | ⚠ cross-reference only |
| `fdaat8` | Safety Issue | 🚫 *"Do not use. For FDA use only"* |

**`fdaat4` = IND was RegOS's own assertion for a year** — E11 flagged it, the M1
specification named NDA and DMF and never named IND's code, and it turns out to
have been right. The second time in two days that an unevidenced claim of ours
survived contact with the document. **The lesson is unchanged**: it was true and
it was not evidence.

### Two constraints a `CDATA` attribute cannot express

> *"The IDE, PMA, and 510k application types should only be used in the
> **cross-reference-application-number** element"*

So `fdaat7`, `fdaat9` and `fdaat10` are **not legal as an application's own
type**, only as a pointer to one. A DTD types `application-type` as `CDATA` and
will accept them anywhere; this file says where they belong.

> *"Do not use. For FDA use only"* — `fdaat8`.

**Both are business rules delivered by a value list**, which is a shape this epic
had not seen: not a schema, not prose, but an enumeration whose comments carry
obligations. Level 2b evidence arriving without a validator.

### And one code that does not exist

**There is no De Novo request code.** The list is complete and `status`-flagged,
so this is an *established absence* rather than an unread gap — a different and
stronger statement than the one RegOS has been making.

The development database holds `Initial NDA - 002`, whose application type is
`FDA_DENOVO`. Its refusal used to mean *"FDA prints that token nowhere we have
read"*. It now means **"FDA's own list has no code for this, so this application
cannot be filed in eCTD at all"** — which is a regulatory fact about the filing,
not a gap in RegOS.

---

## What RegOS may now seed, and what it may not

| | |
|---|---|
| `FDA_IND` → `fdaat4` | already seeded; **now evidenced** rather than asserted |
| `FDA_NDA` → `fdaat1` | seedable |
| `FDA_510K` → `fdaat10`, `FDA_PMA` → `fdaat9` | **must stay null** — cross-reference only, and RegOS has nowhere to record that distinction |
| `FDA_DENOVO` | **must stay null** — no code exists |
| every non-FDA row | unchanged: a null token means *this authority's wire vocabulary has not been modelled* |
