import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "./problemDetail";

export interface CreateRegistrationBody {
  countryId: string;
  authorityId: string;
  holderOrganizationId: string;
  occurredOn: string;
  originatingApplicationId?: string | null;
  note?: string | null;
}

export async function createRegistration(
  globalProductId: string,
  body: CreateRegistrationBody
): Promise<{ id: string }> {
  const response = await apiFetch(
    buildUrl(`/api/products/${globalProductId}/registrations`),
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
