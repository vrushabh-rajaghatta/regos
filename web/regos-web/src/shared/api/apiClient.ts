import { clearAccessToken, getAccessToken } from "@/shared/auth/accessToken";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export const buildUrl = (path: string): string => {
  return `${API_BASE_URL}${path}`;
};

/**
 * Raised when the API says the caller is not authenticated. Distinct from the
 * generic errors each API module throws, so the router can tell "your session
 * ended" apart from "that request failed" without matching on message text.
 */
export class UnauthenticatedError extends Error {
  constructor() {
    super("Your session has ended. Please sign in again.");
    this.name = "UnauthenticatedError";
  }
}

/**
 * `fetch` with the bearer token attached.
 *
 * Every call to the API goes through here, which is the point: no feature
 * module decides whether a request is authenticated, so none of them can
 * forget. There is deliberately no opt-out parameter — sign-in itself uses
 * plain `fetch`, because it is the one request that has no token yet.
 *
 * This replaced `tenantHeaders()`. The tenant now travels inside the token as
 * a signed claim, so the browser no longer asserts which organization it is
 * acting as (ADR-024).
 */
export async function apiFetch(
  url: string,
  init: RequestInit = {},
): Promise<Response> {
  const token = getAccessToken();

  const headers = new Headers(init.headers);

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(url, { ...init, headers });

  if (response.status === 401) {
    // The token is gone or no longer valid. Dropping it here means the next
    // navigation lands on the sign-in page instead of retrying with a
    // credential the server has already rejected.
    clearAccessToken();

    throw new UnauthenticatedError();
  }

  return response;
}
