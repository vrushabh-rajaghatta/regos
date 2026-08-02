import { expect } from "@playwright/test";

import { test, api } from "./support";

/**
 * A canary for the seeded blueprints, asserted at the API rather than through
 * the UI: the Explorer spec proves they *render*, this proves the seed itself
 * is intact and published-immutable.
 *
 * If the counts drift, the seed changed. Update these numbers deliberately when
 * a story grows the blueprint — never loosen them to make a broken seed pass.
 */
const FDA_IND_CTD = "60000000-0000-0000-0000-000000000001";

// Every blueprint shares the ICH-harmonized Modules 2–5 (31 sections, 8 required
// docs, 3 rules); the counts differ only by each one's regional Module 1.
//
// The FDA blueprint carries two versions, and that is the point of EPIC-007a
// S002: v1 placed the Investigator's Brochure at 1.13, which FDA's DTD defines
// as the Annual Report. A published version is frozen, so the correction is a
// new version and the old one is deprecated — never edited, never removed.
const BLUEPRINTS = [
  {
    id: FDA_IND_CTD,
    code: "FDA_IND_CTD",
    source: "ICH eCTD / FDA",
    versions: [
      {
        versionNumber: 1,
        status: "Deprecated",
        effectiveFrom: "2026-01-01",
        sections: 38,
        requiredDocuments: 13,
        validationRules: 4,
      },
      {
        versionNumber: 2,
        status: "Published",
        effectiveFrom: "2026-08-02",
        sections: 40,
        requiredDocuments: 13,
        validationRules: 4,
      },
    ],
  },
  {
    id: "60000000-0000-0000-0000-000000000002",
    code: "HC_CTA_CTD",
    source: "ICH eCTD / Health Canada",
    versions: [
      {
        versionNumber: 1,
        status: "Published",
        effectiveFrom: "2026-01-01",
        sections: 36,
        requiredDocuments: 11,
        validationRules: 4,
      },
    ],
  },
  {
    id: "60000000-0000-0000-0000-000000000003",
    code: "TGA_CTN_CTD",
    source: "ICH eCTD / TGA",
    versions: [
      {
        versionNumber: 1,
        status: "Published",
        effectiveFrom: "2026-01-01",
        sections: 36,
        requiredDocuments: 12,
        validationRules: 4,
      },
    ],
  },
  {
    id: "60000000-0000-0000-0000-000000000004",
    code: "CDSCO_CTA_CTD",
    source: "CDSCO / NDCT Rules 2019",
    versions: [
      {
        versionNumber: 1,
        status: "Published",
        effectiveFrom: "2026-01-01",
        sections: 36,
        requiredDocuments: 12,
        validationRules: 4,
      },
    ],
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

      expect(template.versions).toHaveLength(blueprint.versions.length);

      // Exactly one version is in force. A blueprint that offered two would
      // make "which structure am I judged against?" ambiguous.
      const published = template.versions.filter(
        (v: { status: string }) => v.status === "Published",
      );
      expect(published, "exactly one version binds new submissions")
        .toHaveLength(1);

      for (const expected of blueprint.versions) {
        const version = template.versions.find(
          (v: { versionNumber: number }) =>
            v.versionNumber === expected.versionNumber,
        );

        expect(version, `version ${expected.versionNumber} is seeded`)
          .toBeDefined();

        expect(version).toMatchObject({
          versionNumber: expected.versionNumber,
          status: expected.status,
          effectiveFrom: expected.effectiveFrom,
        });
        expect(
          version.publishedOnUtc,
          "a version that was published keeps its publish stamp, deprecated or not",
        ).not.toBeNull();

        // A deprecated version is retained whole (ES-018). Its structure is
        // what a submission bound to it was judged against.
        expect(version.sections).toHaveLength(expected.sections);
        expect(version.requiredDocuments).toHaveLength(
          expected.requiredDocuments,
        );
        expect(version.validationRules).toHaveLength(expected.validationRules);

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
      }
    });
  }

  test("the FDA correction is in v2, and v1 still says what it said", async () => {
    const template = await (
      await api(`/reference-data/templates/${FDA_IND_CTD}`)
    ).json();

    const byNumber = (n: number) =>
      template.versions.find(
        (v: { versionNumber: number }) => v.versionNumber === n,
      );

    const section = (
      version: { sections: { id: string; code: string; title: string; parentSectionId: string | null }[] },
      code: string,
    ) => version.sections.find((s) => s.code === code);

    // v1 — retained verbatim, defect and all. A filing made against it has to
    // stay explicable, so this is not corrected in place.
    expect(section(byNumber(1), "1.13")).toMatchObject({
      title: "Investigator's Brochure",
    });
    expect(section(byNumber(1), "1.14.4.1")).toBeUndefined();

    // v2 — corrected against us-regional-v3-3.dtd (evidence E9), where
    // m1-13 is the annual report and the brochure lives three levels down at
    // m1-14-4-1-investigational-brochure.
    const v2 = byNumber(2);
    expect(section(v2, "1.13")).toMatchObject({ title: "Annual Report" });

    const labeling = section(v2, "1.14");
    const investigationalLabeling = section(v2, "1.14.4");
    const brochure = section(v2, "1.14.4.1");

    expect(brochure).toMatchObject({ title: "Investigator's Brochure" });

    // Four levels deep — M1 → 1.14 → 1.14.4 → 1.14.4.1 — which is deeper than
    // any section RegOS carried before this correction.
    expect(investigationalLabeling.parentSectionId).toBe(labeling.id);
    expect(brochure.parentSectionId).toBe(investigationalLabeling.id);
  });
});
