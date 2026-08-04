import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { Contraindication, UndesirableEffect } from "../types/Indication";

/** Who must not be given this product in this market. */
export async function listContraindications(
  medicinalProductId: string,
): Promise<Contraindication[]> {
  return read<Contraindication>(medicinalProductId, "contraindications");
}

/** What the approved label says it does to people. */
export async function listUndesirableEffects(
  medicinalProductId: string,
): Promise<UndesirableEffect[]> {
  return read<UndesirableEffect>(medicinalProductId, "undesirable-effects");
}

async function read<T>(
  medicinalProductId: string,
  kind: string,
): Promise<T[]> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/${kind}`),
  );

  if (!response.ok) {
    throw new Error("Unable to load clinical statements.");
  }

  return response.json();
}
