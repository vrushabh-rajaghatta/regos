import { expect } from "@playwright/test";

import { test, api, collectErrors } from "./support";

/**
 * **EPIC-022 S003 — the debt EPIC-018 shipped and could not close.**
 *
 * `LocalLabel.Language` has existed since that epic. Nothing could say which
 * languages a market *needs*, so nobody could be told their Canadian label set
 * was incomplete. That was `Country`'s omission, not Labeling's — and closing
 * it is why `LanguageCode` moved contexts
 * ([ADR-062](../../../docs/adr/ADR-062-a-language-is-a-world-fact.md)).
 *
 * **This spec exists to prove D4: advisory, never blocking.** The three steps
 * are deliberately in that order:
 *
 * 1. the country **declares** what its labelling is expected in — Canada en+fr;
 * 2. the market **records** what it actually has — one English label;
 * 3. the system **says French is missing and lets the work continue anyway**.
 *
 * Step 3 is the assertion that matters. The obligation genuinely varies:
 * Canada's bilingual requirement falls on the product monograph and on most
 * labels but *not* on prescription-only, hospital-only or professional-use
 * ones. A country knows neither the product nor the document, so a rule here
 * would be wrong for real cases — which is why this is advice.
 */
test.describe("Which languages this market's labelling is expected in", () => {
  test("Canada wants two, one is recorded, and nothing is blocked", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();

    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `LANG-${unique}`,
        name: `Bilingual Product ${unique}`,
        type: "Drug",
      }),
    });

    const { id: globalProductId } = await productResponse.json();

    await page.goto(`/regulatory/products/${globalProductId}/registrations`);

    await page.getByRole("button", { name: "Add market" }).click();
    await page.getByLabel("Country").selectOption({ label: "Canada" });
    await page.getByLabel("Present since").fill("2026-01-05");
    await page.getByRole("button", { name: "Add" }).click();

    await page
      .getByTestId("product-market-row")
      .filter({ hasText: "Canada" })
      .getByRole("link", { name: "Canada" })
      .click();

    await expect(page.getByTestId("market-overview")).toBeVisible();

    // --- 1. the country declares -------------------------------------------
    // Before any label exists, both expected languages are already missing —
    // so the advice is about the market, not about the labels that happen to
    // be there.
    await expect(page.getByTestId("languages-missing")).toContainText("en");
    await expect(page.getByTestId("languages-missing")).toContainText("fr");

    // --- 2. the market records ---------------------------------------------
    await page.getByTestId("add-local-label").click();
    await page.getByLabel("Document").click();
    await page
      .getByRole("option", { name: "Prescribing information", exact: true })
      .click();
    await page.getByLabel("Language").fill("en");
    await page.getByRole("button", { name: "Add local label" }).last().click();

    await expect(page.getByTestId("local-label-row")).toHaveCount(1);

    // --- 3. it says what is missing, and refuses nothing --------------------
    // The heart of D4. French is named — and nothing on the page has become a
    // gate: the label that was just created exists with its draft open, the
    // controls are live, and no refusal was raised anywhere.
    await expect(page.getByTestId("languages-missing")).toContainText("fr");

    const label = page.getByTestId("local-label-row").first();

    await expect(label.getByTestId("local-draft")).toBeVisible();
    await expect(page.getByTestId("add-local-label")).toBeEnabled();

    // The three ways this screen reports a refusal. None of them fired.
    await expect(page.getByTestId("add-local-label-error")).toHaveCount(0);
    await expect(page.getByTestId("start-revision-error")).toHaveCount(0);
    await expect(page.getByTestId("discard-local-error")).toHaveCount(0);

    // Advice, not a rule: it is rendered as muted prose rather than as a
    // destructive banner, because a red panel reads as something that stops you.
    await expect(page.getByTestId("label-language-coverage")).toHaveClass(
      /text-muted-foreground/,
    );

    // --- 4. and it closes when the second language arrives -----------------
    await page.getByTestId("add-local-label").click();
    await page.getByLabel("Document").click();
    await page
      .getByRole("option", { name: "Prescribing information", exact: true })
      .click();
    await page.getByLabel("Language").fill("fr");
    await page.getByRole("button", { name: "Add local label" }).last().click();

    await expect(page.getByTestId("local-label-row")).toHaveCount(2);

    await expect(page.getByTestId("languages-covered")).toBeVisible();
    await expect(page.getByTestId("languages-missing")).toHaveCount(0);

    expect(errors()).toEqual([]);
  });

  test("a market expecting one language says so without fuss", async ({
    page,
  }) => {
    // The ordinary case, asserted so the advisory panel is not only ever seen
    // in its unhappy state — and so a market with one language does not read
    // as a market with a problem.
    const errors = collectErrors(page);
    const unique = Date.now();

    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `MONO-${unique}`,
        name: `Monolingual Product ${unique}`,
        type: "Drug",
      }),
    });

    const { id: globalProductId } = await productResponse.json();

    await page.goto(`/regulatory/products/${globalProductId}/registrations`);

    await page.getByRole("button", { name: "Add market" }).click();
    await page.getByLabel("Country").selectOption({ label: "Germany" });
    await page.getByLabel("Present since").fill("2026-01-05");
    await page.getByRole("button", { name: "Add" }).click();

    await page
      .getByTestId("product-market-row")
      .filter({ hasText: "Germany" })
      .getByRole("link", { name: "Germany" })
      .click();

    await expect(page.getByTestId("market-overview")).toBeVisible();

    await page.getByTestId("add-local-label").click();
    await page.getByLabel("Document").click();
    await page
      .getByRole("option", { name: "Prescribing information", exact: true })
      .click();
    await page.getByLabel("Language").fill("de");
    await page.getByRole("button", { name: "Add local label" }).last().click();

    await expect(page.getByTestId("languages-covered")).toContainText("de");

    expect(errors()).toEqual([]);
  });
});
