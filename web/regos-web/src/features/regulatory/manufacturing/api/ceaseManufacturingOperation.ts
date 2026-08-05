import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

/**
 * Closes the period — the site no longer performs this operation.
 *
 * **Closed, never deleted.** A site that made this product for four years made
 * it, and removing the row would make a filing from 2023 unexplainable.
 */
export async function ceaseManufacturingOperation(
  manufacturingOperationId: string,
  ceasedOn: string,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(
      `/api/manufacturing-operations/${manufacturingOperationId}/cessation`,
    ),
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ ceasedOn }),
    },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to close this operation."),
    );
  }
}
