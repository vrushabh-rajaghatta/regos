# EPIC-024 — The invariants nothing checks

**Status:** 🟢 Complete · **Branch:** `epic/EPIC-024-invariants` (cut at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

The second of the two hardening epics raised at EPIC-010's close, and the sibling
of [EPIC-023](EPIC-023-test-schema.md) by kind rather than by subject:

> **EPIC-023 made the *environment* trustworthy. This makes the *architecture*
> trustworthy.**

---

## Phase 1 — Epic plan

### Outcome

> **The requirement, stated once:** *an architectural rule the project relies on
> should be enforced by something that runs, not by a person remembering it.*

**Three rules, all of them already decided and already written down.** None needs
a design; each needs a test. That is unusually cheap for the confidence it buys,
and the argument for doing it now is that a fourth instance would be found the
same expensive way the second one was.

### How each was found — and not one by review

| | Found by |
|---|---|
| **1. The bounded-context dependency graph is not checked** | EPIC-010c S001 adding the first `Product.Domain` → `Organization.Domain` reference and **the architecture suite staying green** |
| **2. A query handler's ordering is not checked for totality** | most of a session chasing an intermittent browser failure that read as environmental and was a missing `ORDER BY` tie-breaker |
| **3. Nothing says a database test must use the test database** | asking at EPIC-023's close what its new guard *actually* asserts |

> What did catch the equivalent of (1) one epic earlier was **the compiler**, when
> ADR-061 §3's proposed edge closed a cycle. **A cycle is self-enforcing; a
> direction is not.** [ADR-063](../../adr/ADR-063-where-a-product-is-made-is-a-product-fact.md)
> closes the reverse edge permanently, and nothing would stop anyone opening it:
> add a project reference, and it builds.

### What was measured before anything was designed

#### The graph is cleaner than expected

**26 `*.Domain` → `*.Domain` edges across 11 contexts**, and the layers above are
stricter than the layers below:

| Layer | Shape | Exceptions |
|---|---|---|
| `*.Domain` | own context + `SharedKernel` + other Domains | the 26 edges |
| `*.Application` | **own Domain + `Persistence`** (+ `Storage`) | **zero** |
| `*.Infrastructure` | own Domain + own Application + `Persistence` | **two, and both turned out to be removable** — see [S001](#s001--the-dependency-graph) |

#### ~~89~~ orderings, and not one ends in a unique key

17 in query syntax, 72 in method syntax, **zero** terminating on an id.

> **The count was wrong and the conclusion was not.** These regexes were too
> narrow; [S002](#s002--deterministic-ordering) found **124** on read paths, of
> which exactly **one** already terminated uniquely. Kept rather than corrected
> in place, because the shape of the error is instructive: **a sample that
> agrees with your conclusion is the one you stop checking.**

And the property is **not syntactically decidable**. `orderby site.Name` is total
*if site names are unique*. `ListManufacturingOperations` — the handler EPIC-010c
fixed — is total only because a **filtered unique index** makes its last keys
unique together. **Totality usually rests on an invariant the ordering keys do
not reveal**, so no scan can tell *already total* from *not*.

That is why [D3](#d3--the-invariant-is-deterministic-ordering) is phrased as a
property with two ways of satisfying it, rather than as a rule about ids.

#### The third rule, as raised, was vacuous

The backlog entry said *"any test project **referencing** `RegOS.Persistence`"*.
**No test project references it directly** — all seven reach it transitively
through `*.Infrastructure`. The correct rule is transitive reach, and it
separates perfectly: **7 reach `Persistence`, the same 7 reference
`RegOS.TestSupport`, 0 mismatches.**

> Recorded rather than quietly fixed, because the rule was written *one day
> earlier* by someone who had just spent an epic in those files. **A rule stated
> from memory is a hypothesis**, and this one was false on its first contact with
> the `.csproj` graph.

---

## Phase 2 — The three decisions *(approved 2026-08-05)*

### D1 — The graph is declared, and the declaration is the specification

A test holds the 26 Domain edges plus the two layer rules.

**This is not a grandfathered list**, and the distinction is the whole point:

| A grandfather list | A specification |
|---|---|
| *these are today's violations* | *this is the architecture* |
| accumulates debt | is executable documentation |
| shrink-only | changes when the architecture does, **with an ADR** |

Changing it is exactly the deliberate act CLAUDE.md's *"new cross-context
dependency → ADR first"* already requires. It will look like a grandfather list
to the next reader, which is why it says so at the top of the file.

### D2 — No new ADR

**This epic makes existing decisions executable; it decides nothing.**
[ADR-063](../../adr/ADR-063-where-a-product-is-made-is-a-product-fact.md)
§Consequences already closes Organization → Product,
[ADR-061](../../adr/ADR-061-a-pack-is-how-a-medicine-is-supplied.md) §3 records
the cycle refusal, and CLAUDE.md carries the ADR-first rule.

> **If a test surfaces an edge nobody intended, that stops the story and becomes
> an ADR.** Finding one would be a good outcome rather than a problem — it is the
> case for the epic, not an obstacle to it.

### D3 — The invariant is deterministic ordering

**Not *"use ids"*.** Ids are the easiest implementation, not the requirement:

> **Every externally observable ordering must be deterministic** — given the same
> database state, repeated execution returns rows in the same order. Where
> determinism is not obvious from the ordering keys, the author must **either
> terminate the ordering with a unique key, or document the invariant that makes
> the existing ordering total.**

**The escape hatch is not a loophole, it is the better outcome where it applies.**
`.OrderBy(x => x.Code)` over a closed vocabulary is already deterministic;
appending `.ThenBy(x => x.Id)` would change nothing and be pure ceremony. The
comment says *"this ordering is already total because…"*, which is knowledge the
code does not otherwise carry — the same idiom as
[Standard 7](../../engineering/testing.md), *"say so at the call site when you
rely on it."*

**Deliberately not narrowed to dates and nullables.** Today's defect happened to
involve a date; the property it violated was **partial ordering**, and tomorrow
that could be a name, a sequence number, a display order, a version or a
priority. Narrowing the rule to the shape of the last bug is how you get the
next one.

**Scoped to *observable* orderings.** An ordering feeding `.First()` has
different implications from one feeding `.ToListAsync()` or a paged query; the
architectural concern is anything whose order becomes part of observable
behaviour. If the test cannot distinguish those cheaply, applying the rule
everywhere is acceptable — but **the epic and its guard are written in terms of
observable determinism, never in terms of LINQ syntax**, so the rule survives
whatever uniqueness mechanism the project prefers in five years.

---

## Phase 3 — Stories

### S001 — the dependency graph ✅

**The two `*.Infrastructure` → `Product.Domain` edges were reviewed first**,
because their fate decided what got written down. **Both were redundant.**
`Registration.Infrastructure` and `RegulatoryApplication.Infrastructure` each
used them for a product **id type** their own `*.Domain` already carried, so the
references came out and the solution built unchanged.

> **The graph got simpler forever rather than gaining two documented
> exceptions** — the better of the two outcomes the story could have had, and it
> only appeared because the review came before the declaration.

Six rules now hold it. The 26 Domain edges are declared per context, so
**ADR-063's permanent closure is enforced by `Product`'s absence from
`Organization`'s list** rather than by anyone remembering. Above them: an
Application references only its own Domain and Persistence (**zero exceptions
across eleven contexts**), an Infrastructure adds its own Application, and the
host reaches neither's internals.

One rule the plan did not ask for, added because the declaration would otherwise
rot: **the graph declares no edge that no project takes.** A permission nobody
asked for would let the next real one through unnoticed — *"the architecture
already allowed it."*

### S002 — deterministic ordering ✅

**The audit was the finding.**

| Measure | Result |
|---|---|
| Read-path orderings audited | **124** |
| Already terminated in a unique key | **1** |
| Mechanical — given a unique final key | **81** |
| **Existing invariants documented** | **42** |
| **Real correctness defects** | **3 classes** |

**The number that matters is the second row.** This was never a partially
enforced convention that had drifted. **There was no convention** — including in
the one handler EPIC-010c had already fixed by hand.

#### The three defects

**1. `TemplateSection.Order` is not unique — the unique index is
`(VersionId, Code)`.** Three read paths ordered by `Order` alone: the submission
content plan, the blueprint validation runner, and the template detail. **The
system still "worked".** But a template with two sections at one `Order`
reshuffled between reloads, and **validation reported its rules in a different
order each run with no data change** — a property users expect and cannot easily
name.

**2. `ListProducts` and `GetUsers` order, then `Skip`/`Take`.** A tie in a paged
query does not merely reorder a page: **it can move a row between pages, or drop
it.** That is correctness, not presentation.

**3. `CreateSubmission` picked a template by tenant-ownership alone.** The defect
was not *there are two templates* — it was that **correctness depended on there
never being two.** Seed data holds one, so the tie is unreachable today; the code
now says what it depends on instead of relying on that staying true.

#### The 42 are the durable half

Before this story, an ordering like `.OrderBy(condition => condition.Code)` was
only understandable to someone holding the query, the EF model **and** the unique
index in their head at once. Now the query carries the reason.

> `SequenceFolderGenerator` is the clearest case. **ADR-049 already required
> byte-identical packages**, and the implementation was already careful — nine
> orderings, every one of them deliberate. **Not one said why it was safe.**

#### The rule had a bug, and it was in the safe direction

It did not follow multi-line query syntax, so it flagged
`ListManufacturingOperations` — already ending in `operation.Id` — as unproven.
**The rule was fixed, not the code**, and the only reason it was caught is that
the result did not look right. A guard's own failure mode is worth knowing:
**this one errs towards false alarm, which is the direction that gets
investigated.**

### S003 — the test-database guard ✅

Deliberately small. Any test project that can **reach** `RegOS.Persistence` must
reference `RegOS.TestSupport` — reach, not reference, because
**no test project references it directly**; all seven arrive transitively through
`*.Infrastructure`, so the rule as first written down matched nothing at all.

Both directions, on S001's lesson: a project that cannot reach persistence must
**not** carry the reference either.

### S004 — capstone ✅

**Every guard, broken on purpose, with what it says.** A guard nobody has seen
fail is a guard nobody should trust.

| The violation | What the suite says |
|---|---|
| Open the edge **ADR-063 closed permanently** | `RegOS.Organization.Domain -> RegOS.Product.Domain` |
| Declare an edge no project takes | `Organization -> Submission` |
| **Reintroduce EPIC-010c's exact defect** — drop the tie-breaker from `ListManufacturingOperations` | `…ListManufacturingOperationsHandler.cs:50  orderby operation.CeasedOn == null descending,` |
| A database test that does not take a test database | `RegOS.Registration.Application.Tests` |

The third is the one worth keeping: **the defect that cost most of a session and
read as environmental is now caught by a test that runs in 0.3 seconds and names
the file and line.**

---

## Retrospective

### Measured outcomes

| Measure | Before | After |
|---|---|---|
| Bounded-context graph | 26 edges + 2 exceptions, held by `.csproj` lines | **26 edges, no exceptions**, declared and enforced |
| ADR-063's closed reverse edge | held by nothing | **enforced by an absence in the declaration** |
| Read-path orderings terminating uniquely | **1 of 124** | **124 of 124 proven** |
| Query invariants documented at the call site | 0 | **42** |
| Correctness defects from partial ordering | 3 classes live | **0** |
| Test projects that could skip the test database | possible, unchecked | **guarded, both directions** |
| Architecture suite | 25 tests | **36** |

### The lessons worth carrying past this epic

#### 1. Writing the architecture down deletes structure

S001's expected output was a document. Its actual output was **two fewer
dependencies**. The redundancy had been invisible for as long as the graph lived
only in `.csproj` files; it was obvious within minutes of the graph being stated
in one place.

#### 2. A rule stated from memory is a hypothesis

S003's rule was written into the backlog **one day earlier**, by someone who had
just spent an epic in those files, and it was **false** — no test project
references `RegOS.Persistence` directly. It cost nothing because it was checked
before it was built. **The cheap moment to falsify a rule is while writing the
test for it.**

#### 3. Ask what the green tick asserts

EPIC-023's connection-string guard passed, and the question *"what does this
actually prove?"* produced S003. **"No file names a database" is not "every
database test uses the fixture"**, and nothing but asking would have separated
them.

#### 4. Prefer the property to the technique

D3 was reframed from *"end orderings with an id"* to *"prove determinism"*. That
change is why **42 sites got a sentence instead of a redundant key** — and the
sentences turned out to be the epic's most durable output. A rule about ids would
have produced 124 mechanical edits and no knowledge.

#### 5. The defect is rarely the shape of the last defect

EPIC-010c's ordering bug involved a date. Narrowing the rule to dates was
explicitly refused, and the sweep then found the same defect in `Order`, in
`Name`, and in paging. **Narrowing a rule to the shape of the last bug is how you
get the next one.**

### What this epic did not do

- **It decided nothing.** Every rule was already stated — in an ADR, in
  `CLAUDE.md`, or in an EF configuration. That was [D2](#d2--no-new-adr), and it
  held: **no ADR was needed, and none was written.**
- **It does not prove the orderings are *right*** — only that they are stable. A
  list can be deterministically in an unhelpful order.
- **Nothing runs any of this but a person.** The same sentence closed EPIC-023,
  and it is still true: these guards make `dotnet test` worth more and do not
  make it run. That remains [EPIC-015](../BACKLOG.md#later)'s.

### Definition of Done

| | | |
|---|---|---|
| 1 | The dependency graph is declared and enforced | ✅ 6 rules, 26 edges, 0 exceptions |
| 2 | Every read-path ordering proves its determinism | ✅ 124 of 124 |
| 3 | A test project that can reach the database takes a test database | ✅ both directions |
| 4 | Every guard demonstrated failing on a deliberate violation | ✅ four, with their messages recorded |
| 5 | `dotnet test RegOS.slnx` green across **19 reporting suites** | ✅ |

---

## Change History

| Date | Change |
|---|---|
| 2026-08-05 | Raised in BACKLOG.md at EPIC-010c's close with two items; a third added at EPIC-023's close |
| 2026-08-05 | Phase 1 + Phase 2 signed off. D3 reframed from *"use ids"* to *"prove determinism"*, so the rule outlives today's preferred technique |
| 2026-08-05 | S001. Both Infrastructure exceptions turned out redundant; the graph got simpler instead of documented |
| 2026-08-05 | S002. 1 of 124 orderings was proven; 42 invariants documented and 3 correctness defects removed |
| 2026-08-05 | S003. Reach, not reference — the rule as raised matched nothing |
| 2026-08-05 | S004. Four guards broken on purpose. Closed |
