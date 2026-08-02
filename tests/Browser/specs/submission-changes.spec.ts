import { expect } from "@playwright/test";

import { test, api, collectErrors, sessionCookies, API_URL } from "./support";

/**
 * **EPIC-004 S002 — what a filing changed.**
 *
 * The epic's central proof: publish 0000, replace one document, publish 0001,
 * and the record shows **exactly one replace and nothing else**.
 *
 * It proves the cumulative model as much as the diff (ADR-045). Sequence 0001
 * carries the whole dossier again — every document 0000 had — and RegOS derives
 * the increment from it. What a user maintains is the regulatory state; what
 * gets transmitted is the difference.
 *
 * The operations are read back from storage, never recomputed: the page shows
 * what the filing said at the moment it was made.
 */
const FDA_IND_CTD = "60000000-0000-0000-0000-000000000001";
const FDA_IND_APPLICATION_TYPE = "40000000-0000-0000-0000-000000000008";
const FDA = "20000000-0000-0000-0000-000000000001";
const UNITED_STATES = "10000000-0000-0000-0000-000000000001";

const PDF = new TextEncoder().encode("%PDF-1.7\n% RegOS browser test\n");

type Requirement = {
  documentTypeId: string;
  sectionId: string;
  isMandatory: boolean;
};

type Seeded = { documentId: string; requirement: Requirement };

test.describe("What a sequence changed", () => {
  test("one replace, and everything else carried forward", async ({ page }) => {
    const errors = collectErrors(page);
    const unique = Date.now();

    const template = await (
      await api(`/reference-data/templates/${FDA_IND_CTD}`)
    ).json();

    // The version in force, not whichever came back first: the FDA IND
    // blueprint carries a deprecated v1 alongside the published v2
    // (EPIC-007a S002), and a submission binds to the published one.
    const requirements: Requirement[] = template.versions.find((v: { status: string }) => v.status === "Published").requiredDocuments
      .filter((d: Requirement) => d.isMandatory);

    expect(requirements.length).toBeGreaterThan(1);

    const globalProductId = await createProduct(unique);
    const applicationId = await createApplication(globalProductId, unique);

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

    const workspace = (submissionId: string) =>
      `/regulatory/products/${globalProductId}/applications/${applicationId}` +
      `/submissions/${submissionId}`;

    // --- a draft has changed nothing, because it has filed nothing ---------
    const first = await createSubmission(applicationId, `Original IND ${unique}`);
    await fillDossier(first, seeded);

    await page.goto(`${workspace(first)}/changes`);
    await expect(page.getByTestId("changes-draft")).toBeVisible();

    // --- 0000: everything is new ------------------------------------------
    await publish(page, workspace(first));

    await page.goto(`${workspace(first)}/changes`);

    await expect(page.getByTestId("changes-baseline")).toContainText(
      "Sequence 0000, measured against the first filing in this application",
    );

    const firstChanges = page.getByTestId("submission-change");
    await expect(firstChanges).toHaveCount(requirements.length);
    await expect(
      page.locator('[data-testid="submission-change"][data-operation="New"]'),
    ).toHaveCount(requirements.length);

    // Nothing was carried forward, because there was nothing to carry.
    await expect(page.getByTestId("changes-unchanged")).toContainText(
      "0 documents carried forward unchanged",
    );

    // --- one document gets a new version ----------------------------------
    const replaced = seeded[0];
    await addVersion(globalProductId, replaced.documentId, unique);

    // --- 0001: the whole dossier again, one document at a newer version ----
    // This is the cumulative model in the plainest possible form. Sequence 0001
    // is not "the protocol amendment"; it is the complete dossier as it now
    // stands, and RegOS works out that only one thing moved.
    const second = await createSubmission(
      applicationId,
      `Amendment ${unique}`,
    );
    await fillDossier(second, seeded);

    await publish(page, workspace(second));

    await page.goto(`${workspace(second)}/changes`);

    await expect(page.getByTestId("changes-baseline")).toContainText(
      "Sequence 0001, measured against Sequence 0000",
    );

    // Exactly one replace, and nothing else at all.
    const changes = page.getByTestId("submission-change");
    await expect(changes).toHaveCount(1);
    await expect(changes).toHaveAttribute("data-operation", "Replace");
    await expect(changes).toContainText("v2 replaced v1");

    // The rest of the dossier is present and untouched — reported as a count,
    // because in a cumulative filing most of it always will be.
    await expect(page.getByTestId("changes-unchanged")).toContainText(
      `${requirements.length - 1} documents carried forward unchanged`,
    );

    await page.screenshot({
      path: "test-results/submission-changes.png",
      fullPage: true,
    });

    // --- and 0000 still says what it said ----------------------------------
    // The point of freezing: publishing 0001 did not rewrite what 0000 claimed.
    await page.goto(`${workspace(first)}/changes`);
    await expect(page.getByTestId("submission-change")).toHaveCount(
      requirements.length,
    );

    expect(errors()).toEqual([]);
  });
});

// --- helpers ---------------------------------------------------------------

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
      code: `BROWSER-DIFF-${unique}`,
      name: `Browser Diff Product ${unique}`,
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
      applicationTypeId: FDA_IND_APPLICATION_TYPE,
      applicantOrganizationId: applicant.id,
      name: `Browser Diff Application ${unique}`,
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
  form.append("name", `Browser Diff Doc ${documentTypeId} ${unique}`);

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

/** A second version of an existing document — what makes a Replace happen. */
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

  const upload = await fetch(
    `${API_URL}/api/products/${globalProductId}/documents/${documentId}/versions`,
    {
      method: "POST",
      body: form,
      headers: { Cookie: await sessionCookies() },
    },
  );

  expect(upload.ok, "uploading a new version").toBeTruthy();

  // No re-activation: the document is already Active, and a new version does
  // not send it back to Draft.
  expect((await upload.json()).versionNumber, "the second version").toBe(2);
}
