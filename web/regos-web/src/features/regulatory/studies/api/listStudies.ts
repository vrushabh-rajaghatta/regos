import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { Study } from "../types/Study";

/**
 * Both kinds, newest first. One call, because the question "what studies do we
 * have?" spans two aggregates and the API composes the read.
 */
export async function listStudies(): Promise<Study[]> {
  const response = await apiFetch(buildUrl("/api/studies"));

  if (!response.ok) {
    throw new Error("Unable to load studies.");
  }

  return response.json();
}
