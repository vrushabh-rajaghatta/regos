import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { ContinuableSubmission } from "../types/ContinuableSubmission";

export async function listContinuableSubmissions(
  applicationId: string
): Promise<ContinuableSubmission[]> {
  const response = await apiFetch(
    buildUrl(`/api/applications/${applicationId}/submissions/continuable`)
  );

  if (!response.ok) {
    throw new Error("Unable to load Regulatory Activities.");
  }

  return response.json();
}
