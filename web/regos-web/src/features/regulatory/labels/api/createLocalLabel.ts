import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface CreateLocalLabelBody {
  labelTypeCode: string;
  language: string;
}

/**
 * Starts holding a controlled labelling document for this market. The first
 * revision opens with it.
 */
export async function createLocalLabel(
  medicinalProductId: string,
  body: CreateLocalLabelBody,
): Promise<{ id: string; draftRevisionId: string }> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/local-labels`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to add the local label."));
  }

  return response.json();
}
