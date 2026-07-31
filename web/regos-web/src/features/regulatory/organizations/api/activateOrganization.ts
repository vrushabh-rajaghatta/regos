import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import { detailOf } from "./problemDetail";

export async function activateOrganization(
  organizationId: string,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/organizations/${organizationId}/activate`),
    { method: "POST" },
  );

  if (response.ok) return;

  throw new Error(
    await detailOf(response, "Unable to activate this organization."),
  );
}
