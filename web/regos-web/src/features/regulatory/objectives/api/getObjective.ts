import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { ObjectiveDetail } from "../types/ProcessObjective";

export async function getObjective(id: string): Promise<ObjectiveDetail> {
  const response = await apiFetch(buildUrl(`/api/process-objectives/${id}`));

  if (!response.ok) {
    throw new Error("Unable to load the objective.");
  }

  return response.json();
}
