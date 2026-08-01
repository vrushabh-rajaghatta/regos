import { expect } from "@playwright/test";

import { test, api, collectErrors } from "./support";

/**
 * **EPIC-006 S005 — the only lifecycle an authority controls.**
 *
 * Every other status graph in this context records our own operational
 * progression. A meeting's contains a fork *they* choose — granted or
 * declined — which is the entire reason it is the one object here with a
 * transition table.
 *
 * The spec proves the table by what it refuses, not by what it allows.
 */
test.describe("Meetings with an authority", () => {
  test("requested, granted, held — and what the authority concluded", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();
    const subject = `Type B meeting ${unique}`;

    await page.goto("/regulatory/meetings");
    await expect(page.getByRole("heading", { name: "Meetings", exact: true }))
      .toBeVisible();

    await page.getByRole("button", { name: "Record meeting" }).click();
    await expect(
      page.getByRole("heading", { name: "A meeting with an authority" })
    ).toBeVisible();

    await page
      .getByLabel("Health authority")
      .selectOption({ label: "Food and Drug Administration" });
    await page.getByLabel("Who asked for it").selectOption("Requested");
    await page.getByLabel("Subject").fill(subject);
    await page.getByLabel("Raised on").fill("2026-02-01");
    await page.getByLabel("Scheduled for (optional)").fill("2026-03-05");
    await page.getByRole("button", { name: "Record" }).click();

    const card = page.getByTestId("meeting-card").filter({ hasText: subject });
    await expect(card).toBeVisible();

    // The authority's decision, then the meeting itself.
    await card.getByLabel("On").fill("2026-02-10");
    await card.getByRole("button", { name: "Granted" }).click();

    await expect(card.getByTestId("meeting-history").getByText("Granted"))
      .toBeVisible();

    await card.getByLabel("On").fill("2026-03-05");
    await card.getByRole("button", { name: "Held" }).click();

    // It concludes, and leaves a list that answers "what is coming?" — the
    // same framing that makes a decomposed letter stop being work. A meeting's
    // value is the work it produced, not a continuing lifecycle.
    await expect(card).toHaveCount(0);

    await page.getByRole("button", { name: "Show concluded too" }).click();

    const concluded = page
      .getByTestId("meeting-card")
      .filter({ hasText: subject });

    await expect(concluded.getByText("held 2026-03-05")).toBeVisible();
    await expect(concluded.getByTestId("meeting-history").getByText("Held"))
      .toBeVisible();

    expect(errors()).toEqual([]);
  });

  test("the table refuses what the authority has not decided", async () => {
    const authorities = await (await api("/master-data/authorities")).json();

    const created = await api("/api/meetings", {
      method: "POST",
      body: JSON.stringify({
        authorityId: authorities[0].id,
        subject: `Transition guard ${Date.now()}`,
        initialStatus: "Requested",
        occurredOn: "2026-02-01",
      }),
    });
    expect(created.status).toBe(201);
    const meetingId = (await created.json()).id;

    // A meeting cannot be held before it is granted. This is the one place in
    // EPIC-006 where the graph itself is a rule, because the branch is theirs.
    const heldTooEarly = await api(`/api/meetings/${meetingId}/status`, {
      method: "POST",
      body: JSON.stringify({ status: "Held", occurredOn: "2026-03-01" }),
    });
    expect(heldTooEarly.status).toBe(409);

    const declined = await api(`/api/meetings/${meetingId}/status`, {
      method: "POST",
      body: JSON.stringify({ status: "Declined", occurredOn: "2026-02-10" }),
    });
    expect(declined.status).toBe(204);

    // Declined is terminal: a second meeting is a second meeting.
    const revived = await api(`/api/meetings/${meetingId}/status`, {
      method: "POST",
      body: JSON.stringify({ status: "Granted", occurredOn: "2026-02-11" }),
    });
    expect(revived.status).toBe(409);
  });

  test("a meeting the authority called begins granted, not requested", async () => {
    const authorities = await (await api("/master-data/authorities")).json();

    const created = await api("/api/meetings", {
      method: "POST",
      body: JSON.stringify({
        authorityId: authorities[0].id,
        subject: `Summoned ${Date.now()}`,
        initialStatus: "Granted",
        occurredOn: "2026-02-01",
      }),
    });
    expect(created.status).toBe(201);

    const meetings = await (await api("/api/meetings")).json();
    const summoned = meetings.find(
      (m: { subject: string }) => m.subject.startsWith("Summoned")
    );

    // One entry, and it says Granted. Recording it as "requested then granted"
    // would put a request in the history that never happened.
    expect(summoned.history).toHaveLength(1);
    expect(summoned.history[0].status).toBe("Granted");
  });

  test("an outcome cannot be recorded for a meeting that has not happened", async () => {
    const authorities = await (await api("/master-data/authorities")).json();

    const created = await api("/api/meetings", {
      method: "POST",
      body: JSON.stringify({
        authorityId: authorities[0].id,
        subject: `Premature minutes ${Date.now()}`,
        initialStatus: "Granted",
        occurredOn: "2026-02-01",
      }),
    });
    const meetingId = (await created.json()).id;

    // Minutes of a meeting that has not taken place are a plan.
    const early = await api(`/api/meetings/${meetingId}/outcome`, {
      method: "PUT",
      body: JSON.stringify({ minutes: "Notes", outcome: "Agreed" }),
    });
    expect(early.status).toBe(409);
  });
});
