import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface RecordDecisionBody {
  status: string;
  occurredOn: string;
  note: string | null;
}

/**
 * Appends what an authority decided. Nothing is ever rewritten: an indication
 * must not silently become withdrawn, it must have become withdrawn on a date.
 */
export async function recordIndicationDecision(
  indicationId: string,
  body: RecordDecisionBody,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/indications/${indicationId}/decisions`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to record the decision."));
  }
}
