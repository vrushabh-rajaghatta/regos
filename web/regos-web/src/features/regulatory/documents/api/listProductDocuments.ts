import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { ProductDocumentSummary } from "../types/ProductDocumentSummary";

export async function listProductDocuments(
  globalProductId: string
): Promise<ProductDocumentSummary[]> {
  const response = await apiFetch(
    buildUrl(`/api/products/${globalProductId}/documents`)
  );

  if (!response.ok) {
    throw new Error("Failed to load Documents.");
  }

  return response.json();
}
