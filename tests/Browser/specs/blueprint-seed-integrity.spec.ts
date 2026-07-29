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

// Every blueprint shares the ICH-harmonized Modules 2–5 (31 sections, 8 required
// docs, 3 rules); the counts differ only by each one's regional Module 1.
const BLUEPRINTS = [
  {
    id: FDA_IND_CTD,
    code: "FDA_IND_CTD",
    source: "ICH eCTD / FDA",
    sections: 38,
    requiredDocuments: 13,
    validationRules: 4,
  },
  {
    id: "60000000-0000-0000-0000-000000000002",
    code: "HC_CTA_CTD",
    source: "ICH eCTD / Health Canada",
    sections: 36,
    requiredDocuments: 11,
    validationRules: 4,
  },
  {
    id: "60000000-0000-0000-0000-000000000003",
    code: "TGA_CTN_CTD",
    source: "ICH eCTD / TGA",
    sections: 36,
    requiredDocuments: 12,
    validationRules: 4,
  },
  {
    id: "60000000-0000-0000-0000-000000000004",
    code: "CDSCO_CTA_CTD",
    source: "CDSCO / NDCT Rules 2019",
    sections: 36,
    requiredDocuments: 12,
    validationRules: 4,
  },
];

test.describe("Blueprint seed integrity", () => {
  for (const blueprint of BLUEPRINTS) {
    test(`${blueprint.code} is published and intact`, async () => {
      const template = await (
        await api(`/reference-data/templates/${blueprint.id}`)
      ).json();

      expect(template).toMatchObject({
        code: blueprint.code,
        status: "Active",
        source: blueprint.source,
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

      // Shape of the seeded blueprint — regional M1 + harmonized M2–M5.
      expect(version.sections).toHaveLength(blueprint.sections);
      expect(version.requiredDocuments).toHaveLength(
        blueprint.requiredDocuments,
      );
      expect(version.validationRules).toHaveLength(blueprint.validationRules);

      // The harmonized Modules 2–5 are identical across every blueprint.
      const codes = new Set(
        version.sections.map((s: { code: string }) => s.code),
      );
      for (const code of ["M1", "3.2.S", "3.2.S.7", "3.2.P.8", "M5"]) {
        expect(codes, `section ${code} is seeded`).toContain(code);
      }

      // Every blueprint carries the version-wide PDF format rule (Error, no
      // section target); the code is namespaced per blueprint.
      const pdfRule = version.validationRules.find(
        (r: { ruleType: string }) => r.ruleType === "FileFormat",
      );
      expect(pdfRule).toMatchObject({
        severity: "Error",
        sectionId: null,
        parameters: "pdf",
      });
    });
  }
});
