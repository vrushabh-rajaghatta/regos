# ADR-021 — An Email Address Identifies Exactly One User

**Status:** Accepted · **Date:** 2026-07-21 · **Supersedes:** nothing ·
**Related:** [ADR-013](ADR-013-ambient-tenant-context.md) (ambient tenant),
[ADR-015](ADR-015-organization-is-the-tenant.md) (organization is the tenant),
[ADR-014](ADR-014-invitation-is-a-user-status.md) (invitation is a user status)

## Context

Milestone 2 begins with authentication, and the first question a login endpoint
asks is *which user is this?*

Until now the answer was ambiguous. `UserConfiguration` declared

```csharp
builder.HasIndex(x => new { x.OrganizationId, x.Email }).IsUnique();
```

so the same address could exist in two organizations with two different
passwords. `POST /auth/login { email, password }` could not resolve a single
user from that.

The per-organization scope was never a decision. It followed from `User` always
being reached through an ambient tenant (ADR-013): every existing query already
had an organization in hand, so uniqueness was naturally written that way.
Authentication is the first caller with **no tenant yet** — the token is what
establishes it — and that exposed the gap.

Three options were considered:

1. **Global uniqueness.** Login is email + password. Requires a migration and
   gives up the ability for one address to be invited by two organizations.
2. **Organization-qualified login** — an organization field or a tenant-scoped
   URL. No migration, but the user must know which organization they belong to,
   and `Organization` has no `Code` to qualify a URL with.
3. **Disambiguate after email** — return the candidate organizations and let the
   user pick. No migration, no capability lost, but the most complex flow, and
   it enumerates which organizations an address belongs to for anyone who can
   guess an email.

## Decision

**An email address identifies exactly one user across RegOS.** The unique index
is on `Email` alone.

A person who works with two customer organizations needs two addresses. That is
the cost, and it is accepted: RegOS has no membership concept — a `User` belongs
to exactly one organization (ADR-015) — so one-account-per-person and
one-organization-per-account already coincide.

Consequently `IUserPolicy.EnsureEmailIsUniqueAsync` and
`EnsureEmailIsUniqueForUpdateAsync` no longer take an `OrganizationId`. The
parameter is removed rather than ignored, so the narrower rule cannot be
reintroduced by accident.

## Consequences

**Positive**

- Login resolves a user from email alone, with no organization field, no
  tenant-scoped URL and no two-step sign-in.
- One person, one account, one password. Password reset addresses a person
  rather than a person-within-an-organization.
- The uniqueness rule no longer depends on ambient tenant state, so it is
  checkable before a tenant exists — which is exactly what authentication needs.

**Negative**

- A consultant working with two client organizations needs two addresses.
- Reversing this is harder than adopting it: once two people share an address
  across organizations it cannot be reintroduced without a data migration and a
  re-established login contract.
- **A documented Milestone 3 exit criterion is invalidated.** It read *"the same
  email can be invited by a different organization independently."* That was
  derived from the index, not from a business rule. It has been removed rather
  than left to contradict this ADR.
- Existing databases may hold cross-organization duplicates and cannot take the
  index until they are resolved. The development database held one.

## Revisit When

- A membership concept arrives — one person legitimately belonging to several
  organizations — at which point identity separates from `User` and this ADR is
  superseded rather than amended.
- An external identity provider becomes the source of identity, since it will
  bring its own uniqueness semantics.
- A customer requires that their users be invisible to, and non-colliding with,
  every other tenant — the argument for going back to option 2 or 3.
