import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

/** Opens the next revision for preparation. At most one at a time. */
export async function startLocalLabelRevision(
  localLabelId: string,
): Promise<{ id: string; revisionNumber: number }> {
  const response = await apiFetch(
    buildUrl(`/api/local-labels/${localLabelId}/revisions`),
    { method: "POST" },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to start a revision."));
  }

  return response.json();
}
