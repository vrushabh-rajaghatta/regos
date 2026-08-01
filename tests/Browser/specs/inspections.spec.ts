import { expect } from "@playwright/test";

import { test, api, collectErrors } from "./support";

/**
 * **EPIC-006 S006 — an inspection, and the entity it does not need.**
 *
 * The story tests whether the epic's established patterns hold rather than
 * inventing new ones, and in every place they do. Two negatives carry it:
 *
 * - **there is no observation entity** — a Form 483 observation looks like a
 *   question and is a different kind of thing. A question asks for information
 *   and answering it *is* the work; an observation asserts a deficiency and
 *   responding to it *creates* work, which is a `Commitment` that already
 *   exists;
 * - **an inspection concludes**, like a meeting, so it leaves the list of what
 *   is coming.
 */
test.describe("Authority inspections", () => {
  test("announced, conducted, completed — and the site is what was inspected", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();
    const title = `Pre-approval inspection ${unique}`;

    await page.goto("/regulatory/inspections");
    await expect(
      page.getByRole("heading", { name: "Inspections", exact: true })
    ).toBeVisible();

    await page.getByRole("button", { name: "Record inspection" }).click();
    await page
      .getByLabel("Health authority")
      .selectOption({ label: "Food and Drug Administration" });
    await page.getByLabel("How we learned of it").selectOption("Announced");
    await page.getByLabel("What it is").fill(title);
    await page.getByLabel("Learned on").fill("2026-01-15");
    await page.getByLabel("Scheduled for (optional)").fill("2026-03-02");
    await page.getByRole("button", { name: "Record" }).click();

    const card = page.getByTestId("inspection-card").filter({ hasText: title });
    await expect(card).toBeVisible();

    // "The FDA will inspect us in March" arrives before "at Plant A".
    await expect(card.getByText("Site inspected: Not yet known")).toBeVisible();

    await card.getByLabel("On").fill("2026-03-02");
    await card.getByRole("button", { name: "InProgress" }).click();

    await card.getByLabel("On").fill("2026-03-06");
    await card.getByRole("button", { name: "Completed" }).click();

    // It concludes, like a meeting — the same family, and the same disappearance.
    await expect(card).toHaveCount(0);

    await page.getByRole("button", { name: "Show concluded too" }).click();
    await expect(
      page
        .getByTestId("inspection-card")
        .filter({ hasText: title })
        .getByText("completed 2026-03-06")
    ).toBeVisible();

    expect(errors()).toEqual([]);
  });

  test("a finding obliges a commitment, not an observation record", async () => {
    const authorities = await (await api("/master-data/authorities")).json();

    const created = await api("/api/inspections", {
      method: "POST",
      body: JSON.stringify({
        authorityId: authorities[0].id,
        title: `Findings ${Date.now()}`,
        initialStatus: "InProgress",
        occurredOn: "2026-03-02",
      }),
    });
    expect(created.status).toBe(201);
    const inspectionId = (await created.json()).id;

    // Findings before it finishes are a guess.
    const early = await api(`/api/inspections/${inspectionId}/findings`, {
      method: "PUT",
      body: JSON.stringify({ findings: "Three observations." }),
    });
    expect(early.status).toBe(409);

    await api(`/api/inspections/${inspectionId}/status`, {
      method: "POST",
      body: JSON.stringify({ status: "Completed", occurredOn: "2026-03-06" }),
    });

    const findings = await api(`/api/inspections/${inspectionId}/findings`, {
      method: "PUT",
      body: JSON.stringify({
        findings: "Form 483 issued with three observations.",
      }),
    });
    expect(findings.status).toBe(204);

    // What the findings OBLIGE is a commitment — the third independent
    // business origin, and the one the fourth would reopen Phase 2 over.
    const corrective = await api("/api/commitments", {
      method: "POST",
      body: JSON.stringify({
        authorityId: authorities[0].id,
        title: `CAPA for observation 1 (${inspectionId.slice(0, 8)})`,
        givenOn: "2026-03-20",
        dueOn: "2026-06-30",
        sourceInspectionId: inspectionId,
      }),
    });
    expect(corrective.status).toBe(201);

    // And it shows up as work, which is the whole point of modelling it there.
    const due = await (await api("/api/due-work")).json();
    expect(
      due.some(
        (item: { kind: string; title: string }) =>
          item.kind === "Commitment" && item.title.startsWith("CAPA for observation 1")
      )
    ).toBe(true);
  });

  test("an unannounced inspection begins in progress, not announced", async () => {
    const authorities = await (await api("/master-data/authorities")).json();

    const created = await api("/api/inspections", {
      method: "POST",
      body: JSON.stringify({
        authorityId: authorities[0].id,
        title: `Unannounced ${Date.now()}`,
        initialStatus: "InProgress",
        occurredOn: "2026-03-02",
      }),
    });
    expect(created.status).toBe(201);

    const inspections = await (await api("/api/inspections")).json();
    const surprise = inspections.find(
      (i: { title: string }) => i.title.startsWith("Unannounced")
    );

    // Forcing it through Announced would put a notice in the history that was
    // never given.
    expect(surprise.history).toHaveLength(1);
    expect(surprise.history[0].status).toBe("InProgress");
  });
});
