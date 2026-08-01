import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { CorrespondenceDetail } from "../types/CorrespondenceDetail";

export async function getCorrespondence(
  correspondenceId: string,
): Promise<CorrespondenceDetail> {
  const response = await apiFetch(
    buildUrl(`/api/correspondence/${correspondenceId}`),
  );

  if (!response.ok) {
    throw new Error("Unable to load this correspondence.");
  }

  return response.json();
}
