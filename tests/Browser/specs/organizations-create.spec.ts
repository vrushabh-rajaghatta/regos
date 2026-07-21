import { expect, test } from "@playwright/test";

import { api, collectErrors } from "./support";

const LIST = "/platform/organizations";

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

  test("creates an organization and shows it in the list", async ({ page }) => {
    const errors = collectErrors(page);

    await page.goto(LIST);
    await expect(page.getByTestId("organization-list")).toBeVisible();

    const legalName = `Browser Verified Org ${Date.now().toString().slice(-6)}`;

    await page.getByRole("button", { name: "Create Organization" }).click();
    await page.getByLabel("Legal Name").fill(legalName);
    await page.getByLabel("Organization Type").click();
    await page.getByRole("option", { name: "Sponsor", exact: true }).click();
    await page
      .getByRole("button", { name: "Create Organization", exact: true })
      .last()
      .click();

    // The LIST must show it, not a detail view: the list only refreshes if the
    // query cache was genuinely invalidated (ADR-019 rule 4).
    await expect(page.getByTestId("organization-list")).toContainText(legalName);

    // Survives a full reload, so it is persisted rather than client state.
    await page.reload();
    await expect(page.getByTestId("organization-list")).toContainText(legalName);

    expect(errors()).toEqual([]);

    // Leave the directory as we found it. Organizations are never deleted, so
    // the spec deactivates what it created - the retirement path the domain
    // owns, and the same approach products-list.spec.ts takes with archive.
    const organizations = await (await api("/organizations")).json();

    for (const organization of organizations) {
      if (organization.legalName === legalName) {
        await api(`/organizations/${organization.id}/deactivate`, {
          method: "POST",
        });
      }
    }
  });

  test("deactivates an organization from the directory", async ({ page }) => {
    const errors = collectErrors(page);

    const legalName = `Browser Retired Org ${Date.now().toString().slice(-6)}`;

    // Seed through the API rather than the UI: this spec is about the
    // deactivate flow, and creating via the form would make it depend on
    // another feature's markup.
    await api("/organizations", {
      method: "POST",
      body: JSON.stringify({ legalName, type: "Manufacturer" }),
    });

    await page.goto(LIST);

    const row = page.locator("tr", { hasText: legalName });
    await expect(row).toContainText("Active");

    await row.getByRole("button", { name: `Deactivate ${legalName}` }).click();
    await page.getByRole("button", { name: "Deactivate", exact: true }).click();

    // The list reflects the new status without a manual reload.
    await expect(row).toContainText("Inactive");

    // An inactive organization offers no deactivate action.
    await expect(
      row.getByRole("button", { name: `Deactivate ${legalName}` }),
    ).toHaveCount(0);

    await page.reload();
    await expect(page.locator("tr", { hasText: legalName })).toContainText(
      "Inactive",
    );

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
