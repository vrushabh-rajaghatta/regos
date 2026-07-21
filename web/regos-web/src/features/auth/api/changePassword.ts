import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { ChangePasswordRequest } from "../types/ChangePasswordRequest";

/**
 * Replaces the signed-in user's password.
 *
 * Unlike the other two credential flows this one has a session, so it goes
 * through `apiFetch` and gets the usual expired-access-token recovery.
 *
 * On success the server has revoked every session, including this one, and
 * cleared both cookies. The caller is signed out from this moment and must
 * sign in again.
 */
export async function changePassword(
  request: ChangePasswordRequest,
): Promise<void> {
  const response = await apiFetch(buildUrl("/api/auth/change-password"), {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
  });

  if (response.ok) return;

  let message = "Unable to change your password.";

  try {
    const problem = await response.json();

    if (typeof problem?.detail === "string") {
      message = problem.detail;
    }
  } catch {
    // No problem body - fall back to the generic message.
  }

  throw new Error(message);
}
