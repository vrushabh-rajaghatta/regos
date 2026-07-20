import { buildUrl, tenantHeaders } from "@/shared/api/apiClient";

import type { UserDetails } from "../types/UserDetails";

/** Distinguishes a genuine 404 from a transport/server failure. */
export class UserNotFoundError extends Error {}

export async function getUser(userId: string): Promise<UserDetails> {
  // The tenant travels in a header, not the query string - the API decides
  // visibility, the caller cannot ask to see another organization's user.
  const response = await fetch(buildUrl(`/api/platform/users/${userId}`), {
    headers: tenantHeaders(),
  });

  if (response.status === 404) {
    throw new UserNotFoundError("User not found.");
  }

  if (!response.ok) {
    throw new Error("Unable to load user.");
  }

  return response.json();
}
