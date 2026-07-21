import { expect } from "@playwright/test";

import { EXPECTED_401, anonymousTest as test, collectErrors } from "./support";

/**
 * The acceptance page, exercised only along paths that create nothing.
 *
 * A successful acceptance needs an invited user, and a user cannot be deleted —
 * so a happy-path spec would leak a row per run (ADR-019 rule 1). That case is
 * covered by InvitationLifecycleTests, which can clean up after itself. What is
 * left here is what only a browser can check: that the page renders, that a
 * dead link says so, and that a malformed one does not offer a form that could
 * never succeed.
 */
test.describe("Accept invitation", () => {
  test("rejects a link whose token is not valid", async ({ page }) => {
    const errors = collectErrors(page, [EXPECTED_401]);

    await page.goto("/accept-invitation?token=not-a-real-token");

    await page.getByLabel("Password", { exact: true }).fill("a good password");
    await page.getByLabel("Confirm Password").fill("a good password");
    await page.getByRole("button", { name: "Set Password" }).click();

    await expect(page.getByRole("alert")).toContainText(
      "no longer valid",
    );

    expect(errors()).toEqual([]);
  });

  test("refuses a link with no token at all", async ({ page }) => {
    const errors = collectErrors(page);

    await page.goto("/accept-invitation");

    await expect(page.getByRole("alert")).toContainText("incomplete");

    // No form: filling one in could only ever be refused.
    await expect(
      page.getByRole("button", { name: "Set Password" }),
    ).toHaveCount(0);

    expect(errors()).toEqual([]);
  });

  test("will not accept mismatched passwords", async ({ page }) => {
    const errors = collectErrors(page);

    await page.goto("/accept-invitation?token=not-a-real-token");

    await page.getByLabel("Password", { exact: true }).fill("a good password");
    await page.getByLabel("Confirm Password").fill("a different password");
    await page.getByRole("button", { name: "Set Password" }).click();

    await expect(page.getByText("Passwords do not match.")).toBeVisible();

    expect(errors()).toEqual([]);
  });
});
