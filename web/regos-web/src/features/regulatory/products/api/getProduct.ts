import { buildUrl, tenantHeaders } from "@/shared/api/apiClient";

import type { ProductDetails } from "../types/ProductDetails";

/** Distinguishes a genuine 404 from a transport/server failure. */
export class ProductNotFoundError extends Error {}

export async function getProduct(id: string): Promise<ProductDetails> {
  // The tenant travels in a header: a product owned by another organization
  // returns 404, so the caller cannot ask to see it.
  const response = await fetch(buildUrl(`/api/products/${id}`), {
    headers: tenantHeaders(),
  });

  if (response.status === 404) {
    throw new ProductNotFoundError("Product not found.");
  }

  if (!response.ok) {
    throw new Error("Unable to load product.");
  }

  return response.json();
}
