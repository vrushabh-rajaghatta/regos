# RegOS Foundation

The Foundation is `src/Organization` and `src/Platform` — the tenant boundary,
identity, authentication, authorization and audit that every Regulatory module
depends on.

## Documents

Separated by how often they change.

| Document | Answers | Changes |
|---|---|---|
| [`vision.md`](vision.md) | Why the Foundation exists; what belongs in it; where the boundaries are | Almost never |
| [`principles.md`](principles.md) | The rules — principles (judgement) and invariants (non-negotiable) | Occasionally |
| [`roadmap.md`](roadmap.md) | What remains to be built, per epic, against the actual codebase | Regularly |
| [`milestones/`](milestones/) | Engineering record of each closed milestone — decisions, defects, lessons | One per milestone |
| [`../adr/`](../adr/README.md) | What was decided, why, and when to reconsider | Append-only |

Also here: [`sprint-definition.md`](sprint-definition.md),
[`manifesto/`](manifesto/).

## Reading them correctly

Every architectural statement is one of three things, and mixing them is what
makes documentation untrustworthy:

- **Current** — true of the code today.
- **Accepted** — decided; implementation may be incomplete.
- **Proposed** — under discussion, binding on nobody.

`vision.md` and `principles.md` tag their claims. `roadmap.md` is entirely
current-state plus remaining work.

**When a document and the codebase disagree, the codebase wins** and the
document is stale (principle P5). Documents describe; ADRs decide.

## The two things most worth knowing

1. **Organization is both the tenant boundary and a regulatory party.** It knows
   `Manufacturer`, `Sponsor`, `MarketingAuthorizationHolder`. That is why it
   sits beside `src/Platform` rather than inside it, and why the Foundation is
   described as *minimizing* regulatory knowledge rather than containing none.
   [ADR-015](../adr/ADR-015-organization-is-the-tenant.md).

2. **RegOS is not yet tenant-isolated.** Platform enforces it; the regulatory
   domain has no tenant concept at all. The tenant is at least now *proven*
   rather than asserted — it comes from the caller's token
   ([ADR-024](../adr/ADR-024-tenancy-is-derived-from-identity.md)) — but a
   proven tenant that no regulatory handler consults isolates nothing.
   Closing that is the real content of Epic 1.
   [vision.md §4](vision.md#4-the-foundations-central-promise).
