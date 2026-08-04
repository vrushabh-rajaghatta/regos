import { expect } from "@playwright/test";

import { test, api, collectErrors, EXPECTED_409 } from "./support";

/**
 * **EPIC-018 S005 — what this product clashes with.**
 *
 * The fourth clinical statement, and it applies settled patterns: a coded
 * classification, the label's own wording, an owned population that amends in
 * place, and no history of its own.
 *
 * **Two things are new, and both are asserted here:**
 *
 * 1. **An interaction must name at least one interactant.** Every other
 *    statement is meaningful alone — a contraindication with no population
 *    applies to everyone. An interaction with nothing to interact with is not
 *    an under-specified statement; it is not a statement. The aggregate refuses
 *    to remove the last one.
 * 2. **The interactant may point at a `Substance`** — the seam `OtherTherapy`
 *    said would arrive *"beside the text, never instead of it"*. Most
 *    interactants are not compounds RegOS knows, so the text is required and the
 *    link is not.
 */
test.describe("What this product clashes with", () => {
  test("an interaction linked to the catalogue, and the last interactant that cannot be removed", async ({
    page,
  }) => {
    // Removing the last interactant is a 409, and the spec asserts that refusal.
    const errors = collectErrors(page, [EXPECTED_409]);
    const unique = Date.now();

    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `INT-${unique}`,
        name: `Interacting Product ${unique}`,
        type: "Drug",
      }),
    });

    expect(productResponse.ok).toBeTruthy();
    const { id: globalProductId } = await productResponse.json();

    await page.goto(`/regulatory/products/${globalProductId}/registrations`);

    await page.getByRole("button", { name: "Add market" }).click();
    await page.getByLabel("Country").selectOption({ label: "Canada" });
    await page.getByLabel("Present since").fill("2026-01-05");
    await page.getByRole("button", { name: "Add" }).click();

    await page
      .getByTestId("product-market-row")
      .filter({ hasText: "Canada" })
      .getByRole("link", { name: "Canada" })
      .click();

    await expect(page.getByTestId("market-overview")).toBeVisible();
    await expect(page.getByTestId("interactions-empty")).toBeVisible();

    // --- 1. an interaction with something the catalogue knows --------------
    await page.getByTestId("record-interaction").click();

    await page
      .getByLabel("Interacts with")
      .fill("warfarin and other coumarin anticoagulants");

    // The optional seam. Paracetamol is in the shared demonstration catalogue.
    await page.getByLabel("Link to a substance").click();
    await page.getByRole("option", { name: "Paracetamol", exact: true }).click();

    await page
      .getByLabel("What happens")
      .fill("Concomitant use increases the anticoagulant effect.");
    await page
      .getByLabel("What to do")
      .fill("Monitor INR and adjust the dose.");

    await page.getByLabel("Severity").click();
    await page.getByRole("option", { name: "Major", exact: true }).click();

    await page.getByRole("button", { name: "Record interaction" }).click();

    const row = page.getByTestId("interaction-row").first();

    await expect(row).toBeVisible();
    await expect(row.getByTestId("interactant")).toHaveCount(1);
    await expect(row.getByTestId("interaction-severity")).toContainText("Major");
    await expect(row.getByTestId("interaction-management")).toContainText(
      "Monitor INR",
    );

    // --- 2. the qualifier, amended in place on the FOURTH parent -----------
    await row.getByTestId("add-interaction-population").click();
    await page.getByLabel("From age").fill("65");
    await page.getByLabel("Unit").click();
    await page.getByRole("option", { name: "years", exact: true }).click();
    await page.getByRole("button", { name: "Add population" }).click();

    const populations = row.getByTestId("interaction-population-row");

    await expect(populations).toHaveCount(1);
    await expect(populations.first()).toContainText("65+ years");

    await populations
      .first()
      .getByTestId("correct-interaction-population")
      .click();
    await page.getByLabel("From age").fill("75");
    await page.getByRole("button", { name: "Save correction" }).click();

    await expect(populations).toHaveCount(1);
    await expect(populations.first()).toContainText("75+ years");

    // --- 3. the last interactant cannot be removed ------------------------
    // The invariant S005 adds to the context, asserted through the API because
    // the screen offers no way to reach the state at all — which is itself the
    // point: the rule is in the aggregate, not in the button.
    const interactions = await (
      await api(`/api/medicinal-products/${await marketIdOf(page)}/interactions`)
    ).json();

    const interaction = interactions[0];

    expect(interaction.interactants).toHaveLength(1);

    const refused = await api(
      `/api/interactions/${interaction.id}/interactants/${interaction.interactants[0].id}`,
      { method: "DELETE" },
    );

    expect(refused.status, "removing the last interactant").toBe(409);

    // Add a second, and now the first can go.
    const added = await api(`/api/interactions/${interaction.id}/interactants`, {
      method: "POST",
      body: JSON.stringify({ description: "and other CYP3A4 inhibitors" }),
    });

    expect(added.ok).toBeTruthy();

    const allowed = await api(
      `/api/interactions/${interaction.id}/interactants/${interaction.interactants[0].id}`,
      { method: "DELETE" },
    );

    expect(allowed.ok, "removing one of two interactants").toBeTruthy();

    await page.reload();

    await expect(
      page.getByTestId("interaction-row").first().getByTestId("interactant"),
    ).toHaveCount(1);

    expect(errors()).toEqual([]);
  });
});

type Page = import("@playwright/test").Page;

/** The market id, taken from the URL the browser is already on. */
async function marketIdOf(page: Page): Promise<string> {
  const match = page.url().match(/markets\/([0-9a-f-]{36})/i);

  expect(match, "a market id in the URL").toBeTruthy();

  return match![1];
}
