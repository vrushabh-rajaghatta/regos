import { test as base, type Page } from "@playwright/test";

export const API_URL = process.env.REGOS_API_URL ?? "http://localhost:5225";

/**
 * The development account seeded by DevelopmentCredentialSeeder. Specs sign in
 * as this user; there is no invitation acceptance flow yet to create another.
 */
export const DEV_EMAIL = "dev@regos.local";
export const DEV_PASSWORD = "development-password";

/**
 * The organization the development account belongs to, and therefore the
 * tenant every spec acts as. It is no longer a value the caller chooses: it
 * arrives inside the token, and this constant exists only so assertions can
 * name it (ADR-024).
 */
export const TENANT = "30000000-0000-0000-0000-000000000003";

let cachedCookies: Promise<string> | undefined;

/**
 * Signs in once per run and keeps the cookies the API set.
 *
 * The session now lives in HttpOnly cookies (ADR-025), so there is no token to
 * capture. Node's fetch has no cookie jar, so the Set-Cookie headers are parsed
 * once and replayed on every subsequent call.
 */
export function sessionCookies(): Promise<string> {
  cachedCookies ??= fetch(`${API_URL}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email: DEV_EMAIL, password: DEV_PASSWORD }),
  }).then((response) => {
    if (!response.ok) {
      throw new Error(
        `Unable to sign in as ${DEV_EMAIL} (${response.status}). Is the API ` +
          `running in Development, so the account is seeded?`,
      );
    }

    return response.headers
      .getSetCookie()
      .map((cookie) => cookie.split(";")[0])
      .join("; ");
  });

  return cachedCookies;
}

export const api = async (path: string, init: RequestInit = {}) =>
  fetch(`${API_URL}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      Cookie: await sessionCookies(),
      ...(init.headers ?? {}),
    },
  });

/**
 * `test`, with the browser already signed in.
 *
 * Specs import this instead of Playwright's `test`, so each one starts with a
 * real session already established — the login form is covered by its own spec
 * rather than replayed at the top of every other one.
 */
export const test = base.extend<{ signedIn: void }>({
  signedIn: [
    async ({ context }, use) => {
      // Sign in through the browser's own request context, so the API's
      // Set-Cookie lands in the cookie jar every page in this context shares.
      // Nothing is injected: these are the real session cookies, HttpOnly and
      // unreadable by page scripts, exactly as a user would have them.
      const response = await context.request.post(
        `${API_URL}/api/auth/login`,
        { data: { email: DEV_EMAIL, password: DEV_PASSWORD } },
      );

      if (!response.ok()) {
        throw new Error(
          `Unable to sign in as ${DEV_EMAIL} (${response.status()}). Is the ` +
            `API running in Development, so the account is seeded?`,
        );
      }

      await use();
    },
    { auto: true },
  ],
});

/**
 * `test` with no session, for specs that exercise signing in.
 *
 * A separate export rather than an opt-out flag: the fixture above signs in
 * before the test body runs, and there is no way to un-sign-in a context
 * without racing the navigation that is under test.
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
