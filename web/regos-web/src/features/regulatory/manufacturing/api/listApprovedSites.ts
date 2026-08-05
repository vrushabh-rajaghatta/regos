import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

import type { ApprovedSite } from "../types/ApprovedSite";

/**
 * "Which sites do this market's licences approve?"
 *
 * Keyed on the market rather than a licence: a market holds several, and a site
 * may be named on more than one of them.
 */
export async function listApprovedSites(
  medicinalProductId: string,
): Promise<ApprovedSite[]> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/approved-sites`),
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to load which sites are approved here."),
    );
  }

  return response.json();
}
