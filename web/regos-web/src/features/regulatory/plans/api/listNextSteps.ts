import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { NextStep } from "../types/ProcessPlan";

/**
 * The plan board. `asOf` is what lateness is measured against — the server
 * defaults it to today, and passing it makes the answer reproducible.
 */
export async function listNextSteps(asOf?: string): Promise<NextStep[]> {
  const response = await apiFetch(
    buildUrl(`/api/process-plans/next-steps${asOf ? `?asOf=${asOf}` : ""}`),
  );

  if (!response.ok) {
    throw new Error("Unable to load the plan board.");
  }

  return response.json();
}
