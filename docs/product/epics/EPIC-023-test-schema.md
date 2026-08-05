# EPIC-023 — The test suite runs against its own schema

**Status:** 🟢 Complete · **Branch:** `epic/EPIC-023-test-schema` (cut at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

The first epic in RegOS that ships **no user-facing capability and no RIM
object**. It exists because the suite does not test the assumption every one of
its integration tests depends on: that the schema in the database matches the
migrations in source control.

> **Phase 1 was signed off on 2026-08-05**, with two wording corrections the
> founder made and one measurement that overturned the approach the backlog had
> written down. Both are kept below rather than smoothed away.

---

## Phase 1 — Epic plan

### Outcome

> **The requirement, stated once:** *the automated test environment should
> always execute against a schema produced from the current migration chain.*

Per-assembly databases are the implementation. **The requirement is what
matters**, and it is what a future reader should hold this against.

### The finding this responds to

**27 test files hard-code `Database=regos`** — the developer's own working
database. Nothing migrates it, so the suite silently assumes somebody already
did.

Three observations, all inside EPIC-022, and the third is the one that settles
it:

| | What it showed |
|---|---|
| **S001** | the seeder is insert-if-empty, so a migration must carry its own backfill — and today those backfills are only ever proved by hand |
| **S002** | the database had drifted **five migrations** behind and **18 of 19 suites went red** |
| **S004** | the database was **one migration behind and the suite stayed green** |

The first two look like an operational lapse. The third is the actual defect:
**a stale schema only turns a test red when a migration happens to touch a read
path some test already exercises.** EPIC-010b added three tables and stayed
green throughout, because its new tests were domain tests and its new tables
were read by nothing older; S004 added two more and did the same. The first
change to an *existing* read path — `ListRegistrationMarkets` reaching
`CountryRegions` — went red immediately.

> **Green means "nothing collided", not "the schema is current".**

### What was measured before anything was designed

| | |
|---|---|
| **27 files** hard-code the dev database | across **7 of 19 suites** — Api, Organization·Application, Platform·Application, Product·Application, ProductDocument·Persistence, Registration·Application, Submission·Application. The other twelve are domain and architecture suites and never open a connection |
| **85 migrations apply to an empty database in 0.165 s** | raw SQL, measured against the local Postgres |
| `CREATE DATABASE … TEMPLATE` | **0.15 s** — indistinguishable |
| the dev database holds **17,400 `Sessions`** and **810 `RefreshTokens`** | test residue, accumulating in a working database since EPIC-006 |

**This measured the wrong layer, and S001 caught it.** 0.165 s is what *Postgres*
spends; end-to-end provisioning costs **2.7 s per assembly**, because EF's
migration machinery is twelve times the SQL it emits. The planning argument
below — *"the naive implementation is affordable"* — survived, but not for the
reason given here. See [S001](#s001--one-assembly-proves-the-shape) for the
numbers and [ADR-064](../../adr/ADR-064-the-test-suite-provisions-its-own-schema.md)
for the amended argument. **Kept rather than rewritten: the measurement that was
wrong is more instructive than the one that replaced it, because both looked
authoritative.**

> ~~The 0.165 s is what chose the design.~~ The first measurement said 14
> seconds, which would have made a per-assembly database a real cost and a
> template database a real answer — but 14 seconds is what `dotnet ef` spends on
> *build and startup*, and almost none of it is migration. Once the chain itself
> is a sixth of a second, template databases buy nothing, snapshots buy nothing,
> and the naive implementation is affordable.
>
> This is the second time in three epics that measuring first overturned a plan
> already written down — EPIC-022 D6 was the first.

**Three measurements, two of them wrong, and each looked like the answer.**
14 s was `dotnet ef`'s build; 0.165 s was Postgres alone; 2.7 s is what a test
run actually pays. The lesson worth carrying is not *measure first* — that was
done — it is **measure the layer the cost is actually paid at**.

### Two corrections to what the backlog claimed

#### 1. A per-run database does not prove a backfill

The backlog said a fresh database "exercises the migrations themselves —
including their backfills". **It does not, and the distinction is material.**

A backfill only fires on a database that already holds rows. On an empty one,
EPIC-022's country backfill matches zero rows, and the *seeder* supplies the
stability conditions instead —
[`Countries.cs`](../../../src/Persistence/RegOS.Persistence/Initialization/ReferenceData/Geography/Countries.cs)
already carries `30C_70RH` for India. The backfill runs, updates nothing, and
reports success.

So the honest claim this epic can make is narrower than the one it replaces:

| Proved by this epic | Not proved by this epic |
|---|---|
| the schema is created from the current chain | that any migration's **backfill** does what it says |
| the **seeder** runs to completion against that schema | that a **populated** database survives an upgrade |

**Proving migrations both ways stays manual**, as EPIC-010c established. The
trigger that would change it is named rather than left open: **the first
customer database.** Until one exists, every populated database in the world is
a dev seed or a throwaway, and hand-proving an upgrade costs minutes; after one
exists, it cannot be hand-proved at all.

> Written down so nobody later reads "the test suite runs the migrations" and
> concludes the upgrade path is covered. It is not.

#### 2. "The production deployment path" was stronger than the evidence

An earlier draft of this plan called the idempotent script *the production
deployment path*. RegOS has no deployment process yet, so that claimed more than
was shown. What was actually shown is exact and enough:

> **The supported idempotent deployment artifact — what
> `dotnet ef migrations script --idempotent` generates — did not run.**

See [S003](#s003--the-migration-chain-is-proved-not-assumed).

### In scope ✅

- A database per test assembly, created, migrated, seeded and dropped by the
  tests themselves
- One shared `RegOS.TestSupport` project that owns that lifecycle
- The two unterminated raw-SQL statements that break the idempotent script
- Two architecture guards, so neither defect class can return
- ADR-064, and the testing.md standards that follow from it

### Out of scope ⏸️ (deferred, with reason)

| | Why | Where it goes |
|---|---|---|
| **A CI job** | `.github/workflows` does not exist. This epic makes the suite *worth* running in CI, which is the prerequisite, not the same thing | EPIC-015, which already carries the clean-clone check |
| **Deleting the 27 hand-written cleanup routines** | See [D5](#d5--existing-cleanup-remains-intentionally) — they are not redundant | a separate refactor, once redundancy is *demonstrated* |
| **Upgrade-path testing against populated databases** | The trigger is the first customer database, and it does not exist | named above, unscheduled by design |
| **The browser suite's database** | It runs against a running stack, not `dotnet test`. The isolated-stack recipe gets written down; the suite is not changed | [S004](#s004--capstone) documents it |

### Definition of Done

1. No file under `tests/` names the developer's database, and an architecture
   test says so
2. Every one of the seven database-touching assemblies creates, migrates, seeds
   and drops its own database
3. `dotnet ef migrations script --idempotent` produces a script that applies
   cleanly to an empty database, and an architecture test prevents the defect
   class returning
4. `dotnet test RegOS.slnx` is green across **19 reporting suites** with no
   manual migration step first
5. The epic doc states plainly what is still not proved

### It closes no RIM object

Recorded deliberately, because every other epic's table is the first thing read.
EPIC-023 is one of two hardening epics raised at EPIC-010's close — the other is
[EPIC-024](../BACKLOG.md#epic-024--the-invariants-nothing-checks) — and the
split between them is by subject: **this one is about the environment a test
runs in; that one is about rules the ADRs state that no test reads.**

---

## Phase 2 — The eight decisions *(approved 2026-08-05)*

### D1 — The unit is the assembly, not the run

`dotnet test` gives each test project its own process, and nothing coordinates
them. **"Per run" has no implementation**; "per assembly" does. Seven databases
per run, named `regos_test_<assembly>_<short-guid>`.

Stated explicitly because the requirement sentence says *run* and the code will
say *assembly*, and a future reader is entitled to know that is deliberate.

### D2 — The tests create it themselves, on the Postgres already running

```
CREATE DATABASE  →  MigrateAsync()  →  the real initializer chain  →  DROP … (FORCE)
```

**Not Testcontainers.** It would add a Docker dependency to every `dotnet test`
and a package to a repository that has fifteen, and the requirement is *schema
currency*, not *host isolation*.

> **The falsifier:** a CI runner with no Postgres available. Even then
> Testcontainers is not automatic — GitHub Actions' `services: postgres` answers
> the same need without changing how the tests are written.

### D3 — The seed runs through the real `IDataInitializer` chain

Not a test-local fixture that inserts the rows it happens to need. This is the
**second thing worth proving** the backlog named, and it is free: every
initializer depends on `RegOSDbContext` alone, so a `ServiceCollection` calling
`AddPersistence` reproduces exactly what the API does at boot.

It also means the ordering constraint that broke EPIC-010c — `SiteInitializer`
must run after `IdentifierSchemeDataInitializer` — is exercised by every test run
from now on, rather than by whoever next boots an empty database.

### D4 — One new project, `tests/TestSupport/RegOS.TestSupport`

Seven demonstrated needs on the day it is written, so [ADR-018](../../adr/ADR-018-rule-of-three.md)'s
rule of three is satisfied rather than bent — this is not symmetry with another
module, it is seven call sites that exist.

Recorded as a standard in [testing.md](../../engineering/testing.md), not as an
ADR of its own: a test-support project is not a bounded context, a cross-context
dependency, or a change to an accepted decision.

### D5 — Existing cleanup remains intentionally

**Per-assembly isolation replaces cross-run isolation, not intra-assembly
isolation.** Removing the cleanup is a separate refactoring, once redundancy is
demonstrated.

The two mechanisms solve orthogonal problems, and the epic would be wrong to
treat one as a replacement for the other:

| The database lifecycle guarantees | The cleanup routines guarantee |
|---|---|
| the schema starts current | test A does not leak state into test B |
| seed data starts deterministic | order independence within an assembly |
| no drift between migrations and tests | tests remain runnable individually |

Tests in one assembly still share one database, so the `DisposeAsync` cascades
are still doing the job [ADR-019](../../adr/ADR-019-testing-strategy.md) rule 1
gave them. Deleting 27 of them here would also mix mechanical cleanup with
architectural change in a single diff — which is the thing this repository has
consistently refused to do.

### D6 — ADR-064, written before S001

[ADR-019](../../adr/ADR-019-testing-strategy.md)'s four rules stand untouched;
this epic strengthens rule 1 rather than replacing it. But ADR-019's
**Consequences** record that the suite *"can run repeatedly against a shared
development database"*, and that stops being how RegOS works.

ADR-064 states the new decision and names the line it overtakes. **ADR-019 is not
edited** — an accepted ADR never is.

### D7 — The browser suite is out of scope, except for what is written down

It runs against a running stack by design ([ADR-019](../../adr/ADR-019-testing-strategy.md)
§Consequences: *"verification, not CI"*). Nothing about it changes here.

What does change: the **isolated-stack recipe** — throwaway database, API on
5301, web on 5174, `REGOS_WEB_URL`/`REGOS_API_URL`, and the CORS widening that
must be reverted surgically — currently exists only in conversation. S004 writes
it into testing.md, because knowledge held only in a session is knowledge the
project does not have.

### D8 — Two guards, so neither defect class can return

| Guard | Catches |
|---|---|
| no file under `tests/` contains `Database=regos` | the 27 files coming back one at a time |
| every `migrationBuilder.Sql(…)` argument ends in `;` | the defect S003 found |

**Named for the class, not the rule.** The second guard's home is a test about
*raw SQL in migrations*, so that when a second rule is demonstrated — idempotency,
or a construct that must not appear — it has an obvious place to go and does not
arrive as a new file. No such second rule is written today: one demonstrated need
is one, and [ADR-018](../../adr/ADR-018-rule-of-three.md) forbids the speculative
version as firmly as it forbids the speculative deletion.

---

## Phase 3 — Stories

### S001 — one assembly proves the shape ✅

`RegOS.TestSupport`, plus **one** converted assembly:
`RegOS.ProductDocument.Persistence.Tests`, chosen because it is a single file.

Its purpose was to **measure the real per-assembly cost before six more
assemblies depend on the shape**, and to meet the sharp edges once rather than
seven times.

#### What it cost

Median of three runs, decomposed:

| | | |
|---|---|---|
| `CREATE DATABASE` | 79 ms | |
| EF model build | 851 ms | *excluded — every database-touching suite already pays it* |
| **`MigrateAsync()`** | **1,985 ms** | the surprise |
| seed, via the real initializer chain | 611 ms | |
| `DROP … (FORCE)` | 25 ms | |
| **marginal cost per assembly** | **≈ 2.7 s** | ≈ **19 s** across seven, against a **28 s** suite |

**EF's migration machinery costs twelve times the SQL it emits** — 1,985 ms to
produce 165 ms of statements — because it instantiates all 85 `Migration`
classes and regenerates every operation. **The cost is linear in migration
count, not in schema size**, which is why *Revisit when* names a doubling of the
chain rather than a growth in tables.

The cost was accepted and ADR-064 amended in place. The template database that
would have removed it is refused **on correctness**: it is a cache, and
[S003](#s003--the-migration-chain-is-proved-not-assumed) is about to edit two
existing migrations without adding any — which no invalidation key derived from
migration identity can see.

#### The sharp edges it found

| | |
|---|---|
| **`RegOS.TestSupport` ran as a test suite** | It lives under `tests/` and references xunit, so `dotnet test` on the solution launched a testhost for it that died with a bare `Error:`. `<IsTestProject>false</IsTestProject>`. **The run must report 19, not 20** |
| **`AddPersistence` needed a connection-string overload** | The fixture knows its connection string before any `IConfiguration` exists. The alternative was a second initializer registration list in test code — which would silently stop running whichever initializer someone added next. Two lines of production change; the list stays in one place |
| Connection pools must be cleared before `DROP` | Npgsql keeps them open after the last context is disposed, and `DROP` blocks on them |
| Postgres identifiers are capped at 63 characters | and truncated **silently** past it, so two assemblies sharing a long prefix would collide on a name neither chose |
| The server connection comes from one place | `TestPostgres`, overridable by `REGOS_TEST_POSTGRES`, so CI points elsewhere without touching a test file |

### S002 — the remaining six ✅

Mechanical once S001's shape is proved. The one with substance was
`RegOS.Api.Tests`, where `RegOSApiFactory` hosts the real API in-process and is
now pointed at the per-assembly database with `UseSetting`.

#### The measurement it existed to take

| Suite | Before | After |
|---|---|---|
| Organization·Application | 1.99 s | 5.62 s |
| Platform·Application | 3.05 s | 7.76 s |
| Product·Application | 2.20 s | 5.50 s |
| Registration·Application | 2.42 s | 5.16 s |
| **Submission·Application** | **3.68 s** | **8.84 s** |
| Api | 12.01 s | 14.04 s |
| **Full suite** | **28 s** | **39.5 s** *(+41%)* |

Submission was the assembly most likely to invalidate the design — 182 tests
across 11 classes, now sharing one database and therefore no longer running in
parallel with each other. Its +5.1 s is roughly **2.7 s provisioning and 2.4 s
serialisation**.

**No per-assembly exception was added, and the reason is arithmetic.** A
database per class would provision eleven of them at 2.7 s each — **30 s spent
to recover 2.4 s**. The exception would be worse than the rule.

> **The serialisation is an artifact of the collection fixture, not of sharing a
> database.** These classes already shared one database while running in
> parallel. Three things are separable — the database's *lifecycle*, its
> *sharing*, and the runner's *scheduling* — and this epic changed only the
> first. An **assembly-level fixture** keeps classes parallel on one database;
> that is `IAssemblyFixture`, built into xUnit **v3** and needing a package or a
> custom test framework in v2. **Recorded as the optimisation target**, so that
> future effort goes at runner parallelism rather than at cached databases.

#### The proof that the epic was worth running

```
Sessions before a full run = 17716
Sessions after  a full run = 17716
```

**Running the test suite no longer mutates the developer's working
environment.** It had been accumulating residue since EPIC-006 and held 17,400
sessions and 810 refresh tokens on the day this epic started.

#### What the Api suite cost that the other five did not

**xUnit 2 will not inject one collection fixture into another** — a fixture
constructor takes `IMessageSink` and nothing else. Declaring `ApiDatabase` and
`RegOSApiFactory` as sibling fixtures failed all 84 tests in 17 ms with
*"unresolved constructor arguments"*, before one of them ran. The factory owns
the database instead, and `RegOSTestDatabase.DisposeAsync` became idempotent
because a `WebApplicationFactory` is reachable through both `IAsyncLifetime` and
`IAsyncDisposable`.

### S003 — the migration chain is proved, not assumed

**An enabling defect, discovered while proving the chain, and fixed here because
the story cannot honestly complete without it.** S003's objective is that the
migration chain is proved; one of the two supported ways of executing it fails,
so proving only the other would be proving half.

```sql
DELETE FROM "UserCredentials"
WHERE "UserId" NOT IN (SELECT "Id" FROM "Users")   -- ← no ';'
END IF;
```

`Migrate()` sends each migration command separately, so this has never mattered
on the path anybody runs. `--idempotent` wraps commands in `DO $EF$ … END $EF$`
and concatenates them, so a missing terminator is a syntax error. Two occurrences,
in [`LinkUserCredentialToUser`](../../../src/Persistence/RegOS.Persistence/Migrations/20260721083510_LinkUserCredentialToUser.cs)
and [`AddSessions`](../../../src/Persistence/RegOS.Persistence/Migrations/20260721135948_AddSessions.cs);
with both patched the whole chain applies clean.

**Editing a shipped migration is safe here and the reason is narrow:** a
semicolon changes the generated *script*, not the schema any database already
has. The `Up` still produces the same commands.

> **The same defect class as EPIC-015's clean-clone bullet** — a facility nobody
> runs, so nothing says it is broken. It belongs in this epic and not in
> EPIC-015: EPIC-015 is about CI infrastructure, and this is about the
> correctness of the migration chain.

#### What it delivered ✅

**Both statements terminated**, and the script verified twice over — because
applying is not the property the artifact is named for:

| | |
|---|---|
| First run against an empty database | applied clean · **85 migrations recorded · 91 tables** |
| **Second run against the same database** | **no-op** · still 85 migrations, still 91 tables |

**D8's two guards, and the second one is stronger than D8 asked for.** The plan
said *"no file under `tests/` contains `Database=regos`"*. That rule is satisfied
by writing `Database=regos_scratch`, which reintroduces the defect under a new
name — so the guard asserts instead that **exactly one file under `tests/`
carries a connection string at all**, and names it.

It found a file `grep "Database=regos"` never would have. Two architecture tests
inside `Platform.Application.Tests` built a `DbContext` on the fictional
`Host=localhost;Database=model-only` to inspect the model without connecting.
**`UseNpgsql()` takes no connection string for exactly that case**, so both lost
the fiction rather than earning an exemption — the guard's first catch improved
two files instead of acquiring a list.

> **What is proved and what is guarded are different, and the gap is named.**
> The two runs above are a **hand verification**, recorded here with their
> numbers. What runs on every build is narrower: `MigrationRawSqlTests` catches
> the *defect class* found — an unterminated hand-written statement — not "the
> script works". Automating the full script run means invoking `dotnet ef` from
> a test, and that buys less than it costs while the guard covers the only
> failure anyone has seen.

### S004 — capstone ✅

[`SchemaCurrencyTests`](../../../tests/ProductDocument/RegOS.ProductDocument.Persistence.Tests/SchemaCurrencyTests.cs)
asserts the three things the epic set out to make true, in the assembly S001
converted first:

| | |
|---|---|
| `GetPendingMigrationsAsync()` is empty, and every migration in source control is recorded as applied | **the assertion that would have caught it** — against the developer's database on 2026-08-04 it fails naming five missing migrations, and on 2026-08-05 naming one, *on a day the whole suite was green* |
| the database name starts with `regos_test_` and is not `regos` | the assertion above would also pass against a hand-maintained database somebody had just migrated. That is the state the project was in for a year, and the one that kept quietly reverting |
| countries, document types **and sites** are all present | the seed ran to completion, not as far as its first failure — sites are seeded last and only reachable if `IdentifierSchemeDataInitializer` ran before them |

**And the guarantee is structural, not local to one assembly.**
`RegOSTestDatabase` refuses to hand back a database whose schema did not come
from the chain, so all seven carry it. The capstone tests are a *readable*
statement of what the fixture enforces.

#### Proved by making the mistake

The near neighbour of `MigrateAsync()` is **`EnsureCreatedAsync()`**, which
builds the schema from the *model* rather than the migrations, leaves
`__EFMigrationsHistory` empty, and is **faster** — so somebody will propose it.
Swapping one for the other turns the suite red immediately, and says why:

```
System.InvalidOperationException : regos_test_productdocument_persistence_4d7d596b
has no migration history. The schema did not come from the migration chain —
EnsureCreated() builds it from the model instead, and a suite running on that
proves nothing about the migrations (ADR-064).
```

> **The demonstration also answered a question nobody had asked**: a fixture
> that throws during `InitializeAsync` still gets disposed, so the failed run
> left no database behind. Checked rather than assumed, because the alternative
> is a developer collecting orphaned databases every time a migration is broken.

Standards [8 and 9](../../engineering/testing.md) were added in the same story —
the second being the isolated browser-stack recipe (D7), which had lived only in
conversation.

---

## After the capstone — the host boots from nothing

**Outside this epic's requirement, and recorded here rather than folded into it**,
because it shipped on this branch and the epic's boundary should stay legible.
The requirement is about the *automated test environment*; this is about the
*application*. They share a subject and are not the same scope.

Pointing the connection string at a database that did not exist used to fail in
the first initializer with `42P01 relation "Countries" does not exist` — forty
frames deep, and saying nothing about what to do. **One setting now decides it:**

```jsonc
// appsettings.json — the safe default, stated rather than implied
"Database": { "MigrateOnStartup": false }

// appsettings.Development.json
"Database": { "MigrateOnStartup": true }
```

| | |
|---|---|
| **`true`** | the host creates the database if absent, migrates it, then seeds — **0 tables → 91 tables, 85 migrations, seeded reference data**, from `dotnet run` alone |
| **`false`**, including when the key is absent | the host stops, **naming the pending migrations** and what to do about them, and never alters the schema itself |

**Configuration rather than `IsDevelopment()`**, because *"may this process alter
the schema?"* is a property of the deployment, not of the word it was labelled
with — a staging box that owns its database and a production one that does not
are both `Production`. False when absent, so forgetting the setting is the safe
outcome.

> **This changes nothing about the tests**, which provision their own database
> and would be unaffected either way. Recorded in
> [ADR-064](../../adr/ADR-064-the-test-suite-provisions-its-own-schema.md)'s
> addendum anyway, because *"the app migrates at startup"* is exactly the fact a
> future reader would use to argue the tests no longer need to.

It also made [Standard 9](../../engineering/testing.md) — written the same day —
wrong, and it was corrected in the same change. The isolated browser stack no
longer needs a separate `dotnet ef database update` step.

---

## Retrospective

### Measured outcomes

| Measure | Before | After |
|---|---|---|
| **Schema source** | a shared, long-lived database | **fresh migrations, every test assembly** |
| **Writes to the developer's database** | yes — 17,400 sessions and 810 refresh tokens of residue | **none.** 17,716 before a full run, 17,716 after |
| **Test schema drift** | possible, and silent | **eliminated** — there is no window in which a schema can be behind |
| **Idempotent migration script** | **broken**, and never executed by anyone | applied clean, **and verified idempotent** on a second run |
| **Architecture guards** | none on either defect class | raw SQL in migrations · one connection string under `tests/` |
| **Files naming a database** | 27 | **1**, and it names a server |
| **Full suite duration** | 28 s | **39.5 s** *(+41%)* |

The last row is what the others cost. It is stated in the same table
deliberately: **+11.5 s is the price**, and everything above it is what was
bought.

### The lessons worth carrying past this epic

#### 1. Measure the layer the cost is actually paid at

Three measurements, **two of them wrong, and each looked authoritative**: 14 s
was `dotnet ef`'s build; 0.165 s was Postgres alone; **2.7 s** is what a test
assembly actually pays. The house habit was already *measure first* — it was
followed, and it still produced two wrong numbers in a row before the right one.

#### 2. An optimisation can cost more than the time it saves

ADR-064 first refused the template database because provisioning looked cheap.
It is not cheap. The refusal survived anyway, on a **stronger** argument: a
template is a cache, a cache needs an invalidation key, and **no key derived
from migration identity survives an edit to an existing migration** — which
[S003](#s003--the-migration-chain-is-proved-not-assumed) then made, twice,
adding none. The optimisation would have reintroduced the drift the epic exists
to remove.

*"It wasn't worth it"* is a temporary argument. *"It requires solving a problem
we have a concrete counter-example to"* is a durable one.

#### 3. Ban the shape, not the value

D8 asked for a guard that no test file contains `Database=regos`. That rule is
satisfied by typing `Database=regos_scratch`. Phrased instead as **exactly one
file may carry a connection string**, it immediately found two files a `grep`
never would have — and because `UseNpgsql()` needs no connection string for
model-only work, both were *simplified* rather than exempted. **A guard that
describes the intended architecture drives simplification; a guard that bans a
value collects exemptions.**

#### 4. Separate what is proved from what is guarded

The idempotent script was proved **by hand**, twice, with its numbers recorded.
What runs on every build is narrower: the guard catches the *defect class*
found, not "the script works". Both facts are written down, because a reader who
assumes the stronger one will stop looking.

#### 5. Three things that look like one

The database's **lifecycle**, its **sharing**, and the runner's **scheduling**
are separable, and this epic changed only the first. Submission's classes lost
their parallelism to xUnit 2's collection-fixture model, not to sharing a
database — they already shared one. **The optimisation target is runner
parallelism** (`IAssemblyFixture`, built into xUnit v3), **not cached
databases.** Recorded so that future effort goes to the right place.

### What is still not proved

Stated as plainly as the outcomes, because *"the test suite runs the
migrations"* invites a conclusion this epic does not support:

| | Why not, and what would change it |
|---|---|
| **That any migration's backfill does what it says** | A backfill only fires on a database that already holds rows. On an empty one, EPIC-022's country backfill matches nothing and the *seeder* supplies the data instead — the migration runs, updates zero rows, reports success. **No arrangement of freshly-provisioned databases can exercise that path** |
| **That a populated database survives an upgrade** | Proving migrations *both ways* stays manual, as EPIC-010c established. **The trigger is the first customer database.** Until one exists, every populated database is a dev seed or a throwaway and an upgrade costs minutes to hand-prove; after one exists, it cannot be hand-proved at all |
| **That the idempotent script works, on every build** | Only the defect class is guarded. Automating it means invoking `dotnet ef` from a test — worth doing when a second failure appears that the guard does not catch, and not before |

### Definition of Done

| | | |
|---|---|---|
| 1 | No file under `tests/` names the developer's database, and an architecture test says so | ✅ 27 → 1, and the one names a server |
| 2 | All seven database-touching assemblies provision their own | ✅ |
| 3 | The idempotent script applies cleanly, and a guard prevents the defect class returning | ✅ applied twice — second run a no-op |
| 4 | `dotnet test RegOS.slnx` green across **19 reporting suites**, with no manual migration step | ✅ 19, and `IsTestProject=false` keeps `RegOS.TestSupport` from becoming a twentieth |
| 5 | The epic states plainly what is still not proved | ✅ above |

### Did it accomplish the goal? — three hedges, asked for at close

The Definition of Done is met line by line and none of its rows is generous.
The honest summary is narrower than the ticks suggest, and these are worth more
than the ticks:

#### 1. It removed the lie, not the silence

All three founding observations were *"somebody ran the suite and it told them
the wrong thing."* That is fixed. But **nothing runs the suite except a person
typing `dotnet test`** — no CI exists. A green run is now trustworthy; an absent
run is still invisible.

> **The epic made the suite worth running and did not make it run.** Until
> [EPIC-015](../BACKLOG.md#later)'s CI job, the value sits latent — which makes
> that job worth more than it looked the day before this shipped.

#### 2. "The automated test environment" is broader than what was covered

The browser suite still points at whatever stack is brought up by hand.
[D7](#d7--the-browser-suite-is-out-of-scope-except-for-what-is-written-down)
scoped that out with a reason that still holds — it runs against a stack, not
`dotnet test` — but **Standard 9 is a recipe, not a guarantee.** The requirement
sentence does not carve the browser suite out; this epic did.

#### 3. The guard proves less than its green tick implies

`TestDatabaseConventionTests` proves **no file names a database**. It does not
prove **every database test uses the fixture**. A new assembly could build a
context from `TestPostgres.Server` directly and pass, because that connection
string lives in the one permitted file.

> **The stronger guard is cheap and was not built:** *any test project
> referencing `RegOS.Persistence` must also reference `RegOS.TestSupport`* — a
> `.csproj` scan of perhaps fifteen lines. Named as an
> [EPIC-024](../BACKLOG.md#epic-024--the-invariants-nothing-checks) candidate,
> because it is the same shape as the two already there: **a rule the project
> relies on that nothing executes.**

#### What it would have done differently

**Measured the layer the cost is paid at before writing ADR-064, not after.**
The ADR shipped with a wrong premise and had to be amended at S001 — and it was
amendable only because it had not merged. That was timing, not process.

### It closed no RIM object, as forecast

The first epic in RegOS to ship no user-facing capability. What it shipped
instead is that **every guarantee the other 22 epics' tests make is now made
against a schema the project can name the origin of** — and that the project
can say, precisely, which guarantees it still cannot make.

---

## Change History

| Date | Change |
|---|---|
| 2026-08-05 | Raised in BACKLOG.md at EPIC-010c's close, after a third independent observation |
| 2026-08-05 | Phase 1 + Phase 2 signed off. Approach changed by measurement; two backlog claims corrected |
| 2026-08-05 | S001. ADR-064 **amended in place** — provisioning is not cheap, and the template database is refused on correctness instead |
| 2026-08-05 | S002. All seven assemblies; the serialisation cost measured rather than predicted, and no per-assembly exception added |
| 2026-08-05 | S003. Two unterminated statements fixed; the idempotent artifact executed for the first time |
| 2026-08-05 | S004. Capstone, retrospective, testing.md Standards 8 and 9 |
| 2026-08-05 | After the capstone: `Database:MigrateOnStartup`, outside the epic's requirement and recorded as such. Standard 9 corrected the same day it was written |
| 2026-08-05 | Closed. Three hedges recorded against the Definition of Done, one of which — *no CI runs this* — is the reason the value is latent |
