import { expect } from "@playwright/test";

import {
  DEV_EMAIL,
  EXPECTED_401,
  anonymousTest as test,
  collectErrors,
} from "./support";

/**
 * The password reset flow, from the link on the sign-in page to a dead token.
 *
 * The one path missing is redeeming a real link. The token exists only in the
 * plaintext the Development notifier writes to the API's own stdout, and a
 * browser spec has no channel to it — so the happy path is covered by
 * PasswordResetLifecycleTests, which can both obtain a token and clean up the
 * account it resets.
 *
 * What is left is what only a browser can check, and one of those checks is the
 * most important assertion in this file: that requesting a reset for a real
 * account and for an address nobody has produces exactly the same screen. The
 * API is careful to answer 204 either way; a UI that said "we've sent you an
 * email" only when the account existed would hand the enumeration oracle
 * straight back (ADR-022).
 */
test.describe("Password reset", () => {
  test("is reachable from the sign-in page", async ({ page }) => {
    const errors = collectErrors(page, [EXPECTED_401]);

    await page.goto("/login");

    await page.getByRole("link", { name: "Forgot password?" }).click();

    await expect(
      page.getByRole("heading", { name: "Reset your password" }),
    ).toBeVisible();

    expect(errors()).toEqual([]);
  });

  test("says the same thing whether or not the address has an account", async ({
    page,
  }) => {
    const errors = collectErrors(page, [EXPECTED_401]);

    async function requestFor(email: string) {
      await page.goto("/forgot-password");

      await page.getByLabel("Email Address").fill(email);
      await page.getByRole("button", { name: "Send Reset Link" }).click();

      const confirmation = page.getByTestId("password-reset-requested");

      await expect(confirmation).toBeVisible();

      return (await confirmation.innerText()).trim();
    }

    // The seeded development account, which certainly exists...
    //
    // This is the one place these specs knowingly bend rule 1: it leaves a
    // PasswordResets row per run. The row is harmless - the link is never
    // redeemed, it expires in an hour, and the next run supersedes it, so at
    // most one is ever live - and there is no way to withdraw it over HTTP.
    // Using a second unknown address instead would delete the leak and the
    // point of the test with it: parity only means something if one side is
    // real. Recorded with the other token-cleanup debt in the roadmap.
    const known = await requestFor(DEV_EMAIL);

    // ...and an address that certainly does not.
    const unknown = await requestFor(
      `nobody.${Date.now()}@nowhere.invalid`,
    );

    expect(unknown).toBe(known);

    // The form is gone in both cases too - a screen that kept offering "Send
    // Reset Link" for one address and not the other would leak by shape rather
    // than by words.
    await expect(
      page.getByRole("button", { name: "Send Reset Link" }),
    ).toHaveCount(0);

    expect(errors()).toEqual([]);
  });

  test("will not submit an address that is not an address", async ({ page }) => {
    const errors = collectErrors(page, [EXPECTED_401]);

    await page.goto("/forgot-password");

    // "a@b" on purpose. Something like "not-an-address" never reaches the
    // schema at all - the input is type="email", so Chrome refuses to submit
    // and shows its own tooltip, and a test asserting our message there would
    // be asserting a layer that never ran. This address satisfies the browser
    // and fails our rules, which is the only way to see the schema work.
    await page.getByLabel("Email Address").fill("a@b");
    await page.getByRole("button", { name: "Send Reset Link" }).click();

    await expect(page.getByText("Enter a valid email address.")).toBeVisible();

    // Caught in the browser, so nothing was sent - and the confirmation that
    // would normally follow has not appeared.
    await expect(
      page.getByTestId("password-reset-requested"),
    ).toHaveCount(0);

    expect(errors()).toEqual([]);
  });

  test("rejects a link whose token is not valid", async ({ page }) => {
    const errors = collectErrors(page, [EXPECTED_401]);

    await page.goto("/reset-password?token=not-a-real-token");

    await page.getByLabel("New Password", { exact: true }).fill("a good password");
    await page.getByLabel("Confirm New Password").fill("a good password");
    await page.getByRole("button", { name: "Reset Password" }).click();

    await expect(page.getByRole("alert")).toContainText("no longer valid");

    // And it offers the way out, which is the whole reason the message names
    // the likely causes rather than staying silent.
    await expect(
      page.getByRole("link", { name: "Request a new link" }),
    ).toBeVisible();

    expect(errors()).toEqual([]);
  });

  test("refuses a link with no token at all", async ({ page }) => {
    const errors = collectErrors(page, [EXPECTED_401]);

    await page.goto("/reset-password");

    await expect(page.getByRole("alert")).toContainText("incomplete");

    // No form: filling one in could only ever be refused.
    await expect(
      page.getByRole("button", { name: "Reset Password" }),
    ).toHaveCount(0);

    expect(errors()).toEqual([]);
  });

  test("will not reset to mismatched passwords", async ({ page }) => {
    const errors = collectErrors(page, [EXPECTED_401]);

    await page.goto("/reset-password?token=not-a-real-token");

    await page.getByLabel("New Password", { exact: true }).fill("a good password");
    await page.getByLabel("Confirm New Password").fill("a different password");
    await page.getByRole("button", { name: "Reset Password" }).click();

    await expect(page.getByText("Passwords do not match.")).toBeVisible();

    expect(errors()).toEqual([]);
  });
});
