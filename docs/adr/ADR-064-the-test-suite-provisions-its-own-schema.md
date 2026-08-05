# ADR-064 — The Test Suite Provisions Its Own Schema

**Status:** Accepted · **Date:** 2026-08-05 ·
**Related:** [ADR-019](ADR-019-testing-strategy.md) (testing strategy — rule 1 strengthened, one Consequence overtaken),
[ADR-016](ADR-016-persistence-access-model.md) (why these tests use real Postgres at all),
[ADR-031](ADR-031-tenant-isolation-by-query-filters.md) (the filters a seeded database must satisfy),
[ADR-018](ADR-018-rule-of-three.md) (seven demonstrated call sites, not symmetry),
[EPIC-023](../product/epics/EPIC-023-test-schema.md) D1–D8

> **Amended in place at S001, 2026-08-05, before it was merged and before
> anything relied on it.** The decision did not change; **the argument for it
> did.** As first written this ADR said provisioning was cheap and therefore not
> worth optimising. S001 measured it end to end and it is **not** cheap — 2.7 s
> per assembly, ≈ 19 s across seven, against a 28 s suite. The template database
> is still refused, now **on correctness rather than on cost**: it is a cache,
> and no invalidation key derived from migration identity survives an edit to an
> existing migration.
>
> Amended rather than superseded on EPIC-022 D6's precedent — an unmerged,
> not-yet-relied-upon decision is corrected in place; an accepted one that
> something depends on is superseded.

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

### What was measured, at two different layers

The obvious objection to provisioning a database per test assembly is cost, and
the obvious mitigation is a cached template database. **Measuring it took three
attempts, and the first two measured the wrong layer** — which is recorded here
because the layers differ by more than an order of magnitude and a future reader
comparing against the wrong one will reach the wrong conclusion.

| Layer | | |
|---|---|---|
| **Postgres execution** — the 85 migrations as raw SQL against an empty database | **165 ms** | what the server spends |
| **End-to-end provisioning** — `CREATE` + `MigrateAsync()` + the real initializer chain + `DROP` | **≈ 2.7 s** | what a test assembly spends |
| `dotnet ef database update`, wall clock | 14 s | mostly *build and startup*; not a provisioning cost at all |

The gap between the first two is EF, not Postgres. `MigrateAsync()` alone is
**1,985 ms** of the 2.7 s: it instantiates all 85 `Migration` classes and
regenerates every operation, so **the cost scales with the number of migrations
rather than with the SQL they produce**. Seeding is 611 ms, `CREATE` 79 ms,
`DROP` 25 ms, and EF's model build — 851 ms — is excluded because every
database-touching suite already pays it.

**Seven assemblies × 2.7 s is ≈ 19 s, against a full suite of 28 s.**

The decision is unchanged and the argument is not. It is **not** *"provisioning
is cheap, so do not optimise"* — provisioning is not cheap. It is that **the
optimisation costs more than the time it saves**, for the reason given under
*Refused* below.

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
| **Auto-migrate on application startup, *as the answer to this problem*** | It is not one. A suite that runs against whatever the developer's app last migrated has moved the question from *"did you migrate?"* to *"did you restart?"* and still verifies the schema with nothing. **This refuses it as a substitute for provisioning, not as a thing to do** — see the note below, which does exactly that for a different reason |
| **A template database, or a migrated snapshot reused across assemblies** | **Refused on correctness, not on cost.** It would save ≈ 19 s per run, which is real. But a template is a cache, and a cache needs an invalidation key — and **no key derived from migration *identity* detects an edit to an existing migration.** Latest migration id, migration count, `__EFMigrationsHistory`: all unchanged when a migration's *body* changes. That is not hypothetical — **EPIC-023 S003 edits two existing migrations and adds none.** A template built before that edit would survive it, and hand every suite a schema produced from the old bodies while reporting as current. **That is schema drift with extra steps, and harder to notice than the drift this ADR exists to remove.** Any key strong enough — hashing every migration file, or the generated SQL — makes the cache a second artifact whose own correctness has to be proved |
| **A database per test class** | Would make the existing cleanup genuinely redundant, which is its only argument. It multiplies provisioning by the class count for an isolation guarantee the cleanup already provides |
| **An in-memory or SQLite provider** | [ADR-016](ADR-016-persistence-access-model.md) puts real query behaviour — global filters, owned collections, `AsNoTracking()` reads — at the centre of what these tests exist to check. A provider that does not behave like Postgres tests something else and reports it as a pass |

### Addendum, 2026-08-05 — what the host does at boot

Separate from the decision above and recorded beside it, because the two are
easily confused and the refusal table would otherwise read as a blanket ban.

**One setting decides it: `Database:MigrateOnStartup`.**

| | |
|---|---|
| **`true`** — set in `appsettings.Development.json` | the host migrates itself. Point the connection string at an empty database, run the app, and you have a working system: **91 tables, 85 migrations, seeded reference data** |
| **`false`** — set in `appsettings.json`, and the value when the key is absent | the host refuses, naming the pending migrations and what to do about them |

**Configuration rather than `IsDevelopment()`**, because *"may this process alter
the schema?"* is a property of the deployment and not of the word it was
labelled with. A staging box that owns its own database and a production one
that does not are both `Production`.

**False when absent, and `appsettings.json` says so out loud** rather than
leaving it implied — forgetting the setting must be the safe outcome, and a
reader of that file should be able to see the knob exists.

Before either branch existed, an unmigrated database failed in the first
initializer with `42P01 relation "Countries" does not exist` — forty frames deep
and saying nothing about what to do.

**Why `false` is the default.** Three reasons, none of them style:

- instances starting together would race one another
- a long migration would hold the process before it could report healthy
- the alternative grants the application's own credentials the right to alter
  the schema for the entire time it runs

The supported artifact outside Development is
`dotnet ef migrations script --idempotent` — which S003 found broken, fixed and
verified.

> **This changes nothing about the tests.** They provision their own database
> and would be unaffected if the host migrated, refused, or did neither. Written
> down here anyway, because *"the app migrates at startup"* is exactly the fact
> a future reader would use to argue the tests no longer need to.

**Revisit when**

- **The first customer database exists.** Until one does, every populated
  database in the world is a dev seed or a throwaway and an upgrade costs
  minutes to hand-prove; after one does, it cannot be hand-proved at all. That is
  the trigger for upgrade-path testing against populated databases — the half
  §5 says is not covered.
- **CI runs somewhere without a reachable Postgres.** Then the decision in the
  refusal table above is re-opened, and Testcontainers is the candidate.
- **Provisioning stops being compatible with an acceptably fast suite.** Two
  observable thresholds rather than a timing figure, because a figure needs a
  baseline nobody will have: **the suite exceeding roughly two minutes**, or
  **the migration chain roughly doubling** — the cost is linear in migration
  count, so ~170 migrations puts provisioning near 4 s per assembly.
  <br>Had end-to-end provisioning proved incompatible with an acceptably fast
  suite, a cached template database would have been reconsidered **despite** its
  invalidation problem, and the invalidation key would then have been the design
  work rather than an afterthought.
