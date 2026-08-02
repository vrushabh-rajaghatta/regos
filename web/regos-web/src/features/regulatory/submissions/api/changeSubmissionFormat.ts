import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export interface ChangeSubmissionFormatRequest {
  /** `Ectd` | `Nees` | `Paper`. The domain's word, never the screen's. */
  format: string;
}

/**
 * PUT, not PATCH: the body states the whole value, so sending it twice lands
 * in the same place.
 *
 * There is no equivalent for a published sequence — the server refuses it,
 * because a filing's format is a fact about something already filed (ADR-047).
 */
export async function changeSubmissionFormat(
  submissionId: string,
  request: ChangeSubmissionFormatRequest
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/submissions/${submissionId}/format`),
    {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(request),
    }
  );

  if (!response.ok) {
    throw new Error(await extractErrorMessage(response));
  }
}

async function extractErrorMessage(response: Response): Promise<string> {
  try {
    const problem = await response.json();

    if (problem && typeof problem.detail === "string") {
      return problem.detail;
    }
  } catch {
    // Response body was not JSON; fall through to the generic message.
  }

  return "Failed to change the submission format.";
}
