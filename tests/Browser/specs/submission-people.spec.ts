import { expect } from "@playwright/test";

import { test, api, collectErrors, sessionCookies, API_URL } from "./support";

/**
 * **EPIC-004 S005 — the people on a filing belong to the filing.**
 *
 * Two claims (ADR-048):
 *
 * 1. Who is named is editable while a draft and **frozen at publication** —
 *    who was named on sequence 0003 is a fact about a filing already made.
 * 2. **An application's contacts are derived, never stored.** There is no
 *    `ApplicationContact`; the application page reads the latest published
 *    sequence, and publishing a new one *replaces* the answer rather than
 *    adding to it. A stored copy could only differ from this by being stale —
 *    the same argument that removed `SubmissionSnapshot` in S002.
 */
const FDA_IND_SUBMISSION_TYPE = "40000000-0000-0000-0000-000000000008";
const FDA = "20000000-0000-0000-0000-000000000001";
const UNITED_STATES = "10000000-0000-0000-0000-000000000001";

const FDA_IND_CTD = "60000000-0000-0000-0000-000000000001";

const QUALIFIED_PERSON = "81000000-0000-0000-0000-000000000001";
const REGULATORY_CONTACT = "81000000-0000-0000-0000-000000000003";

const PDF = new TextEncoder().encode("%PDF-1.7\n% RegOS browser test\n");

type Requirement = {
  documentTypeId: string;
  sectionId: string;
  isMandatory: boolean;
};

type Seeded = { documentId: string; requirement: Requirement };

test.describe("Who is named on a filing", () => {
  test("named on a draft, frozen once filed — and the application's contacts are derived", async ({
    page,
  }) => {
    const errors = collectErrors(page);
    const unique = Date.now();

    const globalProductId = await createProduct(unique);
    const { applicationId, organizationId } = await createApplication(
      globalProductId,
      unique,
    );

    await createContact(organizationId, "Ana", `Ruiz${unique}`);
    const bo = await createContact(organizationId, "Bo", `Nilsen${unique}`);

    // The blueprint has to be satisfied before anything can be published, so
    // the dossier is seeded once and re-placed on each sequence — the
    // cumulative model (ADR-045).
    const template = await (
      await api(`/reference-data/templates/${FDA_IND_CTD}`)
    ).json();

    const requirements: Requirement[] = template.versions[0].requiredDocuments
      .filter((d: Requirement) => d.isMandatory);

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

    const applicationPage =
      `/regulatory/products/${globalProductId}/applications/${applicationId}`;

    // --- before any filing, nobody has been named on one ------------------
    // An absence of a filing, not missing data.
    await page.goto(applicationPage);
    await expect(page.getByTestId("no-application-contacts")).toBeVisible();

    // --- a draft names people ---------------------------------------------
    const first = await createSubmission(applicationId, `Original IND ${unique}`);

    await page.goto(`${workspace(first)}/people`);
    await expect(page.getByTestId("no-submission-roles")).toBeVisible();

    await nameOnFiling(page, `Ana Ruiz${unique}`, "Qualified Person");
    await expect(page.getByTestId("submission-role")).toHaveCount(1);

    // A second naming, then removed — a draft is corrected, not amended.
    await nameOnFiling(page, `Bo Nilsen${unique}`, "Regulatory Contact");
    await expect(page.getByTestId("submission-role")).toHaveCount(2);

    await page
      .locator('[data-testid="submission-role"][data-role="Regulatory Contact"]')
      .getByTestId("remove-submission-role")
      .click();

    await expect(page.getByTestId("submission-role")).toHaveCount(1);

    // --- 0000 --------------------------------------------------------------
    await fillDossier(first, seeded);
    await publish(page, workspace(first));

    // --- frozen: the controls are gone, not merely disabled ---------------
    await page.goto(`${workspace(first)}/people`);

    await expect(page.getByTestId("submission-role")).toHaveCount(1);
    await expect(page.getByTestId("assign-submission-role")).toHaveCount(0);
    await expect(page.getByTestId("remove-submission-role")).toHaveCount(0);

    // The API refuses it too — the screen only declines to offer the action.
    const refused = await api(`/api/submissions/${first}/roles`, {
      method: "POST",
      body: JSON.stringify({ contactId: bo, roleId: REGULATORY_CONTACT }),
    });

    expect(refused.status, "naming someone on a published sequence").toBe(409);

    // --- the application's contacts are derived from that sequence --------
    await page.goto(applicationPage);

    await expect(page.getByTestId("application-contacts")).toContainText(
      "As filed in Sequence 0000",
    );
    await expect(page.getByTestId("application-contact")).toHaveCount(1);
    await expect(page.getByTestId("application-contact")).toContainText(
      `Ana Ruiz${unique}`,
    );

    // --- 0001 names somebody else -----------------------------------------
    // The latest sequence IS the state, so this replaces the answer rather
    // than adding to it. A stored ApplicationContact would have had to be kept
    // in step with exactly this.
    const second = await createSubmission(applicationId, `Amendment ${unique}`);

    const named = await api(`/api/submissions/${second}/roles`, {
      method: "POST",
      body: JSON.stringify({ contactId: bo, roleId: QUALIFIED_PERSON }),
    });

    expect(named.ok, "naming someone on the second draft").toBeTruthy();

    await fillDossier(second, seeded);
    await publish(page, workspace(second));

    await page.goto(applicationPage);

    await expect(page.getByTestId("application-contacts")).toContainText(
      "As filed in Sequence 0001",
    );
    await expect(page.getByTestId("application-contact")).toHaveCount(1);
    await expect(page.getByTestId("application-contact")).toContainText(
      `Bo Nilsen${unique}`,
    );

    // --- and 0000 still says what it said ---------------------------------
    await page.goto(`${workspace(first)}/people`);
    await expect(page.getByTestId("submission-role")).toContainText(
      `Ana Ruiz${unique}`,
    );

    await page.screenshot({
      path: "test-results/submission-people.png",
      fullPage: true,
    });

    expect(errors()).toEqual([]);
  });
});

// --- helpers ---------------------------------------------------------------

async function nameOnFiling(
  page: import("@playwright/test").Page,
  personName: string,
  roleName: string,
): Promise<void> {
  await page.locator("#contactId").click();

  // Matched on the run-unique surname: the directory is tenant-wide, so a
  // previous run's people are legitimately still in the list.
  await page
    .getByRole("option", { name: new RegExp(`^${personName} `) })
    .click();

  await page.locator("#roleId").click();
  await page.getByRole("option", { name: roleName, exact: true }).click();

  await page.getByTestId("assign-submission-role").click();
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
  form.append("name", `Browser People Doc ${documentTypeId} ${unique}`);

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

async function publish(
  page: import("@playwright/test").Page,
  workspace: string,
): Promise<void> {
  await page.goto(`${workspace}/publishing`);
  await page.getByTestId("publish-submission").click();
  await expect(page.getByTestId("submission-published")).toBeVisible();
}

async function createProduct(unique: number): Promise<string> {
  const response = await api("/api/products", {
    method: "POST",
    body: JSON.stringify({
      code: `BROWSER-PPL-${unique}`,
      name: `Browser People Product ${unique}`,
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
      name: `Browser People Application ${unique}`,
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
  const response = await api(
    `/api/organizations/${organizationId}/contacts`,
    {
      method: "POST",
      body: JSON.stringify({
        firstName,
        lastName,
        statusDate: "2026-01-05",
      }),
    },
  );

  expect(response.ok, `creating contact ${firstName}`).toBeTruthy();

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
