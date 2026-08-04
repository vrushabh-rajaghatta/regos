import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface PublishLocalLabelRevisionBody {
  approvedOn: string;
  effectiveFrom: string;
}

/**
 * Puts a revision in force and retires the one it replaces — one call, because
 * a market with two approved labels in force is not a state that exists.
 */
export async function publishLocalLabelRevision(
  localLabelId: string,
  revisionId: string,
  body: PublishLocalLabelRevisionBody,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(
      `/api/local-labels/${localLabelId}/revisions/${revisionId}/publish`,
    ),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to put the revision in force."),
    );
  }
}
