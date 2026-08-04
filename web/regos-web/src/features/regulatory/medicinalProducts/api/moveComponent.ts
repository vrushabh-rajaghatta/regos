import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

/**
 * Moves an article — and everything inside it — somewhere else, or to the top
 * level.
 *
 * The server refuses a move that would make a component its own ancestor or
 * push its contents past the depth limit. Both are its decisions, not
 * pre-empted here: only the server sees the whole tree.
 */
export async function moveComponent(
  componentId: string,
  newParentComponentId: string | null,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/components/${componentId}/parent`),
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ newParentComponentId }),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to move the component."));
  }
}
