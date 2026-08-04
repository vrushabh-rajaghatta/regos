import { expect } from "@playwright/test";

import { test, api, collectErrors } from "./support";

/**
 * **EPIC-018 S004 — the rest of what an approved label says.**
 *
 * The slice exists to test two decisions rather than to add two more CRUD
 * screens:
 *
 * 1. **`Population` amends in place on a second and third parent.** S003 proved
 *    it once. If correcting a band on a contraindication produced two rows
 *    instead of one, the entity decision would have been right for indications
 *    and wrong for the shape — which is what
 *    [D2's watchpoint](../../../docs/product/epics/EPIC-018-labeling-and-product-information.md)
 *    was written to catch.
 * 2. **Neither statement owns a history.** There is no decision control here and
 *    no timeline, because a contraindication is content inside an approved
 *    label: what changes it is a new `LocalLabel` revision, not a decision
 *    recorded against it.
 */
test.describe("What else the approved label says", () => {
  test("a contraindication corrected in place, and a side effect with a frequency", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();

    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `CLI-${unique}`,
        name: `Clinical Product ${unique}`,
        type: "Drug",
      }),
    });

    expect(productResponse.ok).toBeTruthy();
    const { id: globalProductId } = await productResponse.json();

    await page.goto(`/regulatory/products/${globalProductId}/registrations`);

    await page.getByRole("button", { name: "Add market" }).click();
    await page.getByLabel("Country").selectOption({ label: "France" });
    await page.getByLabel("Present since").fill("2026-01-05");
    await page.getByRole("button", { name: "Add" }).click();

    await page
      .getByTestId("product-market-row")
      .filter({ hasText: "France" })
      .getByRole("link", { name: "France" })
      .click();

    await expect(page.getByTestId("market-overview")).toBeVisible();

    // --- 1. the commonest contraindication in existence --------------------
    await expect(page.getByTestId("contraindications-empty")).toBeVisible();

    await page.getByTestId("record-contraindications").click();
    await page.getByLabel("Condition").click();
    await page
      .getByRole("option", {
        name: "Hypersensitivity to the active substance",
        exact: true,
      })
      .click();
    await page
      .getByLabel("As the label says it")
      .fill("Hypersensitivity to the active substance or any excipient.");
    await page.getByRole("button", { name: "Record" }).last().click();

    const contraindication = page
      .getByTestId("contraindications-row")
      .first();

    await expect(contraindication).toBeVisible();

    // Neither statement type has a decision control or a timeline. Asserted,
    // because it is the design rather than an unfinished screen.
    await expect(
      contraindication.getByTestId("withdraw-indication"),
    ).toHaveCount(0);
    await expect(
      contraindication.getByTestId("indication-history"),
    ).toHaveCount(0);

    // --- 2. the qualifier, corrected in place on the SECOND parent ---------
    await contraindication.getByTestId("add-statement-population").click();
    await page.getByLabel("From age").fill("12");
    await page.getByLabel("Unit").click();
    await page.getByRole("option", { name: "years", exact: true }).click();
    await page.getByRole("button", { name: "Add population" }).click();

    const populations = contraindication.getByTestId(
      "statement-population-row",
    );

    await expect(populations).toHaveCount(1);
    await expect(populations.first()).toContainText("12+ years");

    // The assertion S004 exists for: after the correction there is still one
    // qualifier, because it is the same qualifier.
    await populations.first().getByTestId("correct-statement-population").click();
    await page.getByLabel("From age").fill("6");
    await page.getByRole("button", { name: "Save correction" }).click();

    await expect(populations).toHaveCount(1);
    await expect(populations.first()).toContainText("6+ years");

    // --- 3. a side effect, with the one field the three do not share -------
    await page.getByTestId("record-undesirable-effects").click();
    await page.getByLabel("Effect", { exact: true }).click();
    await page.getByRole("option", { name: "Nausea", exact: true }).click();
    await page
      .getByLabel("As the label says it")
      .fill("Nausea, usually mild and transient.");
    await page.getByLabel("How often").click();
    await page.getByRole("option", { name: "Common", exact: true }).click();
    await page.getByRole("button", { name: "Record" }).last().click();

    const effect = page.getByTestId("undesirable-effects-row").first();

    await expect(effect).toBeVisible();
    await expect(effect.getByTestId("statement-frequency")).toContainText(
      "Common",
    );

    // --- 4. and the third parent amends the same way ----------------------
    await effect.getByTestId("add-statement-population").click();
    await page.getByLabel("Applies to").click();
    await page.getByRole("option", { name: "Female", exact: true }).click();
    await page.getByRole("button", { name: "Add population" }).click();

    const effectPopulations = effect.getByTestId("statement-population-row");

    await expect(effectPopulations).toHaveCount(1);

    await effectPopulations
      .first()
      .getByTestId("correct-statement-population")
      .click();
    await page.getByLabel("Physiological condition").click();
    await page.getByRole("option", { name: "Pregnancy", exact: true }).click();
    await page.getByRole("button", { name: "Save correction" }).click();

    await expect(effectPopulations).toHaveCount(1);
    await expect(effectPopulations.first()).toContainText("Pregnancy");

    // --- 5. the indication above still has its history --------------------
    // The asymmetry, visible on one screen: an authorisation carries decisions,
    // the content of a label does not.
    await expect(page.getByTestId("market-indications")).toBeVisible();

    expect(errors()).toEqual([]);
  });
});
