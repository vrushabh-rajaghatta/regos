import { buildUrl } from "@/shared/api/apiClient";

import type { LoginRequest } from "../types/LoginRequest";

/**
 * Signs in. Returns nothing: the session arrives as HttpOnly cookies the
 * browser stores itself, and there is no token for JavaScript to hold.
 *
 * Uses plain `fetch` rather than `apiFetch` — a 401 here means the credentials
 * were wrong, and attempting a refresh would be nonsense.
 */
export async function login(request: LoginRequest): Promise<void> {
  const response = await fetch(buildUrl("/api/auth/login"), {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    credentials: "include",
    body: JSON.stringify(request),
  });

  if (response.ok) return;

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
