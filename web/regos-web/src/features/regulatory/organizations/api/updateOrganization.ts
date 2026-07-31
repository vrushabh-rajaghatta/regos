import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { UpdateOrganizationRequest } from "../types/UpdateOrganizationRequest";

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

  let message = "Unable to update this organization.";

  try {
    const problem = await response.json();

    if (typeof problem?.detail === "string") {
      message = problem.detail;
    }
  } catch {
    // No problem body — fall back to the generic message.
  }

  throw new Error(message);
}
