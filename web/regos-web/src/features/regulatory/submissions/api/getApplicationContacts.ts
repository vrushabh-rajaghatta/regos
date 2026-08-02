import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { ApplicationContacts } from "../types/ApplicationContacts";

/** Derived from the latest published sequence, never stored (ADR-048). */
export async function getApplicationContacts(
  applicationId: string
): Promise<ApplicationContacts> {
  const response = await apiFetch(
    buildUrl(`/api/applications/${applicationId}/contacts`)
  );

  if (!response.ok) {
    throw new Error("Unable to load the application's contacts.");
  }

  return response.json();
}
