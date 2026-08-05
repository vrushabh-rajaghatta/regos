import { expect, type Page } from "@playwright/test";

import { test, api, collectErrors } from "./support";

/**
 * **EPIC-010c capstone — where is this product made, and is that site on the
 * licence?**
 *
 * **It introduces nothing.** S001 recorded what happens, S002 recorded what a
 * licence permits, and neither knows the other exists. This reads both and puts
 * them side by side — which is only possible *because* they were built apart.
 * An "approved manufacturing operation" entity would have made the difference
 * between them unrepresentable.
 *
 * | Site | Performs | On a licence | |
 * |---|---|---|---|
 * | Köln | ✓ | ✓ | aligned |
 * | Manchester | ✓ | ✗ | **advisory** |
 * | Hyderabad | ✗ | ✓ | **advisory** |
 *
 * **And the assertion the whole family of findings rests on:** with two
 * divergences on screen, manufacturing continues, approval continues, and
 * nothing anywhere refuses. The fourth time this project has made that call —
 * after an expired registration (EPIC-005), a missing label language and an
 * unaccepted stability condition (EPIC-022).
 */
test.describe("Manufacturing against the licences", () => {
  test("aligned, unapproved, unused — and nothing is blocked", async ({
    page,
  }) => {
    const errors = collectErrors(page);

    await openMarket(page, "Germany");

    // --- before either half exists -----------------------------------------
    await expect(page.getByTestId("alignment-empty")).toContainText(
      "the difference between them appears here",
    );

    const licence = await createLicence(page);

    // --- Köln: performs, and is on the licence ------------------------------
    await recordOperation(page, "Demo Pharma Werk", "Manufacture of finished product");
    await approveSite(page, "Demo Pharma Werk", "2024-03-01");

    // --- Manchester: performs, and is on no licence -------------------------
    await recordOperation(page, "Demo Analytical", "Quality control testing");

    // --- Hyderabad: on the licence, and performs nothing --------------------
    await approveSite(page, "Demo Active Ingredients", "2024-06-01");

    // --- the comparison -----------------------------------------------------
    const koln = row(page, "Demo Pharma Werk");
    const manchester = row(page, "Demo Analytical");
    const hyderabad = row(page, "Demo Active Ingredients");

    await expect(page.getByTestId("alignment-row")).toHaveCount(3);

    await expect(koln.getByTestId("alignment-aligned")).toBeVisible();
    await expect(manchester.getByTestId("alignment-unapproved")).toBeVisible();
    await expect(hyderabad.getByTestId("alignment-unused")).toBeVisible();

    // Each row carries the facts the verdict was derived from, so a reader can
    // see *why* rather than being told a conclusion.
    await expect(koln.getByTestId("alignment-operations")).toContainText(
      "Manufacture of finished product",
    );
    await expect(hyderabad.getByTestId("alignment-operations")).toContainText(
      "—",
    );
    await expect(manchester.getByTestId("alignment-approvals")).toContainText(
      "—",
    );

    await expect(page.getByTestId("alignment-advisory")).toContainText(
      "2 sites differ",
    );

    // --- nothing is blocked -------------------------------------------------
    // **The assertion the story rests on**, and it is three separate acts, not
    // one. With two divergences on screen:

    // 1. manufacturing continues — a fourth operation records normally.
    await recordOperation(page, "Demo Pharma Werk", "Batch release");
    await expect(page.getByTestId("manufacturing-error")).toHaveCount(0);

    // 2. approval continues — the unapproved site can be added to the licence,
    //    which is what closing a divergence actually looks like.
    await approveSite(page, "Demo Analytical", "2025-01-15");
    await expect(page.getByTestId("approve-site-error")).toHaveCount(0);

    // 3. and the advisory follows the facts rather than persisting: Manchester
    //    is now aligned, so only Hyderabad differs.
    await expect(manchester.getByTestId("alignment-aligned")).toBeVisible();
    await expect(page.getByTestId("alignment-advisory")).toContainText(
      "1 site differs",
    );

    // Neither panel it reads from was disturbed by any of it.
    await expect(page.getByTestId("manufacturing-row")).toHaveCount(3);
    await expect(page.getByTestId("approved-site-row")).toHaveCount(3);

    expect(licence).toBeTruthy();
    expect(errors()).toEqual([]);
  });

  test("a closed period is history, not a finding", async ({ page }) => {
    // The distinction that keeps the advisory worth reading: a site that
    // stopped in 2023 is not manufacturing without approval today, and an
    // advisory about it would make every transfer look like a problem.
    const errors = collectErrors(page);

    await openMarket(page, "France");

    await recordOperation(page, "Demo Analytical", "Quality control testing");

    await expect(row(page, "Demo Analytical").getByTestId("alignment-unapproved"))
      .toBeVisible();

    await page.getByTestId("cease-manufacturing").first().click();
    await page.getByLabel("Stopped on").fill("2025-06-30");
    await page.getByTestId("confirm-cease").click();

    // The operation is still listed as history; the comparison drops it.
    await expect(page.getByTestId("manufacturing-closed")).toBeVisible();
    await expect(page.getByTestId("alignment-empty")).toBeVisible();
    await expect(page.getByTestId("alignment-advisory")).toHaveCount(0);

    expect(errors()).toEqual([]);
  });
});

function row(page: Page, siteName: string) {
  return page.getByTestId("alignment-row").filter({ hasText: siteName });
}

/** Records a market for a fresh global product and opens it. */
async function openMarket(page: Page, country: string) {
  const unique = Date.now();

  const productResponse = await api("/api/products", {
    method: "POST",
    body: JSON.stringify({
      code: `ALN-${unique}`,
      name: `Alignment Product ${unique}`,
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

async function createLicence(page: Page): Promise<boolean> {
  await page.getByRole("button", { name: "New registration" }).click();
  await page.getByLabel("Authority").selectOption({ index: 1 });
  await page.getByLabel("Authorisation holder").selectOption({ index: 1 });
  await page.getByLabel("Planned on").fill("2021-01-10");
  await page.getByRole("button", { name: "Create" }).click();

  await expect(page.getByTestId("market-registration")).toHaveCount(1);

  return true;
}

async function recordOperation(
  page: Page,
  siteName: string,
  operation: string,
) {
  await page.getByTestId("record-manufacturing").click();

  const sites = page.getByLabel("Site", { exact: true });
  const chosen = await sites
    .locator("option", { hasText: siteName })
    .first()
    .getAttribute("value");

  await sites.selectOption(chosen!);
  await page.getByLabel("Operation").selectOption({ label: operation });
  await page.getByLabel("Performing since").fill("2024-01-01");
  await page.getByRole("button", { name: "Record operation" }).click();

  // **Wait for the dialog to close before returning.** Without this, a second
  // call re-opens it while the first is still shutting, and every locator
  // below resolves against a dialog that is on its way out — which is the
  // 1-in-30 flake this helper produced before the wait was added.
  await expect(
    page.getByRole("button", { name: "Record operation" }),
  ).toHaveCount(0);
}

async function approveSite(page: Page, siteName: string, approvedOn: string) {
  if ((await page.getByTestId("confirm-approve-site").count()) === 0) {
    await page.getByTestId("approve-site").click();
  }

  await page.getByLabel("Licence", { exact: true }).selectOption({ index: 1 });

  const sites = page.getByLabel("Approved site");
  const chosen = await sites
    .locator("option", { hasText: siteName })
    .first()
    .getAttribute("value");

  await sites.selectOption(chosen!);
  await page.getByLabel("Added to the licence on").fill(approvedOn);
  await page.getByTestId("confirm-approve-site").click();
}
