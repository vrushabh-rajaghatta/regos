import { expect } from "@playwright/test";

import { test, api, collectErrors } from "./support";

const LIST = "/regulatory/products";

test.describe("Product directory", () => {
  test("renders, searches, filters and registers", async ({ page }) => {
    const errors = collectErrors(page);

    await page.goto(LIST);
    await expect(page.getByTestId("product-list")).toBeVisible();

    // The projection supplies the code, so it must reach the card.
    await expect(page.locator('[data-testid="product-list"] p').first())
      .toContainText("·");
    await expect(page.getByTestId("product-count")).toContainText("product");

    // Wait on expected CONTENT, not a row count: a count-only wait passes
    // while the previous result is still rendered.
    await page.getByLabel("Search products").fill("ozem");
    await expect(page.locator('[data-testid="product-list"] h3'))
      .toHaveText([/Ozempic/]);

    await page.getByLabel("Search products").fill("asp-");
    await expect(page.locator('[data-testid="product-list"] h3'))
      .toHaveText([/Aspirin/]);

    await page.getByLabel("Search products").fill("zzz-no-such-product");
    await expect(page.getByTestId("product-list")).toHaveCount(0);

    await page.getByLabel("Search products").fill("");
    await expect(page.getByTestId("product-list")).toBeVisible();

    // Assert the business outcome, not a count that depends on what happens to
    // be in the database. This originally asserted "Archived is empty", which
    // was only true before archiving existed - the same ambient-state
    // dependency we removed from the .NET integration tests.
    await page.getByLabel("Filter by status").click();
    await page.getByRole("option", { name: "Archived" }).click();

    const badges = page.getByTestId("product-status-badge");

    if (await page.getByTestId("product-list").count()) {
      const statuses = await badges.allTextContents();
      expect(new Set(statuses.map((s) => s.trim()))).toEqual(new Set(["Archived"]));
    }

    await page.getByLabel("Filter by status").click();
    await page.getByRole("option", { name: "All statuses" }).click();
    await expect(page.getByTestId("product-list")).toBeVisible();

    // Register end to end through the dialog.
    const code = `BRW-${Date.now().toString().slice(-6)}`;

    await page.getByRole("button", { name: "New Product" }).click();
    await page.getByLabel("Product Code").fill(code);
    await page.getByLabel("Product Name").fill("Browser Verified Product");
    await page.getByLabel("Product Type").click();
    await page.getByRole("option", { name: "Drug", exact: true }).click();
    await page.getByRole("button", { name: /register|save|create/i }).click();

    await expect(page.getByTestId("product-list")).toContainText(code);

    expect(errors()).toEqual([]);

    // Leave the directory as we found it. Products are never deleted, so the
    // spec archives what it registered - which is exactly what the product
    // owns as a retirement path.
    const created = await (await api(`/api/products?search=${code}`)).json();

    for (const item of created.items) {
      await api(`/api/products/${item.id}/archive`, { method: "POST" });
    }
  });
});
