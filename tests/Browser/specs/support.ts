import { test as base, type Page } from "@playwright/test";

export const API_URL = process.env.REGOS_API_URL ?? "http://localhost:5225";

/**
 * The development account seeded by DevelopmentCredentialSeeder. Specs sign in
 * as this user; there is no invitation acceptance flow yet to create another.
 */
export const DEV_EMAIL = "dev@regos.local";
export const DEV_PASSWORD = "development-password";

/** Must match web/regos-web/src/shared/auth/accessToken.ts. */
const STORAGE_KEY = "regos.accessToken";

/**
 * The organization the development account belongs to, and therefore the
 * tenant every spec now acts as. It is no longer a value the caller chooses:
 * it arrives inside the token, and this constant exists only so assertions can
 * name it (ADR-024).
 */
export const TENANT = "30000000-0000-0000-0000-000000000003";

let cachedToken: Promise<string> | undefined;

/** Signs in once per run; every spec and every page share the token. */
export function accessToken(): Promise<string> {
  cachedToken ??= fetch(`${API_URL}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email: DEV_EMAIL, password: DEV_PASSWORD }),
  }).then(async (response) => {
    if (!response.ok) {
      throw new Error(
        `Unable to sign in as ${DEV_EMAIL} (${response.status}). Is the API ` +
          `running in Development, so the account is seeded?`,
      );
    }

    const { accessToken } = await response.json();

    return accessToken as string;
  });

  return cachedToken;
}

export const api = async (path: string, init: RequestInit = {}) =>
  fetch(`${API_URL}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${await accessToken()}`,
      ...(init.headers ?? {}),
    },
  });

/**
 * `test`, with the browser already signed in.
 *
 * Specs import this instead of Playwright's `test`. The token is injected
 * before any page script runs, which leaves the app in the same state a real
 * sign-in would — the login form itself is covered by its own spec rather than
 * replayed at the top of every other one.
 */
export const test = base.extend<{ signedIn: void }>({
  signedIn: [
    async ({ page }, use) => {
      const token = await accessToken();

      await page.addInitScript(
        ([key, value]) => window.localStorage.setItem(key, value),
        [STORAGE_KEY, token],
      );

      await use();
    },
    { auto: true },
  ],
});

/**
 * `test` with no token injected, for specs that exercise signing in.
 *
 * A separate export rather than an opt-out flag, because the obvious
 * alternative — an init script that removes the token — silently re-runs on
 * every navigation and signs the user back out mid-test.
 */
export const anonymousTest = base;

/**
 * Collects console and page errors, ignoring only the resource-load messages a
 * spec has declared it expects. Narrow by design: a genuine React or runtime
 * error must still fail the test.
 */
export function collectErrors(page: Page, expected: RegExp[] = []) {
  const errors: string[] = [];

  page.on("console", (message) => {
    if (message.type() === "error") errors.push(message.text());
  });
  page.on("pageerror", (error) => errors.push(`pageerror: ${error.message}`));

  return () => errors.filter((e) => !expected.some((r) => r.test(e)));
}

export const EXPECTED_404 =
  /Failed to load resource: the server responded with a status of 404/;

export const EXPECTED_401 =
  /Failed to load resource: the server responded with a status of 401/;
