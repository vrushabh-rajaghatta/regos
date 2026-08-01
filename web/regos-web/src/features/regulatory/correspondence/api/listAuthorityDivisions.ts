import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export interface AuthorityDivision {
  id: string;
  authorityId: string;
  name: string;
  /** True when this tenant added it; false when the platform ships it. */
  isTenantDefined: boolean;
}

export async function listAuthorityDivisions(
  authorityId: string,
): Promise<AuthorityDivision[]> {
  const response = await apiFetch(
    buildUrl(`/api/master-data/authorities/${authorityId}/divisions`),
  );

  if (!response.ok) {
    throw new Error("Unable to load the divisions for this authority.");
  }

  return response.json();
}
