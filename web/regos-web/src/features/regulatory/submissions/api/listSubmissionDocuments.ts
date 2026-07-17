import { buildUrl } from "@/shared/api/apiClient";
import type { SubmissionDocumentListItem } from "../types/SubmissionDocumentListItem";

export async function listSubmissionDocuments(
  submissionId: string
): Promise<SubmissionDocumentListItem[]> {
  const response = await fetch(
    buildUrl(`/submissions/${submissionId}/documents`)
  );

  if (!response.ok) {
    throw new Error("Unable to load submission documents.");
  }

  return response.json();
}
