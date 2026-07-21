import { apiFetch, buildUrl } from "@/shared/api/apiClient";
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
  const response = await apiFetch(
    buildUrl(`/submissions/${submissionId}/publish`),
    { method: "POST" }
  );

  if (response.ok) {
    return { published: true };
  }

  // Not ready — the body is the validation result, same shape as the
  // validation endpoint. These are readiness issues the user can resolve.
  if (response.status === 400) {
    const validation: SubmissionValidationResult = await response.json();
    return { published: false, validation };
  }

  // 409 means a lifecycle conflict (already published). There is no checklist
  // to show, so surface the API's reason rather than a generic failure.
  let message = "Unable to publish submission.";

  try {
    const problem = await response.json();

    if (typeof problem?.detail === "string") {
      message = problem.detail;
    }
  } catch {
    // No problem body — fall back to the generic message.
  }

  throw new Error(message);
}
