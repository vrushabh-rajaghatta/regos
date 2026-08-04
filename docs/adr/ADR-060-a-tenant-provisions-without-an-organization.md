# ADR-060 — A Tenant Provisions Without an Organization

**Status:** Accepted · **Date:** 2026-08-04 ·
**Amends:** [ADR-032](ADR-032-organizations-are-tenant-owned.md) (the mirror
entry only) ·
**Related:** [ADR-030](ADR-030-tenant-is-its-own-aggregate.md) (tenant split),
[ADR-033](ADR-033-three-roles-and-where-authority-lives.md) (who provisions)

## Context

ADR-032 made each tenant the owner of its organization registry, and closed the
gap that created — a new tenant with an empty registry cannot name an applicant
— by having `CreateTenant` also create a **mirror organization**: the tenant's
own company, carrying the tenant's name and *the tenant's guid*, by stated
convention.

Two problems with that, both visible now that the slice has run:

1. **It fabricates a regulatory fact at account-creation time.** The person
   filling in the provisioning form is a RegOS platform administrator
   (ADR-033), not the customer's regulatory affairs lead. ADR-032 made
   `OrganizationType` an explicit input precisely because it must not be
   guessed — but asking the wrong person is not better than guessing. The legal
   name has the same defect, and ADR-032 conceded it in the same breath: the
   rename handler deliberately does *not* propagate, because "the tenant's name
   is an account label, the organization's legal name is a regulatory fact." A
   row that is born from an account label and then diverges silently was never
   a regulatory fact to begin with.

2. **The shared guid is a cross-context identity coupling bought for a UI
   default.** Two aggregates in different bounded contexts share a key with no
   link column and no foreign key — legible only as prose. ADR-032 named its
   own breaking point ("an operation that changes ids"), and nothing yet
   depends on the convention except the handler that creates it. It is cheapest
   to retire before something does.

Provisioning an account and authoring a regulatory registry are different jobs,
done at different times, by different people. The mirror entry conflated them.

## Decision

**A tenant is provisioned with an empty organization registry.** `CreateTenant`
creates the tenant and its invited administrator, in one unit of work, and
nothing else.

- **`OrganizationType` leaves the provisioning contract** — the command, the
  API request, and the platform-admin form. With it goes Platform.Application's
  project reference to `Organization.Domain`: provisioning no longer names a
  regulatory party, so the dependency the csproj called "genuine" is gone.
- **The tenant administrator records their own company**, after accepting the
  invitation, through the organization registry UI that already exists — as the
  first entry among the applicants, manufacturers and partners they will record
  anyway. It is an ordinary registry entry, distinguished by nothing.
- **The shared-guid convention is retired.** An organization whose id equals a
  tenant's id means nothing. No code may derive one identity from the other,
  in either direction.
- **Existing rows stay.** Organizations created as mirrors are real registry
  entries their tenants own and applications already cite as applicant; ES-018
  keeps them. The seeded demo trio keeps its ids for the same reason. Their
  guid coincidence is a fixture artifact from now on, not a convention, and
  nothing may read meaning back into it.

**What ADR-032 keeps.** Everything else: registries are tenant-owned,
`Organization.TenantId` is required and fail-closed filtered, and the applicant
on a regulatory application must exist in the caller's own registry. This
amendment removes the shortcut ADR-032 offered around that last rule, not the
rule.

## Consequences

**Positive**

- No organization exists in RegOS that no regulatory user ever asserted.
- The provisioning form asks only what a platform operator actually knows: the
  account's name and who to invite.
- One less cross-context coupling, and one less project reference, in the
  context that must stay clean of regulatory concerns.

**Negative — accepted knowingly**

- **A new tenant cannot create a regulatory application until an administrator
  adds an organization.** First-run has one more step and the applicant
  dropdown starts empty. This is the honest state: RegOS does not know who the
  customer is until it is told.
- **No free "your organization" default.** Any feature that needs one now needs
  it stated — an explicit `Tenant.OrganizationId` link, or an "own company"
  marker on `Organization`. That is a new decision, taken when a feature
  demands it, not inherited from a guid.

## Revisit When

- A feature genuinely needs to answer *"which organization is this tenant?"* —
  add the explicit link then. ADR-032 pointed at the same door from the other
  side.
- **Self-serve signup arrives.** When the customer creates their own account,
  the person filling the form *is* the one who knows the legal entity, and
  capturing an organization during provisioning becomes correct. That would be
  a decision taken under different information, not a reversal of this one.
