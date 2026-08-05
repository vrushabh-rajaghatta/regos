import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

/**
 * Removes an authorisation recorded in error.
 *
 * **Not the same act as withdrawing a pack from the market**, which is the
 * pack's own dated marketing status.
 */
export async function withdrawPackAuthorisation(
  packAuthorisationId: string,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/pack-authorisations/${packAuthorisationId}`),
    { method: "DELETE" },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to remove that authorisation."),
    );
  }
}
