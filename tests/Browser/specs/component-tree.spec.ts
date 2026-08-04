import { expect } from "@playwright/test";

import { test, api, collectErrors, EXPECTED_409 } from "./support";

/**
 * **EPIC-010a S004 — what is in the box, and how deep it may go.**
 *
 * A `MedicinalProductComponent` answers a different question from a
 * presentation: not *"what is this when it is given?"* but *"what does the
 * patient physically receive?"* — and only that question justifies the
 * recursion, because a kit contains articles and those articles are themselves
 * things.
 *
 * Three things are proved here that a unit test cannot.
 *
 * **The depth rule is the epic's own DoD** — a component within a component
 * within a component, and the fourth refused. The rule reads the whole tree, so
 * it only holds if the handler loaded the whole tree.
 *
 * **A component holding others cannot be removed.** The alternative was a
 * cascade, and quiet data loss is not something a regulatory record should
 * allow.
 *
 * **The tree survives a round trip.** Depth is computed once, on the server, by
 * the same walk the rules use — so the indentation on screen and the depth the
 * guard measured cannot drift apart.
 */
test.describe("component tree", () => {
  test("builds a kit, refuses a fourth level, and refuses to empty one silently", async ({
    page,
  }) => {
    const errors = collectErrors(page, [EXPECTED_409]);
    const unique = Date.now();

    // --- a market to assemble ---------------------------------------------
    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `KIT-${unique}`,
        name: `Kit Subject ${unique}`,
        type: "Drug",
      }),
    });

    expect(productResponse.ok).toBeTruthy();
    const { id: globalProductId } = await productResponse.json();

    await page.goto(`/regulatory/products/${globalProductId}/registrations`);

    await page.getByRole("button", { name: "Add market" }).click();
    await page.getByLabel("Country").selectOption({ label: "Canada" });
    await page.getByLabel("Present since").fill("2026-01-05");
    await page.getByRole("button", { name: "Add" }).click();

    await page
      .getByTestId("product-market-row")
      .first()
      .getByRole("link", { name: "Canada" })
      .click();

    await expect(page.getByTestId("components-empty")).toBeVisible();

    // --- level one: what the patient is handed ----------------------------
    await page.getByTestId("add-component").click();
    await addComponent(page, "Kit", "Combination pack");

    const kit = row(page, "Combination pack");
    await expect(kit).toHaveAttribute("data-depth", "1");

    // --- level two: what is inside it -------------------------------------
    await kit.getByTestId("add-inside").click();
    await addComponent(page, "Vial", "Vial of powder");

    const vial = row(page, "Vial of powder");
    await expect(vial).toHaveAttribute("data-depth", "2");

    // A second article at the same level — the case a kit exists for.
    await kit.getByTestId("add-inside").click();
    await addComponent(page, "Ampoule", "Ampoule of solvent");

    await expect(row(page, "Ampoule of solvent")).toHaveAttribute(
      "data-depth",
      "2",
    );

    // --- level three: the deepest RegOS accepts ---------------------------
    await vial.getByTestId("add-inside").click();
    await addComponent(page, "Device", "Transfer device");

    const device = row(page, "Transfer device");
    await expect(device).toHaveAttribute("data-depth", "3");

    // --- and the fourth is refused, by name -------------------------------
    await device.getByTestId("add-inside").click();
    await addComponent(page, "Device", "Needle");

    await expect(page.getByTestId("component-error")).toContainText(
      "3 levels deep",
    );

    await page.keyboard.press("Escape");

    await expect(page.getByTestId("component-row")).toHaveCount(4);

    // --- a component holding others is not silently emptied ---------------
    await kit.getByTestId("remove-component").click();

    await expect(page.getByTestId("component-remove-error")).toContainText(
      "still holds others",
    );

    await expect(page.getByTestId("component-row")).toHaveCount(4);

    // --- moving out re-levels the subtree ---------------------------------
    // The vial goes to the top, and the device it holds comes with it — which
    // is why depth is the server's answer rather than a client-side count.
    await vial.getByTestId("move-out").click();

    await expect(row(page, "Vial of powder")).toHaveAttribute("data-depth", "1");
    await expect(row(page, "Transfer device")).toHaveAttribute("data-depth", "2");

    // --- and the emptied kit can now be taken apart -----------------------
    await row(page, "Ampoule of solvent").getByTestId("remove-component").click();
    await expect(page.getByTestId("component-row")).toHaveCount(3);

    await row(page, "Combination pack").getByTestId("remove-component").click();
    await expect(page.getByTestId("component-row")).toHaveCount(2);

    expect(errors()).toEqual([]);
  });
});

const row = (page: import("@playwright/test").Page, name: string) =>
  page.getByTestId("component-row").filter({ hasText: name });

async function addComponent(
  page: import("@playwright/test").Page,
  type: string,
  name: string,
) {
  await page.getByLabel("Type").click();
  await page.getByRole("option", { name: type, exact: true }).click();
  await page.getByLabel("Name", { exact: true }).fill(name);

  await page.getByRole("button", { name: "Add component" }).last().click();
}
