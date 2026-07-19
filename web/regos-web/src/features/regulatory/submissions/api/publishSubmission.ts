import { buildUrl } from "@/shared/api/apiClient";
import type { SubmissionValidationResult } from "../types/SubmissionValidation";

export interface PublishSubmissionOutcome {
  published: boolean;
  // Present only when publishing was refused; carries the reasons so the UI
  // can show them without a second validation request.
  validation?: SubmissionValidationResult;
}

export async function publishSubmission(
  submissionId: string
): Promise<PublishSubmissionOutcome> {
  const response = await fetch(
    buildUrl(`/submissions/${submissionId}/publish`),
    { method: "POST" }
  );

  if (response.ok) {
    return { published: true };
  }

  // Not ready — the body is the validation result, same shape as the
  // validation endpoint.
  if (response.status === 400) {
    const validation: SubmissionValidationResult = await response.json();
    return { published: false, validation };
  }

  throw new Error("Unable to publish submission.");
}
