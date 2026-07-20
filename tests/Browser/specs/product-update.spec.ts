import { expect, test } from "@playwright/test";

import { api, collectErrors } from "./support";

test.describe("Update product", () => {
  test("edits name and type, and cannot edit the code", async ({ page }) => {
    const errors = collectErrors(page);

    const listed = await (await api("/api/products?search=ASP")).json();
    const product = listed.items[0];
    const restore = { name: product.name, type: product.type };
    const newName = `Aspirin 75mg BR${Date.now().toString().slice(-4)}`;

    await page.goto(`/regulatory/products/${product.id}`);
    await page.getByRole("button", { name: "Edit" }).click();

    // The immutability decision, visible in the UI. The API enforces it
    // independently — see the HTTP verification for PROD-005.
    await expect(page.locator("#code")).toBeDisabled();
    await expect(page.locator("#code")).toHaveValue("ASP-75");

    await page.getByLabel("Product Name").fill(newName);
    await page.getByLabel("Product Type").click();
    await page.getByRole("option", { name: "Medical Device" }).click();
    await page.getByRole("button", { name: "Save" }).click();

    await expect(page.getByTestId("product-type")).toHaveText("MedicalDevice");
    await expect(page.getByTestId("product-code")).toHaveText("ASP-75");

    // Verify the CONSUMER of the invalidated cache, not just the page that
    // performed the mutation — the detail view holds fresh state either way.
    await page.goto("/regulatory/products");
    await expect(page.getByTestId("product-list")).toContainText(newName);

    // Client-side validation keeps the dialog open with a message.
    await page.goto(`/regulatory/products/${product.id}`);
    await page.getByRole("button", { name: "Edit" }).click();
    await page.getByLabel("Product Name").fill("   ");
    await page.getByRole("button", { name: "Save" }).click();
    await expect(page.getByText("Product name is required")).toBeVisible();

    expect(errors()).toEqual([]);

    await api(`/api/products/${product.id}`, {
      method: "PUT",
      body: JSON.stringify(restore),
    });
  });
});
