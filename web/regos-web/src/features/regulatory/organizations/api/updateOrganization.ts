import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { UpdateOrganizationRequest } from "../types/UpdateOrganizationRequest";
import { detailOf } from "@/shared/api/problemDetail";

export async function updateOrganization(
  id: string,
  request: UpdateOrganizationRequest,
): Promise<void> {
  const response = await apiFetch(buildUrl(`/api/organizations/${id}`), {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  if (response.ok) return;

  throw new Error(
    await detailOf(response, "Unable to update this organization."),
  );
}
