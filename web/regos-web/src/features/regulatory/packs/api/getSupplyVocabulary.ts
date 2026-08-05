import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { SupplyVocabulary } from "../types/Supply";

export async function getSupplyVocabulary(): Promise<SupplyVocabulary> {
  const response = await apiFetch(
    buildUrl("/api/packaged-products/supply-vocabulary"),
  );

  if (!response.ok) {
    throw new Error("Unable to load the supply vocabulary.");
  }

  return response.json();
}
