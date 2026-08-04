import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

import type { PresentationBody } from "../types/Presentation";

/**
 * Adds a presentation to a market.
 *
 * Nothing in the body says which tenant owns it — the server takes that from
 * the session. A dose form or route RegOS does not know comes back as a refusal
 * naming the words it would have accepted, and that message is surfaced
 * verbatim.
 */
export async function addPresentation(
  medicinalProductId: string,
  body: PresentationBody,
): Promise<{ id: string }> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/presentations`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to add the presentation."));
  }

  return response.json();
}
