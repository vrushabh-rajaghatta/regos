import { expect } from "@playwright/test";

import {
  test,
  api,
  API_URL,
  sessionCookies,
  collectErrors,
  EXPECTED_400,
  EXPECTED_409,
} from "./support";

const PDF = new TextEncoder().encode("%PDF-1.7\n% RegOS label test\n");

/**
 * **EPIC-018 S001 — the label a company holds above any market.**
 *
 * Two things are being proved, and only one of them is the CRUD:
 *
 * 1. A label versions on its own clock — draft, in force, superseded — and at
 *    most one version is ever in force. Publishing the next issue retires the
 *    one it replaces in the same act, so the two states cannot drift apart.
 * 2. **`ProductDocument` stays content storage; `Labeling` owns the meaning**
 *    ([ADR-059](../../../docs/adr/ADR-059-clinical-statements-are-facts-labels-are-artifacts.md) §6).
 *    The file is uploaded through the product's ordinary document library, and
 *    the label points at it. Nothing about a market, a label or a version is
 *    written to that document — which is the boundary this slice exists to
 *    test at its smallest.
 *
 * Nothing here mentions a country, and that is deliberate: what one market says
 * is a *local* label, and it arrives in S002.
 */
test.describe("A global label, versioned", () => {
  test("a core data sheet, its document, and the version that replaces it", async ({
    page,
  }) => {
    // Two refusals are part of what this spec proves, so their transport
    // status codes are declared rather than treated as noise: publishing with
    // no document attached is a 409, and a date on or before the version in
    // force is a 400.
    const errors = collectErrors(page, [EXPECTED_400, EXPECTED_409]);
    const unique = Date.now();

    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `LBL-${unique}`,
        name: `Labelled Product ${unique}`,
        type: "Drug",
      }),
    });

    expect(productResponse.ok).toBeTruthy();
    const { id: globalProductId } = await productResponse.json();

    // The content, uploaded where content lives — the product's document
    // library, which knows nothing about labels.
    await uploadDocument(globalProductId, `CCDS v1 ${unique}`);
    await uploadDocument(globalProductId, `CCDS v2 ${unique}`);

    await page.goto(`/regulatory/products/${globalProductId}/labels`);

    // --- 1. nothing held yet, and the screen says so ----------------------
    await expect(page.getByTestId("global-labels-empty")).toBeVisible();

    // --- 2. the label, born with its first draft --------------------------
    await page.getByTestId("add-global-label").click();
    await page.getByLabel("Name", { exact: true }).fill(`Core data sheet ${unique}`);
    await page.getByLabel("Label type").click();
    await page
      .getByRole("option", { name: "Company Core Data Sheet", exact: true })
      .click();
    await page.getByRole("button", { name: "Add label" }).last().click();

    const row = page
      .getByTestId("global-label-row")
      .filter({ hasText: `Core data sheet ${unique}` });

    await expect(row).toBeVisible();

    // A label exists from the moment someone starts writing it. Nothing is in
    // force, and that is stated rather than shown as a blank.
    await expect(row.getByTestId("label-nothing-in-force")).toBeVisible();
    await expect(row.getByTestId("label-draft")).toContainText("Version 1");

    // --- 3. publishing needs the document ---------------------------------
    await row.getByTestId("show-label-versions").click();

    const versions = row.getByTestId("label-version-row");
    await expect(versions).toHaveCount(1);

    await row.getByTestId("publish-version").click();
    await page.getByLabel("Takes effect").fill("2026-06-01");
    await page.getByRole("button", { name: "Publish version" }).last().click();

    // The rule that makes the content link load-bearing rather than
    // decorative: a version with no document is a number, not a label.
    await expect(page.getByTestId("publish-error")).toContainText(
      "Attach the label document before publishing",
    );

    // --- 4. attach, then publish ------------------------------------------
    await page.keyboard.press("Escape");

    await row.getByTestId("label-content").click();
    await page.getByRole("option", { name: `CCDS v1 ${unique}` }).click();

    await row.getByTestId("publish-version").click();
    await page.getByLabel("Takes effect").fill("2026-06-01");
    await page
      .getByLabel("What changed")
      .fill("First issue of the core data sheet.");
    await page.getByRole("button", { name: "Publish version" }).last().click();

    await expect(row.getByTestId("label-in-force")).toContainText(
      "Version 1 in force since 2026-06-01",
    );

    // --- 5. the next issue retires the one it replaces --------------------
    await row.getByTestId("start-draft").click();
    await expect(row.getByTestId("label-draft")).toContainText("Version 2");

    await row.getByTestId("label-content").click();
    await page.getByRole("option", { name: `CCDS v2 ${unique}` }).click();

    // Refused: business time moves forward, and two versions in force on one
    // day is the ambiguity the rule exists to prevent.
    await row.getByTestId("publish-version").click();
    await page.getByLabel("Takes effect").fill("2026-06-01");
    await page.getByRole("button", { name: "Publish version" }).last().click();

    await expect(page.getByTestId("publish-error")).toContainText(
      "takes effect after the one it replaces",
    );

    await page.getByLabel("Takes effect").fill("2026-09-01");
    await page.getByRole("button", { name: "Publish version" }).last().click();

    await expect(row.getByTestId("label-in-force")).toContainText(
      "Version 2 in force since 2026-09-01",
    );

    // The ranges meet exactly — no gap, no overlap, and no day on which two
    // versions were both in force.
    const retired = row
      .getByTestId("label-version-row")
      .filter({ hasText: "Version 1" });

    await expect(retired).toContainText("Superseded");
    await expect(retired).toContainText("2026-06-01 — 2026-08-31");

    // --- 6. a draft is the one thing that can be thrown away --------------
    await row.getByTestId("start-draft").click();
    await expect(row.getByTestId("label-draft")).toContainText("Version 3");

    await row.getByTestId("discard-draft").click();
    await expect(row.getByTestId("start-draft")).toBeVisible();

    // Nothing published was touched: two issues remain, and the number is
    // reissued because nobody ever cited the discarded one.
    await expect(row.getByTestId("label-in-force")).toContainText("Version 2");

    // --- 7. the document never learned it was a label ---------------------
    // ADR-059 §6, asserted rather than assumed. Labeling reads ProductDocument
    // and writes nothing to it; the library still shows an ordinary file.
    await page.goto(`/regulatory/products/${globalProductId}/documents`);

    await expect(
      page.getByText(`CCDS v1 ${unique}`, { exact: false }).first(),
    ).toBeVisible();

    expect(errors()).toEqual([]);
  });
});

async function uploadDocument(
  globalProductId: string,
  name: string,
): Promise<void> {
  const types = await (await api("/reference-data/document-types")).json();

  expect(types.length, "seeded document types").toBeGreaterThan(0);

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
