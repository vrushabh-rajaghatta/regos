import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { LocalLabel } from "../types/GlobalLabel";

/** What labelling do we hold for this market? */
export async function listLocalLabels(
  medicinalProductId: string,
): Promise<LocalLabel[]> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/local-labels`),
  );

  if (!response.ok) {
    throw new Error("Unable to load local labels.");
  }

  return response.json();
}
