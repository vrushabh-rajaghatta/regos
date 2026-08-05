import { expect, type Page } from "@playwright/test";

import { test, api, collectErrors } from "./support";

/**
 * **EPIC-022 capstone — a country is a set of facts other capabilities reason
 * from, and this proves the reasoning composes.**
 *
 * Each story proved one fact in isolation: ISO identity (S001), regulatory
 * groupings (S002), expected label languages (S003), accepted stability
 * conditions (S004). **This one proves they compose without interfering** — and
 * it does that by isolating the only variable the epic is about.
 *
 * **One global product. Two markets. Identical inputs in both.** The same pack,
 * the same size, the same legal status, the same shelf life, the same storage
 * precaution, the same testing condition, the same single English label, the
 * same licence. Every keystroke below is performed twice, identically.
 *
 * | | Canada | India |
 * |---|---|---|
 * | ISO identity | CAN | IND |
 * | groupings | ICH · PIC/S | **none** |
 * | expected languages | **en + fr** → French missing | en → covered |
 * | accepts | 25/60 **or** 30/65 | **30/70** |
 * | the pack, tested at 25/60 | **accepted** | **not accepted** |
 *
 * **So the assertion that carries the epic is the one about what does *not*
 * change.** The pack-derived line reads identically in both markets, and both
 * packs authorise identically. Every difference on either screen is therefore
 * attributable to the country row and to nothing else.
 *
 * **A note on "the same pack", because ADR-039 makes it precise.** The market
 * tier is market-local: Canada's pack and India's pack are two rows, not one
 * shared record. They are identical here because a person entered them
 * identically — which is what makes the comparison honest. Nothing is shared
 * *except* the global product and the country data, so a difference on screen
 * has exactly one place it can have come from.
 */
test.describe("A country decides things", () => {
  test("two markets, identical inputs, and only the country-derived facts differ", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();

    const productResponse = await api("/api/products", {
      method: "POST",
      body: JSON.stringify({
        code: `DEPTH-${unique}`,
        name: `Country Depth Product ${unique}`,
        type: "Drug",
      }),
    });

    const { id: globalProductId } = await productResponse.json();

    const said: Record<string, MarketReading> = {};

    for (const country of ["Canada", "India"]) {
      said[country] = await recordIdenticalMarket(page, globalProductId, country);
    }

    // --- what does not change ----------------------------------------------
    // **The assertion the epic rests on.** Identical inputs produce an
    // identical pack-derived line, so nothing below can be blamed on the pack.
    expect(said["Canada"].packSummary).toEqual(said["India"].packSummary);
    expect(said["Canada"].authorised).toEqual(said["India"].authorised);

    // **Named, so the equality above cannot pass vacuously.** Two empty
    // summaries are also equal, and would prove nothing at all — this says the
    // line being compared is the one carrying every pack fact the epics before
    // this one added.
    for (const fact of [
      "Prescription only",
      "36 months",
      "Do not store above 25 °C",
    ]) {
      expect(said["Canada"].packSummary).toContain(fact);
    }

    expect(said["Canada"].authorised).toContain("1 licence");

    // --- what does change, and all of it is the country ---------------------
    // 1. Stability (S004). Canada accepts the condition; India does not — and
    //    India's 30 °C/70% RH belongs to no climatic zone anybody publishes,
    //    which is why RegOS stores conditions (E39).
    expect(said["Canada"].accepts).toContain("25 °C / 60% RH");
    expect(said["India"].accepts).toContain("30 °C / 70% RH");
    expect(said["Canada"].stabilityVerdict).toBe("accepted");
    expect(said["India"].stabilityVerdict).toBe("unaccepted");

    // 2. Languages (S003). The same single English label leaves Canada short of
    //    French and leaves India complete — advisory in both cases, and neither
    //    refused anything.
    expect(said["Canada"].languagesMissing).toContain("fr");
    expect(said["India"].languagesMissing).toBeNull();

    expect(errors()).toEqual([]);

    // --- 3. groupings and identity, from the portfolio's front door ---------
    await page.goto("/regulatory/registrations");

    const canada = page
      .getByTestId("registration-market")
      .filter({ hasText: "Canada" });

    const india = page
      .getByTestId("registration-market")
      .filter({ hasText: "India" });

    await expect(canada).toContainText("ICH");
    await expect(canada).toContainText("PIC/S");

    // Empty is a recorded answer, not an unfilled field: CDSCO is an ICH
    // *observer* and India is not a PIC/S participant (E37).
    await expect(india).not.toContainText("ICH");
    await expect(india).not.toContainText("PIC/S");

    await page.getByTestId("region-filter").selectOption("ICH");

    await expect(canada).toHaveCount(1);
    await expect(india).toHaveCount(0);

    await page.getByTestId("region-filter").selectOption("");

    // 4. ISO identity (S001) — what a machine-readable submission names the
    //    country by, and not derivable from the alpha-2 code.
    await india.click();
    await expect(page.getByTestId("country-iso-identity")).toContainText("IND");

    await page.goto("/regulatory/registrations");
    await canada.click();
    await expect(page.getByTestId("country-iso-identity")).toContainText("CAN");

    expect(errors()).toEqual([]);
  });
});

interface MarketReading {
  packSummary: string;
  authorised: string;
  accepts: string;
  stabilityVerdict: "accepted" | "unaccepted" | "unknown";
  languagesMissing: string | null;
}

/**
 * Records one market, and performs **exactly the same actions** in it whichever
 * country it is. Nothing below reads the country name or branches on it — which
 * is what makes the readings it returns comparable.
 */
async function recordIdenticalMarket(
  page: Page,
  globalProductId: string,
  country: string,
): Promise<MarketReading> {
  await page.goto(`/regulatory/products/${globalProductId}/registrations`);

  await page.getByRole("button", { name: "Add market" }).click();
  await page.getByLabel("Country").selectOption({ label: country });
  await page.getByLabel("Present since").fill("2026-01-05");
  await page.getByRole("button", { name: "Add" }).click();

  await page
    .getByTestId("product-market-row")
    .filter({ hasText: country })
    .getByRole("link", { name: country })
    .click();

  await expect(page.getByTestId("market-overview")).toBeVisible();

  // A licence, so the pack has something to be authorised under.
  await page.getByRole("button", { name: "New registration" }).click();
  await page.getByLabel("Authority").selectOption({ index: 1 });
  await page.getByLabel("Authorisation holder").selectOption({ index: 1 });
  await page.getByLabel("Planned on").fill("2026-01-10");
  await page.getByRole("button", { name: "Create" }).click();

  await expect(page.getByTestId("market-registration")).toHaveCount(1);

  // The pack, and how it is supplied — every value identical in both markets.
  await page.getByTestId("add-pack").click();
  await page.getByLabel("Pack", { exact: true }).fill("Carton of 30 tablets");
  await page.getByLabel("Contains").fill("30");
  await page.getByLabel("Of").selectOption({ label: "Tablet" });
  await page.getByLabel("Planned since").fill("2026-02-01");
  await page.getByRole("button", { name: "Add pack" }).last().click();

  const packRow = page
    .getByTestId("pack-row")
    .filter({ hasText: "Carton of 30 " });

  await expect(packRow).toHaveCount(1);

  await packRow.getByTestId("edit-pack-supply").click();
  await page
    .getByLabel("Legal status")
    .selectOption({ label: "Prescription only" });
  await page.getByLabel("Keeps for").fill("36");
  await page.getByLabel("Period").selectOption({ label: "months" });
  await page
    .getByTestId("storage-conditions")
    .getByLabel("Do not store above 25 °C", { exact: true })
    .check();
  await page
    .getByTestId("tested-at")
    .getByLabel("25 °C / 60% RH", { exact: true })
    .check();
  await page.getByRole("button", { name: "Save supply" }).click();

  // One English label. Canada expects two languages and India one, so the same
  // act is complete in one market and short in the other.
  await page.getByTestId("add-local-label").click();
  await page.getByLabel("Document").click();
  await page
    .getByRole("option", { name: "Prescribing information", exact: true })
    .click();
  await page.getByLabel("Language").fill("en");
  await page.getByRole("button", { name: "Add local label" }).last().click();

  await expect(page.getByTestId("local-label-row")).toHaveCount(1);

  const authorisedRow = page.getByTestId("authorised-pack-row").first();

  await authorisedRow.getByTestId("authorise-pack").click();
  await page.getByLabel("Licence").selectOption({ index: 1 });
  await page.getByLabel("Authorised on").fill("2026-02-01");
  await page.getByTestId("confirm-authorise-pack").click();

  await expect(authorisedRow.getByTestId("pack-authorised")).toBeVisible();

  const missing = page.getByTestId("languages-missing");

  return {
    packSummary: (
      await authorisedRow.getByTestId("pack-supply-summary").innerText()
    ).trim(),
    authorised: (
      await authorisedRow.getByTestId("pack-authorised").innerText()
    ).trim(),
    accepts: (
      await page.getByTestId("market-stability-conditions").innerText()
    ).trim(),
    stabilityVerdict: (await authorisedRow
      .getByTestId("pack-stability-accepted")
      .count())
      ? "accepted"
      : (await authorisedRow.getByTestId("pack-stability-unaccepted").count())
        ? "unaccepted"
        : "unknown",
    languagesMissing: (await missing.count())
      ? (await missing.innerText()).trim()
      : null,
  };
}
