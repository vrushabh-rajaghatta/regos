import { buildUrl } from "@/shared/api/apiClient";

export interface CreateRegulatoryApplicationRequest {
  authorityId: string;
  countryId: string;
  applicantOrganizationId: string;
  name: string;
}

export async function createRegulatoryApplication(
  productId: string,
  request: CreateRegulatoryApplicationRequest,
): Promise<void> {
  const response = await fetch(
    buildUrl(`/api/products/${productId}/applications`),
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(request),
    },
  );

  if (!response.ok) {
    throw new Error("Failed to create Application.");
  }
}
