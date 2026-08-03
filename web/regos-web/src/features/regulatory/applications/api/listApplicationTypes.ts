import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { ApplicationTypeOption } from "../types/ApplicationTypeOption";

export async function listApplicationTypes(
  authorityId: string
): Promise<ApplicationTypeOption[]> {
  // Filtered to the chosen authority so a user can only pick a type that
  // belongs to it — mirroring the invariant RegulatoryApplication.Create
  // enforces, rather than letting the form offer a choice the domain refuses.
  const response = await apiFetch(
    buildUrl(`/api/reference-data/application-types?authorityId=${authorityId}`)
  );

  if (!response.ok) {
    throw new Error("Unable to load Application Types.");
  }

  return response.json();
}
