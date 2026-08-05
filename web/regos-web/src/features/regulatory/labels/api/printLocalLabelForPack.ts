import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

/**
 * Names the pack a local label is printed for, or clears it.
 *
 * On the label rather than on a revision: which pack a carton is printed for is
 * what the document *is*, and revising the words on it does not make it a
 * different pack's carton.
 */
export async function printLocalLabelForPack(
  localLabelId: string,
  packagedProductId: string | null,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/local-labels/${localLabelId}/pack`),
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ packagedProductId }),
    },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to name the pack this is printed for."),
    );
  }
}
