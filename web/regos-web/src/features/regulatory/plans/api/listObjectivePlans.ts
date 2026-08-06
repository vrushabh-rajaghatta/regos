import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { ObjectivePlanSummary } from "../types/ProcessPlan";

export async function listObjectivePlans(
  objectiveId: string,
): Promise<ObjectivePlanSummary[]> {
  const response = await apiFetch(
    buildUrl(`/process-objectives/${objectiveId}/plans`),
  );

  if (!response.ok) {
    throw new Error("Unable to load plans for this objective.");
  }

  return response.json();
}
