import { apiFetch, buildUrl } from "@/shared/api/apiClient";

/**
 * Records that a letter serves a step of a plan, or clears the link with null.
 *
 * **The route lives under correspondence**, because the letter owns the column.
 * Process reads it and never writes it (ADR-065 D2).
 */
export async function attachCorrespondenceToStep(
  correspondenceId: string,
  processStepId: string | null,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/correspondence/${correspondenceId}/process-step`),
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ processStepId }),
    },
  );

  if (!response.ok) {
    const problem = await response.json().catch(() => null);

    throw new Error(problem?.detail ?? "Unable to link this correspondence.");
  }
}
