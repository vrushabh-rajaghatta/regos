import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

import type { ComponentBody } from "../types/Component";

/**
 * Adds an article to a market, optionally inside another.
 *
 * A placement that would make the tree too deep comes back as a refusal naming
 * the limit — the rule is the domain's, and its wording is surfaced verbatim.
 */
export async function addComponent(
  medicinalProductId: string,
  body: ComponentBody & { parentComponentId: string | null },
): Promise<{ id: string }> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/components`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to add the component."));
  }

  return response.json();
}
