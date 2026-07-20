import { buildUrl, tenantHeaders } from "@/shared/api/apiClient";
import type { ProductSummary } from "../types/ProductSummary";

export async function listProducts(): Promise<ProductSummary[]> {
  const response = await fetch(buildUrl("/api/products"), {
    headers: tenantHeaders(),
  });
  if (!response.ok) {
    throw new Error("Unable to load products.");
  }
  return response.json();
}
