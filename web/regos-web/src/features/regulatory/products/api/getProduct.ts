import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { ProductDetails } from "../types/ProductDetails";

/** Distinguishes a genuine 404 from a transport/server failure. */
export class ProductNotFoundError extends Error {}

export async function getProduct(id: string): Promise<ProductDetails> {
  // The tenant travels inside the token: a product owned by another
  // organization returns 404, and the caller has no way to ask otherwise.
  const response = await apiFetch(buildUrl(`/api/products/${id}`));

  if (response.status === 404) {
    throw new ProductNotFoundError("Product not found.");
  }

  if (!response.ok) {
    throw new Error("Unable to load product.");
  }

  return response.json();
}
