import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { LabelVocabulary } from "../types/GlobalLabel";

/**
 * The kinds of label a company may hold. Fetched rather than hard-coded, so the
 * picker offers exactly what the server accepts.
 */
export async function getLabelVocabulary(): Promise<LabelVocabulary> {
  const response = await apiFetch(buildUrl("/api/labels/vocabulary"));

  if (!response.ok) {
    throw new Error("Unable to load label types.");
  }

  return response.json();
}
