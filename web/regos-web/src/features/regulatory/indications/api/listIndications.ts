import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { Indication } from "../types/Indication";

/** What is this product approved to treat in this market? */
export async function listIndications(
  medicinalProductId: string,
): Promise<Indication[]> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/indications`),
  );

  if (!response.ok) {
    throw new Error("Unable to load indications.");
  }

  return response.json();
}
