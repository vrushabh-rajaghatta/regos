# RegOS Foundation — Vision

**Scope of this document:** why the Foundation exists, what belongs in it, and
where its boundaries are. It does not describe classes, tables, or schedules.
For decisions see [`docs/adr/`](../adr/README.md); for rules see
[`principles.md`](principles.md); for remaining work see
[`roadmap.md`](roadmap.md).

Every statement here is tagged:

| Tag | Meaning |
|---|---|
| **Current** | True of the code today. |
| **Accepted** | Decided; implementation may be incomplete. |
| **Proposed** | Under discussion. Binding on nobody. |

---

## 1. Purpose

The Foundation provides the capabilities every functional module in RegOS
depends on — identity, tenancy, security, and operational concerns — so that
business modules can concentrate on regulatory work.

It exists to answer:

- Who is the current user?
- Which organization is this request scoped to?
- Is this user allowed to do this?
- How did this account authenticate?
- What happened, and who did it?

It does not answer:

- What is a submission?
- What is a product?
- What is an application?

Those belong to the Regulatory domain.

---

## 2. Structure

**Current.** The Foundation is two sibling modules, not one:

```
Foundation
├── Organization      — tenant boundary and regulatory party
└── Platform          — identity, authentication, authorization, audit
        ▲
        │  depends on
        │
Regulatory Domain
    Product · ProductDocument · RegulatoryApplication · Submission
```

This mirrors the repository: `src/Organization/` is a sibling of `src/Platform/`,
not a child.

### Why Organization is not inside Platform

**Accepted — [ADR-015](../adr/ADR-015-organization-is-the-tenant.md).**

`Organization` carries two responsibilities at once:

1. **Tenant boundary.** `ITenantContext.TenantId` resolves to an organization id;
   `User.OrganizationId` binds a user to its tenant.
2. **Regulatory party.** `OrganizationType` is `Manufacturer | Sponsor |
   MarketingAuthorizationHolder | ContractResearchOrganization` — a regulatory
   taxonomy that a generic SaaS tenant would have no reason to know.

This is a description, not an aspiration. The dual responsibility is accepted
because nothing is currently paying for it. Splitting a generic `Tenant` from a
regulatory `Organization` is a change that needs a reason; see ADR-015's
*Revisit When*.

**"Platform" therefore means identity, authentication, authorization and
audit** — not "everything foundational."

---

## 3. Boundaries

### Permanent — never belongs in the Foundation

These are the Regulatory domain's, and that does not change:

- Products, product documents
- Regulatory applications, submissions
- Country and authority rules, dossier templates
- Any FDA / EMA / MDR-specific logic

The one acknowledged exception is `OrganizationType`, documented above rather
than hidden.

### Deferred — not now, possibly later

Not boundary violations; simply unbuilt. This list is expected to change:

- Teams, departments, organization hierarchies
- Notifications, webhooks
- Billing, licensing, marketplace
- API keys, feature flags, branding
- SSO and external identity providers
- Self-service signup, organization provisioning UI

Adding one of these is a roadmap decision. Adding something from the permanent
list is an architecture violation.

---

## 4. The Foundation's central promise

> **No organization may read or modify another organization's data.**

This matters more than any other Foundation capability. Authentication, users
and roles are all in service of it — a system that authenticates perfectly and
leaks across tenants has failed at its primary job.

### Where this stands today — read carefully

**Accepted, partially implemented.** State it honestly:

| | Status |
|---|---|
| Tenant identity is ambient, never a command parameter | **Current** — [ADR-013](../adr/ADR-013-ambient-tenant-context.md) |
| Platform (user) reads and writes are tenant-scoped, unconditionally | **Current** |
| Product, ProductDocument, RegulatoryApplication, Submission are tenant-scoped | **Not implemented** |
| The caller is proven entitled to the tenant they claim | **Not implemented** |

Two gaps deserve to be stated plainly rather than buried:

1. **The regulatory domain is not tenant-isolated.** Searching those source
   trees for `OrganizationId` or `TenantId` returns nothing.
   `ListRegulatoryApplicationsHandler` returns every regulatory application in
   the database regardless of who is asking. This is not a bug in an isolation
   mechanism — those modules have never had one.

2. ~~**`X-Tenant-Id` is not authentication.**~~ **Closed 2026-07-21 (AUTH-005).**
   The tenant is now the authenticated caller's organization claim, and
   `HeaderTenantContext` is deleted rather than disabled. A request carrying
   `X-Tenant-Id` for another organization returns that organization's data no
   longer — the header is inert.
   [ADR-024](../adr/ADR-024-tenancy-is-derived-from-identity.md).

RegOS v1 is not complete until the first is closed too. See
[`roadmap.md`](roadmap.md) and
[`tenant-inventory.md`](../architecture/tenant-inventory.md).

---

## 5. Dependency direction

**Accepted.** One way, always:

```
Regulatory Domain  ──depends on──▶  Foundation
```

The Foundation must never reference a Regulatory module. This is the constraint
that keeps the Foundation reusable and the Regulatory domain replaceable.

Within a module, dependencies point inward toward the Domain:

```
Presentation ──┐
               ├──▶ Application ──▶ Domain
Infrastructure ┘
```

Infrastructure implements interfaces defined by Domain and Application. Domain
depends on nothing.

---

## 6. What "v1 complete" means

The Foundation is complete for v1 when:

1. A business module can obtain the current user, the current organization, and
   an authorization decision without writing its own mechanism for any of them.
2. Every tenant-scoped read and write is constrained to the caller's
   organization, and the caller's entitlement to that organization is proven
   rather than asserted.
3. A new business module can be built without modifying Foundation code.
4. The actions listed in Epic 5 are recorded without the acting module having to
   remember to record them.

Criterion 2 is the one that is measurably false today. Criterion 3 is the one
that tells us the boundaries were drawn correctly.

---

## 7. Epics

Detail and current state in [`roadmap.md`](roadmap.md).

| # | Epic | Owns | Status |
|---|---|---|---|
| 1 | Organization | Tenant boundary, regulatory party identity | Partially built |
| 2 | User Management | User lifecycle, invitation, profile | Substantially built |
| 3 | Authentication | Credentials, tokens, password reset | Not started |
| 4 | Authorization | Roles, policy enforcement | Not started |
| 5 | Audit | Immutable record of significant actions | Not started |

Epics 1 and 2 are **not** greenfield. Any plan that treats them as such is
planning work that already exists.
