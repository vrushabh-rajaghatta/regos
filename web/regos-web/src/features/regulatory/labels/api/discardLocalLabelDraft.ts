import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

/**
 * Throws away the revision being prepared. The server refuses anything an
 * authority approved.
 */
export async function discardLocalLabelDraft(
  localLabelId: string,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/local-labels/${localLabelId}/draft`),
    { method: "DELETE" },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to discard the draft."));
  }
}
