import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import { detailOf } from "@/shared/api/problemDetail";

export interface AddOrganizationIdentifierRequest {
  schemeId: string;
  value: string;
}

export async function addOrganizationIdentifier(
  organizationId: string,
  request: AddOrganizationIdentifierRequest,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/organizations/${organizationId}/identifiers`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(request),
    },
  );

  if (response.ok) return;

  // A duplicate scheme and an unknown scheme are both stated by the server;
  // repeating those rules here would be a second place to keep them right.
  throw new Error(
    await detailOf(response, "Unable to record this identifier."),
  );
}
