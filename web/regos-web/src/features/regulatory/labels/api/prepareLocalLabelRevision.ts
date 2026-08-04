import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface PrepareLocalLabelRevisionBody {
  contentId: string | null;
  derivedFromGlobalLabelVersionId: string | null;
  dataCarrierCode: string | null;
  changeSummary: string | null;
}

/**
 * Records what is settled while the revision is prepared. A **restate**, not a
 * patch: these facts are decided together, and sending one without the others
 * could point a translation of core v7 at a file that says v8.
 */
export async function prepareLocalLabelRevision(
  localLabelId: string,
  revisionId: string,
  body: PrepareLocalLabelRevisionBody,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/local-labels/${localLabelId}/revisions/${revisionId}`),
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to save the revision."));
  }
}
