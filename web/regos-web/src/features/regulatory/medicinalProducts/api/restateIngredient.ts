import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

import type { IngredientBody } from "../types/Presentation";

/**
 * Corrects an ingredient's role or its strength.
 *
 * Not its substance — a different substance is a different ingredient, so
 * swapping one is add-then-remove. Because the replacement goes in first, the
 * server's "a composition may not lose its last active" guard never blocks it.
 */
export async function restateIngredient(
  presentationId: string,
  ingredientId: string,
  body: IngredientBody,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(
      `/api/presentations/${presentationId}/ingredients/${ingredientId}`,
    ),
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to update the ingredient."),
    );
  }
}
