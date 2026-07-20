import { buildUrl } from "@/shared/api/apiClient";

import type { UserDetails } from "../types/UserDetails";

/** Distinguishes a genuine 404 from a transport/server failure. */
export class UserNotFoundError extends Error {}

export async function getUser(
  userId: string,
  organizationId?: string,
): Promise<UserDetails> {
  const suffix = organizationId
    ? `?organizationId=${encodeURIComponent(organizationId)}`
    : "";

  const response = await fetch(
    buildUrl(`/api/platform/users/${userId}${suffix}`),
  );

  if (response.status === 404) {
    throw new UserNotFoundError("User not found.");
  }

  if (!response.ok) {
    throw new Error("Unable to load user.");
  }

  return response.json();
}
