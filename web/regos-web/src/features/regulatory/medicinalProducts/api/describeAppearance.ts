import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

import type { AppearanceBody } from "../types/Presentation";

/**
 * Its own route: a presentation is recorded when its dose form is known and
 * described when somebody has seen it, which is routinely later.
 */
export async function describeAppearance(
  presentationId: string,
  body: AppearanceBody,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/presentations/${presentationId}/appearance`),
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to save what this looks like."),
    );
  }
}
