import { expect } from "@playwright/test";

import { test, collectErrors } from "./support";

test.describe("Regulatory templates explorer", () => {
  test("lists templates and renders the FDA IND blueprint end to end", async ({
    page,
  }) => {
    const errors = collectErrors(page);

    // List
    await page.goto("/regulatory/templates");
    await expect(page.getByTestId("template-list")).toBeVisible();

    const row = page.getByTestId("template-row").first();
    await expect(row).toContainText("FDA IND");

    // Detail — the blueprint renders end to end.
    await row.click();

    const tree = page.getByTestId("blueprint-tree");
    await expect(tree).toBeVisible();

    // Structure: modules and a nested CMC subsection.
    await expect(tree).toContainText("Administrative Information");
    await expect(tree).toContainText("3.2.S");
    await expect(tree).toContainText("Drug Substance");
    await expect(tree).toContainText("3.2.P.8");

    // Content: at least one required document, resolved to its type name.
    await expect(page.getByTestId("required-document").first()).toBeVisible();
    await expect(tree).toContainText("Cover Letter");

    // Constraints: the version-wide rule block, and a section-scoped rule.
    await expect(page.getByTestId("blueprint-version-rules")).toContainText(
      "Error",
    );
    await expect(tree.getByTestId("blueprint-rule").first()).toBeVisible();

    await page.screenshot({
      path: "test-results/templates-blueprint.png",
      fullPage: true,
    });

    expect(errors()).toEqual([]);
  });
});
