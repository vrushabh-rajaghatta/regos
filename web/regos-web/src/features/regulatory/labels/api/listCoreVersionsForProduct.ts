import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { CoreVersionOption } from "../types/GlobalLabel";

/** Every core-label version this product's markets could derive from. */
export async function listCoreVersionsForProduct(
  globalProductId: string,
): Promise<CoreVersionOption[]> {
  const response = await apiFetch(
    buildUrl(`/api/products/${globalProductId}/core-versions`),
  );

  if (!response.ok) {
    throw new Error("Unable to load core versions.");
  }

  return response.json();
}
