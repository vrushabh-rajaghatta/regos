import type { Page } from "@playwright/test";

export const API_URL = process.env.REGOS_API_URL ?? "http://localhost:5225";

/**
 * The tenant the dev UI is configured with (web/regos-web/.env.development).
 * Kept in one place so a spec never hard-codes it inline.
 */
export const TENANT = "30000000-0000-0000-0000-000000000003";

export const api = (path: string, init: RequestInit = {}) =>
  fetch(`${API_URL}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      "X-Tenant-Id": TENANT,
      ...(init.headers ?? {}),
    },
  });

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

