import { expect } from "@playwright/test";

import { test, api, collectErrors } from "./support";

/**
 * **EPIC-019 S001 — a study is two facts, and the identifier is the sponsor's.**
 *
 * The registry itself is unremarkable; what this spec is for is the one rule
 * that came from outside RegOS. **E24** records that FDA's review tooling
 * recognises a study by its `study-id`, and that a mismatch shows a reviewer two
 * studies where there is one. Read backwards, that is the rule here: two studies
 * sharing an identifier would be shown as one, and the STF carries no kind
 * marker to tell them apart.
 *
 * So the duplicate is refused **across both kinds** — a clinical study cannot
 * take the code a non-clinical one already holds — and the refusal names the
 * study already using it, because that is what tells a typo apart from a
 * genuine duplicate.
 */
const EXPECTED_CONFLICT = /the server responded with a status of 409/;

test.describe("study registry", () => {
  test("registers a study, and refuses a second one with the same ID", async ({
    page,
  }) => {
    const errors = collectErrors(page, [EXPECTED_CONFLICT]);
    const unique = Date.now();
    const identifier = `TOX-${unique}`;

    await page.goto("/regulatory/studies");

    // --- register a non-clinical study through the browser -----------------
    await page.getByTestId("register-study").click();

    await page.getByLabel("Study ID").fill(identifier);
    await page
      .getByLabel("Title")
      .fill("A 13-Week Oral Toxicity Study In Rats");

    await page.getByRole("button", { name: "Register study" }).last().click();

    const row = page
      .getByTestId("study-row")
      .filter({ hasText: identifier });

    await expect(row).toBeVisible();
    await expect(row).toContainText("Non-clinical");
    await expect(row).toContainText("A 13-Week Oral Toxicity Study In Rats");

    // --- the same ID, the other kind: still one study ----------------------
    //
    // Registering as Clinical proves the rule spans both aggregates. A unique
    // index could not have caught this: they are different tables.
    await page.getByTestId("register-study").click();

    await page.getByLabel("Kind").click();
    await page.getByRole("option", { name: /^Clinical/ }).click();

    await page.getByLabel("Study ID").fill(identifier);
    await page.getByLabel("Title").fill("A Different Study Entirely");

    await page.getByRole("button", { name: "Register study" }).last().click();

    const refusal = page.getByTestId("register-study-error");

    await expect(refusal).toBeVisible();
    await expect(refusal).toContainText(identifier);

    // Names the study holding it, so a duplicate is distinguishable from a typo.
    await expect(refusal).toContainText(
      "A 13-Week Oral Toxicity Study In Rats",
    );

    await expect(refusal).not.toContainText("Something went wrong");

    // The form keeps what was typed — the fix is usually one character.
    await expect(page.getByLabel("Title")).toHaveValue(
      "A Different Study Entirely",
    );

    await page.screenshot({
      path: "test-results/study-registry.png",
      fullPage: true,
    });

    expect(errors()).toEqual([]);
  });

  test("a study is the sponsor's, so it is trimmed to what the authority reads", async () => {
    const unique = Date.now();

    // E24 again, at the API rather than the form: " ABC-1 " and "ABC-1" are one
    // study to FDA, so they must be one study here. The aggregate trims, and
    // the policy compares what the aggregate produced.
    const created = await api("/api/studies/clinical", {
      method: "POST",
      body: JSON.stringify({
        sponsorStudyIdentifier: `  ABC-${unique}  `,
        title: "  A Study With Untidy Edges  ",
      }),
    });

    expect(created.ok, "registering with untidy edges").toBeTruthy();

    const studies = await (await api("/api/studies")).json();

    const stored = studies.find(
      (s: { sponsorStudyIdentifier: string }) =>
        s.sponsorStudyIdentifier === `ABC-${unique}`,
    );

    expect(stored, "stored under the trimmed identifier").toBeTruthy();
    expect(stored.title).toBe("A Study With Untidy Edges");

    const duplicate = await api("/api/studies/nonclinical", {
      method: "POST",
      body: JSON.stringify({
        sponsorStudyIdentifier: `ABC-${unique}`,
        title: "The Same Code, Untrimmed",
      }),
    });

    expect(
      duplicate.status,
      "the untrimmed original and this one are the same study",
    ).toBe(409);
  });
});
