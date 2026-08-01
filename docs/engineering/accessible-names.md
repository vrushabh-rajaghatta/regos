# Accessible Names Are Part Of The Domain Language

**Status:** Active · **Effective:** 2026-08-01 ·
**Promoted from:** [tests/Browser/README.md](../../tests/Browser/README.md) convention 6, on the third occurrence

---

## The rule

**Within a working surface — a dialog, a form, a card — interactive controls
and their headings must have distinct accessible names**, unless they
deliberately represent the same logical control.

A Playwright strict-mode failure caused by two elements sharing a name is
**evidence, not an obstacle**. It is reporting that a person using a screen
reader encounters the same ambiguity: two things in one place answering to one
word. The browser is simply the first thing that noticed.

**Fix the words. Not the selector.**

---

## Why this is not a testing rule

It has happened three times, and every fix improved the page for a sighted user
reading it silently:

| Collided | Became | What was actually wrong |
|---|---|---|
| dialog *Record Identifier* · field *Identifier* | field → **Identifier Value** | The field holds half of a scheme-plus-value pair. "Identifier" named the pair, not the half. |
| dialog *Trade name in Canada* · field *Trade name* | dialog → **Name in Canada** | The heading stuttered against its own field. |
| overview label *Launched* · status value *Launched* | label → **Launched on** | A reader parsed "On sale: Launched / Launched: 2021-03-15" to work out which word was the label. |

None was a test defect. Each was a place where **one word was carrying two
jobs** — and that is a vocabulary problem, which this codebase already takes
seriously everywhere else (`MedicinalProduct` vs **Market**; `Planned` reused
across tiers because it means one thing; `Withdrawn` refused because it would
have meant two).

> The same principle the domain applies to types: **never let one word carry two
> meanings.** A screen is not exempt because its words are rendered rather than
> compiled.

---

## Applying it

When a strict-mode violation names two elements:

1. **Read both names out loud.** If you have to say "the *field* Identifier" to
   disambiguate, the field's name is incomplete.
2. **Prefer changing the container.** A heading names the *thing*; a field names
   the *value*. When they collide it is usually the heading that is too
   specific — *"Name in Canada"* over *"Trade name in Canada"*.
3. **Add the preposition.** A date labelled with a bare participle collides with
   the status of the same name. *"Launched on"* cannot be mistaken for a value.
4. Only if none of that helps, narrow the selector — and write down why the
   duplication is correct.

---

## When duplication is correct

Rarely, and it should be argued rather than assumed. Two controls may share a
name when they genuinely are the same logical control rendered twice — a
"Save" in a sticky footer mirroring one in a form, for instance. In that case
say so in the spec, because the next reader will otherwise apply this rule.

---

## Change History

| Version | Date | Summary |
|---|---|---|
| 1.0 | 2026-08-01 | Promoted from a browser-spec convention on the third occurrence (EPIC-017 S005). |
