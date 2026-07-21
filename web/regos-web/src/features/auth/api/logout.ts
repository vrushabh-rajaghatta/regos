import { buildUrl } from "@/shared/api/apiClient";

/**
 * Ends the session server-side: the refresh token is revoked and both cookies
 * are cleared by the response. Never throws — the API answers 204 whatever
 * state the caller was in, and a failed sign-out must still sign the user out
 * of this browser.
 */
export async function logout(): Promise<void> {
  try {
    await fetch(buildUrl("/api/auth/logout"), {
      method: "POST",
      credentials: "include",
    });
  } catch {
    // Offline, or the API is down. The cookies may survive, but they expire,
    // and the app has already forgotten everything it cached.
  }
}
