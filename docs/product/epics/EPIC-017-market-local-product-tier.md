# EPIC-017 — The market-local product tier

**Status:** ⚪ Not Started · **Branch:** `epic/EPIC-017-market-local-product-tier` (cut at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

The structural unlock. RegOS's `Product` is a **global** identity; the regulatory world is **market-local**. This inserts the missing tier and hangs the two facts users ask for first — **what it's called there**, and **whether it's actually on sale**.

> **Phase 1 below is settled.** **Phases 2–3 are a sketch**, written so this epic can be picked up months from now without re-deriving it — they are **not approved design**. Confirm, amend or replace them in the Phase-2 conversation when this epic is pulled into **Now**.

---

## Phase 1 — Epic plan

### Outcome
EPIC-005's portfolio views stop answering *"what do we hold in Canada?"* with a global product code and start answering it the way a regulatory user asks it: **"Product X, sold there as *Brandname*, launched 2021, licence 12345, expires 2027."** Trade name and market status become first-class, market-scoped facts.

### The finding this epic responds to

Compared against the DIA RIM object model, the usual framing — *"RegOS collapsed the product hierarchy"* — is imprecise. Look at where RIM puts market-locality:

| RIM tier | Carries | RegOS |
|---|---|---|
| **Product Family** | molecule/compound family, therapeutic area, IBD dates, sponsor | — |
| **Global Product** | global identity, type, dosage forms, global label | **`Product`** ← we are here |
| **Medicinal Product** | **one trade name, one licence, market status, ATC code, strength, local label** | **missing** |

`Product` has no country and is referenced globally by documents and applications — it sits at **Global Product**. RegOS expresses market-locality on the *transactional* records instead (`CountryId` on `RegulatoryApplication` and `Registration`). So the tier genuinely absent is **Medicinal Product**, and it is where Trade Name, Market Status, Local Label, ATC code and strength all want to live. **About 18 further RIM objects hang below it.**

### The alternative we rejected — and why *(write this down; it will be asked again)*

*"`Registration` already carries (ProductId, CountryId, AuthorityId, HolderOrganizationId) — which is nearly RIM's Medicinal Product identity. Why not just put trade name and market status on `Registration`?"*

Because **a licence and a market presence are different facts with different lifecycles**:

- A market-local product can hold **several licences over time** (renewal, replacement, transfer) and, per EPIC-005 decision 4, **several at once** (strengths, presentations, partial divestment). Trade name would then be duplicated across them and could silently disagree.
- **Market status ≠ licence status.** A product can be `Approved` and *not launched*; `Approved` and *withdrawn from sale* commercially; `Suspended` while stock remains in the channel. RIM separates them deliberately, and so must we.
- A trade name can be **registered before any licence exists** and survives the licence it was granted under.

Collapsing them would be cheap now and wrong at the first renewal.

### In scope ✅
- **`MedicinalProduct`** — the market-local tier: global product + country, its type, ATC code, investigational flag, holder.
- **The reference re-pointing decision** — which tier every existing `ProductId` means, applied and migrated (see Phase 2; this is the real work of the epic).
- **Trade Name** — per country + language, on the medicinal product.
- **Market Status** — status, dated start/end, launch date, risk-of-supply flag + comment; with the dated history pattern.
- **Portfolio views enriched** — registration lists show trade name and market status alongside the licence.
- Browser proof, ADR.

### Out of scope ⏸️ (deferred, with reason)
| Deferred | Why |
|---|---|
| **`Product Family`** (the tier *above* Global Product) | Nothing yet asks a family-level question. Crucially, inserting a tier **above** an existing root is cheap — the existing root gains one nullable FK — whereas inserting one **between** existing objects (what this epic does) is not. Deferring costs almost nothing; deferring *this* tier would keep costing more. |
| **Strength, dosage form, ingredients, ATC beyond a code field** | → **EPIC-010**. This epic establishes the tier; EPIC-010 fills it. |
| **Packaged Product / Packaging / presentations** | → **EPIC-010**. |
| **Local Label / Labeling** | → **EPIC-018**, which depends on this tier existing. |
| **Splitting existing `RegulatoryApplication.ProductId`** | See Phase 2 decision 2 — the recommendation is that applications stay at the **global** tier. If that is overturned, it becomes a story here. |
| **Migrating historical data across tiers** | There is no production data. The migration creates one medicinal product per existing (product, country) pair found on registrations — deterministic, no inference. |

### Definition of Done
- A `MedicinalProduct` exists for a (global product, country) pair, and a `Registration` is created **against it** rather than against the global product.
- Every existing `ProductId` reference in the codebase has an explicit, documented tier — recorded in the epic and asserted by tests where it matters.
- A medicinal product carries **one trade name per language** for its country, and a **dated market-status history** (the `RegistrationStatusEntry` pattern: `OccurredOn` vs `RecordedOnUtc`).
- *"What do we hold in Canada?"* returns trade name + market status + licence + expiry in one read.
- Existing EPIC-005 registration tests still pass, re-pointed rather than rewritten.
- Migration verified on a fresh database **and** on a seeded one.
- ADR written for the tier model and the re-pointing.

---

## Phase 2 — Domain design *(sketch — not approved)*

### The re-pointing table — **the core artifact of this epic**

| Existing reference | Recommended tier | Reasoning |
|---|---|---|
| `Registration.ProductId` | **Medicinal Product** | RIM: License → Medicinal Product (Parent). A licence is granted for a specific market-local product. **This is the one that must change.** |
| `RegulatoryApplication.ProductId` | **Global Product** (keep) | RIM lets an Application span a family (`Product Family`, Multiple, Inherited) while Medicinal Product lists Applications as children — i.e. RIM models it *both* ways. An application is filed *to reach* a market authorisation; the local product is often what the application **creates**. Keeping it global avoids a chicken-and-egg on creation. **The genuinely contested call — settle it explicitly.** |
| `ProductDocument.ProductId` | **Global Product** (keep) | A CMC or nonclinical document is about the product, not one market's presentation. |
| `Submission` (via `ApplicationId`) | follows Application | No direct `ProductId`; inherits whatever (2) decides. |
| `ProductDirectoryRow` | **Global Product** (keep) | The product master list is a global list. |
| `SubmissionSnapshot` | follows Submission | |

### Entities

**`MedicinalProduct`** — aggregate root. New context `src/MedicinalProduct/`, **or** a second aggregate in `src/Product/` (decision 3).

| Field | Type | Notes |
|---|---|---|
| `Id` | `MedicinalProductId` | |
| `TenantId` | | fail-closed filter |
| `ProductId` | `ProductId` | the global product it localises, `Restrict` FK |
| `CountryId` | `CountryId` | what makes it market-local |
| `Name` | string | RIM "Medicinal Product Name" — may differ from the global name |
| `AtcCode?` | string | RIM: Multiple; single to start, note the seam |
| `IsInvestigational` | bool | RIM: required |
| `MarketingAuthorizationHolderId?` | `OrganizationId?` | |
| `Status` | enum | product-record lifecycle, **not** market status |

**`TradeName`** — child entity of `MedicinalProduct`. `Id`, `Name`, `Language`. RIM also carries `Country` on Trade Name; here it is **inherited from the parent** — one of the reasons the tier is worth having.

**`MarketStatus`** — child entity of `MedicinalProduct`, **append-only history** plus a stored current value.

| Field | Notes |
|---|---|
| `Status` | RIM controlled list — e.g. NotLaunched, Launched, TemporarilyUnavailable, Withdrawn, Discontinued |
| `OccurredOn` / `RecordedOnUtc` | the EPIC-005 bitemporal pattern — reuse it verbatim |
| `LaunchDate?` | RIM: conditional |
| `RiskOfSupply`, `RiskOfSupplyComment?` | RIM: bool + conditional text |
| `Note?` | |

### Decisions to settle (Phase 2, on pull-in)

**1. Naming.** RegOS already has `Product`. Options: (a) `Product` stays, add `MedicinalProduct` below; (b) rename `Product` → `GlobalProduct` and add `MedicinalProduct`. *Lean (b)* — RIM's vocabulary is the ubiquitous language, and "Product" next to "MedicinalProduct" will confuse every future reader about which is which. (b) is a mechanical rename plus a migration, cheapest now and never cheaper.

**2. Which tier `RegulatoryApplication` points at.** See the re-pointing table. *Lean: stays global.* This is the decision most likely to be revisited — record the reasoning either way.

**3. New bounded context or second aggregate in `src/Product/`?** *Lean: second aggregate in `src/Product/`*, following the Platform multi-aggregate precedent. The global and local tiers are one ubiquitous-language cluster, always queried together, and a context boundary between them would put a cross-context dependency on the hottest path in the system.

**4. Market status is a dated history, not a field.** Reuse `RegistrationStatusEntry` shape verbatim — append-only, `OccurredOn` vs `RecordedOnUtc`, stored current value for indexed reads. RIM marks Market Status "Single / Historical"; this is where the cross-cutting history rule gets applied first.

**5. Trade name uniqueness.** One per (medicinal product, language)? Or many? *Lean: one per language, enforced* — a market-local product with two simultaneous brand names in one language is a data error, unlike EPIC-005's deliberate multi-registration case. Assert the constraint with a test either way, per the EPIC-005 precedent.

**6. Creating a registration must be able to create its medicinal product.** Otherwise every registration flow gains a mandatory two-step. *Lean: the create-registration handler resolves-or-creates the (product, country) medicinal product.* Keeps the user's one action one action.

### Change-case analysis

| Likely future change | Probability | How the design accommodates it |
|---|---|---|
| Strengths, dosage forms, presentations (EPIC-010) | **High** | Children of `MedicinalProduct` — the tier is precisely what makes them placeable |
| Local labels (EPIC-018) | **High** | Hangs off this tier; blocked without it |
| `Product Family` tier added above | Medium | Inserting **above** a root is one nullable FK — deliberately why it is deferred |
| Several medicinal products per (product, country) | Medium | No uniqueness constraint on the pair — same reasoning as EPIC-005 decision 4 |
| ATC codes become multiple | Medium | Single column now; owned collection later, no data loss |
| A product is marketed under different names in one country over time | Medium | Trade names are child rows; add effective dating if it becomes real |
| MAH transfer | Medium | Holder is a field, not an ownership edge (EPIC-005 decision 5) |

---

## Phase 3 — Candidate stories *(sketch — re-slice on pull-in)*

| # | Story | Slice |
|---|---|---|
| **S001** | **The tier** — `MedicinalProduct` aggregate, the rename (if decision 1 goes that way), re-point `Registration.ProductId`, migration, existing tests re-pointed and green | domain → persistence → API → test |
| **S002** | **Trade Name** — per language, on the medicinal product; surfaced wherever a registration is shown | full slice |
| **S003** | **Market Status** — dated history + current value + launch date + risk of supply; *"is it actually on sale?"* | full slice |
| **S004** | **Capstone** — portfolio views enriched (trade name + market status beside the licence), browser proof, ADR, retro | UI → test → docs |

**ADR to write:** *The market-local product tier, and which tier each reference means* — next free number (expected **ADR-039**).

**Sequencing note:** this epic and **EPIC-004** are genuinely independent — sequences live inside `Submission` and never touch `ProductId`; this never touches submission internals. Neither makes the other harder. Order is a **value call**: this one completes an epic already in flight (EPIC-005); EPIC-004 completes nothing in flight but may be what a customer is waiting on.
