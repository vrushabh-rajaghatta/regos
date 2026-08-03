# Study

**A thing in the world that documents are *about*.** A study is run, and named,
by the sponsor; RegOS records it. It is not a `Submission`, not a
`ProductDocument`, and not a `TemplateSection`.

See [ADR-056](../adr/ADR-056-study-identity-is-owned-by-the-sponsor.md) for why
it is its own context, and
[ADR-054](../adr/ADR-054-a-study-tagging-file-is-a-projection-over-a-study.md)
for what needs it.

## Two aggregates, no parent

| Aggregate | CTD | Answers |
|---|---|---|
| `NonClinicalStudy` | Module 4 (4.2.x) | toxicology, pharmacology, pharmacokinetics |
| `ClinicalStudy` | Module 5 (5.3.x) | in human subjects |

They are **peers**, and there is deliberately no `Study` supertype, no shared
base class, and no `StudyKind` discriminator — the same call
[ADR-040 §3](../adr/ADR-040-the-health-authority-interaction-context.md) made
for the five interaction objects. Identity follows: `ClinicalStudyId` and
`NonClinicalStudyId` are separate types, because one id spanning two aggregates
is an identity space neither owns.

A read that genuinely spans both — the registry list — **composes**. It is two
scans merged in a query handler, not a union type.

## The word the domain uses, and the word the screen uses

| Domain | Screen | Why they differ |
|---|---|---|
| `SponsorStudyIdentifier` | **"Study ID"** | ICH calls it `study-id` and every regulatory user says *"the study ID"*. The type does not, because on a `ClinicalStudy` that name would read as the aggregate's own identity — which is exactly what it is not. |
| `NonClinicalStudy` | **"Non-clinical"** | RIM's spelling against the reader's. Formed in one place — [`studyKinds.ts`](../../web/regos-web/src/features/regulatory/studies/constants/studyKinds.ts). |

**Both are binding.** The screen's word must never reach a type, and the type's
word must never reach a label by default (CLAUDE.md).

## Two facts, and the rule for admitting a third

A study is **the sponsor's identifier and a title**. That is not a placeholder;
it is what the seeded FDA IND blueprint actually demands. ICH requires a study's
species, route, duration and type-of-control only for CTD 4.2.3.1, 4.2.3.2,
4.2.3.4.1 and 5.3.5.1, and the blueprint seeds none of those.

> **A `Study` begins as the smallest sponsor-owned identity capable of
> supporting regulatory filing. Additional attributes are admitted only when
> required by an external regulatory workflow or a demonstrated business
> capability.** (ADR-056 §3)

`phase`, `indication`, `therapeutic area`, `subject count`, `sponsor`, status
history, start and closeout dates are all plausible and all currently
unrequested. *"RIM lists it"* is not a reason.

**The hard line**: study results, endpoints, arms, populations and statistical
data are not admitted by that rule at all. RegOS is a regulatory information
system, not a CTMS.

## What is deliberately absent

| Absent | Why |
|---|---|
| a status | ES-018's Active/Inactive pair exists so records are retired rather than deleted, and nothing deletes a study. A lifecycle would be a column no capability writes. |
| a format rule on the identifier | EPIC-007a settled that an authority's format check belongs at the boundary that needs it. `RecordApplicationNumber` takes any string and the generator refuses a non-FDA one *by name*; S003 does the same for an identifier it cannot put in a filename. |
| navigation properties | Aggregates reference each other by id only (ES-014). |

## The one rule that came from outside RegOS

**One sponsor study identifier names one study, across both kinds.**

**E24** records that FDA's review tooling recognises a study by its `study-id`,
and that a mismatch shows a reviewer two studies where there is one. Read
backwards, that is this rule: two studies sharing an identifier are shown as
**one**, and the STF carries no kind marker to tell them apart — it writes
`<study-id>ABC-123</study-id>` and nothing else.

It spans both aggregates, so it cannot be a unique index alone: an index covers
one table, and uniqueness within each kind would still let a clinical and a
non-clinical study collide in the one namespace FDA reads.

```
SponsorStudyIdentifierPolicy     states the rule, across both sets
  + a unique index per table     closes the race the policy cannot
```

Neither alone is the rule. The refusal **names the study already using the
code**, because that is what tells a typo apart from a genuine duplicate.

The same constraint is why both facts are trimmed on the way in: `" ABC-1 "` and
`"ABC-1"` are one study to FDA, and must be one here.

## Where a study is reported

A study does not know where it is filed. **The placement does** — since S002,
`SubmissionDocument` carries a typed reference to the study it reports, and at
most one:

```
Submission
  SubmissionDocument (the placement)
    ClinicalStudyId?      ─┐ exactly one, or neither
    NonClinicalStudyId?   ─┘
```

Two consequences, both enforced rather than described: **taking a document out
of the dossier takes its study with it**, and **moving it between sections keeps
it**. Refiling changes a row on the placement and never touches this registry.

## Owed

- **`Retitle` is unreachable.** The aggregate has it; nothing calls it. It
  becomes safe — and reachable — when **S003 freezes the identifier and title
  into the published placement**, which is
  [ADR-047](../adr/ADR-047-publication-metadata-exists-only-when-publication-makes-it-true.md)'s
  instrument rather than a policy. S001 predicted a guard shaped like
  `ApplicationNumberPolicy`; that would have pointed `Study` at `Submission` and
  inverted ADR-056 §4. Freezing costs nothing and inverts nothing.
- **Whether the identifier is unique *per tenant* or globally** is settled as
  per tenant. ADR-056 left the choice open and required that whichever was made
  got a test; `study-registry.spec.ts` is it.
