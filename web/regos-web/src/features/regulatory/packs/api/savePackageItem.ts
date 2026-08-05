import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface PackageItemBody {
  parentPackageItemId?: string | null;
  itemTypeCode: string;
  materialCode: string | null;
  quantity: number;
  unitOfPresentationCode: string | null;
  description: string | null;
}

/**
 * Add and restate in one file, because they take the same body and the only
 * difference is where it is sent — the same reasoning the server's shared
 * vocabulary helper uses.
 */
export async function addPackageItem(
  packagedProductId: string,
  body: PackageItemBody,
): Promise<{ id: string }> {
  const response = await apiFetch(
    buildUrl(`/api/packaged-products/${packagedProductId}/items`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to add the layer."));
  }

  return response.json();
}

export async function restatePackageItem(
  packageItemId: string,
  body: PackageItemBody,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/package-items/${packageItemId}`),
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to update the layer."));
  }
}
