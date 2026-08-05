import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

import type { SiteAlignment } from "../types/SiteAlignment";

/**
 * "Where is this product made, and is that site on the licence?"
 *
 * Derived on every read and stored nowhere: a persisted "aligned" flag would
 * rot the moment either side moved, and both sides move.
 */
export async function listSiteAlignment(
  medicinalProductId: string,
): Promise<SiteAlignment[]> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/site-alignment`),
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to compare sites against the licences."),
    );
  }

  return response.json();
}
