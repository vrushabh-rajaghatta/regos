import { buildUrl } from "@/shared/api/apiClient";

import type { CompletePasswordResetRequest } from "../types/PasswordResetRequests";

/**
 * Chooses a new password using a reset link.
 *
 * Plain `fetch`, like acceptance: a 401 here means the link is dead rather than
 * that a token needs refreshing, so it must not go through the client that
 * would try to refresh a session this caller does not have.
 *
 * No session comes back. Holding the link proves control of a mailbox, not
 * knowledge of the password just chosen, so this is followed by signing in.
 */
export async function completePasswordReset(
  request: CompletePasswordResetRequest,
): Promise<void> {
  const response = await fetch(buildUrl("/api/auth/password-reset/complete"), {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
  });

  if (response.ok) return;

  let message = "Unable to reset your password.";

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
