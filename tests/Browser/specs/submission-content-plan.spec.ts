import { expect } from "@playwright/test";

import { test, api, collectErrors, sessionCookies, API_URL } from "./support";

/**
 * The dossier builder: the blueprint's structure as a working surface.
 *
 * Its subject is the content plan itself — that the tree comes from the bound
 * blueprint, that a placeholder's state is what the server says it is, and that
 * placing through the UI changes it. What that means for publishing is the
 * validation spec's subject, not this one's.
 *
 * Prerequisites are created through the API, per the rule that a spec owns the
 * data it mutates (ADR-019). The placement itself is driven through the browser,
 * because that is what this story added.
 */
const FDA_IND_CTD = "60000000-0000-0000-0000-000000000001";
const FDA_IND_SUBMISSION_TYPE = "40000000-0000-0000-0000-000000000008";
const FDA = "20000000-0000-0000-0000-000000000001";
const UNITED_STATES = "10000000-0000-0000-0000-000000000001";

const PDF = new TextEncoder().encode("%PDF-1.7\n% RegOS content plan test\n");

type Requirement = {
  documentTypeId: string;
  sectionId: string;
  isMandatory: boolean;
};

test.describe("Submission content plan", () => {
  test("renders the blueprint's structure and fills a placeholder", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();

    const template = await (
      await api(`/reference-data/templates/${FDA_IND_CTD}`)
    ).json();

    const version = template.versions[0];
    const requirements: Requirement[] = version.requiredDocuments.filter(
      (d: Requirement) => d.isMandatory,
    );

    const globalProductId = await createProduct(unique);
    const applicationId = await createApplication(globalProductId);
    const submissionId = await createSubmission(applicationId, unique);

    const contentPlanUrl =
      `/regulatory/products/${globalProductId}/applications/${applicationId}` +
      `/submissions/${submissionId}/content-plan`;

    // --- 1. an empty dossier still has a shape ----------------------------
    await page.goto(contentPlanUrl);

    await expect(page.getByTestId("content-plan-progress")).toContainText(
      `0 of ${requirements.length} placeholders filled`,
    );

    // The whole blueprint, not only the sections that expect something: an
    // empty section is part of the dossier's structure.
    expect(
      await page.getByTestId("content-plan-section").count(),
    ).toBe(version.sections.length);

    const placeholders = page.getByTestId("content-plan-placeholder");
    await expect(placeholders).toHaveCount(requirements.length);
    expect(
      await placeholders.filter({ has: page.locator('[data-satisfied="true"]') }).count(),
    ).toBe(0);

    // --- 2. filling one, through the UI -----------------------------------
    const target = requirements[0];
    const documentId = await uploadActiveDocument(
      globalProductId,
      target.documentTypeId,
      unique,
    );

    expect(documentId).toBeTruthy();

    const emptyPlaceholder = page
      .getByTestId("content-plan-placeholder")
      .filter({ hasText: await documentTypeNameFor(target.documentTypeId) })
      .first();

    await expect(emptyPlaceholder).toHaveAttribute("data-satisfied", "false");

    await emptyPlaceholder.getByTestId("place-document").click();

    // Nothing is attached yet, so the only route is attach-and-place — one
    // action for what a user experiences as one action.
    await page.getByTestId("attach-and-place-document").first().click();

    await expect(emptyPlaceholder).toHaveAttribute("data-satisfied", "true");
    await expect(emptyPlaceholder.getByTestId("placeholder-document"))
      .toBeVisible();

    await expect(page.getByTestId("content-plan-progress")).toContainText(
      `1 of ${requirements.length} placeholders filled`,
    );

    // Placed, so nothing is loose.
    await expect(page.getByTestId("unplaced-documents")).toHaveCount(0);

    await page.screenshot({
      path: "test-results/content-plan-filled.png",
      fullPage: true,
    });

    // --- 3. and misfiling is reversible where it happened ------------------
    await emptyPlaceholder.getByTestId("remove-placement").click();

    await expect(emptyPlaceholder).toHaveAttribute("data-satisfied", "false");

    // Out of the structure, but still attached — the document was not detached.
    await expect(page.getByTestId("unplaced-documents")).toBeVisible();

    expect(errors()).toEqual([]);
  });

  test("says so plainly when no blueprint governs the submission", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();

    // A device type under the same authority: no blueprint targets it.
    const FDA_510K = "40000000-0000-0000-0000-000000000001";

    const globalProductId = await createProduct(unique);
    const applicationId = await createApplication(globalProductId);
    const submissionId = await createSubmission(
      applicationId,
      unique,
      FDA_510K,
    );

    await page.goto(
      `/regulatory/products/${globalProductId}/applications/${applicationId}` +
        `/submissions/${submissionId}/content-plan`,
    );

    // A state to render, not a failure to report.
    await expect(page.getByTestId("content-plan-unbound")).toBeVisible();
    await expect(page.getByTestId("content-plan-section")).toHaveCount(0);

    expect(errors()).toEqual([]);
  });
});

// --- helpers ---------------------------------------------------------------

async function documentTypeNameFor(documentTypeId: string): Promise<string> {
  const types = await (await api("/reference-data/document-types")).json();

  const type = types.find((t: { id: string }) => t.id === documentTypeId);

  expect(type, `document type ${documentTypeId}`).toBeTruthy();

  return type.name;
}

async function createProduct(unique: number): Promise<string> {
  const response = await api("/api/products", {
    method: "POST",
    body: JSON.stringify({
      code: `BROWSER-PLAN-${unique}-${Math.floor(Math.random() * 100000)}`,
      name: `Browser Content Plan Product ${unique}`,
      type: "Drug",
    }),
  });

  expect(response.ok, "creating the product").toBeTruthy();

  return (await response.json()).id;
}

async function createApplication(globalProductId: string): Promise<string> {
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
      name: "Browser Content Plan Application",
    }),
  });

  expect(response.ok, "creating the application").toBeTruthy();

  return (await response.json()).id;
}

async function createSubmission(
  applicationId: string,
  unique: number,
  submissionTypeId: string = FDA_IND_SUBMISSION_TYPE,
): Promise<string> {
  const response = await api(`/applications/${applicationId}/submissions`, {
    method: "POST",
    body: JSON.stringify({
      submissionTypeId,
      title: `Browser Content Plan Submission ${unique}`,
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
  form.append("name", `Content Plan Doc ${documentTypeId} ${unique}`);

  // Raw fetch rather than the JSON helper: multipart needs fetch to set the
  // Content-Type itself, boundary and all.
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
