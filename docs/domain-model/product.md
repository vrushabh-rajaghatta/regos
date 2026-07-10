# Product

---

Title: Product Domain Model

Owner: Architecture Review Board

Status: Approved

Version: 1.0

Last Reviewed: 2026-07-09

Related Documents:

- business-modeling.md

Related ADRs:

- ADR-0002

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

### ProductId

Represents the immutable identity of a Product.

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
| 1.0     | 2026-07-09 | Initial Product domain model. |
