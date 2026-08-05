import { expect } from "@playwright/test";

import { test, api, collectErrors, EXPECTED_409 } from "./support";

/**
 * **EPIC-010b S005 — the capstone.**
 *
 * > *"Which packs are authorised in this market, and how are they supplied?"*
 *
 * The question the epic was cut to answer, read end to end. Every fact on the
 * screen comes from a different aggregate and none of them is duplicated:
 *
 * | Fact | Story | Aggregate |
 * |---|---|---|
 * | the pack and its size | S001 | `PackagedProduct` |
 * | how many layers it holds | S002 | `PackageItem` |
 * | legal status, shelf life, storage | S003 | `ShelfLifeStorage` |
 * | which licence, and from when | S005 | `PackAuthorisation` |
 *
 * **The `Product` aggregate is never touched by authorising a pack**, which is
 * ADR-061 §3's whole claim: `Registration.Domain` already depends on
 * `Product.Domain`, so the relationship had to live in Registration — and it
 * turned out to want to, because a foreign key cannot carry the date a pack was
 * added to a licence by variation.
 */
test.describe("Which packs are authorised here", () => {
  test("two packs, one licence, and only one of them authorised", async ({
    page,
  }) => {
    // Authorising a pack twice under one licence is a 409, and so is naming a
    // pack from another market.
    const errors = collectErrors(page, [EXPECTED_409]);
    const unique = Date.now();

    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `CAP-${unique}`,
        name: `Capstone Product ${unique}`,
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

    // --- 1. before any pack exists -----------------------------------------
    await expect(
      page.getByTestId("authorised-packs-empty"),
    ).toBeVisible();

    // --- 2. two packs, because "which one?" must be a real question --------
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

    const thirty = page
      .getByTestId("authorised-pack-row")
      .filter({ hasText: "Carton of 30 " });

    const hundred = page
      .getByTestId("authorised-pack-row")
      .filter({ hasText: "Carton of 100 " });

    // Unauthorised is stated, not implied by absence: a pack in design has no
    // licence yet, and that is ordinary rather than an error.
    await expect(thirty.getByTestId("pack-unauthorised")).toBeVisible();
    await expect(hundred.getByTestId("pack-unauthorised")).toBeVisible();

    // --- 3. how the 30 is supplied (S003) and what is in it (S002) ---------
    const thirtyPackRow = page
      .getByTestId("pack-row")
      .filter({ hasText: "Carton of 30 " });

    await thirtyPackRow.getByTestId("edit-pack-supply").click();
    await page.getByLabel("Legal status").selectOption({ label: "Prescription only" });
    await page.getByLabel("Keeps for").fill("36");
    await page.getByLabel("Period").selectOption({ label: "months" });
    await page.getByLabel("Do not store above 25 °C", { exact: true }).check();
    await page.getByRole("button", { name: "Save supply" }).click();

    await thirtyPackRow.getByTestId("add-package-item").click();
    await page.getByLabel("Layer").selectOption({ label: "Carton" });
    await page.getByLabel("How many").fill("1");
    await page.getByRole("button", { name: "Add layer" }).last().click();

    // Every story's fact, on one line, from four aggregates.
    await expect(thirty.getByTestId("pack-supply-summary")).toContainText(
      "Prescription only",
    );
    await expect(thirty.getByTestId("pack-supply-summary")).toContainText(
      "36 months",
    );
    await expect(thirty.getByTestId("pack-supply-summary")).toContainText(
      "Do not store above 25 °C",
    );
    await expect(thirty.getByTestId("pack-supply-summary")).toContainText(
      "1 layer",
    );

    // --- 4. a licence, and the date it gained this pack --------------------
    await page.getByRole("button", { name: "New registration" }).click();
    await page.getByLabel("Authority").selectOption({ index: 1 });
    await page.getByLabel("Authorisation holder").selectOption({ index: 1 });
    await page.getByLabel("Planned on").fill("2021-01-10");
    await page.getByRole("button", { name: "Create" }).click();

    await expect(page.getByTestId("market-registration")).toHaveCount(1);

    const registrationId = await registrationIdOf(page, market(page));

    // Numbered, so the capstone row has something to show.
    const approved = await api(
      `/registrations/${registrationId}/approval`,
      {
        method: "POST",
        body: JSON.stringify({
          registrationNumber: `PL 12345/000${unique % 10}`,
          approvedOn: "2021-06-01",
          expiresOn: "2031-06-01",
        }),
      },
    );

    expect(approved.ok, "the licence is approved").toBeTruthy();

    await page.reload();

    // **The date a foreign key could not carry.** The licence was approved in
    // 2021; this pack was added to it in 2024, by variation.
    await thirty.getByTestId("authorise-pack").click();
    await page.getByLabel("Licence").selectOption({ index: 1 });
    await page.getByLabel("Authorised on").fill("2024-03-01");
    await page.getByTestId("confirm-authorise-pack").click();

    await expect(thirty.getByTestId("pack-authorised")).toContainText(
      "1 licence",
    );
    await expect(thirty.getByTestId("authorised-on")).toContainText(
      "2024-03-01",
    );

    // --- 5. only one of them ------------------------------------------------
    // The 100 is still planned and still unauthorised, which is the whole point
    // of listing every pack rather than only the authorised ones.
    await expect(hundred.getByTestId("pack-unauthorised")).toBeVisible();

    // --- 6. it survives a reload, and the Product aggregate never moved ----
    await page.reload();

    await expect(thirty.getByTestId("authorised-on")).toContainText(
      "2024-03-01",
    );

    // The pack itself carries no licence — Product stays independent of who
    // authorised anything (ADR-061 §3). Read from the pack's own route, which
    // is the one Product owns.
    const packs = await (
      await api(`/api/medicinal-products/${market(page)}/packaged-products`)
    ).json();

    expect(
      Object.keys(packs[0]).some((key) => key.toLowerCase().includes("registration")),
      "the pack knows nothing about registrations",
    ).toBeFalsy();

    // --- 7. the same pack twice under one licence is refused ---------------
    const twice = await api(
      `/api/registrations/${registrationId}/authorised-packs`,
      {
        method: "POST",
        body: JSON.stringify({
          packagedProductId: authorisedPackId(packs, "Carton of 30 tablets"),
          authorisedOn: "2025-01-01",
        }),
      },
    );

    expect(twice.status, "the same pack twice under one licence").toBe(409);

    expect(errors()).toEqual([]);
  });

  test("one licence authorises a family of packs", async ({ page }) => {
    // RIM says License → Packaged Product, *Single*. It is wrong, and this is
    // the departure stated as a test rather than as prose (ADR-061 §3).
    const errors = collectErrors(page);
    const unique = Date.now();

    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `FAM-${unique}`,
        name: `Family Product ${unique}`,
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

    for (const size of ["14", "28", "56"]) {
      await page.getByTestId("add-pack").click();
      await page
        .getByLabel("Pack", { exact: true })
        .fill(`Carton of ${size} tablets`);
      await page.getByLabel("Planned since").fill("2026-02-01");
      await page.getByRole("button", { name: "Add pack" }).last().click();

      await expect(
        page.getByTestId("pack-row").filter({ hasText: `Carton of ${size} ` }),
      ).toHaveCount(1);
    }

    await page.getByRole("button", { name: "New registration" }).click();
    await page.getByLabel("Authority").selectOption({ index: 1 });
    await page.getByLabel("Authorisation holder").selectOption({ index: 1 });
    await page.getByLabel("Planned on").fill("2021-01-10");
    await page.getByRole("button", { name: "Create" }).click();

    await expect(page.getByTestId("market-registration")).toHaveCount(1);

    const marketId = market(page);
    const registrationId = await registrationIdOf(page, marketId);

    const packs = await (
      await api(`/api/medicinal-products/${marketId}/packaged-products`)
    ).json();

    // All three, under the one licence — and each on its own date, because
    // they were not all added at once.
    const dates = ["2022-01-10", "2022-01-10", "2024-09-30"];

    for (const [index, pack] of packs.entries()) {
      const authorised = await api(
        `/api/registrations/${registrationId}/authorised-packs`,
        {
          method: "POST",
          body: JSON.stringify({
            packagedProductId: pack.id,
            authorisedOn: dates[index],
          }),
        },
      );

      expect(authorised.ok, `pack ${index} authorised`).toBeTruthy();
    }

    await page.reload();

    await expect(page.getByTestId("pack-authorised")).toHaveCount(3);
    await expect(page.getByTestId("pack-unauthorised")).toHaveCount(0);

    // The 56 arrived two years after the other two, and the model kept that.
    await expect(
      page
        .getByTestId("authorised-pack-row")
        .filter({ hasText: "Carton of 56 " })
        .getByTestId("authorised-on"),
    ).toContainText("2024-09-30");

    // --- removing one recorded in error ------------------------------------
    await page
      .getByTestId("authorised-pack-row")
      .filter({ hasText: "Carton of 14 " })
      .getByTestId("withdraw-authorisation")
      .click();

    await expect(page.getByTestId("pack-authorised")).toHaveCount(2);

    // The pack is still there. Removing an authorisation removes a statement
    // about a relationship, never the thing it described.
    await expect(
      page
        .getByTestId("authorised-pack-row")
        .filter({ hasText: "Carton of 14 " }),
    ).toHaveCount(1);

    expect(errors()).toEqual([]);
  });
});

type Page = import("@playwright/test").Page;

function market(page: Page): string {
  const match = page.url().match(/markets\/([0-9a-f-]{36})/i);

  expect(match, "a market id in the URL").toBeTruthy();

  return match![1];
}

async function registrationIdOf(
  page: Page,
  medicinalProductId: string,
): Promise<string> {
  const match = page.url().match(/products\/([0-9a-f-]{36})/i);

  const registrations = await (
    await api(`/api/products/${match![1]}/registrations`)
  ).json();

  const held = registrations.find(
    (x: { medicinalProductId: string }) =>
      x.medicinalProductId === medicinalProductId,
  );

  expect(held, "a registration for this market").toBeTruthy();

  return held.registrationId;
}

function authorisedPackId(
  packs: { id: string; description: string }[],
  description: string,
): string {
  const pack = packs.find((x) => x.description === description);

  expect(pack, description).toBeTruthy();

  return pack!.id;
}
