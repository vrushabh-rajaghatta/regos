# ADR-022 — Authentication Failure Is a Fourth Shared Exception

**Status:** Accepted · **Date:** 2026-07-21 · **Extends:**
[ADR-012](ADR-012-shared-semantic-exception-model.md) ·
**Related:** [ADR-009](ADR-009-command-validation-model.md) (command validation),
[ADR-021](ADR-021-email-is-globally-unique.md) (email identifies one user)

## Context

[ADR-012](ADR-012-shared-semantic-exception-model.md) fixed RegOS's failure
vocabulary at three types — 400, 404, 409 — and closed with:

> **Revisit When:** a failure class arrives that genuinely fits none of the
> three — authorization denial (403) is the most likely candidate once Epic 4
> lands.

Login arrives first, and it is the sibling case: **401, not 403.** The caller
has not failed an authorization check; they have failed to establish who they
are.

None of the three existing types can express it:

- `DomainException` (400) would say the request was malformed. It was not — a
  well-formed email and password that simply do not match is a perfectly valid
  request.
- `NotFoundException` (404) would answer *"no such user"*, which is exactly the
  account-enumeration disclosure login must avoid.
- `BusinessRuleViolationException` (409) would imply the request could succeed
  against a different system state. Sometimes true, but 409 tells a client to
  reconcile and retry, which is wrong advice for a wrong password.

## Decision

A fourth type joins the shared vocabulary:

| Exception | Meaning | HTTP |
|---|---|---|
| `AuthenticationFailedException` | The caller has not established who they are | **401** |

It derives from `DomainException`, like the other two specialisations, so it can
never escape the middleware as a 500. The middleware catches it before
`BusinessRuleViolationException`, because catch order is load-bearing.

### It carries one message, always

Every authentication failure — unknown email, wrong password, inactive user,
invited-but-not-yet-activated user, user with no credential — produces the
**same** status and the **same** message. The caller cannot distinguish them.

This is the reason the type exists rather than reusing 404. An endpoint that
answers "no such user" for one address and "wrong password" for another is an
account enumeration oracle, and [ADR-021](ADR-021-email-is-globally-unique.md)
made addresses globally unique, so that oracle would answer questions about
every organization at once.

The specific reason is available to logs, never to the response body.

## Consequences

- The failure vocabulary is four types, not three. That is a real cost: the
  decision tree in [ADR-009](ADR-009-command-validation-model.md) gains a
  branch, and "which exception?" is a slightly harder question than it was.
- Authentication handlers cannot accidentally leak account existence through
  their choice of exception, because the type that models the failure has only
  one message.
- 403 remains unclaimed. When Epic 4 introduces authorization, it needs its own
  type: *"I know who you are and you may not do this"* is a different statement
  from *"I do not know who you are"*, and collapsing them would make the API
  unable to tell a client whether re-authenticating would help.

## Revisit When

- Epic 4 adds authorization, at which point a fifth type (403) is expected and
  the two must stay distinct.
- A requirement arrives to distinguish *locked* from *invalid* — account
  lockout after repeated failures is deliberately absent today, and it is the
  most likely reason to want a second authentication-related status.
