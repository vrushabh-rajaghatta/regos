import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export async function removeTradeName(
  medicinalProductId: string,
  tradeNameId: string,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(
      `/api/medicinal-products/${medicinalProductId}/trade-names/${tradeNameId}`,
    ),
    { method: "DELETE" },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to remove the trade name."),
    );
  }
}
