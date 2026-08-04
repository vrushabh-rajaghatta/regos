import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

/**
 * Records how a market's product is classified, or clears it.
 *
 * A blank value clears — absence is an ordinary state for a market, so "we do
 * not have this" is a correction rather than a separate action. The server
 * checks the shape only; it holds no WHO ATC index, so a malformed code is
 * refused and an unknown one is not.
 */
export async function recordAtcCode(
  medicinalProductId: string,
  atcCode: string | null,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/atc-code`),
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ atcCode }),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to record the ATC code."));
  }
}
