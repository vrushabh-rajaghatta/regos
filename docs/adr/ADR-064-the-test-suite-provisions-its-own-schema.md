# ADR-064 — The Test Suite Provisions Its Own Schema

**Status:** Accepted · **Date:** 2026-08-05 ·
**Related:** [ADR-019](ADR-019-testing-strategy.md) (testing strategy — rule 1 strengthened, one Consequence overtaken),
[ADR-016](ADR-016-persistence-access-model.md) (why these tests use real Postgres at all),
[ADR-031](ADR-031-tenant-isolation-by-query-filters.md) (the filters a seeded database must satisfy),
[ADR-018](ADR-018-rule-of-three.md) (seven demonstrated call sites, not symmetry),
[EPIC-023](../product/epics/EPIC-023-test-schema.md) D1–D8

## Context

**27 test files across 7 of RegOS's 19 test suites hard-code
`Host=localhost;…;Database=regos`** — the developer's own working database.
Nothing migrates it. The suite assumes somebody already did.

That assumption is the one thing the suite never tests, and the failure mode is
not "the tests go red". It is subtler:

> **Green means "nothing collided", not "the schema is current".**

A stale schema only turns a test red when a migration happens to touch a read
path some existing test already exercises. EPIC-010b added three tables and
stayed green throughout, because its new tests were domain tests and its new
tables were read by nothing older. EPIC-022 S004 added two more and did the
same, **one migration behind**. The first change to an *existing* read path —
`ListRegistrationMarkets` reaching `CountryRegions` — went red immediately, and
by then the database was five migrations behind and 18 of 19 suites failed at
once.

Three observations inside one epic, and the third is the one that settles it: a
suite that cannot tell whether its own foundation is current is reporting on
something other than the code.

### What was measured before this was decided

The obvious objection to provisioning a database per test assembly is cost, and
the obvious mitigations are template databases, shared snapshots, or migrating
once and reusing. **None of them are needed**, and the numbers are the reason
this ADR chooses the naive implementation rather than defending it:

| | |
|---|---|
| 86 migrations applied to an empty database | **0.165 s** |
| `CREATE DATABASE … TEMPLATE` of the same schema | **0.15 s** — indistinguishable |
| the figure that nearly changed the design | **14 s** — which turned out to be `dotnet ef`'s *build and startup*, not migration |

**The measurement chose the design.** Had the chain genuinely cost 14 seconds,
this ADR would have specified a template database and inherited its
coordination problem. It costs a sixth of a second, so there is nothing to
optimise and nothing to coordinate.

## Decision

### 1. The automated test environment executes against a schema produced from the current migration chain

Stated as the requirement rather than the mechanism, because the requirement is
what a future reader should hold this against. Any implementation satisfying it
is compliant; the one below is what RegOS builds.

### 2. The unit is the **test assembly**, not the test run

`dotnet test` gives each test project its own process and nothing coordinates
them, so *"a database per run"* has no implementation. Each database-touching
assembly provisions one database, named `regos_test_<assembly>_<short-guid>`.

Recorded explicitly because the requirement sentence says *run* and the code
says *assembly*, and that gap is deliberate rather than a drift.

### 3. The tests provision it themselves, against the Postgres already running

```
CREATE DATABASE  →  MigrateAsync()  →  the real IDataInitializer chain  →  DROP … (FORCE)
```

The server connection is read from configuration in exactly one place, so a CI
runner can point elsewhere without touching a test file.

### 4. Seeding runs through the **real** initializer chain

Not a test-local fixture that inserts the rows a given suite happens to need.
The initializers are what the API runs at boot, and running anything else would
make the tests green against a world the application never produces.

This makes ordering constraints executable for the first time: `SiteInitializer`
must run after `IdentifierSchemeDataInitializer`, which was discovered in
EPIC-010c by booting an empty database and getting a foreign-key violation — not
by reading the registration list.

### 5. The success criterion is schema currency and seeding

The boundary is part of what is being decided, not a caveat on it. **This
decision chooses what counts as done**, and the two halves are stated with equal
weight:

| **This decision proves** | **This decision intentionally does not prove** |
|---|---|
| the schema every test runs against is created from the current migration chain | that any migration's **backfill** does what it says |
| the real initializer chain runs to completion against that schema | that a **populated** database survives an upgrade |

The right-hand column is scope, not shortfall — it names the work this decision
deliberately leaves to a different mechanism, so that the mechanism can be
chosen on its own terms rather than assumed to be covered.

**The reason it cannot be covered here is structural: a backfill only fires on a
database that already holds rows.** On an empty one, EPIC-022's country backfill
matches nothing and the seeder supplies the stability conditions instead — the
migration runs, updates zero rows, and reports success. No arrangement of
freshly-provisioned databases can exercise that path, which is why proving
migrations **both ways** stays manual, as EPIC-010c established, with the trigger
named under *Revisit when*.

## Consequences

**Accepted**

- **[ADR-019](ADR-019-testing-strategy.md) rule 1 is strengthened, not
  replaced.** *"No test may depend on ambient database contents"* was enforced by
  review; a suite that starts from a freshly migrated, freshly seeded database
  now makes the "populated database" half of that rule reproducible.
- **One line of ADR-019's Consequences is overtaken.** It records that the suite
  *"can run repeatedly against a shared development database"* — true when
  written, and no longer how RegOS works. **ADR-019 is not edited**; an accepted
  ADR never is. This ADR is where the change is recorded.
- **The existing hand-written `DisposeAsync` cleanup remains, intentionally.**
  Per-assembly provisioning replaces **cross-run** isolation, not
  **intra-assembly** isolation — tests in one assembly still share one database,
  so the cascades are still doing the job rule 1 gave them. Removing them is a
  separate refactoring once redundancy is *demonstrated*, and mixing 27
  mechanical deletions into an architectural change is exactly what this
  repository has consistently refused to do.
- One new project, `tests/TestSupport/RegOS.TestSupport`, owns the lifecycle.
  Seven demonstrated call sites on the day it is written, so
  [ADR-018](ADR-018-rule-of-three.md) is satisfied rather than bent.
- Every test run now depends on a Postgres the developer can reach **and on
  permission to create databases**. That is a real new requirement, and it is the
  price of the guarantee.
- Test residue stops accumulating in the developer's working database. It held
  **17,400 `Sessions`** and **810 `RefreshTokens`** on the day this was written.

**Refused, with reasons**

| | Why not |
|---|---|
| **Testcontainers** | Adds a Docker dependency to every `dotnet test` and a package to a repository that has fifteen. The requirement is *schema currency*, not *host isolation*. **Falsifier:** a CI runner with no Postgres — and even then, GitHub Actions' `services: postgres` answers the same need without changing how a single test is written |
| **Auto-migrate on application startup** | Moves the question from *"did you migrate?"* to *"did you restart?"*. It makes today's symptom disappear and leaves the schema verified by nothing |
| **A template database, or a migrated snapshot reused across assemblies** | Buys 0.015 s and costs a coordination problem — which process builds the template, and what the others do while it is being built |
| **A database per test class** | Would make the existing cleanup genuinely redundant, which is its only argument. It multiplies provisioning by the class count for an isolation guarantee the cleanup already provides |
| **An in-memory or SQLite provider** | [ADR-016](ADR-016-persistence-access-model.md) puts real query behaviour — global filters, owned collections, `AsNoTracking()` reads — at the centre of what these tests exist to check. A provider that does not behave like Postgres tests something else and reports it as a pass |

**Revisit when**

- **The first customer database exists.** Until one does, every populated
  database in the world is a dev seed or a throwaway and an upgrade costs
  minutes to hand-prove; after one does, it cannot be hand-proved at all. That is
  the trigger for upgrade-path testing against populated databases — the half
  §5 says is not covered.
- **CI runs somewhere without a reachable Postgres.** Then the decision in the
  refusal table above is re-opened, and Testcontainers is the candidate.
- **Provisioning becomes a visible share of a suite's runtime.** At 0.165 s for
  the whole chain there is no case to answer; if the chain grows an order of
  magnitude, the template database refused above becomes worth its coordination
  cost.
