import { buildUrl, tenantHeaders } from "@/shared/api/apiClient";

import type { RegisterProductRequest } from "../types/RegisterProductRequest";
import type { RegisterProductResponse } from "../types/RegisterProductResponse";

export async function registerProduct(
  request: RegisterProductRequest,
): Promise<RegisterProductResponse> {
  const response = await fetch(buildUrl("/api/products"), {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...tenantHeaders(),
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    // Surface the API's reason - a duplicate code (409) or an invalid one
    // (400) - rather than a generic failure.
    let message = "Unable to register product.";

    try {
      const problem = await response.json();

      if (typeof problem?.detail === "string") {
        message = problem.detail;
      }
    } catch {
      // No problem body - fall back to the generic message.
    }

    throw new Error(message);
  }

  return await response.json();
}
