import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

/**
 * Takes a substance out of a composition.
 *
 * The server refuses to leave a formulation with excipients and no active, and
 * that refusal is shown rather than pre-empted here — it is the server that
 * knows what else is in the composition.
 */
export async function removeIngredient(
  presentationId: string,
  ingredientId: string,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(
      `/api/presentations/${presentationId}/ingredients/${ingredientId}`,
    ),
    { method: "DELETE" },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to remove the ingredient."),
    );
  }
}
