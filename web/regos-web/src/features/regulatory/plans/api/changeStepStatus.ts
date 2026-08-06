import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export interface ChangeStepStatusRequest {
  status: "InProgress" | "Complete" | "Skipped";
  occurredOn: string;
  /** Required when skipping — it becomes the record of why work was not done. */
  note: string | null;
}

export async function changeStepStatus(
  planId: string,
  stepId: string,
  request: ChangeStepStatusRequest,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/process-plans/${planId}/steps/${stepId}/status`),
    { method: "POST", body: JSON.stringify(request) },
  );

  if (!response.ok) {
    const problem = await response.json().catch(() => null);

    throw new Error(problem?.detail ?? "Unable to record that.");
  }
}
