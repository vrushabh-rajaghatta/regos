import { buildUrl } from "@/shared/api/apiClient";
import type { RegulatoryApplicationSummary } from "../types/RegulatoryApplicationSummary";

export async function listRegulatoryApplications(
  productId: string,
): Promise<RegulatoryApplicationSummary[]> {
  const response = await fetch(
    buildUrl(`/api/products/${productId}/applications`),
  );

  if (!response.ok) {
    throw new Error("Failed to load Applications.");
  }

  return response.json();
}
