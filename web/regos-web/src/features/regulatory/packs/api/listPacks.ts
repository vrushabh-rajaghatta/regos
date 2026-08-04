import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { Pack } from "../types/Pack";

/** What does this market sell? */
export async function listPacks(medicinalProductId: string): Promise<Pack[]> {
  const response = await apiFetch(
    buildUrl(
      `/api/medicinal-products/${medicinalProductId}/packaged-products`,
    ),
  );

  if (!response.ok) {
    throw new Error("Unable to load packs.");
  }

  return response.json();
}
