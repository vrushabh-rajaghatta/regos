import { expect } from "@playwright/test";

import { test, api, collectErrors } from "./support";

/**
 * EPIC-020's capstone: the workflow the epic was planned around, steps 1 to 6,
 * driven through the browser against real Postgres.
 *
 *   1  the published playbook exists and is frozen
 *   2  an objective is stated
 *   3  a plan is instantiated from a pinned version, dates derived once
 *   4  the team works it
 *   5  real regulatory work attaches to a step
 *   6  the plan says what a delay costs
 *
 * **It introduces nothing.** Every control it touches shipped in S001–S007, and
 * a step that needed a new affordance would mean the capstone had quietly become
 * a ninth story. Its job is to show the pieces compose.
 *
 * **Step 1 is read, not authored.** Playbooks are seeded and published through
 * the API — there is no authoring UI and S001 did not claim one. The steward's
 * browser view of a frozen playbook is what exists, so that is what is proved.
 *
 * ---
 *
 * **It was committed red, and that was the point.**
 *
 * S008 wrote this spec and it failed at step 1: every client call the Process
 * feature made omitted the `/api` prefix its endpoint required, so all fifteen
 * returned 404 and the capability had never been reachable from a browser. A
 * second defect sat behind it — seven writes passed `JSON.stringify` with no
 * headers, so `fetch` sent `text/plain` and the API answered **415**.
 *
 * The capstone recorded both and repaired neither: an audit that quietly fixes
 * what it finds leaves a record reading *"browser proof ✓"*. **S009 is the
 * repair**, and this spec turning green was its acceptance test.
 *
 * Neither defect was visible to `npm run build` or `npm run lint`. A route is a
 * string and a missing header is an absence. `ApiRouteAlignmentTests` now
 * compares both halves mechanically, so the class is closed rather than the
 * instance.
 */
const FDA = "20000000-0000-0000-0000-000000000001";
const MEETING_REQUEST = "90000000-0000-0000-0000-000000000005";

/**
 * The anchor every assertion below is relative to — **sixty days in the past**.
 *
 * Not a fixed date, and the reason is the domain rather than convenience. A plan
 * is opened on its anchor, and its execution history is append-only with a
 * chronology rule (I6), so a plan anchored in the *future* refuses every
 * `occurredOn: today` the UI sends. Backdating the anchor is also what gives
 * step 6 something real to report. **A hundred and twenty days**, not sixty:
 * at sixty the plan is only *projected* late, and `lateSteps` — which lists
 * steps whose planned end has actually passed — is still empty. The distinction
 * is D7's, and this is the browser meeting it.
 */
const ANCHOR = daysAgo(120);

function daysAgo(days: number): string {
  const date = new Date();
  date.setDate(date.getDate() - days);
  return date.toISOString().slice(0, 10);
}

test.describe("Regulatory process — the whole workflow", () => {
  test("playbook to plan to attached work to what a delay costs", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();

    const productName = `Capstone Product ${unique}`;
    await createProduct(unique, productName);

    // --- 1. the playbook is published and frozen -------------------------
    await page.goto("/regulatory/playbooks");

    const playbook = page
      .getByTestId("playbook-row")
      .filter({ hasText: "US FDA IND" });

    await expect(playbook).toHaveCount(1);
    // The row IS the link — the testid sits on the <Link>, not inside it.
    await playbook.click();

    // Two different statuses, and the distinction is I4's: the PLAYBOOK is
    // Active — it may still gain versions — while the VERSION a plan pins is
    // Published and can never change again.
    await expect(page.getByTestId("playbook-status-badge")).toContainText(
      "Active",
    );

    await expect(
      page.getByTestId("playbook-version-button").first(),
    ).toContainText("Published");

    // Twelve steps, the number S001 seeded and every scheduling test measured.
    await expect(page.getByTestId("playbook-step-row")).toHaveCount(12);

    // --- 2. the objective ------------------------------------------------
    await page.goto("/regulatory/objectives");
    await page.getByRole("button", { name: "State objective" }).click();

    await page.getByLabel("Product").click();
    await page.getByRole("option", { name: productName, exact: true }).click();

    await page.getByLabel("Market").click();
    await page
      .getByRole("option", { name: "United States", exact: true })
      .click();

    const objectiveName = `Open an IND ${unique}`;
    await page.getByLabel("Objective", { exact: true }).fill(objectiveName);
    await page
      .getByLabel("Rationale")
      .fill("505(b)(1), pre-IND meeting first.");
    await page.getByLabel("Target completion").fill("2027-03-31");

    await page
      .getByRole("button", { name: "State objective", exact: true })
      .last()
      .click();

    await page
      .getByTestId("objective-row")
      .filter({ hasText: objectiveName })
      .click();

    // --- 3. the plan, from a pinned version ------------------------------
    await page.getByRole("button", { name: "Create plan" }).click();

    await page.getByLabel("Playbook").click();
    await page.getByRole("option", { name: /US FDA IND/ }).first().click();

    await page.getByLabel("Version").click();
    await page.getByRole("option").first().click();

    await page.getByLabel("Start from").fill(ANCHOR);
    await page.getByLabel("Plan name").fill(`US IND filing plan ${unique}`);

    await page.getByRole("button", { name: "Create plan", exact: true })
      .last()
      .click();

    await page
      .getByTestId("objective-plan-row")
      .filter({ hasText: `US IND filing plan ${unique}` })
      .click();

    await expect(page.getByTestId("plan-schedule")).toBeVisible();

    const steps = page.getByTestId("plan-step-row");
    await expect(steps).toHaveCount(12);

    // Derived once, from the offsets: the step with no predecessors starts on
    // the anchor itself. Nothing recalculates after this point.
    await expect(steps.first()).toContainText(ANCHOR);

    const planUrl = page.url();

    // --- 4. the team works it --------------------------------------------
    await page.getByTestId("plan-status-action").first().click();

    // **Filtered by name, not by code** — a row renders its own code AND the
    // codes of everything it waits for, so `hasText: "PRE-IND-REQ"` also matches
    // PRE-IND-PKG, which waits for it. Names appear in one column only.
    const request = steps.filter({ hasText: "Submit pre-IND meeting request" });

    await request.getByTestId("step-start").click();
    await expect(request).toContainText("InProgress");

    await request.getByTestId("step-complete").click();
    await expect(request).toContainText("Complete");

    // --- 5. real work attaches to a step ---------------------------------
    // The letter is created through the API — a spec owns the data it mutates
    // (ADR-019) — and linked through the UI, because the link is what step 5 is.
    const correspondenceId = await recordCorrespondence(unique);

    await page.goto(`/regulatory/correspondence/${correspondenceId}`);

    const planStep = page.getByTestId("correspondence-plan-step");
    await expect(planStep).toContainText("Not linked to a plan");

    await planStep.getByRole("combobox").first().click();
    await page.getByRole("option", { name: objectiveName, exact: true }).click();

    await planStep.getByRole("combobox").nth(1).click();
    await page
      .getByRole("option", { name: `US IND filing plan ${unique}` })
      .click();

    await planStep.getByRole("combobox").nth(2).click();
    await page.getByRole("option", { name: /^PRE-IND-MTG/ }).click();

    await expect(planStep).toContainText("serving a step of a plan");

    // And the plan sees it from the other end — the reverse read, composed.
    await page.goto(planUrl);

    const meetingStep = steps.filter({ hasText: "Pre-IND meeting with FDA" });

    await expect(meetingStep.getByTestId("step-attachment")).toContainText(
      "Correspondence",
    );

    // The link changed discoverability and nothing else (I9): the letter did
    // not complete the step it serves.
    await expect(meetingStep).toContainText("NotStarted");

    // --- 6. what a delay costs -------------------------------------------
    const impact = page.getByTestId("plan-impact");
    await expect(impact).toBeVisible();

    // The anchor is 120 days back and almost nothing was worked, so the
    // projection has something real to say — and says both halves of D7.
    await expect(impact.getByTestId("projected-finish")).not.toContainText("—");

    // "What has slipped" — the finish moves out, and by a stated amount.
    await expect(impact.getByTestId("slip-days")).toContainText(/^\+\d+ days$/);

    // "What is actually overdue" — a different question, answered separately.
    await expect(impact.getByTestId("late-steps")).toBeVisible();

    // Asking did not change the answer to anything (I8). The plan's own dates
    // are still the ones it was scheduled with.
    await expect(steps.first()).toContainText(ANCHOR);

    expect(errors()).toEqual([]);
  });
});

async function createProduct(unique: number, name: string): Promise<string> {
  const response = await api("/api/products", {
    method: "POST",
    body: JSON.stringify({ code: `CAP-${unique}`, name, type: "Drug" }),
  });

  if (!response.ok) {
    throw new Error(`Unable to create a product (${response.status}).`);
  }

  return (await response.json()).id;
}

async function recordCorrespondence(unique: number): Promise<string> {
  const response = await api("/api/correspondence", {
    method: "POST",
    body: JSON.stringify({
      authorityId: FDA,
      correspondenceTypeId: MEETING_REQUEST,
      direction: "Outbound",
      subject: `Type B meeting request ${unique}`,
      occurredOn: ANCHOR,
    }),
  });

  if (!response.ok) {
    throw new Error(
      `Unable to record correspondence (${response.status}): ` +
        `${await response.text()}`,
    );
  }

  return (await response.json()).id;
}
