import { expect } from "@playwright/test";

import { test, api, collectErrors, EXPECTED_409 } from "./support";

/**
 * **EPIC-010a S003 — composition, and the rule that keeps it coherent.**
 *
 * This is the story that makes the epic's headline question askable at all: an
 * ingredient points at a `Substance` by id rather than repeating its name, so
 * *"which of our products contain substance X?"* can be asked backwards.
 *
 * Three things are proved here that a unit test cannot.
 *
 * **A strength survives the round trip as numbers.** `10 mg / 1 mL` is stored
 * as four columns, not a string — the whole reason `Strength` is a value
 * object — and the screen composes the sentence back.
 *
 * **The last active cannot be removed while excipients remain.** That rule
 * reads the whole composition, so it only holds if the repository loaded the
 * whole composition. This is precisely the `Include` EPIC-019 got wrong once,
 * and a spec is the only place a missing one shows up.
 *
 * **An excipient may be recorded first.** Requiring an active on every edit
 * would dictate the order a user types a formulation in; completeness is stated
 * on screen rather than refused by the write path.
 */
test.describe("composition", () => {
  test("records what a presentation is made of, and refuses to hollow it out", async ({
    page,
  }) => {
    const errors = collectErrors(page, [EXPECTED_409]);
    const unique = Date.now();

    // --- a market with a presentation to compose --------------------------
    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `COMP-${unique}`,
        name: `Composition Subject ${unique}`,
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
      .first()
      .getByRole("link", { name: "Canada" })
      .click();

    await page.getByTestId("add-presentation").click();
    await page
      .getByLabel("Name", { exact: true })
      .fill("Solution for injection, 10 mg/mL");
    await page.getByLabel("Dose form").click();
    await page
      .getByRole("option", { name: "Solution for injection", exact: true })
      .click();
    await page.getByTestId("route-INTRAVENOUS").click();
    await page.getByRole("button", { name: "Add presentation" }).last().click();

    await expect(page.getByTestId("composition-empty")).toBeVisible();

    // --- an excipient first, which the write path must accept -------------
    await page.getByTestId("add-ingredient").click();

    await page.getByLabel("Substance").click();
    await page.getByRole("option", { name: "Metformin", exact: true }).click();
    await page.getByLabel("Role").click();
    await page.getByRole("option", { name: /^Excipient/ }).click();

    await page.getByRole("button", { name: "Add ingredient" }).last().click();

    // Accepted, and said to be incomplete — a statement, not a refusal.
    await expect(page.getByTestId("composition-incomplete")).toBeVisible();

    // --- the active, as a concentration -----------------------------------
    await page.getByTestId("add-ingredient").click();

    await page.getByLabel("Substance").click();
    await page.getByRole("option", { name: "Paracetamol", exact: true }).click();

    await page.getByLabel("Strength").fill("10");
    await page.getByLabel("Unit", { exact: true }).click();
    await page.getByRole("option", { name: "mg", exact: true }).click();

    await page.getByLabel("Per", { exact: true }).fill("1");
    await page.getByLabel("Per unit").click();
    await page.getByRole("option", { name: "mL", exact: true }).click();

    await page.getByRole("button", { name: "Add ingredient" }).last().click();

    const paracetamol = page
      .getByTestId("ingredient-row")
      .filter({ hasText: "Paracetamol" });

    // Four numbers in, one sentence out — the round trip that a string column
    // could not have survived.
    await expect(paracetamol).toContainText("10 mg / 1 mL");
    await expect(paracetamol).toContainText("Active");

    await expect(page.getByTestId("composition-incomplete")).toHaveCount(0);

    // --- the same substance twice is one fact stated twice -----------------
    await page.getByTestId("add-ingredient").click();

    await page.getByLabel("Substance").click();
    await page.getByRole("option", { name: "Paracetamol", exact: true }).click();
    await page.getByLabel("Strength").fill("5");
    await page.getByLabel("Unit", { exact: true }).click();
    await page.getByRole("option", { name: "mg", exact: true }).click();

    await page.getByRole("button", { name: "Add ingredient" }).last().click();

    await expect(page.getByTestId("ingredient-error")).toContainText(
      "already in this composition",
    );

    await page.keyboard.press("Escape");

    // --- the last active cannot be removed while excipients remain --------
    // The guard reads the whole composition, so this only passes if the
    // repository loaded the whole composition.
    await paracetamol.getByTestId("remove-ingredient").click();

    await expect(page.getByTestId("remove-ingredient-error")).toContainText(
      "at least one active ingredient",
    );

    await expect(page.getByTestId("ingredient-row")).toHaveCount(2);

    // --- correcting a strength keeps the substance ------------------------
    await paracetamol.getByTestId("edit-ingredient").click();

    await page.getByLabel("Strength").fill("20");

    await page.getByRole("button", { name: "Save ingredient" }).last().click();

    await expect(
      page.getByTestId("ingredient-row").filter({ hasText: "Paracetamol" }),
    ).toContainText("20 mg / 1 mL");

    // --- the excipient goes, and the composition survives -----------------
    await page
      .getByTestId("ingredient-row")
      .filter({ hasText: "Metformin" })
      .getByTestId("remove-ingredient")
      .click();

    await expect(page.getByTestId("ingredient-row")).toHaveCount(1);

    expect(errors()).toEqual([]);
  });
});
