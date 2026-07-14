import { buildUrl } from "@/shared/api/apiClient";
import type { SubmissionDetail } from "../types/SubmissionDetail";

export async function getSubmission(
  submissionId: string
): Promise<SubmissionDetail> {
  const response = await fetch(buildUrl(`/submissions/${submissionId}`));

  if (!response.ok) {
    throw new Error("Unable to load Submission.");
  }

  return response.json();
}
