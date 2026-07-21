import { expect, type Page } from "@playwright/test";

import {
  API_URL,
  DEV_EMAIL,
  DEV_PASSWORD,
  EXPECTED_400,
  EXPECTED_401,
  anonymousTest as test,
  collectErrors,
} from "./support";

/**
 * Changing your own password, in a real browser.
 *
 * This is the first spec that mutates the development account, and it does so
 * because it has no alternative: changing a password requires signing in as the
 * account being changed, and `dev@regos.local` is the only account whose
 * password this suite knows. Inviting a fresh user does not help — acceptance
 * needs a token that exists only in the API's log output.
 *
 * So it borrows the account and puts it back. `afterEach` restores the seeded
 * password whatever the test did, including when it failed halfway through, and
 * fails loudly with instructions if it cannot. `DevelopmentCredentialSeeder`
 * deliberately does not reset an existing password, so nothing else would.
 *
 * ADR-019 rule 1 asks that a full run leave the seeded data unchanged. That is
 * satisfied — but by restoration rather than by abstinence, which is a weaker
 * guarantee and is why this is the only spec allowed to do it.
 */
const TEMPORARY = "a temporary spec password";

async function signInAsync(page: Page, password: string) {
  await page.goto("/login");

  await page.getByLabel("Email Address").fill(DEV_EMAIL);
  await page.getByLabel("Password", { exact: true }).fill(password);
  await page.getByRole("button", { name: "Sign In" }).click();
}

/**
 * Signs in and waits to be somewhere else. Without the wait, the next
 * navigation races the login request and lands on a page the session has not
 * reached yet — which reads as "the form never rendered".
 */
async function signInAndLandAsync(page: Page, password: string) {
  await signInAsync(page, password);

  await expect(page).not.toHaveURL(/\/login$/);
}

/** Signs in over HTTP and returns the cookies, or null if the password is wrong. */
async function cookiesForAsync(password: string): Promise<string | null> {
  const response = await fetch(`${API_URL}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email: DEV_EMAIL, password }),
  });

  if (!response.ok) return null;

  return response.headers
    .getSetCookie()
    .map((cookie) => cookie.split(";")[0])
    .join("; ");
}

async function changeOverHttpAsync(
  cookies: string,
  currentPassword: string,
  newPassword: string,
) {
  return fetch(`${API_URL}/api/auth/change-password`, {
    method: "POST",
    headers: { "Content-Type": "application/json", Cookie: cookies },
    body: JSON.stringify({ currentPassword, newPassword }),
  });
}

test.describe("Change password", () => {
  test.afterEach(async () => {
    // Already correct: nothing to do.
    if (await cookiesForAsync(DEV_PASSWORD)) return;

    const cookies = await cookiesForAsync(TEMPORARY);

    if (!cookies) {
      throw new Error(
        `The development account's password is neither the seeded one nor ` +
          `this spec's temporary one. Restore it by deleting the row from ` +
          `"UserCredentials" for ${DEV_EMAIL} and restarting the API, which ` +
          `will re-seed it.`,
      );
    }

    const restored = await changeOverHttpAsync(
      cookies, TEMPORARY, DEV_PASSWORD);

    if (!restored.ok) {
      throw new Error(
        `Could not restore the development password (${restored.status}).`,
      );
    }
  });

  test("changes the password, signs the user out, and takes effect", async ({
    page,
  }) => {
    // A signed-out visitor's first /api/auth/me is a 401 by design (ADR-025).
    const errors = collectErrors(page, [EXPECTED_401]);

    await signInAndLandAsync(page, DEV_PASSWORD);

    await page.goto("/settings/security");

    await expect(
      page.getByRole("heading", { name: "Security" }),
    ).toBeVisible();

    await page.getByLabel("Current Password").fill(DEV_PASSWORD);
    await page.getByLabel("New Password", { exact: true }).fill(TEMPORARY);
    await page.getByLabel("Confirm New Password").fill(TEMPORARY);
    await page.getByRole("button", { name: "Change Password" }).click();

    // Signed out everywhere, including here (ADR-028).
    await expect(page).toHaveURL(/\/login$/);

    // The old password is genuinely gone...
    await signInAsync(page, DEV_PASSWORD);

    await expect(page.getByRole("alert")).toContainText(
      "Invalid email address or password.",
    );

    // ...and the new one works.
    await signInAsync(page, TEMPORARY);

    await expect(page).not.toHaveURL(/\/login$/);

    expect(errors()).toEqual([]);
  });

  test("is reachable from the navigation", async ({ page }) => {
    const errors = collectErrors(page, [EXPECTED_401]);

    await signInAndLandAsync(page, DEV_PASSWORD);

    await page.getByRole("link", { name: "Settings" }).click();
    await page.getByRole("link", { name: "Security" }).click();

    await expect(page.getByTestId("change-password-form")).toBeVisible();

    expect(errors()).toEqual([]);
  });

  test("reports a wrong current password without signing the user out", async ({
    page,
  }) => {
    // The test that changed the API. It originally answered a wrong current
    // password with 401, and apiFetch treats 401 as "the access token expired"
    // - refresh, replay, and report the second 401 as a dead session. So a typo
    // signed the user out of the application. The status is now 400, which is
    // what it should always have been: the caller is authenticated and
    // permitted, and what is wrong is a field (ADR-028).
    //
    // The 400 is deliberate, so its console message is declared - narrowly, and
    // only in this test.
    const errors = collectErrors(page, [EXPECTED_401, EXPECTED_400]);

    await signInAndLandAsync(page, DEV_PASSWORD);

    await page.goto("/settings/security");

    await page.getByLabel("Current Password").fill("not the password");
    await page.getByLabel("New Password", { exact: true }).fill(TEMPORARY);
    await page.getByLabel("Confirm New Password").fill(TEMPORARY);
    await page.getByRole("button", { name: "Change Password" }).click();

    await expect(page.getByRole("alert")).toContainText(
      "current password is incorrect",
    );

    // Still here, still signed in.
    await expect(page).toHaveURL(/\/settings\/security$/);
    await expect(page.getByTestId("change-password-form")).toBeVisible();

    expect(errors()).toEqual([]);
  });

  test("will not change to mismatched passwords", async ({ page }) => {
    const errors = collectErrors(page, [EXPECTED_401]);

    await signInAndLandAsync(page, DEV_PASSWORD);

    await page.goto("/settings/security");

    await page.getByLabel("Current Password").fill(DEV_PASSWORD);
    await page.getByLabel("New Password", { exact: true }).fill(TEMPORARY);
    await page
      .getByLabel("Confirm New Password")
      .fill("something else entirely");
    await page.getByRole("button", { name: "Change Password" }).click();

    await expect(page.getByText("Passwords do not match.")).toBeVisible();

    expect(errors()).toEqual([]);
  });
});
