# ADR-041 — Platform Contracts, And The Only Identity That Crosses

**Status:** Accepted · **Date:** 2026-08-01 ·
**Related:** [ADR-017](ADR-017-shared-kernel-scope.md) (kernel scope — the rule this turns on),
[ADR-030](ADR-030-tenant-is-its-own-aggregate.md) (`TenantId` in the kernel, and why),
[ADR-040](ADR-040-the-health-authority-interaction-context.md) (the Interaction context),
[ADR-016](ADR-016-persistence-access-model.md),
[ADR-018](ADR-018-rule-of-three.md)

## Context

EPIC-006 S004 needs to answer *"what is due, **to whom**, and when?"* — the
epic's own Outcome sentence. A due view that cannot answer **"mine"** fails that
purpose, and *"mine"* can only be resolved against the authenticated identity.

So a regulatory aggregate must hold the identity of one of our people. Until now
**no regulatory context has referenced `Platform.Domain`** — eight have kept that
boundary, and `Contact`'s own remarks warn against dragging Platform identity
into the regulatory domain, which is why a `Contact` is deliberately not a
`User`.

This is the first decision in EPIC-006 that **expands** the architecture rather
than reducing it, and it was held to a correspondingly higher standard.

## Decision

### 1. The assignee is a user, and the workflow says so

Not asserted from the type system — walked:

| The user does | Which requires the owner to be |
|---|---|
| chases whoever owns Q3 | a name |
| *"what's due for **me**?"* | **comparable to the authenticated identity** |
| *"what's unassigned?"* | absent — nullable |

The counter-cases do not survive. An outsourced consultant with no login cannot
be chased *by RegOS*; the RA lead who manages them owns it, and is a user. A
group mailbox and an unassigned queue are both **the absence of an owner**,
which `null` already expresses.

**Contributors are deferred.** RIM has *response lead + contributors*; the
workflow asked for a lead. RIM's plural is not evidence.

### 2. A new project, `RegOS.Platform.Contracts`, holds `UserId` and nothing else

```
Platform.Domain ──owns──▶ Platform.Contracts.UserId ◀──references── Interaction.Domain
                                                    ◀────────────── (any context that needs it)
```

The reference surface is **exactly one type**. There is no path from a
regulatory context to `User`, `UserCredential`, `Session` or `Invitation`.

### 3. Not the shared kernel — and this is the load-bearing part

Moving `UserId` into `RegOS.SharedKernel` looks like the `TenantId` precedent and
is not.

[ADR-017](ADR-017-shared-kernel-scope.md) rule 2: **the kernel must not know a
bounded context.** [ADR-030](ADR-030-tenant-is-its-own-aggregate.md) could move
`TenantId` only because it argued the tenant had *stopped being* one context's
concept — *"the tenant is an infrastructure concept every context shares"*, and
every aggregate in the system is tenant-scoped.

**`UserId` has not crossed that threshold.** A user is not an intrinsic property
of every domain concept; it is the identity of an aggregate that still plainly
belongs to Platform. The honest question is:

> *Is user identity now a cross-cutting domain concept, or are we only avoiding
> a project reference?*

Today it is the second. So **ownership stays with Platform** and only the
minimum contract crosses — which is the rule this codebase has followed
throughout: *concepts live where they are owned; only the minimum another
context needs crosses the boundary.*

### 4. Not a direct `Interaction.Domain → Platform.Domain` edge

That would be the first true domain-to-domain dependency in the solution. Once
one domain references another's aggregate model, the next reference is much
easier to justify, and the boundary erodes by precedent rather than by decision.

### 5. Not a new `Assignee` concept

Inventing one because Platform feels far away is the same move this epic's
process has already refused four times — `AuthorityInteraction`,
`CorrespondenceDocument`, reusing `OrganizationDivision`, widening
`OrganizationType`. **A placeholder abstraction is still speculation.**

### 6. What may enter `Platform.Contracts` later

Only identities other contexts must *hold*, never behaviour and never data.
`TenantId` stays in the kernel (ADR-030). If a second identity is proposed,
re-run decision 3's question before adding it — the project exists to be a
narrow contract, not a second kernel.

## Consequences

- `UserId` moves from `RegOS.Platform.Domain.Aggregates.User` to
  `RegOS.Platform.Contracts`. **93 files mention it, 74 inside Platform**; every
  change is an added `using`, and every miss is a compile error. Landed as its
  own commit, separate from S004's feature work.
- `HaQuestion.OwnerUserId` and `Commitment.OwnerUserId` are **nullable** — an
  unassigned question is the normal state of a letter that has just arrived.
- Regulatory contexts hold the id and **never navigate to a user** (ES-014).
  Rendering a name is a read model's job.
- `Contact` is still not a `User`, and the distinction is now sharper rather
  than blurred: a contact is a person at another company or an authority; an
  owner is one of ours.

## Revisit When

- **A second regulatory context needs a second Platform identity.** That is when
  decision 3's question gets asked again, and possibly answered differently — if
  user identity really has become cross-cutting, the kernel is where it belongs
  and this project was a waystation.
- **Someone asks who else worked on a response.** Then RIM's *contributors* has
  earned itself, and the single owner becomes a collection.
- **An owner needs to be someone without a RegOS login.** Decision 1's workflow
  analysis would be falsified, and the answer is probably a `Contact`, not a
  widened `UserId`.
- **`Platform.Contracts` accumulates a third type.** Two is a contract; three is
  the beginning of a second kernel, and ADR-017's warning about accumulation
  applies here exactly as it does there.
