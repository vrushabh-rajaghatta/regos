# Engineering Principles

Version: 1.0 (Draft)

---

# Purpose

The Engineering Principles define the architectural and engineering standards that guide the implementation of the Regulatory Operating System (RegOS).

These principles exist to ensure that every module, capability, and implementation follows a consistent approach regardless of who develops it.

When implementation decisions are unclear, these principles should be consulted before introducing new patterns.

---

# Core Philosophy

RegOS is designed as a modular, business-oriented platform.

The architecture emphasizes:

- High cohesion
- Loose coupling
- Clear ownership
- Explicit dependencies
- Predictable implementation
- Long-term maintainability

Every engineering decision should reinforce these goals.

---

# Principle 1 — Business First

Technology exists to support the business—not the other way around.

Business capabilities drive the architecture.

Avoid introducing technical abstractions that do not solve a business problem.

---

# Principle 2 — Single Ownership

Every business concept has exactly one owner.

Examples

- Product owns products.
- Application owns applications.
- Submission owns submissions.
- Validation owns validation results.

Other modules consume the information but never own it.

---

# Principle 3 — One Module, One Responsibility

Modules should have a single, well-defined responsibility.

A module should answer one business question.

Good examples:

- Product Management
- Validation
- Publishing

Poor examples:

- Utilities
- Common Services
- General Management

If a module cannot be described in one sentence, it is probably too broad.

---

# Principle 4 — Producers Before Consumers

Modules that create business data are implemented before modules that present or analyze that data.

Producer examples

- Product Management
- Submission Management
- Validation
- Review

Consumer examples

- Workspace
- Dashboard
- Reporting
- Analytics

---

# Principle 5 — Composition Does Not Own Data

Composition modules aggregate information from multiple modules.

They never:

- Modify business entities
- Execute business rules owned elsewhere
- Become the source of truth

Examples

- Application Workspace
- Dashboard
- Reporting

---

# Principle 6 — Public APIs Only

Modules interact through well-defined public interfaces.

A module must never:

- Reach into another module's internal implementation
- Access another module's database directly
- Modify another module's entities

All communication should occur through published contracts.

---

# Principle 7 — Explicit Dependencies

Dependencies should always be visible and intentional.

Avoid:

- Hidden dependencies
- Global state
- Service locators
- Circular references

Developers should be able to understand a module's dependencies by inspecting its constructor and public interfaces.

---

# Principle 8 — CQRS by Default

Commands modify state.

Queries retrieve state.

Queries should never modify business data.

Commands should not return complex read models.

This separation keeps business logic predictable and simplifies testing.

---

# Principle 9 — Thin Controllers

Controllers exist only to:

- Validate requests
- Delegate to handlers
- Return responses

Business logic belongs in the application layer.

Controllers should contain no business decisions.

---

# Principle 10 — Business Rules Live with the Owner

Validation and business rules belong to the module that owns the business concept.

Examples

Submission Management

- Submission status transitions
- Submission metadata rules

Validation

- Authority rule execution
- Validation issue generation

Publishing

- Package creation
- Publication rules

Rules should never be duplicated across modules.

---

# Principle 11 — Design Before Code

Every capability should be designed before implementation.

Design includes:

- Business objectives
- APIs
- Domain changes
- Database changes
- Security
- Dependencies

Implementation follows an approved design.

---

# Principle 12 — Prefer Explicit Code

Readable code is preferred over clever code.

Good

- Descriptive names
- Small methods
- Clear intent

Avoid

- Excessive abstraction
- Hidden behavior
- Clever optimizations without evidence

Future maintainability is more important than reducing a few lines of code.

---

# Principle 13 — Favor Composition Over Duplication

Shared behavior should be composed through reusable services or abstractions when it represents a stable concept.

Avoid copying business logic between modules.

However, do not create abstractions prematurely.

---

# Principle 14 — Documentation is Part of the Deliverable

Implementation is not complete until documentation is updated.

Changes to modules, capabilities, architecture, or public APIs must be reflected in the appropriate architecture documents.

Documentation and code evolve together.

---

# Principle 15 — Freeze Before Expanding

Complete work before starting new work.

A capability should be:

- Implemented
- Reviewed
- Approved
- Frozen

Only then should the team expand into dependent capabilities.

This reduces rework and architectural drift.

---

# Principle 16 — Build for Evolution

Design modules so they can evolve independently.

Avoid assumptions that prevent future enhancements.

Examples

- New authorities
- New submission types
- Additional publishing formats
- AI-assisted workflows
- External integrations

Future growth should require extension rather than redesign.

---

# Architectural Checklist

Before implementing a new capability, verify:

- Does it belong to an existing module?
- Does the owning module make business sense?
- Are dependencies one-directional?
- Is business logic located with the owner?
- Is the implementation consistent with CQRS?
- Are composition modules read-only?
- Has the capability been designed and approved?
- Will the implementation remain understandable six months from now?

If any answer is "No", revisit the design before writing code.

---

# Guiding Statement

Every line of code should make RegOS easier to understand, easier to extend, and easier to maintain.

Architecture is not measured by the sophistication of the solution, but by how naturally future developers can continue building upon it.
