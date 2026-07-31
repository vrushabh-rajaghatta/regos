import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import { detailOf } from "./problemDetail";

export async function deactivateOrganization(
  organizationId: string,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/organizations/${organizationId}/deactivate`),
    { method: "POST" },
  );

  if (response.ok) return;

  throw new Error(
    await detailOf(response, "Unable to deactivate this organization."),
  );
}
