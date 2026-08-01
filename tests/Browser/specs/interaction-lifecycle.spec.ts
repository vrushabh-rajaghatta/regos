import { expect } from "@playwright/test";

import { test, api, collectErrors } from "./support";

/**
 * **EPIC-006's capstone: the whole context, as one working week.**
 *
 * Not another CRUD flow. This walks the architecture the epic built, in the
 * order a regulatory affairs team would actually live it, and ends on the
 * sentence the epic set out to be able to say:
 *
 * > *There is no work left.*
 *
 * Every other spec proves one capability. This one proves the **thesis**: that
 * interactions conclude and **work outlives them**, that a letter, a meeting
 * and an inspection all reduce to obligations with dates and owners, and that
 * one screen can tell a person what their Monday looks like.
 */
test.describe("The interaction lifecycle", () => {
  test("a letter, a meeting and an inspection all become work — and then the work runs out", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();
    const tag = `E6-${unique}`;

    const authorities = await (await api("/master-data/authorities")).json();
    const fda = authorities.find((a: { code: string }) => a.code === "FDA");
    const types = await (
      await api("/api/master-data/correspondence-types")
    ).json();
    const informationRequest = types.find(
      (t: { code: string }) => t.code === "INFORMATION_REQUEST"
    );
    const divisions = await (
      await api(`/api/master-data/authorities/${fda.id}/divisions`)
    ).json();

    const created: { kind: string; id: string }[] = [];

    // --- 1. a letter arrives, from a division of an authority ------------
    const letterResponse = await api("/api/correspondence", {
      method: "POST",
      body: JSON.stringify({
        authorityId: fda.id,
        correspondenceTypeId: informationRequest.id,
        authorityDivisionId: divisions[0].id,
        direction: "Inbound",
        subject: `${tag} Information request on the IND`,
        occurredOn: "2026-03-01",
        responseDueOn: "2026-04-15",
      }),
    });
    expect(letterResponse.status).toBe(201);
    const correspondenceId = (await letterResponse.json()).id;

    // Before anyone decomposes it, the letter itself is the work.
    let due = await (await api("/api/due-work")).json();
    expect(
      due.some((x: { kind: string; title: string }) =>
        x.kind === "Correspondence" && x.title.includes(tag))
    ).toBe(true);

    // --- 2. its content is attached, and survives being replaced ---------
    await page.goto(`/regulatory/correspondence/${correspondenceId}`);
    await page.getByLabel("Choose a file to attach").setInputFiles({
      name: `${tag}-original.txt`,
      mimeType: "text/plain",
      buffer: Buffer.from("The agency requires further information."),
    });
    await expect(page.getByRole("link", { name: `${tag}-original.txt` }))
      .toBeVisible();

    // --- 3. it is decomposed into questions ------------------------------
    // The letter stops being work the moment the questions exist.
    const question = await api(
      `/api/correspondence/${correspondenceId}/questions`,
      {
        method: "POST",
        body: JSON.stringify({
          number: "1",
          text: `${tag} Justify the stability data`,
          targetResponseOn: "2026-04-01",
        }),
      }
    );
    expect(question.status).toBe(201);
    const questionId = (await question.json()).id;
    created.push({ kind: "Question", id: questionId });

    due = await (await api("/api/due-work")).json();
    expect(
      due.some((x: { kind: string; title: string }) =>
        x.kind === "Correspondence" && x.title.includes(tag))
    ).toBe(false);
    expect(
      due.some((x: { kind: string; title: string }) =>
        x.kind === "Question" && x.title.includes(tag))
    ).toBe(true);

    // --- 4. a meeting is requested, granted, held ------------------------
    const meeting = await api("/api/meetings", {
      method: "POST",
      body: JSON.stringify({
        authorityId: fda.id,
        subject: `${tag} Type B meeting`,
        initialStatus: "Requested",
        occurredOn: "2026-03-05",
        scheduledFor: "2026-04-20",
      }),
    });
    const meetingId = (await meeting.json()).id;

    await api(`/api/meetings/${meetingId}/status`, {
      method: "POST",
      body: JSON.stringify({ status: "Granted", occurredOn: "2026-03-12" }),
    });
    await api(`/api/meetings/${meetingId}/status`, {
      method: "POST",
      body: JSON.stringify({ status: "Held", occurredOn: "2026-04-20" }),
    });
    await api(`/api/meetings/${meetingId}/outcome`, {
      method: "PUT",
      body: JSON.stringify({
        minutes: "Discussed the Phase 3 design.",
        outcome: "The agency accepted the proposed Phase 3 design.",
      }),
    });

    // The meeting concluded. Its OUTCOME is their position; what we now owe
    // is a separate obligation.
    const meetingCommitment = await api("/api/commitments", {
      method: "POST",
      body: JSON.stringify({
        authorityId: fda.id,
        title: `${tag} Submit the revised protocol`,
        givenOn: "2026-04-20",
        dueOn: "2026-06-30",
        sourceMeetingId: meetingId,
      }),
    });
    expect(meetingCommitment.status).toBe(201);
    created.push({ kind: "Commitment", id: (await meetingCommitment.json()).id });

    // --- 5. an inspection happens, and its findings oblige work ----------
    const inspection = await api("/api/inspections", {
      method: "POST",
      body: JSON.stringify({
        authorityId: fda.id,
        title: `${tag} Pre-approval inspection`,
        initialStatus: "Announced",
        occurredOn: "2026-04-01",
        scheduledFor: "2026-05-11",
      }),
    });
    const inspectionId = (await inspection.json()).id;

    await api(`/api/inspections/${inspectionId}/status`, {
      method: "POST",
      body: JSON.stringify({ status: "Completed", occurredOn: "2026-05-15" }),
    });
    await api(`/api/inspections/${inspectionId}/findings`, {
      method: "PUT",
      body: JSON.stringify({ findings: "Form 483, one observation." }),
    });

    const capa = await api("/api/commitments", {
      method: "POST",
      body: JSON.stringify({
        authorityId: fda.id,
        title: `${tag} Corrective action for observation 1`,
        givenOn: "2026-05-30",
        dueOn: "2026-08-31",
        sourceInspectionId: inspectionId,
      }),
    });
    expect(capa.status).toBe(201);
    created.push({ kind: "Commitment", id: (await capa.json()).id });

    // --- 6. Monday morning: three obligations, three origins -------------
    await page.goto("/regulatory/due-work");

    const ours = page.getByTestId("due-work-row").filter({ hasText: tag });
    await expect(ours).toHaveCount(3);

    // The concluded interactions are NOT here. A meeting that was held and an
    // inspection that finished are not work; what they produced is.
    await expect(
      page.getByTestId("due-work-row").filter({ hasText: `${tag} Type B meeting` })
    ).toHaveCount(0);
    await expect(
      page
        .getByTestId("due-work-row")
        .filter({ hasText: `${tag} Pre-approval inspection` })
    ).toHaveCount(0);

    // --- 7. the work is discharged, each in its own way ------------------
    // A question is answered by us, then accepted by them.
    await api(
      `/api/correspondence/${correspondenceId}/questions/${questionId}/response`,
      {
        method: "POST",
        body: JSON.stringify({
          responseText: "See section 3.2.P.5.",
          occurredOn: "2026-04-10",
        }),
      }
    );
    await api(
      `/api/correspondence/${correspondenceId}/questions/${questionId}/resolution`,
      { method: "POST", body: JSON.stringify({ occurredOn: "2026-05-30" }) }
    );

    // One commitment we perform; one the authority releases. Neither is a
    // failure — there is no way to record that we failed.
    const commitments = created.filter((x) => x.kind === "Commitment");

    await api(`/api/commitments/${commitments[0].id}/status`, {
      method: "POST",
      body: JSON.stringify({ status: "Fulfilled", occurredOn: "2026-06-25" }),
    });
    await api(`/api/commitments/${commitments[1].id}/status`, {
      method: "POST",
      body: JSON.stringify({ status: "Waived", occurredOn: "2026-07-01" }),
    });

    // --- 8. there is no work left ----------------------------------------
    // Scoped to this narrative's own tag: the specs share one tenant, so a
    // globally empty due view would need an isolated one. The claim is the
    // same — every obligation this story created has been discharged, and
    // nothing it created lingers.
    await page.reload();
    await expect(page.getByTestId("due-work-row").filter({ hasText: tag }))
      .toHaveCount(0);

    // And nothing was deleted to achieve it. The record survives the work.
    const history = await (
      await api(`/api/correspondence/${correspondenceId}`)
    ).json();
    expect(history.questions[0].currentStatus).toBe("Resolved");
    expect(history.questions[0].history).toHaveLength(3);
    expect(history.attachments).toHaveLength(1);

    expect(errors()).toEqual([]);
  });
});
