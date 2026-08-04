import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface PublishGlobalLabelVersionBody {
  effectiveFrom: string;
  changeSummary: string | null;
}

/**
 * Puts a draft in force from a date, and retires the version it replaces — one
 * call, because a label family with two versions in force is not a state a
 * company can be in.
 */
export async function publishGlobalLabelVersion(
  globalLabelId: string,
  versionId: string,
  body: PublishGlobalLabelVersionBody,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(
      `/api/global-labels/${globalLabelId}/versions/${versionId}/publish`,
    ),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to publish the version."));
  }
}
