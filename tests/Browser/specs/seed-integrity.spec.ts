import { expect } from "@playwright/test";

import { test, api } from "./support";

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
 *
 * Scope: only the organization the acting tenant can legitimately see. The
 * three demo organizations are each seeded into their own tenant, and the
 * development account belongs to Demo MAH Ltd. — so under the fail-closed
 * query filters of ADR-031 the other two are correctly invisible to it. That
 * absence is the isolation working, not data loss, and is asserted below
 * rather than merely tolerated.
 */
const DEMO_MAH = {
  id: "30000000-0000-0000-0000-000000000003",
  legalName: "Demo MAH Ltd.",
  type: "MarketingAuthorizationHolder",
};

/** Seeded into their own tenants; unreachable from the development account. */
const OTHER_TENANTS_ORGANIZATIONS = [
  "30000000-0000-0000-0000-000000000001", // Demo Manufacturer Ltd.
  "30000000-0000-0000-0000-000000000002", // Demo Sponsor Ltd.
];

test.describe("Seed integrity", () => {
  test("the acting tenant's demo organization is untouched", async () => {
    const organizations = await (await api("/organizations")).json();

    const actual = organizations.find(
      (organization: { id: string }) => organization.id === DEMO_MAH.id,
    );

    expect(actual, `seeded organization ${DEMO_MAH.legalName} is missing`)
      .toBeDefined();

    expect(actual).toMatchObject({
      legalName: DEMO_MAH.legalName,
      type: DEMO_MAH.type,
      status: "Active",
    });
  });

  test("organizations belonging to other tenants are not visible", async () => {
    const organizations = await (await api("/organizations")).json();
    const visible = organizations.map((o: { id: string }) => o.id);

    for (const id of OTHER_TENANTS_ORGANIZATIONS) {
      expect(visible, `${id} leaked across the tenant boundary`)
        .not.toContain(id);
    }
  });
});
