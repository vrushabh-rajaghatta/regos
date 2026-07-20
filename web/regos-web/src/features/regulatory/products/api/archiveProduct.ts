import { buildUrl, tenantHeaders } from "@/shared/api/apiClient";

export async function archiveProduct(productId: string): Promise<void> {
  const response = await fetch(
    buildUrl(`/api/products/${productId}/archive`),
    { method: "POST", headers: tenantHeaders() },
  );

  if (response.ok) return;

  // Surface the API's reason - most usefully 409 when the product was already
  // archived by someone else while this page was open.
  let message = "Unable to archive this product.";

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
