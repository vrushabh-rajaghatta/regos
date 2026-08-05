import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

import type { LabelLanguageCoverage } from "../types/GlobalLabel";

/**
 * "Does this market's labelling cover the languages it is expected in?"
 *
 * The question EPIC-018 could not ask, because nothing knew the other half.
 */
export async function getLabelLanguageCoverage(
  medicinalProductId: string,
): Promise<LabelLanguageCoverage> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/label-languages`),
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to check the label languages."),
    );
  }

  return response.json();
}
