import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export interface InstantiatePlanRequest {
  processObjectiveId: string;
  /** A version, never a playbook — the schedule must not depend on when. */
  processDefinitionVersionId: string;
  anchorDate: string;
  name: string;
}

export async function instantiatePlan(
  request: InstantiatePlanRequest,
): Promise<{ id: string; stepCount: number }> {
  const response = await apiFetch(buildUrl("/api/process-plans"), {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const problem = await response.json().catch(() => null);

    throw new Error(problem?.detail ?? "Unable to create the plan.");
  }

  return response.json();
}
