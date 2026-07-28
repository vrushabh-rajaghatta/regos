import { expect } from "@playwright/test";

import { test, api } from "./support";

/**
 * A canary for the seeded FDA IND (CTD) blueprint, asserted at the API rather
 * than through the UI: the Explorer spec proves it *renders*, this proves the
 * seed itself is intact and published-immutable.
 *
 * If the counts drift, the seed changed. Update these numbers deliberately when
 * a story grows the blueprint — never loosen them to make a broken seed pass.
 */
const FDA_IND_CTD = "60000000-0000-0000-0000-000000000001";

const EXPECTED = {
  sections: 38,
  requiredDocuments: 13,
  validationRules: 4,
};

test.describe("Blueprint seed integrity", () => {
  test("the FDA IND (CTD) blueprint is published and intact", async () => {
    const template = await (
      await api(`/reference-data/templates/${FDA_IND_CTD}`)
    ).json();

    expect(template).toMatchObject({
      code: "FDA_IND_CTD",
      status: "Active",
      source: "ICH eCTD / FDA",
    });

    // Exactly one version, published and effective-dated — the immutability seam.
    expect(template.versions).toHaveLength(1);
    const version = template.versions[0];

    expect(version).toMatchObject({
      versionNumber: 1,
      status: "Published",
      effectiveFrom: "2026-01-01",
    });
    expect(version.publishedOnUtc, "published version carries a publish stamp")
      .not.toBeNull();

    // Shape of the seeded blueprint — the representative CTD skeleton.
    expect(version.sections).toHaveLength(EXPECTED.sections);
    expect(version.requiredDocuments).toHaveLength(EXPECTED.requiredDocuments);
    expect(version.validationRules).toHaveLength(EXPECTED.validationRules);

    // Spot-checks that the content — not just the counts — is what we seeded.
    const codes = new Set(
      version.sections.map((s: { code: string }) => s.code),
    );
    // Regional Module 1 places the IB at 1.13 (template data, not app logic),
    // and Module 3 goes one level into the CMC families.
    for (const code of ["M1", "1.13", "3.2.S", "3.2.S.7", "3.2.P.8", "M5"]) {
      expect(codes, `section ${code} is seeded`).toContain(code);
    }

    // The version-wide PDF format rule, an Error, targeting no single section.
    const pdfRule = version.validationRules.find(
      (r: { code: string }) => r.code === "FDA-IND-PDF",
    );
    expect(pdfRule).toMatchObject({
      ruleType: "FileFormat",
      severity: "Error",
      sectionId: null,
      parameters: "pdf",
    });
  });
});
