import { expect } from "@playwright/test";

import { test, api, collectErrors, EXPECTED_400, EXPECTED_409 } from "./support";

/**
 * **EPIC-010b S001 — what the market actually sells.**
 *
 * The slice exists to establish an aggregate boundary, and two things prove it
 * is the right one:
 *
 * 1. **A pack is not a component.** The discriminator in
 *    [ADR-061](../../../docs/adr/ADR-061-a-pack-is-how-a-medicine-is-supplied.md):
 *    *does it change when the same medicine is sold in a different pack size?*
 *    Two packs of one product appear here, and neither touches the presentation
 *    or the component tree above them — same medicine, different packs.
 * 2. **A pack's commercial history is its own.** The 30 can be on sale while the
 *    100 is discontinued, on different dates, and the market above stays
 *    launched throughout. That asymmetry is the reason the status lives on the
 *    pack rather than being read from the market.
 *
 * **Half a pack size is refused**, and the spec asserts it: *30* alone could be
 * tablets, millilitres or vials.
 */
test.describe("What this market sells", () => {
  test("two packs of one product, each with its own commercial history", async ({
    page,
  }) => {
    // Half a pack size is a 400 and re-planning a pack is a 409; both refusals
    // are part of what this proves, so their statuses are declared.
    const errors = collectErrors(page, [EXPECTED_400, EXPECTED_409]);
    const unique = Date.now();

    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `PCK-${unique}`,
        name: `Packed Product ${unique}`,
        type: "Drug",
      }),
    });

    expect(productResponse.ok).toBeTruthy();
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

    // --- 1. nothing packed yet, and the screen says so ---------------------
    await expect(page.getByTestId("packs-empty")).toBeVisible();

    // --- 2. half a pack size is refused ------------------------------------
    await page.getByTestId("add-pack").click();
    await page
      .getByLabel("Pack", { exact: true })
      .fill("Carton of 3 blisters × 10 film-coated tablets");
    await page.getByLabel("Contains").fill("30");
    await page.getByLabel("Planned since").fill("2026-02-01");
    await page.getByRole("button", { name: "Add pack" }).last().click();

    // Client-side, from the same rule the aggregate enforces: 30 alone could be
    // tablets, millilitres or vials.
    await expect(page.getByText("A pack size needs a unit.")).toBeVisible();

    // --- 3. the pack, with its unit ----------------------------------------
    await page.getByLabel("Of").selectOption({ label: "Tablet" });
    await page.getByLabel("Pack code (optional)").fill("PZN 12345678");
    await page.getByRole("button", { name: "Add pack" }).last().click();

    const thirty = page
      .getByTestId("pack-row")
      .filter({ hasText: "3 blisters" });

    await expect(thirty).toBeVisible();
    await expect(thirty.getByTestId("pack-size")).toContainText("30 Tablet");
    await expect(thirty.getByTestId("pack-code")).toContainText("PZN 12345678");

    // A pack begins Planned — it is designed, coded and costed long before it
    // is on sale, and ADR-061 §3 is built on that being representable.
    await expect(thirty.getByTestId("pack-status")).toContainText("Planned");

    // --- 4. a second pack of the same medicine -----------------------------
    // The discriminator, on screen: same presentation, same components, a
    // different pack. Nothing above this section changed.
    await page.getByTestId("add-pack").click();
    await page
      .getByLabel("Pack", { exact: true })
      .fill("Bottle of 100 film-coated tablets");
    await page.getByLabel("Contains").fill("100");
    await page.getByLabel("Of").selectOption({ label: "Tablet" });
    await page.getByLabel("Planned since").fill("2026-02-01");
    await page.getByRole("button", { name: "Add pack" }).last().click();

    await expect(page.getByTestId("pack-row")).toHaveCount(2);

    const hundred = page
      .getByTestId("pack-row")
      .filter({ hasText: "Bottle of 100" });

    // --- 5. each pack has its own commercial history -----------------------
    await thirty.getByTestId("change-pack-status").click();
    await page.getByLabel("Now").selectOption({ label: "On sale" });
    await page.getByLabel("Took effect on").fill("2026-04-01");
    await page.getByRole("button", { name: "Save status" }).click();

    await expect(thirty.getByTestId("pack-status")).toContainText("On sale");

    await hundred.getByTestId("change-pack-status").click();
    await page.getByLabel("Now").selectOption({ label: "Discontinued" });
    await page.getByLabel("Took effect on").fill("2026-05-15");
    await page
      .getByLabel("Note (optional)")
      .fill("Bottle presentation withdrawn from this market.");
    await page.getByRole("button", { name: "Save status" }).click();

    // The assertion the aggregate boundary exists for: one product, two packs,
    // opposite commercial states on different dates.
    await expect(hundred.getByTestId("pack-status")).toContainText(
      "Discontinued",
    );
    await expect(thirty.getByTestId("pack-status")).toContainText("On sale");

    // Nothing was rewritten — the pack became discontinued on a date.
    await expect(hundred.getByTestId("pack-history-row")).toHaveCount(2);
    await expect(hundred.getByTestId("pack-history")).toContainText(
      "Bottle presentation withdrawn",
    );

    // --- 6. a pack that reached the market cannot be planned again ---------
    // Asserted through the API because the screen does not offer Planned at
    // all — which is the point: the rule is in the aggregate, not the dropdown.
    const packs = await (
      await api(
        `/api/medicinal-products/${await marketIdOf(page)}/packaged-products`,
      )
    ).json();

    const onSale = packs.find(
      (pack: { description: string }) =>
        pack.description.includes("3 blisters"),
    );

    const refused = await api(
      `/api/packaged-products/${onSale.id}/marketing-status`,
      {
        method: "POST",
        body: JSON.stringify({ status: "Planned", occurredOn: "2026-06-01" }),
      },
    );

    expect(refused.status, "re-planning a pack on sale").toBe(409);

    // And business time moves forward.
    const backdated = await api(
      `/api/packaged-products/${onSale.id}/marketing-status`,
      {
        method: "POST",
        body: JSON.stringify({
          status: "TemporarilyUnavailable",
          occurredOn: "2026-01-01",
        }),
      },
    );

    expect(backdated.status, "a status before the one it replaces").toBe(400);

    // --- 7. correcting a pack leaves its history alone ---------------------
    await page.reload();

    await thirty.getByTestId("correct-pack").click();
    await page
      .getByLabel("Pack", { exact: true })
      .fill("Carton of 3 blisters × 10 tablets");
    await page.getByRole("button", { name: "Save pack" }).click();

    await expect(thirty.getByTestId("pack-description")).toContainText(
      "× 10 tablets",
    );

    // What a pack *is* and what is commercially true of it move on different
    // clocks — restating one does not touch the other.
    await expect(thirty.getByTestId("pack-status")).toContainText("On sale");

    expect(errors()).toEqual([]);
  });
});

type Page = import("@playwright/test").Page;

/** The market id, taken from the URL the browser is already on. */
async function marketIdOf(page: Page): Promise<string> {
  const match = page.url().match(/markets\/([0-9a-f-]{36})/i);

  expect(match, "a market id in the URL").toBeTruthy();

  return match![1];
}
