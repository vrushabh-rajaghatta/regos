const STORAGE_KEY = "regos.accessToken";

/**
 * Where the access token lives between page loads.
 *
 * `localStorage` is a deliberate, and temporary, choice. It is readable by any
 * script on the origin, so it trades XSS resistance for the ability to survive
 * a refresh without a refresh-token endpoint to rebuild the session from. The
 * alternative — an httpOnly cookie — is genuinely more secure and is what this
 * should become, but it needs the server to set and clear it, which is
 * AUTH-006's problem. Recorded here rather than in a comment nobody reads:
 * this is a known weakness with a scheduled fix.
 *
 * Everything else in the app goes through these three functions, so replacing
 * the mechanism means editing this file and nothing else.
 */
export function getAccessToken(): string | null {
  try {
    return window.localStorage.getItem(STORAGE_KEY);
  } catch {
    // Private browsing modes can throw on access rather than return null.
    return null;
  }
}

export function setAccessToken(token: string): void {
  window.localStorage.setItem(STORAGE_KEY, token);
}

export function clearAccessToken(): void {
  window.localStorage.removeItem(STORAGE_KEY);
}
