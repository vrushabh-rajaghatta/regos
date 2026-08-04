import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

/** Qualifies the authorisation by another therapy. */
export async function addIndicationTherapy(
  indicationId: string,
  body: { relationshipCode: string; therapy: string },
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/indications/${indicationId}/therapies`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to add the therapy."));
  }
}
