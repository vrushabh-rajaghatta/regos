import { expect } from "@playwright/test";

import { test, collectErrors, EXPECTED_409 } from "./support";

/**
 * **EPIC-010a S001 — a substance is a shared fact, and a tenant may add its own.**
 *
 * The directory is unremarkable to look at; what this spec is for is the
 * boundary underneath it. `Substance` takes the second of ADR-038's three
 * filter shapes — `TenantId == null || TenantId == CurrentTenant` — and that
 * shape has **two** ways to fail, not one. It can leak another tenant's
 * compound, and it can just as easily *hide* the platform's, which looks like
 * an empty table rather than a bug.
 *
 * So the proof runs both directions in one screen: the shared catalogue is
 * visible, a compound this organisation adds sits beside it marked as theirs,
 * and the two are still tellable apart afterwards.
 *
 * The refusal at the end is the other half of ADR-058 §2. A tenant adding a
 * name the shared catalogue already carries would fork the answer to *"which
 * products contain substance X?"* — the question the whole epic exists to
 * answer — on its very first screen.
 */
test.describe("substance directory", () => {
  test("shows the shared catalogue, takes a proprietary compound, and refuses a name already in use", async ({
    page,
  }) => {
    const errors = collectErrors(page, [EXPECTED_409]);
    const compound = `RGX-${Date.now()}`;

    await page.goto("/regulatory/substances");

    // --- the shared catalogue is visible -----------------------------------
    const paracetamol = page
      .getByTestId("substance-row")
      .filter({ hasText: "Paracetamol" });

    await expect(paracetamol).toBeVisible();
    await expect(paracetamol.getByText("Shared")).toBeVisible();

    // Its terminology says whose word it is. RegOS holds no licensed
    // vocabulary, and the screen must not imply otherwise (ADR-058 §6).
    await expect(paracetamol.getByText("RegOS terminology")).toBeVisible();

    // --- search reaches the INN, not only the preferred name ---------------
    // Aspirin is the seeded row where the two genuinely differ, which is why
    // Name and Inn are two fields rather than one.
    await page.getByTestId("substance-search").fill("acetylsalicylic");

    await expect(
      page.getByTestId("substance-row").filter({ hasText: "Aspirin" }),
    ).toBeVisible();

    await expect(page.getByTestId("substance-row")).toHaveCount(1);

    await page.getByTestId("substance-search").fill("");

    // --- add a proprietary compound, with no INN ---------------------------
    // The case the tenant half of the aggregate exists to serve: an innovator
    // holds a molecule before anyone assigns it a nonproprietary name, and
    // that absence is the fact being recorded.
    await page.getByTestId("add-substance").click();

    await page.getByLabel("Name").fill(compound);
    await page.getByLabel("Molecular formula").fill("C21H28N6O3");

    await page.getByRole("button", { name: "Add substance" }).last().click();

    const ours = page.getByTestId("substance-row").filter({ hasText: compound });

    await expect(ours).toBeVisible();
    await expect(ours.getByText("Ours")).toBeVisible();

    // --- the two halves stay tellable apart --------------------------------
    await page.getByTestId("substance-origin-shared").click();

    await expect(paracetamol).toBeVisible();
    await expect(ours).toHaveCount(0);

    await page.getByTestId("substance-origin-proprietary").click();

    await expect(ours).toBeVisible();
    await expect(paracetamol).toHaveCount(0);

    await page.getByTestId("substance-origin-any").click();

    await expect(paracetamol).toBeVisible();
    await expect(ours).toBeVisible();

    // --- a shared name cannot be forked ------------------------------------
    await page.getByTestId("add-substance").click();

    await page.getByLabel("Name").fill("Paracetamol");

    await page.getByRole("button", { name: "Add substance" }).last().click();

    // The refusal names *which* catalogue the clash is in, because that is
    // what tells "use the one that is there" apart from "you added this
    // already".
    await expect(page.getByTestId("add-substance-error")).toContainText(
      "already in the shared catalogue",
    );

    expect(errors()).toEqual([]);
  });
});
