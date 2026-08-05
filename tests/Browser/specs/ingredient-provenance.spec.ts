import { expect, type Page } from "@playwright/test";

import { test, api, collectErrors } from "./support";

/**
 * **EPIC-010c S003 — where does this ingredient come from?**
 *
 * The story exists for **D2**, and this spec is the case that decides it. The
 * two facts are close enough to look like one, and the ordinary configuration
 * separates them:
 *
 * ```
 * Finished product          made at Site Gamma      ← ManufacturingOperation
 * ├── API A                 from Site Alpha         ← Ingredient source
 * └── API B                 from Site Beta          ← Ingredient source
 * ```
 *
 * **Neither is derivable from the other.** An operation set cannot say which
 * active came from where; a source cannot say who packed the carton. Everything
 * below is one product carrying all three statements at once, without either
 * panel disturbing the other.
 *
 * It also takes a seam `Ingredient` recorded in EPIC-010a, which named its own
 * trigger — *"sourcing belongs to cluster D"*. Cluster D is this epic.
 */
test.describe("Where an ingredient comes from", () => {
  test("two actives from two sites, a finished product made at a third", async ({
    page,
  }) => {
    const errors = collectErrors(page);

    await openMarket(page, "Germany");

    // --- the finished product is made at one site --------------------------
    await page.getByTestId("record-manufacturing").click();
    await page.getByLabel("Site").selectOption({ index: 1 });
    await page
      .getByLabel("Operation")
      .selectOption({ label: "Manufacture of finished product" });
    await page.getByLabel("Performing since").fill("2024-01-01");
    await page.getByRole("button", { name: "Record operation" }).click();

    const madeAt = await page
      .getByTestId("manufacturing-row")
      .first()
      .locator("span")
      .first()
      .innerText();

    await expect(page.getByTestId("manufacturing-row")).toHaveCount(1);

    // --- a presentation with two actives from two other sites --------------
    await page.getByTestId("add-presentation").click();
    await page
      .getByLabel("Name", { exact: true })
      .fill("Film-coated tablet, dual active");
    await page.getByLabel("Dose form").click();
    await page.getByRole("option", { name: "Tablet", exact: true }).click();
    await page.getByRole("button", { name: "Add presentation" }).last().click();

    await addActive(page, "Paracetamol", "500", "Demo Analytical");
    await addActive(page, "Ibuprofen", "200", "Demo Active Ingredients");

    // --- the assertion D2 exists for ---------------------------------------
    // Two actives, two different sources, in one composition — which no set of
    // finished-product operations could express, because an operation names
    // the product and not the substance.
    const sources = page.getByTestId("ingredient-source");

    await expect(sources).toHaveCount(2);
    await expect(sources.nth(0)).toContainText("Demo");
    await expect(sources.nth(1)).toContainText("Demo");

    const [firstSource, secondSource] = await sources.allInnerTexts();

    expect(
      firstSource,
      "two actives sourced from two different sites",
    ).not.toEqual(secondSource);

    // --- and neither is the site the product is made at ---------------------
    // The three statements coexist. If provenance were the operation restated,
    // one of these would have to equal the other.
    expect(firstSource).not.toContain(madeAt);
    expect(secondSource).not.toContain(madeAt);

    // The operations panel is untouched by any of it: still exactly the one
    // finished-product row, still naming its own site.
    await expect(page.getByTestId("manufacturing-row")).toHaveCount(1);
    await expect(page.getByTestId("manufacturing-row").first()).toContainText(
      madeAt,
    );

    expect(errors()).toEqual([]);
  });

  test("an ingredient nobody has sourced says nothing at all", async ({
    page,
  }) => {
    // **Absent means "nobody has said", never "unsourced".** RegOS holds no
    // provenance for anything recorded before this story, so a row reading
    // "source: not stated" on every ingredient would turn an honest absence
    // into a nag.
    const errors = collectErrors(page);

    await openMarket(page, "France");

    await page.getByTestId("add-presentation").click();
    await page.getByLabel("Name", { exact: true }).fill("Film-coated tablet");
    await page.getByLabel("Dose form").click();
    await page.getByRole("option", { name: "Tablet", exact: true }).click();
    await page.getByRole("button", { name: "Add presentation" }).last().click();

    await addActive(page, "Paracetamol", "500");

    await expect(page.getByTestId("ingredient-row")).toHaveCount(1);
    await expect(page.getByTestId("ingredient-source")).toHaveCount(0);

    // --- and it can be given one afterwards, without moving the substance ---
    await page.getByTestId("edit-ingredient").first().click();
    await page
      .getByLabel("Sourced from (optional)")
      .selectOption({ index: 1 });
    await page.getByRole("button", { name: /ingredient/ }).last().click();

    await expect(page.getByTestId("ingredient-source")).toHaveCount(1);
    await expect(page.getByTestId("ingredient-row").first()).toContainText(
      "Paracetamol",
    );

    expect(errors()).toEqual([]);
  });
});

/** Records a market for a fresh global product and opens it. */
async function openMarket(page: Page, country: string) {
  const unique = Date.now();

  const productResponse = await api("/api/products", {
    method: "POST",
    body: JSON.stringify({
      code: `SRC-${unique}`,
      name: `Sourced Product ${unique}`,
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
 * Adds an active, optionally naming where its substance comes from.
 *
 * The source picker is a native select whose empty option is a real choice —
 * *"nobody has said"* — unlike the unit pickers beside it, which are Radix
 * comboboxes and refuse an empty value.
 */
async function addActive(
  page: Page,
  substance: string,
  strength: string,
  sourceName?: string,
) {
  await page.getByTestId("add-ingredient").click();

  await page.getByLabel("Substance").click();
  await page.getByRole("option", { name: substance, exact: true }).click();

  await page.getByLabel("Strength").fill(strength);
  await page.getByLabel("Unit", { exact: true }).click();
  await page.getByRole("option", { name: "mg", exact: true }).click();

  if (sourceName) {
    const sites = page.getByLabel("Sourced from (optional)");

    const chosen = await sites
      .locator("option", { hasText: sourceName })
      .first()
      .getAttribute("value");

    await sites.selectOption(chosen!);
  }

  await page.getByRole("button", { name: "Add ingredient" }).last().click();
}
