# EPIC-023 — The test suite runs against its own schema

**Status:** 🔵 In progress · **Branch:** `epic/EPIC-023-test-schema` (cut at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

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
| **86 migrations apply to an empty database in 0.165 s** | raw SQL, measured against the local Postgres |
| `CREATE DATABASE … TEMPLATE` | **0.15 s** — indistinguishable |
| the dev database holds **17,400 `Sessions`** and **810 `RefreshTokens`** | test residue, accumulating in a working database since EPIC-006 |

**The 0.165 s is what chose the design.** The first measurement said 14 seconds,
which would have made a per-assembly database a real cost and a template
database a real answer — but 14 seconds is what `dotnet ef` spends on *build and
startup*, and almost none of it is migration. Once the chain itself is a sixth of
a second, template databases buy nothing, snapshots buy nothing, and the naive
implementation is affordable. **The epic builds the naive implementation because
the measurement says it can, not because it is simpler.**

> This is the second time in three epics that measuring first overturned a plan
> already written down — EPIC-022 D6 was the first. Both were caught before code,
> and in both cases the wrong version had been argued convincingly.

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

### S001 — one assembly proves the shape

`RegOS.TestSupport`, plus **one** converted assembly:
`RegOS.ProductDocument.Persistence.Tests`, chosen because it is a single file.

The point is to **measure the real per-assembly cost before six more assemblies
depend on the shape**, and to meet the sharp edges once rather than seven times:

- connection pools must be cleared before `DROP`, or the drop blocks on EF's own
  idle connections
- Postgres identifiers are capped at 63 characters, and
  `regos_test_regos_productdocument_persistence_tests_<guid>` is not
- the server connection must come from one place, so CI can point elsewhere
  without touching seven files

**Sign-off gate:** the measured cost, before S002 begins.

### S002 — the remaining six

Mechanical once S001's shape is proved. The one with substance is
`RegOS.Api.Tests`, where `RegOSApiFactory` hosts the real API in-process and must
be pointed at the per-assembly database rather than at configuration's.

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

Delivers: both fixes, D8's two guards, and the idempotent script run end to end
against an empty database.

### S004 — capstone

Demonstrates that a stale schema can no longer produce a green run, and states
plainly **what is still not proved** — every backfill, and every upgrade of a
populated database — with the trigger that would change it.

Also writes the isolated-stack recipe into testing.md (D7).

---

## Change History

| Date | Change |
|---|---|
| 2026-08-05 | Raised in BACKLOG.md at EPIC-010c's close, after a third independent observation |
| 2026-08-05 | Phase 1 + Phase 2 signed off. Approach changed by measurement; two backlog claims corrected |
