import { defineConfig, devices } from "@playwright/test";

/**
 * Browser verification for RegOS.
 *
 * Uses the locally installed Google Chrome (`channel: "chrome"`) rather than a
 * Playwright-managed browser, so `npm install` here downloads no browsers.
 *
 * These are verification tests, not a CI suite. They run against a RUNNING
 * stack — see README.md — because their value is proving the real UI talks to
 * the real API against real Postgres. Nothing is mocked.
 */
export default defineConfig({
  testDir: "./specs",
  fullyParallel: false,
  workers: 1,
  reporter: [["list"]],
  use: {
    baseURL: process.env.REGOS_WEB_URL ?? "http://localhost:5173",
    trace: "retain-on-failure",
    screenshot: "only-on-failure",
  },
  projects: [
    {
      name: "chrome",
      use: { ...devices["Desktop Chrome"], channel: "chrome" },
    },
  ],
});
