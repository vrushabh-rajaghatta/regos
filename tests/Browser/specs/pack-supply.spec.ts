import { expect } from "@playwright/test";

import { test, api, collectErrors, EXPECTED_409 } from "./support";

/**
 * **EPIC-010b S003 — how it is supplied, how long it lasts.**
 *
 * Two facts, both on the **pack** rather than the product, which is
 * [ADR-061](../../../docs/adr/ADR-061-a-pack-is-how-a-medicine-is-supplied.md)
 * §1's discriminator used a third time:
 *
 * 1. **Legal status** — a 16-tablet pack may be general sale where a 100-tablet
 *    pack of the same tablets is pharmacy-only.
 * 2. **Shelf life and storage** — the same tablets in different containers keep
 *    for different lengths of time, because the container closure system is
 *    what the stability data was generated against.
 *
 * The spec also carries this story's **falsifier**. `ShelfLifeStorage` is
 * mapped as a *required* owned reference, and the reason is that an optional
 * one is read back as null when every column it shares is null — which is
 * exactly a pack whose only statement is *"protect from light"*. The
 * conditions-only round trip below is what would have caught that, and it runs
 * against real Postgres rather than a domain test's memory.
 */
test.describe("How a pack is supplied", () => {
  test("a statement built in two sittings, and the half that has no columns", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();

    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `SUP-${unique}`,
        name: `Supplied Product ${unique}`,
        type: "Drug",
      }),
    });

    expect(productResponse.ok).toBeTruthy();
    const { id: globalProductId } = await productResponse.json();

    await page.goto(`/regulatory/products/${globalProductId}/registrations`);

    await page.getByRole("button", { name: "Add market" }).click();
    await page.getByLabel("Country").selectOption({ label: "United Kingdom" });
    await page.getByLabel("Present since").fill("2026-01-05");
    await page.getByRole("button", { name: "Add" }).click();

    await page
      .getByTestId("product-market-row")
      .filter({ hasText: "United Kingdom" })
      .getByRole("link", { name: "United Kingdom" })
      .click();

    await expect(page.getByTestId("market-overview")).toBeVisible();

    await page.getByTestId("add-pack").click();
    await page.getByLabel("Pack", { exact: true }).fill("Carton of 30 tablets");
    await page.getByLabel("Contains").fill("30");
    await page.getByLabel("Of").selectOption({ label: "Tablet" });
    await page.getByLabel("Planned since").fill("2026-02-01");
    await page.getByRole("button", { name: "Add pack" }).last().click();

    const pack = page.getByTestId("pack-row").first();

    // --- 1. nothing said yet is a state, not a gap -------------------------
    await expect(pack.getByTestId("pack-legal-status-unstated")).toBeVisible();
    await expect(pack.getByTestId("pack-shelf-life")).toHaveCount(0);

    // --- 2. half a shelf life is refused, visibly --------------------------
    // The lesson S002 paid for: a form that fails validation silently is worse
    // than one that refuses out loud.
    await pack.getByTestId("edit-pack-supply").click();
    await page.getByLabel("Keeps for").fill("36");
    await page.getByRole("button", { name: "Save supply" }).click();

    await expect(
      page.getByText("A shelf life needs a period"),
    ).toBeVisible();

    // --- 3. the falsifier: a statement with no columns of its own ----------
    // Storage conditions and nothing else. Every scalar the owned type shares
    // with PackagedProducts is null, so an optional owned reference would come
    // back null and take these rows with it.
    await page.getByLabel("Keeps for").fill("");
    await page.getByLabel("Do not store above 25 °C").check();
    await page.getByLabel("Protect from light").check();
    await page.getByRole("button", { name: "Save supply" }).click();

    await expect(pack.getByTestId("pack-storage-conditions")).toContainText(
      "Protect from light",
    );

    // Reloaded from Postgres, not from the cache that just wrote it.
    await page.reload();

    await expect(pack.getByTestId("pack-storage-conditions")).toContainText(
      "Do not store above 25 °C",
    );
    await expect(pack.getByTestId("pack-storage-conditions")).toContainText(
      "Protect from light",
    );

    // --- 4. the rest of the statement, stated later ------------------------
    // Which is how it actually arrives: storage is settled before stability
    // data extends the shelf life.
    await pack.getByTestId("edit-pack-supply").click();
    await page.getByLabel("Legal status").selectOption({ label: "Pharmacy only" });
    await page.getByLabel("Keeps for").fill("3");
    await page.getByLabel("Period").selectOption({ label: "years" });
    await page
      .getByLabel("Wording on the label (optional)")
      .fill("After first opening: use within 28 days.");
    await page.getByRole("button", { name: "Save supply" }).click();

    await expect(pack.getByTestId("pack-legal-status")).toContainText(
      "Pharmacy only",
    );

    // --- 5. kept literal --------------------------------------------------
    // Three years is read back as three years. Normalising to 36 months would
    // be the first unit conversion in RegOS, and a shelf life is quoted on a
    // label in the words it was approved in.
    await expect(pack.getByTestId("pack-shelf-life")).toContainText("3 years");
    await expect(pack.getByTestId("pack-shelf-life")).not.toContainText("36");

    await expect(pack.getByTestId("pack-shelf-life-text")).toContainText(
      "within 28 days",
    );

    // The conditions stated in the earlier sitting were not disturbed by this
    // one — the whole statement is restated, so it had to be sent back intact.
    await expect(pack.getByTestId("pack-storage-conditions")).toContainText(
      "Protect from light",
    );

    expect(errors()).toEqual([]);
  });

  test("two packs of one product, supplied differently", async ({ page }) => {
    // The server's refusal of "none needed beside a precaution" is a 409.
    const errors = collectErrors(page, [EXPECTED_409]);
    const unique = Date.now();

    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `PAR-${unique}`,
        name: `Paracetamol ${unique}`,
        type: "Drug",
      }),
    });

    const { id: globalProductId } = await productResponse.json();

    await page.goto(`/regulatory/products/${globalProductId}/registrations`);

    await page.getByRole("button", { name: "Add market" }).click();
    await page.getByLabel("Country").selectOption({ label: "United Kingdom" });
    await page.getByLabel("Present since").fill("2026-01-05");
    await page.getByRole("button", { name: "Add" }).click();

    await page
      .getByTestId("product-market-row")
      .filter({ hasText: "United Kingdom" })
      .getByRole("link", { name: "United Kingdom" })
      .click();

    await expect(page.getByTestId("market-overview")).toBeVisible();

    for (const size of ["16", "100"]) {
      await page.getByTestId("add-pack").click();
      await page
        .getByLabel("Pack", { exact: true })
        .fill(`Pack of ${size} tablets`);
      await page.getByLabel("Contains").fill(size);
      await page.getByLabel("Of").selectOption({ label: "Tablet" });
      await page.getByLabel("Planned since").fill("2026-02-01");
      await page.getByRole("button", { name: "Add pack" }).last().click();

      await expect(
        page.getByTestId("pack-row").filter({ hasText: `Pack of ${size} ` }),
      ).toHaveCount(1);
    }

    const small = page.getByTestId("pack-row").filter({ hasText: "Pack of 16 " });
    const large = page.getByTestId("pack-row").filter({ hasText: "Pack of 100 " });

    // --- the discriminator, on screen --------------------------------------
    // Identical tablets, identical composition, different legal status. This is
    // why the classification is on the pack and not on the product.
    await small.getByTestId("edit-pack-supply").click();
    await page.getByLabel("Legal status").selectOption({ label: "General sale" });
    await page.getByRole("button", { name: "Save supply" }).click();

    await large.getByTestId("edit-pack-supply").click();
    await page.getByLabel("Legal status").selectOption({ label: "Pharmacy only" });

    // --- "none needed" is a conclusion, and it stands alone ----------------
    await page.getByLabel("No special storage precautions").check();
    await page.getByLabel("Protect from light").check();
    await page.getByRole("button", { name: "Save supply" }).click();

    await expect(
      page.getByText('"No special storage precautions" cannot sit beside'),
    ).toBeVisible();

    await page.getByLabel("Protect from light").uncheck();
    await page.getByRole("button", { name: "Save supply" }).click();

    await expect(small.getByTestId("pack-legal-status")).toContainText(
      "General sale",
    );
    await expect(large.getByTestId("pack-legal-status")).toContainText(
      "Pharmacy only",
    );

    // Rendered from its own testid, because "somebody checked and none are
    // needed" is a different statement from an empty list and the screen must
    // not blur them either.
    await expect(large.getByTestId("pack-storage-none-needed")).toContainText(
      "No special storage precautions",
    );
    await expect(small.getByTestId("pack-storage-conditions")).toHaveCount(0);
    await expect(small.getByTestId("pack-storage-none-needed")).toHaveCount(0);

    // --- the same rule, through the API ------------------------------------
    // The form disables nothing and simply refuses; the invariant lives in the
    // value object, so a caller that never loads the form meets it too.
    const packs = await (
      await api(`/api/medicinal-products/${marketIdOf(page)}/packaged-products`)
    ).json();

    const refused = await api(
      `/api/packaged-products/${packs[0].id}/supply`,
      {
        method: "PUT",
        body: JSON.stringify({
          legalStatusOfSupplyCode: null,
          shelfLifeValue: null,
          shelfLifeUnitCode: null,
          shelfLifeText: null,
          storageConditionCodes: ["NO_SPECIAL_PRECAUTIONS", "BELOW_25"],
        }),
      },
    );

    expect(refused.status, "none needed, beside a precaution").toBe(409);

    expect(errors()).toEqual([]);
  });
});

type Page = import("@playwright/test").Page;

/** The market the browser is on, read out of the URL. */
function marketIdOf(page: Page): string {
  const match = page.url().match(/markets\/([0-9a-f-]{36})/i);

  expect(match, "a market id in the URL").toBeTruthy();

  return match![1];
}
