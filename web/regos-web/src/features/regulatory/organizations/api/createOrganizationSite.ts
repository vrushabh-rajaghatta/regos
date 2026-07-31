import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import { detailOf } from "./problemDetail";

export interface CreateOrganizationSiteRequest {
  name: string;
  type: string;
  countryId: string;
  statusDate: string;
  nameNativeLanguage?: string | null;
  addressLine1?: string | null;
  city?: string | null;
  stateProvince?: string | null;
  postalCode?: string | null;
  email?: string | null;
  phone?: string | null;
  identifiers?: { schemeId: string; value: string }[];
}

export async function createOrganizationSite(
  organizationId: string,
  request: CreateOrganizationSiteRequest,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/organizations/${organizationId}/sites`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(request),
    },
  );

  if (response.ok) return;

  throw new Error(await detailOf(response, "Unable to record this site."));
}
