import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export interface ChangeObjectiveStatusRequest {
  status: string;
  occurredOn: string;
  note: string | null;
}

export async function changeObjectiveStatus(
  id: string,
  request: ChangeObjectiveStatusRequest,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/process-objectives/${id}/status`),
    { method: "POST", body: JSON.stringify(request) },
  );

  if (!response.ok) {
    const problem = await response.json().catch(() => null);

    throw new Error(problem?.detail ?? "Unable to change the status.");
  }
}
