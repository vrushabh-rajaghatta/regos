# Milestone 1 — Organization

**Status:** Complete · **Closed:** 2026-07-20 · **Commits:** `28a3893`..`f77edce`

An engineering record, not an architecture document. It exists so that six
months from now the answer to *"why is `Reclassify` separate from `Rename`?"* or
*"why do browser specs always create their own organizations?"* is one page away
instead of reconstructed from commits.

---

## What shipped

The organization lifecycle, end to end, from browser to Postgres.

```
Create ──▶ Active ──Deactivate──▶ Inactive ──Activate──▶ Active
```

Editing is available in either state and changes neither.

| # | Slice | Commit |
|---|---|---|
| 1 | Create Organization | `28a3893` |
| 2 | Deactivate Organization | `edf16e0` |
| 3 | Get Organization | `3e1d0af` |
| 4 | Update Organization | `eb4efd2` |
| 5 | Activate Organization | `f77edce` |

`ListOrganizations` already existed. Persistence — EF configuration, the
`AddOrganizations` migration, and the demo seeder — already existed too, which
the plan had not accounted for.

**Endpoints:** `POST /organizations`, `GET /organizations`,
`GET /organizations/{id}`, `PUT /organizations/{id}`,
`POST /organizations/{id}/activate`, `POST /organizations/{id}/deactivate`.

**UI:** `features/platform/organizations/` — directory, details page, create and
edit dialogs, activate and deactivate confirmations. (Moved to
`features/regulatory/organizations/` in EPIC-016 S004; the path above is where
this milestone left it.)

**Tests at close:** 245 unit, 14 browser specs.

---

## Decisions worth remembering

### The aggregate speaks in business verbs

There is no `UpdateOrganization(...)` method. There is `Rename`, `Reclassify`,
`Activate`, `Deactivate`.

`Reclassify` is deliberately separate from `Rename` even though both are plain
assignments today. The two carry different intent and will grow different rules
— reclassifying an organization that already holds marketing authorizations is a
conversation that has not happened, and an explicit method is where that rule
will live when it does. Same reasoning Product wrote down for `ChangeType`.

### Editing an inactive organization is allowed

Deactivation says *"do not start new work with this"*, not *"freeze the
record"*. A misspelled legal name is worth correcting either way. This follows
Product, where an archived product can still be renamed, and it keeps lifecycle
transitions separate from data corrections.

Status is absent from the update command and the edit form for the same reason:
it belongs to `Activate` and `Deactivate`.

### No-op updates are no-ops

Submitting unchanged values does nothing. There is no version to increment and
no concurrency token, so no rule was invented to reject it.

### Create is the one command outside tenant scope

Every other tenant-scoped command resolves its organization from
`ITenantContext` ([ADR-013](../../adr/ADR-013-ambient-tenant-context.md)).
`CreateOrganization` cannot: it is what brings a tenant into existence, so there
is nothing to resolve from. It is the bootstrap case.

Consequence, recorded rather than solved: **anyone can create an organization
today, unauthenticated.** That is Milestone 4's problem, and this is its first
concrete instance.

`GetOrganization` likewise applies no tenant filter, unlike the product and user
detail queries. An organization *is* a tenant, so scoping that read to the
caller's own would reduce the directory to a single row.

---

## Defects implementation exposed

Each was found by building or verifying, not by review.

### `{"type": 99}` returned 201 Created

Model binding turns an out-of-range integer into an enum without complaint, so
an organization persisted with a type that had no name. Decidable from the
request alone, therefore 400
([ADR-009](../../adr/ADR-009-command-validation-model.md)). Fixed in the
aggregate with `Enum.IsDefined`.

`Product.Register` has the identical hole and was left alone as out of scope —
logged as **AB-007**, with a note that ProductDocument, RegulatoryApplication
and Submission were never audited for it.

### `Activate()` and `Deactivate()` were unconditional

Both silently succeeded when there was no transition to make, which tells a
caller with a stale view that their operation worked. Both now raise
`BusinessRuleViolationException` → 409.

### `Activate()` and `Deactivate()` were unreachable

They existed on the aggregate with no command, handler or endpoint touching
them. Working behavior nobody could invoke.

### A seeded organization was silently mutated

`Demo MAH Ltd.` — which is also `TENANT`, the organization every browser spec
runs as — was found `Inactive`. The cause was never conclusively identified; the
best candidate is a spec that selected `organizations.find(status === "Active")`
and acted on ambient data. It was found by inspection, not by a failing test,
which is the part that mattered.

Two safeguards now exist, doing different jobs:

- `OrganizationInitializer` reconciles demo data on startup — developer
  convenience. It updates only rows that already exist, so it never pushes demo
  data into a database holding real organizations.
- `seed-integrity.spec.ts` is a canary. If it fails, a spec mutated something it
  did not create.

---

## Testing improvements

**Principle 7 — a test owns every entity it mutates.** Seed via the API, capture
the id, operate on that id, retire it. Never select a subject from ambient data.
Recorded in [`docs/engineering/testing.md`](../../engineering/testing.md);
strengthens [ADR-019](../../adr/ADR-019-testing-strategy.md) rule 1.

**A create slice needs its retirement path before it can be tested end to end.**
The ORG-001 browser spec was written, run, and then deleted, because an
organization could not be removed and the spec leaked a row per run. This
reordered the milestone: Deactivate moved from last to second, and ORG-002
restored the spec. The lesson generalizes — delete, archive, deactivate or
cancel, whichever the domain owns.

**Playwright matches `getByRole` names as case-insensitive substrings.** So
`name: "Activate"` also matches `"Deactivate"`. An assertion that no Activate
button remained was matching the Deactivate button that had correctly replaced
it. Lifecycle assertions now use `exact: true`.

---

## Patterns validated

**Handlers stayed thin across all five slices** — load the aggregate, invoke its
behavior, persist. No business rule leaked out of the aggregate. If a future
handler accumulates branching logic, treat it as a smell.

**Repositories are duplicated twice, not three times.** `OrganizationRepository`
and `ProductRepository` are near-identical `AddAsync`/`GetByIdAsync`/`UpdateAsync`
wrappers. Deliberately left alone under
[ADR-018](../../adr/ADR-018-rule-of-three.md) — a generic repository looks
attractive at the second implementation and becomes a liability by the fifth.

**The list is a directory; the details page is where actions happen.** Deactivate
briefly lived on the table row and moved in ORG-004. A row that both navigates
and acts makes the click target ambiguous.

---

## Known assumptions and open items

| Item | Status |
|---|---|
| *"A deactivated organization rejects user invitations"* | **Claimed in the original DoD and never true here.** Nothing in Organization invites anyone. Moved to Milestone 3, the first place it can be verified. |
| Organization `Code` | Never built. The UI sketch showed one; the domain never had one. No feature has needed it. |
| Settings (time zone, culture) | Never built. Would be a value object on the aggregate, not a second aggregate. |
| Status filter on the directory | Retired organizations accumulate from every browser run. Harmless today — the applicant dropdown filters to active — but grows monotonically. |
| `AB-007` out-of-range enums | `Product.Register` still accepts them; three other contexts unaudited. |
| Two conventions for repository interfaces | Product puts them in `Application/Persistence`, Platform in `Domain/Aggregates`. Organization followed Product. Not worth reconciling yet. |
| Routes are inconsistent solution-wide | `/organizations` vs `/api/products` vs `/api/platform/users`. Organization matched its own existing endpoint. |

---

## What this milestone did not change

No new ADRs were written. Every decision above either followed an existing ADR
or was a domain choice recorded in a commit message — which was the intent when
architecture work was closed out before ORG-001.
