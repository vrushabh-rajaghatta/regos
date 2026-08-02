import { expect } from "@playwright/test";

import { test, api, collectErrors, sessionCookies, API_URL } from "./support";

/**
 * **EPIC-004 S004 — what a filing will be rendered as, and what that does not
 * change.**
 *
 * Two claims, and the second is the one that matters (ADR-047):
 *
 * 1. Format is chosen while drafting and **frozen at publication**. After the
 *    sequence is filed, the screen stops offering the choice, because what a
 *    filing was made as is no longer anybody's decision.
 * 2. **The delta is domain; the format is rendering.** A paper sequence still
 *    derives exactly the operations an eCTD one would. ADR-045 records the
 *    cumulative dossier as the product thesis — if derivation only ran for
 *    eCTD, that thesis would quietly be an eCTD implementation detail.
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

test.describe("A submission's format", () => {
  test("is chosen while a draft, frozen once filed — and the paper delta is derived all the same", async ({
    page,
  }) => {
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

    // --- created as paper, and the screen says so -------------------------
    // Deliberately not eCTD: an application whose early sequences predate its
    // move to electronic is the case format exists for.
    const first = await createSubmission(
      applicationId,
      `Original IND ${unique}`,
      "Paper",
    );

    await page.goto(workspace(first));

    const field = page.getByTestId("submission-format");
    await expect(field).toHaveAttribute("data-format", "Paper");

    // The screen's word, not the domain's.
    await expect(page.getByTestId("header-format")).toHaveText("Paper");

    // --- a draft can change its mind --------------------------------------
    await field.locator("#submission-format").click();
    await page.getByRole("option", { name: "eCTD", exact: true }).click();

    await expect(field).toHaveAttribute("data-format", "Ectd");
    await expect(page.getByTestId("header-format")).toHaveText("eCTD");

    // ...and back, because this sequence really was filed on paper.
    await field.locator("#submission-format").click();
    await page.getByRole("option", { name: "Paper", exact: true }).click();
    await expect(field).toHaveAttribute("data-format", "Paper");

    // --- 0000 on paper -----------------------------------------------------
    await fillDossier(first, seeded);
    await publish(page, workspace(first));

    // --- frozen: the control is gone, not merely disabled -----------------
    await page.goto(workspace(first));

    await expect(field).toHaveAttribute("data-format", "Paper");
    await expect(field.locator("#submission-format")).toHaveCount(0);
    await expect(field).toContainText("Fixed when the sequence was published.");

    // The API refuses it too — the screen is declining to offer an action the
    // aggregate would reject, not enforcing the rule itself.
    const refused = await api(`/api/submissions/${first}/format`, {
      method: "PUT",
      body: JSON.stringify({ format: "Ectd" }),
    });

    expect(refused.status, "changing a published sequence's format").toBe(409);

    // --- and the delta was derived anyway ---------------------------------
    // Paper has no XML backbone to write leaf operations into. RegOS still
    // knows what changed, because that is a fact about the dossier.
    await page.goto(`${workspace(first)}/changes`);

    await expect(
      page.locator('[data-testid="submission-change"][data-operation="New"]'),
    ).toHaveCount(requirements.length);

    // --- 0001, also paper, with one document at a newer version -----------
    const replaced = seeded[0];
    await addVersion(globalProductId, replaced.documentId, unique);

    const second = await createSubmission(
      applicationId,
      `Amendment ${unique}`,
      "Paper",
    );
    await fillDossier(second, seeded);
    await publish(page, workspace(second));

    await page.goto(`${workspace(second)}/changes`);

    // Exactly the result the eCTD spec asserts, for a paper filing.
    const changes = page.getByTestId("submission-change");
    await expect(changes).toHaveCount(1);
    await expect(changes).toHaveAttribute("data-operation", "Replace");
    await expect(page.getByTestId("changes-unchanged")).toContainText(
      `${requirements.length - 1} documents carried forward unchanged`,
    );

    await page.screenshot({
      path: "test-results/submission-format.png",
      fullPage: true,
    });

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
      code: `BROWSER-FMT-${unique}`,
      name: `Browser Format Product ${unique}`,
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
      name: `Browser Format Application ${unique}`,
    }),
  });

  expect(response.ok, "creating the application").toBeTruthy();

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
  form.append("name", `Browser Format Doc ${documentTypeId} ${unique}`);

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
