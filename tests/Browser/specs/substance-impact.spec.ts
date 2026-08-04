import { expect } from "@playwright/test";

import { test, api, collectErrors } from "./support";

/**
 * **EPIC-010a's capstone: the question the epic was built to answer.**
 *
 * > *Which of our products contain this substance?*
 *
 * Not another CRUD flow. Every other spec in this epic proves one capability;
 * this one proves they **compose** — that the aggregate boundaries chosen in
 * S001–S004 assemble into a feature a regulatory user can act on:
 *
 * ```
 * Substance → Ingredient → PharmaceuticalProductDetail → MedicinalProduct
 *           → GlobalProduct + Country → API → UI
 * ```
 *
 * **Every hop is a join on an id.** That is what the split into `Substance` and
 * `Ingredient` bought, and the only reason the question can be asked backwards
 * at all: a composition that stored substance *names* could be read forwards
 * only, and this would be a string match over free text (ADR-058 §1).
 *
 * The spec walks it in the order a person would live it — add the compound,
 * enter two markets, state what each is, say what each contains — and then asks
 * the question from the substance's own row and expects both answers.
 */
test.describe("Which products contain this substance", () => {
  test("a proprietary compound, two markets, and one question that finds both", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();
    const compound = `RGX-${unique}`;

    // --- 1. the substance, which exists before any product uses it --------
    await page.goto("/regulatory/substances");

    await page.getByTestId("add-substance").click();
    await page.getByLabel("Name", { exact: true }).fill(compound);
    await page.getByLabel("Molecular formula").fill("C21H28N6O3");
    await page.getByRole("button", { name: "Add substance" }).last().click();

    const substanceRow = page
      .getByTestId("substance-row")
      .filter({ hasText: compound });

    await expect(substanceRow).toBeVisible();

    // Nothing contains it yet, and the screen says so rather than showing an
    // empty list that could be mistaken for a failure.
    await substanceRow.getByTestId("show-substance-usage").click();
    await expect(page.getByTestId("substance-usage-empty")).toBeVisible();

    // --- 2. a product in two markets --------------------------------------
    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `CAP-${unique}`,
        name: `Capstone Product ${unique}`,
        type: "Drug",
      }),
    });

    expect(productResponse.ok).toBeTruthy();
    const { id: globalProductId } = await productResponse.json();

    await page.goto(`/regulatory/products/${globalProductId}/registrations`);

    await addMarket(page, "Canada");
    await addMarket(page, "United States");

    // --- 3. what the product is, and what it contains, in each market ------
    // Two markets, two presentations, one substance — the shape that makes the
    // answer worth asking for rather than obvious.
    await enterMarket(page, "Canada");
    await addPresentation(page, "Film-coated tablet, 10 mg", "Film-coated tablet");
    await addIngredient(page, compound, "10");

    await page.goto(`/regulatory/products/${globalProductId}/registrations`);
    await enterMarket(page, "United States");
    await addPresentation(page, "Solution for injection", "Solution for injection");
    await addIngredient(page, compound, "25");

    // --- 4. the question, asked from the substance ------------------------
    await page.goto("/regulatory/substances");
    await page.getByTestId("substance-search").fill(compound);

    await page
      .getByTestId("substance-row")
      .filter({ hasText: compound })
      .getByTestId("show-substance-usage")
      .click();

    const usage = page.getByTestId("substance-usage-row");

    await expect(usage).toHaveCount(2);

    // Both markets, each with the strength recorded *there* — the join carried
    // the ingredient's own strength through, not the product's.
    const canada = usage.filter({ hasText: "Canada" });
    await expect(canada).toContainText("Film-coated tablet, 10 mg");
    await expect(canada).toContainText("10 mg");
    await expect(canada).toContainText("Active");

    const usa = usage.filter({ hasText: "United States" });
    await expect(usa).toContainText("Solution for injection");
    await expect(usa).toContainText("25 mg");

    // The impact question is about what is on sale, so the market's commercial
    // status travels with the answer.
    await expect(canada).toContainText("Planned");

    // --- 5. and the answer leads back to where the work would be done -----
    await canada.getByRole("link", { name: `Capstone Product ${unique}` }).click();

    await expect(page.getByTestId("market-overview")).toBeVisible();
    await expect(
      page.getByTestId("presentation-row").filter({ hasText: "10 mg" }),
    ).toBeVisible();

    expect(errors()).toEqual([]);
  });
});

type Page = import("@playwright/test").Page;

async function addMarket(page: Page, country: string) {
  await page.getByRole("button", { name: "Add market" }).click();
  await page.getByLabel("Country").selectOption({ label: country });
  await page.getByLabel("Present since").fill("2026-01-05");
  await page.getByRole("button", { name: "Add" }).click();

  await expect(
    page.getByTestId("product-market-row").filter({ hasText: country }),
  ).toBeVisible();
}

async function enterMarket(page: Page, country: string) {
  await page
    .getByTestId("product-market-row")
    .filter({ hasText: country })
    .getByRole("link", { name: country })
    .click();

  await expect(page.getByTestId("market-overview")).toBeVisible();
}

async function addPresentation(page: Page, name: string, doseForm: string) {
  await page.getByTestId("add-presentation").click();
  await page.getByLabel("Name", { exact: true }).fill(name);
  await page.getByLabel("Dose form").click();
  await page.getByRole("option", { name: doseForm, exact: true }).click();
  await page.getByRole("button", { name: "Add presentation" }).last().click();

  await expect(
    page.getByTestId("presentation-row").filter({ hasText: name }),
  ).toBeVisible();
}

async function addIngredient(page: Page, substance: string, milligrams: string) {
  await page.getByTestId("add-ingredient").click();

  await page.getByLabel("Substance").click();
  await page.getByRole("option", { name: substance, exact: false }).click();

  await page.getByLabel("Strength").fill(milligrams);
  await page.getByLabel("Unit", { exact: true }).click();
  await page.getByRole("option", { name: "mg", exact: true }).click();

  await page.getByRole("button", { name: "Add ingredient" }).last().click();

  await expect(
    page.getByTestId("ingredient-row").filter({ hasText: substance }),
  ).toBeVisible();
}
