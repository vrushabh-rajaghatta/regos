# Process

**What we are trying to achieve, what we intend to do about it, and by when.**
Regulatory Process is the layer that turns a record system into one that says
what to do next.

See [ADR-065](../adr/ADR-065-regulatory-process-is-an-optional-bounded-context.md)
for why it is its own context, why nothing requires it, and why a plan is pinned
to a published version forever.

## The word the domain uses, and the word the screen uses

| Domain | Screen | Why they differ |
|---|---|---|
| `ProcessDefinition` | **"Playbook"** | A user says *"publish the FDA IND playbook"*, never *"the process definition"*. The type does not say playbook, because **a playbook sounds like something you copy** — and this is something you *conform to*: versioned, published, immutable, and pinned to permanently ([ADR-065 I4](../adr/ADR-065-regulatory-process-is-an-optional-bounded-context.md)). |
| `ProcessDefinitionVersion` | **"Version"** | Unqualified on screen because the playbook is the only thing being versioned there. Qualified in the model because `ProcessPlan` also has versions of a sort, and *"which version?"* must never be ambiguous. |
| `ProcessStepDefinition` | **"Step"** | Same split. On a playbook screen every step is authored, so *"definition"* adds nothing; in the model it is what distinguishes an authored step from a live dated one. |

**Both are binding.** The screen's word must never reach a type, and the type's
word must never reach a label by default (CLAUDE.md).

**RIM's own word is deliberately not used.** RIM calls this object
`Process Plan Template`. RegOS adopts RIM's *questions and concepts, not
necessarily its object model* — the third time it has done so, after `Artwork`
(EPIC-018) and `PackAuthorisation` (EPIC-010b).

```
   template                              definition
   ─────────                             ──────────
   copy it        →  edit the copy       conform to it   →  pin the version
   copies diverge freely                 immutable once published
```

The lifecycle is the tell: as governance arrives, *Published Definition* and
*Superseded Definition* are what regulated systems call these artefacts, while
*Approved Template* reads as a mistake.

## The two halves, and why every noun keeps its prefix

| What we conform to | What we are doing |
|---|---|
| `ProcessDefinition` · `ProcessDefinitionVersion` · `ProcessStepDefinition` | `ProcessObjective` · `ProcessPlan` · `ProcessStep` |
| authored, versioned, frozen | live, dated, ours |

Never bare `Process`. Every aggregate says **which context** it belongs to *and*
**which role** it plays, and the symmetry is what makes the pairing legible.

> `RegOS.Process` shadows `System.Diagnostics.Process` for any code inside the
> `RegOS` root namespace. Two test files that shell out to `xmllint` now
> fully-qualify it. That is the whole cost, and it was known before the name was
> chosen.

## What exists today

**S001 only.** `ProcessDefinition`, its versions and their steps —
authored, publishable, seeded for US·FDA·IND, and readable. There is no
objective, no plan, no live step, and nothing attaches to anything.

`ProcessObjective` and `ProcessPlan` are **separate aggregates by decision**
([ADR-065 decision 3](../adr/ADR-065-regulatory-process-is-an-optional-bounded-context.md)),
on the argument that an objective is stateable with no schedule under it at all —
*FDA approval for Product X*, *CE MDR transition*, *expand an indication*,
*renew a licence*. **An objective is the goal; plans are attempts.**

`ProcessObjectiveGroup` is **refused**, not deferred by oversight: nobody asks
its question and RegOS holds no product with objectives in two markets. Revisit
when both are true.
