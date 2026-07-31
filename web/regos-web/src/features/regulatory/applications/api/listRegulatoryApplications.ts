import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { RegulatoryApplicationSummary } from "../types/RegulatoryApplicationSummary";

export async function listRegulatoryApplications(
  globalProductId: string,
): Promise<RegulatoryApplicationSummary[]> {
  const response = await apiFetch(
    buildUrl(`/api/products/${globalProductId}/applications`),
  );

  if (!response.ok) {
    throw new Error("Failed to load Applications.");
  }

  return response.json();
}
