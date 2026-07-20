import { buildUrl, tenantHeaders } from "@/shared/api/apiClient";

export async function getProduct(id: string) {
  const response = await fetch(buildUrl(`/api/products/${id}`), {
    headers: tenantHeaders(),
  });

  if (!response.ok) {
    throw new Error("Unable to load product.");
  }

  return response.json();
}
