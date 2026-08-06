import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { PlanImpact } from "../types/ProcessPlan";

/**
 * What today's facts imply, if nothing changes. An analysis — never the plan.
 */
export async function getPlanImpact(
  planId: string,
  asOf?: string,
): Promise<PlanImpact> {
  const response = await apiFetch(
    buildUrl(`/process-plans/${planId}/impact${asOf ? `?asOf=${asOf}` : ""}`),
  );

  if (!response.ok) {
    throw new Error("Unable to work out the impact.");
  }

  return response.json();
}
