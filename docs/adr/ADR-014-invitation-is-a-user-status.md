# ADR-014 — Invitation Is a User Status, Not an Aggregate

**Status:** Superseded by
[ADR-027](ADR-027-invitation-is-a-consumable-grant.md) ·
**Date:** 2026-07-20 (retro-documented) · **Superseded:** 2026-07-21 ·
**Implemented by:** commits `9dbeb04`, `9ebfa5a`

> **Partly superseded, and it named its own successor.** Its central choice —
> that an invited person is a real `User` row in `Invited` status, not a
> pre-user record — is unchanged and still describes the code.
>
> What changed is the sentence *"there is no `Invitation` aggregate"*. There now
> is one, holding the acceptance token, its expiry and its consumption. It is
> addressed to a `UserId` that already exists, so it creates no second identity
> and no accept-time reconciliation. The negatives this ADR accepted — no
> expiry, no revocation, no resend — are all closed by
> [ADR-027](ADR-027-invitation-is-a-consumable-grant.md). Cite that for guidance.

> **Retro-documented.** This decision exists in code and has never been written
> down. It is recorded here as the *current* architecture, not as a preference.
> A proposal to introduce a separate `Invitation` aggregate would be a new ADR
> superseding this one — with a migration cost — not a correction of it.

## Context

Inviting someone into an organization creates a person who can eventually sign
in but cannot yet. There are two ways to model the gap:

1. An `Invitation` aggregate addressed to an email, which produces a `User` when
   accepted.
2. A `User` created immediately in a pre-active state.

RegOS implemented (2) without recording why.

## Decision

There is no `Invitation` aggregate. `InviteUser` calls `User.Create(...)`
directly, which sets `Status = UserStatus.Invited`. `UserStatus` is
`Active | Inactive | Invited`.

The invited user is a real row with a real `UserId` from the moment of
invitation.

## Consequences

**Positive**

- One aggregate, one lifecycle, one repository. No accept-time reconciliation
  between two records.
- The user appears in the directory immediately, which is what the Users screen
  shows today.
- Email uniqueness is enforced in one place — `IUserPolicy.EnsureEmailIsUniqueAsync`
  — rather than across both a users table and a pending-invitations table.

**Negative**

- No invitation-specific state: no expiry, no token, no revocation, no resend
  history, no record of who invited whom.
- An invited-but-never-activated user permanently occupies its email address
  within the organization.
- "Invited" is a `User` status rather than a lifecycle of its own, so an
  invitation cannot be declined — only the user deactivated.

## Revisit When

Reconsider — as a new ADR — if any of these become requirements:

- Invitations must expire, be revoked, or be resent with a new token.
- The same email must be invitable to several organizations independently.
- Audit needs to distinguish "invitation sent" from "user created".
- Accepting an invitation must be possible for someone who already has an
  account elsewhere in RegOS.

The first of these is the most likely, and arrives with Epic 3 (Authentication),
because an invitation token and a password-reset token are the same shape.
