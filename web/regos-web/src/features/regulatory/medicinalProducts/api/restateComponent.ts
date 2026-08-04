import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

import type { ComponentBody } from "../types/Component";

/**
 * Corrects what an article is, not where it sits.
 *
 * Position has its own call, because moving a component is the operation with
 * the cycle and depth rules attached.
 */
export async function restateComponent(
  componentId: string,
  body: ComponentBody,
): Promise<void> {
  const response = await apiFetch(buildUrl(`/api/components/${componentId}`), {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to update the component."),
    );
  }
}
