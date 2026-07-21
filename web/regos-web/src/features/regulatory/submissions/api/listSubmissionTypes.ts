import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { SubmissionTypeOption } from "../types/SubmissionTypeOption";

export async function listSubmissionTypes(
  authorityId: string
): Promise<SubmissionTypeOption[]> {
  // Filtered to the application's authority so a user can only pick a type
  // that belongs to it — mirrors the backend's Rule 3 (authority match).
  const response = await apiFetch(
    buildUrl(`/submission-types?authorityId=${authorityId}`)
  );

  if (!response.ok) {
    throw new Error("Unable to load Submission Types.");
  }

  return response.json();
}
