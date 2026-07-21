import { buildUrl } from "@/shared/api/apiClient";

import type { OrganizationListItem } from "../types/OrganizationListItem";

/**
 * No tenant header: the organization directory is not tenant-scoped. An
 * organization *is* a tenant, so this read cannot be filtered by the caller's
 * own organization without returning exactly one row.
 */
export async function listOrganizations(): Promise<OrganizationListItem[]> {
  const response = await fetch(buildUrl("/organizations"));

  if (!response.ok) {
    throw new Error("Unable to load organizations.");
  }

  return response.json();
}
