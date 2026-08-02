# RegOS

A metadata-driven Regulatory Operating System. Regulatory knowledge is
versioned **data**, not code — the value is the template and rule engine, not
any one screen. First vertical: pharma, US·FDA·IND (CTD).

.NET 10 · minimal APIs · EF Core / PostgreSQL · React + Vite + TanStack Query.

---

## Before writing code

**Read [docs/engineering/slice-conventions.md](docs/engineering/slice-conventions.md).**
It says where files go and what they are called, and every backend rule in it
is enforced by a test.

Do not infer conventions by copying the nearest file. That is how this codebase
ended up with five contexts answering the same question five different ways.
The conventions doc cites a specific reference file per rule — copy that one.

Then run:

```bash
dotnet test tests/Architecture/RegOS.Architecture.Tests
```

If it fails, the slice is not finished. **Never** add an entry to a
grandfathered list to make new code pass — those lists are shrink-only, and
adding to one defeats the mechanism entirely.

---

## The rules most often broken

| | Rule | Not this |
|---|---|---|
| SC-001 | Every route starts `/api` | `/organizations/{id}/sites` |
| SC-002 | `I<X>Repository` in the **Domain** project (ADR-016) | in `Application/Persistence/` |
| SC-003 | Query folder holds `<Name>Query.cs` | loose params on `HandleAsync` |
| SC-004 | Endpoint handler is a named static method | inline `async (…) =>` lambda |
| SC-005 | One handler per file, named after it | `ContactQueries.cs` with three |

Frontend (reviewed, not yet linted): one file per API call, one file per hook,
a zod schema in `validation/` for every form, `Dialog` and `Form` as separate
components.

---

## Architecture canon

Read in this order when the question is "how should this work":

1. **[docs/adr/](docs/adr/)** — the single immutable decision series, ADR-001
   onward. Next number is **ADR-052**. Never edit an accepted ADR; supersede it.
2. **[docs/engineering/slice-conventions.md](docs/engineering/slice-conventions.md)** — mechanical file/folder rules.
3. **[docs/engineering/implementation-standards.md](docs/engineering/implementation-standards.md)** — principles behind them.
4. **[docs/ENGINEERING_STANDARDS.md](docs/ENGINEERING_STANDARDS.md)** — cross-cutting platform standards (ES-001…).

Where code and docs disagree, **the code is the truth** — then fix the doc in
the same PR.

**[docs/evidence/](docs/evidence/README.md) is not part of that series and does
not answer "how should this work".** It records **facts that came from outside
RegOS** — a regulator's DTD, a published example, a parser's verdict — each with
an evidence level and the decisions relying on it. An ADR is ours and changes
when we change our minds; an external fact can simply be *wrong*, and then every
decision resting on it has to be re-examined. Cite an evidence level (2a, 3, …)
rather than restating what a specification says.

### Decisions you will otherwise re-derive

- **[ADR-016](docs/adr/ADR-016-persistence-access-model.md)** — repositories for writes, `RegOSDbContext` + `AsNoTracking()` for reads. A query handler never loads an aggregate.
- **[ADR-030](docs/adr/ADR-030-tenant-is-its-own-aggregate.md)** / **[ADR-032](docs/adr/ADR-032-organizations-are-tenant-owned.md)** — `Tenant` (Platform) is not `Organization` (regulatory party).
- **[ADR-031](docs/adr/ADR-031-tenant-isolation-by-query-filters.md)** — tenant isolation is fail-closed EF query filters. Any entity with a `TenantId` and no filter fails `TenantFilterArchitectureTests`.
- **[ADR-018](docs/adr/ADR-018-rule-of-three.md)** — duplicate twice, abstract on the third *demonstrated* need. Symmetry with another module is not a demonstration. This forbids speculative deletion as much as speculative creation.
- **[ADR-012](docs/adr/ADR-012-shared-semantic-exception-model.md)** / **[ADR-022](docs/adr/ADR-022-authentication-failure-is-a-fourth-exception.md)** — semantic exceptions map to status codes in middleware. Endpoints do not catch.

---

## Aggregates

Frozen shape — private constructor, static `Create()` factory (never
`Register()`, ES-004), behaviour methods, no public setters. Aggregates
reference each other **by id only** — no navigation properties (ES-014).

Identity is `sealed class <X>Id : StronglyTypedId`, and the entity inherits
`AggregateRoot<TId>` or `Entity<TId>` (ES-020, ADR-043). "Strongly typed" is not
sufficient — `readonly record struct <X>Id(Guid Value)` cannot satisfy the
`Entity<TId>` constraint, so those entities have no base class, no identity
equality, and no empty-guid guard. Copy
[CommitmentId.cs](src/Interaction/RegOS.Interaction.Domain/Commitments/CommitmentId.cs),
never the nearest id — 15 record-struct ids are still pending migration
(`RegistrationId`, all of Blueprint, `ProductDocument`, every `*StatusEntry`) and
copying one propagates it. `IdentityConventionTests` enforces this, and carries
the one step of the conversion the compiler cannot find: a **shadow foreign key
declared with the id type becomes optional** once that id is a reference type,
and an optional FK severs instead of deleting.

The exception is **flat master data** (`CountryId`, `AuthorityId`,
`DocumentTypeId` and five more): deterministic ids, no children, no lifecycle,
no `Entity<TId>`. They keep record structs permanently (ADR-043 §2). This is by
shape, not by context — Blueprint lives in ReferenceData and is a real aggregate.

Lifecycle over deletion (ES-018): entities move `Active ↔ Inactive` rather than
being removed. Regulatory records are retained.

---

## Layout

```
src/<Context>/RegOS.<Context>.{Domain,Application,Infrastructure}
src/Host/RegOS.Api                  minimal API host, endpoints + Program.cs
src/Persistence/RegOS.Persistence   RegOSDbContext, all EF config + migrations
src/Shared/RegOS.SharedKernel       ADR-017 scope only
web/regos-web                       React frontend, feature-first
tests/                              mirrors src/, plus tests/Architecture/
```

Persistence is centralised on purpose — EF configuration for every context
lives in `RegOS.Persistence`, not beside the aggregate.

Contexts today: Platform · Organization · Product · ProductDocument ·
RegulatoryApplication · Submission · Registration · Interaction · ReferenceData.

---

## Working agreements

- **One story at a time**, delivered as a vertical slice — domain through API
  through UI. Flow is `docs/product/BACKLOG.md` → epic → story → PR.
- **Use the ubiquitous language.** `Registration`, `Submission`,
  `RegulatoryApplication` — never `Record`, `Item`, `Data`.
- **The domain's word and the screen's word may differ, and both are binding.**
  `MedicinalProduct` is the aggregate; **"Market"** is what the UI calls it.
  RIM's vocabulary keeps the model precise; the screen uses the word a
  regulatory user would say out loud. Where they differ, record the pair in
  [docs/domain-model/](docs/domain-model/) — and never let the screen's word
  reach a type, or the type's word reach a label by default.
- Generic folders (`Common`, `Shared`, `Helpers`, `Utils`, `Misc`) are
  prohibited in `src/` without an ADR (repository.md Standard 4).
- New bounded context, new cross-context dependency, or a change to an accepted
  decision → **ADR first**.
- Commit only when asked. Branch before committing if on `main`.

## Commands

```bash
dotnet build RegOS.slnx
dotnet test RegOS.slnx
dotnet test tests/Architecture/RegOS.Architecture.Tests   # conventions

cd web/regos-web && npm run dev
npm run build && npm run lint
```
