import { buildUrl } from "@/shared/api/apiClient";

import type { RegisterProductRequest } from "../types/RegisterProductRequest";
import type { RegisterProductResponse } from "../types/RegisterProductResponse";

export async function registerProduct(
  request: RegisterProductRequest,
): Promise<RegisterProductResponse> {
  const response = await fetch(buildUrl("/api/products"), {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    throw new Error("Unable to register product.");
  }

  return await response.json();
}
