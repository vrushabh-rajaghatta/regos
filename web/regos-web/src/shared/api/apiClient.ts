const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export const buildUrl = (path: string): string => {
  return `${API_BASE_URL}${path}`;
};

/**
 * Raised when the API says the caller is not authenticated and refreshing did
 * not help. Distinct from the generic errors each API module throws, so the
 * router can tell "your session ended" apart from "that request failed"
 * without matching on message text.
 */
export class UnauthenticatedError extends Error {
  constructor() {
    super("Your session has ended. Please sign in again.");
    this.name = "UnauthenticatedError";
  }
}

/**
 * Sends the session cookies, and renews them when they have expired.
 *
 * There is nothing to attach: the access token is an HttpOnly cookie the
 * browser sends on its own, and this code could not read it if it wanted to
 * (ADR-025). `credentials: "include"` is required because the API is on a
 * different origin in development.
 *
 * A 401 is not final. Access tokens last fifteen minutes, so the common cause
 * is simply that one expired, and the refresh cookie is still good. One refresh
 * is attempted and the original request replayed; only if that fails is the
 * session genuinely over.
 */
export async function apiFetch(
  url: string,
  init: RequestInit = {},
): Promise<Response> {
  const response = await send(url, init);

  if (response.status !== 401) return response;

  if (!(await refresh())) throw new UnauthenticatedError();

  const retried = await send(url, init);

  // Retried exactly once. A second 401 after a successful refresh is not an
  // expiry problem, and retrying again would spin.
  if (retried.status === 401) throw new UnauthenticatedError();

  return retried;
}

const send = (url: string, init: RequestInit) =>
  fetch(url, { ...init, credentials: "include" });

/**
 * In flight at most once. Several requests failing together must not each fire
 * their own refresh — the second would present a token the first had already
 * rotated, which reads as a replay and ends the whole session.
 */
let inFlight: Promise<boolean> | null = null;

function refresh(): Promise<boolean> {
  inFlight ??= fetch(buildUrl("/api/auth/refresh"), {
    method: "POST",
    credentials: "include",
  })
    .then((response) => response.ok)
    .catch(() => false)
    .finally(() => {
      inFlight = null;
    });

  return inFlight;
}
