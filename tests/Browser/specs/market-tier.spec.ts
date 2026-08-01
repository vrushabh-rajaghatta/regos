import { expect } from "@playwright/test";

import { test, api, collectErrors } from "./support";

/**
 * **EPIC-017's capstone: the whole tier, as one story.**
 *
 * Not another CRUD flow. This walks the architecture the epic built, in the
 * order a regulatory user would actually live it — and ends on the sentence the
 * epic set out to be able to answer:
 *
 * > *What do we hold in Canada?*
 *
 * Every other spec proves one capability. This one proves they compose: that a
 * global product, a market, a name, a commercial history and a licence are five
 * separate facts owned by three aggregates, and that one screen can still
 * answer a person's question with all of them at once.
 */
const CANADA_NAME = "Canada";

test.describe("The market-local tier", () => {
  test("a product enters a market, is named, launched, licensed — and the portfolio can say so", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();

    const productName = `Tier Product ${unique}`;
    const globalProductId = await createProduct(unique, productName);

    // --- 1. a global product is not in any market ------------------------
    // The distinction the whole epic exists for: a product identity is not a
    // market presence, and RegOS says so rather than implying one.
    await page.goto(`/regulatory/products/${globalProductId}/registrations`);
    await expect(page.getByTestId("product-markets-empty")).toBeVisible();

    // --- 2. entering a market is its own business act --------------------
    // Years before any authority agrees. Nothing here mentions a licence.
    await page.getByRole("button", { name: "Add market" }).click();
    await page.getByLabel("Country").selectOption({ label: CANADA_NAME });
    await page.getByLabel("Present since").fill("2019-06-01");
    await page.getByRole("button", { name: "Add" }).click();

    await page
      .getByTestId("product-market-row")
      .first()
      .getByRole("link", { name: CANADA_NAME })
      .click();

    // --- 3. the market is a place you work, not a row --------------------
    await expect(page.getByTestId("market-overview")).toBeVisible();
    await expect(
      page.getByRole("heading", { name: CANADA_NAME })
    ).toBeVisible();

    // --- 4. what it is called there --------------------------------------
    // One name per language. Both are real; neither is primary.
    await addTradeName(page, "English", `Cardiolex ${unique}`);
    await addTradeName(page, "French", `Cardiolexe ${unique}`);
    await expect(page.getByTestId("market-trade-name")).toHaveCount(2);

    // --- 5. whether it is on sale ----------------------------------------
    // A commercial fact, independent of any regulator's decision.
    await recordSaleStatus(page, "Launched", "2021-03-15");
    await expect(page.getByTestId("market-status-history-entry")).toHaveCount(2);

    // --- 6. and the licence, granted over the market -----------------------
    await page.getByRole("button", { name: "New registration" }).click();
    await page.getByLabel("Authority").selectOption({ index: 1 });
    await page.getByLabel("Authorisation holder").selectOption({ index: 1 });
    await page.getByLabel("Planned on").fill("2020-01-10");
    await page.getByRole("button", { name: "Create" }).click();

    await expect(page.getByTestId("market-registration")).toHaveCount(1);

    await page
      .getByTestId("market-registration")
      .first()
      .getByRole("link")
      .click();

    await expect(page).toHaveURL(/\/regulatory\/registrations\/[0-9a-f-]{36}$/);

    await page
      .getByTestId("registration-actions")
      .getByRole("button", { name: "Record grant" })
      .click();

    await page.getByLabel("Registration number").fill(`CA-${unique}`);
    await page.getByLabel("Approved on").fill("2021-02-08");
    await page.getByLabel("Expires on (optional)").fill("2031-02-08");
    await page.getByRole("button", { name: "Save" }).click();

    await expect(
      page.getByRole("heading", { name: `CA-${unique}` })
    ).toBeVisible();

    // --- 7. the question the epic set out to answer ------------------------
    // Five facts, three aggregates, one row. None of them belong together in
    // the domain; all of them belong together in the question.
    await page.goto("/regulatory/registrations");
    await page
      .getByTestId("registration-market")
      .filter({ hasText: CANADA_NAME })
      .click();

    const row = page
      .getByTestId("market-registration-row")
      .filter({ hasText: productName });

    await expect(row).toHaveCount(1);

    // What product.
    await expect(row).toContainText(productName);

    // What it is called there — every name, because there is no primary.
    await expect(row.getByTestId("registration-trade-names")).toContainText(
      `Cardiolex ${unique}`
    );
    await expect(row.getByTestId("registration-trade-names")).toContainText(
      `Cardiolexe ${unique}`
    );

    // Whether it is on sale, and since when.
    await expect(row.getByTestId("registration-market-status")).toContainText(
      "Launched"
    );
    await expect(row.getByTestId("registration-market-status")).toContainText(
      "2021-03-15"
    );

    // The licence, and how long it lasts.
    await expect(row).toContainText(`CA-${unique}`);
    await expect(row).toContainText("Approved");
    await expect(row.getByTestId("registration-expiry")).toBeVisible();

    // --- 8. and the two lifecycles stay apart -----------------------------
    // A withdrawn licence does not take a product off the shelf. The row
    // still reports it as launched, because it still is.
    await page.goBack();
    await page.goto(`/regulatory/products/${globalProductId}/registrations`);
    await page.getByTestId("product-registration-row").first()
      .getByRole("link").click();

    await page
      .getByTestId("registration-actions")
      .getByRole("button", { name: "Withdrawn", exact: true })
      .click();

    await page.getByLabel("Took effect on").fill("2026-06-01");
    await page.getByRole("button", { name: "Save" }).click();

    await expect(page.getByTestId("registration-terminal")).toBeVisible();

    await page.goto("/regulatory/registrations");
    await page
      .getByTestId("registration-market")
      .filter({ hasText: CANADA_NAME })
      .click();

    const after = page
      .getByTestId("market-registration-row")
      .filter({ hasText: productName });

    await expect(after).toContainText("Withdrawn");
    await expect(after.getByTestId("registration-market-status")).toContainText(
      "Launched"
    );

    expect(errors()).toEqual([]);
  });
});

async function recordSaleStatus(
  page: import("@playwright/test").Page,
  status: string,
  occurredOn: string,
) {
  await page.getByRole("button", { name: "Record sale status" }).click();
  await page.getByLabel("Now").selectOption({ label: status });
  await page.getByLabel("Took effect on").fill(occurredOn);
  await page.getByRole("button", { name: "Save" }).click();
}

async function addTradeName(
  page: import("@playwright/test").Page,
  language: string,
  name: string,
) {
  await page.getByRole("button", { name: "Add name" }).click();
  await page.getByLabel("Language").selectOption({ label: language });
  await page.getByLabel("Trade name").fill(name);
  await page.getByRole("button", { name: "Save" }).click();
}

async function createProduct(unique: number, name: string): Promise<string> {
  const response = await api("/api/products", {
    method: "POST",
    body: JSON.stringify({ code: `TIER-${unique}`, name, type: "Drug" }),
  });

  if (!response.ok) {
    throw new Error(`Unable to create a product (${response.status}).`);
  }

  return (await response.json()).id;
}
