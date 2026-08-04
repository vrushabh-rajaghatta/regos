import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

/**
 * Points a draft at the document it is. The file already exists in the
 * product's document library; this records what it means (ADR-059 §6).
 *
 * The server refuses a document held for a different product, and that refusal
 * is surfaced verbatim — it is the anti-corruption check, not a validation
 * nicety.
 */
export async function attachGlobalLabelContent(
  globalLabelId: string,
  versionId: string,
  contentId: string,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(
      `/api/global-labels/${globalLabelId}/versions/${versionId}/content`,
    ),
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ contentId }),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to attach the document."));
  }
}
