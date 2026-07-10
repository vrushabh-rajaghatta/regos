import { buildUrl } from "@/shared/api/apiClient";
import type { ProductSummary } from "../types/ProductSummary";

export async function listProducts(): Promise<ProductSummary[]> {
  const response = await fetch(buildUrl("/api/products"));
  if (!response.ok) {
    throw new Error("Unable to load products.");
  }
  return response.json();
}
