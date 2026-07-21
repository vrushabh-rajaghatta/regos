# ADR-027 — An Invitation Is a Consumable Grant Against an Existing User

**Status:** Accepted · **Date:** 2026-07-21 ·
**Supersedes:** [ADR-014](ADR-014-invitation-is-a-user-status.md) ·
**Related:** [ADR-021](ADR-021-email-is-globally-unique.md) (email identifies a
user), [ADR-022](ADR-022-authentication-failure-is-a-fourth-exception.md) (401),
[ADR-025](ADR-025-sessions-are-server-owned-cookies.md) (sessions),
[ADR-026](ADR-026-lifecycle-owned-satellites.md) (satellite lifetime)

## Context

ADR-014 recorded that RegOS has no `Invitation` aggregate: `InviteUser` calls
`User.Create` directly, which sets `Status = Invited`. It listed the absence of
expiry, revocation and resend as accepted negatives, and it named its own
successor — *"a proposal to introduce a separate `Invitation` aggregate would be
a new ADR superseding this one."*

Nothing was wrong with that decision. What changed is that the machinery it was
waiting for now exists: credentials, hashing, sessions, and — from AUTH-006 — a
worked example of a secret that is generated randomly, stored hashed, expires,
and can be revoked.

A review before AUTH-007 also found something that is not a modelling question
at all:

> **`Active` without a credential is reachable through the current UI.**
> `UserDetailsPage` offers *Activate User* for `Invited` users, so an
> administrator can activate someone who has never set a password. The
> development database contains two such accounts.

That is a live path, not historical residue, and it is the reason this ADR
changes an endpoint's meaning as well as adding an aggregate.

## Decision

### What ADR-014 decided, and still stands

**An invited person is a real `User` row from the moment of invitation**, in
`Status = Invited`, with a real `UserId`. There is no pre-user record that
becomes a user on acceptance, so there is no accept-time reconciliation between
two identities, and email uniqueness is still enforced in exactly one place.

### What changes

**An `Invitation` is a separate aggregate: a consumable grant addressed to a
`UserId` that already exists.** It is not an alternative representation of the
person — it is permission to establish that person's first credential.

```
User (Invited) ──────────────┐
                             ├── Invitation ── token (hashed), expiry, consumption
                             └── created together, never one without the other
```

An invitation is **pending or finished, never both**. Finished means consumed or
revoked or expired; those are asked as one question, so no caller can check half
of it.

- **Single use.** Consuming an invitation is a state transition, not a flag a
  second caller could race.
- **Expires.** Seven days by default. ADR-014 accepted "never expires"; that did
  not survive contact with the authentication model, where every other long-lived
  secret expires.
- **Revocable, and resend revokes.** Resending issues a new token and revokes the
  previous one, so at most one token is ever live per user.
- **Stored hashed.** SHA-256, never the value — the same reasoning as refresh
  tokens (ADR-025): 256 bits of RNG output has nothing to guess, and a per-value
  salt would make the stored hash impossible to *look up*, which is the one
  operation the store exists for.

### Acceptance is an application orchestrator

```
validate token → set password → activate user → consume invitation
```

Every step but the first already existed. `SetUserPasswordHandler` is used
**unchanged**, and no new method appears on `User`: `Activate()` already covers
`Invited → Active`.

The password is set **before** the user is activated. The two are separate units
of work, and if the second fails the user is left inactive with a credential —
recoverable by retrying — rather than active without one, which is the state
this ADR exists to eliminate.

### A deactivated invitation cannot be accepted

If the user was deactivated between invitation and acceptance, acceptance fails.
Someone withdrew access deliberately, so the invitation no longer represents the
organization's intent.

### `Activate` no longer applies to an invited user

`ActivateUserHandler` now rejects any user who is not `Inactive`, and the UI no
longer offers the action for `Invited` users. There is exactly one edge into
`Active` from `Invited`, and it runs through acceptance:

```
Invited ──accept──▶ Active ──deactivate──▶ Inactive ──activate──▶ Active
```

This is the change that makes the invariant enforceable rather than aspirational:

> **Every `Active` user has exactly one credential.**

The restriction lives in the handler, not the aggregate. `User.Activate()` still
means *make this user active* and is invoked by both paths; the handlers decide
who may invoke it and from what state. The aggregate owns what activation
means; the application owns who is allowed to ask.

### Delivery is abstracted, not built

`IInvitationNotifier` has one implementation that logs the acceptance URL.
Building real email delivery is a slice of its own, and `InviteUser` must not
know about SMTP either way.

## Consequences

**Positive**

- Every negative ADR-014 accepted — no expiry, no revocation, no resend — is
  closed.
- An unaccepted invitation stops being valid forever. A mailbox compromised a
  year later is not an account takeover.
- The invariant *every `Active` user has exactly one credential* becomes true and
  stays true, because the only path that could violate it is gone.
- `SetUserPasswordHandler` gained its first real caller without changing, which
  is what it was written for. It is not tenant-scoped, and acceptance — being
  anonymous — could not have used it if it were.

**Negative**

- **Two records are created per invitation** where ADR-014 had one, and they must
  be created together. The foreign key makes an orphaned invitation impossible
  (ADR-026), but nothing at the schema level requires an invited user to *have*
  one.
- **Existing invited users have no invitation.** Four in the development
  database. They cannot accept until they are re-invited, and there is no
  migration to manufacture tokens for them — a token nobody was ever sent is not
  a token.
- **An administrator can no longer activate an invited user**, which is a
  capability being removed rather than added. It was the only way to reach
  `Active` without a password, so removing it is the point; anyone relying on it
  as a shortcut now has to resend the invitation instead.
- Delivery still does not exist. In development the acceptance URL is written to
  the log, which is fine there and is not a deployment story.
- The invitation token and the refresh token now share a generator and a hash,
  but nothing else. If a third consumable secret appears and also diverges, the
  shared piece is the part to re-examine (ADR-018).

## Revisit When

- Real delivery arrives, which decides whether the URL is built by the API or by
  the notifier, and where the web base URL comes from.
- Password reset is built (AUTH-008). It is the second consumable grant, and the
  first honest test of whether `Invitation` generalizes or whether the two
  simply share a token generator.
- An invitation needs to be addressed to someone who is *not* yet a user —
  self-service signup, or inviting an address that already belongs to another
  organization. That would reopen ADR-014's original question rather than this
  one.
