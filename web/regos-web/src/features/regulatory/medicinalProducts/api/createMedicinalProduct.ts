import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface CreateMedicinalProductBody {
  countryId: string;
  statusDate: string;
}

export async function createMedicinalProduct(
  globalProductId: string,
  body: CreateMedicinalProductBody,
): Promise<{ id: string }> {
  const response = await apiFetch(
    buildUrl(`/api/products/${globalProductId}/medicinal-products`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to add the market."));
  }

  return response.json();
}
