# EPIC-007a — external validation evidence

**Decisions are backed by artifacts, not memories.** This directory holds the
evidence that something outside RegOS checked a RegOS-generated package — not a
claim that it did.

An epic that says *"it validated"* and keeps nothing has produced a memory. One
that keeps the report, the tool version and the exact package that was checked
has produced an **architectural asset**: a later reader can see what was
validated, by what, and when, and can tell whether it still holds.

---

## Task 1 — the oracle. Outcome: **partly failed, and replaced.**

### What was decided, and did not survive

| | |
|---|---|
| **Candidate oracle** | LORENZ eValidator Basic, US eCTD 3.2 profile |
| **Outcome** | ✖ **not obtainable** — commercial tooling, Windows-only, and no licence available to this project |
| **Decided** | 2026-08-02 |

The epic said Task 1 was allowed to fail and that the honest response was to say
so rather than describe self-validation as external evidence. **This is that
failure, recorded rather than worked around.**

### What replaced it

Failing to obtain eValidator does **not** collapse the epic to Level 1, because
the founder supplied the primary sources on 2026-08-02 — including the actual
`us-regional-v3-3.dtd`. That splits the old Level 2 into two, and only the
second half is blocked:

| | Oracle | Reachable now? |
|---|---|---|
| **2a — structural** | FDA's published DTD, checked by **any third-party XML parser** (`xmllint`, .NET `XmlReaderSettings.DtdProcessing`) | ✅ **yes** — free, offline, no licence |
| **2b — business rules** | eValidator's FDA validation criteria | ✖ **no** — needs the commercial tool |

**Level 2a is genuine external evidence.** The specification is FDA's, the DTD is
FDA's, and the implementation doing the checking is a standard parser that knows
nothing about RegOS. It is not the same as 2b — a package can be perfectly
DTD-valid and still break FDA business rules — and this directory will never
claim otherwise.

**Level 3 also became reachable.** FDA publishes complete `us-regional.xml`
examples, including two IND sequences and their amendment (#21–#24). Comparing
our output against those is convention evidence that needs no tool at all.

> **The principle is unchanged, and now cheaper to honour.** *The validator is an
> oracle, not a dependency.* A DTD held in `spec/` and read by a parser we do not
> own is the purest form of that: it can only ever tell us we are wrong.

### Carried to EPIC-007b

**2b — FDA business-rule validation.** The trigger is a licence becoming
available, or a customer engagement that supplies one. Until then no document in
this repository may describe a RegOS package as *validated* without saying
against which of 2a or 2b.

---

## Task 2 — the specifications, pinned

> **ICH eCTD v3.2.2** (the `index.xml` backbone) **and FDA us-regional DTD v3.3**
> (the Module 1 backbone).

Task 2 was recorded as one pin and was **incomplete**. The two backbones version
independently, and `submission-sub-type` — required on every sequence — exists
only from regional v3.3. FDA's current pairing is eCTD 3.2.2 with regional 3.3,
which is what every worked example in their own document uses.

**v4.0 stays out.** Supporting both would double the surface before one package
has ever been checked, and would make a failure ambiguous — we would not know
which target we had got wrong.

The regional DTD is held at [`spec/us-regional-v3-3.dtd`](spec/us-regional-v3-3.dtd),
which is not a convenience: **every eCTD package must ship its DTDs inside
`util/dtd/`** (ICH Appendix 4, rows 372–376), so the file is a build input, not
just a reference.

---

## Task 3 — the mapping

[`ectd-mapping.md`](ectd-mapping.md) — element by element, with confidence marked
per row and the gaps ordered by how much of a package is impossible without them.

It found two defects in RegOS that only an external reference could find: the
seeded FDA IND blueprint mislabels section **1.13**, and RegOS numbers sequences
from **0000** where every FDA example starts at **0001**.

It also found that **`submission-id` groups sequences into a regulatory
activity** — which is EPIC-004's hypothesis 1, arriving from the US IND case
rather than the EU market that was predicted to settle it.

---

## What must land here before the epic claims anything

| Artifact | Claim it supports |
|---|---|
| `validator-report.*` + `validator-version.txt` | whichever of 2a / 2b produced it — **named explicitly** |
| `package.zip` | the exact package checked, so the report can be re-run |
| `how-to-reproduce.md` | the invocation, step by step |
| `comparison-to-fda-examples.md` | Level 3 — where we match FDA's published XML and where we differ |

**The acceptance rule:** a claim of external validation requires a report here
that corresponds to a package here, produced by a tool version named here, at a
level stated here. Anything less is Level 1 wearing Level 2's clothes.
