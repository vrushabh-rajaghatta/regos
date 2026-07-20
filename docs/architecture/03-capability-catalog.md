# Module & Capability Catalog

Version: 1.0 (Draft)

---

# Purpose

The Module & Capability Catalog is the authoritative inventory of all business modules and capabilities within the Regulatory Operating System (RegOS).

It defines how the product is organized from a business perspective and provides a single source of truth for planning, implementation, and future enhancements.

Unlike the Product Vision, which describes what RegOS is, and the Domain Model, which defines the business entities, this document describes **what RegOS can do**.

---

# Capability Lifecycle

Every capability progresses through the following lifecycle.

| Status         | Description                                   |
| -------------- | --------------------------------------------- |
| Planned        | Identified but not yet designed               |
| Designing      | Discovery and design in progress              |
| Approved       | Design approved for implementation            |
| In Development | Currently being implemented                   |
| Completed      | Implementation complete                       |
| Frozen         | Stable and maintained through change requests |

---

# Platform Module

Provides the foundational capabilities required by RegOS.

| Capability              | Description                 | Status       |
| ----------------------- | --------------------------- | ------------ |
| Organization Management | Manage tenant organizations | ✅ Completed |
| User Management         | Manage users                | ⏳ Planned   |
| Role Management         | Manage security roles       | ⏳ Planned   |
| Permission Management   | Manage permissions          | ⏳ Planned   |
| Authentication          | User authentication         | ⏳ Planned   |
| Audit Management        | System audit trail          | ⏳ Planned   |

---

# Reference Data Module

Defines the regulatory landscape supported by RegOS.

| Capability                    | Description                                      | Status     |
| ----------------------------- | ------------------------------------------------ | ---------- |
| Country Management            | Manage supported countries                       | ⏳ Planned |
| Authority Management          | Manage regulatory authorities                    | ⏳ Planned |
| Authority Division Management | Manage authority divisions                       | ⏳ Planned |
| Submission Type Management    | Manage submission types                          | ⏳ Planned |
| Document Type Management      | Manage document classifications                  | ⏳ Planned |
| Authority Template Management | Maintain authority-specific submission templates | ⏳ Planned |
| Validation Rule Management    | Maintain validation rules                        | ⏳ Planned |

---

# Product Management Module

Owns regulated products.

| Capability                 | Description                  | Status       |
| -------------------------- | ---------------------------- | ------------ |
| Create Product             | Create regulated products    | ✅ Completed |
| View Product               | Retrieve product information | ✅ Completed |
| Update Product             | Update product information   | ✅ Completed |
| Archive Product            | Archive products             | ⏳ Planned   |
| Product Lifecycle          | Manage product lifecycle     | ⏳ Planned   |
| Product Version Management | Manage product versions      | ⏳ Planned   |

---

# Application Management Module

Owns regulatory applications.

| Capability            | Description                       | Status            |
| --------------------- | --------------------------------- | ----------------- |
| Create Application    | Create applications               | ✅ Completed      |
| View Application      | Retrieve application information  | ✅ Completed      |
| Update Application    | Update applications               | ✅ Completed      |
| Archive Application   | Archive applications              | ⏳ Planned        |
| Clone Application     | Duplicate applications            | ⏳ Planned        |
| Application Workspace | Aggregate application information | 🚧 In Development |

---

# Submission Management Module

Owns regulatory submissions.

| Capability           | Description                 | Status       |
| -------------------- | --------------------------- | ------------ |
| Create Submission    | Create submissions          | ✅ Completed |
| View Submission      | Retrieve submissions        | ✅ Completed |
| Update Submission    | Update submissions          | ✅ Completed |
| Clone Submission     | Duplicate submissions       | ⏳ Planned   |
| Submission Lifecycle | Manage submission lifecycle | ⏳ Planned   |
| Submission Metadata  | Manage submission metadata  | ⏳ Planned   |

---

# Document Management Module

Owns submission documents.

| Capability               | Description                 | Status     |
| ------------------------ | --------------------------- | ---------- |
| Upload Document          | Upload submission documents | ⏳ Planned |
| View Document            | Retrieve documents          | ⏳ Planned |
| Update Document Metadata | Maintain metadata           | ⏳ Planned |
| Version Documents        | Manage document versions    | ⏳ Planned |
| Document Relationships   | Manage document references  | ⏳ Planned |
| eCTD Structure           | Manage eCTD hierarchy       | ⏳ Planned |

---

# Validation Module

Validates submissions before review and publishing.

| Capability         | Description                   | Status     |
| ------------------ | ----------------------------- | ---------- |
| Execute Validation | Run validation engine         | ⏳ Planned |
| Validation Rules   | Execute authority rules       | ⏳ Planned |
| Validation Results | Store validation outcomes     | ⏳ Planned |
| Validation History | View previous validation runs | ⏳ Planned |

---

# Review Module

Supports internal regulatory review.

| Capability        | Description               | Status     |
| ----------------- | ------------------------- | ---------- |
| Create Review     | Start review process      | ⏳ Planned |
| Review Comments   | Capture reviewer comments | ⏳ Planned |
| Review Decisions  | Record review decisions   | ⏳ Planned |
| Approval Workflow | Approve submissions       | ⏳ Planned |

---

# Publishing Module

Produces authority-ready submission packages.

| Capability                   | Description                 | Status     |
| ---------------------------- | --------------------------- | ---------- |
| Publish Submission           | Generate submission package | ⏳ Planned |
| Publication History          | Track published submissions | ⏳ Planned |
| Published Package Management | Maintain published packages | ⏳ Planned |

---

# Workflow Module

Coordinates regulatory work.

| Capability              | Description       | Status     |
| ----------------------- | ----------------- | ---------- |
| Task Management         | Manage work items | ⏳ Planned |
| Assignment Management   | Assign work       | ⏳ Planned |
| Due Date Management     | Manage deadlines  | ⏳ Planned |
| Notification Management | Notify users      | ⏳ Planned |

---

# Activity Module

Records business events.

| Capability        | Description                   | Status     |
| ----------------- | ----------------------------- | ---------- |
| Activity Timeline | Record business events        | ⏳ Planned |
| Audit Timeline    | Display chronological history | ⏳ Planned |

---

# Experience Module

Provides composed views across multiple modules.

| Capability            | Description              | Status            |
| --------------------- | ------------------------ | ----------------- |
| Application Workspace | Unified application view | 🚧 In Development |
| Dashboard             | Operational dashboard    | ⏳ Planned        |
| Global Search         | Search across RegOS      | ⏳ Planned        |
| Reporting             | Business reporting       | ⏳ Planned        |
| Analytics             | Operational analytics    | ⏳ Planned        |

---

# Ownership Rules

Every capability belongs to exactly one module.

A module owns:

- Business rules
- APIs
- Domain entities
- Validation
- Permissions

No capability may exist outside a module.

---

# Composition Rule

Capabilities within the Experience Module never own business data.

They consume information from business modules and present a unified user experience.

---

# Evolution

The Module & Capability Catalog is expected to evolve throughout the lifetime of RegOS.

Adding a new capability requires:

- Assigning it to an existing module (or introducing a new module)
- Defining its responsibilities
- Identifying dependencies
- Updating the Capability Dependency Map
- Including it in the Implementation Roadmap

This document serves as the master inventory of functionality within RegOS.
