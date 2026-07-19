import { buildUrl } from "@/shared/api/apiClient";

export interface CreateSubmissionRequest {
  submissionTypeId: string;
  title: string;
}

export interface CreateSubmissionResponse {
  id: string;
}

export async function createSubmission(
  applicationId: string,
  request: CreateSubmissionRequest
): Promise<CreateSubmissionResponse> {
  const response = await fetch(
    buildUrl(`/applications/${applicationId}/submissions`),
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(request),
    }
  );

  if (!response.ok) {
    // Surface the backend's business-rule message (ProblemDetails.detail),
    // e.g. "Application is closed." — falling back to a generic message.
    throw new Error(await extractErrorMessage(response));
  }

  return response.json();
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

  return "Failed to create Submission.";
}
