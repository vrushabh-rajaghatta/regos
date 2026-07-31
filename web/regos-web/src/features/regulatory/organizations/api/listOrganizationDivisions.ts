import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { OrganizationDivision } from "../types/OrganizationDivision";

/** Null when the organization itself does not exist, so the page can 404. */
export async function listOrganizationDivisions(
  organizationId: string,
): Promise<OrganizationDivision[] | null> {
  const response = await apiFetch(
    buildUrl(`/api/organizations/${organizationId}/divisions`),
  );

  if (response.status === 404) return null;

  if (!response.ok) {
    throw new Error("Unable to load divisions.");
  }

  return response.json();
}
