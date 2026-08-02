# Slice Conventions

**Status:** Active · **Effective:** 2026-07-31 ·
**Enforced by:** `tests/Architecture/RegOS.Architecture.Tests`

> The mechanical companion to [implementation-standards.md](implementation-standards.md).
>
> That document says *what* to build (capabilities, aggregates, intent). This
> one says *where the files go* and *what they are called*. It exists because
> principles did not stop the drift: five contexts answered "where does the
> repository interface live?" differently, and each was written by someone
> reasonable copying their nearest neighbour.
>
> Every rule below is checked by a test. Prose has already been tried.

---

## How to use this

Building a slice, in order:

1. Find the rule here. It gives a path and a filename.
2. Copy the **cited reference file** for that rule — not "a nearby file".
3. Run `dotnet test tests/Architecture/RegOS.Architecture.Tests`.

If a rule blocks something the slice genuinely needs, that is an ADR
conversation, not a local exception. Say so in the PR.

---

## The grandfathered lists

Each test carries a list of files that predate its rule. Those lists exist so
the conventions could be turned on without a repo-wide rewrite.

**They may shrink. They must never grow.**

An entry is a known inconsistency, not an approved exception. Retire entries
opportunistically — when you are already editing that slice — rather than in a
sweep. A companion test fails if an entry goes stale, so the lists cannot
quietly stop describing reality.

Adding a new entry to unblock new code defeats the entire mechanism. If you
believe you need to, that is the ADR conversation.

---

# Backend

## SC-001 — Every route lives under `/api`

```
/api/products/{id:guid}                     ✅
/api/organizations/{organizationId}/sites   ✅
/organizations/{organizationId}/sites       ❌
```

The prefix keeps the API in its own namespace on a host that also serves
OpenAPI, health checks and static files.

Route groups carry the prefix once:

```csharp
var tenants = app.MapGroup("/api/platform/tenants");
tenants.MapGet("/{id:guid}", GetAsync);
```

**Reference:** [Products/GetProductEndpoint.cs](../../src/Host/RegOS.Api/Endpoints/Products/GetProductEndpoint.cs) ·
**Test:** `RouteConventionTests`

> Moving a grandfathered route means changing the frontend caller in the same
> commit. Do the pair together or not at all.

---

## SC-002 — Repository interfaces live in the Domain project

```
src/<Context>/RegOS.<Context>.Domain/Aggregates/<Aggregate>/I<Aggregate>Repository.cs   ✅
src/<Context>/RegOS.<Context>.Application/Persistence/I<Aggregate>Repository.cs         ❌
```

The interface is part of the aggregate's contract; the implementation lives in
`Infrastructure`. This restates **[ADR-016](../adr/ADR-016-persistence-access-model.md)**,
which was already explicit and was violated twice after it was written.

Writes go through the repository. Reads use `RegOSDbContext` directly with
`AsNoTracking()` — a query handler never loads an aggregate.

**Reference:** [Registration/IRegistrationRepository.cs](../../src/Registration/RegOS.Registration.Domain/Aggregates/Registration/IRegistrationRepository.cs) ·
**Test:** `PersistenceConventionTests`

> **No exemptions remain.** The last grandfathered entry, `IProductRepository`,
> was retired in EPIC-017 S001 — opportunistically, while that slice was already
> adding a sibling interface to the same context.

---

## SC-003 — A query folder holds a query record

```
Queries/GetProduct/
├── GetProductQuery.cs       ← the question and its parameters
├── GetProductHandler.cs     ← the answer
└── ProductDetails.cs        ← the shape returned
```

Queries mirror commands. Passing loose parameters straight to `HandleAsync`
works until the third parameter is appended to a method signature and nobody
notices the query grew.

The intermediate grouping folders some contexts use (`Queries/Sites/`,
`Queries/Blueprint/`) are fine — the rule applies to the folder holding the
handler.

**Reference:** [Product/Queries/GetProduct/](../../src/Product/RegOS.Product.Application/Queries/GetProduct/) ·
**Test:** `QueryConventionTests`

> This is the majority-minority rule: most contexts written after Product
> dropped query records, so the grandfathered list is long. The convention
> still holds for new work — see the decision note at the end.

---

## SC-004 — Endpoint handlers are named methods

```csharp
// ✅ the route line reads as a table of contents
endpoints.MapGet("/api/products/{id:guid}", HandleAsync);

private static async Task<IResult> HandleAsync(
    Guid id, GetProductHandler handler, CancellationToken cancellationToken) { … }
```

```csharp
// ❌ path, verb and behaviour all buried in one expression
app.MapGet("/organization-sites/{siteId:guid}", async (
        Guid siteId, GetOrganizationSiteHandler handler, CancellationToken ct) =>
    { … });
```

A named method can be read, moved, and given a comment explaining the
capability. `HandleAsync` is the usual name; a file mapping several routes uses
verb names (`ListAsync`, `RevokeAsync`).

**Reference:** [Products/GetProductEndpoint.cs](../../src/Host/RegOS.Api/Endpoints/Products/GetProductEndpoint.cs) ·
**Test:** `EndpointConventionTests`

---

## SC-005 — One handler per file, named after it

```
Commands/CreateRegistration/
├── CreateRegistrationCommand.cs
├── CreateRegistrationHandler.cs
└── CreateRegistrationResult.cs
```

```
Commands/CreateContact/
└── CreateContact.cs            ❌ command and handler bundled

Queries/Contacts/
└── ContactQueries.cs           ❌ three handlers and two DTOs in one file
```

Knowing the capability's name should tell you the filename without opening
anything. Its opposite is a file named for its folder, accumulating handlers
until the context's surface is invisible.

This rule also keeps the others checkable: SC-003 and SC-004 are folder-scoped,
and a bundle file hides its contents from both.

**Reference:** [Registration/Commands/CreateRegistration/](../../src/Registration/RegOS.Registration.Application/Commands/CreateRegistration/) ·
**Test:** `SliceLayoutConventionTests`

---

## Backend slice shape, end to end

```
src/<Context>/
├── RegOS.<Context>.Domain/
│   └── Aggregates/<Aggregate>/
│       ├── <Aggregate>.cs              private ctor + Create() factory
│       ├── <Aggregate>Id.cs            : StronglyTypedId — ES-020, not a record struct
│       ├── <Aggregate>Errors.cs
│       ├── <Aggregate>Status.cs
│       └── I<Aggregate>Repository.cs   SC-002
│
├── RegOS.<Context>.Application/
│   ├── Commands/<Capability>/          SC-005
│   │   ├── <Capability>Command.cs
│   │   ├── <Capability>Handler.cs
│   │   └── <Capability>Result.cs       when the caller needs the new id
│   ├── Queries/<Capability>/           SC-003, SC-005
│   │   ├── <Capability>Query.cs
│   │   ├── <Capability>Handler.cs
│   │   └── <Shape>.cs
│   ├── Services/I<X>Policy.cs          cross-aggregate rules only
│   ├── <Context>RuleErrors.cs
│   └── DependencyInjection.cs
│
└── RegOS.<Context>.Infrastructure/
    ├── Repositories/<Aggregate>Repository.cs
    ├── Services/<X>Policy.cs
    └── DependencyInjection.cs
```

EF configuration and migrations live centrally in
[src/Persistence/RegOS.Persistence/](../../src/Persistence/RegOS.Persistence/),
not in the context — this is deliberate and predates the contexts.

Endpoints live in [src/Host/RegOS.Api/Endpoints/\<Plural\>/](../../src/Host/RegOS.Api/Endpoints/),
one file per endpoint, and are registered in
[Program.cs](../../src/Host/RegOS.Api/Program.cs) under a `MapGroup` per context.

---

# Frontend

Not yet enforced by tests. The conventions are just as real; they are checked
in review until a linter covers them.

## SC-101 — One file per API call

```
api/getProduct.ts        ✅   export async function getProduct(…)
api/listProducts.ts      ✅
api/registrations.ts     ❌   eight calls in one module
```

**Reference:** [products/api/](../../web/regos-web/src/features/regulatory/products/api/)

## SC-102 — One file per hook

```
hooks/useProduct.ts      ✅
hooks/useProducts.ts     ✅
hooks/useRegistrations.ts ❌  eight hooks in one module
```

The file name matches the exported hook. Query keys are namespaced by feature:
`["registrations", "product", productId]`.

**Reference:** [products/hooks/](../../web/regos-web/src/features/regulatory/products/hooks/)

## SC-103 — Every form has a zod schema in `validation/`

```
validation/registerProductSchema.ts   ✅
```

Applies to any surface collecting typed input. It does **not** apply to
pickers that only select an existing record (`AttachProductDocumentDialog`).

Server-side rules are not duplicated client-side — the schema covers shape and
required-ness, and the server's `detail` message is surfaced verbatim for
business refusals.

**Reference:** [products/validation/registerProductSchema.ts](../../web/regos-web/src/features/regulatory/products/validation/registerProductSchema.ts)

## SC-104 — Dialog and Form are separate components

```
components/RegisterProductDialog.tsx   ← shell: open state, title, mounting
components/RegisterProductForm.tsx     ← react-hook-form + zodResolver + fields
```

A dialog holding raw `useState` per field is the signal SC-103 was skipped.

**Reference:** [products/components/RegisterProductForm.tsx](../../web/regos-web/src/features/regulatory/products/components/RegisterProductForm.tsx)

## SC-105 — Non-component helpers leave `components/`

```
constants/productStatuses.ts     ✅   fixed reference lists
utils/formatFileSize.ts          ✅   pure functions
components/statusLabel.ts        ❌   not a component
```

## SC-106 — A failed mutation is visible, and distinguishable from a stale read

Every mutation hook's error state is rendered. A screen that shows a list and
offers actions on it has **two ways to look wrong and one appearance**:

```tsx
{remove.isError && (                                    // ✅
  <p className="text-sm text-destructive">{remove.error.message}</p>
)}
```

```tsx
<Button onClick={() => remove.mutate(role.id)}>Remove</Button>   // ❌ alone
```

Without it, a rejected mutation and a projection that has not refreshed look
identical — the row is simply still there. **The user cannot tell, and neither
can the developer**: EPIC-004 S005 spent a debugging cycle on refresh behaviour
before a direct API call revealed a 404.

The rule is not "better error handling". It is that **a mutation failure and a
stale view must never be the same pixels**, because they have opposite fixes.

## Frontend slice shape

```
web/regos-web/src/features/<area>/<feature>/
├── api/           one file per call            SC-101
├── components/    .tsx only                    SC-104, SC-105
├── constants/     fixed reference lists
├── hooks/         one file per hook            SC-102
├── layout/        workspace shell + nav        when the feature owns a workspace
├── pages/         route components
├── types/         one model per file
├── utils/         pure helpers
├── validation/    zod schemas                  SC-103
└── index.ts       the feature's public surface
```

Data flows one way: `Page → Components → Hooks → API → Backend`. A page never
calls `api/` directly.

---

# Decisions this document settles

These were open forks where two conventions coexisted. Recorded here so they
are not relitigated per slice.

| Fork | Decision | Losing side |
|---|---|---|
| Route prefix | `/api` on everything | the newer unprefixed routes |
| Repository interface | Domain project (ADR-016) | Product / Organization in Application |
| Query records | Required, one per query | the newer parameter-passing style |
| Endpoint handlers | Named methods | inline lambdas |
| Handler files | One per file, named after the class | bundled `<Thing>Queries.cs` |

Two of these went to the **minority** convention — query records and the `/api`
prefix are both outnumbered in today's code. That is deliberate: the older
convention was the considered one, and the newer variants arrived by copying,
not by decision. The grandfathered lists carry the cost.

Folder-naming variants left unresolved on purpose, per
**[ADR-018](../adr/ADR-018-rule-of-three.md)** — churn without demonstrated
need: `Domain/Aggregates/<X>/` vs `Domain/<X>/`, `Infrastructure/Repositories/`
vs `Infrastructure/Persistence/`, `DependencyInjection.cs` vs
`DependencyInjection/<X>ServiceCollectionExtensions.cs`. New contexts should
follow the left-hand form.

---

# Change History

| Version | Date | Summary |
|---|---|---|
| 1.1 | 2026-07-31 | SC-002's grandfathered list emptied (EPIC-017 S001). |
| 1.0 | 2026-07-31 | Initial version. Five backend rules put under test; five frontend rules documented. |
