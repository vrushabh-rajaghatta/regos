import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface RecordIndicationBody {
  conditionCode: string;
  labelText: string;
  approvedOn: string;
}

/** Records what an authority approved this product to treat. */
export async function recordIndication(
  medicinalProductId: string,
  body: RecordIndicationBody,
): Promise<{ id: string }> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/indications`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to record the indication."));
  }

  return response.json();
}
