import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

import type { PresentationBody } from "../types/Presentation";

/**
 * Restates a presentation — every field, every time.
 *
 * The server replaces the whole statement including its routes, which is why
 * the form sends what it currently shows rather than a diff. A partial update
 * would offer several ways to leave a presentation half-corrected.
 *
 * Addressed by the presentation's own id rather than through the market: it is
 * its own aggregate, and routing the correction through the market would imply
 * the market is what changed.
 */
export async function restatePresentation(
  presentationId: string,
  body: PresentationBody,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/presentations/${presentationId}`),
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to update the presentation."),
    );
  }
}
