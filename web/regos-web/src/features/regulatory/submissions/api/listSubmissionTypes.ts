import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { SubmissionTypeOption } from "../types/SubmissionTypeOption";

export async function listSubmissionTypes(
  authorityId: string
): Promise<SubmissionTypeOption[]> {
  // Filtered to the application's authority, mirroring the rule the handler
  // enforces rather than offering a choice the domain refuses.
  const response = await apiFetch(
    buildUrl(`/api/reference-data/submission-types?authorityId=${authorityId}`)
  );

  if (!response.ok) {
    throw new Error("Unable to load Regulatory Activity types.");
  }

  return response.json();
}
