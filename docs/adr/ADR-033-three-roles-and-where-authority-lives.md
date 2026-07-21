# ADR-033 — Three Roles, and Where Authority Lives

**Status:** Accepted · **Date:** 2026-07-21 ·
**Related:** [ADR-022](ADR-022-authentication-failure-is-a-fourth-exception.md)
(403 left unclaimed), [ADR-028](ADR-028-credential-replacement-revokes-derived-trust.md)
(revocation machinery), [ADR-030](ADR-030-tenant-is-its-own-aggregate.md)
(platform users), [ADR-032](ADR-032-organizations-are-tenant-owned.md)
(tenant-owned registries)

## Context

Until now authorization was binary — authenticated or not — with 403
explicitly unclaimed (ADR-022) and roles explicitly deferred
(`ICurrentUser`'s doc, the token issuer's refusal of role claims). The tenant
administration epic ends the deferral: it needs a platform operator who
creates tenants, a tenant administrator who manages users, and members who do
neither. Three distinguishable kinds of caller is a role model, however small.

## Decision

**1. Three roles, an enum, not a permission system.**
`UserRole { PlatformAdministrator, TenantAdministrator, Member }` on the User
aggregate. The pressure on authorization models is always to grow matrices
before any feature needs them; this ADR deliberately resists it. Permissions,
custom roles and per-resource grants arrive when a feature demands them.

**2. Role and tenant agree by construction.** Only `CreatePlatformUser`
produces `PlatformAdministrator`, and it never has a tenant;
`CreateForTenant` rejects the platform role. A tenant-bound platform
administrator is unexpressible — the same factory technique that guards the
nullable tenant (ADR-030).

**3. The role travels as a claim** (`regos:role`, the enum member's name).
This reverses the issuer's written stance that a token carries identity and
never an authorization snapshot. Reversed knowingly:

- Staleness is capped at the fifteen-minute access-token lifetime, and a
  demotion that cannot wait ends the user's sessions through the ADR-028
  machinery.
- The alternative — a database read per authorization check — buys instant
  demotion at the price of a query on every guarded request and a caching
  question nobody has asked yet.
- Name and status stay out of the token: those really are snapshots nothing
  downstream checks.

**4. Gating happens in policies, never in handlers.** Two exact-match
policies (`RegOSPolicies.PlatformAdministrator`, `.TenantAdministrator`);
a failed policy is the framework's 403, finally claiming the status ADR-022
left open. The policies are deliberately non-hierarchical: a platform
administrator does not satisfy the tenant-administrator policy — they have no
tenant, so every tenant-scoped endpoint would throw at `ITenantContext`
anyway; the policy says so with a 403 instead of a confusing 401.

**5. User administration belongs to the tenant administrator.** The seven
`/api/platform/users*` endpoints require the role. A member keeps their own
surface — sign-in, `/me`, sessions, password — and loses nothing they were
meant to have.

**6. Cross-tenant reads are a platform-administrator grant.** The tenant
administration slice adds the platform views (all tenants, a tenant's users);
each such query handler pairs `RequireAuthorization(PlatformAdministrator)`
with `IgnoreQueryFilters()` and joins ADR-031's named bypass list. The pairing
is the rule: an unfiltered query without the policy — or a handler checking
roles inline instead of a policy — should fail review.

## Consequences

- 403 exists and means "we know who you are; this is not yours" — distinct
  from 401's "who are you" and 404's "nothing here", each carrying exactly
  the information the caller is entitled to.
- Role changes for existing users have **no endpoint yet** — the only role
  assignments happen at creation (seeding today, tenant provisioning next).
  Promotion/demotion is its own slice, and will decide who may change whose
  role.
- A demoted user keeps acting on their old role for up to fifteen minutes
  unless their sessions are ended. Accepted, recorded, bounded.

## Revisit When

- A feature needs permissions finer than these three roles.
- Role management (promote/demote) is built — it will test rule 2's edges
  (can a tenant administrator be demoted while being the only one?).
- Delegated administration (a CRO administering a sponsor's tenant) arrives —
  that is a relationship, not a role, and must not be modelled by widening
  this enum.
