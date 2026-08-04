import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { SubstanceUsage } from "../types/SubstanceUsage";

/**
 * *"Which of our products contain this substance?"* — the question EPIC-010a
 * was built to answer, asked from the substance end.
 *
 * The chain it walks is `Substance → Ingredient → presentation → market →
 * product`, every hop a join on an id. A composition that stored substance
 * names could only be read the other way.
 */
export async function listProductsContainingSubstance(
  substanceId: string,
): Promise<SubstanceUsage[]> {
  const response = await apiFetch(
    buildUrl(`/api/substances/${substanceId}/products`),
  );

  if (!response.ok) {
    throw new Error("Unable to load the products containing this substance.");
  }

  return response.json();
}
