import { expect, test } from "@playwright/test";

import { api, collectErrors } from "./support";

test.describe("Archive product", () => {
  test("retires a product from the directory without deleting it", async ({ page }) => {
    const errors = collectErrors(page);

    const code = `ARC-${Date.now().toString().slice(-6)}`;
    const created = await (
      await api("/api/products", {
        method: "POST",
        body: JSON.stringify({ code, name: `Archive Probe ${code}`, type: "Drug" }),
      })
    ).json();

    // Visible in the directory to begin with.
    await page.goto("/regulatory/products");
    await expect(page.getByTestId("product-list")).toContainText(code);

    await page.goto(`/regulatory/products/${created.id}`);
    await page.getByRole("button", { name: "Archive" }).click();

    // The confirmation must not imply deletion - existing regulatory work is
    // untouched, and that is what the wording promises.
    await expect(page.getByText(/not affected/i)).toBeVisible();
    await page.getByTestId("confirm-archive").click();

    // Status changes in place; the product remains readable.
    await expect(page.getByTestId("product-status")).toHaveText("Archived");
    await expect(page.getByTestId("product-code")).toHaveText(code);

    // Archiving has nowhere further to go, so the action is gone.
    await expect(page.getByRole("button", { name: "Archive" })).toHaveCount(0);

    // The consumer of the invalidated cache: the directory hides it by default.
    await page.goto("/regulatory/products");
    await expect(page.getByTestId("product-list")).toBeVisible();
    await expect(page.getByTestId("product-list")).not.toContainText(code);

    // But it is hidden, not deleted - the filter still finds it.
    await page.getByLabel("Filter by status").click();
    await page.getByRole("option", { name: "Archived" }).click();
    await expect(page.getByTestId("product-list")).toContainText(code);

    expect(errors()).toEqual([]);
  });
});
