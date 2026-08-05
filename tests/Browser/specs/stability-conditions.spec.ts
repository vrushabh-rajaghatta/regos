import { expect, type Page } from "@playwright/test";

import { test, api, collectErrors } from "./support";

/**
 * **EPIC-022 S004 — does our stability data support this market?**
 *
 * **The design changed when the source was read**, and this spec exists to
 * prove what it changed to. The plan called for a climatic zone per country and
 * a match on zone letters. WHO publishes the long-term testing *condition* each
 * member state accepts and declines to publish a zone letter per country; ICH
 * withdrew Q1F; and **India accepts 30 °C/70% RH — which is neither Zone IVA
 * (30/65) nor Zone IVB (30/75)**. A stored zone would have been RegOS's reading
 * of WHO rather than WHO's data, so RegOS stores conditions.
 *
 * **The same pack fact, two markets, two answers:**
 *
 * | Market | Accepts | Tested at | |
 * |---|---|---|---|
 * | Germany | 25/60 or 30/65 | 25/60 | accepted |
 * | India | 30/70 | 25/60 | **not accepted** |
 *
 * **And the third assertion is the one the story rests on.** *Not accepted* is
 * advice, never a gate: the supply saves, the pack authorises, and no refusal
 * surface fires. That is the EPIC-005 expiry precedent carried forward, and the
 * call S003 made about a missing label language — derive the interpretation,
 * and let a person decide.
 */
test.describe("Whether this market accepts the pack's stability data", () => {
  test("Germany accepts a pack tested at 25 °C/60% RH", async ({ page }) => {
    const errors = collectErrors(page);

    await openMarket(page, "Germany", "STAB-DE");

    // What the market accepts is stated once, in WHO's own words rather than
    // as a zone letter RegOS never read.
    const accepts = page.getByTestId("market-stability-conditions");

    await expect(accepts).toContainText("25 °C / 60% RH");
    await expect(accepts).toContainText("30 °C / 65% RH");
    await expect(accepts).not.toContainText("IV");

    await addPackTestedAt(page, "25 °C / 60% RH");

    const pack = page.getByTestId("authorised-pack-row").first();

    await expect(pack.getByTestId("pack-stability")).toContainText(
      "25 °C / 60% RH",
    );
    await expect(pack.getByTestId("pack-stability-accepted")).toBeVisible();
    await expect(pack.getByTestId("pack-stability-unaccepted")).toHaveCount(0);

    expect(errors()).toEqual([]);
  });

  test("India does not — and nothing is blocked", async ({ page }) => {
    const errors = collectErrors(page);

    await openMarket(page, "India", "STAB-IN");

    // India's row is the one a careful guess gets wrong, and it is the reason
    // the model holds conditions rather than zones.
    await expect(page.getByTestId("market-stability-conditions")).toContainText(
      "30 °C / 70% RH",
    );

    // --- 1. the supply saves ------------------------------------------------
    // Stating a condition this market does not accept is a legitimate record
    // of what the dossier holds, not an error, and the form does not refuse it.
    await addPackTestedAt(page, "25 °C / 60% RH");

    await expect(page.getByTestId("pack-supply-error")).toHaveCount(0);

    const pack = page.getByTestId("authorised-pack-row").first();

    // --- 2. the advice appears ----------------------------------------------
    await expect(pack.getByTestId("pack-stability-unaccepted")).toContainText(
      "does not accept that condition",
    );

    // Advice, not a rule: muted prose rather than a destructive banner,
    // because a red panel reads as something that stops you.
    await expect(pack.getByTestId("pack-stability")).toHaveClass(
      /text-muted-foreground/,
    );

    // --- 3. the pack still authorises ---------------------------------------
    // The assertion the whole story rests on. An unaccepted condition is a
    // finding about the dossier, not a lock on the licence.
    await page.getByRole("button", { name: "New registration" }).click();
    await page.getByLabel("Authority").selectOption({ index: 1 });
    await page.getByLabel("Authorisation holder").selectOption({ index: 1 });
    await page.getByLabel("Planned on").fill("2026-01-10");
    await page.getByRole("button", { name: "Create" }).click();

    await expect(page.getByTestId("market-registration")).toHaveCount(1);

    await pack.getByTestId("authorise-pack").click();
    await page.getByLabel("Licence").selectOption({ index: 1 });
    await page.getByLabel("Authorised on").fill("2026-02-01");
    await page.getByTestId("confirm-authorise-pack").click();

    await expect(pack.getByTestId("pack-authorised")).toBeVisible();
    await expect(page.getByTestId("authorise-error")).toHaveCount(0);

    // And the advice is still there afterwards — it was never a precondition,
    // and authorising did not make the finding go away.
    await expect(pack.getByTestId("pack-stability-unaccepted")).toBeVisible();

    expect(errors()).toEqual([]);
  });
});

/** Records a market for a fresh global product and opens it. */
async function openMarket(page: Page, country: string, prefix: string) {
  const unique = Date.now();

  const productResponse = await api("/api/products", {
    method: "POST",
    body: JSON.stringify({
      code: `${prefix}-${unique}`,
      name: `Stability Product ${unique}`,
      type: "Drug",
    }),
  });

  const { id: globalProductId } = await productResponse.json();

  await page.goto(`/regulatory/products/${globalProductId}/registrations`);

  await page.getByRole("button", { name: "Add market" }).click();
  await page.getByLabel("Country").selectOption({ label: country });
  await page.getByLabel("Present since").fill("2026-01-05");
  await page.getByRole("button", { name: "Add" }).click();

  await page
    .getByTestId("product-market-row")
    .filter({ hasText: country })
    .getByRole("link", { name: country })
    .click();

  await expect(page.getByTestId("market-overview")).toBeVisible();
}

/**
 * Adds a pack and states what its shelf life was demonstrated under.
 *
 * **The two checkbox groups on that form are deliberately not the same list.**
 * *Storage conditions* is how the pack must be kept — a label instruction.
 * *Shelf life demonstrated at* is the study condition, and only that one
 * decides which markets accept the period. This states both, so the spec proves
 * they are read apart rather than merely that one of them exists.
 */
async function addPackTestedAt(page: Page, condition: string) {
  await page.getByTestId("add-pack").click();
  await page
    .getByLabel("Pack", { exact: true })
    .fill("Carton of 30 tablets");
  await page.getByLabel("Contains").fill("30");
  await page.getByLabel("Of").selectOption({ label: "Tablet" });
  await page.getByLabel("Planned since").fill("2026-02-01");
  await page.getByRole("button", { name: "Add pack" }).last().click();

  const row = page.getByTestId("pack-row").filter({ hasText: "Carton of 30 " });

  await expect(row).toHaveCount(1);

  await row.getByTestId("edit-pack-supply").click();

  await page.getByLabel("Keeps for").fill("36");
  await page.getByLabel("Period").selectOption({ label: "months" });

  // The label instruction. Different vocabulary, different question — and it
  // deliberately does not agree with the testing condition below, because a
  // 30 °C study is what supports a "below 25 °C" label in a hot market.
  await page
    .getByTestId("storage-conditions")
    .getByLabel("Do not store above 25 °C", { exact: true })
    .check();

  await page
    .getByTestId("tested-at")
    .getByLabel(condition, { exact: true })
    .check();

  await page.getByRole("button", { name: "Save supply" }).click();
}
