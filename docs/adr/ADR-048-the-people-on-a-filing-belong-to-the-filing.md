# ADR-048 — The People on a Filing Belong to the Filing

**Status:** Accepted · **Date:** 2026-08-02 ·
**Related:** [ADR-045](ADR-045-the-cumulative-dossier-and-the-derived-delta.md) (the cumulative dossier — this is a consequence of it),
[ADR-047](ADR-047-publication-metadata-exists-only-when-publication-makes-it-true.md) (frozen at publication),
[ADR-039](ADR-039-the-market-local-product-tier.md) (principle 7 — reads compose),
[ADR-038](ADR-038-organization-depth-roots-and-the-three-filter-shapes.md) (`Contact` is a root, referenced by id),
[ADR-018](ADR-018-rule-of-three.md)

## Context

EPIC-004 S005 names people on a submission — the regulatory contact, the
qualified person, the authorised representative. The role itself is additive.
**Where it lives is not**, and `Contact` already carries roles of its own, so
the obvious reading is that this is the same fact twice.

## Decision

### 1. Role assignments live on `Submission`, and nowhere else

There is **no `ApplicationContact`**, and its absence is the decision.

EPIC-016's `Contact` anticipated *"an application's QP"*, which would have put
the assignment on `RegulatoryApplication`. The cumulative model rules it out:

> Under [ADR-045](ADR-045-the-cumulative-dossier-and-the-derived-delta.md), **the
> latest published sequence *is* the current regulatory state.** An
> application-level contact and "the contact on the latest sequence" are the same
> regulatory fact stored twice, and two copies of one fact can only differ by one
> of them being stale.

That is the argument that removed `SubmissionSnapshot` in S002, applied to
people instead of documents. The symmetry is the point: in both cases the
temptation is to keep a convenient copy of current state, and in both cases the
cumulative model already supplies a single authoritative source.

### 2. An application's contacts are derived

`GetApplicationContactsHandler` reads the roles of the highest-numbered
published submission. **By sequence number, not by date** — number order is
transmission order by construction (ADR-044), so a backdated import cannot
become "current" by carrying a later timestamp.

**The cost, accepted knowingly:** an application that has published nothing has
no contacts, and *"who is the regulatory contact for this IND?"* has no answer.
That is the absence of a filing, not missing data. Recording who *will* be named
is planning (EPIC-020), not regulatory state.

### 3. Frozen at publication

Who was named on sequence 0003 is a fact about a filing already made. The draft
guard in `AssignRole` / `RemoveRole` is the whole mechanism — the same call
ADR-047 made for `Format`, and for the same reason: **a rule with two homes
grows two behaviours.**

### 4. Shared vocabulary, separate fact

`SubmissionRole` reuses `ContactRoleId`, the same reference data
`Contact.Roles` draws on. These are not the same fact:

| | Subject | Fact |
|---|---|---|
| `ContactRole` | — | **the vocabulary**: what roles exist |
| `Contact.Roles` | a person | what they are, in general |
| `SubmissionRole` | **a filing** | who was named on it, and as what |

**Reference data names concepts; aggregates record facts.** The same shape as
`SubmissionType` beside `Submission`, and `ContactRole` beside `Contact`.

It follows that **naming someone as Qualified Person does not require their
profile to list Qualified Person.** The two have different provenance: the
profile is organisational metadata maintained by whoever keeps the directory;
the submission is a historical record of what was declared to an authority. If
they disagree that is potentially interesting, and it is not invalid. Coupling
them would let a directory edit invalidate a filed sequence.

### 5. `Submission.Domain` references `Organization.Domain` directly

A new cross-context edge, and **not a new kind of edge**:
`Interaction.Domain` already references `Organization.Domain` for
`OrganizationSiteId`.

No `Organization.Contracts` project is created for one identifier. The
`Platform.Contracts` precedent does not apply — it exists because
authentication is deliberately *not* part of the regulatory model, and
`Contact` says so in as many words. Organization **is** the regulatory model.

Names are still read by composition (ADR-039 principle 7): the naming holds two
ids, and the person's name, their company and the role's name are read through
the aggregates that own them. The Submission context claims none of those facts.

### 6. No carry-forward — yet, and deliberately not here

Most sequences will name the same people as the one before, so inheriting them
would be closer to *remembering regulatory state* than to planning. It is still
not built in S005.

> **Carry-forward is its own capability, identified as such in S002.** Adding it
> for contacts only would leave RegOS with two independent carry-forward
> mechanisms and two mental models.

When general carry-forward arrives, contacts participate in the same mechanism
as documents.

## Consequences

- One assignment per `(person, role)` per submission — naming the same person as
  the same thing twice says it twice, not doubly. The same call
  `Contact.AddRole` already made.
- **An inactive contact cannot be named on a new filing**, which is exactly what
  deactivation means (EPIC-016). Existing namings on filed sequences are
  untouched.
- The foreign keys to `Contacts` and `ContactRoles` are **`Restrict`**. Contacts
  are retired rather than deleted (ES-018), so it should never fire — and if it
  does, losing the record of who was named would be worse than the failure.
- A submission may name nobody. Unusual, not invalid, and nothing in the model
  requires a role to be present to publish.

## Revisit When

- **General carry-forward arrives.** Decision 6 expires, and contacts join it
  alongside documents.
- **Someone proposes `ApplicationContact`.** Decision 1 is the answer, and the
  test is whether the proposed fact can differ from the latest published
  sequence's without one of them being wrong.
- **A planning capability names future contacts** (EPIC-020). That is a
  different fact from decision 2's, and it may legitimately live on the
  application — because *intended* is not *filed*.
- **A regulator requires the filing and the profile to agree.** That, and only
  that, would justify the validation decision 4 refuses.
