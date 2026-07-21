import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { CurrentUser } from "../types/CurrentUser";

export async function getCurrentUser(): Promise<CurrentUser> {
  const response = await apiFetch(buildUrl("/api/auth/me"));

  if (!response.ok) {
    throw new Error("Unable to load the current user.");
  }

  return response.json();
}
