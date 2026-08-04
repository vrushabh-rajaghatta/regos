import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { Component } from "../types/Component";

/**
 * Every article in one market, flat and in reading order — parents before
 * their contents.
 */
export async function listComponents(
  medicinalProductId: string,
): Promise<Component[]> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/components`),
  );

  if (!response.ok) {
    throw new Error("Unable to load components.");
  }

  return response.json();
}
