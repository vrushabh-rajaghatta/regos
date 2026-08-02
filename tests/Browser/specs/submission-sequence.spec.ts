import { expect } from "@playwright/test";

import { test, api, collectErrors, sessionCookies, API_URL } from "./support";

/**
 * **EPIC-004 S001 — a submission is a sequence.**
 *
 * The word `Submission` is the domain's; *"Sequence 0003"* is the screen's, and
 * both are binding (ADR-044 decision 3). This spec proves the pair, and the two
 * facts underneath it:
 *
 *   - the first sequence in an application is **0000**, and the next is 0001 —
 *     numbering is claimed at publish, so it follows filing order by
 *     construction;
 *   - a **draft has no number**, and says so in words that express an
 *     expectation rather than an identity.
 *
 * The dossier is filled through the API — that path has its own coverage in
 * submission-validation.spec.ts, and re-driving it here would be testing the
 * upload widget rather than numbering. Publishing is driven through the
 * browser, because that is the gesture that mints the number.
 */
const FDA_IND_CTD = "60000000-0000-0000-0000-000000000001";
const FDA_IND_SUBMISSION_TYPE = "40000000-0000-0000-0000-000000000008";
const FDA = "20000000-0000-0000-0000-000000000001";
const UNITED_STATES = "10000000-0000-0000-0000-000000000001";

const PDF = new TextEncoder().encode("%PDF-1.7\n% RegOS browser test\n");

type Requirement = {
  documentTypeId: string;
  sectionId: string;
  isMandatory: boolean;
};

test.describe("Submission sequence numbering", () => {
  test("0000, then 0001 — and a draft that has neither", async ({ page }) => {
    const errors = collectErrors(page);
    const unique = Date.now();

    // --- what this application's dossier owes -----------------------------
    const template = await (
      await api(`/reference-data/templates/${FDA_IND_CTD}`)
    ).json();

    const requirements: Requirement[] = template.versions[0].requiredDocuments
      .filter((d: Requirement) => d.isMandatory);

    const globalProductId = await createProduct(unique);
    const applicationId = await createApplication(globalProductId, unique);

    // One active document per requirement. Seeded once and reused: a Product
    // Document may appear only once *per submission*, so the same set fills
    // every sequence in this application — which is also how a real replace
    // works, and what S002 will diff.
    const documents = [];
    for (const requirement of requirements) {
      documents.push({
        requirement,
        documentId: await uploadActiveDocument(
          globalProductId,
          requirement.documentTypeId,
          unique,
        ),
      });
    }

    const workspace = (submissionId: string) =>
      `/regulatory/products/${globalProductId}/applications/${applicationId}` +
      `/submissions/${submissionId}`;

    // --- the first sequence in an application is 0000 ----------------------
    const first = await createSubmission(applicationId, `Original IND ${unique}`);
    await fillDossier(first, documents);

    // Before it is filed it has no number — only an expectation, and the
    // wording says so.
    await page.goto(workspace(first));
    await expect(
      page.getByText("Will publish as next sequence (currently 0000)"),
    ).toBeVisible();

    await publishThroughTheBrowser(page, workspace(first));

    await page.goto(workspace(first));
    await expect(page.getByText("Sequence 0000", { exact: true })).toBeVisible();
    // And the expectation is gone: it is a fact now, not a forecast.
    await expect(page.getByText("Will publish as next sequence")).toHaveCount(0);

    // --- the next one is 0001 ---------------------------------------------
    const second = await createSubmission(
      applicationId,
      `Protocol amendment ${unique}`,
    );
    await fillDossier(second, documents);

    await publishThroughTheBrowser(page, workspace(second));

    await page.goto(workspace(second));
    await expect(page.getByText("Sequence 0001", { exact: true })).toBeVisible();

    // --- a draft has no number, and predicts the one it would get ----------
    const draft = await createSubmission(
      applicationId,
      `Annual report ${unique}`,
    );

    await page.goto(workspace(draft));

    await expect(
      page.getByText("Will publish as next sequence (currently 0002)"),
    ).toBeVisible();
    await expect(page.getByText(/^Sequence \d{4}$/)).toHaveCount(0);

    // --- and the application's list shows what was filed, and what was not --
    await page.goto(
      `/regulatory/products/${globalProductId}/applications/${applicationId}` +
        `/submissions`,
    );

    await expect(page.getByText("Sequence 0000", { exact: true })).toBeVisible();
    await expect(page.getByText("Sequence 0001", { exact: true })).toBeVisible();
    // The draft is in the list and carries no number: the list reports filings,
    // not intentions.
    await expect(page.getByText(`Annual report ${unique}`)).toBeVisible();
    await expect(page.getByText(/^Sequence \d{4}$/)).toHaveCount(2);

    await page.screenshot({
      path: "test-results/submission-sequences.png",
      fullPage: true,
    });

    expect(errors()).toEqual([]);
  });
});

// --- helpers ---------------------------------------------------------------

async function publishThroughTheBrowser(
  page: import("@playwright/test").Page,
  workspace: string,
): Promise<void> {
  await page.goto(`${workspace}/publishing`);
  await page.getByTestId("publish-submission").click();
  await expect(page.getByTestId("submission-published")).toBeVisible();
}

/** Attaches and places every seeded document, so the blueprint is satisfied. */
async function fillDossier(
  submissionId: string,
  documents: { documentId: string; requirement: Requirement }[],
): Promise<void> {
  for (const { documentId, requirement } of documents) {
    const response = await api(`/submissions/${submissionId}/documents`, {
      method: "POST",
      body: JSON.stringify({
        productDocumentId: documentId,
        templateSectionId: requirement.sectionId,
      }),
    });

    expect(response.ok, `placing ${documentId}`).toBeTruthy();
  }
}

async function createProduct(unique: number): Promise<string> {
  const response = await api("/api/products", {
    method: "POST",
    body: JSON.stringify({
      code: `BROWSER-SEQ-${unique}`,
      name: `Browser Sequence Product ${unique}`,
      type: "Drug",
    }),
  });

  expect(response.ok, "creating the product").toBeTruthy();

  return (await response.json()).id;
}

/**
 * Its own application, deliberately. Sequence numbers are scoped to one, so a
 * spec that shared an application with another spec would be asserting against
 * a numbering space it does not control.
 */
async function createApplication(
  globalProductId: string,
  unique: number,
): Promise<string> {
  const organizations = await (await api("/api/organizations")).json();

  const applicant = organizations.find(
    (o: { status: string }) => o.status === "Active",
  );

  expect(applicant, "an active organization to apply as").toBeTruthy();

  const response = await api(`/api/products/${globalProductId}/applications`, {
    method: "POST",
    body: JSON.stringify({
      countryId: UNITED_STATES,
      authorityId: FDA,
      applicantOrganizationId: applicant.id,
      name: `Browser Sequence Application ${unique}`,
    }),
  });

  expect(response.ok, "creating the application").toBeTruthy();

  return (await response.json()).id;
}

async function createSubmission(
  applicationId: string,
  title: string,
): Promise<string> {
  const response = await api(`/applications/${applicationId}/submissions`, {
    method: "POST",
    body: JSON.stringify({
      submissionTypeId: FDA_IND_SUBMISSION_TYPE,
      title,
    }),
  });

  expect(response.ok, "creating the submission").toBeTruthy();

  return (await response.json()).id;
}

async function uploadActiveDocument(
  globalProductId: string,
  documentTypeId: string,
  unique: number,
): Promise<string> {
  const form = new FormData();
  form.append(
    "file",
    new Blob([PDF], { type: "application/pdf" }),
    `document-${documentTypeId}.pdf`,
  );
  form.append("documentTypeId", documentTypeId);
  form.append("name", `Browser Seq Doc ${documentTypeId} ${unique}`);

  const upload = await fetch(`${API_URL}/api/products/${globalProductId}/documents`, {
    method: "POST",
    body: form,
    headers: { Cookie: await sessionCookies() },
  });

  expect(upload.ok, `uploading a ${documentTypeId} document`).toBeTruthy();

  const documentId = (await upload.json()).id;

  const activate = await api(
    `/api/products/${globalProductId}/documents/${documentId}/activate`,
    { method: "POST" },
  );

  expect(activate.ok, "activating the document").toBeTruthy();

  return documentId;
}
