import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface RecordCorrespondenceBody {
  authorityId: string;
  correspondenceTypeId: string;
  direction: string;
  subject: string;
  occurredOn: string;
  responseDueOn?: string | null;
  authorityReference?: string | null;
  authorityDivisionId?: string | null;
  regulatoryApplicationId?: string | null;
}

export async function recordCorrespondence(
  body: RecordCorrespondenceBody,
): Promise<{ id: string }> {
  const response = await apiFetch(buildUrl("/api/correspondence"), {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to log this correspondence."),
    );
  }

  return response.json();
}
