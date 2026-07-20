# RegOS Foundation — Roadmap

The architecture was planned module-first. Implementation is planned
**vertical-slice-first**: every milestone ends with something that runs in a
browser, is tested, and can be verified by using it.

This roadmap answers **what remains to be built**. Every "current state" claim
below was verified against the codebase on 2026-07-20 — backend *and* frontend.
Definitions of done are written as **behaviors**; "CQRS implemented" and "tests
passing" describe activity, not a system that does something it could not do
before.

---

## Cadence

One milestone per sprint.

| Sprint | Milestone | Outcome |
|---|---|---|
| 1 | Organization | Complete organization lifecycle |
| 2 | Authentication | Secure login and protected APIs |
| 3 | User Management | Full user administration |
| 4 | Authorization | Role-based access control |
| 5 | Audit | Traceability and history |
| 6 | Hardening | Production readiness |

### Execution checklist — every milestone

1. **Review current implementation.** Assume something already exists; it
   usually does.
2. **Identify the gap.** Define only the missing work.
3. **Implement backend** — domain, application, infrastructure, API.
4. **Implement frontend** — pages, forms, navigation.
5. **Write tests** — unit and integration where appropriate.
6. **Browser verification** — the end-to-end flow actually works.
7. **Refactor** while the context is fresh.
8. **Update docs only if an ADR changes.** Otherwise keep moving.

### Explicitly deferred

- New ADRs, unless implementation forces a decision.
- Refactors unrelated to the current milestone.
- Foundation capabilities outside this roadmap.
- Speculative abstractions. `IClock` earns its place the first time a test needs
  to control time; not before.

---

## Cross-cutting: what already exists

Not a phase — an inventory. Foundation abstractions are extracted when a
consumer needs them (principle P4).

| Capability | Status |
|---|---|
| `AggregateRoot<TId>`, `Entity`, `ValueObject`, `StronglyTypedId` | ✅ `RegOS.SharedKernel` |
| Three shared exception types | ✅ [ADR-012](../adr/ADR-012-shared-semantic-exception-model.md) |
| Exception → ProblemDetails middleware | ✅ `Program.cs:92`; 0 of 35 endpoints carry a try/catch |
| `ITenantContext` (ambient tenant) | ✅ [ADR-013](../adr/ADR-013-ambient-tenant-context.md) |
| Single `RegOSDbContext`, shared migrations | ✅ |
| Repositories for writes, DbContext for reads | ✅ [ADR-016](../adr/ADR-016-persistence-access-model.md) |
| `ICurrentUser` | ❌ Milestone 2 — cannot be designed honestly before then |
| `IClock` | ❌ Not built |
| Domain events | ❌ Not built — Milestone 5 is the first real consumer |
| Structured logging, correlation id | ❌ Milestone 6 |

---

## Milestone 1 — Organization

**Goal:** the organization lifecycle works from browser to database.

### Current state

✅ `Organization` aggregate — `LegalName`, `Type`, `Status`; `Create`,
`Activate`, `Deactivate`
✅ `OrganizationId`, `OrganizationStatus`, `OrganizationType`,
`OrganizationErrors`
✅ `ListOrganizations` query + endpoint (consumed today only by the invite-user
organization dropdown)

❌ `Activate` / `Deactivate` exist on the aggregate but **no command, handler or
endpoint reaches them** — working behavior, unreachable from outside
❌ No `CreateOrganization`, `UpdateOrganization`, `GetOrganization`
❌ No `IOrganizationRepository`; `RegOS.Organization.Infrastructure` registers
nothing at all
❌ **No UI whatsoever** — `web/regos-web/src/features/platform/` contains
`layout/` and `users/`, and no `organizations/`
❌ No `Code` field — the UI sketch shows Name / Code / Status; the aggregate has
`LegalName` / `Type` / `Status`
❌ No settings (time zone, culture)
❌ No tests — `tests/` has no Organization project

This is the least-built module in the Foundation, which is why it is first.

### Scope

Backend: repository + DI registration; `CreateOrganization`,
`UpdateOrganization`, `ActivateOrganization`, `DeactivateOrganization`,
`GetOrganization`; endpoints.

Frontend: organizations list, organization details, create and edit forms,
activate/deactivate actions, navigation entry — mirroring the existing
`features/platform/users/` structure, which is the proven pattern.

Tests: Organization domain and application test projects.

### Exit criteria — behaviors

- An organization cannot be created without a legal name.
- A deactivated organization rejects user invitations.
- Reactivating a deactivated organization restores invitation.
- Activating an already-active organization is rejected as a **409 state
  conflict**, not silently accepted. `Activate()` is currently unconditional, so
  this is a real change, and [ADR-009](../adr/ADR-009-command-validation-model.md)
  is what decides the status code.
- Settings can be changed without changing organization identity.
- The full lifecycle is verified in a browser: create → edit → deactivate →
  reactivate.
- All tests pass.

### Decisions this milestone will force

- **Does `Organization` need a `Code` separate from its id?** The UI sketch says
  yes; the domain has never had one. If adopted, decide its uniqueness scope.
  Write it as ADR-017 when the command is written — not before.

---

## Milestone 2 — Authentication

**Goal:** every subsequent milestone runs as a real authenticated user.

Moved ahead of User Management deliberately, and the reason holds: the `User`
aggregate already exists, so authentication has something to attach credentials
to. Building user administration first would mean building it twice — once
against `X-Tenant-Id`, once against a real identity.

### Current state

Not started. No credential, token or password-reset type exists.

`HeaderTenantContext` reads `X-Tenant-Id` and is the placeholder this milestone
removes. It decides *which* tenant a request is scoped to and never establishes
that the caller is entitled to it — any caller can send any value.

### Scope

- `UserCredential`, password hashing.
- Login, logout.
- JWT access token; refresh token with rotation.
- Forgot password / reset password.
- **Replace `HeaderTenantContext` with a claims-based `ITenantContext`.** Nothing
  above it changes, by design (ADR-013).
- `ICurrentUser`.
- Authentication middleware; React login flow and protected routes.

### Exit criteria — behaviors

- A user with valid credentials receives an access token and a refresh token.
- An invalid credential is rejected without revealing whether the email exists.
- An expired access token can be exchanged using a valid refresh token.
- A used refresh token cannot be reused.
- A password reset invalidates every existing refresh token for that user.
- An inactive or invited user cannot obtain a token.
- **A request cannot be scoped to an organization the authenticated user does not
  belong to, regardless of headers.**
- The React login flow works end to end and protected routes redirect when
  unauthenticated.

The second-to-last criterion is the point of the milestone. Everything else is
machinery serving it.

### Decisions this milestone will force

**How does an invited user obtain a credential?** This is unavoidable here, and
moving Authentication earlier moved the decision earlier with it.

[ADR-014](../adr/ADR-014-invitation-is-a-user-status.md) records the current
model: invitation is a `User` status with **no token, no expiry, no acceptance
step**. Activation is an administrative action, not something the invitee
performs. So an invited user has no path to a password.

Two ways through, and the milestone must pick one explicitly:

- **Defer.** Seed a development user with a credential; invited users cannot yet
  log in. Milestone 3 then owns acceptance. Cheapest, and keeps this milestone
  focused.
- **Resolve now.** Build invitation acceptance — which means an invitation token,
  which is the same shape as the password-reset token this milestone already
  builds. Supersede ADR-014.

The second is tempting because the machinery overlaps. Decide on evidence when
the reset-token work is in front of you, then write the ADR.

---

## Milestone 3 — User Management

**Goal:** organization administrators manage users as authenticated users.

### Current state — read this before planning the sprint

**This milestone is already built, end to end.** Every item in its original
scope exists in backend, frontend and tests:

| Capability | Backend | Frontend |
|---|---|---|
| Invite user | ✅ `InviteUserHandler` | ✅ `InviteUserDialog`, `InviteUserForm`, `inviteUserSchema` |
| Update profile | ✅ `UpdateUserProfileHandler` | ✅ `EditUserProfileDialog`, `EditUserProfileForm` |
| Activate user | ✅ `ActivateUserHandler` | ✅ `useActivateUser` |
| Deactivate user | ✅ `DeactivateUserHandler` | ✅ `DeactivateUserDialog` |
| User listing | ✅ `GetUsersHandler` (paged, tenant-filtered) | ✅ `UsersPage`, `UsersTable`, `UserStatusBadge` |
| User details | ✅ `GetUserByIdHandler` | ✅ `UserDetailsPage` |

✅ `IUserRepository`, `IUserPolicy` and implementations
✅ `tests/Platform/RegOS.Platform.Domain.Tests`,
`RegOS.Platform.Application.Tests`

**Plan this as a verification and adaptation pass, not a build sprint.** The
real work is what Milestone 2 changes underneath it.

### Scope

1. **Re-verify every flow under real authentication.** These screens were built
   against `X-Tenant-Id`; the tenant now arrives from a claim. Every user screen
   is a place that assumption could be baked in.
2. **Invitation acceptance**, if Milestone 2 deferred it.
3. **Self-service profile editing** — a user editing their *own* profile, which
   is a different authorization case from an admin editing someone else's, and
   is the first place `ICurrentUser` gets exercised in anger.
4. Gaps browser verification exposes.

### Exit criteria — behaviors

- An invited user appears in the directory in `Invited` status.
- A second invitation to the same email in the same organization is rejected as
  a conflict.
- The same email can be invited by a different organization independently.
- An invited user cannot act until activated.
- **No user query returns a user from another organization, whatever parameters
  are supplied**, now enforced by claims rather than by a header.
- A user can update their own profile; a user cannot update another user's
  profile without permission — recorded as a gap if Milestone 4 owns the check.
- Browser verified as an authenticated administrator.

---

## Milestone 4 — Authorization

**Goal:** what an authenticated user may do.

### Current state

Not started. No role type exists.

`IUserPolicy` handles *domain* policy — can this organization accept users, is
this email unique. That is not authorization and should not be extended into it.

### Scope

- `Role`, `UserRole`.
- Seed roles: Organization Admin, Regulatory Manager, Contributor, Viewer.
- Assign role, remove role.
- Policies — `CanManageUsers`, `CanManageProducts`, `CanViewAudit` — mapped to
  roles internally. **No `Permission` table.**
- Enforcement in the Application layer (invariant I6); UI authorization.

### Exit criteria — behaviors

- A user without `CanManageUsers` cannot invite a user, and receives an
  authorization failure distinct from "not found" and from "invalid request".
- A role cannot be assigned across organizations.
- The last Organization Admin in an organization cannot be demoted or
  deactivated.
- **Every command enforces its policy when invoked directly, not only through its
  endpoint.** A command must be safe from a background job or a test.
- The UI hides actions the user cannot perform, and the API still rejects them if
  called anyway.
- Role assignment takes effect on the next request without re-login, or the token
  lifetime is documented as the propagation delay.

### Decisions this milestone will force

- **ADR-018:** roles map to policies internally, no `Permission` entity. Correct
  for v1; the table earns its way in when a customer needs custom roles. Worth
  recording so the answer to "why is there no permission table?" is written down
  rather than rediscovered.

---

## Milestone 5 — Audit

**Goal:** significant actions are traceable.

Last among the feature milestones because it now has meaningful activity to
observe.

### Current state

Not started.

### Scope

- `AuditLog`, append-only.
- Recording mechanism. The exit criteria require this not be a per-handler
  responsibility, which makes this the first genuine consumer of domain events
  (P4).
- Audit history endpoint and UI.

### Recorded actions

Login, user invited, user activated, user deactivated, organization created,
organization updated, organization activated/deactivated, role assigned, role
removed.

### Exit criteria — behaviors

- Every action above produces an audit entry **without the handler explicitly
  writing one**.
- An entry records actor, organization, action, subject and time.
- Audit entries cannot be modified or deleted through any application path.
- Audit history is tenant-scoped like everything else.
- An action performed by a background job is attributable to something, not to
  `null`.
- Audit history is viewable in the browser by a user with `CanViewAudit`, and
  not by anyone else.

---

## Milestone 6 — Foundation Hardening

Quality, not functionality.

### Scope

- **Close invariant I4** — see below.
- Performance review.
- Structured logging and correlation id.
- Exception handling and validation review.
- Integration tests.
- Security review.

### The item that must not wait for a security review to discover

**Tenant isolation does not exist outside Platform.** Product, ProductDocument,
RegulatoryApplication and Submission have no tenant concept at all — searching
those source trees for `OrganizationId` or `TenantId` returns nothing.
`ListRegulatoryApplicationsHandler` returns every regulatory application in the
database regardless of who is asking.

This is invariant I4, and it is the Foundation's central promise
([vision.md §4](vision.md#4-the-foundations-central-promise)).

It appears in Milestone 6 only because the milestones above are Foundation
features and this is regulatory-domain work. **Do not let that ordering imply it
is low priority.** Two constraints:

- It cannot start before Milestone 2, because scoping to a tenant is meaningless
  while any caller can claim any tenant via a header.
- It cannot be done mechanically. Each context needs a domain decision first —
  beginning with **what owns a `Product`** — and `Submission` and
  `ProductDocument` should inherit scope from their parents rather than gaining
  their own organization column. See
  [`tenant-inventory.md`](../architecture/tenant-inventory.md).

If RegOS is to be exposed to a second customer before Milestone 6, this work
moves forward, not the customer.

### Exit criteria

- Every tenant-scoped read and write is constrained to the caller's organization,
  and no code path can widen it.
- A request carrying a valid token for organization A cannot read organization
  B's products, documents, applications or submissions.
- Integration tests cover the isolation boundary, not just the happy path.

---

## Deferred

Not boundary violations — simply unbuilt. See
[`vision.md §3`](vision.md#3-boundaries).

**Next** — platform administration, organization provisioning UI, self-service
signup.
**Later** — notifications, feature flags, teams, API keys, webhooks.
**Much later** — billing, licensing, marketplace, SSO, external identity
providers.
