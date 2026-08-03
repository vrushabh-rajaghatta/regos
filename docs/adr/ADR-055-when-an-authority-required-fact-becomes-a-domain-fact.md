# ADR-055 — When An Authority-Required Fact Becomes A Domain Fact

**Status:** Accepted · **Date:** 2026-08-03 ·
**Related:** [ADR-048](ADR-048-the-people-on-a-filing-belong-to-the-filing.md) (contact roles stay ours, translated at the boundary),
[ADR-053](ADR-053-instance-qualifiers-belong-to-the-placement.md) (instance qualifiers — the case this rule explains),
[ADR-054](ADR-054-a-study-tagging-file-is-a-projection-over-a-study.md) (a `file-tag` is one, and a `Study` is not),
[ADR-051](ADR-051-two-more-lookups-and-what-a-lookup-is.md) (`Token` — what a wire value looks like when it stays at the boundary),
[E30, E31, E32](../evidence/README.md)

## Context

EPIC-007a has made the same decision six times without ever naming the rule.

Each time, an external specification demanded a value RegOS did not hold, and
each time the answer was different: a wire token on a lookup, a translation in a
renderer, a refusal by name, a new ADR, and — once — **a new property on an
aggregate in another bounded context.** From the outside those look arbitrary.

The trigger was `telephone-number-type`. FDA's regional DTD makes
`telephone-number-type` `#REQUIRED` on a `<telephone>` that is itself mandatory
(E30), so no `us-regional.xml` could be written without one. RegOS's
`ContactPhone` was a bare number. **The obvious move — carry FDA's token — is
the wrong one**, and the reason why is the rule this ADR records.

## Decision

> **An authority-required fact is promoted into the domain model only when it is
> an ordinary business concept that would exist if the authority did not.
> Otherwise it stays at the boundary, as a translation or a refusal.**

The test is a question, and it is asked about the *concept*, never about the
*format*:

> **Would a regulatory professional ask this, in these words, if no authority
> had ever demanded an answer?**

For a phone number, yes — *"is that your office line, your fax, or your
mobile?"* is the obvious next question about any number a registry holds, and
`ContactPhone` was under-modelled without it. FDA's list happens to enumerate
exactly those three. **The coincidence is the world's, not the wire's.**

For `applicant-contact-type`, no. Nobody asks *"is this person an `fdaact1`?"*
They ask *"who is the regulatory contact?"* — which RegOS already answers with
`ContactRole`. The format's taxonomy is a second, differently-drawn answer to a
question we have already modelled, and adopting it would give one fact two
homes.

### The rule applied to this epic, in full

| The authority requires | Ordinary business concept? | Where it lives |
|---|---|---|
| **phone kind** (`telephone-number-type`) | **yes** — asked of every number, regardless of who is asking | ✅ **`ContactPhone.Kind`** — a domain enum, translated in the renderer |
| **DUNS number** (`applicant-info/id`) | **yes** — a company's registry identifier | ✅ **already modelled** as `Organization.Identifiers`, scheme `DUNS`. It had been there all along |
| **which study a document reports** (STF) | **yes** — a study is a thing in the world that documents are *about* | ✅ [ADR-054](ADR-054-a-study-tagging-file-is-a-projection-over-a-study.md) — and it gets its own ADR, because *where* it lives is a bounded-context question |
| **contact role** (`applicant-contact-type`) | **no** — `ContactRole` already answers this question, differently | ⛔ **boundary translation**, `REG → fdaact1` and nothing else (ADR-048, E31) |
| **the wire codes themselves** (`fdaat4`, `fdast1`) | **no** — an opaque token means nothing to anyone but the format | ⛔ **`Token` on reference data** (ADR-051) — stored beside the business value, never instead of it |
| **instance qualifiers** (substance, manufacturer, indication) | **not yet decidable** — they *may* be, and two of the three vocabularies are files we do not hold | ⛔ **refuse by name** until the third case can be expressed (ADR-053) |
| **`file-tag`** (what role a document plays in a study report) | **probably not** — it reads as generated metadata over a placement | ⛔ **refuse**, and revisit with ADR-054's third shape |

### Two corollaries, and both have already been paid for

**1. A promoted fact is modelled in the domain's words, not the format's.**
`ContactPhone.Kind` is `Business | Fax | Mobile`, not `fdatnt1 | fdatnt2 |
fdatnt3`, and it carries **no token column**. The one-to-one correspondence is a
convenience at the boundary today and may not survive the second authority —
which is exactly what a token column would freeze in place.

**2. A promoted fact is nullable where history has no answer**, and the null
means *recorded before RegOS asked* rather than *unknown*. This is the third time
(`SubmissionSubTypeId`, `Token`, now `Kind`), and it is what keeps the migration
honest: a default would assert, for every row already stored, an answer nobody
was ever offered the chance to give.

**A null of this kind produces a data-completeness refusal**, which is a fourth
sentence beside the epic's other three — *someone using the system can fix this
by answering* — and not a statement about our history, the authority's
vocabulary, or a gap in the model.

## Consequences

**This ADR does not license reaching into other contexts.** `ContactPhone.Kind`
is a change to `Organization` made from an eCTD story, and it is legitimate
because the fact is Organization's — the specification only revealed that it was
missing. **The test is whether the concept belongs there without the
specification**, and if the honest answer is *"only FDA wants this"*, the rule
forbids the change rather than permitting it.

**The epic gets a fourth refusal category** — data completeness — which the S004
table anticipated for a missing application number and now has a second
occupant.

**It explains a decision already taken and does not reopen it.** ADR-048's
contact roles, ADR-051's `Token`, ADR-053's refusals and ADR-054's `Study` were
each decided on their own merits before this rule existed. That they all satisfy
it is the evidence for it; **if a future case satisfies the rule and still feels
wrong, the rule is what to doubt**, not the case.

## Revisit when

- **A second authority's phone taxonomy is read.** If it does not decompose to
  office/fax/mobile, corollary 1 is under test: either the domain enum is the
  wrong shape, or the boundary translation stops being one-to-one, and only one
  of those is a problem.
- **The `file-tag` question is answered.** It is the row above marked *probably
  not*, and the only one where this ADR is guessing rather than deciding.
- **A promoted fact turns out to be needed by exactly one authority.** That is
  the falsifier: it would mean the question *"would anyone ask this anyway?"* was
  answered by imagination rather than by anyone actually asking it.
