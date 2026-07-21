import { buildUrl } from "@/shared/api/apiClient";

import type { CreateOrganizationRequest } from "../types/CreateOrganizationRequest";
import type { CreateOrganizationResponse } from "../types/CreateOrganizationResponse";

export async function createOrganization(
  request: CreateOrganizationRequest,
): Promise<CreateOrganizationResponse> {
  const response = await fetch(buildUrl("/organizations"), {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    // Surface the API's ProblemDetails message so the user sees why the
    // organization was rejected rather than a generic failure.
    let message = "Unable to create organization.";

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

  return response.json();
}
