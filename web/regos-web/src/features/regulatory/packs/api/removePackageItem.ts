import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export async function removePackageItem(
  packageItemId: string,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/package-items/${packageItemId}`),
    { method: "DELETE" },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to remove the layer."));
  }
}
