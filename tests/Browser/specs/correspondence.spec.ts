import { expect } from "@playwright/test";

import { test, api, collectErrors, EXPECTED_400 } from "./support";

/**
 * **EPIC-006 S001 — a letter, filed where it belongs.**
 *
 * Proves the three verbs the story was designed around: a regulatory user can
 * **file** a letter, **find** it again, and **understand** it without opening
 * anything else.
 *
 * Two things are asserted as absences, because they are decisions rather than
 * omissions (ADR-040): correspondence has **no status** — it is an event, not a
 * lifecycle — and it carries **no division**, because the division on a letter
 * is the authority's and `OrganizationDivision` cannot express one.
 */
const FDA_NAME = "Food and Drug Administration";

test.describe("Health-authority correspondence", () => {
  test("a letter is filed, found in the list, and understood on its own page", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();
    const subject = `Information request ${unique}`;

    // --- 1. the working surface exists and is reachable ------------------
    await page.goto("/regulatory/correspondence");

    // `exact` here is a narrowed selector, not a papered-over wording defect —
    // the guideline's step 4, so the reason is written down. The page heading
    // is "Correspondence" and the empty state is "No correspondence yet."; a
    // person reading them aloud is never confused, and only Playwright's
    // substring matching sees a collision.
    await expect(
      page.getByRole("heading", { name: "Correspondence", exact: true })
    ).toBeVisible();

    // --- 2. filing a letter that already happened ------------------------
    // Nothing defaults to today: the date on the letter is a fact about the
    // letter, and a mailbox carried into RegOS is mostly historic.
    await page.getByRole("button", { name: "Log correspondence" }).click();

    await expect(
      page.getByRole("heading", { name: "New correspondence" })
    ).toBeVisible();

    await page
      .getByLabel("Health authority")
      .selectOption({ label: FDA_NAME });
    await page.getByLabel("Received or sent").selectOption("Inbound");
    await page
      .getByLabel("Correspondence type")
      .selectOption({ label: "Information Request" });
    await page.getByLabel("Subject").fill(subject);
    await page.getByLabel("Dated").fill("2019-06-14");
    await page.getByLabel("Response due (optional)").fill("2019-07-14");
    await page
      .getByLabel("Authority reference (optional)")
      .fill(`IND ${unique}`);

    await page.getByRole("button", { name: "Log" }).click();

    // --- 3. found in the list --------------------------------------------
    // The list, not the detail page: it only refreshes if the mutation
    // genuinely invalidated its cache (browser convention 3).
    const row = page.getByRole("link", { name: subject });
    await expect(row).toBeVisible();

    // --- 4. filtering is how the list is actually used --------------------
    await page.getByLabel("Filter by direction").selectOption("Outbound");
    await expect(page.getByRole("link", { name: subject })).toHaveCount(0);

    await page.getByLabel("Filter by direction").selectOption("Inbound");
    await expect(page.getByRole("link", { name: subject })).toBeVisible();

    // --- 5. understood on its own page ------------------------------------
    await page.getByRole("link", { name: subject }).click();

    await expect(page.getByRole("heading", { name: subject })).toBeVisible();
    await expect(page.getByText(`IND ${unique}`)).toBeVisible();

    // Both dates are shown: the letter is from 2019, the record is from today,
    // and a reader who cannot see both will eventually mistake one for the
    // other.
    await expect(page.getByText("2019-06-14")).toBeVisible();
    await expect(page.getByText("Recorded in RegOS")).toBeVisible();

    // The response is long overdue — proximity derived at the edge, stored
    // nowhere (ADR-037).
    await expect(page.getByText(/overdue by/)).toBeVisible();

    // An unfiled letter is a real letter. This one names nothing, and the page
    // says so rather than showing an empty field.
    await expect(
      page.getByText("Nothing — general correspondence")
    ).toBeVisible();

    // --- 6. the two deliberate absences -----------------------------------
    // Asserted so that adding either one is a conversation, not a commit.
    await expect(page.getByText("Status")).toHaveCount(0);
    await expect(page.getByText("Division")).toHaveCount(0);

    expect(errors()).toEqual([]);
  });

  test("the server's refusal is rendered, and nothing escapes to the window", async ({
    page,
  }) => {
    // Browser convention 5: every new mutation dialog is walked through at
    // least one real server refusal. Six forms once shipped rendering the
    // message correctly *and* letting the rejection escape unhandled.
    const errors = collectErrors(page, [EXPECTED_400]);

    await page.goto("/regulatory/correspondence");
    await page.getByRole("button", { name: "Log correspondence" }).click();

    await page.getByLabel("Health authority").selectOption({ label: FDA_NAME });
    await page.getByLabel("Received or sent").selectOption("Inbound");
    await page
      .getByLabel("Correspondence type")
      .selectOption({ label: "Information Request" });
    await page.getByLabel("Subject").fill("A refused letter");
    await page.getByLabel("Dated").fill("2026-03-01");

    // The business refusal available here: a response cannot be due before the
    // letter it answers. The rule is not mirrored client-side (SC-103), so this
    // reaches the server and renders its message verbatim.
    await page.getByLabel("Response due (optional)").fill("2026-02-01");
    await page.getByRole("button", { name: "Log" }).click();

    await expect(
      page.getByText("A response cannot be due before the correspondence itself.")
    ).toBeVisible();

    expect(errors()).toEqual([]);
  });

  test("the API refuses a response due before the letter", async () => {
    // The same rule at the boundary the UI cannot bypass, so the invariant is
    // proven to live in the domain rather than in the form.
    const authorities = await (await api("/master-data/authorities")).json();
    const types = await (
      await api("/api/master-data/correspondence-types")
    ).json();

    const response = await api("/api/correspondence", {
      method: "POST",
      body: JSON.stringify({
        authorityId: authorities[0].id,
        correspondenceTypeId: types[0].id,
        direction: "Inbound",
        subject: "Chronology check",
        occurredOn: "2026-03-01",
        responseDueOn: "2026-02-01",
      }),
    });

    expect(response.status).toBe(400);

    const problem = await response.json();
    expect(problem.detail).toContain("cannot be due before");
  });
});
