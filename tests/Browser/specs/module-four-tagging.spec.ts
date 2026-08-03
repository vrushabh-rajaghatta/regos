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
 * **EPIC-019 S003 — the Module 4 blocker, through the screens.**
 *
 * The backend proof lives in `SequenceFolderGeneratorTests`: the projection, the
 * snapshot, and both oracles. This asks the other question — **can a user
 * actually do it?**
 *
 * It is worth asking separately because this epic already produced one defect
 * that only a browser could find: documents filed in a section that expects a
 * different type rendered as a sentence with no controls, which is exactly the
 * shape a study report's supporting files take in 4.2.x. The backend supported
 * Module 4 and the UI quietly prevented finishing it.
 *
 * The assertion at the end is deliberately negative. A green package needs facts
 * this spec does not set up — a DUNS, an application number, a reachable
 * contact — so success is not the claim. **The claim is that the study tagging
 * refusal is gone**, and that whatever refusal remains names something else.
 */
const FDA_IND_CTD = "60000000-0000-0000-0000-000000000001";
const FDA_IND_APPLICATION_TYPE = "40000000-0000-0000-0000-000000000008";
const FDA = "20000000-0000-0000-0000-000000000001";
const UNITED_STATES = "10000000-0000-0000-0000-000000000001";

const PDF = new TextEncoder().encode("%PDF-1.7\n% RegOS module four test\n");

const EXPECTED_CONFLICT = /the server responded with a status of 409/;

test.describe("Filing a Module 4 study report", () => {
  test("tagged through the screens, and no longer refused for want of a study", async ({
    page,
  }) => {
    const errors = collectErrors(page, [EXPECTED_CONFLICT]);
    const unique = Date.now();
    const studyCode = `TOX-M4-${unique}`;

    const studyId = await registerStudy(
      studyCode,
      "A 13-Week Oral Toxicity Study In Rats",
    );

    const globalProductId = await createProduct(unique);
    const applicationId = await createApplication(globalProductId, unique);
    const submissionId = await createSubmission(applicationId, unique);

    // 4.2.3 — Toxicology. Inside FDA's study-tagged range (4.2.x), and the
    // section that refused every package before this epic.
    const section = await sectionOf("4.2.3");

    // Publishing is gated on the blueprint, so the dossier has to be complete
    // before Module 4 can be added to it. The mandatory placeholders are the
    // submission-validation spec's subject; here they are only the price of
    // reaching publish.
    await fillMandatoryDocuments(submissionId, globalProductId, unique);

    const documentId = await uploadActiveDocument(
      globalProductId,
      await anyDocumentTypeAsync(),
      unique,
    );

    const placed = await api(`/submissions/${submissionId}/documents`, {
      method: "POST",
      body: JSON.stringify({
        productDocumentId: documentId,
        templateSectionId: section,
      }),
    });

    expect(placed.ok, "placing into 4.2.3").toBeTruthy();

    const workspace =
      `/regulatory/products/${globalProductId}/applications/${applicationId}` +
      `/submissions/${submissionId}`;

    await page.goto(`${workspace}/content-plan`);

    // --- the row a user has to be able to reach ---------------------------
    //
    // It satisfies no placeholder — the section does not expect this document
    // type — so it renders as supporting content. That is the shape a study
    // report's appendices take, and it must still carry its controls.
    const row = page
      .getByTestId("additional-document")
      .filter({ hasText: `Report ${await anyDocumentTypeAsync()} ${unique}` })
      .first();

    await expect(row).toBeVisible();
    await expect(row.getByTestId("report-study")).toHaveText("Set study");

    await row.getByTestId("report-study").click();

    await page.getByLabel("Study", { exact: true }).selectOption(studyId);

    await expect(page.getByTestId("file-tag-select")).toBeVisible();
    await page
      .getByTestId("file-tag-select")
      .selectOption("pre-clinical-study-report");

    await page.getByRole("button", { name: "Save" }).click();

    await expect(row.getByTestId("placement-study")).toHaveText(
      `${studyCode} · pre-clinical-study-report`,
    );

    await page.screenshot({
      path: "test-results/module-four-tagging.png",
      fullPage: true,
    });

    // --- publishing freezes what it says ----------------------------------
    await page.goto(`${workspace}/publishing`);
    await page.getByTestId("publish-submission").click();
    await expect(page.getByTestId("submission-published")).toBeVisible();

    // Still shown after filing, and now read from the snapshot rather than
    // from the registry.
    await page.goto(`${workspace}/content-plan`);
    await expect(page.getByTestId("placement-study")).toHaveText(
      `${studyCode} · pre-clinical-study-report`,
    );

    // --- and the blocker is gone ------------------------------------------
    //
    // Not "the package succeeds" — this submission has no application number
    // and its applicant may have no DUNS, both of which are their own refusals.
    // What must be true is that nothing is refused for want of a study.
    await page.getByTestId("generate-package").click();

    const refusal = page.getByTestId("generate-package-error");

    if (await refusal.isVisible().catch(() => false)) {
      const message = (await refusal.textContent()) ?? "";

      expect(
        message,
        "Module 4 no longer refuses for want of a study tagging file",
      ).not.toContain("Study Tagging File");

      // A refusal that still names something is the point of the epic before
      // this one: every refusal says whose next action it is.
      expect(message.length).toBeGreaterThan(0);
    }

    expect(errors()).toEqual([]);
  });
});

// --- helpers ---------------------------------------------------------------

type Requirement = {
  documentTypeId: string;
  sectionId: string;
  isMandatory: boolean;
};

async function fillMandatoryDocuments(
  submissionId: string,
  globalProductId: string,
  unique: number,
): Promise<void> {
  const template = await (
    await api(`/reference-data/templates/${FDA_IND_CTD}`)
  ).json();

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

    expect(response.ok, `placing ${requirement.documentTypeId}`).toBeTruthy();
  }
}

async function sectionOf(code: string): Promise<string> {
  const template = await (
    await api(`/reference-data/templates/${FDA_IND_CTD}`)
  ).json();

  const version = template.versions.find(
    (v: { status: string }) => v.status === "Published",
  );

  const section = version.sections.find(
    (s: { code: string }) => s.code === code,
  );

  expect(section, `section ${code} in the FDA IND blueprint`).toBeTruthy();

  return section.id;
}

async function anyDocumentTypeAsync(): Promise<string> {
  const types = await (await api("/reference-data/document-types")).json();

  expect(types.length, "seeded document types").toBeGreaterThan(0);

  return types[0].id;
}

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
      code: `BROWSER-M4-${unique}-${Math.floor(Math.random() * 100000)}`,
      name: `Browser Module Four Product ${unique}`,
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
      name: `Module Four IND ${unique}`,
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
      title: `Module Four Sequence ${unique}`,
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
    "toxicology-report.pdf",
  );
  form.append("documentTypeId", documentTypeId);
  form.append("name", `Report ${documentTypeId} ${unique}`);

  const upload = await fetch(
    `${API_URL}/api/products/${globalProductId}/documents`,
    { method: "POST", body: form, headers: { Cookie: await sessionCookies() } },
  );

  expect(upload.ok, "uploading the report").toBeTruthy();

  const documentId = (await upload.json()).id;

  const activate = await api(
    `/api/products/${globalProductId}/documents/${documentId}/activate`,
    { method: "POST" },
  );

  expect(activate.ok, "activating the report").toBeTruthy();

  return documentId;
}
