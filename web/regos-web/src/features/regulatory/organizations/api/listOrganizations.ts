import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { OrganizationListItem } from "../types/OrganizationListItem";

/**
 * The caller's own registry (ADR-032): the server's query filter scopes this
 * to the signed-in tenant, so what comes back is their organizations and
 * nobody else's. (This comment once claimed the opposite; that was the fused
 * tenant/organization model, retired by ADR-030.)
 */
export async function listOrganizations(): Promise<OrganizationListItem[]> {
  const response = await apiFetch(buildUrl("/organizations"));

  if (!response.ok) {
    throw new Error("Unable to load organizations.");
  }

  return response.json();
}
