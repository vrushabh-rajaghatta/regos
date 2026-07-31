import { apiFetch, buildUrl } from "@/shared/api/apiClient";

/**
 * Moves an already-attached document into a section of the dossier, or — with a
 * null section — takes it out of the structure without detaching it.
 *
 * The request states the whole placement rather than a change to it, so sending
 * it twice lands in the same place.
 */
export async function placeSubmissionDocument(
  submissionId: string,
  submissionDocumentId: string,
  templateSectionId: string | null
): Promise<void> {
  const response = await apiFetch(
    buildUrl(
      `/submissions/${submissionId}/documents/${submissionDocumentId}/placement`
    ),
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ templateSectionId }),
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
    // Body was not JSON; fall through to the generic message.
  }

  return "Failed to place the document.";
}
