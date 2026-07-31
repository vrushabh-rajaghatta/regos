import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import { detailOf } from "@/shared/api/problemDetail";

export async function removeOrganizationIdentifier(
  organizationId: string,
  identifierId: string,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(
      `/api/organizations/${organizationId}/identifiers/${identifierId}`,
    ),
    { method: "DELETE" },
  );

  if (response.ok) return;

  throw new Error(
    await detailOf(response, "Unable to withdraw this identifier."),
  );
}
