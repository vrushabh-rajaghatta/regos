import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface CreateRegistrationBody {
  authorityId: string;
  holderOrganizationId: string;
  occurredOn: string;
  originatingApplicationId?: string | null;
  note?: string | null;
}

/**
 * Addressed by the medicinal product, not the global one: a licence is granted
 * over a product in a market. The country is not in the body because it is not
 * the caller's to state — it belongs to the market being registered in.
 */
export async function createRegistration(
  medicinalProductId: string,
  body: CreateRegistrationBody
): Promise<{ id: string }> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/registrations`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to create the registration.")
    );
  }

  return response.json();
}
