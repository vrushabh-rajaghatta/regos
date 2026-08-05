import { expect, type Page } from "@playwright/test";

import { test, api, collectErrors } from "./support";

/**
 * **EPIC-010c S002 — which sites does this market's licence approve?**
 *
 * The other half of the epic's question, and the half a regulator owns. S001
 * records what we do; this records what the authorisation permits. **They are
 * deliberately not joined** — comparing them is S004's, and merging them would
 * make a divergence impossible to see.
 *
 * Three things are proved:
 *
 * 1. **A site joins a licence on its own date.** Asked for, never defaulted: a
 *    licence granted in 2021 that added a packaging site in 2024 by variation
 *    has two dates, and only one of them is the registration's. This is the
 *    second time the project has needed that fact after `PackAuthorisation`,
 *    which is why the pattern was copied rather than abstracted (ADR-018).
 * 2. **One site, two licences.** A plant supplying two of this market's
 *    licences is one approved site with two dates — not two rows, and not a
 *    conflict.
 * 3. **Nothing here knows what the site does.** The operations panel above it
 *    is untouched by approving a site, which is what leaves a gap for S004 to
 *    report.
 */
test.describe("Which sites this market's licences approve", () => {
  test("a site on a licence, dated — and the same site on a second one", async ({
    page,
  }) => {
    const errors = collectErrors(page);

    await openMarket(page, "Germany");

    // --- before any licence exists ------------------------------------------
    // A market with no authorisation cannot approve anything, and saying so
    // beats an empty list that reads as a gap.
    await expect(
      page.getByTestId("approved-sites-no-licence"),
    ).toContainText("A market approves sites through its authorisations");

    await expect(page.getByTestId("approve-site")).toBeDisabled();

    // --- 1. a licence, and a site added to it years later -------------------
    const first = await createLicence(page, "2021-01-10");

    await expect(page.getByTestId("approved-sites-empty")).toBeVisible();

    await approveSite(page, first, "Demo Pharma Werk", "2024-03-01");

    const site = page.getByTestId("approved-site-row").first();

    await expect(site).toContainText("Demo Pharma Werk");
    await expect(site.getByTestId("site-approved-on")).toContainText(
      "2024-03-01",
    );

    // --- 2. the same site, a second licence, a different date ---------------
    const second = await createLicence(page, "2023-05-01");

    await approveSite(page, second, "Demo Pharma Werk", "2023-09-15");

    // One site, two approvals — grouped, because the question is about the
    // site rather than about the licences.
    await expect(page.getByTestId("approved-site-row")).toHaveCount(1);
    await expect(site.getByTestId("site-approval-row")).toHaveCount(2);
    await expect(site).toContainText("2 licences");

    // --- 3. approving says nothing about what the site does -----------------
    // The operations panel is still empty. That gap is the whole subject of
    // S004, and it exists precisely because these two are separate statements.
    await expect(page.getByTestId("manufacturing-empty")).toBeVisible();

    // --- 4. a correction removes the row; it is not a variation -------------
    await site
      .getByTestId("site-approval-row")
      .first()
      .getByTestId("withdraw-site-approval")
      .click();

    await expect(site.getByTestId("site-approval-row")).toHaveCount(1);
    await expect(page.getByTestId("withdraw-approval-error")).toHaveCount(0);

    expect(errors()).toEqual([]);
  });

  test("the same site cannot be named twice on one licence", async ({
    page,
  }) => {
    const errors = collectErrors(page, [/409/]);

    await openMarket(page, "France");

    const licence = await createLicence(page, "2022-02-02");

    await approveSite(page, licence, "Demo Analytical", "2022-03-01");

    await expect(page.getByTestId("approved-site-row")).toHaveCount(1);

    // Same pair again. Refused, and the message names the act rather than a
    // database constraint.
    await approveSite(page, licence, "Demo Analytical", "2022-09-01");

    await expect(page.getByTestId("approve-site-error")).toContainText(
      "already approves that site",
    );

    await expect(page.getByTestId("site-approval-row")).toHaveCount(1);

    expect(errors()).toEqual([]);
  });
});

/** Records a market for a fresh global product and opens it. */
async function openMarket(page: Page, country: string) {
  const unique = Date.now();

  const productResponse = await api("/api/products", {
    method: "POST",
    body: JSON.stringify({
      code: `APR-${unique}`,
      name: `Approved Sites Product ${unique}`,
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

/** Creates a licence and returns the number it was given. */
async function createLicence(page: Page, plannedOn: string): Promise<string> {
  const before = await page.getByTestId("market-registration").count();

  await page.getByRole("button", { name: "New registration" }).click();
  await page.getByLabel("Authority").selectOption({ index: 1 });
  await page.getByLabel("Authorisation holder").selectOption({ index: 1 });
  await page.getByLabel("Planned on").fill(plannedOn);
  await page.getByRole("button", { name: "Create" }).click();

  await expect(page.getByTestId("market-registration")).toHaveCount(before + 1);

  return "Number not issued";
}

/**
 * Names a site on a licence, on a date.
 *
 * The licence is chosen by index because an unapproved registration has no
 * number yet — which is itself the ordinary case this panel has to handle.
 */
async function approveSite(
  page: Page,
  _licence: string,
  siteName: string,
  approvedOn: string,
) {
  // Opened only if it is closed: the button toggles, and a refused submission
  // leaves the row open with what was chosen still in it — which is the
  // behaviour the duplicate case depends on.
  if ((await page.getByTestId("confirm-approve-site").count()) === 0) {
    await page.getByTestId("approve-site").click();
  }

  // **`exact` matters here.** getByLabel is substring *and* case-insensitive,
  // so a bare "Licence" also matches "Added to the licence on" one field over —
  // the same trap "White" matching "Off-white" set in EPIC-010b.
  const licences = page.getByLabel("Licence", { exact: true });
  const count = await licences.locator("option").count();

  // The most recently created licence is last in the list.
  await licences.selectOption({ index: count - 1 });

  // Chosen by the option's value rather than its label: the label carries the
  // country too ("Demo Pharma Werk Köln — Germany"), and selectOption matches a
  // label exactly, so hard-coding the whole string would couple this spec to
  // the seed's punctuation rather than to the site it means.
  const sites = page.getByLabel("Approved site");
  const chosen = await sites
    .locator("option", { hasText: siteName })
    .first()
    .getAttribute("value");

  await sites.selectOption(chosen!);

  await page.getByLabel("Added to the licence on").fill(approvedOn);
  await page.getByTestId("confirm-approve-site").click();
}
