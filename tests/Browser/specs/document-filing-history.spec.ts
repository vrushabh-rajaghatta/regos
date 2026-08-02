import { expect } from "@playwright/test";

import { test, api, collectErrors, sessionCookies, API_URL } from "./support";

/**
 * **EPIC-004 S006 — the capstone. One document, followed across an
 * application's filings.**
 *
 * This spec stores nothing new. Every assertion below reads a fact an earlier
 * story decided to record, which is the whole point of the story:
 *
 * | | comes from |
 * |---|---|
 * | `Sequence 0000`, `0001`, `0002` | **S001** — numbering claimed at publish |
 * | `New` → `Replace` → `Delete` | **S002** — derived and frozen at publish |
 * | published, in that order | **S003** — the lifecycle we own |
 * | `Paper`, then `eCTD` | **S004** — format frozen per sequence |
 * | Dr Chen, then Dr Singh | **S005** — named per filing, frozen at publish |
 *
 * The withdrawal is the interesting row. A placement is a `SubmissionDocument`;
 * an absence cannot be frozen, so it is a `SubmissionDeletion` (ADR-045). They
 * are different tables with different shapes, and they read back as one stream
 * because both carry the diff key — `(ProductDocumentId, TemplateSectionId)`.
 */
const FDA_IND_CTD = "60000000-0000-0000-0000-000000000001";
const FDA_IND_SUBMISSION_TYPE = "40000000-0000-0000-0000-000000000008";
const FDA = "20000000-0000-0000-0000-000000000001";
const UNITED_STATES = "10000000-0000-0000-0000-000000000001";

const REGULATORY_CONTACT = "81000000-0000-0000-0000-000000000003";

const PDF = new TextEncoder().encode("%PDF-1.7\n% RegOS browser test\n");

type Requirement = {
  documentTypeId: string;
  sectionId: string;
  isMandatory: boolean;
};

type Seeded = { documentId: string; requirement: Requirement };

test.describe("One document, across an application's filings", () => {
  test("first filed, replaced, withdrawn — and every column was decided by an earlier story", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();

    const template = await (
      await api(`/reference-data/templates/${FDA_IND_CTD}`)
    ).json();

    const requirements: Requirement[] = template.versions[0].requiredDocuments
      .filter((d: Requirement) => d.isMandatory);

    expect(requirements.length).toBeGreaterThan(0);

    const globalProductId = await createProduct(unique);
    const { applicationId, organizationId } = await createApplication(
      globalProductId,
      unique,
    );

    const chen = await createContact(organizationId, "Chen", `Wei${unique}`);
    const singh = await createContact(organizationId, "Singh", `Kaur${unique}`);

    const seeded: Seeded[] = [];
    for (const requirement of requirements) {
      seeded.push({
        requirement,
        documentId: await uploadActiveDocument(
          globalProductId,
          requirement.documentTypeId,
          unique,
        ),
      });
    }

    // **The document we follow is a supporting one, and it has to be.**
    // Every slot in the seeded FDA IND blueprint is mandatory, so a document
    // the blueprint requires can never be withdrawn — the validator would
    // refuse the next filing for an incomplete dossier, which is correct: you
    // cannot withdraw something the dossier is required to contain.
    //
    // So the tracked document sits *beside* the required set, in a section the
    // blueprint knows, and the mandatory dossier stays complete throughout.
    const trackedDocumentId = await uploadActiveDocument(
      globalProductId,
      requirements[0].documentTypeId,
      unique + 1,
    );

    const tracked: Seeded = {
      documentId: trackedDocumentId,
      requirement: requirements[0],
    };

    const filings =
      `/regulatory/products/${globalProductId}/documents/${trackedDocumentId}/usage`;

    const workspace = (submissionId: string) =>
      `/regulatory/products/${globalProductId}/applications/${applicationId}` +
      `/submissions/${submissionId}`;

    // --- nothing yet -------------------------------------------------------
    await page.goto(filings);
    await expect(page.getByTestId("no-filing-history")).toBeVisible();

    // --- Sequence 0000 — Paper, Dr Chen, first filed -----------------------
    const first = await createSubmission(
      applicationId,
      `Original IND ${unique}`,
      "Paper",
    );

    await nameOnFiling(first, chen);
    await fillDossier(first, [...seeded, tracked]);
    await publish(page, workspace(first));

    // --- Sequence 0001 — eCTD, Dr Singh, the tracked document replaced -----
    // The whole dossier again, one document at a newer version: the cumulative
    // model, and RegOS works out that only one thing moved.
    await addVersion(globalProductId, trackedDocumentId, unique);

    const second = await createSubmission(
      applicationId,
      `Amendment ${unique}`,
      "Ectd",
    );

    await nameOnFiling(second, singh);
    await fillDossier(second, [...seeded, tracked]);
    await publish(page, workspace(second));

    // --- Sequence 0002 — the tracked document withdrawn -------------------
    // Filed without it. Under the cumulative model its absence *is* the
    // withdrawal, and S002 writes that absence down.
    const third = await createSubmission(
      applicationId,
      `Withdrawal ${unique}`,
      "Ectd",
    );

    await nameOnFiling(third, singh);
    await fillDossier(third, seeded);

    // Publishing is allowed to refuse an incomplete dossier (S002's validator),
    // so this asserts what actually happened rather than assuming.
    await page.goto(`${workspace(third)}/publishing`);
    await page.getByTestId("publish-submission").click();
    await expect(page.getByTestId("submission-published")).toBeVisible();

    // --- the whole life, on one screen ------------------------------------
    await page.goto(filings);

    const events = page.getByTestId("filing-history-event");
    await expect(events).toHaveCount(3);

    // One application, so one group — a sequence number only means anything
    // inside one (ADR-044).
    await expect(page.getByTestId("filing-history-application")).toHaveCount(1);

    const at = (sequence: string) =>
      page.locator(
        `[data-testid="filing-history-event"][data-sequence="${sequence}"]`,
      );

    // 0000 — first filed, on paper.
    await expect(at("0")).toHaveAttribute("data-operation", "New");
    await expect(at("0")).toContainText("First filed");
    await expect(at("0")).toContainText("v1");
    await expect(at("0")).toContainText("Paper");

    // 0001 — replaced, and the format changed with the sequence.
    await expect(at("1")).toHaveAttribute("data-operation", "Replace");
    await expect(at("1")).toContainText("Replaced with a newer version");
    await expect(at("1")).toContainText("v2");
    await expect(at("1")).toContainText("eCTD");

    // 0002 — withdrawn. No version, because nothing was placed: this row is a
    // SubmissionDeletion, and it reads back beside the placements without any
    // identity being reconstructed.
    await expect(at("2")).toHaveAttribute("data-operation", "Delete");
    await expect(at("2")).toContainText("Withdrawn");
    await expect(at("2")).toContainText("—");

    // And the screen says what the next filing would be numbered.
    await expect(
      page.getByText("Will publish as next sequence (currently 0003)"),
    ).toBeVisible();

    // --- each sequence still says what it said -----------------------------
    // S004 and S005 froze these at publication, so reading them back three
    // filings later is the proof that freezing meant something.
    await page.goto(`${workspace(first)}/people`);
    await expect(page.getByTestId("submission-role")).toContainText(
      `Chen Wei${unique}`,
    );
    await expect(page.getByTestId("header-format")).toHaveText("Paper");

    await page.goto(`${workspace(second)}/people`);
    await expect(page.getByTestId("submission-role")).toContainText(
      `Singh Kaur${unique}`,
    );
    await expect(page.getByTestId("header-format")).toHaveText("eCTD");

    // The application's current contacts come from 0002, the latest filing —
    // derived, never stored (ADR-048).
    await page.goto(
      `/regulatory/products/${globalProductId}/applications/${applicationId}`,
    );
    await expect(page.getByTestId("application-contacts")).toContainText(
      "As filed in Sequence 0002",
    );

    await page.screenshot({
      path: "test-results/document-filing-history.png",
      fullPage: true,
    });

    expect(errors()).toEqual([]);
  });
});

// --- helpers ---------------------------------------------------------------

async function nameOnFiling(
  submissionId: string,
  contactId: string,
): Promise<void> {
  const response = await api(`/api/submissions/${submissionId}/roles`, {
    method: "POST",
    body: JSON.stringify({ contactId, roleId: REGULATORY_CONTACT }),
  });

  expect(response.ok, "naming the regulatory contact").toBeTruthy();
}

async function publish(
  page: import("@playwright/test").Page,
  workspace: string,
): Promise<void> {
  await page.goto(`${workspace}/publishing`);
  await page.getByTestId("publish-submission").click();
  await expect(page.getByTestId("submission-published")).toBeVisible();
}

async function fillDossier(
  submissionId: string,
  seeded: Seeded[],
): Promise<void> {
  for (const { documentId, requirement } of seeded) {
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
): Promise<{ applicationId: string; organizationId: string }> {
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

  return {
    applicationId: (await response.json()).id,
    organizationId: applicant.id,
  };
}

async function createContact(
  organizationId: string,
  firstName: string,
  lastName: string,
): Promise<string> {
  const response = await api(`/api/organizations/${organizationId}/contacts`, {
    method: "POST",
    body: JSON.stringify({ firstName, lastName, statusDate: "2026-01-05" }),
  });

  expect(response.ok, `creating contact ${firstName}`).toBeTruthy();

  return (await response.json()).id;
}

async function createSubmission(
  applicationId: string,
  title: string,
  format: string,
): Promise<string> {
  const response = await api(`/applications/${applicationId}/submissions`, {
    method: "POST",
    body: JSON.stringify({
      submissionTypeId: FDA_IND_SUBMISSION_TYPE,
      title,
      format,
    }),
  });

  expect(response.ok, "creating the submission").toBeTruthy();

  return (await response.json()).id;
}

async function addVersion(
  globalProductId: string,
  documentId: string,
  unique: number,
): Promise<void> {
  const form = new FormData();
  form.append(
    "file",
    new Blob([PDF], { type: "application/pdf" }),
    `revised-${unique}.pdf`,
  );

  const response = await fetch(
    `${API_URL}/api/products/${globalProductId}/documents/${documentId}/versions`,
    {
      method: "POST",
      body: form,
      headers: { Cookie: await sessionCookies() },
    },
  );

  expect(response.ok, "uploading a new version").toBeTruthy();
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

  const upload = await fetch(
    `${API_URL}/api/products/${globalProductId}/documents`,
    {
      method: "POST",
      body: form,
      headers: { Cookie: await sessionCookies() },
    },
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
