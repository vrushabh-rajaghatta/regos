import { buildUrl } from "@/shared/api/apiClient";
import type { SubmissionSummary } from "../types/SubmissionSummary";

export async function listSubmissions(
  applicationId: string
): Promise<SubmissionSummary[]> {
  const response = await fetch(
    buildUrl(`/applications/${applicationId}/submissions`)
  );

  if (!response.ok) {
    throw new Error("Failed to load Submissions.");
  }

  return response.json();
}
