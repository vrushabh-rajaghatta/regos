import { expect, test } from "@playwright/test";

import { collectErrors } from "./support";

const LIST = "/platform/organizations";

/**
 * The happy-path create is verified manually against a running stack but is
 * deliberately NOT automated here yet.
 *
 * An organization cannot be removed: there is no delete endpoint, and
 * Deactivate exists on the aggregate with no command or endpoint reaching it.
 * A spec that creates one leaks a row on every run, which breaks ADR-019 rule 1
 * and pollutes the applicant dropdown that the regulatory specs rely on.
 *
 * The create test lands with the DeactivateOrganization slice — the first point
 * at which it can clean up after itself.
 */
test.describe("Organization directory", () => {
  test("renders the directory", async ({ page }) => {
    const errors = collectErrors(page);

    await page.goto(LIST);

    await expect(page.getByTestId("organization-list")).toBeVisible();
    await expect(page.getByTestId("organization-count")).toContainText(
      "organization",
    );

    // Every row renders a status, so the projection reaches the table.
    const statuses = await page
      .getByTestId("organization-status")
      .allTextContents();

    expect(statuses.length).toBeGreaterThan(0);
    expect(statuses.every((status) => status.trim().length > 0)).toBe(true);

    expect(errors()).toEqual([]);
  });

  test("rejects an organization with no legal name", async ({ page }) => {
    const errors = collectErrors(page);

    await page.goto(LIST);

    await page.getByRole("button", { name: "Create Organization" }).click();
    await page
      .getByRole("button", { name: "Create Organization", exact: true })
      .last()
      .click();

    await expect(page.getByText("Legal name is required.")).toBeVisible();

    // Nothing was created, so the directory is unchanged.
    expect(errors()).toEqual([]);
  });
});
