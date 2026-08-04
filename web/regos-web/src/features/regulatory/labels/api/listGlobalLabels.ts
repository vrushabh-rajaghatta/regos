import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { GlobalLabel } from "../types/GlobalLabel";

/** What labels do we hold for this product? */
export async function listGlobalLabels(
  globalProductId: string,
): Promise<GlobalLabel[]> {
  const response = await apiFetch(
    buildUrl(`/api/products/${globalProductId}/global-labels`),
  );

  if (!response.ok) {
    throw new Error("Unable to load labels.");
  }

  return response.json();
}
