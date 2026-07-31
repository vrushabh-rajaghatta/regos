# Repository Architecture

---
**Title:** Repository Architecture

**Owner:** Architecture Review Board

**Status:** Approved

**Version:** 1.1

**Effective Date:** 2026-07-08

**Last Reviewed:** 2026-07-31

**Next Review:** 2027-07-08

**Related Documents:**
- ENGINEERING.md
- philosophy.md
- README.md
- slice-conventions.md

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

├── src/
├── web/
├── tests/
├── docs/
├── CLAUDE.md
```

> **Corrected 2026-07-31.** Version 1.0 of this document described `apps/`,
> `.ai/`, `.github/`, `tools/`, `scripts/` and `docker/`. None were ever
> created. They are recorded under *Planned Structure* below rather than
> described as if they exist — a structural document that is wrong about the
> basics teaches engineers to stop reading structural documents.

## web/

Contains the deployable web application (`regos-web`), organized feature-first.

Applications expose business capabilities but do not own business logic.

Version 1.0 named this `apps/`, anticipating more than one deployable. One
exists, so the directory is named for it. It becomes `apps/` when a second
arrives.

---

## CLAUDE.md

Repository instructions for AI engineering agents, loaded automatically.

It is deliberately thin and points at the canon rather than restating it — a
second copy of the standards would drift from the first. Version 1.0 proposed
a `.ai/` directory for this purpose, before the convention of a root
`CLAUDE.md` settled.

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

`docs/adr/` is the single immutable decision series. `docs/engineering/` holds
the standards, including [slice-conventions.md](slice-conventions.md), which
specifies file and folder layout inside a bounded context and is enforced by
`tests/Architecture/RegOS.Architecture.Tests`.

---

# Planned Structure

The following do not exist yet. They are recorded as intent, and each should be
created when it has a first real occupant — not in advance
([ADR-018](../adr/ADR-018-rule-of-three.md)).

| Directory | Purpose | Created when |
|---|---|---|
| `.github/` | Workflows, PR templates, governance | CI runs on push — the natural home for the architecture tests |
| `tools/` | Generators, validators, dev tooling | A first tool exists |
| `scripts/` | Development and release automation | A first script outgrows a README snippet |
| `docker/` | Container configuration | Deployment stops being local-only |
| `apps/` | Replaces `web/` | A second deployable exists |

---

# Solution Structure

Business capabilities are implemented as bounded contexts.

Each bounded context owns its implementation and evolves independently.

The contexts that exist today:

```text
src/

Platform/                 identity, tenancy, sessions — engineering capabilities
Organization/             the regulatory party, its sites and contacts
Product/
ProductDocument/
RegulatoryApplication/
Submission/
Registration/
ReferenceData/            countries, authorities, document types, templates
Persistence/              RegOSDbContext, EF configuration, migrations
Shared/                   RegOS.SharedKernel (ADR-017 scope only)
```

> Version 1.0 listed `Regulations/`, `Evidence/`, `Decision/`, `Change/`,
> `Processes/` and `Connectors/` as examples. None were built, and the domain
> was carved differently once the first vertical was real. The list above is
> the actual solution, not an aspiration.

New bounded contexts require architectural approval before implementation.

---

# Project Structure

Every bounded context follows a consistent internal structure.

```text
RegOS.<Context>.Domain

RegOS.<Context>.Application

RegOS.<Context>.Infrastructure
```

> Version 1.0 also mandated `RegOS.<Context>.Contracts`. No context has one.
> Cross-context reads have not yet needed a published contract surface, and
> `IProductReader` — built for exactly that and never consumed — was deleted
> along with its project ([ADR-018](../adr/ADR-018-rule-of-three.md)). The
> project returns when a real cross-context consumer exists.

The internal layout of these projects — folders, filenames, where a repository
interface goes — is specified by
[slice-conventions.md](slice-conventions.md) and enforced by test.

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
| 1.1 | 2026-07-31 | Corrected to describe the repository that exists. Unbuilt directories moved to *Planned Structure*; context list and project structure replaced with actual; linked slice-conventions.md. |