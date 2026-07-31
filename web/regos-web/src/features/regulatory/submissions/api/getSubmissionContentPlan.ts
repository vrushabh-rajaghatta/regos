import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { SubmissionContentPlan } from "../types/SubmissionContentPlan";

export async function getSubmissionContentPlan(
  submissionId: string
): Promise<SubmissionContentPlan> {
  const response = await apiFetch(
    buildUrl(`/submissions/${submissionId}/content-plan`)
  );

  if (!response.ok) {
    throw new Error("Unable to load the content plan.");
  }

  return response.json();
}
