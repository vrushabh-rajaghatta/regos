import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

import type { ManufacturingOperation } from "../types/ManufacturingOperation";

/**
 * "Which sites make this product?"
 *
 * Keyed on the market rather than the global product: secondary packaging in
 * particular is done per market, and the answer is compared against one
 * market's licence (ADR-039, ADR-063).
 */
export async function listManufacturingOperations(
  medicinalProductId: string,
): Promise<ManufacturingOperation[]> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/manufacturing`),
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to load where this product is made."),
    );
  }

  return response.json();
}
