import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { DrugInteraction } from "../types/Indication";

/** What does this product clash with in this market? */
export async function listInteractions(
  medicinalProductId: string,
): Promise<DrugInteraction[]> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/interactions`),
  );

  if (!response.ok) {
    throw new Error("Unable to load interactions.");
  }

  return response.json();
}
