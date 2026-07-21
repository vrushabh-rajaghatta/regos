# ADR-029 — A Session Is a First-Class Resource, and Records Minimal Device Context

**Status:** Accepted · **Date:** 2026-07-21 ·
**Related:** [ADR-022](ADR-022-authentication-failure-is-a-fourth-exception.md)
(uniform failures), [ADR-024](ADR-024-tenancy-is-derived-from-identity.md)
(identity is proven, not asserted),
[ADR-025](ADR-025-sessions-are-server-owned-cookies.md) (session transport),
[ADR-026](ADR-026-lifecycle-owned-satellites.md) (satellite lifetime),
[ADR-028](ADR-028-credential-replacement-revokes-derived-trust.md) (revocation)

## Context

Before AUTH-010 a "session" was not a thing. It was an inference: a refresh
token existed, therefore somebody was signed in. That was adequate while nobody
had to look at one.

Making sessions visible broke the inference, because **refresh tokens rotate and
sessions do not**. Every refresh revokes one token and issues another, so with
fifteen-minute access tokens a single browser produces roughly thirty-two token
rows in a working day. A list built from tokens would show thirty-two entries
for one laptop, and a `DELETE` would target an id that stopped existing at the
next refresh.

The obvious repair — stop rotating, and update the token in place — was
rejected. Recognising a *superseded* token is what makes replay detectable, and
replay detection is the strongest security property in the subsystem. Keeping
the token chain is not incidental; it is the point.

A second question arrived with the first. A list showing only

```
Created 2026-07-19 14:22 · last used 3 minutes ago
Created 2026-07-21 09:05 · last used just now
```

is still implementation vocabulary wearing a friendlier label. A user cannot
tell which row is the laptop they left at the office, so "sign out that device"
remains unanswerable. Making it answerable means storing something about the
device — which is the first behavioural personal data RegOS holds, and a data
governance decision rather than an implementation detail.

## Decision

**A `Session` is an aggregate: one row per sign-in, whose identity survives
rotation.** Refresh tokens belong to it. The inversion is deliberate — refresh
tokens now exist because sessions do, rather than the reverse.

**Each session records exactly two pieces of device context, captured once at
sign-in:**

- the raw `User-Agent` header, truncated to 512 characters;
- the source IP address at creation, in `CreatedFromIp`.

And nothing else. Specifically **not**: parsed browser or OS names, geolocation,
device fingerprints, screen or locale data, a per-request IP history, or any
derived "trust score".

Three rules constrain it:

1. **Captured from the transport, never from the request body.** A caller must
   not choose what their own session says about them. `LoginCommand` carries
   these as parameters the endpoint fills from `HttpContext`, in the same spirit
   as ADR-024: what identifies you is observed, not asserted.
2. **Stored raw, displayed raw.** No parsing into "Chrome on macOS". The moment
   RegOS interprets a User-Agent it owns the interpretation, and a confidently
   wrong guess about someone's device is worse than an ugly string they
   recognise. The UI shows the string.
3. **Recorded once, not tracked.** `CreatedFromIp` answers "where did this begin",
   not "where is this now". Updating it per request would turn a session list
   into a movement log, which is a different product with different obligations.

**Retention.** Device context lives and dies with its session row. Sessions are
deleted with their user (ADR-026 cascade), and revoked or expired sessions
become eligible for deletion 90 days after they end — long enough to answer
"was that me?", short enough that it is not an archive. The sweeper that
enforces this does not exist yet and is recorded with the other token-cleanup
work; **the policy is stated here so the eventual cleanup has a specification to
implement rather than a number to invent.**

## Consequences

- A user can be shown one entry per device and can end one, end all the others,
  or end their own current session. `POST /api/auth/sessions/revoke-others`
  finally has a meaning for "others", which AUTH-009 deliberately deferred for
  want of the vocabulary.
- **Revoking a session revokes its tokens too.** `SessionRevoker` does both, and
  doing only the first would produce a sessions page that lies — an entry gone
  from the list whose refresh token still worked.
- **Signing out ends the session, not merely the token.** Otherwise the browser
  that just left would still be listed as live on the user's own page.
- **A session id is supplied by the caller, so ownership is proven, never
  assumed** — and a session belonging to somebody else answers **404, not 403**.
  "That is not yours" confirms the id was real, which is the oracle ADR-022
  closed at sign-in reappearing at a new endpoint.
- **Existing refresh tokens were deleted by the migration** rather than
  backfilled. They predate sessions and have nothing to point at; the entire
  cost is that everyone signs in once more, whereas inventing session rows would
  put fabricated device data in front of users. Confirmed necessary by running
  the migration without the delete and watching Postgres reject the foreign key
  (23503).
- **The IP is the proxy's until forwarded headers are configured.** There is no
  `UseForwardedHeaders` anywhere in `src/`, so behind any load balancer this
  field shows every user the same wrong address — worse than absent, because it
  looks authoritative. Recorded with SEC-001, which needs the same fix for a
  different reason.
- The in-process test host has no socket, so `RemoteIpAddress` is null there and
  the integration tests assert the User-Agent only. Stated in the test rather
  than quietly dropped.
