import { expect, test } from "@playwright/test";

import { api } from "./support";

/**
 * A canary, not a feature test.
 *
 * Every browser spec must own the business entities it mutates: seed through
 * the API, capture the id, operate on that id. Never `list.find(...)` and act
 * on whatever came back — that is how a spec silently deactivated a seeded
 * organization, and how ADR-019 rule 1 gets violated without anyone noticing.
 *
 * If this fails, a spec has mutated data it did not create. Fix the spec; do
 * not relax this assertion.
 */
const SEEDED = [
  {
    id: "30000000-0000-0000-0000-000000000001",
    legalName: "Demo Manufacturer Ltd.",
    type: "Manufacturer",
  },
  {
    id: "30000000-0000-0000-0000-000000000002",
    legalName: "Demo Sponsor Ltd.",
    type: "Sponsor",
  },
  {
    id: "30000000-0000-0000-0000-000000000003",
    legalName: "Demo MAH Ltd.",
    type: "MarketingAuthorizationHolder",
  },
];

test.describe("Seed integrity", () => {
  test("the demo organizations are untouched", async () => {
    const organizations = await (await api("/organizations")).json();

    for (const expected of SEEDED) {
      const actual = organizations.find(
        (organization: { id: string }) => organization.id === expected.id,
      );

      expect(actual, `seeded organization ${expected.legalName} is missing`)
        .toBeDefined();

      expect(actual).toMatchObject({
        legalName: expected.legalName,
        type: expected.type,
        status: "Active",
      });
    }
  });
});
