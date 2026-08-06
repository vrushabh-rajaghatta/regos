import { apiFetch, buildUrl } from "@/shared/api/apiClient";

/**
 * Records that a submission contributes to a step of a plan, or clears the link
 * with null.
 *
 * **The route lives under submissions**, because the submission owns the column.
 * Process reads it and never writes it.
 */
export async function attachSubmissionToStep(
  submissionId: string,
  processStepId: string | null,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/submissions/${submissionId}/process-step`),
    { method: "PUT", body: JSON.stringify({ processStepId }) },
  );

  if (!response.ok) {
    const problem = await response.json().catch(() => null);

    throw new Error(problem?.detail ?? "Unable to link this submission.");
  }
}
