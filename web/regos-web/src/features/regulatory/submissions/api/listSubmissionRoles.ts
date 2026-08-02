import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { SubmissionRole } from "../types/SubmissionRole";

export async function listSubmissionRoles(
  submissionId: string
): Promise<SubmissionRole[]> {
  const response = await apiFetch(
    buildUrl(`/api/submissions/${submissionId}/roles`)
  );

  if (!response.ok) {
    throw new Error("Unable to load who is named on this submission.");
  }

  return response.json();
}
