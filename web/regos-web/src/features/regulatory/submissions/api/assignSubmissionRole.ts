import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export interface AssignSubmissionRoleRequest {
  contactId: string;
  roleId: string;
}

export async function assignSubmissionRole(
  submissionId: string,
  request: AssignSubmissionRoleRequest
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/submissions/${submissionId}/roles`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
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

  return "Failed to name that person on this submission.";
}
