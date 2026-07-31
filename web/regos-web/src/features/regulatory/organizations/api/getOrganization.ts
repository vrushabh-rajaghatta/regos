import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { OrganizationDetails } from "../types/OrganizationDetails";

/** Distinguishes a genuine 404 from a transport/server failure. */
export class OrganizationNotFoundError extends Error {}

export async function getOrganization(
  id: string,
): Promise<OrganizationDetails> {
  const response = await apiFetch(buildUrl(`/organizations/${id}`));

  if (response.status === 404) {
    throw new OrganizationNotFoundError("Organization not found.");
  }

  if (!response.ok) {
    throw new Error("Unable to load organization.");
  }

  return response.json();
}
