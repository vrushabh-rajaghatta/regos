import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { RegistrationSummary } from "../types/RegistrationSummary";

export async function listProductRegistrations(
  productId: string
): Promise<RegistrationSummary[]> {
  const response = await apiFetch(
    buildUrl(`/api/products/${productId}/registrations`)
  );

  if (!response.ok) {
    throw new Error("Unable to load this product's registrations.");
  }

  return response.json();
}
