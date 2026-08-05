import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { ManufacturingVocabulary } from "../types/ManufacturingOperation";

export async function getManufacturingVocabulary(): Promise<ManufacturingVocabulary> {
  const response = await apiFetch(
    buildUrl("/api/manufacturing-operations/vocabulary"),
  );

  if (!response.ok) {
    throw new Error("Unable to load the manufacturing vocabulary.");
  }

  return response.json();
}
