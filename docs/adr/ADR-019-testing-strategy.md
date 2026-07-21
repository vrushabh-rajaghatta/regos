# ADR-019 — Testing Strategy

**Status:** Accepted · **Date:** 2026-07-20 (retro-documented) ·
**Related:** [ADR-011](ADR-011-development-lifecycle.md) (development lifecycle)

> **Retro-documented.** Every convention below was learned from a test that
> reported the wrong answer. They are recorded here because three of them were
> discovered more than once, in different layers — evidence that the lesson does
> not transfer on its own.

## Context

RegOS has four kinds of test, and each answers a question the others cannot:

| Layer | Answers |
|---|---|
| Domain unit tests | Do the aggregate's invariants hold? |
| Application integration tests | Does the handler behave against real Postgres? |
| HTTP verification | Does the API return the right status and body? |
| Browser specs | Does the user-facing behaviour actually work? |

A green suite is only worth what its assertions are worth. Each rule below
exists because a suite was green while the behaviour was wrong.

## Decision

### 1. No test may depend on ambient database contents

A test seeds what it needs and cleans up after itself. It must pass against an
empty database and against a populated one.

This was discovered three times:

- Six integration test classes called `RegulatoryApplications.FirstAsync()` and
  used whatever was there. Resetting product data failed **30 tests at once**.
- A browser spec asserted "filtering by Archived yields an empty list" — true
  only until archiving was implemented.
- A browser spec edited the first search result for `"ASP"` and broke when that
  product was archived, since the directory hides archived products. It would
  have failed as a UI bug that was not one.

The measurable property: **after a full run, the seeded data set is unchanged.**

### 2. Wait for the observable business outcome, not incidental state

Waiting on a row count passed while the *previous* result was still rendered,
and the assertion then read stale data. Wait for the content expected.

```ts
// Wrong — passes while the old result is still on screen
await page.waitForFunction(() => rows().length === 1);

// Right — cannot pass on stale content
await expect(page.locator('...h3')).toHaveText([/Aspirin/]);
```

### 3. Fail on unexpected errors, never disable the check

A spec that deliberately provokes a 404 will see the browser log it. Filter that
one message narrowly; a genuine React or runtime error must still fail. Widening
the filter to make a test pass destroys the assertion's value — it was a missing
React `key` in `RegulatoryNavigation`, caught by exactly this check, that
nothing else in the pipeline could see.

### 4. Verify the consumer of invalidated state

Asserting that a detail page shows an edit proves little: that view holds fresh
state regardless. Assert the **list** also reflects it — the list only refreshes
if the cache was genuinely invalidated.

### 5. Browser verification is part of Done

A story that touches the UI is not complete until exercised in a real browser.
Compilation, type-checking, HTTP verification and unit tests all pass on a UI
that is broken. Specs live in `tests/Browser/` and drive the locally installed
Chrome, so no browser is downloaded.

They run against a **running stack** — real app, real API, real Postgres —
because the defects this gate exists to catch only appear when the whole thing
runs together.

**Refined 2026-07-21, by AUTH-008A.** The rule is about *capabilities*, not
*channels*. Where the last step of a flow crosses a boundary deliberately
outside the product — email, SMS, an external identity provider — the browser
verifies everything up to that boundary and host integration tests own the rest.

Password reset is the first case. A spec can drive the request, the identical
confirmation, the navigation, the validation and the completion page; it cannot
read the link, because the link arrives by a channel RegOS does not implement.
The tempting fix is a development-only endpoint that hands the last token to
whoever asks. It was rejected: its only consumer would be Playwright, and it
would create a second way to obtain a grant that the product itself does not
have. **A test must not be the reason an application gains behaviour.**

This is a narrow exception and should stay narrow. It does not license skipping
browser coverage because a flow is awkward — only because its final step is not
ours to perform.

### 6. Verify invariants by attacking the contract, not the UI

A disabled input proves nothing about the model. `ProductCode` immutability is
verified by `PUT`ing a `code` field and confirming it is ignored. The UI is
convenience; the command contract is enforcement.

## Consequences

- Specs are slower to write, because each seeds and cleans up its own data.
- The suite is deterministic and order-independent, and can run repeatedly
  against a shared development database.
- Browser specs are verification, not CI: they assume a running environment.
- These conventions are enforced by review, not tooling. Rule 1 was violated
  twice *after* being fixed once, so the review question is explicit: **what
  does this test assume already exists?**
