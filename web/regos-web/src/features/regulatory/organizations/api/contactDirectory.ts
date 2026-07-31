import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { Contact } from "../types/Contact";

/** No role means everyone; there is no default filter. */
export async function contactDirectory(roleId?: string): Promise<Contact[]> {
  const suffix = roleId ? `?roleId=${roleId}` : "";

  const response = await apiFetch(buildUrl(`/api/contacts${suffix}`));

  if (!response.ok) {
    throw new Error("Unable to load the contact directory.");
  }

  return response.json();
}
