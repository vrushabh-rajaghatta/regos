import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { PackageItem } from "../types/PackageItem";

/** What is inside this pack? Every layer, in reading order. */
export async function listPackageItems(
  packagedProductId: string,
): Promise<PackageItem[]> {
  const response = await apiFetch(
    buildUrl(`/api/packaged-products/${packagedProductId}/items`),
  );

  if (!response.ok) {
    throw new Error("Unable to load what is inside this pack.");
  }

  return response.json();
}
