import { expect } from "@playwright/test";

import {
  test,
  api,
  collectErrors,
  EXPECTED_404,
  EXPECTED_409,
  EXPECTED_400,
} from "./support";

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
   * What the product is called there — one name per language, and the refusal
   * of a second is a real business rule rather than a structural error, so this
   * also discharges the EPIC-016 house rule for the trade-name dialog.
   *
   * The opposite of the rule one tier up, which this same page proves elsewhere:
   * two market presences in one country are allowed, two English names for one
   * market presence are not.
   */
  test("a market is named once per language, and a second name in one is refused", async ({
    page,
  }) => {
    const errors = collectErrors(page, [EXPECTED_409]);
    const unique = Date.now();

    const globalProductId = await createProduct(
      unique,
      `Trade Name Product ${unique}`,
    );

    await page.goto(`/regulatory/products/${globalProductId}/registrations`);

    await page.getByRole("button", { name: "Add market" }).click();
    await page.getByLabel("Country").selectOption({ label: "Canada" });
    await page.getByLabel("Present since").fill("2019-06-01");
    await page.getByRole("button", { name: "Add" }).click();

    // --- 1. a market with no branding says so ----------------------------
    await expect(page.getByTestId("market-unnamed")).toBeVisible();

    // --- 2. named in two languages, which is the point of the tier -------
    await addTradeName(page, "English", `Cardiolex ${unique}`);
    await expect(page.getByTestId("market-trade-name")).toHaveCount(1);

    await addTradeName(page, "French", `Cardiolexe ${unique}`);
    await expect(page.getByTestId("market-trade-name")).toHaveCount(2);
    await expect(page.getByTestId("product-markets")).toContainText("French");

    // --- 3. a second English name is one of them being wrong -------------
    await page.getByRole("button", { name: "Add name" }).click();
    await page.getByLabel("Language").selectOption({ label: "English" });
    await page.getByLabel("Trade name").fill("Conflicting");
    await page.getByRole("button", { name: "Save" }).click();

    await expect(page.getByTestId("add-trade-name-error")).toContainText(
      "already has a trade name in that language",
    );

    // The dialog stays open holding what was typed, and nothing was added.
    await expect(page.getByLabel("Trade name")).toHaveValue("Conflicting");
    await page.keyboard.press("Escape");
    await expect(page.getByTestId("market-trade-name")).toHaveCount(2);

    // --- 4. removing frees the language, which is how a name is corrected -
    await page
      .getByRole("button", { name: `Remove Cardiolex ${unique}` })
      .click();

    await expect(page.getByTestId("market-trade-name")).toHaveCount(1);

    await addTradeName(page, "English", `Renamed ${unique}`);
    await expect(page.getByTestId("market-trade-name")).toHaveCount(2);
    await expect(page.getByTestId("product-markets")).toContainText(
      `Renamed ${unique}`,
    );

    expect(errors()).toEqual([]);
  });

  /**
   * Whether it is actually on sale — the commercial life of a market, driven
   * through the browser.
   *
   * Its subject is the pair of facts a registration cannot answer: a licence
   * being granted does not put a product on a shelf, and a product leaving the
   * shelf does not surrender the licence. It also proves the launch date is
   * derived rather than typed — a relaunch does not move it, because nobody
   * can move it.
   */
  test("a market is launched, lost, relaunched — and the launch date never moves", async ({
    page,
  }) => {
    const errors = collectErrors(page, [EXPECTED_400]);
    const unique = Date.now();

    const globalProductId = await createProduct(
      unique,
      `Market Status Product ${unique}`,
    );

    await page.goto(`/regulatory/products/${globalProductId}/registrations`);

    await page.getByRole("button", { name: "Add market" }).click();
    await page.getByLabel("Country").selectOption({ label: "Canada" });
    await page.getByLabel("Present since").fill("2019-06-01");
    await page.getByRole("button", { name: "Add" }).click();

    const status = page.getByTestId("market-status");
    const launched = page.getByTestId("market-launched");

    // --- 1. a market begins as an intention, not a failure to launch -----
    await expect(status).toHaveText("Planned");
    await expect(launched).toHaveText("—");

    // --- 2. on sale ------------------------------------------------------
    await recordSaleStatus(page, "Launched", "2021-03-15");
    await expect(status).toHaveText("Launched");
    await expect(launched).toHaveText("2021-03-15");

    // --- 3. off the shelf, licence untouched -----------------------------
    await recordSaleStatus(
      page,
      "Temporarily unavailable",
      "2023-08-01",
      "API supply interruption.",
    );

    await expect(status).toHaveText("Temporarily unavailable");

    // The launch happened. Losing supply does not un-happen it.
    await expect(launched).toHaveText("2021-03-15");

    // --- 4. business time only moves forward -----------------------------
    await page.getByRole("button", { name: "Record sale status" }).click();
    await page.getByLabel("Now").selectOption({ label: "Discontinued" });
    await page.getByLabel("Took effect on").fill("2020-01-01");
    await page.getByRole("button", { name: "Save" }).click();

    await expect(page.getByTestId("market-status-error")).toContainText(
      "History is read in business time",
    );

    await page.keyboard.press("Escape");
    await expect(status).toHaveText("Temporarily unavailable");

    // --- 5. back on sale, and the launch date is still the first one -----
    await recordSaleStatus(page, "Launched", "2024-02-01");
    await expect(status).toHaveText("Launched");
    await expect(launched).toHaveText("2021-03-15");

    // --- 6. discontinued — and the record itself is untouched ------------
    await recordSaleStatus(page, "Discontinued", "2026-01-15");
    await expect(status).toHaveText("Discontinued");
    await expect(launched).toHaveText("2021-03-15");

    // Commercial state is not operability: the market row is still here,
    // still nameable, still able to hold authorisations.
    await expect(page.getByTestId("product-market-row")).toHaveCount(1);

    expect(errors()).toEqual([]);
  });

  /**
   * Operability — the third question asked of a market, and the one that
   * touches neither of the others.
   *
   * Retiring a market record excludes it from normal work. It does not
   * surrender a licence, does not take a product off sale, and does not delete
   * anything. This walks all three through the browser at once, because the
   * boundary is only convincing when you can see the other two not moving.
   */
  test("retiring a market record touches neither its sale status nor its licences", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();

    const globalProductId = await createProduct(
      unique,
      `Retired Market Product ${unique}`,
    );

    await page.goto(`/regulatory/products/${globalProductId}/registrations`);

    await page.getByRole("button", { name: "Add market" }).click();
    await page.getByLabel("Country").selectOption({ label: "Canada" });
    await page.getByLabel("Present since").fill("2019-06-01");
    await page.getByRole("button", { name: "Add" }).click();

    // A market that is on sale and holds a licence — so retiring it has
    // something to leave alone.
    await recordSaleStatus(page, "Launched", "2021-03-15");

    await page
      .getByTestId("product-market-row")
      .first()
      .getByRole("button", { name: "New registration" })
      .click();

    await page.getByLabel("Authority").selectOption({ index: 1 });
    await page.getByLabel("Authorisation holder").selectOption({ index: 1 });
    await page.getByLabel("Planned on").fill("2020-01-10");
    await page.getByRole("button", { name: "Create" }).click();

    await expect(page.getByTestId("product-registration-row")).toHaveCount(1);

    // --- retire it, and be told what it holds ----------------------------
    await page.getByRole("button", { name: "Retire" }).click();

    // A warning, not a refusal: the licence is a fact about the market, not a
    // reason the record must stay in circulation.
    await expect(page.getByTestId("retire-warning")).toContainText(
      "1 authorisation",
    );

    await page.getByLabel("Retired on").fill("2026-04-01");
    await page.getByRole("button", { name: "Save" }).click();

    // --- the record is retired, and nothing else moved -------------------
    await expect(page.getByTestId("market-retired")).toBeVisible();
    await expect(page.getByTestId("market-status")).toHaveText("Launched");
    await expect(page.getByTestId("market-launched")).toHaveText("2021-03-15");
    await expect(page.getByTestId("product-registration-row")).toHaveCount(1);

    // Retained, not deleted (ES-018): the row is still on the page.
    await expect(page.getByTestId("product-market-row")).toHaveCount(1);

    // --- and it comes back ------------------------------------------------
    await page.getByRole("button", { name: "Restore" }).click();
    await page.getByLabel("Restored on").fill("2026-05-01");
    await page.getByRole("button", { name: "Save" }).click();

    await expect(page.getByTestId("market-retired")).toHaveCount(0);
    await expect(page.getByTestId("market-status")).toHaveText("Launched");

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

async function recordSaleStatus(
  page: import("@playwright/test").Page,
  status: string,
  occurredOn: string,
  note?: string,
) {
  await page.getByRole("button", { name: "Record sale status" }).click();
  await page.getByLabel("Now").selectOption({ label: status });
  await page.getByLabel("Took effect on").fill(occurredOn);
  if (note) await page.getByLabel("Note (optional)").fill(note);
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
