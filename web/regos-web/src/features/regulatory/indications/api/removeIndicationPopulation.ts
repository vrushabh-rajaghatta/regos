import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

/** Removes a qualifier recorded in error. */
export async function removeIndicationPopulation(
  indicationId: string,
  populationId: string,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/indications/${indicationId}/populations/${populationId}`),
    { method: "DELETE" },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to remove the population."),
    );
  }
}
