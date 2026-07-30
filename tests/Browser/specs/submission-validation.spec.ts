import { expect } from "@playwright/test";

import { test, api, collectErrors, sessionCookies, API_URL } from "./support";

/**
 * The epic's end-to-end proof: reference data governs a real submission all the
 * way to the browser and the publish decision.
 *
 *   reference data -> blueprint -> binding -> validation engine -> UI -> publish
 *
 * Prerequisites are created through the API rather than the UI, per the rule
 * that a spec owns the data it mutates (ADR-019) — and because driving the
 * upload dialog once per required document would exercise the upload widget,
 * which has its own coverage, rather than anything this epic added. The steps
 * this epic *is* about are driven through the browser: validating, attaching,
 * re-validating, and publishing.
 *
 * The required document types are read from the blueprint itself, so this spec
 * keeps working as the FDA IND template grows.
 */
const FDA_IND_CTD = "60000000-0000-0000-0000-000000000001";
const FDA_IND_SUBMISSION_TYPE = "40000000-0000-0000-0000-000000000008";
const FDA = "20000000-0000-0000-0000-000000000001";
// The authority must belong to the application's country, and the FDA is the
// United States' — so this is pinned rather than "whichever country came back".
const UNITED_STATES = "10000000-0000-0000-0000-000000000001";

const PDF = new TextEncoder().encode("%PDF-1.7\n% RegOS browser test\n");

/** A placeholder: a document type the blueprint expects in a given section. */
type Requirement = {
  documentTypeId: string;
  sectionId: string;
  isMandatory: boolean;
};

type Seeded = { documentId: string; requirement: Requirement };

test.describe("Submission validation against the blueprint", () => {
  test("blocks publishing until the blueprint is satisfied, then publishes", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();

    // --- the blueprint tells the spec what this dossier owes ---------------
    const template = await (
      await api(`/reference-data/templates/${FDA_IND_CTD}`)
    ).json();

    const version = template.versions[0];

    // A *placeholder* — a document type expected in a particular section — is
    // the unit of completeness, so the spec works from requirements rather than
    // from a set of types (EPIC-003 / ADR-036).
    const requirements: Requirement[] = version.requiredDocuments.filter(
      (d: Requirement) => d.isMandatory,
    );

    expect(requirements.length).toBeGreaterThan(1);

    const requiredTypeIds = [...new Set(requirements.map((r) => r.documentTypeId))];

    // A product holds one document per type, so this spec can only seed one
    // document per requirement while that holds. It does today; if a blueprint
    // ever requires one type in two sections, this is the line that will say so.
    expect(
      requiredTypeIds,
      "today's blueprint requires each document type exactly once",
    ).toHaveLength(requirements.length);

    // --- prerequisites, through the API -----------------------------------
    const productId = await createProduct(unique);
    const applicationId = await createApplication(productId);
    const submissionId = await createSubmission(applicationId, unique);

    // One active Product Document per requirement, each remembering the section
    // its placeholder lives in.
    const seeded: Seeded[] = [];
    for (const requirement of requirements) {
      seeded.push({
        documentId: await uploadActiveDocument(
          productId,
          requirement.documentTypeId,
          unique,
        ),
        requirement,
      });
    }

    const validationUrl =
      `/regulatory/products/${productId}/applications/${applicationId}` +
      `/submissions/${submissionId}/validation`;

    // --- 1. an empty submission is blocked, and says exactly why -----------
    await page.goto(validationUrl);

    await expect(page.getByTestId("validation-status")).toHaveAttribute(
      "data-valid",
      "false",
    );

    await expect(
      page.locator('[data-testid="validation-group"][data-severity="Error"]'),
    ).toBeVisible();

    // One blocking issue per unfilled placeholder, plus the validator's own
    // "this submission has no documents" summary.
    expect(await countIssues(page, "Error")).toBe(requirements.length + 1);

    // Checks this validator cannot perform yet are disclosed, not hidden.
    await expect(
      page.locator('[data-testid="validation-group"][data-severity="Information"]'),
    ).toBeVisible();
    await expect(page.getByTestId("unevaluated-rule-type").first()).toBeVisible();

    await page.screenshot({
      path: "test-results/validation-blocked.png",
      fullPage: true,
    });

    // --- 2. attaching, through the UI, satisfies nothing on its own -------
    // This expectation changed deliberately in EPIC-003: completeness is now
    // based on placement, not attachment (ADR-036). A document that sits
    // nowhere in the dossier satisfies no placeholder, however right its type.
    // The attach dialog has no section picker until STORY-004, so this is the
    // unplaced path — and the spec asserts the new rule rather than avoiding it.
    await page.goto(
      `/regulatory/products/${productId}/applications/${applicationId}` +
        `/submissions/${submissionId}/documents`,
    );

    await page.getByTestId("open-attach-dialog").click();
    await page.getByTestId("attach-document").first().click();

    await page.goto(validationUrl);
    await expect(page.getByTestId("validation-status")).toBeVisible();

    // Every placeholder is still unfilled. Only the "no documents at all"
    // summary cleared, which is why this is one fewer than before, not one
    // fewer requirement.
    expect(await countIssues(page, "Error")).toBe(requirements.length);

    // And the document is not ignored either: attaching something that counts
    // for nothing, and hearing nothing about it, is how a dossier gets
    // published with a document its author believed was included.
    await expect(
      page.getByTestId("validation-issue").filter({ hasText: "not been placed" }),
    ).toBeVisible();

    await page.screenshot({
      path: "test-results/validation-attached-but-unplaced.png",
      fullPage: true,
    });

    // --- 3. placing it is what satisfies the placeholder ------------------
    // The content plan says which document is unplaced, and of what type — the
    // read model doing the job it exists for.
    const plan = await (
      await api(`/submissions/${submissionId}/content-plan`)
    ).json();

    expect(plan.unplacedDocuments).toHaveLength(1);

    const unplaced = plan.unplacedDocuments[0];
    const itsSection = requirements.find(
      (r) => r.documentTypeId === unplaced.documentTypeId,
    )!.sectionId;

    const placed = await api(
      `/submissions/${submissionId}/documents/${unplaced.submissionDocumentId}/placement`,
      { method: "PUT", body: JSON.stringify({ templateSectionId: itsSection }) },
    );

    expect(placed.ok, "placing the attached document").toBeTruthy();

    await page.goto(validationUrl);
    await expect(page.getByTestId("validation-status")).toBeVisible();

    expect(await countIssues(page, "Error")).toBe(requirements.length - 1);

    // --- 4. the rest through the API, attached and placed -----------------
    // "Attachable" already excludes whatever the UI attached a moment ago, so
    // this never re-attaches it.
    const attachable = await (
      await api(`/submissions/${submissionId}/attachable-documents`)
    ).json();

    const attachableIds = attachable.map(
      (d: { productDocumentId: string }) => d.productDocumentId,
    );

    const remaining = seeded.filter((s) => attachableIds.includes(s.documentId));

    expect(remaining).toHaveLength(requirements.length - 1);

    for (const { documentId, requirement } of remaining) {
      const response = await api(`/submissions/${submissionId}/documents`, {
        method: "POST",
        body: JSON.stringify({
          productDocumentId: documentId,
          templateSectionId: requirement.sectionId,
        }),
      });

      expect(response.ok, `placing ${documentId}`).toBeTruthy();
    }

    await page.goto(validationUrl);

    await expect(page.getByTestId("validation-status")).toHaveAttribute(
      "data-valid",
      "true",
    );
    await expect(page.getByTestId("validation-status")).toContainText(
      "Ready to publish",
    );

    // Being publishable does not mean there is nothing to report: the
    // disclosure survives success. This is the assertion the whole epic's
    // "passed / failed / not evaluated" distinction comes down to.
    await expect(
      page.locator('[data-testid="validation-group"][data-severity="Information"]'),
    ).toBeVisible();
    expect(await countIssues(page, "Error")).toBe(0);

    await page.screenshot({
      path: "test-results/validation-ready.png",
      fullPage: true,
    });

    // --- 4. and it publishes ----------------------------------------------
    await page.goto(
      `/regulatory/products/${productId}/applications/${applicationId}` +
        `/submissions/${submissionId}/publishing`,
    );

    await page.getByTestId("publish-submission").click();

    await expect(page.getByTestId("submission-published")).toBeVisible();

    expect(errors()).toEqual([]);
  });
});

// --- helpers ---------------------------------------------------------------

async function countIssues(
  page: import("@playwright/test").Page,
  severity: string,
): Promise<number> {
  const group = page.locator(
    `[data-testid="validation-group"][data-severity="${severity}"]`,
  );

  if ((await group.count()) === 0) return 0;

  return group.getByTestId("validation-issue").count();
}

async function createProduct(unique: number): Promise<string> {
  const response = await api("/api/products", {
    method: "POST",
    body: JSON.stringify({
      code: `BROWSER-VAL-${unique}`,
      name: `Browser Validation Product ${unique}`,
      type: "Drug",
    }),
  });

  expect(response.ok, "creating the product").toBeTruthy();

  return (await response.json()).id;
}

async function createApplication(productId: string): Promise<string> {
  const organizations = await (await api("/organizations")).json();

  // An active one specifically — other specs create and deactivate
  // organizations, and "whatever came back first" is how a suite starts
  // depending on the order it happened to run in.
  const applicant = organizations.find(
    (o: { status: string }) => o.status === "Active",
  );

  expect(applicant, "an active organization to apply as").toBeTruthy();

  const response = await api(`/api/products/${productId}/applications`, {
    method: "POST",
    body: JSON.stringify({
      countryId: UNITED_STATES,
      authorityId: FDA,
      applicantOrganizationId: applicant.id,
      name: "Browser Validation Application",
    }),
  });

  expect(response.ok, "creating the application").toBeTruthy();

  return (await response.json()).id;
}

async function createSubmission(
  applicationId: string,
  unique: number,
): Promise<string> {
  const response = await api(`/applications/${applicationId}/submissions`, {
    method: "POST",
    body: JSON.stringify({
      submissionTypeId: FDA_IND_SUBMISSION_TYPE,
      title: `Browser Validation Submission ${unique}`,
    }),
  });

  expect(response.ok, "creating the submission").toBeTruthy();

  return (await response.json()).id;
}

/** Uploads a PDF and activates it, so it can be attached to a dossier. */
async function uploadActiveDocument(
  productId: string,
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
  form.append("name", `Browser Doc ${documentTypeId} ${unique}`);

  // Raw fetch rather than the JSON helper: multipart needs fetch to set the
  // Content-Type itself, boundary and all.
  const upload = await fetch(`${API_URL}/api/products/${productId}/documents`, {
    method: "POST",
    body: form,
    headers: { Cookie: await sessionCookies() },
  });

  expect(upload.ok, `uploading a ${documentTypeId} document`).toBeTruthy();

  const documentId = (await upload.json()).id;

  const activate = await api(
    `/api/products/${productId}/documents/${documentId}/activate`,
    { method: "POST" },
  );

  expect(activate.ok, "activating the document").toBeTruthy();

  return documentId;
}

