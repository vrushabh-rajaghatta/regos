import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface ApproveSiteBody {
  registrationId: string;
  organizationSiteId: string;
  approvedOn: string;
}

/**
 * Records that a licence names a manufacturing site, from a date.
 *
 * **Recording what an authority decided, not deciding it.**
 */
export async function approveSite(body: ApproveSiteBody): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/registrations/${body.registrationId}/sites`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to record this site on the licence."),
    );
  }
}
