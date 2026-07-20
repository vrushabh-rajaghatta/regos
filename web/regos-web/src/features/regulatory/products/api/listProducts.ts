import { buildUrl, tenantHeaders } from "@/shared/api/apiClient";

import type { PagedResult } from "../types/PagedResult";
import type { ProductSummary } from "../types/ProductSummary";

export interface ListProductsParams {
  search?: string;
  type?: string;
  status?: string;
  page?: number;
  pageSize?: number;
}

export async function listProducts(
  params: ListProductsParams = {},
): Promise<PagedResult<ProductSummary>> {
  const query = new URLSearchParams();

  if (params.search) query.set("search", params.search);
  if (params.type) query.set("type", params.type);
  if (params.status) query.set("status", params.status);
  if (params.page) query.set("page", String(params.page));
  if (params.pageSize) query.set("pageSize", String(params.pageSize));

  const suffix = query.toString() ? `?${query}` : "";

  const response = await fetch(buildUrl(`/api/products${suffix}`), {
    headers: tenantHeaders(),
  });

  if (!response.ok) {
    throw new Error("Unable to load products.");
  }

  return response.json();
}
