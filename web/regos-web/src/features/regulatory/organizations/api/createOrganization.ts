import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { CreateOrganizationRequest } from "../types/CreateOrganizationRequest";
import type { CreateOrganizationResponse } from "../types/CreateOrganizationResponse";
import { detailOf } from "@/shared/api/problemDetail";

export async function createOrganization(
  request: CreateOrganizationRequest,
): Promise<CreateOrganizationResponse> {
  const response = await apiFetch(buildUrl("/api/organizations"), {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  // Surface the API's ProblemDetails message so the user sees why the
  // organization was rejected rather than a generic failure.
  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to create organization."));
  }

  return response.json();
}
