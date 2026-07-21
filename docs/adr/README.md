# Architecture Decision Records

`docs/adr/` is the canonical, single source of truth for RegOS architecture
decisions. Every decision has a unique, monotonically increasing number that
never changes meaning once assigned.

## Rules

1. **Numbers are immutable.** Once ADR-N means something, it means that forever.
   A reversed decision produces a *new* ADR that supersedes the old one; the old
   file stays, with its status changed.
2. **One decision per file.** No aggregate documents.
3. **Append-only.** Edit an ADR to correct a broken reference or change its
   status — never to change what was decided.
4. **New ADRs take the next free number.** Today that is **ADR-024**.
5. **Cite by number in code and commits.** `// ADR-009` in a source file is the
   reason the ADR must never be renumbered.

## Status vocabulary

| Status | Meaning |
|---|---|
| **Accepted** | Decided and in force. Implementation may be incomplete — the ADR says so where that is true. |
| **Proposed** | Under discussion. Not binding. Do not implement against it. |
| **Superseded** | Replaced by a later ADR. Retained as history; never cite as guidance. |
| **Deprecated** | No longer recommended, with no replacement. |

An ADR is Accepted because it was agreed, not because it was typed.

## Index

| ID | Decision | Status | Kind |
|---|---|---|---|
| [001](ADR-001-modular-architecture.md) | Modular architecture | Accepted | Direction |
| [002](ADR-002-cqrs.md) | CQRS | Accepted | Direction |
| [003](ADR-003-no-mediatr.md) | No MediatR | Accepted | Direction |
| [004](ADR-004-explicit-dependency-injection.md) | Explicit dependency injection | Accepted | Direction |
| [005](ADR-005-dbcontext-usage.md) | DbContext usage | **Superseded** by 016 | — |
| [006](ADR-006-read-model-composition.md) | Read model composition | Accepted | Direction |
| [007](ADR-007-module-ownership.md) | Module ownership | Accepted | Direction |
| [008](ADR-008-composition-modules.md) | Composition modules | Accepted | Direction |
| [009](ADR-009-command-validation-model.md) | Command validation model | Accepted | Current |
| [010](ADR-010-documentation-as-code.md) | Documentation as code | Accepted | Direction |
| [011](ADR-011-development-lifecycle.md) | Development lifecycle | Accepted | Direction |
| [012](ADR-012-shared-semantic-exception-model.md) | Shared semantic exception model | Accepted | Current |
| [013](ADR-013-ambient-tenant-context.md) | Tenant context is ambient | Accepted | Current (partial) |
| [014](ADR-014-invitation-is-a-user-status.md) | Invitation is a user status | Accepted | Current |
| [015](ADR-015-organization-is-the-tenant.md) | Organization is the tenant | Accepted | Current |
| [016](ADR-016-persistence-access-model.md) | Repositories for writes, DbContext for reads | Accepted | Current |
| [017](ADR-017-shared-kernel-scope.md) | Shared kernel scope | Accepted | Current |
| [018](ADR-018-rule-of-three.md) | Duplicate twice, abstract on the third | Accepted | Current |
| [019](ADR-019-testing-strategy.md) | Testing strategy | Accepted | Current |
| [020](ADR-020-ef-read-model-strategy.md) | EF Core read model strategy | Accepted | Current |
| [021](ADR-021-email-is-globally-unique.md) | An email address identifies exactly one user | Accepted | Current |
| [022](ADR-022-authentication-failure-is-a-fourth-exception.md) | Authentication failure is a fourth exception | Accepted | Current |
| [023](ADR-023-satellite-aggregate-lifetime.md) | A satellite aggregate's lifetime is enforced by the database | Accepted | Current |

**Kind** distinguishes the three truths every architecture record mixes up:
*Current* — describes the code as it is today. *Direction* — decided, possibly
not fully implemented. *Proposed* — under discussion, binding on nobody.

ADR-013 is marked *Current (partial)* deliberately: it is fully implemented in
Platform and not implemented at all in the regulatory contexts. Read its
**Scope Limit** section before citing it.

---

## The 2026-07-20 reconciliation

Before this date RegOS had two conflicting ADR series. The same numbers meant
different things depending on which file you opened, and **both series were
cited in source code**.

| ID | `09-technology-decisions.md` | `docs/adr/` |
|---|---|---|
| 007 | Module Ownership | "shared exception contract" |
| 008 | Composition Modules | "tenant context" |
| 009 | Development Lifecycle | Command Validation Model |

### How each collision was resolved

**ADR-009 → Command Validation Model.** Decided by source citations, not
preference. Four files cite ADR-009 meaning command validation
(`StronglyTypedId.cs:24`, `ProductPolicy.cs:34`,
`PublishSubmissionHandler.cs:13`, `PublishSubmissionTests.cs:221`). Nothing
cites "ADR-009 — Development Lifecycle". The uncited document moved to
**ADR-011**.

**ADR-007 and ADR-008 → unchanged.** Module Ownership and Composition Modules
were written, published ADRs. The "shared exception contract" and "tenant
context" decisions were real and shipped but had *never been written down* —
they had a claim to those numbers only inside ADR-009's prose. Written ADRs keep
their numbers; the two undocumented decisions were written up at the next free
numbers, **ADR-012** and **ADR-013**, and ADR-009's references were corrected.

**ADR-006 → unchanged.** Cited in `IProductRepository.cs:9` with the meaning it
already had. No conflict.

`docs/architecture/09-technology-decisions.md` is retired; each of its decisions
now lives in its own file.

### Decisions found in code but never recorded

The reconciliation surfaced three decisions the codebase had already made:

- **ADR-014** — Invitation is a `User` status, not an aggregate.
- **ADR-015** — Organization is the tenant.
- **ADR-016** — Repositories for writes, `DbContext` for reads. This one
  *contradicted* ADR-005, which had rejected repositories; ADR-005 is now
  superseded.

---

## Unresolved references

Citations whose provenance cannot be established. **Architecture documentation
does not fabricate history** — an unresolvable reference is recorded as unknown
rather than guessed at.

### `ADR-0002`

**Referenced by**
- `docs/capabilities/register-product.md:21`
- `docs/domain-model/product.md:21`

**Meaning**
Unknown. The four-digit format belongs to neither historical series. ADR-002
(CQRS) is the likeliest reading, but nothing corroborates it and both citing
documents concern product registration, where CQRS is not an obviously relevant
decision.

**Action**
Resolve before any further ADR renumbering. Whoever wrote these documents is the
cheapest source of truth. Once resolved, correct both citations and delete this
entry.
