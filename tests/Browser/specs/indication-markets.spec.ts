import { expect } from "@playwright/test";

import { test, api, API_URL, sessionCookies, collectErrors } from "./support";

const PDF = new TextEncoder().encode("%PDF-1.7\n% RegOS capstone test\n");

/**
 * **EPIC-018 S006 — the capstone, and the epic's Definition of Done in one
 * journey.**
 *
 * The DoD asked for exactly this walk: *create a global label → derive a local
 * label for one market → add an indication with a paediatric population → see it
 * on the product's market view.* It is done here in one pass rather than in five
 * specs that each prove a step, because the thing under test is that the steps
 * join up.
 *
 * **Then the question the epic was built to answer:**
 *
 * > *Which markets is this product approved for type 2 diabetes in?*
 *
 * It works because the condition is **coded**. Japan's indication and France's
 * are separate aggregates with separate wording, separate qualifiers and
 * separate decision histories — `T2DM` is the only thing they share, and it is
 * the join key S003 wrote down before this read existed.
 *
 * **The assertion that makes this a test rather than a demonstration** is
 * France: the same code, recorded and then withdrawn. A query that could only
 * say *"recorded here"* would report two approvals. Separating the two is what
 * S003's status history is for, and this is the first read that depends on it.
 */
test.describe("Which markets is this product approved for this indication in", () => {
  test("the whole journey, and the question it was built to answer", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();

    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `CAP-${unique}`,
        name: `Capstone Product ${unique}`,
        type: "Drug",
      }),
    });

    expect(productResponse.ok).toBeTruthy();
    const { id: globalProductId } = await productResponse.json();

    await uploadDocument(globalProductId, `CCDS ${unique}`);
    await uploadDocument(globalProductId, `JP PI ${unique}`);

    // --- 1. the global label, published --------------------------------------
    await page.goto(`/regulatory/products/${globalProductId}/labels`);

    await page.getByTestId("add-global-label").click();
    await page
      .getByLabel("Name", { exact: true })
      .fill(`Core data sheet ${unique}`);
    await page.getByLabel("Label type").click();
    await page
      .getByRole("option", { name: "Company Core Data Sheet", exact: true })
      .click();
    await page.getByRole("button", { name: "Add label" }).last().click();

    const label = page
      .getByTestId("global-label-row")
      .filter({ hasText: `Core data sheet ${unique}` });

    await label.getByTestId("show-label-versions").click();
    await label.getByTestId("label-content").click();
    await page.getByRole("option", { name: `CCDS ${unique}` }).click();

    await label.getByTestId("publish-version").click();
    await page.getByLabel("Takes effect").fill("2026-01-01");
    await page.getByRole("button", { name: "Publish version" }).last().click();

    await expect(label.getByTestId("label-in-force")).toContainText("Version 1");

    // --- 2. two markets ------------------------------------------------------
    await page.goto(`/regulatory/products/${globalProductId}/registrations`);

    await addMarket(page, "Japan");
    await addMarket(page, "France");

    // --- 3. Japan: a local label derived from the core version ---------------
    await enterMarket(page, "Japan");

    await page.getByTestId("add-local-label").click();
    await page.getByLabel("Document").click();
    await page
      .getByRole("option", { name: "Prescribing information", exact: true })
      .click();
    await page.getByLabel("Language").fill("ja");
    await page.getByRole("button", { name: "Add local label" }).last().click();

    const local = page.getByTestId("local-label-row").first();

    await local.getByTestId("show-local-revisions").click();
    await choose(page, local, "local-label-content", `JP PI ${unique}`);
    await choose(page, local, "derived-from", `Core data sheet ${unique} v1`);

    await local.getByTestId("publish-revision").first().click();
    await page.getByLabel("Approved on").fill("2026-02-10");
    await page.getByLabel("Takes effect").fill("2026-03-01");
    await page.getByRole("button", { name: "Put in force" }).last().click();

    await expect(local.getByTestId("local-in-force")).toContainText(
      "Revision 1 in force",
    );

    // --- 4. the indication, with the paediatric population ------------------
    await recordIndication(
      page,
      "Treatment of type 2 diabetes mellitus in adults and children from 6 years.",
      "2026-03-01",
    );

    const indication = page.getByTestId("indication-row").first();

    await indication.getByTestId("add-population").click();
    await page.getByLabel("From age").fill("6");
    await page.getByLabel("Unit").click();
    await page.getByRole("option", { name: "years", exact: true }).click();
    await page.getByRole("button", { name: "Add population" }).last().click();

    // The DoD's last step: it is on the product's market view, beside the
    // market's own label and its trade names.
    await expect(indication.getByTestId("indication-status")).toContainText(
      "Approved",
    );
    await expect(indication.getByTestId("population-row")).toContainText(
      "6+ years",
    );
    await expect(page.getByTestId("market-overview")).toBeVisible();

    // --- 5. France: the same code, in France's words, then withdrawn --------
    await page.goto(`/regulatory/products/${globalProductId}/registrations`);
    await enterMarket(page, "France");

    await recordIndication(
      page,
      "Traitement du diabète de type 2 chez l'adulte.",
      "2026-04-15",
    );

    const french = page.getByTestId("indication-row").first();

    await french.getByTestId("withdraw-indication").click();
    await expect(french.getByTestId("indication-status")).toContainText(
      "Withdrawn",
    );

    // --- 6. the capstone question -------------------------------------------
    await page.goto(`/regulatory/products/${globalProductId}/registrations`);

    const section = page.getByTestId("indication-markets");

    await expect(section).toBeVisible();

    // Unasked is not the same as "approved nowhere", and the screen says so.
    await expect(section.getByTestId("no-condition")).toBeVisible();

    await section.getByTestId("condition-picker").click();
    await page
      .getByRole("option", { name: "Type 2 diabetes mellitus", exact: true })
      .click();

    // The answer, and the assertion the whole epic turns on: **one** approval,
    // not two. Both markets recorded T2DM; only Japan still holds it.
    const approved = section.getByTestId("condition-approved");

    await expect(approved).toContainText("Approved in 1 market");
    await expect(approved.getByTestId("condition-market-row")).toHaveCount(1);
    await expect(approved).toContainText("Japan");
    await expect(approved.getByTestId("condition-market-status")).toContainText(
      "Approved",
    );
    await expect(approved).toContainText("since 2026-03-01");

    // The same coded fact, in the words this market's label uses — shown, never
    // compared. Cross-market divergence reporting is EPIC-011.
    await expect(approved.getByTestId("condition-market-text")).toContainText(
      "children from 6 years",
    );

    const withdrawn = section.getByTestId("condition-withdrawn");

    await expect(withdrawn).toContainText("France");
    await expect(withdrawn.getByTestId("condition-market-status")).toContainText(
      "Withdrawn",
    );
    await expect(withdrawn.getByTestId("condition-market-text")).toContainText(
      "diabète de type 2",
    );

    // --- 7. and a condition nobody recorded ---------------------------------
    // "Nowhere" is an answer, not an empty screen — which is what shows the read
    // is driven by the coded condition rather than by the rows that exist.
    await section.getByTestId("condition-picker").click();
    await page.getByRole("option", { name: "Epilepsy", exact: true }).click();

    await expect(section.getByTestId("condition-nowhere")).toBeVisible();
    await expect(section.getByTestId("condition-approved")).toHaveCount(0);

    // --- 8. the row leads back to the market it describes --------------------
    await section.getByTestId("condition-picker").click();
    await page
      .getByRole("option", { name: "Type 2 diabetes mellitus", exact: true })
      .click();

    await approved.getByRole("link", { name: "Japan" }).click();

    await expect(page.getByTestId("market-overview")).toBeVisible();
    await expect(page.getByTestId("indication-row").first()).toContainText(
      "Type 2 diabetes mellitus",
    );

    expect(errors()).toEqual([]);
  });
});

type Page = import("@playwright/test").Page;
type Row = ReturnType<Page["getByTestId"]>;

async function recordIndication(
  page: Page,
  labelText: string,
  approvedOn: string,
) {
  await page.getByTestId("record-indication").click();
  await page.getByLabel("Condition").click();
  await page
    .getByRole("option", { name: "Type 2 diabetes mellitus", exact: true })
    .click();
  await page.getByLabel("As the label says it").fill(labelText);
  await page.getByLabel("Approved on").fill(approvedOn);
  await page.getByRole("button", { name: "Record indication" }).last().click();

  await expect(page.getByTestId("indication-row").first()).toBeVisible();
}

async function choose(page: Page, row: Row, testId: string, option: string) {
  await row.getByTestId(testId).first().click();
  await page.getByRole("option", { name: option, exact: false }).first().click();
}

async function addMarket(page: Page, country: string) {
  await page.getByRole("button", { name: "Add market" }).click();
  await page.getByLabel("Country").selectOption({ label: country });
  await page.getByLabel("Present since").fill("2026-01-05");
  await page.getByRole("button", { name: "Add" }).click();

  await expect(
    page.getByTestId("product-market-row").filter({ hasText: country }),
  ).toBeVisible();
}

async function enterMarket(page: Page, country: string) {
  await page
    .getByTestId("product-market-row")
    .filter({ hasText: country })
    .getByRole("link", { name: country })
    .click();

  await expect(page.getByTestId("market-overview")).toBeVisible();
}

async function uploadDocument(
  globalProductId: string,
  name: string,
): Promise<void> {
  const types = await (await api("/reference-data/document-types")).json();

  const form = new FormData();
  form.append("file", new Blob([PDF], { type: "application/pdf" }), `${name}.pdf`);
  form.append("documentTypeId", types[0].id);
  form.append("name", name);

  const upload = await fetch(
    `${API_URL}/api/products/${globalProductId}/documents`,
    {
      method: "POST",
      body: form,
      headers: { Cookie: await sessionCookies() },
    },
  );

  expect(upload.ok, `uploading ${name}`).toBeTruthy();
}
