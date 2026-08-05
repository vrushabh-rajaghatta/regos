# EPIC-024 — The invariants nothing checks

**Status:** 🔵 In progress · **Branch:** `epic/EPIC-024-invariants` (cut at Phase 1) · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)

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

#### 89 orderings, and not one ends in a unique key

17 in query syntax, 72 in method syntax, **zero** terminating on an id.

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

### S001 — the dependency graph

**Review the two `*.Infrastructure` → `Product.Domain` edges first**, because
their fate changes what gets written down: if they stay, they become
intentional; if they go, the graph is simpler forever.

Then declare the graph and the two layer rules, with the ADRs that constrain
individual edges cited where they apply.

### S002 — deterministic ordering

D3's rule, then the call sites. **The largest story by a wide margin** — 89
orderings, of which an unknown fraction are already total and need a sentence
rather than a key. Mechanical, and behaviour-preserving in every case: adding a
tie-breaker changes only the order of rows that were previously arbitrary.

### S003 — the test-database guard

Transitive `.csproj` reach: any test project that can see `RegOS.Persistence`
must also reference `RegOS.TestSupport`. Passes today, so it is a guard rather
than a fix.

### S004 — capstone

**Every guard demonstrated failing on a deliberate violation**, as EPIC-023's
was. A guard nobody has seen fail is a guard nobody should trust, and that epic
found two of its own assertions weaker than they looked by making exactly this
check.

### Out of scope ⏸️

**Anything that requires a new architectural decision.** If a rule turns out to
be *wrong* rather than *unenforced*, the story stops and the answer is an ADR.

---

## Change History

| Date | Change |
|---|---|
| 2026-08-05 | Raised in BACKLOG.md at EPIC-010c's close with two items; a third added at EPIC-023's close |
| 2026-08-05 | Phase 1 + Phase 2 signed off. D3 reframed from *"use ids"* to *"prove determinism"*, so the rule outlives today's preferred technique |
