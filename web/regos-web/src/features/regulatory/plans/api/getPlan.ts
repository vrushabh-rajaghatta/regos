import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { PlanDetail } from "../types/ProcessPlan";

export async function getPlan(id: string): Promise<PlanDetail> {
  const response = await apiFetch(buildUrl(`/api/process-plans/${id}`));

  if (!response.ok) {
    throw new Error("Unable to load the plan.");
  }

  return response.json();
}
