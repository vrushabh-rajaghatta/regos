import { buildUrl } from "@/shared/api/apiClient";

import type { AcceptInvitationRequest } from "../types/AcceptInvitationRequest";

/**
 * Sets a first password and activates an invited account.
 *
 * Plain `fetch`, like sign-in: the caller has no session, and a 401 here means
 * the link is dead rather than that a token needs refreshing.
 *
 * No session comes back. Holding an invitation proves you were invited, not
 * that you know the password you have just chosen, so acceptance is followed by
 * signing in through the one path that issues sessions.
 */
export async function acceptInvitation(
  request: AcceptInvitationRequest,
): Promise<void> {
  const response = await fetch(buildUrl("/api/auth/invitations/accept"), {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
  });

  if (response.ok) return;

  let message = "Unable to accept this invitation.";

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
