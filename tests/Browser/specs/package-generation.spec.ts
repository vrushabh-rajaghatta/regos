import { expect } from "@playwright/test";

import {
  test,
  api,
  collectErrors,
  sessionCookies,
  API_URL,
  FDA_ORIGINAL_APPLICATION,
  FDA_SUBTYPE_APPLICATION,
} from "./support";

/**
 * **EPIC-007a S007 — delivery, and the words it is not allowed to use.**
 *
 * The Definition of Done makes this a **product requirement, not a
 * documentation one**: a DTD-valid package with a wrong `submission-type` token
 * is perfectly legal XML that a gateway rejects, so structural validity is a
 * weaker promise than it sounds. RegOS reaches **Level 2a** — structurally
 * legal, checked by a third-party parser — and not 2b, which is FDA's own
 * business rules.
 *
 * | Permitted | Forbidden |
 * |---|---|
 * | Generate eCTD Package | FDA-ready |
 * | Download Generated Package | Validated |
 * | | Ready for submission |
 *
 * Each forbidden phrase asserts a level of evidence this epic does not reach.
 * **Asserted absent rather than merely avoided**, because a phrase nobody is
 * watching for is a phrase that arrives in a later story.
 *
 * The second half is the more interesting one: **a refusal reaches the user in
 * its own words.** The generator refuses in five distinct ways and each names a
 * different person's next action — a missing DUNS is someone entering data, an
 * unread vocabulary is someone reading a specification, a study report is a
 * feature nobody has built. A screen that renders "could not generate package"
 * would collapse the distinction the whole epic exists to draw.
 */
const FDA_IND_APPLICATION_TYPE = "40000000-0000-0000-0000-000000000008";
const FDA = "20000000-0000-0000-0000-000000000001";
const UNITED_STATES = "10000000-0000-0000-0000-000000000001";

const FORBIDDEN = ["FDA-ready", "Validated", "Ready for submission"];

test.describe("eCTD package generation", () => {
  test("offers a package in permitted words, and refuses in its own", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();

    const globalProductId = await createProduct(unique);
    const applicationId = await createApplication(globalProductId, unique);
    const submissionId = await createSubmission(
      applicationId,
      `Original IND ${unique}`,
    );

    const workspace =
      `/regulatory/products/${globalProductId}/applications/${applicationId}` +
      `/submissions/${submissionId}`;

    // --- a draft is not offered a package ---------------------------------
    //
    // A package is what was filed, so only a published sequence has one.
    // Offering the button here would invite a refusal nobody could act on.
    await page.goto(workspace);
    await expect(page.getByTestId("generate-package")).toHaveCount(0);

    await fillDossier(submissionId, globalProductId, unique);
    await publishThroughTheBrowser(page, workspace);

    // --- published: the button appears, in the only words it may use -------
    await page.goto(workspace);

    await expect(page.getByTestId("generate-package")).toBeVisible();
    await expect(
      page.getByRole("button", { name: "Generate eCTD Package" }),
    ).toBeVisible();

    for (const claim of FORBIDDEN) {
      await expect(
        page.getByText(claim, { exact: false }),
        `"${claim}" asserts evidence this epic does not reach`,
      ).toHaveCount(0);
    }

    // --- and the refusal arrives in its own words --------------------------
    //
    // This application has no number recorded, so the generator stops at the
    // first fact it cannot state truthfully. The exact sentence matters: it
    // says what to do, and it is not the same sentence as any other refusal.
    await page.getByTestId("generate-package").click();

    const refusal = page.getByTestId("generate-package-error");

    await expect(refusal).toBeVisible();
    await expect(refusal).toContainText("no number");
    await expect(refusal).toContainText("Record the number FDA assigned");

    // Not collapsed into a generic failure, and not dressed up as one either.
    await expect(refusal).not.toContainText("Something went wrong");
    for (const claim of FORBIDDEN) {
      await expect(page.getByText(claim, { exact: false })).toHaveCount(0);
    }

    await page.screenshot({
      path: "test-results/package-generation.png",
      fullPage: true,
    });

    // A refused package is a 409 the app renders deliberately, so the browser
    // logs the response as a failed resource. Filtered by that one status
    // rather than by disabling the check — the refusal being visible on screen
    // is asserted above, and this only stops the console gate calling it a
    // defect.
    expect(
      errors().filter((message) => !message.includes("409 (Conflict)")),
    ).toEqual([]);
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

type Requirement = {
  documentTypeId: string;
  sectionId: string;
  isMandatory: boolean;
};

const FDA_IND_CTD = "60000000-0000-0000-0000-000000000001";
const PDF = new TextEncoder().encode("%PDF-1.7\n% RegOS browser test\n");

async function fillDossier(
  submissionId: string,
  globalProductId: string,
  unique: number,
): Promise<void> {
  const template = await (
    await api(`/reference-data/templates/${FDA_IND_CTD}`)
  ).json();

  // The version in force, not whichever came back first: the FDA IND blueprint
  // carries a deprecated v1 alongside the published one (S002).
  const requirements: Requirement[] = template.versions
    .find((v: { status: string }) => v.status === "Published")
    .requiredDocuments.filter((d: Requirement) => d.isMandatory);

  for (const requirement of requirements) {
    const documentId = await uploadActiveDocument(
      globalProductId,
      requirement.documentTypeId,
      unique,
    );

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
      code: `BROWSER-PKG-${unique}`,
      name: `Browser Package Product ${unique}`,
      type: "Drug",
    }),
  });

  expect(response.ok, "creating the product").toBeTruthy();

  return (await response.json()).id;
}

/**
 * Its own application, with **no number recorded** — which is the state this
 * spec is about. Recording one is a separate gesture (EPIC-007a), and its
 * absence is what the refusal names.
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
      applicationTypeId: FDA_IND_APPLICATION_TYPE,
      applicantOrganizationId: applicant.id,
      name: `Browser Package Application ${unique}`,
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
      title,
      submissionTypeId: FDA_ORIGINAL_APPLICATION,
      submissionSubTypeId: FDA_SUBTYPE_APPLICATION,
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
  form.append("name", `Browser Package Doc ${documentTypeId} ${unique}`);

  const upload = await fetch(
    `${API_URL}/api/products/${globalProductId}/documents`,
    { method: "POST", body: form, headers: { Cookie: await sessionCookies() } },
  );

  expect(upload.ok, `uploading a ${documentTypeId} document`).toBeTruthy();

  const documentId = (await upload.json()).id;

  const activate = await api(
    `/api/products/${globalProductId}/documents/${documentId}/activate`,
    { method: "POST" },
  );

  expect(activate.ok, "activating the document").toBeTruthy();

  return documentId;
}
