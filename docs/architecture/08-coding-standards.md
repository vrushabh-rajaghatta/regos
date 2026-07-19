# Coding Standards

Version: 1.0 (Draft)

---

# Purpose

The Coding Standards define the implementation conventions for the Regulatory Operating System (RegOS).

These standards ensure that code remains consistent, maintainable, and easy to understand regardless of who contributes to the project.

Consistency is preferred over personal coding style.

---

# General Principles

- Write code for humans first.
- Prefer readability over cleverness.
- Keep implementations explicit.
- Minimize hidden behavior.
- Follow existing patterns before introducing new ones.

---

# Project Structure

The solution is organized by **Module**, not by technical layer.

Example

```
src/

Platform/
    Organization/
    Users/
    Roles/

ProductManagement/
    CreateProduct/
    UpdateProduct/

ApplicationManagement/
    CreateApplication/
    Workspace/

SubmissionManagement/
    CreateSubmission/
    Validation/

Experience/
    Dashboard/
    Workspace/
```

Each module owns its complete implementation.

---

# Feature Structure

Each capability should remain self-contained.

Example

```
CreateProduct/

    CreateProductCommand.cs

    CreateProductHandler.cs

    CreateProductValidator.cs

    CreateProductEndpoint.cs

    CreateProductTests.cs
```

Avoid spreading implementation across unrelated folders.

---

# Naming Conventions

Use business terminology.

Good

```
CreateSubmissionCommand

PublishSubmissionHandler

ApplicationWorkspaceDto

ValidationRun
```

Avoid technical names.

Bad

```
SubmissionProcessor

SubmissionUtility

DataManager

HelperClass
```

Names should describe business intent.

---

# Classes

One class should have one responsibility.

Prefer

```
CreateSubmissionHandler
```

Over

```
SubmissionService
```

Large service classes should be split into focused handlers.

---

# Methods

Methods should:

- Perform one logical operation.
- Have descriptive names.
- Avoid deep nesting.
- Return early where appropriate.

Avoid methods that exceed approximately 50 lines unless complexity clearly justifies it.

---

# CQRS Conventions

Commands

- Modify business state.
- Return identifiers or simple results.
- Execute business rules.

Queries

- Never modify business state.
- Build read models.
- Optimize for retrieval.

Command and query handlers should remain independent.

---

# Controllers

Controllers should:

- Accept requests.
- Validate input.
- Delegate work.
- Return responses.

Controllers must not:

- Execute business rules.
- Access the database directly.
- Perform orchestration.

---

# Dependency Injection

Use constructor injection exclusively.

Avoid:

- Service locators
- Static dependencies
- Global state

Dependencies should be explicit.

---

# Validation

Business validation belongs inside the owning module.

Request validation should occur before business execution.

Avoid duplicating validation across multiple modules.

---

# Entity Framework

Guidelines

- Use DbContext only within the application layer.
- Keep queries explicit.
- Avoid unnecessary eager loading.
- Use projections for read models.
- Track entities only when updates are required.

---

# DTOs

DTOs represent API contracts.

Rules

- Never expose entities directly.
- Keep DTOs immutable where practical.
- Use descriptive names.

Examples

```
ProductDto

ApplicationSummaryDto

SubmissionWorkspaceDto
```

---

# Error Handling

Use exceptions only for exceptional situations.

Business validation failures should return structured validation results.

Do not suppress exceptions silently.

---

# Logging

Log meaningful business events.

Examples

- Submission created.
- Validation started.
- Submission published.

Avoid excessive debug logging in production code.

Sensitive information must never be logged.

---

# Testing

Every capability should include tests.

Minimum expectations

- Unit tests for business logic.
- Integration tests for APIs.
- Regression tests for resolved defects.

Tests should describe business behavior rather than implementation details.

---

# Comments

Prefer self-explanatory code.

Use comments only when explaining:

- Business rationale
- Regulatory requirements
- Non-obvious implementation decisions

Avoid comments that repeat the code.

---

# Documentation

When implementing a capability:

- Update architecture documents if required.
- Update API documentation.
- Update README files when behavior changes.

Documentation is considered part of the implementation.

---

# Code Review Checklist

Every pull request should verify:

- Architecture follows module boundaries.
- Naming follows business terminology.
- CQRS conventions are respected.
- Tests are included.
- Documentation is updated.
- No duplicated business logic.
- Dependencies remain one-directional.

---

# Refactoring

Refactoring is encouraged when it:

- Improves readability.
- Reduces duplication.
- Simplifies implementation.
- Preserves behavior.

Large architectural refactoring should be planned rather than performed opportunistically.

---

# Guiding Principle

Write code that another developer can understand quickly, modify confidently, and extend safely.

Code should communicate business intent first and technical implementation second.
