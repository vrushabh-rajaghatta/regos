import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

/** Opens the next issue for writing. At most one draft at a time. */
export async function startGlobalLabelDraft(
  globalLabelId: string,
): Promise<{ id: string; versionNumber: number }> {
  const response = await apiFetch(
    buildUrl(`/api/global-labels/${globalLabelId}/versions`),
    { method: "POST" },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to start a draft."));
  }

  return response.json();
}
