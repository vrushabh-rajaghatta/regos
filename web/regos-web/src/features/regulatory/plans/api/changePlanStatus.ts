import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export interface ChangePlanStatusRequest {
  status: "Active" | "Completed" | "Cancelled";
  occurredOn: string;
  note: string | null;
}

export async function changePlanStatus(
  planId: string,
  request: ChangePlanStatusRequest,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/process-plans/${planId}/status`),
    { method: "POST", body: JSON.stringify(request) },
  );

  if (!response.ok) {
    const problem = await response.json().catch(() => null);

    throw new Error(problem?.detail ?? "Unable to change the plan status.");
  }
}
