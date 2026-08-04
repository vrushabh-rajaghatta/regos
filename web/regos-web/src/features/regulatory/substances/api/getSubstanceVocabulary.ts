import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { SubstanceVocabulary } from "../types/Substance";

/**
 * The class and type words the server will accept.
 *
 * Fetched rather than hard-coded in the form, so the picker offers exactly what
 * the API takes. A frontend copy of the list is the first place the two drift,
 * and this vocabulary is expected to be replaced wholesale when licensed
 * terminology arrives.
 */
export async function getSubstanceVocabulary(): Promise<SubstanceVocabulary> {
  const response = await apiFetch(buildUrl("/api/substances/vocabulary"));

  if (!response.ok) {
    throw new Error("Unable to load the substance vocabulary.");
  }

  return response.json();
}
