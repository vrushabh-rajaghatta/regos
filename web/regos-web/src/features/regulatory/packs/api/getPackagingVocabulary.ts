import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { PackagingVocabulary } from "../types/PackageItem";

export async function getPackagingVocabulary(): Promise<PackagingVocabulary> {
  const response = await apiFetch(
    buildUrl("/api/packaged-products/vocabulary"),
  );

  if (!response.ok) {
    throw new Error("Unable to load the packaging vocabulary.");
  }

  return response.json();
}
