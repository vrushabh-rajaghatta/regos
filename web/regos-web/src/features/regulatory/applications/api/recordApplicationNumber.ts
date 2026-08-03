import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import { detailOf } from "@/shared/api/problemDetail";

export async function recordApplicationNumber(
  applicationId: string,
  applicationNumber: string,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/applications/${applicationId}/application-number`),
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ applicationNumber }),
    },
  );

  if (response.ok) return;

  throw new Error(
    await detailOf(response, "Unable to record this application number."),
  );
}
