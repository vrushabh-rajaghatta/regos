# Capability Dependency Map

Version: 1.0 (Draft)

---

# Purpose

The Capability Dependency Map defines how modules within the Regulatory Operating System (RegOS) depend on one another.

It establishes the architectural boundaries between modules and determines the order in which capabilities should be implemented.

The dependency map ensures that business capabilities are built on stable foundations and prevents circular dependencies between modules.

---

# Dependency Principles

The following principles govern all module dependencies.

## 1. Dependencies Flow Downward

Dependencies always flow from foundational modules toward user-facing modules.

Modules may depend only on capabilities that appear earlier in the dependency hierarchy.

---

## 2. No Circular Dependencies

Two modules must never depend on each other.

Example:

❌ Invalid

Submission → Validation

Validation → Submission

✔ Valid

Submission → Validation

Validation consumes Submission but Submission has no knowledge of Validation.

---

## 3. Producers Before Consumers

Business modules produce data.

Experience modules consume data.

Consumers never own business state.

Example

✔ Submission produces submissions.

✔ Validation produces validation results.

✔ Workspace consumes both.

---

## 4. Composition Modules Never Own Data

Workspace

Dashboard

Reporting

Search

Analytics

These modules aggregate information from other modules but never own domain entities.

---

## 5. Modules Own Their Business Rules

Every business rule belongs to exactly one module.

Other modules may consume the outcome but never duplicate the logic.

---

# Dependency Hierarchy

```text
Platform
│
├── Organization
├── Users
├── Authentication
├── Authorization
└── Audit
│
▼
Regulatory Configuration
│
├── Countries
├── Authorities
├── Authority Divisions
├── Submission Types
├── Document Types
└── Authority Templates
│
▼
Product Management
│
├── Products
└── Product Lifecycle
│
▼
Application Management
│
├── Applications
└── Application Lifecycle
│
▼
Submission Management
│
├── Submissions
└── Submission Metadata
│
▼
Document Management
│
├── Documents
├── Document Versions
└── eCTD Structure
│
▼
Validation
│
├── Validation Runs
├── Validation Rules
└── Validation Results
│
▼
Review
│
├── Reviews
├── Comments
└── Decisions
│
▼
Publishing
│
├── Published Packages
└── Submission Publishing
│
▼
Workflow
│
├── Tasks
├── Notifications
└── Assignments
│
▼
Activity
│
└── Timeline
│
▼
Experience
│
├── Workspace
├── Dashboard
├── Search
├── Reporting
└── Analytics
```

---

# Module Dependencies

| Module                   | Depends On                                                           |
| ------------------------ | -------------------------------------------------------------------- |
| Platform                 | None                                                                 |
| Regulatory Configuration | Platform                                                             |
| Product Management       | Platform                                                             |
| Application Management   | Product Management, Regulatory Configuration                         |
| Submission Management    | Application Management                                               |
| Document Management      | Submission Management                                                |
| Validation               | Submission Management, Document Management, Regulatory Configuration |
| Review                   | Validation                                                           |
| Publishing               | Review                                                               |
| Workflow                 | Application Management, Submission Management                        |
| Activity                 | All business modules                                                 |
| Experience               | All business modules                                                 |

---

# Build Order

Modules should be implemented in the following sequence.

## Phase 1

Platform

- Organization
- Users
- Authentication
- Authorization
- Audit

---

## Phase 2

Regulatory Configuration

- Countries
- Authorities
- Templates
- Submission Types
- Document Types

---

## Phase 3

Core Regulatory

- Products
- Applications
- Submissions

---

## Phase 4

Submission Processing

- Documents
- Validation
- Review
- Publishing

---

## Phase 5

Operations

- Workflow
- Activity

---

## Phase 6

Experience

- Workspace
- Dashboard
- Search
- Reporting
- Analytics

---

# Allowed Dependency Rules

A module may:

- Consume another module's public APIs
- Consume another module's read models
- Reference another module's identifiers
- Subscribe to another module's events (future)

A module must not:

- Modify another module's entities
- Duplicate another module's business rules
- Directly update another module's database tables
- Depend on a higher-level module

---

# Dependency Validation Checklist

Before introducing a new capability, verify:

- It belongs to an existing module.
- It depends only on lower-level modules.
- It does not introduce circular dependencies.
- It owns its own business rules.
- It exposes clear public APIs.
- It does not duplicate existing functionality.

---

# Architectural Goal

The dependency hierarchy ensures that RegOS evolves as a collection of loosely coupled, highly cohesive modules.

Every module should be independently understandable, independently testable, and independently evolvable while contributing to a unified regulatory platform.
