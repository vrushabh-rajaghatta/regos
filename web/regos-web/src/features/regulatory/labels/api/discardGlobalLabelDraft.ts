import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

/**
 * Throws away the open draft. Only ever a draft — the server refuses anything
 * that has been in force, so this verb cannot reach a regulatory record.
 */
export async function discardGlobalLabelDraft(
  globalLabelId: string,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/global-labels/${globalLabelId}/draft`),
    { method: "DELETE" },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to discard the draft."));
  }
}
