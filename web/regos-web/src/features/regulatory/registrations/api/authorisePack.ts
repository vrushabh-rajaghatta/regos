import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export async function authorisePack(
  registrationId: string,
  body: { packagedProductId: string; authorisedOn: string },
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/registrations/${registrationId}/authorised-packs`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to authorise that pack."),
    );
  }
}
