import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { ObjectiveSummary } from "../types/ProcessObjective";

export async function listObjectives(
  includeClosed = false,
): Promise<ObjectiveSummary[]> {
  const response = await apiFetch(
    buildUrl(`/process-objectives?includeClosed=${includeClosed}`),
  );

  if (!response.ok) {
    throw new Error("Unable to load objectives.");
  }

  return response.json();
}
