import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { GlobalLabelVersion } from "../types/GlobalLabel";

/** Every issue this label has had, newest first. */
export async function listGlobalLabelVersions(
  globalLabelId: string,
): Promise<GlobalLabelVersion[]> {
  const response = await apiFetch(
    buildUrl(`/api/global-labels/${globalLabelId}/versions`),
  );

  if (!response.ok) {
    throw new Error("Unable to load label versions.");
  }

  return response.json();
}
