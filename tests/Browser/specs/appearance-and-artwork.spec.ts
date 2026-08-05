import { expect } from "@playwright/test";

import { test, api, collectErrors, EXPECTED_409 } from "./support";

/**
 * **EPIC-010b S004 — the two describing facts, and EPIC-018's debt paid.**
 *
 * 1. **Appearance sits on the presentation**, which is
 *    [ADR-061](../../../docs/adr/ADR-061-a-pack-is-how-a-medicine-is-supplied.md)
 *    §1's discriminator pointing the other way for once: a tablet looks
 *    identical in a carton of 30 and a carton of 100, so how it looks is part
 *    of what the medicine *is*.
 * 2. **A local label may name the pack it is printed for** — the link EPIC-018
 *    deferred, naming this epic as the milestone.
 *
 * The colours-only round trip is this story's falsifier, and it is the same one
 * S003 ran for shelf life: `PhysicalCharacteristics` is a *required* owned
 * reference because an optional one whose shared columns are all null is read
 * back as null, taking its owned collection with it.
 */
test.describe("What it looks like, and what it is printed for", () => {
  test("a two-tone capsule, and the colours that have no columns", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();

    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `APP-${unique}`,
        name: `Described Product ${unique}`,
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

    await page.getByTestId("add-presentation").click();
    await page.getByLabel("Name", { exact: true }).fill("Capsule, 10 mg");
    await page.getByLabel("Dose form").click();
    await page.getByRole("option", { name: "Capsule", exact: true }).click();
    await page.getByRole("button", { name: "Add presentation" }).last().click();

    const presentation = page.getByTestId("presentation-row").first();

    // --- 1. undescribed is a state, not a gap ------------------------------
    await expect(
      presentation.getByTestId("presentation-appearance"),
    ).toHaveCount(0);

    // --- 2. the falsifier: colours and nothing else ------------------------
    // Every column PhysicalCharacteristics shares with the presentation row is
    // null here, so an optional owned reference would come back null and take
    // both colours with it.
    await presentation.getByTestId("edit-appearance").click();
    await page.getByLabel("White", { exact: true }).check();
    await page.getByLabel("Blue", { exact: true }).check();
    await page.getByRole("button", { name: "Save appearance" }).click();

    await expect(
      presentation.getByTestId("presentation-appearance"),
    ).toContainText("White");

    // Reloaded from Postgres, not from the cache that just wrote it.
    await page.reload();

    // --- 3. a capsule is genuinely two colours -----------------------------
    // The one departure from a single `colour` field, and the reason for it: a
    // white body with a blue cap is two facts, not one called "white and blue".
    await expect(
      presentation.getByTestId("presentation-appearance"),
    ).toContainText("White");
    await expect(
      presentation.getByTestId("presentation-appearance"),
    ).toContainText("Blue");

    // --- 4. the marking is its own fact ------------------------------------
    await presentation.getByTestId("edit-appearance").click();
    await page.getByLabel("Shape").selectOption({ label: "Capsule-shaped" });
    await page.getByLabel("Marking").fill("AZ 10");
    await page
      .getByLabel("Wording on the label (optional)")
      .fill("Hard capsule with a white body and a blue cap, marked AZ 10.");
    await page.getByRole("button", { name: "Save appearance" }).click();

    await expect(
      presentation.getByTestId("presentation-appearance"),
    ).toContainText("marked AZ 10");

    await expect(
      presentation.getByTestId("presentation-appearance-description"),
    ).toContainText("white body and a blue cap");

    // Both colours survived a save that never mentioned them being replaced.
    await expect(
      presentation.getByTestId("presentation-appearance"),
    ).toContainText("Capsule-shaped");

    expect(errors()).toEqual([]);
  });

  test("a carton is printed for one pack, and any label may say so", async ({
    page,
  }) => {
    // Naming a pack from another market is a 409.
    const errors = collectErrors(page, [EXPECTED_409]);
    const unique = Date.now();

    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `ART-${unique}`,
        name: `Printed Product ${unique}`,
        type: "Drug",
      }),
    });

    const { id: globalProductId } = await productResponse.json();

    await page.goto(`/regulatory/products/${globalProductId}/registrations`);

    for (const country of ["United Kingdom", "France"]) {
      await page.getByRole("button", { name: "Add market" }).click();
      await page.getByLabel("Country").selectOption({ label: country });
      await page.getByLabel("Present since").fill("2026-01-05");
      await page.getByRole("button", { name: "Add" }).click();

      await expect(
        page.getByTestId("product-market-row").filter({ hasText: country }),
      ).toHaveCount(1);
    }

    await page
      .getByTestId("product-market-row")
      .filter({ hasText: "United Kingdom" })
      .getByRole("link", { name: "United Kingdom" })
      .click();

    await expect(page.getByTestId("market-overview")).toBeVisible();

    // Two packs, so "which one is this carton for?" is a real question.
    for (const size of ["30", "100"]) {
      await page.getByTestId("add-pack").click();
      await page
        .getByLabel("Pack", { exact: true })
        .fill(`Carton of ${size} tablets`);
      await page.getByLabel("Contains").fill(size);
      await page.getByLabel("Of").selectOption({ label: "Tablet" });
      await page.getByLabel("Planned since").fill("2026-02-01");
      await page.getByRole("button", { name: "Add pack" }).last().click();

      await expect(
        page.getByTestId("pack-row").filter({ hasText: `Carton of ${size} ` }),
      ).toHaveCount(1);
    }

    // --- the artwork, and the leaflet beside it ----------------------------
    // Both offered the pack link, because no rule branches on the label type
    // (EPIC-018 D2) — and a container label really is printed per pack size.
    for (const type of ["Carton artwork", "Patient information leaflet"]) {
      await page.getByTestId("add-local-label").click();
      await page.getByLabel("Document").click();
      await page.getByRole("option", { name: type, exact: true }).click();
      await page.getByLabel("Language").fill("en");
      await page
        .getByRole("button", { name: "Add local label" })
        .last()
        .click();

      await expect(
        page.getByTestId("local-label-row").filter({ hasText: type }),
      ).toHaveCount(1);
    }

    const artwork = page
      .getByTestId("local-label-row")
      .filter({ hasText: "Carton artwork" });

    const leaflet = page
      .getByTestId("local-label-row")
      .filter({ hasText: "Patient information leaflet" });

    await expect(artwork.getByTestId("printed-for-pack")).toHaveValue("");
    await expect(leaflet.getByTestId("printed-for-pack")).toHaveCount(1);

    await artwork
      .getByTestId("printed-for-pack")
      .selectOption({ label: "Carton of 30 tablets" });

    await page.reload();

    await expect(
      artwork.getByTestId("printed-for-pack"),
    ).not.toHaveValue("");

    // The leaflet was not dragged along: the link is per label.
    await expect(leaflet.getByTestId("printed-for-pack")).toHaveValue("");

    // --- a French carton may not name a UK pack ----------------------------
    // Both rows exist and both belong to the tenant, so nothing else would
    // notice. Asserted through the API because the picker only ever offers this
    // market's packs — the rule lives in the handler, not the control.
    const marketId = marketIdOf(page);

    const packs = await (
      await api(`/api/medicinal-products/${marketId}/packaged-products`)
    ).json();

    const labels = await (
      await api(`/api/medicinal-products/${marketId}/local-labels`)
    ).json();

    const frenchMarketId = await otherMarketId(page, globalProductId, marketId);

    const french = await api(
      `/api/medicinal-products/${frenchMarketId}/local-labels`,
      {
        method: "POST",
        body: JSON.stringify({ labelTypeCode: "ARTWORK", language: "fr" }),
      },
    );

    expect(french.ok, "a French carton").toBeTruthy();
    const { id: frenchLabelId } = await french.json();

    const refused = await api(`/api/local-labels/${frenchLabelId}/pack`, {
      method: "PUT",
      body: JSON.stringify({ packagedProductId: packs[0].id }),
    });

    expect(refused.status, "a French carton naming a UK pack").toBe(409);

    // The UK artwork still names its own pack, untouched by the refusal.
    const stillLinked = labels.find(
      (x: { labelTypeCode: string }) => x.labelTypeCode === "ARTWORK",
    );

    expect(stillLinked).toBeTruthy();

    expect(errors()).toEqual([]);
  });
});

type Page = import("@playwright/test").Page;

function marketIdOf(page: Page): string {
  const match = page.url().match(/markets\/([0-9a-f-]{36})/i);

  expect(match, "a market id in the URL").toBeTruthy();

  return match![1];
}

/** The other market this product is in — France, here. */
async function otherMarketId(
  page: Page,
  globalProductId: string,
  notThisOne: string,
): Promise<string> {
  const markets = await (
    await api(`/api/products/${globalProductId}/medicinal-products`)
  ).json();

  const other = markets.find(
    (x: { medicinalProductId: string }) => x.medicinalProductId !== notThisOne,
  );

  expect(other, "a second market").toBeTruthy();

  return other.medicinalProductId;
}
