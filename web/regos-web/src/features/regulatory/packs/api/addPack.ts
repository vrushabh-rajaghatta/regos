import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface PackBody {
  description: string;
  packSizeQuantity: number | null;
  packSizeUnitCode: string | null;
  packCode: string | null;
  statusDate: string;
}

export async function addPack(
  medicinalProductId: string,
  body: PackBody,
): Promise<{ id: string }> {
  const response = await apiFetch(
    buildUrl(
      `/api/medicinal-products/${medicinalProductId}/packaged-products`,
    ),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to add the pack."));
  }

  return response.json();
}
