import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { ClinicalVocabulary } from "../types/Indication";

/**
 * The demonstration clinical vocabulary RegOS ships. Fetched rather than
 * hard-coded, so the picker offers exactly what the server accepts.
 */
export async function getClinicalVocabulary(): Promise<ClinicalVocabulary> {
  const response = await apiFetch(buildUrl("/api/indications/vocabulary"));

  if (!response.ok) {
    throw new Error("Unable to load the clinical vocabulary.");
  }

  return response.json();
}
