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
 * **THIS SPEC IS RED, AND IT IS COMMITTED RED ON PURPOSE.**
 *
 * It fails at step 1 with `playbook-row` resolving to zero elements, because
 * every client call the Process feature makes omits the `/api` prefix its
 * endpoint requires and therefore 404s. Fifteen calls, S001 through S007. The
 * backend is correct and its 40 database-backed tests pass; the browser has
 * never been able to reach it.
 *
 * **S008 found this and deliberately did not fix it.** A capstone audits; it
 * does not quietly repair what it is auditing, or the record would read
 * *"browser proof ✓"* when what happened was *"browser proof → systemic
 * defect"*. The repair is EPIC-020 **S009**, along with the guard that matters
 * more than the fifteen edits: nothing mechanical compares a client path to the
 * routes the host actually maps, which is why seven stories reached Done with
 * this unnoticed.
 *
 * **This spec turning green is S009's acceptance test.**
 */
const FDA = "20000000-0000-0000-0000-000000000001";
const MEETING_REQUEST = "90000000-0000-0000-0000-000000000005";

/** The anchor every assertion below is relative to. */
const ANCHOR = "2026-09-01";

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
    await playbook.getByRole("link").first().click();

    await expect(page.getByTestId("playbook-status-badge")).toContainText(
      "Published",
    );

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
      .getByRole("link")
      .first()
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
      .getByRole("link")
      .first()
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

    const request = steps.filter({ hasText: "PRE-IND-REQ" });

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

    await expect(
      steps.filter({ hasText: "PRE-IND-MTG" }).getByTestId("step-attachment"),
    ).toContainText("Correspondence");

    // The link changed discoverability and nothing else (I9): the letter did
    // not complete the step it serves.
    await expect(steps.filter({ hasText: "PRE-IND-MTG" })).toContainText(
      "NotStarted",
    );

    // --- 6. what a delay costs -------------------------------------------
    const impact = page.getByTestId("plan-impact");
    await expect(impact).toBeVisible();

    // The anchor is in the past by the time this runs, and most of the plan was
    // never started — so the projection has something real to report.
    await expect(impact.getByTestId("projected-finish")).not.toContainText("—");
    await expect(impact.getByTestId("slip-days")).toBeVisible();
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

  return (await response.json()).correspondenceId;
}
