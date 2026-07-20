# RegOS Foundation — Roadmap

This roadmap answers **what remains to be built**, not what a foundation would
contain in theory. Every "current state" entry below was verified against the
codebase on 2026-07-20.

Definitions of done are written as **behaviors**. "CQRS implemented" and "tests
passing" are not outcomes — they describe activity, not a system that does
something it previously could not.

---

## Cross-cutting: what already exists

Foundation abstractions are extracted when a consumer needs them (principle P4),
so this is not a phase. It is an inventory.

| Capability | Status |
|---|---|
| `AggregateRoot<TId>`, `Entity`, `ValueObject`, `StronglyTypedId` | ✅ `RegOS.SharedKernel` |
| `DomainException`, `NotFoundException`, `BusinessRuleViolationException` | ✅ ADR-012 |
| Exception → ProblemDetails middleware | ✅ `ExceptionHandlingMiddleware`, registered at `Program.cs:92` |
| `ITenantContext` (ambient tenant) | ✅ ADR-013 |
| Single `RegOSDbContext`, shared migrations | ✅ |
| Repositories for writes, DbContext for reads | ✅ ADR-016 |
| `ICurrentUser` | ❌ Blocked on Epic 3 |
| `IClock` | ❌ Not built — aggregates take `DateTime` or default it internally |
| Domain events | ❌ Not built |
| Structured logging, correlation id | ❌ Not built |
| Validation pipeline (FluentValidation) | ❌ Not built, and explicitly out of scope per ADR-009 |

**Stale backlog entries.** `ARCHITECTURE_BACKLOG.md` lists AB-001 (consolidate
DbContexts) and AB-002 (global exception handling) as *Planned*. Both are done.
The backlog needs reconciling the same way the ADRs did.

**Do not build the missing items speculatively.** `ICurrentUser` cannot be
designed honestly before authentication exists. `IClock` earns its place the
first time a test needs to control time.

---

## Epic 1 — Organization

**Goal:** a tenant boundary that can be managed, and regulatory contexts that
actually respect it.

### Current state

✅ `Organization` aggregate — `LegalName`, `Type`, `Status`; `Create`,
`Activate`, `Deactivate`
✅ `OrganizationId`, `OrganizationStatus` (`Active | Inactive`),
`OrganizationType` (4 regulatory types), `OrganizationErrors`
✅ `ListOrganizations` query and endpoint

❌ `Activate` / `Deactivate` exist on the aggregate but **no command, handler or
endpoint reaches them** — the behavior is unreachable from outside
❌ No `CreateOrganization` or `UpdateOrganization` command
❌ No `GetOrganization` query
❌ No `IOrganizationRepository`; `RegOS.Organization.Infrastructure`
registers nothing
❌ No `Code` field — Epic 1's UI sketch shows one, the aggregate has none
❌ No settings (time zone, culture)
❌ No tests — `tests/` contains no Organization project

### Remaining work

1. `IOrganizationRepository` + implementation + DI registration.
2. `CreateOrganization`, `UpdateOrganization`, `ActivateOrganization`,
   `DeactivateOrganization` commands and endpoints.
3. `GetOrganization` query.
4. `Code` as a value object, unique per system, if the UI sketch is still wanted.
   Decide this explicitly — it is currently a UI mockup with no domain behind it.
5. `OrganizationSettings` as a **value object** on the aggregate (P2), not a
   second aggregate.
6. Development seed organization.
7. **Close invariant I4 for one regulatory context**, as proof the mechanism
   works. `Product` first, per `tenant-inventory.md`.
8. Organization domain and application test projects.

### Definition of done

- An organization cannot be created without a legal name.
- Two organizations cannot share a code (if item 4 is adopted).
- A deactivated organization rejects user invitations.
- Reactivating an organization restores invitation.
- Activating an already-active organization is rejected as a state conflict
  (409), not silently accepted — the current `Activate()` is unconditional.
- Settings can be changed without changing organization identity.
- `Product` reads and writes return only the calling organization's rows, and no
  code path can widen that.

The last one is the epic. The rest is plumbing around it.

### Open decisions

- **ADR-017 (proposed):** does `Organization` need a `Code` distinct from its
  id, and is it unique per system or per anything else?
- **What owns a `Product`?** I4 cannot be closed for Product without answering
  it. A modelling decision, not plumbing.

---

## Epic 2 — User Management

**Goal:** manage users within an organization.

### Current state

Substantially built. The original roadmap listed six commands and queries; all
six exist.

✅ `InviteUser`, `ActivateUser`, `DeactivateUser`, `UpdateUserProfile`
✅ `GetUserById`, `GetUsers` (paged, tenant-filtered, clamped page size)
✅ `User` aggregate, `UserId`, `UserStatus` (`Active | Inactive | Invited`),
`Email` value object
✅ `IUserRepository` + implementation, `IUserPolicy` + implementation
✅ All six endpoints
✅ Domain and application test projects

❌ No roles on a user — Epic 4
❌ No invitation lifecycle: no expiry, revocation, resend, or record of who
invited whom ([ADR-014](../adr/ADR-014-invitation-is-a-user-status.md))
❌ No way for an invited user to *accept* — activation is an administrative
action, not something the invitee does

### Remaining work

1. **Resolve the invitation model.** ADR-014 records the current design: invitation
   is a `User` status, with no token and no expiry. Epic 3 needs an invitation
   acceptance flow, and an invitation token is the same shape as a
   password-reset token. Decide whether to extend `User` or introduce an
   `Invitation` aggregate **before** Epic 3 builds tokens, not during.
2. Profile completeness — what a user must supply on first sign-in.
3. UI for the user directory and user detail.

### Definition of done

- An invited user appears in the directory in `Invited` status.
- A second invitation to the same email in the same organization is rejected as
  a conflict.
- The same email can be invited by a different organization independently.
- An invited user cannot act until activated.
- A deactivated user's data is retained.
- No user query returns a user from another organization, whatever parameters
  are supplied.

---

## Epic 3 — Authentication

**Goal:** prove that a caller is who they claim, and replace `X-Tenant-Id`.

### Current state

Not started. No credential, token, or password-reset type exists.

`HeaderTenantContext` is the placeholder this epic removes.

### Remaining work

1. `UserCredential` — password hashing.
2. Login, logout.
3. Refresh token with rotation.
4. Forgot password / reset password.
5. Invitation acceptance — first-time password creation. Depends on Epic 2's
   invitation decision.
6. **Replace `HeaderTenantContext` with a claims-based `ITenantContext`.**
   Nothing above it changes, by design (ADR-013).
7. `ICurrentUser`, which can now be designed honestly.

### Definition of done

- A user with valid credentials receives an access token and a refresh token.
- An invalid credential is rejected without revealing whether the email exists.
- An expired access token can be exchanged using a valid refresh token.
- A used refresh token cannot be reused.
- A password reset invalidates every existing refresh token for that user.
- **A request cannot be scoped to an organization the authenticated user does
  not belong to**, regardless of headers.
- An inactive or invited user cannot obtain a token.

The second-to-last is the point of the epic. Everything else is machinery.

---

## Epic 4 — Authorization

**Goal:** decide what an authenticated user may do.

Depends on Epic 3.

### Current state

Not started. No role type exists. `IUserPolicy` handles domain policy (can this
organization accept users, is this email unique) — it is not authorization and
should not be extended into it.

### Remaining work

1. `Role`, `UserRole`.
2. Seed roles: Organization Admin, Regulatory Manager, Contributor, Viewer.
3. Assign role, remove role.
4. Policies — `CanManageUsers`, `CanManageProducts`, `CanViewAudit` — mapped to
   roles internally. **No `Permission` table** (see open decisions).
5. Enforcement in the Application layer per invariant I6.

### Definition of done

- A user without `CanManageUsers` cannot invite a user, and receives a
  authorization failure distinct from "not found" and from "invalid request".
- Role assignment takes effect on the next request without re-login, or the
  token lifetime is documented as the propagation delay.
- A role cannot be assigned across organizations.
- The last Organization Admin in an organization cannot be demoted or
  deactivated.
- Every command enforces its policy when invoked directly, not only through its
  endpoint.

### Open decisions

- **ADR-018 (proposed):** roles map to policies internally, with no `Permission`
  entity. Correct for v1; the table earns its way in when a customer needs
  custom roles. Worth an ADR precisely so the answer to "why is there no
  permission table?" is written down rather than rediscovered.

---

## Epic 5 — Audit

**Goal:** an immutable record of significant actions.

Last, because it needs the other four to have something to record.

### Current state

Not started.

### Remaining work

1. `AuditLog` — append-only.
2. Recording mechanism. The DoD requires this not be a per-handler
   responsibility; domain events are the likely vehicle, which makes this the
   first genuine consumer of the domain-events abstraction (P4).
3. Audit history query.

### Recorded actions

Login, user invited, user activated, user deactivated, organization updated,
organization activated/deactivated, role assigned, role removed.

### Definition of done

- Every action in the list above produces an audit entry without the handler
  explicitly writing one.
- An audit entry records actor, organization, action, subject and time.
- Audit entries cannot be modified or deleted through any application path.
- Audit history is tenant-scoped like everything else.
- An action performed by a background job or a system process is attributable to
  something, not to `null`.

---

## Sequencing

Dependency order, not epic order:

```
Epic 1 (Organization)  ──▶  Epic 2 (Users)  ──▶  Epic 3 (Auth)  ──▶  Epic 4 (Authz)  ──▶  Epic 5 (Audit)
```

Two things worth stating:

- **Epics 1 and 2 overlap.** Epic 2 is largely built on top of an Epic 1 that is
  half-built. Epic 1's real remaining work — closing invariant I4 — is the
  blocker for everything, including whether the regulatory domain is safe to
  expose to a second customer.
- **The invitation decision must be made before Epic 3 starts**, not during.

---

## Deferred

Not boundary violations — simply unbuilt. See
[`vision.md`](vision.md#3-boundaries).

**Next** — platform administration, organization provisioning UI, self-service
signup.
**Later** — notifications, feature flags, teams, API keys, webhooks.
**Much later** — billing, licensing, marketplace, SSO, external identity
providers.
