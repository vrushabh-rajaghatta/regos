import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { Contact } from "../types/Contact";

export async function listOrganizationContacts(
  organizationId: string,
): Promise<Contact[] | null> {
  const response = await apiFetch(
    buildUrl(`/api/organizations/${organizationId}/contacts`),
  );

  if (response.status === 404) return null;

  if (!response.ok) {
    throw new Error("Unable to load contacts.");
  }

  return response.json();
}
