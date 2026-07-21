# ADR-025 — A Session Is Server-Owned, and Travels in Cookies

**Status:** Accepted · **Date:** 2026-07-21 ·
**Supersedes:** the `localStorage` transport accepted by
[ADR-024](ADR-024-tenancy-is-derived-from-identity.md) ·
**Related:** [ADR-022](ADR-022-authentication-failure-is-a-fourth-exception.md)
(401), [ADR-026](ADR-026-lifecycle-owned-satellites.md) (satellite lifetime)

## Context

AUTH-005 left three things open, all recorded at the time rather than
discovered later:

1. The access token lived in `localStorage`, readable by any script on the
   origin.
2. Signing out was purely client-side. The server was never told, so a captured
   token kept working.
3. A fifteen-minute access token with no refresh meant a session ended after
   fifteen minutes, or the token had to be made long-lived to compensate.

These are one problem. All three follow from the browser owning the session and
the server merely issuing bearer tokens into it.

## Decision

**The server owns the session.** It is carried by two `HttpOnly` cookies that
JavaScript cannot read, and the endpoints that begin, renew and end it are
responsible for their whole lifecycle.

| | `regos_access` | `regos_refresh` |
|---|---|---|
| Contains | The JWT | 256 random bits |
| Lifetime | 15 minutes | 14 days |
| Path | `/` | `/api/auth` |
| Stored server-side | Not at all | SHA-256 of the value |

`POST /api/auth/login` returns **204 with no body**. The response used to carry
the access token for JavaScript to store; returning it now "for convenience"
would put it straight back within reach of any script on the page.

### Refresh tokens are hashed, and rotated

`RefreshToken` is its own aggregate. `UserCredential` answers *can this person
sign in at all* and there is exactly one per user forever; a refresh token
answers *is this session still alive*, is created and destroyed repeatedly, and
there will one day be several per user. Different lifecycle, different
aggregate.

Only a hash is stored, for the same reason passwords are hashed — with the same
consequence: RegOS cannot show a user their own token, only recognise it.

The hash is **SHA-256, not the password hasher**, and that is not a weaker
choice but a different one. A password is low-entropy and human-chosen, so it
needs a slow salted hash to survive offline guessing. A refresh token is 256
bits of RNG output: there is nothing to guess, slowness buys nothing, and a
per-value salt would make the stored hash impossible to *look up* — which is the
one operation the store exists for.

Every refresh **rotates**: the presented token is revoked, records the id of its
replacement, and a new one is issued. Rotation is what makes theft detectable.

### Presenting a rotated token ends every session for that user

If an already-rotated token arrives, either the legitimate client is out of step
or a stolen token is being replayed. Those are indistinguishable from the
server's side, so the safe reading is the pessimistic one: every live refresh
token for that user is revoked and both parties must sign in again.

### The client cannot know whether it is signed in

An `HttpOnly` cookie is invisible to JavaScript, so "am I signed in" stops being
a local question. `RequireAuth` asks the API. This is strictly better than what
it replaced — that trusted the mere presence of a value in local storage, and a
forged one produced a rendered page full of failing requests.

`apiFetch` treats a 401 as recoverable: it attempts one refresh and replays the
request. At most one refresh is ever in flight, because several parallel
requests each firing their own would have the second present a token the first
had already rotated — which reads as replay and would end the session.

## Consequences

**Positive**

- An XSS flaw can still *act* as the user, but can no longer exfiltrate the
  tokens and replay them elsewhere, later, from another machine. Asserted by a
  browser spec that reads `document.cookie`, `localStorage` and
  `sessionStorage` after signing in and finds nothing.
- Signing out is real. The refresh token is revoked server-side, so a token
  captured beforehand is worthless afterwards.
- Sessions last fourteen days while access tokens stay at fifteen minutes.
  Deactivating a user takes effect within fifteen minutes, because status is
  re-checked on every refresh rather than only at sign-in.
- A stolen refresh token is detectable and self-limiting rather than silently
  valid for its full lifetime.

**Negative**

- **Cookies bring CSRF risk that bearer headers do not**, because the browser
  attaches them automatically. `SameSite=Strict` is the mitigation, and it is
  the only one — there is no anti-forgery token. That is defensible for an
  internal tool where no flow is meant to be reachable from another site, and it
  is the thing to revisit first if RegOS ever gains one that is.
- `SameSite=Strict` means a link into RegOS from an external site lands signed
  out. Chosen over `Lax` deliberately.
- **A signed-out visitor causes two 401s on first load** — `/api/auth/me`, then
  one refresh attempt. Unavoidable: the client cannot tell "no cookie" from
  "expired cookie" without asking. The browser specs declare these rather than
  filtering all console errors.
- **The access token still cannot be revoked.** Logout clears the cookie and
  kills the refresh token, but a JWT already extracted keeps working until it
  expires. A test asserts this explicitly, so the limit is recorded rather than
  assumed away.
- `Secure` is set even in development. Browsers exempt `localhost`, so this
  needs no environment switch — and an environment switch is exactly how a
  cookie ends up sent in clear text in production.
- CORS now requires `AllowCredentials`, which is only legal beside a named
  origin. `AllowAnyOrigin` can never be reintroduced.

## Revisit When

- RegOS gains a flow that must be reachable from another site, or a
  browser-behaviour change weakens `SameSite`. That is when an anti-forgery
  token stops being optional.
- Access token revocation is genuinely needed — a deny-list keyed on `jti`,
  paid for with a lookup on every request.
- Users need visible session management ("sign out everywhere", device lists).
  The `ReplacedBy` chain already records enough to build it.
- The API and the SPA are served from one origin, which would remove the
  cross-origin CORS requirement and allow `SameSite` to be reconsidered.
