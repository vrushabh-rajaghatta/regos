import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

/** Null lifts the layer to the outermost level. The subtree travels with it. */
export async function movePackageItem(
  packageItemId: string,
  newParentPackageItemId: string | null,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/package-items/${packageItemId}/parent`),
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ newParentPackageItemId }),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to move the layer."));
  }
}
