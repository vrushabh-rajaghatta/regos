# Testing

---
**Title:** Testing

**Owner:** Architecture Review Board

**Status:** Approved

**Version:** 1.1

**Effective Date:** 2026-07-08

**Last Reviewed:** 2026-08-01

**Next Review:** 2027-07-08

**Related Documents:**
- philosophy.md
- business-modeling.md
- implementation-standards.md
- engineering-workflow.md

**Related ADRs:**
- None

---

# Purpose

The purpose of testing in RegOS is to increase confidence in business behavior.

Tests exist to verify that the platform correctly models the regulatory domain, protects business invariants, produces explainable decisions, and remains trustworthy as it evolves.

Code coverage is a metric.

Business confidence is the objective.

---

# Testing Principles

## Principle 1 — Test Business Behavior

Tests should verify business outcomes rather than implementation details.

Refactoring should not require rewriting tests unless business behavior changes.

---

## Principle 2 — Protect Business Invariants

Every business invariant should be protected by automated tests.

If violating an invariant would create an invalid regulatory state, it must be tested.

---

## Principle 3 — Business Rules Must Be Independently Verifiable

Business Rules should be testable without APIs, databases, or user interfaces.

Rule evaluation should remain deterministic and repeatable.

---

## Principle 4 — Explainability Is Testable

If the platform produces a regulatory decision, the reasoning behind that decision must also be verifiable.

Testing should confirm both the decision and its explanation.

---

## Principle 5 — Prefer Deterministic Tests

Tests should produce identical results given identical inputs.

Time, randomness, and external dependencies should be controlled through abstractions.

---

## Principle 6 — Architecture Is Also Tested

Architecture is part of the product.

Dependency rules, project boundaries, and implementation standards should be verified through automated architecture tests.

---

## Principle 7 — A Test Owns Every Entity It Mutates

A browser spec must create the business entities it acts on, capture their
identifiers, and operate on those identifiers only.

```
seed via the API  →  capture the id  →  operate on that id  →  retire it
```

Never select a subject from ambient data:

```ts
// Wrong — acts on whatever the database happens to contain
const target = organizations.find((o) => o.status === "Active");

// Right — the spec owns what it touches
const id = await seedOrganization(unique("Browser Edit Org"));
```

This is stricter than "clean up after yourself", and it exists because a
violation is silent. A spec that reads ambient data passes for months and then
mutates seeded data that another spec depends on. RegOS lost a seeded
organization to exactly this — it was found by inspection, not by a failing
test.

Two safeguards back the rule, and they do different jobs:

- **`OrganizationInitializer` reconciles demo data on startup.** Developer
  convenience: experiment locally, restart, get a known state back. It updates
  only rows that already exist, so it never pushes demo data into a database
  holding real records.
- **`seed-integrity.spec.ts` is a canary.** It asserts the seeded organizations
  are present, unmodified and Active. If it fails, a spec mutated something it
  did not create. Fix the spec — never relax the assertion.

Related: [ADR-019](../adr/ADR-019-testing-strategy.md) rule 1, which this
principle strengthens.

---

## Principle 8 — Both Halves Of A Capability Are Reachable

For every new business capability, ask two questions:

1. **Can the user perform the action?**
2. **Can the user then observe the business fact that action created?**

A capability is not shipped until both are true. The question is symmetric on
purpose, because RegOS has now shipped each half without the other:

| | Missing half | Found |
|---|---|---|
| Milestone 1 | `Activate`/`Deactivate` existed on the aggregate; no endpoint reached them | EPIC-016, by inspection |
| EPIC-016 S003 | Organization identity was modelled and unreachable | during the same epic |
| EPIC-017 S003 | Market status history was **written on every change and readable nowhere** | EPIC-017 S004, when the row ran out of room |

The third is the instructive one. Nothing was broken: the write worked, the
data was correct, the tests passed. But the whole reason that history stores
`OccurredOn` *and* `RecordedOnUtc` is so a reader can tell a backdated entry
from a late one — and with no reader, the second timestamp was pure cost.

**An unobservable fact is indistinguishable from a fact that was never
recorded.** Write the observation surface in the same story, or say in the epic
which story owns it.

> Applies to reviews as much as to tests: *can perform → can observe* is short
> enough to run against every story in your head.

---

## Principle 9 — A Reloaded Aggregate Still Enforces Its Rules

Domain tests build an aggregate in memory, where **every collection is
populated by construction**. That is exactly the condition a repository can
fail to reproduce, so an entire class of defect is structurally invisible to
them:

> **A persisted aggregate, reloaded, must be able to enforce every rule its
> in-memory version enforces.**

EPIC-004 S005 shipped `SubmissionRepository` including `Documents` but not
`Roles`. Eleven domain tests passed. Removing a naming returned a silent 404,
and the duplicate check was vacuously true — the unique index refused what the
domain should have (see
[implementation-standards.md](implementation-standards.md)).

**Neither existing layer could see it.** The domain tests were correct and the
browser test was correct; the missing class was between them:

```csharp
var submission = await repository.GetByIdAsync(id, default);

submission!.Roles.Should().ContainSingle(
    "the repository must load the collection the aggregate reasons over");

var act = () => submission.AssignRole(person, sameRole);
act.Should().Throw<BusinessRuleViolationException>();   // the domain, not 23505
```

Not many of these. **One per aggregate rule that reads a collection** — enough
to prove the round trip preserves what the rules depend on.

> The tell that you need one: an aggregate rule whose body mentions a private
> collection. If a repository could omit it, a test should say so.

---

# Confidence Levels

RegOS organizes testing by confidence rather than by framework.

## Level 1 — Model Confidence

Verifies that Aggregates, Entities, Value Objects, Specifications, and Domain Services correctly represent the regulatory domain.

Typical examples include:

- Aggregate invariants
- Value Object validation
- Specification evaluation

---

## Level 2 — Capability Confidence

Verifies complete business capabilities.

Examples include:

- Register Product
- Release Product Version
- Assess Submission Evidence

A capability succeeds only when the intended business outcome is achieved.

---

## Level 3 — Decision Confidence

Verifies that identical facts always produce identical regulatory decisions.

Decision testing should validate:

- Facts
- Rules
- Evidence
- Decision outcome
- Decision explanation

---

## Level 4 — Integration Confidence

Verifies communication with external systems.

Examples include:

- Regulatory authority APIs
- Identity providers
- Messaging infrastructure
- File storage

Business behavior should remain isolated from integration failures whenever possible.

---

## Level 5 — System Confidence

Verifies complete end-to-end regulatory workflows.

Examples include:

- Product registration
- Submission preparation
- Regulatory assessment
- Decision publication

These tests provide confidence that the platform functions correctly as an integrated system.

---

# Testing Standards

## Standard 1

Every Capability must have business-focused automated tests.

---

## Standard 2

Every Aggregate must have invariant tests.

---

## Standard 3

Every Business Rule must have deterministic tests.

---

## Standard 4

Every Domain Event should be verified through business behavior rather than implementation details.

---

## Standard 5

External systems should be replaceable during testing.

---

## Standard 6

Tests should improve confidence, not inflate coverage metrics.

Coverage percentages should never become the primary engineering objective.

---

## Standard 7

**Browser specs default to `{ exact: true }` on `getByLabel()`**, unless
substring matching is the explicit intent.

*Adopted 2026-08-05, at the second occurrence of the same defect.*

Playwright's `getByLabel()` matches **substrings**, and matches them
**case-insensitively**. That is a reasonable default for a library and a trap in
a domain whose labels legitimately share words:

| Spec | Meant | Also matched |
|---|---|---|
| EPIC-010b, appearance | `"White"` | *"Off-white"* |
| EPIC-010c S002, approvals | `"Licence"` | *"Added to the **l**icence on"* |

**Both failures look like a product bug and are not one.** The first selected a
colour nobody chose; the second resolved to two elements and failed strict mode.
Neither says anything about the application, and both cost a debugging round.

The rule is deliberately about the **default**, not a prohibition. Substring
matching is right when a label is genuinely a prefix of what is rendered — say
so at the call site when you rely on it, so a reader can tell the difference
between an intention and an oversight.

---

## Standard 8

**A `dotnet test` suite never names a database. It takes one.**

*Adopted 2026-08-05, EPIC-023 · [ADR-064](../adr/ADR-064-the-test-suite-provisions-its-own-schema.md)*

Every database-touching test assembly provisions its own — created, migrated
from the current chain, seeded by the real `IDataInitializer` chain, and dropped
when the assembly finishes.

```csharp
// One per assembly, and the subclass is what names the database after it.
public sealed class SubmissionDatabase : RegOSTestDatabase
{
    public const string Collection = "Submission database";
}

[CollectionDefinition(SubmissionDatabase.Collection)]
public sealed class SubmissionDatabaseCollection
    : ICollectionFixture<SubmissionDatabase>;

// Then, on each class that needs it:
[Collection(SubmissionDatabase.Collection)]
public sealed class MyTests(SubmissionDatabase database)
{
    private RegOSDbContext New() => database.NewContext(TestTenant.Context);
}
```

**Exactly one file under `tests/` may carry a connection string** —
[`TestPostgres.cs`](../../tests/TestSupport/RegOS.TestSupport/TestPostgres.cs),
which names a *server* and never a RegOS database. Override
`REGOS_TEST_POSTGRES` to point somewhere else. `TestDatabaseConventionTests`
enforces this, and the rule is phrased as *"one file"* rather than *"not that
database"* on purpose: a ban on one value is satisfied by inventing another.

Three things that look interchangeable and are not:

| | |
|---|---|
| `MigrateAsync()` ✅ | builds the schema **from the migration chain** |
| `EnsureCreatedAsync()` ❌ | builds it from the **model**, leaves `__EFMigrationsHistory` empty, and is faster — so somebody will propose it. A suite running on it is green while proving nothing about the migrations |
| a hand-maintained database | what this replaced. Correct exactly as often as somebody remembers |

**The existing per-test cleanup stays.** Per-assembly provisioning replaces
*cross-run* isolation, not *intra-assembly* isolation — classes in one assembly
still share one database, so Principle 7 and [ADR-019](../adr/ADR-019-testing-strategy.md)
rule 1 are as load-bearing as they ever were.

---

## Standard 9

**Browser specs run against an isolated stack, never the one you are working
in.**

*Written down 2026-08-05, EPIC-023 S004. It had lived only in conversation.*

The browser suite is verification against a running stack
([ADR-019](../adr/ADR-019-testing-strategy.md)), so unlike `dotnet test` it
cannot provision anything for itself. Bring up a second stack rather than
pointing it at the one you develop in — a suite that seeds and retires business
entities will otherwise do so in the database you are reading.

```bash
# 1. A database of its own, migrated from the current chain.
docker exec -i postgres-local psql -U admin -d postgres -c 'CREATE DATABASE regos_verify;'
ConnectionStrings__RegOS="Host=localhost;Port=5432;Database=regos_verify;Username=admin;Password=password123" \
  dotnet ef database update --project src/Persistence/RegOS.Persistence --startup-project src/Host/RegOS.Api

# 2. The API on 5301, seeding itself at boot.
ConnectionStrings__RegOS="…Database=regos_verify;…" Storage__RootPath=/tmp/regos-verify \
  ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Host/RegOS.Api --no-launch-profile --urls http://localhost:5301

# 3. The web app on 5174, pointed at it.
VITE_API_BASE_URL=http://localhost:5301 npm run dev -- --port 5174

# 4. The specs, pointed at both.
REGOS_WEB_URL=http://localhost:5174 REGOS_API_URL=http://localhost:5301 npm test
```

Four things that are easy to get wrong, each of which has cost a session:

- **`--no-launch-profile` *and* `ASPNETCORE_ENVIRONMENT=Development`.** Without
  the first, `launchSettings.json` overrides the URL; without the second, the
  development credentials are never seeded and every spec fails at sign-in.
- **The API must be told to seed into the new database**, which it does at boot —
  but it does **not** migrate. Step 1 is not optional.
- **Widening CORS for port 5174 is a temporary edit to `Program.cs`.** Revert it
  *surgically* and verify: `grep -rn "5174" src/ --include='*.cs'` must return
  nothing. Never revert by checking out the whole file — that discards
  unrelated work in it.
- **Kill by PID** — `lsof -ti tcp:5301 | xargs kill` — never `pkill -f dotnet`,
  which takes the stack you were protecting.

---

# Test Pyramid

The traditional testing pyramid focuses on implementation.

RegOS focuses on confidence.

```text
System Confidence
        ▲
Integration Confidence
        ▲
Decision Confidence
        ▲
Capability Confidence
        ▲
Model Confidence
```

Every layer builds confidence in the one above it.

---

# Testing Checklist

Before considering a capability complete, verify:

- [ ] Business invariants are tested.
- [ ] Capability outcomes are tested.
- [ ] Business Rules are independently verified.
- [ ] Decision reasoning is validated where applicable.
- [ ] Integration boundaries are tested.
- [ ] Failure scenarios are covered.
- [ ] Historical behavior remains preserved.
- [ ] Architectural boundaries remain valid.
- [ ] Every browser spec owns the entities it mutates (Principle 7).
- [ ] Both halves of every new capability are reachable — the user can perform
      the action *and* observe the fact it created (Principle 8).

---

# Change History

| Version | Date | Summary |
|----------|------------|------------------------------------------|
| 1.3 | 2026-08-05 | Standard 8 — a `dotnet test` suite takes a database rather than naming one (EPIC-023, ADR-064). Standard 9 — the isolated browser stack, written down after living only in conversation. |
| 1.2 | 2026-08-05 | Standard 7 — `{ exact: true }` on `getByLabel()`, at the second occurrence of the same defect (EPIC-010c S002). |
| 1.1 | 2026-08-01 | Principle 8 — both halves of a capability are reachable (EPIC-017 S005). |
| 1.0 | 2026-07-08 | Initial approved version. |