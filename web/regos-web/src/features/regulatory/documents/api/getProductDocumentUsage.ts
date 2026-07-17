import { buildUrl } from "@/shared/api/apiClient";
import type { DocumentUsageItem } from "../types/DocumentUsageItem";

export async function getProductDocumentUsage(
  productId: string,
  documentId: string
): Promise<DocumentUsageItem[]> {
  const response = await fetch(
    buildUrl(`/api/products/${productId}/documents/${documentId}/usage`)
  );

  if (!response.ok) {
    throw new Error("Unable to load document usage.");
  }

  return response.json();
}
