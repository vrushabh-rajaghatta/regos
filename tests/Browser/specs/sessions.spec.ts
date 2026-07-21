import { expect } from "@playwright/test";

import {
  API_URL,
  DEV_EMAIL,
  DEV_PASSWORD,
  EXPECTED_401,
  anonymousTest as test,
  collectErrors,
} from "./support";

/**
 * The sessions page.
 *
 * Unlike the change-password spec this one mutates nothing durable: it creates
 * extra sessions for the development account and ends them again, which is what
 * the page is for. Sessions are disposable by design — signing in is how you
 * make another one — so this leaves the account exactly as it found it.
 */

/** Signs in over HTTP, producing a session that is not this browser's. */
async function signInElsewhereAsync(userAgent: string) {
  const response = await fetch(`${API_URL}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json", "User-Agent": userAgent },
    body: JSON.stringify({ email: DEV_EMAIL, password: DEV_PASSWORD }),
  });

  if (!response.ok) {
    throw new Error(`Could not create a second session (${response.status}).`);
  }
}

test.describe("Active sessions", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/login");

    await page.getByLabel("Email Address").fill(DEV_EMAIL);
    await page.getByLabel("Password", { exact: true }).fill(DEV_PASSWORD);
    await page.getByRole("button", { name: "Sign In" }).click();

    await expect(page).not.toHaveURL(/\/login$/);

    // Start from exactly one session: the development account accumulates one
    // per sign-in across every suite - 81 of them when this spec was written -
    // and nothing expires them for fourteen days. Using the feature itself to
    // tidy up is both deterministic and honest, and leaves the account cleaner
    // than it was found. The accumulation is real and is recorded as
    // token-cleanup debt, not hidden by this line.
    const tidied = await page.request.post(
      `${API_URL}/api/auth/sessions/revoke-others`,
    );

    expect(tidied.status()).toBe(204);
  });

  test("lists this device and marks it as current", async ({ page }) => {
    const errors = collectErrors(page, [EXPECTED_401]);

    await page.goto("/settings/sessions");

    await expect(page.getByTestId("active-sessions")).toBeVisible();

    // Chrome's own User-Agent, unparsed, is what identifies the row.
    await expect(page.getByText("This device")).toBeVisible();

    expect(errors()).toEqual([]);
  });

  test("is reachable from Settings", async ({ page }) => {
    const errors = collectErrors(page, [EXPECTED_401]);

    await page.getByRole("link", { name: "Settings" }).click();
    await page.getByRole("link", { name: "Active Sessions" }).click();

    await expect(page.getByTestId("active-sessions")).toBeVisible();

    expect(errors()).toEqual([]);
  });

  test("shows another sign-in, and can end it", async ({ page }) => {
    const errors = collectErrors(page, [EXPECTED_401]);

    await signInElsewhereAsync("RegOS-Spec-Laptop/1.0");

    await page.goto("/settings/sessions");

    await expect(page.getByTestId("session-row")).toHaveCount(2);

    // The other device is recognisable by the string it sent, which is the
    // entire justification for storing it (ADR-029).
    await expect(page.getByText("RegOS-Spec-Laptop/1.0")).toBeVisible();

    await page
      .getByTestId("active-sessions")
      .getByRole("button", { name: "End session" })
      .click();

    // Back to one - and the list refetched rather than being spliced locally.
    await expect(page.getByTestId("session-row")).toHaveCount(1);
    await expect(page.getByText("RegOS-Spec-Laptop/1.0")).toHaveCount(0);

    // This browser is untouched.
    await expect(page).toHaveURL(/\/settings\/sessions$/);

    expect(errors()).toEqual([]);
  });

  test("signs out every other session at once, keeping this one", async ({
    page,
  }) => {
    const errors = collectErrors(page, [EXPECTED_401]);

    await signInElsewhereAsync("RegOS-Spec-A/1.0");
    await signInElsewhereAsync("RegOS-Spec-B/1.0");

    await page.goto("/settings/sessions");

    await expect(page.getByTestId("session-row")).toHaveCount(3);

    await page
      .getByTestId("active-sessions")
      .getByRole("button", { name: /Sign out 2 other sessions/ })
      .click();

    await expect(page.getByTestId("session-row")).toHaveCount(1);
    await expect(page.getByText("This device")).toBeVisible();

    // Still signed in: "others" really meant others.
    await expect(page).toHaveURL(/\/settings\/sessions$/);

    expect(errors()).toEqual([]);
  });

  test("refreshing the access token does not add a session", async ({
    page,
  }) => {
    // The property the aggregate exists for, and only a browser can show it
    // end to end: rotation mints a new refresh token, and the list must not
    // grow by one every fifteen minutes.
    const errors = collectErrors(page, [EXPECTED_401]);

    await page.goto("/settings/sessions");

    await expect(page.getByTestId("session-row")).toHaveCount(1);

    // Force the client through a real refresh, exactly as an expired access
    // token would.
    const refreshed = await page.request.post(`${API_URL}/api/auth/refresh`);

    expect(refreshed.status()).toBe(204);

    await page.reload();

    await expect(page.getByTestId("session-row")).toHaveCount(1);

    expect(errors()).toEqual([]);
  });

  test("ending this device signs the user out", async ({ page }) => {
    const errors = collectErrors(page, [EXPECTED_401]);

    await page.goto("/settings/sessions");

    // Scoped to the list: the application header carries its own "Sign Out",
    // and an unscoped selector matches both.
    await page
      .getByTestId("session-row")
      .getByRole("button", { name: "Sign out" })
      .click();

    await expect(page).toHaveURL(/\/login$/);

    expect(errors()).toEqual([]);
  });
});
