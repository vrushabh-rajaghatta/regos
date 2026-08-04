import { expect } from "@playwright/test";

import {
  test,
  api,
  API_URL,
  sessionCookies,
  collectErrors,
  EXPECTED_400,
} from "./support";

const PDF = new TextEncoder().encode("%PDF-1.7\n% RegOS local label test\n");

/**
 * **EPIC-018 S002 — the market's own controlled document.**
 *
 * The story this proves is the one that decided the model
 * ([EPIC-018 S002](../../../docs/product/epics/EPIC-018-labeling-and-product-information.md)):
 *
 * ```
 * Core label v1 published
 *      ├── Japan  Revision 1   approved 12 May, effective 1 Jun
 *      │          Revision 2   translation fix — SAME core version
 *      └── France Revision 1   approved 20 Aug, effective 1 Sep
 * ```
 *
 * **Revision 2 is the whole argument.** Nothing changed globally; the derived-from
 * link is identical; the approved artifact changed. A single `contentId` on the
 * local label would have lost it. And France's history is not Japan's — same
 * core version, different dates, different current revision.
 *
 * Carton artwork appears as an ordinary label type, because a printed carton is
 * a controlled document an authority approved (D2). No branch, no special case.
 */
test.describe("A market's own approved label", () => {
  test("two markets adopting one core version, and a revision that changes nothing globally", async ({
    page,
  }) => {
    // Publishing a revision that takes effect before the one in force is a 400,
    // and the spec asserts that refusal on purpose.
    const errors = collectErrors(page, [EXPECTED_400]);
    const unique = Date.now();

    const { id: globalProductId } = await create("/api/products", {
      code: `LOC-${unique}`,
      name: `Localised Product ${unique}`,
      type: "Drug",
    });

    // --- the core position, published once ---------------------------------
    const coreDocument = await uploadDocument(globalProductId, `CCDS ${unique}`);

    // Selected by name in the UI, so only the core document's id is needed here.
    await uploadDocument(globalProductId, `JP PI ${unique}`);
    await uploadDocument(globalProductId, `JP PI fix ${unique}`);
    await uploadDocument(globalProductId, `FR PI ${unique}`);

    const core = await create(
      `/api/products/${globalProductId}/global-labels`,
      { name: `Core data sheet ${unique}`, labelTypeCode: "CCDS" },
    );

    await put(
      `/api/global-labels/${core.id}/versions/${core.draftVersionId}/content`,
      { contentId: coreDocument },
    );

    await post(
      `/api/global-labels/${core.id}/versions/${core.draftVersionId}/publish`,
      { effectiveFrom: "2026-01-01", changeSummary: null },
    );

    // --- two markets -------------------------------------------------------
    await page.goto(`/regulatory/products/${globalProductId}/registrations`);

    await addMarket(page, "Japan");
    await addMarket(page, "France");

    // --- 1. Japan, revision 1 ---------------------------------------------
    await enterMarket(page, "Japan");

    await expect(page.getByTestId("local-labels-empty")).toBeVisible();

    await addLocalLabel(page, "Prescribing information", "ja");

    const row = page.getByTestId("local-label-row").first();

    await expect(row.getByTestId("local-nothing-in-force")).toBeVisible();
    await expect(row.getByTestId("local-draft")).toContainText("Revision 1");

    await row.getByTestId("show-local-revisions").click();

    await choose(page, row, "local-label-content", `JP PI ${unique}`);
    await choose(page, row, "derived-from", `Core data sheet ${unique} v1`);

    await putInForce(page, row, "2026-05-12", "2026-06-01");

    await expect(row.getByTestId("local-in-force")).toContainText(
      "Revision 1 in force — approved 2026-05-12, effective 2026-06-01",
    );

    // --- 2. the revision that changes nothing globally ---------------------
    await row.getByTestId("start-local-revision").click();
    await expect(row.getByTestId("local-draft")).toContainText("Revision 2");

    await choose(page, row, "local-label-content", `JP PI fix ${unique}`);
    await choose(page, row, "derived-from", `Core data sheet ${unique} v1`);

    // Refused: a revision takes effect after the one it replaces.
    await putInForce(page, row, "2026-08-20", "2026-06-01");
    await expect(page.getByTestId("publish-revision-error")).toContainText(
      "takes effect after the one it replaces",
    );

    await page.getByLabel("Takes effect").fill("2026-09-01");
    await page.getByRole("button", { name: "Put in force" }).last().click();

    await expect(row.getByTestId("local-in-force")).toContainText(
      "Revision 2 in force",
    );

    // The core label never moved. Both revisions descend from v1, and the
    // retired one keeps its dates — which is the entire argument for a local
    // revision history.
    const revisions = row.getByTestId("local-revision-row");
    await expect(revisions).toHaveCount(2);

    const first = revisions.filter({ hasText: "Revision 1" });
    await expect(first).toContainText("Superseded");
    await expect(first).toContainText("from core v1");
    await expect(first).toContainText("2026-06-01 — 2026-08-31");

    await expect(revisions.filter({ hasText: "Revision 2" })).toContainText(
      "from core v1",
    );

    // --- 3. France's history is not Japan's --------------------------------
    await page.goto(`/regulatory/products/${globalProductId}/registrations`);
    await enterMarket(page, "France");

    await addLocalLabel(page, "Prescribing information", "fr");

    const frRow = page.getByTestId("local-label-row").first();

    await frRow.getByTestId("show-local-revisions").click();
    await choose(page, frRow, "local-label-content", `FR PI ${unique}`);
    await choose(page, frRow, "derived-from", `Core data sheet ${unique} v1`);

    await putInForce(page, frRow, "2026-08-20", "2026-09-01");

    // Same core version, its own approval date, and still on revision 1 while
    // Japan is on revision 2.
    await expect(frRow.getByTestId("local-in-force")).toContainText(
      "Revision 1 in force — approved 2026-08-20",
    );

    // --- 4. artwork is an ordinary label type ------------------------------
    await addLocalLabel(page, "Carton artwork", "fr");

    await expect(
      page.getByTestId("local-label-row").filter({ hasText: "Carton artwork" }),
    ).toBeVisible();

    expect(errors()).toEqual([]);
  });
});

type Page = import("@playwright/test").Page;
type Row = ReturnType<Page["getByTestId"]>;

async function addLocalLabel(page: Page, document: string, language: string) {
  await page.getByTestId("add-local-label").click();
  await page.getByLabel("Document").click();
  await page.getByRole("option", { name: document, exact: true }).click();
  await page.getByLabel("Language").fill(language);
  await page.getByRole("button", { name: "Add local label" }).last().click();

  await expect(page.getByTestId("add-local-label")).toBeVisible();
}

async function choose(page: Page, row: Row, testId: string, option: string) {
  await row.getByTestId(testId).first().click();
  await page.getByRole("option", { name: option, exact: false }).first().click();
}

async function putInForce(
  page: Page,
  row: Row,
  approvedOn: string,
  effectiveFrom: string,
) {
  await row.getByTestId("publish-revision").first().click();
  await page.getByLabel("Approved on").fill(approvedOn);
  await page.getByLabel("Takes effect").fill(effectiveFrom);
  await page.getByRole("button", { name: "Put in force" }).last().click();
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

async function create(path: string, body: unknown) {
  const response = await api(path, {
    method: "POST",
    body: JSON.stringify(body),
  });

  expect(response.ok, `POST ${path}`).toBeTruthy();

  return response.json();
}

async function post(path: string, body: unknown) {
  const response = await api(path, {
    method: "POST",
    body: JSON.stringify(body),
  });

  expect(response.ok, `POST ${path}`).toBeTruthy();
}

async function put(path: string, body: unknown) {
  const response = await api(path, {
    method: "PUT",
    body: JSON.stringify(body),
  });

  expect(response.ok, `PUT ${path}`).toBeTruthy();
}

async function uploadDocument(
  globalProductId: string,
  name: string,
): Promise<string> {
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

  return (await upload.json()).id;
}
