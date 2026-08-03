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
 * **EPIC-019 S002 — which study a placement reports.**
 *
 * The fact belongs to the *placement*, and this spec is written to make that
 * observable rather than merely stated. Two things follow from it and neither
 * is visible in the domain tests:
 *
 * - **Taking the document out of the dossier takes its study with it.** A
 *   document that sits nowhere reports nothing; a reference left behind would
 *   outlive the placement it describes.
 * - **The study travels on the placement, not the document.** It survives a
 *   move between sections, because moving a document is not a statement about
 *   which study it reports.
 *
 * The registry itself is `study-registry.spec.ts`. What this one adds is the
 * join between the two, through the screen a user actually files on.
 */
const FDA_IND_CTD = "60000000-0000-0000-0000-000000000001";
const FDA_IND_APPLICATION_TYPE = "40000000-0000-0000-0000-000000000008";
const FDA = "20000000-0000-0000-0000-000000000001";
const UNITED_STATES = "10000000-0000-0000-0000-000000000001";

const PDF = new TextEncoder().encode("%PDF-1.7\n% RegOS placement study test\n");

type Requirement = {
  documentTypeId: string;
  sectionId: string;
  isMandatory: boolean;
};

test.describe("Which study a placement reports", () => {
  test("named on the placement, kept across a move, and gone when unplaced", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();
    const studyCode = `TOX-PL-${unique}`;

    const study = await registerNonClinicalStudy(
      studyCode,
      "A 13-Week Oral Toxicity Study In Rats",
    );

    expect(study).toBeTruthy();

    const template = await (
      await api(`/reference-data/templates/${FDA_IND_CTD}`)
    ).json();

    const version = template.versions.find(
      (v: { status: string }) => v.status === "Published",
    );

    const requirements: Requirement[] = version.requiredDocuments.filter(
      (d: Requirement) => d.isMandatory,
    );

    const globalProductId = await createProduct(unique);
    const applicationId = await createApplication(globalProductId);
    const submissionId = await createSubmission(applicationId, unique);

    // Placed through the API: what this spec is about is the study, and the
    // placement itself already has its own spec.
    const target = requirements[0];

    const documentId = await uploadActiveDocument(
      globalProductId,
      target.documentTypeId,
      unique,
    );

    const placed = await api(`/submissions/${submissionId}/documents`, {
      method: "POST",
      body: JSON.stringify({
        productDocumentId: documentId,
        templateSectionId: target.sectionId,
      }),
    });

    expect(placed.ok, "placing the document").toBeTruthy();

    const contentPlanUrl =
      `/regulatory/products/${globalProductId}/applications/${applicationId}` +
      `/submissions/${submissionId}/content-plan`;

    await page.goto(contentPlanUrl);

    const placeholder = page
      .getByTestId("content-plan-placeholder")
      .filter({ has: page.getByTestId("placeholder-document") })
      .first();

    // --- nothing reported yet ---------------------------------------------
    await expect(placeholder.getByTestId("placement-study")).toHaveCount(0);

    // Asserted on the visible label, not the accessible name: the button's
    // aria-label names the document and section deliberately, so that a screen
    // reader hears which of several "Set study" buttons it is on.
    await expect(placeholder.getByTestId("report-study")).toHaveText(
      "Set study",
    );

    // --- name the study through the browser --------------------------------
    await placeholder.getByTestId("report-study").click();

    // By value, not by label: the option's text is the sponsor code, title and
    // kind, and matching on a slice of it would silently pick a neighbour.
    await page.getByLabel("Study", { exact: true }).selectOption(study);

    // The role only appears once a study is named: a file tag with no study
    // describes nothing, and the server refuses it.
    await expect(page.getByTestId("file-tag-select")).toBeVisible();
    await page.getByTestId("file-tag-select").selectOption("synopsis");

    await page.getByRole("button", { name: "Save" }).click();

    // The sponsor's code is what shows, because that is what a user
    // recognises — and the role beside it, as ICH publishes it.
    await expect(placeholder.getByTestId("placement-study")).toHaveText(
      `${studyCode} · synopsis`,
    );

    await page.screenshot({
      path: "test-results/placement-study.png",
      fullPage: true,
    });

    // --- moving the document keeps the study -------------------------------
    //
    // A different mandatory section, so the same document reports the same
    // study from somewhere else. Driven through the API because the assertion
    // is about what survives, not about how the move is made.
    //
    // It lands there satisfying no placeholder — a document type the section
    // does not expect is supporting content, which is exactly the shape a study
    // report's appendices take in 4.2.x. It still carries its study, and the
    // screen still lets someone name one.
    const elsewhere = requirements.find(
      (r) => r.sectionId !== target.sectionId,
    );

    expect(elsewhere, "a second mandatory section to move into").toBeTruthy();

    const moved = await api(
      `/submissions/${submissionId}/documents/${await placementIdOf(
        submissionId,
        documentId,
      )}/placement`,
      {
        method: "PUT",
        body: JSON.stringify({ templateSectionId: elsewhere!.sectionId }),
      },
    );

    expect(moved.ok, "moving the document").toBeTruthy();

    await page.reload();

    const moved_row = page
      .getByTestId("additional-document")
      .filter({ hasText: studyCode });

    await expect(moved_row).toBeVisible();
    await expect(moved_row.getByTestId("placement-study")).toHaveText(
      `${studyCode} · synopsis`,
    );

    // --- taking it out of the dossier takes the study with it --------------
    await moved_row.getByTestId("remove-placement").click();

    await expect(page.getByTestId("unplaced-documents")).toBeVisible();
    await expect(page.getByTestId("placement-study")).toHaveCount(0, {
      timeout: 10_000,
    });

    expect(errors()).toEqual([]);
  });

  test("a file tag must be a published word, and needs a study", async () => {
    const unique = Date.now();

    const studyId = await registerNonClinicalStudy(
      `TOX-FT-${unique}`,
      "A Study With A Role",
    );

    const template = await (
      await api(`/reference-data/templates/${FDA_IND_CTD}`)
    ).json();

    const version = template.versions.find(
      (v: { status: string }) => v.status === "Published",
    );

    const target: Requirement = version.requiredDocuments.filter(
      (d: Requirement) => d.isMandatory,
    )[0];

    const globalProductId = await createProduct(unique);
    const applicationId = await createApplication(globalProductId);
    const submissionId = await createSubmission(applicationId, unique);

    const documentId = await uploadActiveDocument(
      globalProductId,
      target.documentTypeId,
      unique,
    );

    await api(`/submissions/${submissionId}/documents`, {
      method: "POST",
      body: JSON.stringify({
        productDocumentId: documentId,
        templateSectionId: target.sectionId,
      }),
    });

    const placementId = await placementIdOf(submissionId, documentId);

    const put = (body: unknown) =>
      api(`/api/submissions/${submissionId}/documents/${placementId}/study`, {
        method: "PUT",
        body: JSON.stringify(body),
      });

    // The whole reason the vocabulary had to be held. "sinopsis" is one
    // keystroke from valid; the DTD accepts it (E34) and a reviewer's tool
    // does not recognise it, so the refusal has to happen here.
    const misspelled = await put({
      clinicalStudyId: null,
      nonClinicalStudyId: studyId,
      fileTag: "sinopsis",
    });

    expect(misspelled.status, "a tag ICH does not publish").toBe(409);
    expect(await misspelled.text()).toContain("sinopsis");

    // A tag with no study describes nothing.
    const orphaned = await put({
      clinicalStudyId: null,
      nonClinicalStudyId: null,
      fileTag: "synopsis",
    });

    expect(orphaned.status, "a tag with no study").toBe(409);

    // A regional tag is as valid as an ICH one — the realm is a property of
    // the word, not a second thing to state.
    const regional = await put({
      clinicalStudyId: null,
      nonClinicalStudyId: studyId,
      fileTag: "annotated-crf",
    });

    expect(regional.ok, "a us-realm tag").toBeTruthy();

    const plan = await (
      await api(`/submissions/${submissionId}/content-plan`)
    ).json();

    const row = documentsOf(plan).find(
      (d) => d.submissionDocumentId === placementId,
    );

    expect(row?.fileTag).toBe("annotated-crf");
  });

  test("a placement reports one study, not two", async () => {
    const unique = Date.now();

    const clinical = await registerClinicalStudy(
      `CLIN-PL-${unique}`,
      "A Clinical Study",
    );

    const nonClinical = await registerNonClinicalStudy(
      `TOX-PL2-${unique}`,
      "A Non-Clinical Study",
    );

    const template = await (
      await api(`/reference-data/templates/${FDA_IND_CTD}`)
    ).json();

    const version = template.versions.find(
      (v: { status: string }) => v.status === "Published",
    );

    const target: Requirement = version.requiredDocuments.filter(
      (d: Requirement) => d.isMandatory,
    )[0];

    const globalProductId = await createProduct(unique);
    const applicationId = await createApplication(globalProductId);
    const submissionId = await createSubmission(applicationId, unique);

    const documentId = await uploadActiveDocument(
      globalProductId,
      target.documentTypeId,
      unique,
    );

    await api(`/submissions/${submissionId}/documents`, {
      method: "POST",
      body: JSON.stringify({
        productDocumentId: documentId,
        templateSectionId: target.sectionId,
      }),
    });

    const placementId = await placementIdOf(submissionId, documentId);

    // Naming both is a caller bug, and picking one would file the document
    // under a study nobody chose.
    const both = await api(
      `/api/submissions/${submissionId}/documents/${placementId}/study`,
      {
        method: "PUT",
        body: JSON.stringify({
          clinicalStudyId: clinical,
          nonClinicalStudyId: nonClinical,
        }),
      },
    );

    expect(both.status, "naming two studies").toBe(409);

    // Naming one, then the other, replaces rather than accumulates.
    for (const body of [
      { clinicalStudyId: clinical, nonClinicalStudyId: null },
      { clinicalStudyId: null, nonClinicalStudyId: nonClinical },
    ]) {
      const response = await api(
        `/api/submissions/${submissionId}/documents/${placementId}/study`,
        { method: "PUT", body: JSON.stringify(body) },
      );

      expect(response.ok, `reporting ${JSON.stringify(body)}`).toBeTruthy();
    }

    const plan = await (
      await api(`/submissions/${submissionId}/content-plan`)
    ).json();

    const row = documentsOf(plan).find(
      (d) => d.submissionDocumentId === placementId,
    );

    expect(row?.studyKind).toBe("NonClinical");
    expect(row?.studyId).toBe(nonClinical);
  });
});

// --- helpers ---------------------------------------------------------------

type PlanDocument = {
  submissionDocumentId: string;
  studyId: string | null;
  studyKind: string | null;
  fileTag: string | null;
};

type PlanSection = {
  placeholders: { documents: PlanDocument[] }[];
  additionalDocuments: PlanDocument[];
  children: PlanSection[];
};

function documentsOf(plan: {
  sections: PlanSection[];
  unplacedDocuments: PlanDocument[];
}): PlanDocument[] {
  const walk = (sections: PlanSection[]): PlanDocument[] =>
    sections.flatMap((section) => [
      ...section.placeholders.flatMap((p) => p.documents),
      ...section.additionalDocuments,
      ...walk(section.children),
    ]);

  return [...walk(plan.sections), ...plan.unplacedDocuments];
}

async function placementIdOf(
  submissionId: string,
  productDocumentId: string,
): Promise<string> {
  const plan = await (
    await api(`/submissions/${submissionId}/content-plan`)
  ).json();

  const walk = (
    sections: (PlanSection & {
      placeholders: { documents: (PlanDocument & { productDocumentId: string })[] }[];
    })[],
  ): (PlanDocument & { productDocumentId: string })[] =>
    sections.flatMap((section) => [
      ...section.placeholders.flatMap((p) => p.documents),
      ...(section.additionalDocuments as (PlanDocument & {
        productDocumentId: string;
      })[]),
      ...walk(section.children as never),
    ]);

  const match = [
    ...walk(plan.sections),
    ...(plan.unplacedDocuments as (PlanDocument & {
      productDocumentId: string;
    })[]),
  ].find((d) => d.productDocumentId === productDocumentId);

  expect(match, "the placement for the uploaded document").toBeTruthy();

  return match!.submissionDocumentId;
}

async function registerNonClinicalStudy(
  identifier: string,
  title: string,
): Promise<string> {
  const response = await api("/api/studies/nonclinical", {
    method: "POST",
    body: JSON.stringify({ sponsorStudyIdentifier: identifier, title }),
  });

  expect(response.ok, "registering the non-clinical study").toBeTruthy();

  return (await response.json()).id;
}

async function registerClinicalStudy(
  identifier: string,
  title: string,
): Promise<string> {
  const response = await api("/api/studies/clinical", {
    method: "POST",
    body: JSON.stringify({ sponsorStudyIdentifier: identifier, title }),
  });

  expect(response.ok, "registering the clinical study").toBeTruthy();

  return (await response.json()).id;
}

async function createProduct(unique: number): Promise<string> {
  const response = await api("/api/products", {
    method: "POST",
    body: JSON.stringify({
      code: `BROWSER-STUDY-${unique}-${Math.floor(Math.random() * 100000)}`,
      name: `Browser Placement Study Product ${unique}`,
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

  const response = await api(`/api/products/${globalProductId}/applications`, {
    method: "POST",
    body: JSON.stringify({
      countryId: UNITED_STATES,
      authorityId: FDA,
      applicationTypeId: FDA_IND_APPLICATION_TYPE,
      applicantOrganizationId: applicant.id,
      name: "Browser Placement Study Application",
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
      title: `Browser Placement Study Submission ${unique}`,
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
  form.append("name", `Placement Study Doc ${documentTypeId} ${unique}`);

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
