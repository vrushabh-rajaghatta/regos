import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { LocalLabelRevision } from "../types/GlobalLabel";

/** This market's regulatory history for one labelling document. */
export async function listLocalLabelRevisions(
  localLabelId: string,
): Promise<LocalLabelRevision[]> {
  const response = await apiFetch(
    buildUrl(`/api/local-labels/${localLabelId}/revisions`),
  );

  if (!response.ok) {
    throw new Error("Unable to load revisions.");
  }

  return response.json();
}
