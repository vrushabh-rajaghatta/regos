import { expect, type Page } from "@playwright/test";

import { test, api, collectErrors, EXPECTED_404 } from "./support";

/**
 * EPIC-016's definition of done, walked in a real browser:
 * create an organization, give it the identity it carries, add a division, a
 * site and a contact, then find that site in the country-filtered directory.
 *
 * The last step is the point. It is the question that made OrganizationSite an
 * aggregate root rather than a child of Organization, and it is the only one
 * here that cannot be answered from inside a single company's workspace.
 */
const ORGS = "/regulatory/organizations";

/** Expected while proving the one-identifier-per-scheme rule. */
const EXPECTED_409 = /409 \(Conflict\)/;

const unique = (prefix: string) =>
  `${prefix} ${Date.now().toString().slice(-6)}`;

/** Organizations are never deleted, so retire what the spec created. */
async function retire(legalName: string) {
  const organizations = await (await api("/api/organizations")).json();

  const created = organizations.find(
    (organization: { legalName: string }) =>
      organization.legalName === legalName,
  );

  if (created) {
    await api(`/api/organizations/${created.id}/deactivate`, {
      method: "POST",
    });
  }
}

async function createOrganization(page: Page, legalName: string) {
  await page.goto(ORGS);
  await expect(page.getByTestId("organization-list")).toBeVisible();

  await page.getByRole("button", { name: "Create Organization" }).click();
  await page.getByLabel("Legal Name").fill(legalName);
  await page.getByLabel("Organization Type").click();
  await page.getByRole("option", { name: "Manufacturer", exact: true }).click();
  await page
    .getByRole("button", { name: "Create Organization", exact: true })
    .last()
    .click();

  await page.locator("tr", { hasText: legalName }).getByRole("link").click();

  await expect(page.getByTestId("organization-workspace-header")).toBeVisible();
}

function tab(page: Page, name: string) {
  return page
    .getByTestId("organization-workspace-nav")
    .getByRole("link", { name });
}

test.describe("Organization workspace", () => {
  test("records a company, its structure, and finds its site in the directory", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const legalName = unique("Workspace Pharma");
    const siteName = unique("Hyderabad Plant");

    await createOrganization(page, legalName);

    // --- the identity S003 modelled and S004 made reachable ---
    await page.getByRole("button", { name: "Edit" }).click();
    await page.getByLabel("Acronym").fill("WSP");
    await page.getByLabel("Name (Native Language)").fill("ワークスペース製薬");
    await page.getByRole("button", { name: "Save Changes" }).click();

    await expect(page.getByTestId("organization-details")).toContainText("WSP");
    await expect(page.getByTestId("organization-details")).toContainText(
      "ワークスペース製薬",
    );

    // --- an identifier, read back with the scheme's code ---
    await page.getByRole("button", { name: "Record Identifier" }).first().click();
    await page.getByLabel("Scheme").click();
    await page.getByRole("option", { name: /DUNS/ }).click();
    await page.getByLabel("Identifier Value").fill("150483782");
    await page
      .getByRole("button", { name: "Record Identifier", exact: true })
      .last()
      .click();

    await expect(page.getByTestId("organization-identifier")).toContainText(
      "DUNS",
    );
    await expect(page.getByTestId("organization-identifier")).toContainText(
      "150483782",
    );

    // --- a division ---
    await tab(page, "Divisions").click();

    await page.getByRole("button", { name: "Add Division" }).click();
    await page.getByLabel("Name").fill("Regulatory Affairs");
    await page.getByLabel("Acronym").fill("RA");
    await page
      .getByRole("button", { name: "Add Division", exact: true })
      .last()
      .click();

    await expect(page.getByTestId("division-row")).toContainText(
      "Regulatory Affairs",
    );

    // --- a site ---
    await tab(page, "Sites").click();

    await page.getByRole("button", { name: "Add Site" }).click();
    await page.getByLabel("Site Name").fill(siteName);
    await page.getByLabel("Site Type").click();
    await page
      .getByRole("option", { name: "Manufacturing", exact: true })
      .click();
    await page.getByLabel("Country").click();
    await page.getByRole("option", { name: "India", exact: true }).click();
    await page.getByLabel("City").fill("Hyderabad");
    await page
      .getByRole("button", { name: "Add Site", exact: true })
      .last()
      .click();

    await expect(page.getByTestId("site-row")).toContainText(siteName);

    // --- a contact, at that site ---
    await tab(page, "Contacts").click();

    await page.getByRole("button", { name: "Add Contact" }).click();
    await page.getByLabel("First Name").fill("Asha");
    await page.getByLabel("Last Name").fill("Rao");
    await page.getByLabel("Title").fill("Qualified Person");
    await page.getByLabel("Role").click();
    await page.getByRole("option", { name: "Qualified Person" }).click();
    await page.getByLabel("Site").click();
    await page.getByRole("option", { name: siteName }).click();
    await page.getByLabel("Email").fill("asha.rao@example.com");
    await page
      .getByRole("button", { name: "Add Contact", exact: true })
      .last()
      .click();

    await expect(page.getByTestId("contact-row")).toContainText("Asha Rao");
    await expect(page.getByTestId("contact-row")).toContainText(siteName);

    // --- and the site, found across the registry rather than within it ---
    await page.goto("/regulatory/sites");
    await page.getByTestId("site-country-filter").click();
    await page.getByRole("option", { name: "India", exact: true }).click();

    const row = page.locator('[data-testid="site-directory-row"]', {
      hasText: siteName,
    });

    // The directory names the owning company, which the workspace never had to.
    await expect(row).toContainText(legalName);
    await expect(row).toContainText("Manufacturing");

    expect(errors()).toEqual([]);

    await retire(legalName);
  });

  /**
   * The regression this story earned the hard way.
   *
   * Six forms in this slice awaited mutateAsync with no catch, so a refused
   * command rendered its message AND escaped to the window as an unhandled
   * page error. A throwaway walk found it; collectErrors is what pins it,
   * because the visible alert looked correct on its own.
   */
  test("a refused command states its reason without crashing the page", async ({
    page,
  }) => {
    const errors = collectErrors(page, [EXPECTED_409]);
    const legalName = unique("Workspace Duplicate");

    await createOrganization(page, legalName);

    for (const value of ["150483782", "999999999"]) {
      await page
        .getByRole("button", { name: "Record Identifier" })
        .first()
        .click();

      await page.getByLabel("Scheme").click();
      await page.getByRole("option", { name: /DUNS/ }).click();
      await page.getByLabel("Identifier Value").fill(value);
      await page
        .getByRole("button", { name: "Record Identifier", exact: true })
        .last()
        .click();
    }

    // The server's words, not a paraphrase written here.
    await expect(page.getByRole("alert")).toContainText(
      "already has an identifier from that scheme",
    );

    // The dialog stays open with the values in it, so the user can correct them.
    await expect(page.getByLabel("Identifier Value")).toHaveValue("999999999");

    await page.keyboard.press("Escape");

    // One identifier, not two: the refusal changed nothing.
    await expect(page.getByTestId("organization-identifier")).toHaveCount(1);

    expect(errors()).toEqual([]);

    await retire(legalName);
  });

  test("withdrawing an identifier removes it", async ({ page }) => {
    const errors = collectErrors(page);
    const legalName = unique("Workspace Withdraw");

    await createOrganization(page, legalName);

    await page.getByRole("button", { name: "Record Identifier" }).first().click();
    await page.getByLabel("Scheme").click();
    await page.getByRole("option", { name: /FEI/ }).click();
    await page.getByLabel("Identifier Value").fill("3001234567");
    await page
      .getByRole("button", { name: "Record Identifier", exact: true })
      .last()
      .click();

    await expect(page.getByTestId("organization-identifier")).toHaveCount(1);

    await page.getByRole("button", { name: "Withdraw" }).click();

    await expect(page.getByTestId("organization-identifier")).toHaveCount(0);

    expect(errors()).toEqual([]);

    await retire(legalName);
  });

  /**
   * Not-found is answered by the workspace layout rather than by each of the
   * four pages, so the sidebar must not render either — a nav offering
   * Divisions and Sites for a company that does not exist is worse than none.
   */
  test("an organization that does not exist says so, and shows no tabs", async ({
    page,
  }) => {
    const errors = collectErrors(page, [EXPECTED_404]);

    await page.goto(`${ORGS}/11111111-1111-1111-1111-111111111111`);

    await expect(page.getByTestId("organization-not-found")).toBeVisible();
    await expect(page.getByTestId("organization-error")).toHaveCount(0);
    await expect(page.getByTestId("organization-workspace-nav")).toHaveCount(0);

    expect(errors()).toEqual([]);
  });

  test("the contact directory spans the registry and filters by role", async ({
    page,
  }) => {
    const errors = collectErrors(page);

    await page.goto("/regulatory/contacts");

    await expect(page.getByTestId("contact-directory-count")).toBeVisible();

    const unfiltered = await page
      .getByTestId("contact-directory-row")
      .count();

    await page.getByTestId("contact-role-filter").click();
    await page.getByRole("option", { name: "Qualified Person" }).click();

    // Filtering can only narrow. Nothing is hidden by default — the unfiltered
    // directory is a legitimate question, not an accident.
    await expect(page.getByTestId("contact-directory-count")).toBeVisible();

    expect(await page.getByTestId("contact-directory-row").count())
      .toBeLessThanOrEqual(unfiltered);

    expect(errors()).toEqual([]);
  });
});
