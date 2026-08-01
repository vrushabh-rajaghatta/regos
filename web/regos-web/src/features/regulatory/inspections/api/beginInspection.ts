import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface BeginInspectionBody {
  authorityId: string;
  title: string;
  initialStatus: string;
  occurredOn: string;
  organizationSiteId?: string | null;
  scheduledFor?: string | null;
}

export async function beginInspection(
  body: BeginInspectionBody,
): Promise<{ id: string }> {
  const response = await apiFetch(buildUrl("/api/inspections"), {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to record this inspection."));
  }

  return response.json();
}
