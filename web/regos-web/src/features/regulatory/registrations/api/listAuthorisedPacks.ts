import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

import type { AuthorisedPack } from "../types/AuthorisedPack";

/**
 * "Which packs are authorised in this market, and how are they supplied?"
 *
 * Keyed on the market, not on a licence: a market has several licences and a
 * pack may be authorised under more than one, so asking a licence answers a
 * narrower question than anybody has.
 */
export async function listAuthorisedPacks(
  medicinalProductId: string,
): Promise<AuthorisedPack[]> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/authorised-packs`),
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to load what this market is authorised to sell."),
    );
  }

  return response.json();
}
