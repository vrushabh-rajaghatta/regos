import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { PharmaceuticalVocabulary } from "../types/Presentation";

/**
 * The dose forms, routes and units the server will accept.
 *
 * Fetched rather than hard-coded, so the picker offers exactly what the API
 * takes. A frontend copy of the list is the first place the two drift, and this
 * vocabulary is expected to be replaced wholesale when licensed terminology
 * arrives.
 */
export async function getPharmaceuticalVocabulary(): Promise<PharmaceuticalVocabulary> {
  const response = await apiFetch(buildUrl("/api/presentations/vocabulary"));

  if (!response.ok) {
    throw new Error("Unable to load the presentation vocabulary.");
  }

  return response.json();
}
