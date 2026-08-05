import { expect } from "@playwright/test";

import { test, api, collectErrors } from "./support";

/**
 * **EPIC-022 S002 — which of our markets are in the EU?**
 *
 * The question the story exists for, read off the portfolio's front door.
 *
 * Two things are being proved, and the second is the interesting one:
 *
 * 1. **The groupings overlap.** Germany is EU *and* ICH *and* PIC/S — which the
 *    single nullable `RegionCode` this replaces could not have said even if
 *    anything had ever written to it.
 * 2. **Empty is a recorded answer.** India belongs to none of the five: CDSCO
 *    is an ICH *observer* rather than a member, and India is not a PIC/S
 *    participant ([E37](../../../docs/evidence/EPIC-022/regional-membership.md)).
 *    Both were fetched rather than recalled, and both contradict what a careful
 *    guess produces.
 */
test.describe("Which of our markets are in the EU", () => {
  test("overlapping groupings, and a market that is in none", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();

    // Registrations are what make a country appear on this page at all, so the
    // portfolio needs one in each market being asked about.
    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `REG-${unique}`,
        name: `Regional Product ${unique}`,
        type: "Drug",
      }),
    });

    const { id: globalProductId } = await productResponse.json();

    await page.goto(`/regulatory/products/${globalProductId}/registrations`);

    // Germany is in three groupings, India in none — the two rows that make
    // this question worth asking.
    for (const country of ["Germany", "India"]) {
      await page.getByRole("button", { name: "Add market" }).click();
      await page.getByLabel("Country").selectOption({ label: country });
      await page.getByLabel("Present since").fill("2026-01-05");
      await page.getByRole("button", { name: "Add" }).click();

      await expect(
        page.getByTestId("product-market-row").filter({ hasText: country }),
      ).toHaveCount(1);

      await page
        .getByTestId("product-market-row")
        .filter({ hasText: country })
        .getByRole("link", { name: country })
        .click();

      await expect(page.getByTestId("market-overview")).toBeVisible();

      await page.getByRole("button", { name: "New registration" }).click();
      await page.getByLabel("Authority").selectOption({ index: 1 });
      await page.getByLabel("Authorisation holder").selectOption({ index: 1 });
      await page.getByLabel("Planned on").fill("2026-01-10");
      await page.getByRole("button", { name: "Create" }).click();

      await expect(page.getByTestId("market-registration")).toHaveCount(1);

      await page.goto(`/regulatory/products/${globalProductId}/registrations`);
    }

    // --- the portfolio's front door ----------------------------------------
    await page.goto("/regulatory/registrations");

    const germany = page
      .getByTestId("registration-market")
      .filter({ hasText: "Germany" });

    const india = page
      .getByTestId("registration-market")
      .filter({ hasText: "India" });

    await expect(germany).toHaveCount(1);
    await expect(india).toHaveCount(1);

    // --- 1. they overlap ----------------------------------------------------
    for (const grouping of ["EU", "ICH", "PIC/S"]) {
      await expect(germany).toContainText(grouping);
    }

    // --- 2. India is in none, and says so by carrying no badge -------------
    await expect(india).not.toContainText("ICH");
    await expect(india).not.toContainText("PIC/S");
    await expect(india).not.toContainText("EU");

    // --- 3. the question, asked ---------------------------------------------
    await page.getByTestId("region-filter").selectOption("EU");

    await expect(germany).toHaveCount(1);
    await expect(india).toHaveCount(0);

    // PIC/S keeps Germany and still excludes India — so the filter is reading
    // membership rather than just hiding everything but one row.
    await page.getByTestId("region-filter").selectOption("PIC_S");

    await expect(germany).toHaveCount(1);
    await expect(india).toHaveCount(0);

    await page.getByTestId("region-filter").selectOption("");

    await expect(india).toHaveCount(1);

    // --- 4. the ISO identity S001 added, on the market page ----------------
    await india.click();

    await expect(page.getByTestId("country-iso-identity")).toContainText("IND");

    expect(errors()).toEqual([]);
  });
});
