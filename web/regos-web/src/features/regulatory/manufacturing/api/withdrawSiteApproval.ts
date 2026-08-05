import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

/**
 * Removes an approval recorded in error.
 *
 * **A correction, not a variation.** A site genuinely removed from a licence is
 * a different act with its own date.
 */
export async function withdrawSiteApproval(
  siteApprovalId: string,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/site-approvals/${siteApprovalId}`),
    { method: "DELETE" },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to remove this approval."),
    );
  }
}
