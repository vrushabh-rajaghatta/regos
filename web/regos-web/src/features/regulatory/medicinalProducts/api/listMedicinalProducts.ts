import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { MedicinalProduct } from "../types/MedicinalProduct";

export async function listMedicinalProducts(
  globalProductId: string,
): Promise<MedicinalProduct[]> {
  const response = await apiFetch(
    buildUrl(`/api/products/${globalProductId}/medicinal-products`),
  );

  if (!response.ok) {
    throw new Error("Unable to load the markets for this product.");
  }

  return response.json();
}
