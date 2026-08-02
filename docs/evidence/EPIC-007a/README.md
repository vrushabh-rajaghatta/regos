# EPIC-007a — external validation evidence

**Decisions are backed by artifacts, not memories.** This directory holds the
evidence that an independent validator accepted a RegOS-generated package —
not a claim that it did.

An epic that says *"the validator passed"* and keeps nothing has produced a
memory. One that keeps the report, the tool version and the exact package that
was checked has produced an **architectural asset**: a later reader can see what
was validated, by what, and when, and can tell whether it still holds.

---

## Task 1 — the oracle decision record

| | |
|---|---|
| **Chosen oracle** | LORENZ eValidator Basic |
| **Target profile** | US eCTD 3.2 (FDA) |
| **Independent of RegOS** | **Yes** — separate vendor, separate implementation, validating against FDA's own published criteria |
| **Automatable** | **Not initially.** Manual invocation, documented so it is reproducible |
| **Licence assumption** | the free single-user edition is sufficient for development |
| **Known limitations** | one validation profile; reduced feature set; **not part of production architecture** |
| **Decided** | 2026-08-02, by the founder |

### Why this one

It gives four-way separation, which is the whole point:

| | |
|---|---|
| the specification | FDA, published |
| the validation criteria | FDA, published |
| the implementation that checks | **LORENZ — not us** |
| the package under test | RegOS |

FDA documents that its submission standards are validated using eValidator and
publishes the corresponding criteria and technical conformance guides. That
makes the oracle's judgement traceable to the regulator's own rules rather than
to a vendor's opinion — which is what
[the epic's principle](../../product/epics/EPIC-007a-ectd-package-generation.md)
requires:

> **The validator is an oracle, not a dependency.** It challenges our
> interpretation and never defines it.

### Still to be confirmed on install — and each one can fail Task 1

The table above records a **decision**, not a verified capability. The
following are assumptions until the tool is actually in hand, and the epic said
plainly that Task 1 is allowed to fail:

- [ ] the Basic edition is obtainable under its current licence terms
- [ ] it runs in an environment we have (**it is Windows software; the
      development machine is macOS**)
- [ ] the US eCTD 3.2 profile is included in the free edition
- [ ] it will validate a package of the shape EPIC-007a produces

> **If the free edition cannot validate the package we need, that is the
> evidence of failure — not an assumption to work around.** The epic's response
> is to say so, drop the claim to Level 1, and reconsider the priority call.

---

## Task 2 — the specification version, pinned

> **EPIC-007a targets FDA eCTD v3.2.2.**

FDA currently supports both v3.2.2 and v4.0. v3.2.2 remains the common case and
is the natural fit for the IND work EPIC-004 modelled.

**v4.0 is a later capability, not an accidental side effect of package
generation.** Supporting both here would double the surface before a single
package has ever been validated, and would make a failure ambiguous — we would
not know which target we had got wrong.

---

## What must land here before the epic can claim Level 2

| Artifact | Why |
|---|---|
| `validator-report.*` | the oracle's actual output, not a summary of it |
| `validator-version.txt` | tool name, edition, version, profile — a report means nothing without what produced it |
| `package.zip` | **the exact package that was checked**, so the report can be re-run against it |
| `how-to-reproduce.md` | the manual invocation, step by step |

**The acceptance rule:** the epic may claim independent validation only when a
report in this directory corresponds to a package in this directory, produced by
a tool version named in this directory. Anything less is Level 1 wearing Level
2's clothes.
