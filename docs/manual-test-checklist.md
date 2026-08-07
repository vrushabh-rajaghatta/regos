# Manual test checklist

One pass through everything RegOS does today, against a **fresh database**.

**The rule this is built on: nothing is created twice.** One product carries the
whole chain; one market carries everything below it; one document is uploaded
once and reused by the submission. Everything else is already seeded — do not
create organizations, sites, substances, document types, countries or
templates.

---

## 0 — Boot

You do **not** need to create the database. In Development
`Database:MigrateOnStartup` is `true`, so the host creates it, applies the 85
migrations and seeds it.

```bash
dotnet run --project src/Host/RegOS.Api          # :5225
cd web/regos-web && npm run dev                  # :5173 — must be 5173, CORS allows only that
```

- [ ] The API log shows migrations applied, then the initializers running
- [ ] Sign in at `/login` as `dev@regos.local` / `development-password`

### What the seed gives you

| | |
|---|---|
| Tenant | **Demo MAH Ltd.** — the dev user's tenant |
| Products **you can see** | **ASP-75** Aspirin 75mg · **OZE-1** Ozempic |
| Products you must **not** see | ACE-500, IBU-200, NAP-250 — owned by Demo Manufacturer Ltd. |
| Also seeded | organizations, sites, countries, authorities, document types, submission types, substances, blueprints, the `US-FDA-IND-INITIAL` process definition |

- [ ] **Tenant isolation** — the product list shows exactly two products. The
      three Manufacturer products are absent. *(Free ADR-031 test; if they
      appear, stop.)*

**ASP-75 is the vehicle for everything below. Leave OZE-1 untouched** — it is
the control for empty states.

---

## 1 — Identity & session

- [ ] `/settings` — profile loads
- [ ] `/settings/security` — change password, then sign in with the new one
- [ ] `/settings/sessions` — the current session is listed; revoke others
- [ ] Sign out; a protected route bounces to `/login`

---

## 2 — Reference data *(API only — EPIC-012 has no screens yet)*

One `curl` each, cookie or bearer from the login above. All should return rows:

- [ ] `/api/reference-data/application-types` · `/submission-types` ·
      `/submission-sub-types` · `/contact-roles` · `/identifier-schemes`
- [ ] `/api/master-data/correspondence-types` · `/api/measurement-units`
- [ ] `/api/substances/vocabulary` · `/api/presentations/vocabulary` ·
      `/api/packaged-products/vocabulary` · `/api/labels/vocabulary` ·
      `/api/indications/vocabulary` · `/api/manufacturing-operations/vocabulary`
- [ ] `/api/study-tagging/file-tags`

---

## 3 — Organization depth *(reuse the seeded org — do not create one)*

`/regulatory/organizations` → **Demo MAH Ltd.**

- [ ] Divisions tab — add one
- [ ] Contacts tab — add a contact with a role
- [ ] Sites tab — sites are seeded; open one
- [ ] Add an identifier (DUNS or FEI — the schemes are seeded)
- [ ] Deactivate and reactivate the organization

---

## 4 — Product documents *(ASP-75)*

`/regulatory/products/{ASP-75}/documents`

- [ ] Upload a small PDF, any seeded document type → **this is the document the
      submission uses later. Only upload one.**
- [ ] Versions tab — upload a second version; it becomes v2 and current
- [ ] Activate it *(a Draft cannot be attached to a submission)*
- [ ] Usage tab — empty for now; you will come back to it at step 10
- [ ] Confirm there is **no download button** — that is finding #9, not a bug you found

---

## 5 — Market (medicinal product) *(create exactly one)*

`/regulatory/products/{ASP-75}` → Markets

- [ ] Create one market for **United States**
- [ ] Add a trade name
- [ ] Set market status
- [ ] Set the ATC code
- [ ] Check label languages resolve from the country (EPIC-022)

**Everything from here to step 9 hangs off this one market.**

---

## 6 — Composition

On the market page:

- [ ] Add a presentation (tablet, strength from the seeded units)
- [ ] Add an ingredient using a **seeded** substance — do not create a substance
- [ ] Set the appearance
- [ ] Add a component; nest a child under it
- [ ] `/regulatory/substances` → open that substance → **"which products contain
      it?"** lists ASP-75 *(EPIC-010a's payoff query)*

---

## 7 — Packs

- [ ] Create a packaged product on the market
- [ ] Add package items; nest one
- [ ] Set supply details and shelf life
- [ ] Set marketing status

---

## 8 — Manufacturing

- [ ] Add a manufacturing operation at a **seeded site**
- [ ] Check approved sites for the market
- [ ] **Site alignment** — *"is the site we manufacture at on the licence?"*
      Expect a mismatch until step 12 adds the registration; re-check after
- [ ] Cease an operation; it leaves history rather than disappearing

---

## 9 — Labeling

**Global label** (product level):
- [ ] Create → draft → new version → add content → publish

**Clinical statements** (market level):
- [ ] Add an indication, with text and a population; add a therapy
- [ ] Add a contraindication, an undesirable effect, and an interaction with an
      interactant — each with one population
- [ ] Record an indication decision

**Local label**:
- [ ] Create for the market → draft → revision → publish → link to the pack

- [ ] **The payoff query** — *"which markets is this approved for this
      condition?"* returns the US market

---

## 10 — Application, submission, package *(the spine)*

`/regulatory/products/{ASP-75}/applications`

- [ ] Create an application (IND, FDA) → set the application number
- [ ] Add a contact from step 3
- [ ] Create a submission; confirm it binds to a published blueprint version
- [ ] **Content plan** — the CTD structure renders with placeholders
- [ ] **Documents** — attach the document from step 4 into a section
- [ ] **Validation** — fails while required placeholders are unfilled, and says which
- [ ] **People** — add a submission role
- [ ] Set the format
- [ ] **Publish** — blocked until validation passes; then it snapshots
- [ ] **Changes** — the derived delta against the cumulative dossier
- [ ] **Generate the package** — download the sequence zip, confirm
      `index.xml`, `index-md5.txt`, the FDA regional backbone, and your PDF at
      its blueprint path
- [ ] Back to step 4's **Usage** tab — it now names this submission
- [ ] History tab — the sequence appears

---

## 11 — Studies

- [ ] `/regulatory/studies` — create one clinical and one nonclinical study
- [ ] Link the clinical study to the application
- [ ] Tag the submission document with the study → check the study's filings
- [ ] Confirm the Study Tagging File appears in a regenerated package

---

## 12 — Registrations

- [ ] Create a registration for the market from step 5
- [ ] Move its status (Planned → Submitted → Approved); history is append-only
      with `OccurredOn` distinct from `RecordedOnUtc`
- [ ] Link the authorised pack from step 7
- [ ] Link the site from step 8 → **re-run step 8's site alignment; it now passes**
- [ ] `/api/registrations/expiring` and `/api/registrations/markets` return it
- [ ] `/api/countries/{US}/registrations` returns it

---

## 13 — Health-authority interactions

`/regulatory/correspondence`

- [ ] Record an inbound letter; attach a file; **download it back** *(the read
      path product documents don't have)*
- [ ] Raise two questions from it; assign an owner; respond to one; resolve it
- [ ] Remove the attachment
- [ ] `/regulatory/meetings` — request a meeting, set status, record the outcome
- [ ] `/regulatory/inspections` — record one, add a finding, set status
- [ ] Create a commitment with a due date; move its status
- [ ] **`/regulatory/due-work`** — the epic's headline read. The undecomposed
      letter is **absent** (its questions replaced it), the open question,
      commitment and inspection are **present**

---

## 14 — Process & planning

- [ ] `/regulatory/playbooks` — the seeded `US-FDA-IND-INITIAL` definition loads
- [ ] `/regulatory/objectives` — create one, attach the market record, set status
- [ ] Create a plan from the definition → `/regulatory/plan-board`
- [ ] Move a step's status; check plan impact
- [ ] Link a step to the submission, the correspondence and the commitment
      (`/process-step` on each)
- [ ] `/api/process-plans/next-steps` returns what's due

---

## 15 — Platform administration

- [ ] `/platform/users` — invite a user. **The token is in the API log**
      (Development only) — copy the link
- [ ] Accept the invitation in a private window; set a password; sign in
- [ ] Deactivate then reactivate that user
- [ ] `/platform/tenants` — sign in as `platform@regos.local` /
      `platform-password`; confirm the tenant list loads and **every
      tenant-scoped query returns nothing** (the null-tenant path)
- [ ] Forgot-password flow — request, take the token from the log, complete

---

## Known-absent — do not raise these

| | |
|---|---|
| No download for product documents | finding #9 |
| No SPA route for reference data | EPIC-012 |
| No contact **edit** screen | EPIC-016 debt |
| Invitation and reset emails don't send | Development logs the token; `Unconfigured*Notifier` elsewhere |
| `npm run lint` — 6 problems | pre-existing baseline |
