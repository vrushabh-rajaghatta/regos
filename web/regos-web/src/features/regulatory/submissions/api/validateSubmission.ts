import { buildUrl } from "@/shared/api/apiClient";
import type { SubmissionValidationResult } from "../types/SubmissionValidation";

export async function validateSubmission(
  submissionId: string
): Promise<SubmissionValidationResult> {
  const response = await fetch(
    buildUrl(`/submissions/${submissionId}/validation`)
  );

  if (!response.ok) {
    throw new Error("Unable to load submission validation.");
  }

  return response.json();
}
