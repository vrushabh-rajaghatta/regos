import { buildUrl } from "@/shared/api/apiClient";

import type { LoginRequest } from "../types/LoginRequest";
import type { LoginResponse } from "../types/LoginResponse";

/**
 * The one API call that uses plain `fetch` rather than `apiFetch`: it is the
 * request that has no token yet, and routing it through the authenticated
 * client would clear a stored token on every failed sign-in attempt.
 */
export async function login(request: LoginRequest): Promise<LoginResponse> {
  const response = await fetch(buildUrl("/api/auth/login"), {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    // The API answers every failure with one message on purpose (ADR-022), so
    // this shows what it said rather than inventing a more specific reason.
    let message = "Unable to sign in.";

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

  return response.json();
}
