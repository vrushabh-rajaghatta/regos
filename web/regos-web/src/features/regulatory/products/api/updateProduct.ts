import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export interface UpdateProductRequest {
  name: string;
  type: string;
}

export async function updateProduct(
  productId: string,
  request: UpdateProductRequest,
): Promise<void> {
  const response = await apiFetch(buildUrl(`/api/products/${productId}`), {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  // 204 No Content on success - nothing to parse.
  if (response.ok) return;

  // Surface the API's ProblemDetails (invalid name, missing product) rather
  // than a generic failure.
  let message = "Unable to save changes.";

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
