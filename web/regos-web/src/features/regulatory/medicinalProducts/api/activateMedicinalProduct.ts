import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export async function activateMedicinalProduct(
  medicinalProductId: string,
  on: string,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/activate`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ on }),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to restore the market."));
  }
}
