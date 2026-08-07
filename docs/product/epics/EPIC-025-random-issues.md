# EPIC-025 — Random Issues

**Status:** 🟡 Open (standing) · **Branch:** none — bugs are fixed on whatever
branch is convenient · **Process:** [FEATURE-DEVELOPMENT-FLOW.md](../FEATURE-DEVELOPMENT-FLOW.md)
(deliberately only in part — see below)

A register for defects found **incidentally**: while building something else,
while reading, while using the app. Not planned work, and not a theme.

---

## What this is, and how it differs from every other epic

Every other epic in the backlog states an outcome, breaks into stories, meets a
Definition of Done and closes. **This one does none of those things**, on
purpose:

- **It has no DoD and never closes.** An empty register is the goal state, not
  the finish line.
- **It has no branch.** A bug here is fixed inside whatever work happens to be
  touching that code, and the commit cites `EPIC-025 BUG-nnn`.
- **It has no priority position.** It is not competing with EPIC-021 for a slot.

That is a real departure from the flow, and it is worth naming rather than
letting it be discovered: **a standing register can become a place where
problems go to be forgotten.** The rules below exist to stop that.

### Rules

1. **Record it when you find it, not when you can fix it.** The cost of a bug is
   mostly in re-finding it. A one-line entry beats a resolution to remember.
2. **Evidence, not suspicion.** An entry says how it was observed — an error, a
   failing command, a file and line. *"Ordering looks wrong somewhere"* is not
   an entry.
3. **A defect class is one bug, however many sites it has.** BUG-001 has 15
   call sites and is one entry, because it is one cause and one fix.
4. **Promote when it stops being random.** If an entry needs an ADR, changes a
   contract, or spans more than a few files of design work, it leaves here and
   becomes its own epic or a story on a live one. This register is for things
   that can be fixed without deciding anything.
5. **Nothing is closed silently.** An entry moves to ✅ with the commit that
   fixed it, or to ❌ with the reason it will not be.

### The severity vocabulary

| | Meaning |
|---|---|
| **Live** | Users or developers are hitting it now |
| **Latent** | Real defect, currently unreachable — reachable on data we will plausibly have |
| **Friction** | Not a defect; costs time every time it is met |

---

## The register

| ID | Issue | Severity | Found | Status |
|---|---|---|---|---|
| [BUG-001](#bug-001--in-memory-ordering-by-an-id-throws-once-a-collection-holds-two-rows) | In-memory ordering by an id throws once a collection holds two rows | **Live** | 2026-08-06 | 🟡 **6 of 14 fixed** — reproduced, and one listed site was not a defect |
| [BUG-002](#bug-002--the-frontend-does-not-compile) | The frontend does not compile — `npm run build` fails | **Live** | 2026-08-06 | ⚪ Open — **does not reproduce on `main`'s descendants**, see the entry |
| [BUG-003](#bug-003--five-seed-initializers-can-never-pick-up-a-newly-added-row) | Five seed initializers can never pick up a newly added seed row | **Latent** | 2026-08-06 | ⚪ Open |

---

### BUG-001 — In-memory ordering by an id throws once a collection holds two rows

**Severity:** Live · **Found:** 2026-08-06, by a market page returning a 500

```
System.InvalidOperationException: Failed to compare two elements in the array.
 ---> System.ArgumentException: At least one object must implement IComparable.
   at ListMarketRegistrationsHandler.HandleAsync(…) line 99
```

**Cause.** Neither id family implements `IComparable` — not
`readonly record struct <X>Id(Guid Value)`, and not
`abstract class StronglyTypedId : IEquatable<StronglyTypedId>`. Inside an EF
query `.ThenBy(x => x.Id)` becomes SQL and is fine; **after materialisation it
needs `IComparable` and throws.** The two forms are mutually exclusive:
`.Value` has no SQL translation, `.Id` has no in-memory comparison.

**Why it hid.** Sorting fewer than two elements never invokes the comparer, so
every one of these sites is invisible until its collection holds a second row.

**Where it came from.** EPIC-024 S002 added the id tie-breaker at ~124 read
paths to make ordering deterministic — correctly; before that these sorts had
no tie-breaker at all, which is worse. What it exposed is that the kernel has
no ordering, which predates it.
[`DeterministicOrderingTests`](../../../tests/Architecture/RegOS.Architecture.Tests/DeterministicOrderingTests.cs)
accepts both `.Id` and `.Id.Value`, so it cannot catch the illegal one.

**The 15 sites** — of 66 id-keyed orderings, 51 translate to SQL and are safe:

| # | Site | Key | Throws when |
|---|---|---|---|
| 1 | `ListMarketRegistrationsHandler.cs:102` | `Id` | **observed** |
| 2 | `ListSiteAlignmentHandler.cs:90` | `Id` | a product has 2+ sites |
| 3 | `GetRegistrationHandler.cs:102` | `Id` | 2+ status entries |
| 4 | `GetRegistrationHandler.cs:103` | `Id` | 2+ status entries |
| 5 | `ListRegistrationMarketsHandler.cs:81` | `CountryId` | 2+ markets |
| 6 | `GetMedicinalProductHandler.cs:81` | `Id` | 2+ market-status entries |
| 7 | `ListProductsContainingSubstanceHandler.cs:80` | `PresentationId` | 2+ presentations |
| 8 | `ListDueWorkHandler.cs:147` | `Id` | 2+ due items |
| 9 | `ListInspectionsHandler.cs:66` | `Id` | 2+ history entries |
| 10 | `ListMeetingsHandler.cs:63` | `Id` | 2+ history entries |
| 11 | `GetRegulatoryTemplateHandler.cs:58` | `DocumentTypeId` | 2+ required docs in a section |
| 12 | `GetSubmissionContentPlanHandler.cs:157` | `DocumentTypeId` | 2+ placeholders in a section |
| 13 | `GetSubmissionContentPlanHandler.cs:211` | `SubmissionDocumentId` | 2+ documents in a section |
| 14 | `ListNextStepsHandler.cs:127` | `StepId` | 2+ next steps |
| 15 | `CreateSubmissionHandler.cs:204` | `Id` | 2+ template candidates |

**#2 is next to fire** — `ListSiteAlignment` is the manufacturing read, and the
EPIC-010c migration seeds three demo sites.

**#15 is the one to read.** It is the exact site EPIC-024 S002 listed as its
third correctness defect, whose fix carries the comment *"Seed data holds one,
so the tie is unreachable today; this ordering does not depend on that staying
true."* It now does depend on it — differently. **A silent nondeterminism was
traded for a loud crash, and neither was catchable with one row.**

**Decided, 2026-08-06 — the fix is not to make ids comparable.** Adding
`IComparable` to `StronglyTypedId` would work and was rejected: it asserts that
identities have an ordering, which they do not, and it is a permanent kernel
change bought to work around a current EF limitation. The evidence against it
came from the caveat itself — **.NET `Guid.CompareTo` and PostgreSQL `uuid`
ordering are not the same**, so the ordering is a property of a runtime, not of
the identity.

**The direction instead**, which
[`DeterministicOrderingTests:40-43`](../../../tests/Architecture/RegOS.Architecture.Tests/DeterministicOrderingTests.cs)
already articulates — *"LINQ-to-Objects sorts stably, so such an ordering is
deterministic exactly when its source is"*: give the in-memory sort a
**deterministically ordered source** (an `orderby` in the EF query) and let
stability carry it. No `.Value`, no kernel change, no new id API.

**Reproduced, 2026-08-07**, before anything was changed —
`RegistrationTests.Two_registrations_in_one_market_sort_without_throwing`. Two
registrations of the **same** product in one market, so prominence and name tie
and the id tie-break actually runs. It threw exactly the reported exception.
**Every existing test on that handler used one row**, which is the whole reason
this was invisible.

**Fixed — 6 sites**, in the decided direction. The tie-break moves into SQL and
LINQ-to-Objects stability carries it. No `.Value`, no kernel change, no new id
API.

| # | Site | Where the order now comes from |
|---|---|---|
| 1 | `ListMarketRegistrationsHandler` | `orderby` in the query |
| 2 | `ListSiteAlignmentHandler` | `orderby` in the query |
| 3–4 | `GetRegistrationHandler` | **ordered `Include`** |
| 5 | `ListRegistrationMarketsHandler` | `orderby` after the `group by` |
| 7 | `ListProductsContainingSubstanceHandler` | `orderby` in the query |

**The ordered `Include` extends the decided direction**, which said *"an `orderby`
in the EF query"* and did not obviously cover a collection loaded with its
aggregate. `.Include(x => x.History.OrderBy(e => e.Id))` is applied in SQL, where
an id translates — so the in-memory sort only has to be stable.

**#3 and #4 are one sort, not two.** The line `.ThenBy(entry => entry.Id)` was
written twice in succession. The duplicate is gone.

**#8 is not a defect, and the count is now 14.** `ListDueWorkHandler:147` orders
by `DueWorkItem.Id`, which is a plain `Guid` — `IComparable`, and it cannot
throw. It belongs with the 51 safe orderings. **The list was evidence, not
gospel**, which is why the remaining eight will each have their key type checked
before they are touched.

**`DeterministicOrderingTests` caught the fix twice** — once for each ordering
that lost its tie-break, and again for the ordered `Include`. Both now carry a
`// Deterministic:` sentence saying why stability suffices. **EPIC-024's guard,
from the epic that caused this bug, is what kept its fix honest.**

**Remaining — 8 sites:** `GetMedicinalProduct`, `ListInspections`, `ListMeetings`,
`GetRegulatoryTemplate`, `GetSubmissionContentPlan` (×2), `ListNextSteps`,
`CreateSubmission`. Three of them project a child collection inside an in-memory
`Select`, which is a source shape none of the six fixed so far had.

> **This entry is at the edge of rule 4.** The per-site fixes belong here; the
> guard that prevents recurrence needs an ADR and possibly a Roslyn semantic
> model, and should be promoted when taken.

**Open — not yet audited:** 66 in-memory orderings exist across all key types.
The 15 above are the id-keyed ones. Sorting on a bare **value object** fails
identically, and establishing that needs type resolution rather than regex.

---

### BUG-002 — The frontend does not compile

**Severity:** Live · **Found:** 2026-08-06, running `npm run build`

```
src/features/regulatory/labels/components/GlobalLabelVersions.tsx(135,21):
error TS2322: Type 'string | null' is not assignable to type 'string'.
```

The file is committed and unmodified; last touched by `42c599a`
*"feat(labeling): the label a company holds above any market"*. `tsc -b` fails,
so **`npm run build` produces no bundle** — this is not a warning.

`npm run lint` separately reports 6 problems (3 errors, 3 warnings) in four
files, none of them this one. Those are pre-existing and not part of this entry.

> **Does not reproduce — checked 2026-08-06 on `feat/EPIC-020-S005-late-work`.**
> `tsc -b --force` with every `.tsbuildinfo` deleted exits **0**, so this is not
> an incremental-cache artefact. `npm run lint` reports **3 problems (0 errors,
> 3 warnings)**, not 6.
>
> **The entry names `42c599a` as the file's last commit, and it is not.**
> `2a82753` — *"fix(labeling): the null the Select emits, and the build nobody
> ran"*, 2026-08-04 — touches this exact file and **is an ancestor of
> `origin/main`**. So the fix predates the finding by two days on every branch
> that descends from `main`.
>
> **The likeliest explanation is a worktree that does not contain `2a82753`**,
> and that is worth confirming rather than assuming: if it reproduces somewhere,
> the interesting fact is *where*, not the error. **Left open rather than closed
> — the finder verifies their own environment, not me.**

---

### BUG-003 — Five seed initializers can never pick up a newly added row

**Severity:** Latent · **Found:** 2026-08-06, while verifying seeding on a fresh
database

Two idempotency patterns are in use across the initializers:

- **Insert-missing-by-id** (11 of them) — compares ids, inserts what is absent.
  Add a new row to the seed data and every database picks it up on next boot.
- **Whole-table guard** (5: `Organization`, `Tenant`, `Site`, `Product`,
  `GeographyAndRegulatory`) — `if (!AnyAsync()) insert all; else reconcile`, and
  `ReconcileAsync` **updates only rows that already exist**.

**The consequence:** add a fourth demo organization and only databases created
*after* that change will ever have it. Existing developer databases will not,
and neither will anyone else's — a live source of "works on my machine".

**This is a deliberate design, not an oversight**, and the reason is written at
`OrganizationInitializer.ReconcileAsync`: *"Inserting here would push demo data
into any database that happens to hold real organizations."* The
EPIC-010c manufacturing migration repeats the same reasoning for its demo sites.

**So the entry is not "fix the guard".** It is that the consequence is undocumented
and surprises people, and that the two patterns are chosen per-initializer with
nothing stating which to use when. Possible resolutions: document it and move on;
or separate "demo data" from "reference data" so the two get different rules.

---

## Not recorded here, and why

**The dev database needs `dotnet ef database update` by hand.**
`Program.cs` has no `MigrateAsync()`, so every migration anyone lands on a
shared branch breaks the running app until someone applies it — hit **six
times** on 2026-08-06 alone, and on a genuinely new database the app cannot
start at all.

That is Friction and it is real, but it is **not a bug** — it is an open
decision (Development-only auto-migrate versus applying deliberately), and rule
4 sends it elsewhere. Raised here only so the register is not silently the
place it was dropped.

---

## Change History

| Date | Change |
|---|---|
| 2026-08-06 | Epic created; BUG-001, BUG-002, BUG-003 recorded |
| 2026-08-07 | BUG-001 reproduced and 6 of its sites fixed; site #8 disproved, so the class is 14 sites rather than 15 |
