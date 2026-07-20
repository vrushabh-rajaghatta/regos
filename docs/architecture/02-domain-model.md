# Domain Model

Version: 1.0 (Draft)

---

# Purpose

This document defines the core business domain of the Regulatory Operating System (RegOS).

It identifies the primary business entities, their ownership, relationships, and aggregate boundaries. The domain model provides a shared language for architects, developers, product owners, QA engineers, and regulatory subject matter experts.

This document intentionally avoids implementation details such as database design, APIs, or user interfaces.

---

# Domain Principles

The RegOS domain follows several core principles.

## Business Ownership

Every business entity belongs to exactly one module.

Only the owning module is responsible for creating, updating, and enforcing the business rules for that entity.

Other modules may consume the entity but must not own its lifecycle.

---

## Aggregate Boundaries

Each aggregate protects its own consistency.

Changes across aggregates should occur through application services rather than direct manipulation.

---

## Composition

Consumer modules (Workspace, Dashboard, Reporting, Search) compose information from multiple aggregates but never own business state.

---

# Core Domain

The RegOS business domain is centered around the lifecycle of a regulatory submission.

```
Organization
    │
    ├── Users
    │
    └── Products
            │
            └── Applications
                    │
                    └── Submissions
                            │
                            ├── Documents
                            ├── Validation Runs
                            ├── Reviews
                            ├── Published Packages
                            ├── Tasks
                            └── Activities
```

This represents the natural flow of regulatory work.

---

# Domain Modules

## Platform

Owns the platform and tenancy.

### Aggregate Root

Organization

### Entities

- Organization

### Future Entities

- User
- Role
- Permission
- Authentication Settings
- Audit Configuration

---

## Reference Data

Defines the regulatory landscape supported by RegOS.

### Aggregate Roots

- Country
- Authority
- Authority Template

### Entities

Country

Represents a regulatory market.

Authority

Represents a health authority.

Examples:

- FDA
- EMA
- MHRA
- CDSCO

Authority Division

Represents organizational divisions within an authority.

Examples:

- CDRH
- CDER
- CBER

Submission Type

Defines the type of regulatory submission.

Examples:

- 510(k)
- PMA
- NDA
- IND

Document Type

Defines document classifications.

Authority Template

Defines the submission requirements for an authority.

Includes:

- Required documents
- Validation rules
- Submission structure
- Lifecycle rules

---

## Product Management

Owns regulated products.

### Aggregate Root

Product

### Child Entities

Future examples include:

- Product Variant
- Manufacturer
- Product Classification
- Market Registration

### Relationships

One Organization owns many Products.

One Product may have many Applications.

---

## Application Management

Owns regulatory applications.

### Aggregate Root

Application

Applications represent a regulatory filing for a product within a specific market and authority.

### Relationships

One Product

↓

Many Applications

One Application

↓

Many Submissions

---

## Submission Management

Owns regulatory submissions.

### Aggregate Root

Submission

Represents a single regulatory submission sequence.

### Child Entities

Potential future entities:

- Submission Sequence
- Submission Metadata
- Submission Lifecycle
- Submission Status History

### Relationships

One Submission

↓

Many Documents

---

## Document Management

Owns submission documents.

### Aggregate Root

Document

Documents are versioned business assets associated with a submission.

### Child Entities

Potential future entities:

- Document Version
- Metadata
- Storage Reference
- eCTD Node
- Attachments

---

## Validation

Owns submission validation.

### Aggregate Root

Validation Run

### Entities

- Validation Result
- Validation Issue
- Validation Summary

Consumes:

- Submission
- Documents
- Authority Template

Produces:

- Validation Outcomes

---

## Review

Owns regulatory review.

### Aggregate Root

Review

### Entities

- Review Decision
- Review Comment
- Approval

Consumes:

- Submission
- Validation Results

Produces:

- Review Outcome

---

## Publishing

Owns published submissions.

### Aggregate Root

Published Package

### Entities

- Published Submission
- Published Sequence
- Published Document

Consumes:

- Submission
- Review
- Validation

Produces:

Authority-ready submission packages.

---

## Workflow

Owns work coordination.

### Aggregate Root

Task

### Entities

- Assignment
- Approval
- Due Date
- Notification

Consumes:

- Applications
- Submissions

---

## Activity

Owns business events.

### Aggregate Root

Activity

### Entities

Future examples include:

- Timeline Event
- Audit Event
- User Action

Activities provide a chronological history of the regulatory process.

---

# Composition Modules

These modules do not own business entities.

Instead, they aggregate information from multiple modules.

## Application Workspace

Composes:

- Application
- Submission
- Validation
- Publishing
- Activity

Owns:

Nothing.

---

## Dashboard

Composes:

- Tasks
- Activities
- Notifications
- KPIs

Owns:

Nothing.

---

## Search

Indexes information across business modules.

Owns:

Nothing.

---

## Reporting

Aggregates business data.

Owns:

Nothing.

---

# Aggregate Relationships

```
Organization
    │
    ├── Products
    │       │
    │       └── Applications
    │               │
    │               └── Submissions
    │                       │
    │                       ├── Documents
    │                       ├── Validation Runs
    │                       ├── Reviews
    │                       ├── Published Packages
    │                       ├── Tasks
    │                       └── Activities
    │
    └── Users
```

---

# Ownership Matrix

| Business Entity    | Owning Module          |
| ------------------ | ---------------------- |
| Organization       | Platform               |
| User               | Platform               |
| Country            | Reference Data         |
| Authority          | Reference Data         |
| Authority Template | Reference Data         |
| Product            | Product Management     |
| Application        | Application Management |
| Submission         | Submission Management  |
| Document           | Document Management    |
| Validation Run     | Validation             |
| Validation Result  | Validation             |
| Review             | Review                 |
| Published Package  | Publishing             |
| Task               | Workflow               |
| Activity           | Activity               |

---

# Business Rules

The following rules govern the domain model.

- Every Product belongs to one Organization.
- Every Application belongs to one Product.
- Every Submission belongs to one Application.
- Every Document belongs to one Submission.
- Validation is performed against a Submission.
- Review is performed after validation.
- Publishing occurs only after successful review.
- Activities record significant business events but do not modify business state.
- Composition modules never own business entities.

---

# Evolution

The domain model is expected to evolve as RegOS grows.

New capabilities should integrate into the existing domain by:

- identifying the owning module,
- defining aggregate ownership,
- documenting dependencies,
- avoiding duplication of existing business entities.

The domain model should remain the authoritative representation of the business concepts within RegOS.
