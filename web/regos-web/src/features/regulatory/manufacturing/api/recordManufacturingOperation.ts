import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface ManufacturingOperationBody {
  organizationSiteId: string;
  operationCode: string;
  effectiveFrom: string;
}

/**
 * Records that a site performs an operation for this market's product.
 *
 * **Recording, not approving.** Whether the licence permits it is a different
 * statement made elsewhere, and the gap between them is what the market view
 * reports.
 */
export async function recordManufacturingOperation(
  medicinalProductId: string,
  body: ManufacturingOperationBody,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/manufacturing`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to record where this is made."),
    );
  }
}
