# ADR-028 — Replacing a Credential Revokes Everything Derived From It

**Status:** Accepted · **Date:** 2026-07-21 ·
**Related:** [ADR-018](ADR-018-rule-of-three.md) (when duplication becomes an
abstraction), [ADR-020](ADR-020-ef-read-model-strategy.md) (domain rules must not
be re-expressed in SQL),
[ADR-025](ADR-025-sessions-are-server-owned-cookies.md) (sessions),
[ADR-027](ADR-027-invitation-is-a-consumable-grant.md) (invitations)

## Context

By the end of AUTH-009 there are three ways to establish or replace a password,
and each proves entitlement differently:

| Flow | Proof of entitlement | Credential |
|---|---|---|
| Accept invitation | possession of an invitation token | first |
| Complete password reset | possession of a reset token (a mailbox) | replaced |
| Change password | knowledge of the current password | replaced |

All three call the same primitive, `SetUserPasswordHandler`, which knows none of
that. **The primitive stays dumb; each flow carries its own proof of
entitlement.** That separation has held cleanly since AUTH-007 and this ADR does
not disturb it.

What the three flows did *not* share was what happens afterwards. Password reset
revoked refresh sessions because AUTH-008 remembered to. Change password would
have had to remember separately. And a review before AUTH-009 found a case
neither had considered:

> Someone reading a user's mailbox requests a password reset. The user notices
> something is wrong and changes their password — the action a person takes when
> they believe they have been compromised. Every session dies. The attacker's
> reset link stays live for the rest of its hour, and redeeming it sets a
> password of their choosing and locks the real user out.

The counter-argument is that an attacker holding the mailbox can request another
link anyway. True, and beside the point. The problem is not that the attacker has
no other route; it is that **changing your password appears to shut them out and
does not**. A security control that is weaker than it looks is worse than one
that is honestly absent, because people act on the appearance.

That reframes the question. The rule is not "change password revokes sessions" —
that is a behaviour, and behaviours get re-derived flow by flow until one of them
forgets. The rule is about what a credential *entitles*.

## Decision

**When a credential is replaced, every trust relationship derived from the
previous credential is revoked.**

Today that means, for the user whose password changed:

- every live refresh session, because each was opened by proving the old
  password;
- every outstanding password reset grant, because each is an alternative way of
  replacing the credential that somebody else may be holding.

Invitations are deliberately excluded. An invitation establishes a *first*
credential and cannot exist for a user who has one (ADR-027), so there is
nothing for it to invalidate.

Encoded in `CredentialTrustRevoker`, called by both flows that replace a
credential. `SessionRevoker` underneath it does the narrower job — end every
session for a user — and has one other caller, refresh-token replay detection,
which revokes sessions but **not** reset grants: no credential was replaced
there, so the wider rule does not apply.

Two consequences follow deliberately:

- **The session making the request dies too.** Keeping it would require knowing
  which refresh token is the current one, which means threading session
  transport into an authenticated command. "Sign out my other devices" belongs
  to AUTH-010, which will have the vocabulary for it.
- **Access tokens survive until they expire.** Unchanged, and the same honest
  limit as sign-out: a JWT is a signed statement, not a database row. The
  fifteen-minute lifetime is the mitigation, and a test states it rather than a
  comment.

## Why this and not the alternatives

**Why not leave the revocation inline in each flow?** Because ADR-018's
threshold is not a headcount. Three occurrences justify *asking* whether an
abstraction exists; they do not require creating one — that is exactly why
`PasswordResetTokenIssuer` remains the third identically-shaped token issuer and
was left alone. The difference here is what the duplication protects. Forgetting
to extract an issuer costs twelve lines of tidiness. Forgetting one clause of
this rule is a vulnerability that no test would notice unless someone thought to
write it.

**Why not a bulk `UPDATE`?** Revoking sessions one aggregate at a time is N
round trips where one would do. The one-statement version would be
`UPDATE "RefreshTokens" SET "RevokedOn" = @now WHERE "UserId" = @id AND
"RevokedOn" IS NULL` — and that `WHERE` clause is a second implementation of
`RefreshToken.Revoke`, including its promise to keep the *first* revocation
time, written where the domain cannot see it. They agree today. They would not
survive the first change to either (ADR-020). If the round trips ever matter,
the optimisation goes *through* the aggregate, not around it.

**Why not put it in the domain?** There is no invariant spanning refresh tokens;
each already owns its own rule, and `Revoke` is already idempotent. What was
missing is orchestration across two aggregates, which is an application concern.

## Consequences

- A user who changes their password is signed out everywhere, including the tab
  they did it in. The endpoint clears both cookies so the browser does not sit in
  a half-signed-in state for fifteen minutes.
- Any future credential-derived trust — remembered devices, WebAuthn recovery,
  long-lived API keys — has one place to be added, and one test that will fail
  loudly if it is not.
- `DeactivateUserHandler` still revokes nothing. That gap predates this ADR and
  is not a credential replacement, so it is out of scope here — but the service
  it needs now exists, which turns the fix into one line.
- A wrong current password returns **400** with a *specific* message. It began
  as 401 on the reasoning that the uniform-message discipline (ADR-022) exists
  to prevent enumeration and there is nothing left to enumerate once the caller
  is authenticated. That reasoning was right about the *message* and wrong about
  the *status*, and AUTH-009A's browser spec proved it: 401 means
  "re-authenticate", `apiFetch` acts on it by refreshing and replaying, and the
  second 401 is reported as a dead session — so mistyping your current password
  signed you out of the application. The caller is authenticated (not 401) and
  permitted (not 403); what is wrong is a field in the request, which is 400.

  The general lesson is worth more than the fix: **an HTTP status is an
  instruction to the client, not only a description of what happened.** Choosing
  one on semantics alone, without asking what a conforming client will *do* with
  it, is how a security-motivated decision becomes a usability defect.
- No password-reuse rule was added. Validity is the `Password` value object's
  business, and reuse policy — none, last N, minimum age — is a product feature
  that should be asked for rather than invented on the way past.
