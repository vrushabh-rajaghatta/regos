import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { SubmissionSubTypeOption } from "../types/SubmissionSubTypeOption";

export async function listSubmissionSubTypes(
  authorityId: string
): Promise<SubmissionSubTypeOption[]> {
  const response = await apiFetch(
    buildUrl(
      `/api/reference-data/submission-sub-types?authorityId=${authorityId}`
    )
  );

  if (!response.ok) {
    throw new Error("Unable to load Sequence Actions.");
  }

  return response.json();
}
