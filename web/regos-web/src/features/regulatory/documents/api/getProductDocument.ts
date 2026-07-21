import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { ProductDocumentDetail } from "../types/ProductDocumentDetail";

export async function getProductDocument(
  productId: string,
  documentId: string
): Promise<ProductDocumentDetail> {
  const response = await apiFetch(
    buildUrl(`/api/products/${productId}/documents/${documentId}`)
  );

  if (!response.ok) {
    throw new Error("Unable to load Document.");
  }

  return response.json();
}
