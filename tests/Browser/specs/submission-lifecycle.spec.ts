import { expect } from "@playwright/test";

import { test, api, collectErrors, sessionCookies, API_URL } from "./support";

/**
 * **EPIC-004 S003 — two lifecycles, and only one of them belongs to the
 * submission.**
 *
 * A submission's own history records what *we* did: it became a draft, then it
 * was published. What the *authority* did is not a status on it — an
 * acknowledgement is a letter, and letters live in the Interaction context
 * (ADR-046).
 *
 * The page proves the separation by showing both without either context knowing
 * about the other: two projections, composed at the edge.
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

test.describe("A submission's lifecycle, and the authority's", () => {
  test("draft, then published — and the acknowledgement is a letter", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();

    const template = await (
      await api(`/reference-data/templates/${FDA_IND_CTD}`)
    ).json();

    const requirements: Requirement[] = template.versions[0].requiredDocuments
      .filter((d: Requirement) => d.isMandatory);

    const globalProductId = await createProduct(unique);
    const applicationId = await createApplication(globalProductId, unique);
    const submissionId = await createSubmission(
      applicationId,
      `Original IND ${unique}`,
    );

    const history =
      `/regulatory/products/${globalProductId}/applications/${applicationId}` +
      `/submissions/${submissionId}/history`;

    // --- a draft already has a history -------------------------------------
    // It begins at creation, not at publication: becoming a draft is a step.
    await page.goto(history);

    const steps = page.getByTestId("submission-status-step");
    await expect(steps).toHaveCount(1);
    await expect(steps).toHaveAttribute("data-status", "Draft");

    // And the authority has said nothing, because nothing has been filed.
    await expect(page.getByTestId("no-authority-response")).toBeVisible();

    // --- publishing appends, it does not replace ---------------------------
    for (const requirement of requirements) {
      const documentId = await uploadActiveDocument(
        globalProductId,
        requirement.documentTypeId,
        unique,
      );

      const placed = await api(`/submissions/${submissionId}/documents`, {
        method: "POST",
        body: JSON.stringify({
          productDocumentId: documentId,
          templateSectionId: requirement.sectionId,
        }),
      });

      expect(placed.ok, "placing a required document").toBeTruthy();
    }

    await page.goto(
      `/regulatory/products/${globalProductId}/applications/${applicationId}` +
        `/submissions/${submissionId}/publishing`,
    );
    await page.getByTestId("publish-submission").click();
    await expect(page.getByTestId("submission-published")).toBeVisible();

    await page.goto(history);

    await expect(page.getByTestId("submission-status-step")).toHaveCount(2);
    await expect(
      page.locator('[data-testid="submission-status-step"][data-status="Draft"]'),
    ).toHaveCount(1);
    await expect(
      page.locator(
        '[data-testid="submission-status-step"][data-status="Published"]',
      ),
    ).toHaveCount(1);

    // --- the authority acknowledges, and it is a letter --------------------
    // Not a status transition on the submission. A piece of correspondence,
    // anchored to this sequence, recorded in a different bounded context — and
    // the page composes the two without either knowing about the other.
    const types = await (await api("/api/master-data/correspondence-types")).json();

    const acknowledgement = await api("/api/correspondence", {
      method: "POST",
      body: JSON.stringify({
        authorityId: FDA,
        correspondenceTypeId: types[0].id,
        direction: "Inbound",
        subject: `Acknowledgement of sequence 0000 (${unique})`,
        occurredOn: "2026-08-05",
        submissionId,
      }),
    });

    expect(acknowledgement.ok, "recording the acknowledgement").toBeTruthy();

    await page.goto(history);

    await expect(page.getByTestId("authority-response")).toHaveCount(1);
    await expect(page.getByTestId("authority-response")).toContainText(
      `Acknowledgement of sequence 0000 (${unique})`,
    );

    // The submission's own history is unchanged by it — which is the whole
    // point. Nothing the authority does is a step in our lifecycle.
    await expect(page.getByTestId("submission-status-step")).toHaveCount(2);

    await page.screenshot({
      path: "test-results/submission-lifecycle.png",
      fullPage: true,
    });

    expect(errors()).toEqual([]);
  });
});

// --- helpers ---------------------------------------------------------------

async function createProduct(unique: number): Promise<string> {
  const response = await api("/api/products", {
    method: "POST",
    body: JSON.stringify({
      code: `BROWSER-LIFE-${unique}`,
      name: `Browser Lifecycle Product ${unique}`,
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

  expect(applicant, "an active organization to apply as").toBeTruthy();

  const response = await api(`/api/products/${globalProductId}/applications`, {
    method: "POST",
    body: JSON.stringify({
      countryId: UNITED_STATES,
      authorityId: FDA,
      applicantOrganizationId: applicant.id,
      name: `Browser Lifecycle Application ${unique}`,
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
  form.append("name", `Browser Life Doc ${documentTypeId} ${unique}`);

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
