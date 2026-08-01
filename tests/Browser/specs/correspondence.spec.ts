import { expect } from "@playwright/test";

import {
  test,
  api,
  collectErrors,
  sessionCookies,
  API_URL,
  EXPECTED_400,
} from "./support";

/**
 * **EPIC-006 S001 — a letter, filed where it belongs.**
 *
 * Proves the three verbs the story was designed around: a regulatory user can
 * **file** a letter, **find** it again, and **understand** it without opening
 * anything else.
 *
 * One thing is asserted as an absence, because it is a decision rather than an
 * omission (ADR-040): correspondence has **no status** — it is an event, not a
 * lifecycle.
 *
 * The division **was** an asserted absence in S001, and S001a deliberately
 * flips it. That is the tripwire working: the earlier assertion made adding a
 * division a decision rather than a commit, and the division that arrived is
 * the *authority's* (`AuthorityDivision`), never `OrganizationDivision`.
 */
const FDA_NAME = "Food and Drug Administration";

test.describe("Health-authority correspondence", () => {
  test("a letter is filed, found in the list, and understood on its own page", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();
    const subject = `Information request ${unique}`;

    // --- 1. the working surface exists and is reachable ------------------
    await page.goto("/regulatory/correspondence");

    // `exact` here is a narrowed selector, not a papered-over wording defect —
    // the guideline's step 4, so the reason is written down. The page heading
    // is "Correspondence" and the empty state is "No correspondence yet."; a
    // person reading them aloud is never confused, and only Playwright's
    // substring matching sees a collision.
    await expect(
      page.getByRole("heading", { name: "Correspondence", exact: true })
    ).toBeVisible();

    // --- 2. filing a letter that already happened ------------------------
    // Nothing defaults to today: the date on the letter is a fact about the
    // letter, and a mailbox carried into RegOS is mostly historic.
    await page.getByRole("button", { name: "Log correspondence" }).click();

    await expect(
      page.getByRole("heading", { name: "New correspondence" })
    ).toBeVisible();

    await page
      .getByLabel("Health authority")
      .selectOption({ label: FDA_NAME });
    await page.getByLabel("Received or sent").selectOption("Inbound");
    await page
      .getByLabel("Correspondence type")
      .selectOption({ label: "Information Request" });

    // The division list is scoped to the authority already chosen — the picker
    // cannot compose the refusal the server would give.
    await page
      .getByLabel("Division (optional)")
      .selectOption({ label: "Center for Drug Evaluation and Research" });

    await page.getByLabel("Subject").fill(subject);
    await page.getByLabel("Dated").fill("2019-06-14");
    await page.getByLabel("Response due (optional)").fill("2019-07-14");
    await page
      .getByLabel("Authority reference (optional)")
      .fill(`IND ${unique}`);

    await page.getByRole("button", { name: "Log" }).click();

    // --- 3. found in the list --------------------------------------------
    // The list, not the detail page: it only refreshes if the mutation
    // genuinely invalidated its cache (browser convention 3).
    const row = page.getByRole("link", { name: subject });
    await expect(row).toBeVisible();

    // --- 4. filtering is how the list is actually used --------------------
    await page.getByLabel("Filter by direction").selectOption("Outbound");
    await expect(page.getByRole("link", { name: subject })).toHaveCount(0);

    await page.getByLabel("Filter by direction").selectOption("Inbound");
    await expect(page.getByRole("link", { name: subject })).toBeVisible();

    // --- 5. understood on its own page ------------------------------------
    await page.getByRole("link", { name: subject }).click();

    await expect(page.getByRole("heading", { name: subject })).toBeVisible();
    await expect(page.getByText(`IND ${unique}`)).toBeVisible();

    // Both dates are shown: the letter is from 2019, the record is from today,
    // and a reader who cannot see both will eventually mistake one for the
    // other.
    await expect(page.getByText("2019-06-14")).toBeVisible();
    await expect(page.getByText("Recorded in RegOS")).toBeVisible();

    // The response is long overdue — proximity derived at the edge, stored
    // nowhere (ADR-037).
    await expect(page.getByText(/overdue by/)).toBeVisible();

    // An unfiled letter is a real letter. This one names nothing, and the page
    // says so rather than showing an empty field.
    await expect(
      page.getByText("Nothing — general correspondence")
    ).toBeVisible();

    // --- 6. the authority's division, and the one remaining absence -------
    await expect(
      page.getByText("Center for Drug Evaluation and Research")
    ).toBeVisible();

    // Still asserted, so adding a status stays a conversation rather than a
    // commit.
    await expect(page.getByText("Status")).toHaveCount(0);

    expect(errors()).toEqual([]);
  });

  test("the server's refusal is rendered, and nothing escapes to the window", async ({
    page,
  }) => {
    // Browser convention 5: every new mutation dialog is walked through at
    // least one real server refusal. Six forms once shipped rendering the
    // message correctly *and* letting the rejection escape unhandled.
    const errors = collectErrors(page, [EXPECTED_400]);

    await page.goto("/regulatory/correspondence");
    await page.getByRole("button", { name: "Log correspondence" }).click();

    await page.getByLabel("Health authority").selectOption({ label: FDA_NAME });
    await page.getByLabel("Received or sent").selectOption("Inbound");
    await page
      .getByLabel("Correspondence type")
      .selectOption({ label: "Information Request" });
    await page.getByLabel("Subject").fill("A refused letter");
    await page.getByLabel("Dated").fill("2026-03-01");

    // The business refusal available here: a response cannot be due before the
    // letter it answers. The rule is not mirrored client-side (SC-103), so this
    // reaches the server and renders its message verbatim.
    await page.getByLabel("Response due (optional)").fill("2026-02-01");
    await page.getByRole("button", { name: "Log" }).click();

    await expect(
      page.getByText("A response cannot be due before the correspondence itself.")
    ).toBeVisible();

    expect(errors()).toEqual([]);
  });

  test("the API refuses a response due before the letter", async () => {
    // The same rule at the boundary the UI cannot bypass, so the invariant is
    // proven to live in the domain rather than in the form.
    const authorities = await (await api("/master-data/authorities")).json();
    const types = await (
      await api("/api/master-data/correspondence-types")
    ).json();

    const response = await api("/api/correspondence", {
      method: "POST",
      body: JSON.stringify({
        authorityId: authorities[0].id,
        correspondenceTypeId: types[0].id,
        direction: "Inbound",
        subject: "Chronology check",
        occurredOn: "2026-03-01",
        responseDueOn: "2026-02-01",
      }),
    });

    expect(response.status).toBe(400);

    const problem = await response.json();
    expect(problem.detail).toContain("cannot be due before");
  });
});

test.describe("Authority divisions", () => {
  test("a division belongs to its authority, and the server says so", async () => {
    // The first genuinely semantic creation policy in the codebase: not "does
    // this row exist" but "does this child belong to the parent you chose".
    // A letter from the FDA cannot name a Health Canada directorate.
    const authorities = await (await api("/master-data/authorities")).json();
    const types = await (
      await api("/api/master-data/correspondence-types")
    ).json();

    const fda = authorities.find((a: { code: string }) => a.code === "FDA");
    const healthCanada = authorities.find(
      (a: { code: string }) => a.code === "HC"
    );

    const canadianDivisions = await (
      await api(`/api/master-data/authorities/${healthCanada.id}/divisions`)
    ).json();

    expect(canadianDivisions.length).toBeGreaterThan(0);

    const response = await api("/api/correspondence", {
      method: "POST",
      body: JSON.stringify({
        authorityId: fda.id,
        correspondenceTypeId: types[0].id,
        direction: "Inbound",
        subject: "Cross-authority division",
        occurredOn: "2026-03-01",
        authorityDivisionId: canadianDivisions[0].id,
      }),
    });

    // A valid request that business state forbids — ADR-012's 409.
    expect(response.status).toBe(409);

    const problem = await response.json();
    expect(problem.detail).toContain("does not belong to");
  });

  test("the divisions offered are scoped to the authority chosen", async () => {
    const authorities = await (await api("/master-data/authorities")).json();
    const fda = authorities.find((a: { code: string }) => a.code === "FDA");

    const divisions = await (
      await api(`/api/master-data/authorities/${fda.id}/divisions`)
    ).json();

    expect(divisions.length).toBeGreaterThan(0);
    expect(
      divisions.every((d: { authorityId: string }) => d.authorityId === fda.id)
    ).toBe(true);
  });
});

test.describe("Correspondence content", () => {
  test("content is attached, downloaded under its own name, removed — and the letter survives", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();
    const subject = `Deficiency letter ${unique}`;
    const fileName = `FDA-DL-${unique}.txt`;

    // Deliberately asymmetric, and the point is the second half. Attaching and
    // downloading is CRUD; what matters is that REMOVING the content leaves the
    // correspondence intact — the record is the letter, the file is its content.
    const correspondenceId = await recordLetter(subject);
    await page.goto(`/regulatory/correspondence/${correspondenceId}`);

    await expect(page.getByTestId("correspondence-content-empty")).toBeVisible();

    // --- attach ----------------------------------------------------------
    await page
      .getByLabel("Choose a file to attach")
      .setInputFiles({
        name: fileName,
        mimeType: "text/plain",
        buffer: Buffer.from("The agency requires further information."),
      });

    await expect(page.getByRole("link", { name: fileName })).toBeVisible();

    // --- download, under the name it arrived with -------------------------
    const download = await Promise.all([
      page.waitForEvent("download"),
      page.getByRole("link", { name: fileName }).click(),
    ]).then(([d]) => d);

    expect(download.suggestedFilename()).toBe(fileName);

    // --- remove, and the letter is untouched -----------------------------
    await page.getByRole("button", { name: "Remove" }).click();

    await expect(page.getByTestId("correspondence-content-empty")).toBeVisible();

    // The business record survives its content being wrong and corrected.
    await expect(page.getByRole("heading", { name: subject })).toBeVisible();
    await expect(page.getByText("Not stated")).toBeVisible();

    // --- and content can be attached again -------------------------------
    await page
      .getByLabel("Choose a file to attach")
      .setInputFiles({
        name: `replacement-${unique}.txt`,
        mimeType: "text/plain",
        buffer: Buffer.from("The corrected letter."),
      });

    await expect(
      page.getByRole("link", { name: `replacement-${unique}.txt` })
    ).toBeVisible();

    expect(errors()).toEqual([]);
  });

  test("an attachment id from another letter cannot delete this one's file", async () => {
    // The aggregate owns the check, so an id that exists but belongs elsewhere
    // is a 404 rather than a deletion.
    const first = await recordLetter(`First ${Date.now()}`);
    const second = await recordLetter(`Second ${Date.now()}`);

    const attachmentId = await attachFile(first, "first.txt");

    const response = await api(
      `/api/correspondence/${second}/content/${attachmentId}`,
      { method: "DELETE" }
    );

    expect(response.status).toBe(404);
  });
});

test.describe("Questions inside a letter", () => {
  test("a question is raised, answered, accepted — and its history reads back on the page", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();
    const subject = `Information request ${unique}`;

    const correspondenceId = await recordLetter(subject);
    await page.goto(`/regulatory/correspondence/${correspondenceId}`);

    await expect(
      page.getByTestId("correspondence-questions-empty")
    ).toBeVisible();

    // --- raise ------------------------------------------------------------
    await page.getByRole("button", { name: "Raise question" }).click();
    await page.getByLabel("Number, as the letter gives it").fill("3a");
    await page
      .getByLabel("What they asked")
      .fill(`Justify the stability data ${unique}`);
    await page.getByLabel("Our target response date (optional)").fill("2026-04-15");
    await page.getByRole("button", { name: "Raise" }).click();

    await expect(page.getByTestId("correspondence-question")).toHaveCount(1);
    await expect(page.getByText("target 2026-04-15")).toBeVisible();

    // --- answer -----------------------------------------------------------
    await page.getByLabel("Our answer").fill("See section 3.2.P.5.");
    await page.getByLabel("Sent on").fill("2026-04-10");
    await page.getByRole("button", { name: "Record answer" }).click();

    await expect(page.getByText("See section 3.2.P.5.")).toBeVisible();
    await expect(page.getByText("answered 2026-04-10")).toBeVisible();

    // --- accepted ---------------------------------------------------------
    // Weeks later, and the gap is the point: Responded is us, Resolved is
    // them. Collapsing the two would lose exactly the period a regulatory team
    // is anxious about.
    await page.getByLabel("Accepted on").fill("2026-05-30");
    await page.getByRole("button", { name: "Mark resolved" }).click();

    // --- the history is READ BACK on the same page ------------------------
    // testing.md principle 8. EPIC-017 S003 shipped a history that was written
    // correctly and readable nowhere; this is the story that would repeat it.
    const history = page.getByTestId("question-history");
    await expect(history).toBeVisible();
    await expect(history.getByText("Open")).toBeVisible();
    await expect(history.getByText("Responded")).toBeVisible();
    await expect(history.getByText("Resolved")).toBeVisible();

    expect(errors()).toEqual([]);
  });

  test("the API refuses a history that goes backwards", async () => {
    const correspondenceId = await recordLetter(`Chronology ${Date.now()}`);

    const raised = await api(
      `/api/correspondence/${correspondenceId}/questions`,
      {
        method: "POST",
        body: JSON.stringify({ number: "1", text: "Clarify." }),
      }
    );

    expect(raised.status).toBe(201);
    const questionId = (await raised.json()).id;

    // The letter is dated 2026-03-01; answering it in February is not a late
    // entry, it is a wrong one.
    const response = await api(
      `/api/correspondence/${correspondenceId}/questions/${questionId}/response`,
      {
        method: "POST",
        body: JSON.stringify({
          responseText: "Answer",
          occurredOn: "2026-02-01",
        }),
      }
    );

    expect(response.status).toBe(400);

    const problem = await response.json();
    expect(problem.detail).toContain("cannot go backwards");
  });
});

async function recordLetter(subject: string): Promise<string> {
  const authorities = await (await api("/master-data/authorities")).json();
  const types = await (
    await api("/api/master-data/correspondence-types")
  ).json();

  const response = await api("/api/correspondence", {
    method: "POST",
    body: JSON.stringify({
      authorityId: authorities[0].id,
      correspondenceTypeId: types[0].id,
      direction: "Inbound",
      subject,
      occurredOn: "2026-03-01",
    }),
  });

  expect(response.status).toBe(201);

  return (await response.json()).id;
}

async function attachFile(
  correspondenceId: string,
  fileName: string
): Promise<string> {
  const form = new FormData();
  form.append("file", new Blob(["content"], { type: "text/plain" }), fileName);

  // Not the shared `api()` helper: it forces Content-Type: application/json,
  // and multipart needs fetch to set its own boundary.
  const response = await fetch(
    `${API_URL}/api/correspondence/${correspondenceId}/content`,
    { method: "POST", body: form, headers: { Cookie: await sessionCookies() } }
  );

  expect(response.status).toBe(201);

  return (await response.json()).id;
}
