import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { MedicinalProductDetail } from "../types/MedicinalProductDetail";

export async function getMedicinalProduct(
  medicinalProductId: string,
): Promise<MedicinalProductDetail | null> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}`),
  );

  // Null rather than an error: "this market does not exist" is a state the
  // page renders, not a failure it reports.
  if (response.status === 404) return null;

  if (!response.ok) {
    throw new Error("Unable to load the market.");
  }

  return response.json();
}
