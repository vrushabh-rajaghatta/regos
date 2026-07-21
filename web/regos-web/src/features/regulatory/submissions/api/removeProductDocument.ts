import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export async function removeProductDocument(
  submissionId: string,
  submissionDocumentId: string
): Promise<void> {
  const response = await apiFetch(
    buildUrl(
      `/submissions/${submissionId}/documents/${submissionDocumentId}`
    ),
    { method: "DELETE" }
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
    // Body was not JSON; fall through to the generic message.
  }

  return "Failed to remove document.";
}
