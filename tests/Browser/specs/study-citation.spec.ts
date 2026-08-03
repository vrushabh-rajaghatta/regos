import { expect } from "@playwright/test";

import {
  test,
  api,
  collectErrors,
  FDA_ORIGINAL_APPLICATION,
  FDA_SUBTYPE_APPLICATION,
} from "./support";

/**
 * **EPIC-019 S004 — which studies support a filing, and where a study is filed.**
 *
 * Driver A, the question this epic was originally scoped for: *"which studies
 * support this filing?"* and its inverse. The two are asserted **against each
 * other** rather than separately — a citation that shows on one screen and not
 * the other is the failure worth catching, and it is invisible to either
 * assertion alone.
 *
 * A citation is a claim the **application** makes, which is why the screen for
 * it is in the application's workspace. It is a different fact from the study a
 * *placement* reports (S002): an application can rest on a study it has not yet
 * filed a document for, and a sequence can report one the application never
 * cited. Both appear in the inverse view, labelled.
 */
const FDA_IND_APPLICATION_TYPE = "40000000-0000-0000-0000-000000000008";
const FDA = "20000000-0000-0000-0000-000000000001";
const UNITED_STATES = "10000000-0000-0000-0000-000000000001";

test.describe("Which studies support a filing", () => {
  test("cited from the application, and visible from the study", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();
    const studyCode = `CIT-${unique}`;

    const studyId = await registerStudy(studyCode, "A Study Worth Citing");

    const globalProductId = await createProduct(unique);
    const applicationId = await createApplication(globalProductId, unique);

    const workspace =
      `/regulatory/products/${globalProductId}/applications/${applicationId}`;

    // --- nothing cited yet -------------------------------------------------
    await page.goto(`${workspace}/studies`);

    await expect(page.getByTestId("application-studies-empty")).toBeVisible();

    // --- cite one, through the browser -------------------------------------
    // By value: an option's label is the code, title and kind together, and
    // Playwright matches labels as exact strings.
    await page.getByLabel("Cite a study").selectOption(studyId);

    await page.getByRole("button", { name: "Cite" }).click();

    const row = page.getByTestId("cited-study").filter({ hasText: studyCode });

    await expect(row).toBeVisible();
    await expect(row).toContainText("A Study Worth Citing");
    await expect(row).toContainText("Non-clinical");

    // Already cited, so it is no longer on offer — a duplicate click would be a
    // server-side no-op, which reads as the button being broken.
    await expect(
      page.getByLabel("Cite a study").getByRole("option", {
        name: new RegExp(`^${studyCode} `),
      }),
    ).toHaveCount(0);

    await page.screenshot({
      path: "test-results/study-citation.png",
      fullPage: true,
    });

    // --- and the study knows where it is filed -----------------------------
    //
    // The half that makes the citation visible from both ends. Asserted through
    // the registry rather than the API, because "visible from both ends" is a
    // claim about screens.
    await page.goto("/regulatory/studies");

    const studyRow = page
      .getByTestId("study-row")
      .filter({ hasText: studyCode });

    await studyRow.getByTestId("show-study-filings").click();

    const filing = studyRow.getByTestId("study-filing");

    await expect(filing).toHaveCount(1);
    await expect(filing).toContainText("Application");
    await expect(filing).toContainText(`Citation IND ${unique}`);

    // --- withdrawing it removes it from both ------------------------------
    await page.goto(`${workspace}/studies`);
    await page.getByTestId("stop-citing-study").click();

    await expect(page.getByTestId("application-studies-empty")).toBeVisible();

    await page.goto("/regulatory/studies");
    await studyRow.getByTestId("show-study-filings").click();

    await expect(
      studyRow.getByTestId("study-filings-empty"),
    ).toBeVisible();

    expect(errors()).toEqual([]);
  });

  test("a sequence that reports a study is a filing too", async () => {
    const unique = Date.now();
    const studyCode = `CIT2-${unique}`;

    const studyId = await registerStudy(studyCode, "A Reported Study");

    const globalProductId = await createProduct(unique);
    const applicationId = await createApplication(globalProductId, unique);

    const submission = await api(
      `/applications/${applicationId}/submissions`,
      {
        method: "POST",
        body: JSON.stringify({
          title: `Citation Sequence ${unique}`,
          submissionTypeId: FDA_ORIGINAL_APPLICATION,
          submissionSubTypeId: FDA_SUBTYPE_APPLICATION,
        }),
      },
    );

    expect(submission.ok, "creating the submission").toBeTruthy();

    // Nothing is placed, so nothing reports the study: the inverse view is
    // empty even though a submission exists. A sequence is a filing only when
    // one of its placements names the study.
    const before = await (
      await api(`/api/studies/${studyId}/filings`)
    ).json();

    expect(before).toEqual([]);

    // Citing at the application level makes it one filing, of the other kind.
    const cited = await api(`/api/applications/${applicationId}/studies`, {
      method: "POST",
      body: JSON.stringify({
        clinicalStudyId: null,
        nonClinicalStudyId: studyId,
      }),
    });

    expect(cited.ok, "citing the study").toBeTruthy();

    const after = await (await api(`/api/studies/${studyId}/filings`)).json();

    expect(after).toHaveLength(1);
    expect(after[0].kind).toBe("Application");
    expect(after[0].submissionId).toBeNull();

    // Idempotent: one claim stated twice is still one claim.
    const again = await api(`/api/applications/${applicationId}/studies`, {
      method: "POST",
      body: JSON.stringify({
        clinicalStudyId: null,
        nonClinicalStudyId: studyId,
      }),
    });

    expect(again.ok).toBeTruthy();

    const stillOne = await (
      await api(`/api/studies/${studyId}/filings`)
    ).json();

    expect(stillOne).toHaveLength(1);

    // Naming two studies at once is a caller bug, not something to resolve.
    const both = await api(`/api/applications/${applicationId}/studies`, {
      method: "POST",
      body: JSON.stringify({
        clinicalStudyId: studyId,
        nonClinicalStudyId: studyId,
      }),
    });

    expect(both.status, "citing two studies at once").toBe(409);
  });
});

// --- helpers ---------------------------------------------------------------

async function registerStudy(
  identifier: string,
  title: string,
): Promise<string> {
  const response = await api("/api/studies/nonclinical", {
    method: "POST",
    body: JSON.stringify({ sponsorStudyIdentifier: identifier, title }),
  });

  expect(response.ok, "registering the study").toBeTruthy();

  return (await response.json()).id;
}

async function createProduct(unique: number): Promise<string> {
  const response = await api("/api/products", {
    method: "POST",
    body: JSON.stringify({
      code: `BROWSER-CIT-${unique}-${Math.floor(Math.random() * 100000)}`,
      name: `Browser Citation Product ${unique}`,
      type: "Drug",
    }),
  });

  expect(response.ok, "creating the product").toBeTruthy();

  return (await response.json()).id;
}

async function createApplication(
  globalProductId: string,
  unique: number,
): Promise<string> {
  const organizations = await (await api("/api/organizations")).json();

  const applicant = organizations.find(
    (o: { status: string }) => o.status === "Active",
  );

  const response = await api(`/api/products/${globalProductId}/applications`, {
    method: "POST",
    body: JSON.stringify({
      countryId: UNITED_STATES,
      authorityId: FDA,
      applicationTypeId: FDA_IND_APPLICATION_TYPE,
      applicantOrganizationId: applicant.id,
      name: `Citation IND ${unique}`,
    }),
  });

  expect(response.ok, "creating the application").toBeTruthy();

  return (await response.json()).id;
}
