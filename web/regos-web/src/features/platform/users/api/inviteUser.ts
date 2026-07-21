import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { InviteUserRequest } from "../types/InviteUserRequest";
import type { InviteUserResponse } from "../types/InviteUserResponse";

export async function inviteUser(
  request: InviteUserRequest,
): Promise<InviteUserResponse> {
  const response = await apiFetch(buildUrl("/api/platform/users/invitations"), {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    // Surface the API's ProblemDetails message (e.g. inactive organization or a
    // duplicate email) so the user sees why the invitation was rejected.
    let message = "Unable to invite user.";

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
