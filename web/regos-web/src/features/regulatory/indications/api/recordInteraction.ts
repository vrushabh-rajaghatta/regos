import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface RecordInteractionBody {
  interactionTypeCode: string;
  labelText: string;
  /** Required — an interaction with nothing to interact with is not one. */
  interactant: string;
  /** Optional. Set it and the interaction becomes joinable to the catalogue. */
  interactantSubstanceId: string | null;
  management: string | null;
  severityCode: string | null;
}

export async function recordInteraction(
  medicinalProductId: string,
  body: RecordInteractionBody,
): Promise<{ id: string }> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/interactions`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to record the interaction."));
  }

  return response.json();
}
