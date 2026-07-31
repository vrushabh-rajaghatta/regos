import { expect } from "@playwright/test";

import { test, api, collectErrors, EXPECTED_400 } from "./support";

/**
 * The capstone: an authorisation's whole life, driven through the browser.
 *
 * Planned, filed, assessed, granted, suspended, reinstated, surrendered — with
 * the portfolio views and the derived expiry checked at each point that matters.
 * Its subject is the loop EPIC-005 set out to close: record what we hold, know
 * what state it is in, and see which authorisations deserve attention today.
 *
 * Every transition goes through the UI, because the point is that the workspace
 * drives the domain rather than merely reporting it. Only the product is set up
 * through the API, per the rule that a spec owns the data it mutates (ADR-019).
 */
const UNITED_STATES = "10000000-0000-0000-0000-000000000001";

/** A grant that lapses inside the attention window, whenever this runs. */
function soon(days: number): string {
  const date = new Date();
  date.setDate(date.getDate() + days);
  return date.toISOString().slice(0, 10);
}

test.describe("Registration lifecycle", () => {
  test("an authorisation from planned to surrendered, and what needs attention", async ({
    page,
  }) => {
    // The deliberate backdating below is refused with a 400 — expected, and the
    // only resource error this journey should produce.
    const errors = collectErrors(page, [EXPECTED_400]);
    const unique = Date.now();

    const productName = `Lifecycle Product ${unique}`;
    const globalProductId = await createProduct(unique, productName);

    // --- 1. record an intention ------------------------------------------
    await page.goto(`/regulatory/products/${globalProductId}/registrations`);
    await page.getByRole("button", { name: "New registration" }).click();

    await page.getByLabel("Market").selectOption({ label: "United States" });
    await page.getByLabel("Authority").selectOption({ index: 1 });
    await page.getByLabel("Authorisation holder").selectOption({ index: 1 });
    await page.getByLabel("Planned on").fill("2020-01-10");
    await page.getByRole("button", { name: "Create" }).click();

    await page.getByTestId("product-registration-row").first()
      .getByRole("link").click();

    await expect(page).toHaveURL(/\/regulatory\/registrations\/[0-9a-f-]{36}$/);
    const registrationUrl = page.url();

    const status = page.getByTestId("registration-status");
    const actions = page.getByTestId("registration-actions");

    await expect(status).toContainText("Planned");

    // --- 2. filed, then assessed ------------------------------------------
    await changeStatus(page, "Submitted", "2020-03-02");
    await expect(status).toContainText("Submitted");

    await changeStatus(page, "Under Review", "2020-04-15");
    await expect(status).toContainText("Under Review");

    // --- 3. the grant — its own operation, because it carries more --------
    await actions.getByRole("button", { name: "Record grant" }).click();
    await page.getByLabel("Registration number").fill(`NDA-${unique}`);
    await page.getByLabel("Approved on").fill("2021-02-08");
    await page.getByLabel("Expires on (optional)").fill(soon(45));
    await page.getByLabel("Note (optional)").fill("Original approval.");
    await page.getByRole("button", { name: "Save" }).click();

    await expect(page.getByRole("heading", { name: `NDA-${unique}` }))
      .toBeVisible();
    await expect(status).toContainText("Approved");

    // Expiry proximity is derived on read — never entered, never stored.
    await expect(page.getByTestId("registration-expiry"))
      .toContainText("in 45 days");

    // --- 4. it deserves attention, across every market --------------------
    await page.goto("/regulatory/registrations");

    const attention = page
      .getByTestId("expiring-registration")
      .filter({ hasText: productName });

    await expect(attention).toBeVisible();
    await expect(attention).toContainText("in 45 days");

    // --- 5. and it is visible on both portfolio axes ----------------------
    await page.goto(`/regulatory/registrations/markets/${UNITED_STATES}`);
    await expect(
      page.getByTestId("market-registration-row").filter({ hasText: productName }),
    ).toHaveCount(1);

    await page.goto(`/regulatory/products/${globalProductId}/registrations`);
    await expect(page.getByTestId("product-registration-row")).toHaveCount(1);

    // --- 6. suspended, then reinstated ------------------------------------
    await page.goto(registrationUrl);

    await changeStatus(page, "Suspended", "2023-09-14", "GMP non-compliance.");
    await expect(status).toContainText("Suspended");

    await changeStatus(page, "Approved", "2024-01-30", "Suspension lifted.");
    await expect(status).toContainText("Approved");

    // --- 7. history is read in business time ------------------------------
    await actions.getByRole("button", { name: "Expired" }).click();
    await page.getByLabel("Took effect on").fill("2020-01-01");
    await page.getByRole("button", { name: "Save" }).click();

    await expect(page.getByTestId("status-change-error"))
      .toContainText("History is read in business time");
    await page.getByRole("button", { name: "Cancel" }).click();

    // --- 8. surrendered: terminal, and off the validity timeline ----------
    await changeStatus(page, "Withdrawn", "2025-06-01", "Portfolio review.");

    await expect(page.getByTestId("registration-terminal")).toBeVisible();
    await expect(page.getByTestId("registration-actions")).toHaveCount(0);

    // The expiry date survives as history; the countdown does not.
    await expect(page.getByTestId("registration-expiry")).toHaveCount(0);

    // --- 9. it stops asking for attention ---------------------------------
    await page.goto("/regulatory/registrations");
    await expect(
      page.getByTestId("expiring-registration").filter({ hasText: productName }),
    ).toHaveCount(0);

    // --- 10. the whole story, in one chronological record ------------------
    await page.goto(registrationUrl);

    // Planned, Submitted, UnderReview, Approved, Suspended, Approved,
    // Withdrawn — one entry per transition, and the reinstatement is its own.
    await expect(page.getByTestId("registration-history-entry")).toHaveCount(7);
    await expect(page.getByTestId("registration-history")).toContainText(
      "Suspension lifted.",
    );

    // The grant survives everything that happened after it.
    await expect(page.getByRole("heading", { name: `NDA-${unique}` }))
      .toBeVisible();

    expect(errors()).toEqual([]);
  });
});

/** One transition through the dialog the server's own answer offered. */
async function changeStatus(
  page: import("@playwright/test").Page,
  label: string,
  occurredOn: string,
  note?: string,
) {
  await page
    .getByTestId("registration-actions")
    .getByRole("button", { name: label, exact: true })
    .click();

  await page.getByLabel("Took effect on").fill(occurredOn);
  if (note) await page.getByLabel("Note (optional)").fill(note);

  await page.getByRole("button", { name: "Save" }).click();
}

async function createProduct(unique: number, name: string): Promise<string> {
  const response = await api("/api/products", {
    method: "POST",
    body: JSON.stringify({ code: `LIFE-${unique}`, name, type: "Drug" }),
  });

  if (!response.ok) {
    throw new Error(`Unable to create a product (${response.status}).`);
  }

  return (await response.json()).id;
}
