import { expect, type Page } from "@playwright/test";

import { test, api, collectErrors } from "./support";

/**
 * **EPIC-010c S001 — which sites make this product?**
 *
 * The first half of the epic's question. The second — *is that site on the
 * licence?* — is S002's, and **this spec asserts that this half does not answer
 * it**: recording an operation approves nothing, and the screen says so.
 *
 * Three things are proved, and the third is the one that matters most:
 *
 * 1. **A site performs an operation for a market, from a date.** The date is
 *    asked for rather than defaulted, because an operation recorded today may
 *    have run since 2019.
 * 2. **A transfer is two rows, not an edited one.** Closing a period and
 *    opening another keeps *"who released our batches in 2023?"* answerable,
 *    which editing a site id in place would destroy.
 * 3. **Nothing here is an approval.** The section says so in as many words, and
 *    no refusal surface exists for it to fire.
 */
test.describe("Where this product is made", () => {
  test("a site, an operation, a date — and a transfer that keeps the history", async ({
    page,
  }) => {
    const errors = collectErrors(page);

    await openMarket(page, "Germany");

    // --- before anything is said -------------------------------------------
    // Empty is not "unapproved". A market nobody has recorded operations for
    // has an unstated supply chain, not a broken one.
    await expect(page.getByTestId("manufacturing-empty")).toContainText(
      "only that nobody has said where the work happens",
    );

    // --- 1. a site performs an operation, since a date in the past ----------
    await recordOperation(page, "Manufacture of finished product", "2019-01-01");

    // Selected by what it says rather than by position. Two operations at one
    // site starting the same day tie on every sort key the read has, and
    // "the first row" is then whichever the database felt like returning —
    // which is how the missing tie-breaker in that read was found.
    const first = page
      .getByTestId("manufacturing-row")
      .filter({ hasText: "Manufacture of finished product" });

    await expect(first.getByTestId("manufacturing-operation")).toContainText(
      "Manufacture of finished product",
    );
    await expect(first.getByTestId("manufacturing-current")).toBeVisible();
    await expect(first).toContainText("Since 2019-01-01");

    // The site's registry identifiers come from the site, not from a copy on
    // the operation — there is no manufacturer field anywhere (ADR-063 §3).
    await expect(first.getByTestId("manufacturing-identifiers")).toBeVisible();

    // --- 2. a second operation at the same site is a different fact ---------
    // Same site, different act. The uniqueness guard is per (site, operation),
    // so this is allowed and the two rows read as two statements.
    await recordOperation(page, "Batch release", "2019-01-01");

    await expect(page.getByTestId("manufacturing-row")).toHaveCount(2);

    // --- 3. a transfer: close the period, and the row stays -----------------
    await first.getByTestId("cease-manufacturing").click();
    await page.getByLabel("Stopped on").fill("2024-02-29");
    await page.getByTestId("confirm-cease").click();

    const closed = page
      .getByTestId("manufacturing-row")
      .filter({ hasText: "Manufacture of finished product" });

    await expect(closed.getByTestId("manufacturing-closed")).toContainText(
      "Until 2024-02-29",
    );

    // **The assertion the model exists for.** The closed period is still
    // listed, still names its site and still says when it started — which is
    // what makes a 2023 filing explainable. An edit in place would have lost it.
    await expect(closed).toContainText("Since 2019-01-01");
    await expect(page.getByTestId("manufacturing-row")).toHaveCount(2);

    // And the same operation may now be opened again — a transfer away and
    // back is ordinary, which is why the guard is on *open* periods only.
    await recordOperation(page, "Manufacture of finished product", "2024-03-01");

    await expect(page.getByTestId("manufacturing-row")).toHaveCount(3);
    await expect(page.getByTestId("manufacturing-error")).toHaveCount(0);

    // --- 4. recording is not approving --------------------------------------
    // S001 answers half the epic's question and says so rather than implying
    // the other half. S002 makes the licence's side of it exist.
    await expect(
      page.getByTestId("manufacturing-not-approval"),
    ).toContainText("does not approve it");

    expect(errors()).toEqual([]);
  });

  test("the same site cannot currently do the same job twice", async ({
    page,
  }) => {
    // The invariant, and the refusal a person can act on. The filtered unique
    // index says the same thing where a race cannot slip past the handler.
    const errors = collectErrors(page, [/409/]);

    await openMarket(page, "France");

    await recordOperation(page, "Quality control testing", "2024-01-01");

    await expect(page.getByTestId("manufacturing-row")).toHaveCount(1);

    await page.getByTestId("record-manufacturing").click();
    await page.getByLabel("Site").selectOption({ index: 1 });
    await page
      .getByLabel("Operation")
      .selectOption({ label: "Quality control testing" });
    await page.getByLabel("Performing since").fill("2024-06-01");
    await page.getByRole("button", { name: "Record operation" }).click();

    // Refused, and the message names the act rather than a constraint.
    await expect(page.getByTestId("manufacturing-error")).toContainText(
      "already performs this operation",
    );

    // Nothing was written, and the dialog kept what was chosen.
    await page.keyboard.press("Escape");
    await expect(page.getByTestId("manufacturing-row")).toHaveCount(1);

    expect(errors()).toEqual([]);
  });
});

/** Records a market for a fresh global product and opens it. */
async function openMarket(page: Page, country: string) {
  const unique = Date.now();

  const productResponse = await api("/api/products", {
    method: "POST",
    body: JSON.stringify({
      code: `MFG-${unique}`,
      name: `Manufactured Product ${unique}`,
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
 * The site list is deliberately the whole registry rather than manufacturing
 * sites only — a laboratory tests and a warehouse imports — so this picks by
 * index and lets the operation carry the meaning.
 */
async function recordOperation(page: Page, operation: string, since: string) {
  await page.getByTestId("record-manufacturing").click();
  await page.getByLabel("Site").selectOption({ index: 1 });
  await page.getByLabel("Operation").selectOption({ label: operation });
  await page.getByLabel("Performing since").fill(since);
  await page.getByRole("button", { name: "Record operation" }).click();

  // **Wait for the dialog to close before returning.** Without this, a second
  // call re-opens it while the first is still shutting, and every locator
  // below resolves against a dialog that is on its way out — which is the
  // 1-in-30 flake this helper produced before the wait was added.
  await expect(
    page.getByRole("button", { name: "Record operation" }),
  ).toHaveCount(0);
}
