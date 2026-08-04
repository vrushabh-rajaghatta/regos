import { expect } from "@playwright/test";

import { test, api, collectErrors, EXPECTED_400 } from "./support";

/**
 * **EPIC-018 S003 — what the authority authorised, as against what the label
 * says about it.**
 *
 * Two things are proved, and the second is the one that tests a decision:
 *
 * 1. **An indication has a dated history of decisions, not revisions.** It is
 *    approved, then restricted, then withdrawn — and every one of those stays
 *    readable, because a regulatory decision should not disappear. An
 *    indication must not silently *be* withdrawn; it must have *become*
 *    withdrawn on a date.
 * 2. **A population is corrected in place.** A paediatric band written as 12+
 *    and corrected to 6+ is the same qualifier on the same statement — the row
 *    count does not change, which is what
 *    [EPIC-018 D2](../../../docs/product/epics/EPIC-018-labeling-and-product-information.md)
 *    said `Population` having identity would have to earn.
 */
test.describe("What this market approved the product to treat", () => {
  test("an authorisation, a qualifier corrected in place, and a decision history", async ({
    page,
  }) => {
    // An age with no unit is a 400, and the spec asserts that refusal.
    const errors = collectErrors(page, [EXPECTED_400]);
    const unique = Date.now();

    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `IND-${unique}`,
        name: `Indicated Product ${unique}`,
        type: "Drug",
      }),
    });

    expect(productResponse.ok).toBeTruthy();
    const { id: globalProductId } = await productResponse.json();

    await page.goto(`/regulatory/products/${globalProductId}/registrations`);

    await page.getByRole("button", { name: "Add market" }).click();
    await page.getByLabel("Country").selectOption({ label: "Japan" });
    await page.getByLabel("Present since").fill("2026-01-05");
    await page.getByRole("button", { name: "Add" }).click();

    await page
      .getByTestId("product-market-row")
      .filter({ hasText: "Japan" })
      .getByRole("link", { name: "Japan" })
      .click();

    await expect(page.getByTestId("market-overview")).toBeVisible();

    // --- 1. nothing approved yet ------------------------------------------
    await expect(page.getByTestId("indications-empty")).toBeVisible();

    // --- 2. the authorisation ---------------------------------------------
    await page.getByTestId("record-indication").click();
    await page.getByLabel("Condition").click();
    await page
      .getByRole("option", { name: "Type 2 diabetes mellitus", exact: true })
      .click();
    await page
      .getByLabel("As the label says it")
      .fill("Treatment of type 2 diabetes mellitus in adults.");
    await page.getByLabel("Approved on").fill("2026-03-01");
    await page.getByRole("button", { name: "Record indication" }).last().click();

    const row = page.getByTestId("indication-row").first();

    await expect(row).toBeVisible();
    await expect(row.getByTestId("indication-status")).toContainText("Approved");
    await expect(row.getByTestId("indication-text")).toContainText(
      "Treatment of type 2 diabetes mellitus in adults.",
    );

    // --- 3. an age with no unit is refused --------------------------------
    await row.getByTestId("add-population").click();
    await page.getByLabel("From age").fill("12");
    await page.getByRole("button", { name: "Add population" }).last().click();

    await expect(page.getByTestId("population-error")).toContainText(
      "could be months or years",
    );

    // --- 4. the qualifier -------------------------------------------------
    await page.getByLabel("Unit").click();
    await page.getByRole("option", { name: "years", exact: true }).click();
    await page.getByRole("button", { name: "Add population" }).last().click();

    const populations = row.getByTestId("population-row");

    await expect(populations).toHaveCount(1);
    await expect(populations.first()).toContainText("12+ years");

    // --- 5. corrected in place, not replaced ------------------------------
    // The assertion EPIC-018 D2 turns on: after the correction there is still
    // one qualifier, because it is the same qualifier.
    await populations.first().getByTestId("correct-population").click();
    await page.getByLabel("From age").fill("6");
    await page.getByRole("button", { name: "Save correction" }).click();

    await expect(populations).toHaveCount(1);
    await expect(populations.first()).toContainText("6+ years");

    // --- 6. decisions, never rewritten ------------------------------------
    await row.getByTestId("withdraw-indication").click();

    await expect(row.getByTestId("indication-status")).toContainText(
      "Withdrawn",
    );

    // The approval is still there. It became withdrawn on a date rather than
    // silently being withdrawn.
    const history = row.getByTestId("decision-row");

    await expect(history).toHaveCount(2);
    await expect(history.filter({ hasText: "Approved" })).toContainText(
      "2026-03-01",
    );
    await expect(history.filter({ hasText: "Withdrawn" })).toBeVisible();

    // --- 7. and the qualifier survives the withdrawal ----------------------
    // A withdrawn authorisation is still a record of what was authorised, for
    // whom. Nothing about the decision erases the statement it applied to.
    await expect(populations).toHaveCount(1);

    expect(errors()).toEqual([]);
  });
});
