import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { OrganizationDto } from "../types/OrganizationDto";

export async function listOrganizations(): Promise<OrganizationDto[]> {
  const response = await apiFetch(buildUrl("/api/organizations"));

  if (!response.ok) {
    throw new Error("Unable to load organizations.");
  }

  return response.json();
}
