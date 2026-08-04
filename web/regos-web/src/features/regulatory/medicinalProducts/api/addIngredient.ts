import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

import type { IngredientBody } from "../types/Presentation";

/**
 * Adds a substance to a presentation's composition.
 *
 * The substance travels as an id, not a name — that is what makes *"which
 * products contain substance X?"* answerable backwards. An active with no
 * strength, or a substance already in the composition, comes back as a refusal
 * whose wording is surfaced verbatim.
 */
export async function addIngredient(
  presentationId: string,
  body: IngredientBody & { substanceId: string },
): Promise<{ id: string }> {
  const response = await apiFetch(
    buildUrl(`/api/presentations/${presentationId}/ingredients`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to add the ingredient."));
  }

  return response.json();
}
