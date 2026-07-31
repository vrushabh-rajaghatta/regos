import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { OrganizationSiteSummary } from "../types/OrganizationSiteSummary";

export async function listOrganizationSites(
  organizationId: string,
): Promise<OrganizationSiteSummary[] | null> {
  const response = await apiFetch(
    buildUrl(`/api/organizations/${organizationId}/sites`),
  );

  if (response.status === 404) return null;

  if (!response.ok) {
    throw new Error("Unable to load sites.");
  }

  return response.json();
}
