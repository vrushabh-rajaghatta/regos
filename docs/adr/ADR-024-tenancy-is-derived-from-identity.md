# ADR-024 — Tenancy Is Derived From Identity, Not Asserted By The Caller

**Status:** Accepted · **Date:** 2026-07-21 ·
**Supersedes:** the development mechanism in
[ADR-013](ADR-013-ambient-tenant-context.md) (the abstraction stands) ·
**Related:** [ADR-015](ADR-015-organization-is-the-tenant.md) (organization is
the tenant), [ADR-021](ADR-021-email-is-globally-unique.md) (email identifies a
user), [ADR-022](ADR-022-authentication-failure-is-a-fourth-exception.md) (401)

> **The transport described in Decision points 2 and 3 is superseded.** The
> browser no longer sends a bearer header or holds a token in `localStorage`;
> the session is `HttpOnly` cookies
> ([ADR-025](ADR-025-sessions-are-server-owned-cookies.md)). The decision this
> ADR records — that tenancy comes from the authenticated caller's claim and
> never from a request header — is unchanged.

## Context

ADR-013 introduced `ITenantContext` and implemented it with
`HeaderTenantContext`, which read `X-Tenant-Id`. That ADR was explicit that this
was a development mechanism and not authentication: *any caller could set the
header to any value*, so it established which tenant a request was scoped to,
never that the caller was entitled to it.

By the end of AUTH-004 the platform issued tokens, validated them, and exposed
`ICurrentUser` — and still scoped every query by the header. Two identity
systems were running side by side: one the caller proved, one the caller
asserted, and only the second one decided which data came back.

That is worse than having neither. A reviewer looking at the codebase would see
authentication and reasonably assume tenant isolation followed from it.

## Decision

**The tenant is the authenticated caller's organization.** `ITenantContext` is
implemented by `ClaimsTenantContext`:

```csharp
public Guid TenantId => _currentUser.OrganizationId.Value;
```

`HeaderTenantContext` and `TenantErrors` are deleted rather than retained
behind a flag. There is no configuration under which RegOS reads a tenant from
a request header, because a switch that restores caller-asserted tenancy is a
switch that will eventually be found on in an environment that matters.

Three supporting decisions come with it:

1. **Authenticated by default.** The authorization fallback policy requires an
   authenticated user for every endpoint. With tenancy derived from a claim, an
   endpoint that forgot `RequireAuthorization` would not merely be public — it
   would reach `ITenantContext` with no identity behind it. Exactly one endpoint
   opts out with `AllowAnonymous`, and it is the one that issues tokens.

2. **The browser sends the token from one place.** `apiFetch` attaches the
   bearer header for every call, so no feature module decides whether a request
   is authenticated and none of them can forget. It has no opt-out parameter;
   sign-in uses plain `fetch` because it is the one request with no token yet.

3. **A 401 discards the token.** `apiFetch` clears local storage on a 401, so a
   rejected credential is not retried on every subsequent request.

## Consequences

**Positive**

- Tenant isolation is now a property of the system rather than of the client's
  good manners. The header is inert: verified by signing in as Demo
  Manufacturer (4 products), sending `X-Tenant-Id` for Demo MAH (33 products),
  and receiving 4.
- **None of the fourteen `ITenantContext` consumers changed.** The abstraction
  was introduced for exactly this substitution, and it paid.
- Requesting tenant-scoped data without proving identity is now 401 rather than
  400. The failure moved from "your request is malformed" to "I do not know who
  you are", which is the more accurate statement.
- A browser spec asserts that no request carries `X-Tenant-Id`, so the removal
  is enforced rather than remembered.

**Negative**

- **Every endpoint now requires a token, including reference data.** Countries
  and authorities are not tenant-scoped and arguably need not be protected.
  They are, because a default-open list is a list someone will add a
  tenant-scoped endpoint to.
- ~~**The access token is stored in `localStorage`**~~ — **closed 2026-07-21
  (AUTH-006).** Superseded by
  [ADR-025](ADR-025-sessions-are-server-owned-cookies.md): the session is now
  two `HttpOnly` cookies no script can read.
- ~~**Signing out is client-side only.**~~ **Partly closed 2026-07-21
  (AUTH-006).** Sign-out now revokes the refresh token server-side (ADR-025).
  The access token still cannot be revoked and remains valid until it expires,
  which is why it lasts fifteen minutes.
- **The development account moved to Demo MAH Ltd.** It previously belonged to
  Demo Manufacturer, but the UI had always acted as Demo MAH through the
  header. Now that the account's organization *is* the tenant, the two had to
  agree or development would open onto a near-empty system.
- Every browser spec authenticates. `tests/Browser/specs/support.ts` signs in
  once per run and injects the token before page scripts run.

## Revisit When

- Refresh tokens arrive (AUTH-006) — the natural moment to move the token out
  of `localStorage` and into an httpOnly cookie, and to give sign-out a server
  side.
- A user may belong to more than one organization, which would make "the
  tenant" a choice again rather than a fact. It would need an explicit,
  *authorized* tenant-switch — a claim the server issues after checking
  membership, never a header the client sets.
- A service-to-service caller needs tenant-scoped access without a user. That
  caller has no `OrganizationId` claim today, and inventing a header for it
  would reintroduce precisely what this ADR removes.
