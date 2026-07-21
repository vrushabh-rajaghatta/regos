import { buildUrl } from "@/shared/api/apiClient";

import type { RequestPasswordResetRequest } from "../types/PasswordResetRequests";

/**
 * Asks for a password reset link.
 *
 * Plain `fetch`, like sign-in: the caller has no session.
 *
 * The API answers 204 whether or not the address belongs to an account, and
 * this function deliberately does nothing to distinguish the cases — there is
 * nothing in the response to distinguish. The screen that calls it must show
 * the same confirmation either way, or the browser would reintroduce the
 * account-enumeration oracle the API was careful not to offer.
 */
export async function requestPasswordReset(
  request: RequestPasswordResetRequest,
): Promise<void> {
  const response = await fetch(buildUrl("/api/auth/password-reset/request"), {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
  });

  // Only a genuine failure - a network error, or the API being down. A refused
  // request is not one of the outcomes this endpoint has.
  if (!response.ok) {
    throw new Error("Unable to request a password reset. Please try again.");
  }
}
