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
| `ITenantContext` (from the caller's claim) | ✅ [ADR-013](../adr/ADR-013-ambient-tenant-context.md), [ADR-024](../adr/ADR-024-tenancy-is-derived-from-identity.md) |
| Single `RegOSDbContext`, shared migrations | ✅ |
| Repositories for writes, DbContext for reads | ✅ [ADR-016](../adr/ADR-016-persistence-access-model.md) |
| `ICurrentUser` | ✅ AUTH-004 — `UserId`, `OrganizationId`, `Email`, `IsAuthenticated`, and deliberately nothing else |
| `IClock` | ❌ Not built |
| Domain events | ❌ Not built — Milestone 5 is the first real consumer |
| Structured logging, correlation id | ❌ Milestone 6 |

---

## Milestone 1 — Organization

**Goal:** the organization lifecycle works from browser to database.

### Slices, in order

Reordered on 2026-07-20 by an implementation constraint, not a preference —
see *The retirement-path rule* below.

| # | Slice | Status |
|---|---|---|
| 1 | Create Organization | ✅ `28a3893` (ORG-001) |
| 2 | Deactivate Organization | ✅ `edf16e0` (ORG-002) |
| 3 | Get Organization | ✅ `3e1d0af` (ORG-003) |
| 4 | Update Organization | ✅ `eb4efd2` (ORG-004) |
| 5 | Activate Organization | ✅ (ORG-005) |

**Milestone 1 is complete.** The lifecycle closes:

```
Create ──▶ Active ──Deactivate──▶ Inactive ──Activate──▶ Active
```

Editing is available in both states and changes neither.

Deactivate moved from last to second because until it exists, nothing can
return the database to a known state: an organization cannot be deleted, so
every automated create test leaks a row. Activate is last because it is the
inverse of Deactivate and reads more cleanly once Get exists to inspect the
result.

### Current state

✅ `Organization` aggregate — `LegalName`, `Type`, `Status`; `Create`,
`Activate`, `Deactivate`
✅ `OrganizationId`, `OrganizationStatus`, `OrganizationType`,
`OrganizationErrors`
✅ `ListOrganizations` query + endpoint
✅ `IOrganizationRepository`, `OrganizationRepository`, DI wiring
✅ `CreateOrganization` command, handler and `POST /organizations`
✅ Organization type is validated against the defined values
✅ `features/platform/organizations/` — list, create dialog, create form
✅ `tests/Organization/RegOS.Organization.Domain.Tests` — 9 tests

❌ `Activate` / `Deactivate` exist on the aggregate but **no command, handler or
endpoint reaches them** — working behavior, unreachable from outside
❌ No `UpdateOrganization`, `GetOrganization`
❌ No `Code` field — the UI sketch shows Name / Code / Status; the aggregate has
`LegalName` / `Type` / `Status`
❌ No settings (time zone, culture)
❌ No organization detail page

### The retirement-path rule

Discovered while writing the browser spec for slice 1, and general enough to
apply to every future slice:

> **A feature that creates long-lived business data needs its corresponding
> lifecycle operation before we can rely on automated end-to-end testing.**

The operation differs by domain — delete, archive, deactivate, cancel — but the
requirement is the same: **run the suite N times, and the repository is
unchanged afterwards** ([ADR-019](../adr/ADR-019-testing-strategy.md) rule 1).

When a create slice ships without one, the browser spec for it is deferred to
the slice that provides the retirement path. That is not a defect in the create
slice; it is what makes the retirement slice urgent.

### Exit criteria — behaviors

- ✅ An organization cannot be created without a legal name.
- ✅ An organization cannot be created with a type outside the defined values.
- ✅ Deactivating an already-inactive organization is rejected as a **409 state
  conflict**, not silently accepted — and activating an already-active one is
  rejected the same way. Both methods were unconditional;
  [ADR-009](../adr/ADR-009-command-validation-model.md) decided the status code.
- ✅ A missing organization is a 404 with a distinct not-found page, not a
  generic error.
- ✅ Editing is permitted in either state and changes neither. Deactivation
  retires an organization; it does not freeze the record.
- ✅ The full lifecycle is verified in a browser: create → view → edit →
  deactivate → activate.

### Deferred out of this milestone

- **"A deactivated organization rejects user invitations."** This was claimed as
  a Milestone 1 exit criterion and **was never true here** — nothing in
  Organization invites anyone. `IUserPolicy.EnsureOrganizationCanAcceptUsersAsync`
  exists in Platform and is called by `InviteUserHandler`, but no test exercises
  it against a deactivated organization.

  It is a **dependency, not an Organization behavior**: Milestone 3 (User
  Management) is the first place it can be verified end to end, and its exit
  criteria now carry it.

- **Organization `Code`, and settings such as time zone and culture.** Never
  built; no feature has needed them. See *Decisions deferred* below.

- **A status filter on the directory.** Retired organizations accumulate from
  every browser run and the list shows all of them. Harmless today — the
  regulatory applicant dropdown already filters to active — but it grows
  monotonically.

### Decisions this milestone will force

- **Does `Organization` need a `Code` separate from its id?** The UI sketch says
  yes; the domain has never had one. If adopted, decide its uniqueness scope.
  Write it when the Update slice is built — not before. (Next free ADR number is
  **021**; ADR-017 through ADR-020 were taken on 2026-07-20.)

---

## Milestone 2 — Authentication

**Goal:** every subsequent milestone runs as a real authenticated user.

Moved ahead of User Management deliberately, and the reason holds: the `User`
aggregate already exists, so authentication has something to attach credentials
to. Building user administration first would mean building it twice — once
against `X-Tenant-Id`, once against a real identity.

### Current state — Phase 1 substantially complete

| Slice | | Commit |
|---|---|---|
| AUTH-001 | An email address identifies exactly one user (ADR-021) | `60edabd` |
| AUTH-002 | Store and verify a user password | `5e1b7c7` |
| AUTH-003 | Sign in and receive an access token (ADR-022) | `592494a` |
| — | A credential cannot outlive its user (ADR-023, restated by ADR-026) | `e6c6041` |
| AUTH-004 | Validate access tokens; `ICurrentUser` | `b7fbe13` |
| AUTH-005 | Tenancy from identity; `X-Tenant-Id` removed (ADR-024) | `b5a9e85` |
| AUTH-006 | Refresh tokens, rotation, cookie sessions (ADR-025) | `6d99045` |
| — | Satellites are defined by lifecycle ownership (ADR-026) | `b9e8441` |
| AUTH-007 | Invitation acceptance; first credential (ADR-027) | this slice |

`HeaderTenantContext` is **deleted**. The tenant is the authenticated caller's
organization claim, so it is proven rather than asserted.

**Milestone 2 Phase 1 and Phase 2 are complete.** The lifecycle closes:

```
Invited ──accept──▶ Active ──deactivate──▶ Inactive ──activate──▶ Active
```

Exactly one edge into `Active` from `Invited`, and it establishes a password on
the way through — so **every Active user has exactly one credential**, and the
admin shortcut that used to violate it is gone (ADR-027).

**AUTH-008 is done.** Password reset is a second consumable grant, and the test
of whether `Invitation` generalizes has been run: `PasswordReset` was written
without reading `Invitation`, and with names normalized and comments stripped
the two differ only in the name of one predicate and one error constant.

They are still separate aggregates. The similarity is structural; the
distinction is semantic — different meaning, expiry (one hour against seven
days), callers and eligibility rules — and merging them would need a
discriminator column, which is how a `Grant` table acquires a `switch`. The
finding is recorded as evidence about the domain, not as a mandate about the
code: what has actually emerged is a *lifecycle* (issue → usable → consume once
→ revocable → expires), and a lifecycle is not a class.

Known gaps carried forward:

- **No UI for password reset.** Both endpoints work and are covered at three
  layers, but nothing in the React app calls them: no "forgot password" link,
  no `/reset-password` page. The slice is verifiable by API and by test, not in
  a browser. That is a deliberate exception to the vertical-slice rule and it
  should be closed before the authentication subsystem is called finished.
- **Nothing revokes a user's sessions when they are deactivated.** Today the
  guarantee is weaker than it looks: a deactivated user cannot *refresh*,
  because `RefreshSessionHandler` re-checks status, but their access token
  keeps working for up to fifteen minutes. The invariant worth holding is
  "deactivating a user revokes every session immediately." Backlog, not bug.

- **Nothing deletes expired, revoked or consumed tokens** — refresh tokens,
  invitations or password resets. All three tables grow with every sign-in,
  rotation, invite and reset request.
  Harmless at current scale, unbounded at any other. Cleanup strategy pending
  operational requirements; periodic sweep, opportunistic cleanup and a
  database TTL are all defensible and none is obviously right today.
- **No invitation is ever delivered.** `IInvitationNotifier` has one
  implementation that logs the link in Development and one that records the
  failure everywhere else. Real delivery is its own slice.
- **No browser spec covers inviting, activating or deactivating a user.** A
  user cannot be deleted, so any such spec leaks a row per run — the retirement
  path rule from Milestone 1. Covered by host integration tests instead, which
  can clean up after themselves.

### Scope

- `UserCredential`, password hashing.
- Login, logout.
- ~~JWT access token; refresh token with rotation.~~ Done in AUTH-003/AUTH-006.
- ~~Forgot password / reset password.~~ Done in AUTH-008. It reused
  `SecretTokenFactory` and nothing else; `PasswordResetTokenIssuer` is the
  third issuer of the same shape, which is the strongest abstraction candidate
  the slice produced — left alone deliberately, pending the retrospective.
- ~~**Replace `HeaderTenantContext` with a claims-based `ITenantContext`.**~~
  Done in AUTH-005. Nothing above it changed, by design (ADR-013) — all
  fourteen `ITenantContext` consumers were untouched.
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
- **A deactivated organization rejects user invitations, and reactivating it
  restores them.** Inherited from Milestone 1, which claimed this and could not
  verify it — nothing in Organization invites anyone.
  `IUserPolicy.EnsureOrganizationCanAcceptUsersAsync` already exists and is
  called by `InviteUserHandler`; no test has ever exercised it against a
  deactivated organization. This is the first milestone that can.
- A second invitation to the same email is rejected as a conflict — **including
  from a different organization**. An email address identifies exactly one user
  across RegOS ([ADR-021](../adr/ADR-021-email-is-globally-unique.md)).

  This replaces the earlier criterion *"the same email can be invited by a
  different organization independently"*, which was derived from the old
  `(OrganizationId, Email)` index rather than from a business rule, and which
  made login unable to resolve a user from an email.
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
