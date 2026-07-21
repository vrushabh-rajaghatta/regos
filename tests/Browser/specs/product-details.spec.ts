import { expect } from "@playwright/test";

import { test, api, collectErrors, EXPECTED_404 } from "./support";

test.describe("Product details", () => {
  test("navigates from the list and projects every field", async ({ page }) => {
    const errors = collectErrors(page);

    await page.goto("/regulatory/products");
    await expect(page.getByTestId("product-list")).toBeVisible();

    await page
      .locator('[data-testid="product-list"] a', { hasText: "Ozempic" })
      .click();

    await expect(page).toHaveURL(/\/regulatory\/products\/[0-9a-f-]{36}/);
    await expect(page.getByTestId("product-code")).toHaveText("OZE-1");
    await expect(page.getByTestId("product-type")).toHaveText("Drug");
    await expect(page.getByTestId("product-status")).toHaveText("Registered");

    expect(errors()).toEqual([]);
  });

  test("shows a distinct not-found state, not a generic error", async ({ page }) => {
    // The 404 this test provokes is logged by the browser as a resource error.
    const errors = collectErrors(page, [EXPECTED_404]);

    await page.goto("/regulatory/products/99999999-9999-9999-9999-999999999999");

    await expect(page.getByTestId("product-not-found")).toBeVisible();
    await expect(page.getByTestId("product-error")).toHaveCount(0);

    expect(errors()).toEqual([]);
  });
});
