# Register Product

---

Title: Register Product Capability

Owner: Architecture Review Board

Status: Approved

Version: 1.0

Last Reviewed: 2026-07-09

Related Domain Models:

- product.md

Related ADRs:

- ADR-0002

---

# Purpose

Register a new Product within an Organization.

Registration establishes the long-lived business identity of the Product.

Registration does not create regulatory content.

---

# Business Goal

Allow an Organization to establish a Product that can later evolve through one or more Product Versions.

---

# Primary Actor

Regulatory Affairs User

---

# Preconditions

- The user belongs to an Organization.
- The user has permission to register Products.
- The Organization exists.

---

# Inputs

The minimum information required to register a Product.

Current assumptions:

- ProductName
- ProductType
- Owning Organization

Additional inputs may be introduced after further domain discovery.

---

# Business Rules

1. A Product receives a unique ProductId.

2. A Product belongs to exactly one Organization.

3. Registration creates the Product only.

4. Registration does not create a Product Version.

5. Registration does not create Claims, Intended Use, Characteristics, Evidence, or Submissions.

6. A Product may exist without any Product Versions.

7. ProductName must satisfy all ProductName business rules.

---

# Process

1. User initiates Product registration.
2. Business validates permissions.
3. Product identity is created.
4. Product is associated with the Organization.
5. Product is persisted.
6. Product Registered event is produced.

---

# Success Outcome

A new Product exists within the Organization.

The Product is ready for future Product Versions.

After successful registration:

- A Product exists.
- The Product is in the Registered state.
- The Product is ready for future Product Versions.

---

# Failure Scenarios

Registration fails when:

- The user lacks permission.
- The Organization cannot be resolved.
- Business validation fails.

---

# Domain Changes

Creates:

- Product

No other aggregates are modified.

---

# Business Events

- Product Registered

---

# Future Enhancements

Potential future enhancements include:

- Product numbering strategies
- Product templates
- Duplicate detection
- Product categories
- Audit enhancements

---

# Open Questions

- Is Product Name mandatory at registration?
- Can Product ownership be transferred?
- Should duplicate Product Names be allowed within the same Organization?
- Should Product Codes be introduced as a business identifier?

---

# Change History

| Version | Date       | Summary                        |
| ------- | ---------- | ------------------------------ |
| 1.0     | 2026-07-09 | Initial capability definition. |
