import { expect } from "@playwright/test";

import {
  DEV_EMAIL,
  DEV_PASSWORD,
  EXPECTED_401,
  anonymousTest as test,
  collectErrors,
} from "./support";

/**
 * The only spec that starts signed out. Everything else is signed in by the
 * `test` fixture; this one exercises the form that produces the session.
 */
test.describe("Sign in", () => {
  test("signs in and lands on the application", async ({ page }) => {
    // Two 401s are expected before signing in: the app asks /api/auth/me who
    // it is, then tries one refresh. With HttpOnly cookies it cannot know it
    // has no session without asking (ADR-025).
    const errors = collectErrors(page, [EXPECTED_401]);

    await page.goto("/login");

    await page.getByLabel("Email Address").fill(DEV_EMAIL);
    await page.getByLabel("Password").fill(DEV_PASSWORD);
    await page.getByRole("button", { name: "Sign In", exact: true }).click();

    // The header renders the email the API reports, so seeing it proves the
    // cookies were set, sent back, validated, and read as claims.
    await expect(page.getByText(DEV_EMAIL)).toBeVisible();

    expect(errors()).toEqual([]);
  });

  test("never sends a tenant header again", async ({ page }) => {
    // The point of AUTH-005, asserted rather than assumed. Tenancy is carried
    // by the token; if X-Tenant-Id ever reappears in browser traffic, two
    // identity systems are running again and this fails (ADR-024).
    const offenders: string[] = [];

    page.on("request", (request) => {
      const headers = request.headers();

      if (headers["x-tenant-id"]) offenders.push(request.url());
    });

    await page.goto("/login");

    await page.getByLabel("Email Address").fill(DEV_EMAIL);
    await page.getByLabel("Password").fill(DEV_PASSWORD);
    await page.getByRole("button", { name: "Sign In", exact: true }).click();

    await expect(page.getByText(DEV_EMAIL)).toBeVisible();

    await page.goto("/regulatory/organizations");
    await expect(page.getByTestId("organization-list")).toBeVisible();

    await page.goto("/regulatory/products");
    await expect(page.locator('[data-testid="product-list"]')).toBeVisible();

    expect(offenders).toEqual([]);
  });

  test("keeps the session out of reach of page scripts", async ({ page }) => {
    // The reason for moving off localStorage (ADR-025). An XSS flaw can still
    // act as the user, but it can no longer read the tokens and replay them
    // somewhere else, later.
    await page.goto("/login");

    await page.getByLabel("Email Address").fill(DEV_EMAIL);
    await page.getByLabel("Password").fill(DEV_PASSWORD);
    await page.getByRole("button", { name: "Sign In", exact: true }).click();

    await expect(page.getByText(DEV_EMAIL)).toBeVisible();

    const reachable = await page.evaluate(() => ({
      cookie: document.cookie,
      storage: JSON.stringify(window.localStorage),
      session: JSON.stringify(window.sessionStorage),
    }));

    expect(reachable.cookie).not.toContain("regos_access");
    expect(reachable.cookie).not.toContain("regos_refresh");
    expect(reachable.storage).not.toContain("eyJ");
    expect(reachable.session).not.toContain("eyJ");
  });

  test("stays signed in across a full page reload", async ({ page }) => {
    // localStorage used to provide this. Cookies must too, or every refresh
    // would sign the user out.
    await page.goto("/login");

    await page.getByLabel("Email Address").fill(DEV_EMAIL);
    await page.getByLabel("Password").fill(DEV_PASSWORD);
    await page.getByRole("button", { name: "Sign In", exact: true }).click();

    await expect(page.getByText(DEV_EMAIL)).toBeVisible();

    await page.reload();

    await expect(page.getByText(DEV_EMAIL)).toBeVisible();
  });

  test("rejects a wrong password without saying why", async ({ page }) => {
    const errors = collectErrors(page, [EXPECTED_401]);

    await page.goto("/login");

    await page.getByLabel("Email Address").fill(DEV_EMAIL);
    await page.getByLabel("Password").fill("not the password");
    await page.getByRole("button", { name: "Sign In", exact: true }).click();

    // One message for every failure (ADR-022). It must not name the email, the
    // account, or which half of the credentials was wrong.
    await expect(page.getByRole("alert")).toHaveText(
      "Invalid email address or password.",
    );

    expect(errors()).toEqual([]);
  });

  test("sends an unknown email to the same message", async ({ page }) => {
    const errors = collectErrors(page, [EXPECTED_401]);

    await page.goto("/login");

    await page.getByLabel("Email Address").fill("nobody@regos.local");
    await page.getByLabel("Password").fill(DEV_PASSWORD);
    await page.getByRole("button", { name: "Sign In", exact: true }).click();

    await expect(page.getByRole("alert")).toHaveText(
      "Invalid email address or password.",
    );

    expect(errors()).toEqual([]);
  });

  test("redirects a signed-out visitor away from a protected page", async ({
    page,
  }) => {
    // Two 401s are expected before signing in: the app asks /api/auth/me who
    // it is, then tries one refresh. With HttpOnly cookies it cannot know it
    // has no session without asking (ADR-025).
    const errors = collectErrors(page, [EXPECTED_401]);

    await page.goto("/regulatory/organizations");

    await expect(page).toHaveURL(/\/login$/);

    // The directory must not render at all — not even briefly with an empty
    // table, which would mean the page had tried to load without a token.
    await expect(page.getByTestId("organization-list")).toHaveCount(0);

    expect(errors()).toEqual([]);
  });

  test("returns to the page that was originally requested", async ({
    page,
  }) => {
    // Two 401s are expected before signing in: the app asks /api/auth/me who
    // it is, then tries one refresh. With HttpOnly cookies it cannot know it
    // has no session without asking (ADR-025).
    const errors = collectErrors(page, [EXPECTED_401]);

    await page.goto("/regulatory/organizations");
    await expect(page).toHaveURL(/\/login$/);

    await page.getByLabel("Email Address").fill(DEV_EMAIL);
    await page.getByLabel("Password").fill(DEV_PASSWORD);
    await page.getByRole("button", { name: "Sign In", exact: true }).click();

    await expect(page).toHaveURL(/\/regulatory\/organizations$/);

    expect(errors()).toEqual([]);
  });

  test("signs out and cannot get back in without signing in again", async ({
    page,
  }) => {
    // Two 401s are expected before signing in: the app asks /api/auth/me who
    // it is, then tries one refresh. With HttpOnly cookies it cannot know it
    // has no session without asking (ADR-025).
    const errors = collectErrors(page, [EXPECTED_401]);

    await page.goto("/login");
    await page.getByLabel("Email Address").fill(DEV_EMAIL);
    await page.getByLabel("Password").fill(DEV_PASSWORD);
    await page.getByRole("button", { name: "Sign In", exact: true }).click();

    await expect(page.getByText(DEV_EMAIL)).toBeVisible();

    await page.getByRole("button", { name: "Sign Out", exact: true }).click();

    await expect(page).toHaveURL(/\/login$/);

    await page.goto("/regulatory/organizations");
    await expect(page).toHaveURL(/\/login$/);

    expect(errors()).toEqual([]);
  });
});
