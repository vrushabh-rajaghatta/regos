import { expect, test } from "@playwright/test";

import { api, collectErrors } from "./support";

test.describe("Update product", () => {
  test("edits name and type, and cannot edit the code", async ({ page }) => {
    const errors = collectErrors(page);

    // Seed our own product rather than reaching for a seeded one. An earlier
    // version of this spec picked the first result for "ASP" and broke the
    // moment that product was archived, because the directory hides archived
    // products - the same ambient-state dependency the README warns about.
    const code = `UPD-${Date.now().toString().slice(-6)}`;
    const created = await (
      await api("/api/products", {
        method: "POST",
        body: JSON.stringify({ code, name: `Update Probe ${code}`, type: "Drug" }),
      })
    ).json();

    const newName = `Renamed ${code}`;

    await page.goto(`/regulatory/products/${created.id}`);
    await page.getByRole("button", { name: "Edit" }).click();

    // The immutability decision, visible in the UI. The API enforces it
    // independently - see the HTTP verification for PROD-005.
    await expect(page.locator("#code")).toBeDisabled();
    await expect(page.locator("#code")).toHaveValue(code);

    await page.getByLabel("Product Name").fill(newName);
    await page.getByLabel("Product Type").click();
    await page.getByRole("option", { name: "Medical Device" }).click();
    await page.getByRole("button", { name: "Save" }).click();

    await expect(page.getByTestId("product-type")).toHaveText("MedicalDevice");
    await expect(page.getByTestId("product-code")).toHaveText(code);

    // Verify the CONSUMER of the invalidated cache, not just the page that
    // performed the mutation - the detail view holds fresh state either way.
    await page.goto("/regulatory/products");
    await expect(page.getByTestId("product-list")).toContainText(newName);

    // Client-side validation keeps the dialog open with a message.
    await page.goto(`/regulatory/products/${created.id}`);
    await page.getByRole("button", { name: "Edit" }).click();
    await page.getByLabel("Product Name").fill("   ");
    await page.getByRole("button", { name: "Save" }).click();
    await expect(page.getByText("Product name is required")).toBeVisible();

    expect(errors()).toEqual([]);

    await api(`/api/products/${created.id}/archive`, { method: "POST" });
  });
});
