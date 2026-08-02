import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { SubmissionChanges } from "../types/SubmissionChanges";

export async function getSubmissionChanges(
  submissionId: string
): Promise<SubmissionChanges> {
  const response = await apiFetch(
    buildUrl(`/api/submissions/${submissionId}/changes`)
  );

  if (!response.ok) {
    throw new Error("Unable to load what this sequence changed.");
  }

  return response.json();
}
