# Product

---

Title: Product Domain Model

Owner: Architecture Review Board

Status: Approved

Version: 1.1

Last Reviewed: 2026-07-31

Related Documents:

- business-modeling.md

Related ADRs:

- ADR-0002

---

# The two tiers

Since EPIC-017, the Product context holds **two** aggregates, and every
statement below about "a Product" means the **global** one unless it says
otherwise.

| Tier | Aggregate | Answers |
| --- | --- | --- |
| Global | `GlobalProduct` | *What product is this?* — one identity for the whole world |
| Market-local | `MedicinalProduct` | *What is this product, here?* — one jurisdiction |

The dependency runs one way and only one way:

```
GlobalProduct → MedicinalProduct → Registration
```

A `Registration` names **only** a `MedicinalProduct`. It carries neither the
global product nor the country, because both are the medicinal product's facts
and a second copy could disagree with them.

Several medicinal products may exist for one (global product, country) pair —
presentations, strengths, the two halves of a partial divestment — so nothing
enforces uniqueness on the pair, and nothing resolves-or-creates one on a
caller's behalf.

---

# Vocabulary — domain word and screen word

They differ here on purpose, and both are binding.

| Domain (code, ADRs, this document) | UI (navigation, labels, headings) |
| --- | --- |
| `MedicinalProduct` | **Market** |
| `GlobalProduct` | **Product** |
| `PharmaceuticalProductDetail` | **Presentation** |
| `Registration` | **Registration** / *market authorisation* in prose |
| `PackagedProduct` | **Pack** |
| `PackageItem` | **What's inside** |
| `LegalStatusOfSupply` | **Legal status** |
| `ShelfLifeStorage` | **Shelf life & storage** |
| `PhysicalCharacteristics` | **Appearance** |

RIM's `Medicinal Product` keeps the model precise and is what a future
contributor will search for. **"Market"** is what a regulatory user says out
loud — *"we're in Canada"* — and it is what the screens show: a product page
lists its **Markets**, and an authorisation is recorded against one.

`PharmaceuticalProductDetail` is IDMP's term for the administrable form — what
the product physically *is*, as against what it is called or whether it is on
sale. Nobody says it out loud. **"Presentation"** is the word a regulatory user
uses for the same thing, and it is what the market page shows.

`PackagedProduct` is what a market sells — a carton of thirty. **"Pack"** is
what anyone says, and the route keeps the domain noun
(`/api/packaged-products`) while the screen keeps the spoken one. `PackageItem`
is IDMP's noun for a layer of that pack, deliberately chosen over RIM's
`Packaging` and over `PackagingComponent`, which would reuse the exact word that
means the *other* recursive tree
([ADR-061](../adr/ADR-061-a-pack-is-how-a-medicine-is-supplied.md) §5).

The screen's word must never reach a type, and the type's word must never reach
a label without a reason to prefer it.

---

# Purpose

A Product represents the long-lived business identity of a regulated product.

It exists independently of individual Product Versions and serves as the parent aggregate for all versions created throughout the product's lifecycle.

---

# Business Definition

A Product is the business identity under which one or more Product Versions are managed.

The Product itself contains only information that remains true across all Product Versions.

Version-specific regulatory content belongs to Product Versions.

---

# Responsibilities

The Product aggregate is responsible for:

- Owning Product identity
- Owning the Product business lifecycle
- Managing Product Versions
- Protecting Product invariants

---

# Does Not Own

The Product aggregate does not own:

- Claims
- Intended Use
- Characteristics
- Regulatory Evidence
- Regulatory Approvals
- Regulatory Submissions

These belong to Product Versions or other aggregates.

---

# Aggregate Rules

1. A Product owns only the business concepts that must remain consistent together.

2. A Product can exist before its first Product Version.

3. Every Product Version belongs to exactly one Product.

4. A Product owns its business lifecycle independently of its Product Versions.

5. Product Versions own their regulatory lifecycle independently of the Product.

---

# Lifecycle

The Product lifecycle represents the business existence of the Product.

The Product lifecycle is independent of Product Version lifecycles.

The exact lifecycle states will be defined during implementation.

---

## Value Objects

### GlobalProductId

Represents the immutable identity of a global Product. Named `ProductId` until
EPIC-017 S000, which renamed the type so the two tiers could not be confused.
The `Products` **table** deliberately kept its name.

### MedicinalProductId

Represents the immutable identity of a market-local product. Explicit on
`CreateRegistrationCommand`: a registration names the medicinal product it is
granted over, and never a (product, country) pair to be resolved.

### ProductName

Represents the business name of a Product.

Business Rules:

- Cannot be empty.
- Leading and trailing whitespace is removed.
- Normalized values represent the same business value.
- Uniqueness is enforced outside the Value Object.

## Product Status

A Product has a business lifecycle.

States:

- Registered
- Active
- Archived

Business Rules:

- Every newly registered Product starts in the Registered state.
- A Registered Product may be Archived.
- An Archived Product cannot return to Registered without an explicit Restore Product capability.

## Product Type

Represents the regulatory classification of a Product.

Business Rules

- Every Product has exactly one ProductType.
- ProductType is assigned during registration.
- ProductType cannot be changed.
- Supported values:
  - Medical Device
  - Drug
  - Biologic
  - Combination Product
  - IVD

---

# Relationships

Product

├── owns → Product Versions

└── belongs to → Organization

---

# Business Events

Examples include:

- Product Registered
- Product Archived
- Product Ownership Changed

Additional events will be introduced as capabilities are implemented.

---

# Future Capabilities

Planned capabilities include:

- Register Product
- Update Product
- Archive Product
- Create Product Version
- List Product Versions

---

# Open Questions

- What business states make up the Product lifecycle?
- Can Product ownership be transferred between Organizations?
- Can a Product ever be restored after archival?

---

# Change History

| Version | Date       | Summary                       |
| ------- | ---------- | ----------------------------- |
| 1.1     | 2026-07-31 | EPIC-017: the two tiers, the `GlobalProduct`/`MedicinalProduct` split, and the domain-word/screen-word pair. |
| 1.0     | 2026-07-09 | Initial Product domain model. |
