import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

/**
 * Removes an article.
 *
 * The server refuses one that still holds others rather than cascading —
 * emptying it first makes the intent explicit, and quiet data loss is not
 * something a regulatory record should allow.
 */
export async function removeComponent(componentId: string): Promise<void> {
  const response = await apiFetch(buildUrl(`/api/components/${componentId}`), {
    method: "DELETE",
  });

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to remove the component."),
    );
  }
}
