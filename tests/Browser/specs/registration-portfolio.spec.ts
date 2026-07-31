import { expect } from "@playwright/test";

import { test, api, collectErrors, EXPECTED_404 } from "./support";

/**
 * The registration workspace: both portfolio axes, and the one canonical page
 * a registration has whichever direction you arrive from.
 *
 * Its subject is discoverability — that a registration created for a product
 * appears under that product *and* under its market, and that both routes land
 * on the same page. The full lifecycle journey is STORY-004's capstone, not
 * this spec's.
 *
 * Since EPIC-017 S001 it also proves the tier the licence hangs from: the
 * market is added first, as its own act, and the authorisation is granted over
 * it. Pick-or-create lives here, in the UI, precisely so that the write model
 * never has to guess which market-local product a caller meant.
 *
 * Both are created through the browser, because those forms are what these
 * stories added. Only the product they belong to is set up through the API, per
 * the rule that a spec owns the data it mutates (ADR-019).
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

    // --- 1. a product in no market, holding nothing, says both -----------
    await page.goto(`/regulatory/products/${globalProductId}/registrations`);

    await expect(page.getByTestId("product-markets-empty")).toBeVisible();
    await expect(
      page.getByTestId("product-registrations-empty"),
    ).toBeVisible();

    // --- 2. the market comes first, and is its own act -------------------
    // A licence is granted over a product in a country, so the country has to
    // exist as a thing before there is anything to grant a licence over.
    await page.getByRole("button", { name: "Add market" }).click();
    await page.getByLabel("Country").selectOption({ label: "United States" });
    await page.getByLabel("Present since").fill("2019-06-01");
    await page.getByRole("button", { name: "Add" }).click();

    const marketRows = page.getByTestId("product-market-row");
    await expect(marketRows).toHaveCount(1);
    await expect(marketRows.first()).toContainText("United States");

    // A market with no authorisation in it is an ordinary state, not a gap.
    await expect(marketRows.first()).toContainText("0");
    await expect(
      page.getByTestId("product-registrations-empty"),
    ).toBeVisible();

    // --- 3. then the authorisation, granted over that market -------------
    await marketRows
      .first()
      .getByRole("button", { name: "New registration" })
      .click();

    await expect(
      page.getByRole("heading", { name: "New registration in United States" }),
    ).toBeVisible();

    await page.getByLabel("Authority").selectOption({ index: 1 });
    await page.getByLabel("Authorisation holder").selectOption({ index: 1 });
    await page.getByLabel("Planned on").fill("2020-01-10");
    await page.getByRole("button", { name: "Create" }).click();

    // --- 4. it appears on the product axis --------------------------------
    const productRows = page.getByTestId("product-registration-row");
    await expect(productRows).toHaveCount(1);
    await expect(productRows.first()).toContainText("United States");
    await expect(productRows.first()).toContainText("Planned");

    // --- 5. and on the market axis, which now lists the market at all -----
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

    // --- 6. both axes lead to the same page -------------------------------
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

    // --- 7. the actions are the server's answer, not the page's ----------
    // Planned permits five onward statuses, and the first grant is offered as
    // a grant rather than a plain status change.
    const actions = page.getByTestId("registration-actions");
    await expect(actions.getByRole("button")).toHaveCount(5);
    await expect(
      actions.getByRole("button", { name: "Record grant" }),
    ).toBeVisible();

    expect(errors()).toEqual([]);
  });

  /**
   * The EPIC-016 house rule: every new mutation dialog is walked through at
   * least one real server refusal, because success-path verification is what
   * let six forms ship with an unhandled promise rejection escaping to the
   * window. Only <c>collectErrors</c> sees that half of the failure.
   *
   * Adding a market has no *business* refusal — a duplicate country is
   * deliberately allowed, and both inputs come from pickers — so the refusal
   * exercised here is the structural one: a product that is not there.
   */
  test("a refused market is reported in the dialog, not thrown at the page", async ({
    page,
  }) => {
    const errors = collectErrors(page, [EXPECTED_404]);
    const missing = "00000000-0000-0000-0000-0000000000ff";

    await page.goto(`/regulatory/products/${missing}/registrations`);

    await page.getByRole("button", { name: "Add market" }).click();
    await page.getByLabel("Country").selectOption({ label: "United States" });
    await page.getByLabel("Present since").fill("2019-06-01");
    await page.getByRole("button", { name: "Add" }).click();

    // The server's own words, and the dialog stays open holding what was typed.
    await expect(page.getByTestId("add-market-error")).toContainText(
      "Product does not exist.",
    );
    await expect(
      page.getByRole("heading", { name: "Add market" }),
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
