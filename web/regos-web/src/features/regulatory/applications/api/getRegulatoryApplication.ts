import { buildUrl } from "@/shared/api/apiClient";
import type { RegulatoryApplicationDetail } from "../types/RegulatoryApplicationDetail";

export async function getRegulatoryApplication(
  productId: string,
  applicationId: string
): Promise<RegulatoryApplicationDetail> {
  const response = await fetch(
    buildUrl(`/api/products/${productId}/applications/${applicationId}`)
  );

  if (!response.ok) {
    throw new Error("Failed to load regulatory application.");
  }

  return response.json();
}
