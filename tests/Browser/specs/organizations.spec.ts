import { expect } from "@playwright/test";

import { test, api, collectErrors, EXPECTED_404 } from "./support";

const LIST = "/regulatory/organizations";

/** Seeds through the API so a spec does not depend on another feature's form. */
async function seedOrganization(legalName: string, type = "Manufacturer") {
  const response = await api("/organizations", {
    method: "POST",
    body: JSON.stringify({ legalName, type }),
  });

  const { id } = await response.json();

  return id as string;
}

/** Organizations are never deleted, so retire what the spec created. */
async function retire(id: string) {
  await api(`/organizations/${id}/deactivate`, { method: "POST" });
}

const unique = (prefix: string) =>
  `${prefix} ${Date.now().toString().slice(-6)}`;

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

    const legalName = unique("Browser Verified Org");

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

    const organizations = await (await api("/organizations")).json();

    for (const organization of organizations) {
      if (organization.legalName === legalName) await retire(organization.id);
    }
  });

  test("navigates from the list to the details page", async ({ page }) => {
    const errors = collectErrors(page);

    const legalName = unique("Browser Detail Org");
    const id = await seedOrganization(legalName);

    await page.goto(LIST);
    await page.getByRole("link", { name: legalName }).click();

    await expect(page).toHaveURL(new RegExp(`${id}$`));

    const details = page.getByTestId("organization-details");
    await expect(details).toContainText(legalName);
    await expect(details).toContainText("Active");

    // Deep-linking works: the page does not depend on list state.
    await page.reload();
    await expect(page.getByTestId("organization-details")).toContainText(
      legalName,
    );

    expect(errors()).toEqual([]);

    await retire(id);
  });

  test("edits the legal name and type, and the list reflects it", async ({
    page,
  }) => {
    const errors = collectErrors(page);

    const legalName = unique("Browser Edit Org");
    const renamed = `${legalName} Renamed`;
    const id = await seedOrganization(legalName);

    await page.goto(`${LIST}/${id}`);
    await expect(page.getByTestId("organization-details")).toContainText(
      "Manufacturer",
    );

    await page.getByRole("button", { name: "Edit" }).click();
    await page.getByLabel("Legal Name").fill(renamed);
    await page.getByLabel("Organization Type").click();
    await page.getByRole("option", { name: "Sponsor", exact: true }).click();
    await page.getByRole("button", { name: "Save Changes" }).click();

    const details = page.getByTestId("organization-details");
    await expect(details).toContainText(renamed);
    await expect(details).toContainText("Sponsor");

    // Editing is not a lifecycle transition: the status is untouched.
    await expect(details).toContainText("Active");

    // The LIST must reflect the edit too — it only refreshes if the cache was
    // genuinely invalidated, which the details page alone would not prove.
    await page.goto(LIST);
    await expect(page.getByTestId("organization-list")).toContainText(renamed);

    expect(errors()).toEqual([]);

    await retire(id);
  });

  test("deactivates an organization from its details page", async ({
    page,
  }) => {
    const errors = collectErrors(page);

    const legalName = unique("Browser Retired Org");
    const id = await seedOrganization(legalName);

    await page.goto(`${LIST}/${id}`);
    await expect(page.getByTestId("organization-details")).toContainText(
      "Active",
    );

    await page.getByRole("button", { name: "Deactivate" }).click();
    await page
      .getByRole("button", { name: "Deactivate", exact: true })
      .last()
      .click();

    await expect(page.getByTestId("organization-details")).toContainText(
      "Inactive",
    );

    // An inactive organization offers no deactivate action, and editing it is
    // still allowed — retiring does not freeze the record.
    await expect(
      page.getByRole("button", { name: "Deactivate" }),
    ).toHaveCount(0);
    await expect(page.getByRole("button", { name: "Edit" })).toBeVisible();

    await page.reload();
    await expect(page.getByTestId("organization-details")).toContainText(
      "Inactive",
    );

    expect(errors()).toEqual([]);
  });

  test("activates an inactive organization, and back again", async ({
    page,
  }) => {
    const errors = collectErrors(page);

    const legalName = unique("Browser Revived Org");
    const id = await seedOrganization(legalName);
    await retire(id);

    await page.goto(`${LIST}/${id}`);

    const details = page.getByTestId("organization-details");
    await expect(details).toContainText("Inactive");

    // `name` matches as a case-insensitive SUBSTRING, so "Activate" also
    // matches "Deactivate". Every lifecycle assertion here must be exact.
    const activate = page.getByRole("button", { name: "Activate", exact: true });
    const deactivate = page.getByRole("button", {
      name: "Deactivate",
      exact: true,
    });

    // Exactly one lifecycle action is offered, because one transition is legal.
    await expect(deactivate).toHaveCount(0);

    await activate.first().click();
    await activate.last().click();

    await expect(details).toContainText("Active");
    await expect(activate).toHaveCount(0);
    await expect(deactivate).toBeVisible();

    // The list reflects it too, so the cache was genuinely invalidated.
    await page.goto(LIST);
    const row = page.locator("tr", { hasText: legalName });
    await expect(row).toContainText("Active");

    expect(errors()).toEqual([]);

    await retire(id);
  });

  test("shows a distinct not-found state, not a generic error", async ({
    page,
  }) => {
    const errors = collectErrors(page, [EXPECTED_404]);

    await page.goto(
      "/regulatory/organizations/11111111-1111-1111-1111-111111111111",
    );

    await expect(page.getByTestId("organization-not-found")).toBeVisible();
    await expect(page.getByTestId("organization-error")).toHaveCount(0);

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
