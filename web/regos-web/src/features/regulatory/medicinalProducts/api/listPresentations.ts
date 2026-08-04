import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { Presentation } from "../types/Presentation";

/**
 * Fetched separately from the market itself. The two are different aggregates
 * with different lifecycles, and folding presentations into the market's read
 * would put composition on the critical path of every market load — including
 * the ones that only wanted a trade name.
 */
export async function listPresentations(
  medicinalProductId: string,
): Promise<Presentation[]> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/presentations`),
  );

  if (!response.ok) {
    throw new Error("Unable to load presentations.");
  }

  return response.json();
}
