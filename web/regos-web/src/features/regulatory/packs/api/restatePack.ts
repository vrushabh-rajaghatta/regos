import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface RestatePackBody {
  description: string;
  packSizeQuantity: number | null;
  packSizeUnitCode: string | null;
  packCode: string | null;
}

export async function restatePack(
  packagedProductId: string,
  body: RestatePackBody,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/packaged-products/${packagedProductId}`),
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to update the pack."));
  }
}
