import { expect } from "@playwright/test";

import { EXPECTED_401, collectErrors, test } from "./support";

/**
 * What happens when the access token runs out.
 *
 * Access tokens last fifteen minutes, so this is the single most common event
 * in a working session and the one no other spec would ever reach. Rather than
 * wait, the access cookie is deleted — indistinguishable, from the browser's
 * point of view, from one that expired.
 */
test.describe("Session refresh", () => {
  test("renews an expired access token without the user noticing", async ({
    page,
    context,
  }) => {
    // The 401 that triggers the refresh is the behaviour under test, not a
    // failure, so it is declared rather than hidden.
    const errors = collectErrors(page, [EXPECTED_401]);

    await page.goto("/regulatory/organizations");
    await expect(page.getByTestId("organization-list")).toBeVisible();

    const before = await context.cookies();
    const refreshBefore = before.find((c) => c.name === "regos_refresh")?.value;

    expect(refreshBefore).toBeTruthy();

    await context.clearCookies({ name: "regos_access" });

    await page.goto("/regulatory/products");

    // No redirect to /login, and the data still loads: the app refreshed and
    // replayed the request on its own.
    await expect(page.locator('[data-testid="product-list"]')).toBeVisible();
    await expect(page).toHaveURL(/\/regulatory\/products$/);

    const after = await context.cookies();

    expect(after.find((c) => c.name === "regos_access")?.value).toBeTruthy();

    // Rotation: renewing the session replaced the refresh token too.
    expect(after.find((c) => c.name === "regos_refresh")?.value)
      .not.toBe(refreshBefore);

    expect(errors()).toEqual([]);
  });

  test("sends the user to sign in when the whole session is gone", async ({
    page,
    context,
  }) => {
    const errors = collectErrors(page, [EXPECTED_401]);

    await page.goto("/regulatory/organizations");
    await expect(page.getByTestId("organization-list")).toBeVisible();

    await context.clearCookies();

    await page.goto("/regulatory/organizations");

    await expect(page).toHaveURL(/\/login$/);

    expect(errors()).toEqual([]);
  });
});
