import { expect } from "@playwright/test";

import { test, api, collectErrors, EXPECTED_400 } from "./support";

/**
 * **EPIC-010a S002 — what a product *is* in one market.**
 *
 * The panel is unremarkable; what this spec is for is the two decisions
 * underneath it.
 *
 * **A market may have several presentations, and nothing constrains it.** 10 mg
 * and 20 mg tablets are one commercial presence — one set of trade names, one
 * commercial history, one set of licences — and forcing a tenant to duplicate
 * the whole market to record the second strength would be the wrong shape. So
 * the spec adds two to one market and expects both.
 *
 * **Restate replaces the whole statement, including its routes.** That is why
 * the aggregate has one `Restate` rather than five setters, why the repository
 * must `Include` the routes, and why the form opens pre-filled. A correction
 * that silently dropped the routes it did not mention would be invisible here
 * unless the spec checks a route survives one.
 *
 * The ATC refusal at the end is the honest half of ADR-058 §6: RegOS checks the
 * shape of a code it cannot verify, and says so rather than implying it holds
 * WHO ATC.
 */
test.describe("market presentations", () => {
  test("records what a product is, corrects it, and keeps two presentations apart", async ({
    page,
  }) => {
    const errors = collectErrors(page, [EXPECTED_400]);
    const unique = Date.now();

    // --- a market to hang presentations from ------------------------------
    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `PRES-${unique}`,
        name: `Presentation Subject ${unique}`,
        type: "Drug",
      }),
    });

    expect(productResponse.ok).toBeTruthy();
    const { id: globalProductId } = await productResponse.json();

    // Entered through the UI, the same way EPIC-017's own spec does — a market
    // is a business act, and driving it any other way would prove less.
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

    await expect(page.getByTestId("market-overview")).toBeVisible();

    // --- nothing yet, and the empty state says why that is ordinary --------
    await expect(page.getByTestId("presentations-empty")).toBeVisible();

    // --- the first presentation, with two routes --------------------------
    await page.getByTestId("add-presentation").click();

    await page.getByLabel("Name", { exact: true }).fill("Solution for injection, 10 mg/mL");
    await page.getByLabel("Dose form").click();
    await page.getByRole("option", { name: "Solution for injection", exact: true }).click();

    await page.getByLabel("Unit of presentation").click();
    await page.getByRole("option", { name: "Vial", exact: true }).click();

    // Several routes is the ordinary case, not the exception.
    await page.getByTestId("route-INTRAVENOUS").click();
    await page.getByTestId("route-INTRAMUSCULAR").click();

    await page
      .getByRole("button", { name: "Add presentation" })
      .last()
      .click();

    const injection = page
      .getByTestId("presentation-row")
      .filter({ hasText: "Solution for injection, 10 mg/mL" });

    await expect(injection).toBeVisible();
    await expect(injection).toContainText("Intravenous");
    await expect(injection).toContainText("Intramuscular");
    await expect(injection).toContainText("per vial");

    // Whose word this is, said out loud — RegOS holds no EDQM licence.
    await expect(injection).toContainText("RegOS terminology");

    // --- a second presentation in the same market -------------------------
    await page.getByTestId("add-presentation").click();

    await page.getByLabel("Name", { exact: true }).fill("Film-coated tablet, 20 mg");
    await page.getByLabel("Dose form").click();
    await page.getByRole("option", { name: "Film-coated tablet", exact: true }).click();
    await page.getByTestId("route-ORAL").click();

    await page
      .getByRole("button", { name: "Add presentation" })
      .last()
      .click();

    await expect(page.getByTestId("presentation-row")).toHaveCount(2);

    // --- restating replaces the whole statement ---------------------------
    const tablet = page
      .getByTestId("presentation-row")
      .filter({ hasText: "Film-coated tablet, 20 mg" });

    await tablet.getByTestId("edit-presentation").click();

    // The form opens on what the presentation currently says — the route
    // included. Only the name is changed here, so a Restate that dropped the
    // unmentioned route would show up as "No route recorded" below.
    await page.getByLabel("Name", { exact: true }).fill("Film-coated tablet, 40 mg");

    await page
      .getByRole("button", { name: "Save presentation" })
      .last()
      .click();

    const restated = page
      .getByTestId("presentation-row")
      .filter({ hasText: "Film-coated tablet, 40 mg" });

    await expect(restated).toBeVisible();
    await expect(restated).toContainText("Oral");

    // The other presentation is untouched: they are separate aggregates.
    await expect(injection).toContainText("Intravenous");
    await expect(page.getByTestId("presentation-row")).toHaveCount(2);

    // --- ATC: the shape is checked, membership is not ---------------------
    await page.getByRole("button", { name: "ATC code" }).click();

    await page.getByLabel("ATC code").fill("NOT-A-CODE");
    await page.getByRole("button", { name: "Save" }).last().click();

    await expect(page.getByTestId("atc-code-error")).toContainText(
      "does not hold the WHO ATC index",
    );

    await page.getByLabel("ATC code").fill("N02BE01");
    await page.getByRole("button", { name: "Save" }).last().click();

    await expect(page.getByTestId("market-overview")).toContainText("N02BE01");

    expect(errors()).toEqual([]);
  });
});
