import { expect } from "@playwright/test";

import { test, api, collectErrors, EXPECTED_409 } from "./support";

/**
 * **EPIC-010b S002 — what is in the box.**
 *
 * The second recursive structure in RegOS, and the spec exists to show it is a
 * second **structure** rather than a second copy
 * ([ADR-061](../../../docs/adr/ADR-061-a-pack-is-how-a-medicine-is-supplied.md) §2):
 *
 * 1. **The pattern is `ComponentTree`'s** — a depth guard, a cycle guard, and a
 *    reading order computed once and used by both the rules and the screen.
 * 2. **The numbers are not.** A pack may be **four** layers deep where a
 *    component tree allows three, and siblings read most-first rather than
 *    alphabetically. A shared abstraction would already be wrong for one of
 *    them.
 *
 * **Material is the attribute that makes a layer not a component** (§1), and it
 * is asserted here: the blister's laminate is what the stability data was
 * generated against.
 */
test.describe("What is in the box", () => {
  test("a carton, its blisters, and the layer that cannot be removed", async ({
    page,
  }) => {
    // Removing a layer that still holds others is a 409, and so is a cycle.
    const errors = collectErrors(page, [EXPECTED_409]);
    const unique = Date.now();

    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `BOX-${unique}`,
        name: `Boxed Product ${unique}`,
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

    // --- the pack this is about --------------------------------------------
    await page.getByTestId("add-pack").click();
    await page
      .getByLabel("Pack", { exact: true })
      .fill("Carton of 3 blisters × 10 tablets");
    await page.getByLabel("Contains").fill("30");
    await page.getByLabel("Of").selectOption({ label: "Tablet" });
    await page.getByLabel("Planned since").fill("2026-02-01");
    await page.getByRole("button", { name: "Add pack" }).last().click();

    const pack = page.getByTestId("pack-row").first();

    await expect(pack.getByTestId("pack-contents-empty")).toBeVisible();

    // --- 1. the outermost layer --------------------------------------------
    await pack.getByTestId("add-package-item").click();
    await page.getByLabel("Layer").selectOption({ label: "Carton" });
    await page.getByLabel("How many").fill("1");
    await page.getByLabel("Made of (optional)").selectOption({
      label: "Paperboard",
    });
    await page.getByRole("button", { name: "Add layer" }).last().click();

    const rows = pack.getByTestId("package-item-row");

    await expect(rows).toHaveCount(1);
    await expect(rows.first().getByTestId("package-item-type")).toContainText(
      "Carton",
    );

    // --- 2. the blisters inside it -----------------------------------------
    // The material assertion: a blister's laminate is the fact a component
    // could never carry, because a component has a dose form instead.
    await rows.first().getByTestId("add-inside").click();
    await page.getByLabel("Layer").selectOption({ label: "Blister" });
    await page.getByLabel("How many").fill("3");
    await page.getByLabel("Made of (optional)").selectOption({
      label: "PVC/PVdC/aluminium",
    });
    await page.getByRole("button", { name: "Add layer" }).last().click();

    await expect(rows).toHaveCount(2);

    const blister = rows.filter({ hasText: "Blister" });

    await expect(blister.getByTestId("package-item-material")).toContainText(
      "PVC/PVdC/aluminium",
    );

    // The server computed the depth, and the row carries it — the same tree the
    // guard measured with.
    await expect(blister).toHaveAttribute("data-depth", "2");

    // --- 3. siblings read most-first, not alphabetically -------------------
    // A packing list says "3 blisters, 1 leaflet". By name the leaflet would
    // come first, which is the second place this tree differs from a component
    // tree.
    await rows.first().getByTestId("add-inside").click();
    await page.getByLabel("Layer").selectOption({ label: "Wallet" });
    await page.getByLabel("How many").fill("1");
    await page.getByRole("button", { name: "Add layer" }).last().click();

    await expect(rows).toHaveCount(3);
    await expect(rows.nth(1).getByTestId("package-item-type")).toContainText(
      "Blister",
    );
    await expect(rows.nth(2).getByTestId("package-item-type")).toContainText(
      "Wallet",
    );

    // --- 4. a layer that still holds others cannot be removed --------------
    await rows.first().getByTestId("remove-package-item").click();

    await expect(pack.getByTestId("remove-item-error")).toContainText(
      "Empty this layer",
    );

    // Nothing was taken with it.
    await expect(rows).toHaveCount(3);

    // --- 5. four layers deep, and no more ----------------------------------
    // Carton → blister is two. A wallet inside the blister is three, and a
    // blister inside that is four — the limit, one more than a component tree
    // allows.
    await blister.getByTestId("add-inside").click();
    await page.getByLabel("Layer").selectOption({ label: "Wallet" });
    await page.getByLabel("How many").fill("1");
    await page
      .getByLabel("Anything else (optional)")
      .fill("Inner wallet.");
    await page.getByRole("button", { name: "Add layer" }).last().click();

    // Named, because two wallets now exist and the nested one is emitted before
    // its outer sibling — the subtree comes with its parent, which is what
    // reading order means.
    const inner = rows.filter({ hasText: "Inner wallet" });

    await expect(inner).toHaveAttribute("data-depth", "3");

    await inner.getByTestId("add-inside").click();
    await page.getByLabel("Layer").selectOption({ label: "Sachet" });
    await page.getByLabel("How many").fill("2");
    await page.getByRole("button", { name: "Add layer" }).last().click();

    await expect(rows.filter({ hasText: "Sachet" })).toHaveAttribute(
      "data-depth",
      "4",
    );

    // The fifth is refused, and the message names the rule rather than a column.
    await rows
      .filter({ hasText: "Sachet" })
      .getByTestId("add-inside")
      .click();
    await page.getByLabel("Layer").selectOption({ label: "Ampoule" });
    await page.getByLabel("How many").fill("1");
    await page.getByRole("button", { name: "Add layer" }).last().click();

    await expect(page.getByTestId("package-item-error")).toContainText(
      "four layers deep",
    );

    await page.keyboard.press("Escape");

    // --- 6. a layer cannot be placed inside itself -------------------------
    // Asserted through the API: the screen offers "Lift out" and "Add inside"
    // and no way to name an arbitrary parent, so the cycle is unreachable by
    // clicking — which is the point. The rule is in the tree, not the control.
    const packId = await packIdOf(page);

    const items = await (
      await api(`/api/packaged-products/${packId}/items`)
    ).json();

    const carton = items.find(
      (x: { itemTypeCode: string }) => x.itemTypeCode === "CARTON",
    );
    const inBlister = items.find(
      (x: { itemTypeCode: string }) => x.itemTypeCode === "BLISTER",
    );

    const cycle = await api(`/api/package-items/${carton.id}/parent`, {
      method: "PUT",
      body: JSON.stringify({ newParentPackageItemId: inBlister.id }),
    });

    expect(cycle.status, "a carton inside its own blister").toBe(409);

    // --- 7. lifting a layer out takes its contents with it -----------------
    await page.reload();

    await pack
      .getByTestId("package-item-row")
      .filter({ hasText: "Blister" })
      .getByTestId("lift-package-item")
      .click();

    // The blister is now outermost, and the wallet it holds moved with it.
    await expect(
      pack.getByTestId("package-item-row").filter({ hasText: "Blister" }),
    ).toHaveAttribute("data-depth", "1");

    expect(errors()).toEqual([]);
  });
});

type Page = import("@playwright/test").Page;

/** The pack's id, read from the API for the market the browser is on. */
async function packIdOf(page: Page): Promise<string> {
  const match = page.url().match(/markets\/([0-9a-f-]{36})/i);

  expect(match, "a market id in the URL").toBeTruthy();

  const packs = await (
    await api(`/api/medicinal-products/${match![1]}/packaged-products`)
  ).json();

  return packs[0].id;
}
