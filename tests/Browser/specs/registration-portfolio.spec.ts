import { expect } from "@playwright/test";

import { test, api, collectErrors } from "./support";

/**
 * The registration workspace: both portfolio axes, and the one canonical page
 * a registration has whichever direction you arrive from.
 *
 * Its subject is discoverability — that a registration created for a product
 * appears under that product *and* under its market, and that both routes land
 * on the same page. The full lifecycle journey is STORY-004's capstone, not
 * this spec's.
 *
 * The registration is created through the browser, because the creation form is
 * what this story added. Only the product it belongs to is set up through the
 * API, per the rule that a spec owns the data it mutates (ADR-019).
 */
const UNITED_STATES = "10000000-0000-0000-0000-000000000001";

test.describe("Registration portfolio", () => {
  test("a new registration is discoverable by product and by market", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();

    const productName = `Registration Product ${unique}`;
    const globalProductId = await createProduct(unique, productName);

    // --- 1. a product with nothing registered says so --------------------
    await page.goto(`/regulatory/products/${globalProductId}/registrations`);

    await expect(
      page.getByTestId("product-registrations-empty"),
    ).toBeVisible();

    // --- 2. record one through the form ----------------------------------
    await page.getByRole("button", { name: "New registration" }).click();

    await page.getByLabel("Market").selectOption({ label: "United States" });
    await page.getByLabel("Authority").selectOption({ index: 1 });
    await page.getByLabel("Authorisation holder").selectOption({ index: 1 });
    await page.getByLabel("Planned on").fill("2020-01-10");
    await page.getByRole("button", { name: "Create" }).click();

    // --- 3. it appears on the product axis --------------------------------
    const productRows = page.getByTestId("product-registration-row");
    await expect(productRows).toHaveCount(1);
    await expect(productRows.first()).toContainText("United States");
    await expect(productRows.first()).toContainText("Planned");

    // --- 4. and on the market axis, which now lists the market at all -----
    await page.goto("/regulatory/registrations");

    const market = page
      .getByTestId("registration-market")
      .filter({ hasText: "United States" });

    await expect(market).toBeVisible();
    await market.click();

    await expect(page).toHaveURL(
      new RegExp(`/regulatory/registrations/markets/${UNITED_STATES}$`),
    );

    const marketRow = page
      .getByTestId("market-registration-row")
      .filter({ hasText: productName });

    await expect(marketRow).toHaveCount(1);

    // --- 5. both axes lead to the same page -------------------------------
    await marketRow.getByRole("link", { name: productName }).click();

    await expect(page).toHaveURL(/\/regulatory\/registrations\/[0-9a-f-]{36}$/);
    const canonical = page.url();

    // Nothing is granted yet, so the page says so rather than showing a number.
    await expect(
      page.getByRole("heading", { name: "Not yet granted" }),
    ).toBeVisible();

    await expect(page.getByTestId("registration-history")).toBeVisible();
    await expect(
      page.getByTestId("registration-history-entry"),
    ).toHaveCount(1);

    // The same registration reached from the product side is the same URL.
    await page.goto(`/regulatory/products/${globalProductId}/registrations`);
    await productRows.first().getByRole("link").click();

    expect(page.url()).toBe(canonical);

    // --- 6. the actions are the server's answer, not the page's ----------
    // Planned permits five onward statuses, and the first grant is offered as
    // a grant rather than a plain status change.
    const actions = page.getByTestId("registration-actions");
    await expect(actions.getByRole("button")).toHaveCount(5);
    await expect(
      actions.getByRole("button", { name: "Record grant" }),
    ).toBeVisible();

    expect(errors()).toEqual([]);
  });
});

async function createProduct(unique: number, name: string): Promise<string> {
  const response = await api("/api/products", {
    method: "POST",
    body: JSON.stringify({
      code: `REG-${unique}`,
      name,
      type: "Drug",
    }),
  });

  if (!response.ok) {
    throw new Error(`Unable to create a product (${response.status}).`);
  }

  return (await response.json()).id;
}
