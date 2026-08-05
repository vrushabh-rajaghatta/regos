import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface PackSupplyBody {
  legalStatusOfSupplyCode: string | null;
  shelfLifeValue: number | null;
  shelfLifeUnitCode: string | null;
  shelfLifeText: string | null;
  storageConditionCodes: string[];
  testedAtCodes: string[];
}

/**
 * Its own route rather than part of the pack: restating what a pack *is* and
 * stating how it may be *supplied* are two acts, and the second usually arrives
 * when stability data does.
 */
export async function statePackSupply(
  packagedProductId: string,
  body: PackSupplyBody,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/packaged-products/${packagedProductId}/supply`),
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to save how this pack is supplied."),
    );
  }
}
