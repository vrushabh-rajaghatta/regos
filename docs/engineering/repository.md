# Repository Architecture

---
**Title:** Repository Architecture

**Owner:** Architecture Review Board

**Status:** Approved

**Version:** 1.0

**Effective Date:** 2026-07-08

**Last Reviewed:** 2026-07-08

**Next Review:** 2027-07-08

**Related Documents:**
- ENGINEERING.md
- philosophy.md
- README.md

**Related ADRs:**
- None

---

# Purpose

The RegOS repository is organized to reflect the business architecture of the platform rather than the technologies used to implement it.

Its purpose is to make the platform understandable, predictable, and maintainable for both human engineers and AI engineering agents.

Every directory, project, and file exists because it represents an intentional architectural responsibility.

---

# Repository Principles

## Principle 1 — Organize Around the Business

The repository is organized around business capabilities and bounded contexts rather than technical layers.

Engineers should discover business concepts before implementation technologies.

---

## Principle 2 — One Responsibility Per Directory

Every directory has a single architectural responsibility.

Directories should never become collections of unrelated artifacts.

---

## Principle 3 — Business Discoverability Over Technical Discoverability

A new engineer should be able to locate business capabilities without understanding the implementation technology.

Business terminology always takes precedence over framework terminology.

---

## Principle 4 — Platform Contains Engineering Capabilities

The Platform contains engineering primitives and shared technical capabilities.

Business capabilities never belong inside Platform.

---

## Principle 5 — Projects Represent Architectural Boundaries

A project exists only when it represents a meaningful architectural boundary.

Projects are never created simply to organize files.

---

# Repository Structure

The top-level repository is organized as follows.

```text
regos/

├── apps/
├── src/
├── tests/
├── docs/
├── .ai/
├── .github/
├── tools/
├── scripts/
├── docker/
```

## apps/

Contains deployable applications.

Examples include the web application and public API.

Applications expose business capabilities but do not own business logic.

---

## src/

Contains all platform and business source code.

Business capabilities are organized into bounded contexts.

---

## tests/

Contains repository-wide integration, architecture, performance, and end-to-end tests.

Unit tests belong alongside the projects they verify.

---

## docs/

Contains the company's foundational knowledge, architecture, capability specifications, and engineering standards.

---

## .ai/

Contains documentation and standards intended for AI engineering agents.

These documents enable AI to implement architecture consistently.

---

## .github/

Contains repository automation, workflows, templates, and governance configuration.

---

## tools/

Contains engineering utilities such as generators, validators, and development tooling.

---

## scripts/

Contains development and release automation scripts.

---

## docker/

Contains container configurations used during development and deployment.

---

# Solution Structure

Business capabilities are implemented as bounded contexts.

Each bounded context owns its implementation and evolves independently.

Example:

```text
src/

Platform/

Product/

Regulations/

Evidence/

Decision/

Submission/

Change/

Processes/

Connectors/
```

New bounded contexts require architectural approval before implementation.

---

# Project Structure

Every bounded context follows a consistent internal structure.

```text
RegOS.<Context>.Domain

RegOS.<Context>.Application

RegOS.<Context>.Contracts

RegOS.<Context>.Infrastructure
```

Additional projects may only be introduced when they represent a genuine architectural responsibility.

Consistency is preferred over customization.

---

# Dependency Rules

Repository dependencies follow the principles of Clean Architecture.

The dependency direction is always toward the business domain.

```text
API
    │
Application
    │
Domain
    ▲
Infrastructure
```

The following rules apply.

- Domain references no business project.
- Application references Domain.
- Infrastructure implements Domain and Application contracts.
- Applications depend on Application projects rather than Infrastructure.
- Platform never depends on business capabilities.
- Cross-context dependencies require an approved ADR.

---

# Repository Standards

## Standard 1

Business capabilities are first-class citizens.

---

## Standard 2

Platform contains engineering capabilities only.

---

## Standard 3

Shared business code is prohibited.

Business knowledge always has a clear owner.

---

## Standard 4

Generic folders such as:

- Common
- Shared
- Helpers
- Utils
- Misc

are prohibited unless explicitly approved through an ADR.

---

## Standard 5

Repository structure should mirror the business architecture.

Technical convenience must never determine architectural organization.

---

# Adding New Projects

Before introducing a new project, the following questions must be answered.

- Does a genuine architectural boundary exist?
- Is the project aligned with an existing bounded context?
- Has an ADR been approved if a new architectural boundary is introduced?
- Does the project preserve the dependency rules?
- Can the same objective be achieved without introducing another project?

Projects are introduced deliberately.

They are never created for convenience alone.

---

# Repository Checklist

Before adding a new directory or project, verify the following.

- [ ] The responsibility is clearly defined.
- [ ] An existing project cannot reasonably own the functionality.
- [ ] The naming follows business terminology.
- [ ] Dependency rules remain valid.
- [ ] Cross-context references have architectural approval.
- [ ] The repository structure remains aligned with the business architecture.

---

# Change History

| Version | Date | Summary |
|----------|------------|-----------------------------------------|
| 1.0 | 2026-07-08 | Initial approved version. |