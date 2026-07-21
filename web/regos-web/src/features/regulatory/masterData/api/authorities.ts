import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { AuthorityDto } from "../types/AuthorityDto";

export async function listAuthorities(): Promise<AuthorityDto[]> {
  const response = await apiFetch(buildUrl("/master-data/authorities"));

  if (!response.ok) {
    throw new Error("Unable to load authorities.");
  }

  return response.json();
}
