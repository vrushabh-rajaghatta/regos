import { buildUrl } from "@/shared/api/apiClient";

export async function getProduct(id: string) {
  const response = await fetch(buildUrl(`/api/products/${id}`));

  if (!response.ok) {
    throw new Error("Unable to load product.");
  }

  return response.json();
}
