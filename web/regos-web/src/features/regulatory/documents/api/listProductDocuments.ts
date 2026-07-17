import { buildUrl } from "@/shared/api/apiClient";
import type { ProductDocumentSummary } from "../types/ProductDocumentSummary";

export async function listProductDocuments(
  productId: string
): Promise<ProductDocumentSummary[]> {
  const response = await fetch(
    buildUrl(`/api/products/${productId}/documents`)
  );

  if (!response.ok) {
    throw new Error("Failed to load Documents.");
  }

  return response.json();
}
