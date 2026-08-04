import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { ConditionMarket } from "../types/ConditionMarket";

/** Which markets is this product approved for this condition in? */
export async function listMarketsForCondition(
  globalProductId: string,
  conditionCode: string,
): Promise<ConditionMarket[]> {
  const response = await apiFetch(
    buildUrl(
      `/api/products/${globalProductId}/indications/${encodeURIComponent(
        conditionCode,
      )}/markets`,
    ),
  );

  if (!response.ok) {
    throw new Error("Unable to load the markets for this condition.");
  }

  return response.json();
}
